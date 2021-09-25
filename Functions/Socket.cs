using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class SocketWriter: IDisposable
    {
        TcpClient tcpClient = null;
        NetworkStream ns = null;
        System.IO.StreamWriter sw = null;
        public SocketWriter(string IP,int Port)
        {
            this.tcpClient = new TcpClient(IP, Port);
            //Connects fine
            this.ns = tcpClient.GetStream();
            this.sw = new System.IO.StreamWriter(ns);
            this.sw.AutoFlush = true;
        }

        public void Write(string Data)
        {
            sw.Write(Data);

        }
        public void EndWrite()
        {
            sw.Write("<EOF>");
        }

        public string ReadToEnd()
        {
            return new System.IO.StreamReader(this.ns).ReadToEnd();
        }


        public void Dispose()
        {
            this.sw.Close();
            this.ns.Close();
            this.tcpClient.Close();

        }
    }


    public class SocketListener : Core
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        Dictionary<string, Action<string, Socket>> tasks = null;
        Action<Exception> Error = null;
        public static SocketListener instance = null;
        IPEndPoint localEndPoint = null;
        public SocketListener(string IpPort, Dictionary<string,Action<string, Socket>> Tasks,Action<Exception> Error)
        {
            SocketListener.instance = this;
            this.localEndPoint = CreateIPEndPoint(IpPort);
            this.Error = Error;
            this.tasks = Tasks;
            StartListening();
        }

        // Handles IPv4 and IPv6 notation.
        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            try
            {
                string[] ep = endPoint.Split(':');
                if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
                IPAddress ip;
                if (ep.Length > 2)
                {
                    if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                    {
                        throw new FormatException("Invalid ip-adress");
                    }
                }
                else
                {
                    if (!IPAddress.TryParse(ep[0], out ip))
                    {
                        throw new FormatException("Invalid ip-adress");
                    }
                }
                int port;
                if (!int.TryParse(ep[ep.Length - 1], System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.CurrentInfo, out port))
                {
                    throw new FormatException("Invalid port");
                }
                return new IPEndPoint(ip, port);
            }
            catch (Exception e)
            {
                SocketListener.instance.Error(e);
            }
            return null;
        }

        public static void StartListening()
        {


            Socket listener = new Socket(SocketListener.instance.localEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(SocketListener.instance.localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                SocketListener.instance.Error(e);
            }

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    content = content.Substring(0, content.Length - 5);
                    var Data = content.Split('|');
                    Data[0] = Data[0].ToLower();
                    if (SocketListener.instance.tasks.ContainsKey(Data[0]))
                    {
                        if (Data.Length > 1)
                            SocketListener.instance.tasks[Data[0]](Data[1], handler);
                    }

                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        public static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                SocketListener.instance.Error(e);
            }
        }

        public override void DisplayDebug()
        {
            throw new NotImplementedException();
        }

        public override void DisplayDebugWithBuffer()
        {
            throw new NotImplementedException();
        }
    }
}
