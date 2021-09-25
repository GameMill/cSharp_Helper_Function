using System;
using System.Collections;

delegate void MenuCallback();
namespace Functions
{
    public class Menu  : Core
    {
        

	    private ArrayList m_Items = new ArrayList();
	
	    public void Add(string text)
	    {
		    m_Items.Add(text);
	    }

        


        public Menu()
        {
        }

        DoubleBuffer.buffer buffer = null;
        public Menu(ref DoubleBuffer.buffer buffer)
        {
            this.buffer = buffer;
        }

        public static bool GetKeyWithTimeout(out Nullable<ConsoleKeyInfo> Key,int Timeout,ref DoubleBuffer.buffer buffer,int TimeoutLineRow)
        {
            if (Timeout == 0)
            {
                Key = Console.ReadKey();
                return true;
            }
            long Start = DateTime.Now.Ticks;
            int Total = 0;
            while (Total < Timeout)
            {

                Total = (int)(new TimeSpan(DateTime.Now.Ticks - Start)).TotalSeconds;
                buffer.Draw("Timeout In: " + (Timeout - Total) + "      ", 0, TimeoutLineRow, 15);
                buffer.Print();
                if (Console.KeyAvailable)
                {
                    Key = Console.ReadKey(true);
                    return true;
                }
                else
                    System.Threading.Thread.Sleep(100);
            }

            Key = null;
            return false;

        }
        int TimeoutLine = 0;
        public string Show(string Header, string Default, int timeout=5)
	    {
            if (Default == "")
            {
                timeout = 0;
            }
            if (buffer == null)
            {
                Functions.Header.New(Header);
                for (int i = 0; i < m_Items.Count; ++i)
                {
                    string mi = m_Items[i].ToString();
                    Console.WriteLine(" [{0}] {1} {2}", i + 1, mi, mi == Default ? "*" : "");
                }
                Console.WriteLine();
                Functions.Header.Line(); Console.Write("Please Choose Your Item: ");
            }
            else
            {
                buffer.Clear();
                Functions.Header.New(Header,ref buffer); //Console.SetCursorPosition
                int start = 4;
                for (int i = 0; i < m_Items.Count; ++i)
                {
                    string mi = m_Items[i].ToString();

                    buffer.Draw(string.Format(" [{0}] {1} {2}", i + 1, mi, mi == Default ? "*" : ""),0, start,15);
                    start++;
                }
                start++;
                TimeoutLine = start + 1;
                buffer.Draw("Please Choose Your Item:",0, start,15);
                if (timeout != 0)
                    buffer.Draw("Timeout In: " + timeout, 0, TimeoutLine, 15);
                
                Console.SetCursorPosition("Please Choose Your Item:".Length + 2, start);
                buffer.Print();
            }


            int choosen = 0;
            if (m_Items.Count >= 10)
            {
                var Line = Console.ReadLine();
                if (Line == "")
                {
                    return Default;
                }
                choosen = Int32.Parse(Line);
            }
            else
            {
                ConsoleKeyInfo? keyinfo;
                if (GetKeyWithTimeout(out keyinfo,timeout,ref buffer,TimeoutLine))
                {
                    if (keyinfo.Value.Key == ConsoleKey.Enter)
                    {
                        return Default;
                    }
                    choosen = Int32.Parse(keyinfo.Value.KeyChar.ToString());
                }
                else
                    return Default;
            }
		    
		    if ( choosen > m_Items.Count || choosen < 1 )
		    {
			    return Show(Header,Default);
		    }
		    else
		    {
			    return m_Items[choosen-1].ToString();
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