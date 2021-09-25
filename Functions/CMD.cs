using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class CMD
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1); //1 = SW_SHOWNORMAL           
                else ShowWindow(hWnd, 0); //0 = SW_HIDE               
            }
        }




        public static void LowCpuLoop(System.Threading.TimerCallback Task,int seconds, int HowOften)
        {
            // Create a IPC wait handle with a unique identifier.
            bool createdNew;
            var waitHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset, "CF2D4313-33DE-1241-9721-6AFF69841DEA", out createdNew);
            var signaled = false;

            if (!createdNew)
            {
                waitHandle.Set();
                return;
            }

            var timer = new System.Threading.Timer(Task, null, TimeSpan.Zero, TimeSpan.FromSeconds(HowOften));

            do
            {
                signaled = waitHandle.WaitOne(TimeSpan.FromSeconds(5));
            } while (!signaled);
        }
        [System.Security.SuppressUnmanagedCodeSecurity()]
        internal class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool FreeConsole();

            [DllImport("kernel32", SetLastError = true)]
            internal static extern bool AttachConsole(int dwProcessId);

            [DllImport("user32.dll")]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        }

        public static bool AllocConsole()
        { return NativeMethods.AllocConsole(); }

        public static bool FreeConsole()
        { return NativeMethods.FreeConsole(); }

        public static bool AttachConsole(int dwProcessId)
        { return NativeMethods.AttachConsole(dwProcessId); }

        public static IntPtr GetForegroundWindow()
        { return NativeMethods.GetForegroundWindow(); }

        public static int GetWindowThreadProcessId(IntPtr hWnd)
        { 
            int lpdwProcessId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out lpdwProcessId);
            return lpdwProcessId;
        }

        public static string exec(string Command, string Arg)
        {
            System.Diagnostics.ProcessStartInfo psi =
               new System.Diagnostics.ProcessStartInfo(Command);
            psi.Arguments = Arg;
            psi.RedirectStandardOutput = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            System.Diagnostics.Process listFiles;
            listFiles = System.Diagnostics.Process.Start(psi);
            System.IO.StreamReader myOutput = listFiles.StandardOutput;
            listFiles.WaitForExit();
            string a = myOutput.ReadToEnd();
            listFiles.Close();
            return a;
        }

        public static void Run(string anyCommand)
        {
            Process.Start("cmd", "/C " + anyCommand); 
            
        }


        public static void Diskpart(string Command, bool No_Return)
        {
            string sd = System.IO.Path.GetTempPath();

            File.Delete(sd + "dpScript.scr");       // Delete the script, if it exists
            // Create the script to resize C and make D
            File.AppendAllText(sd + "dpScript.scr", string.Format(Command + "\r\nexit"));

            System.Diagnostics.ProcessStartInfo psi =
              new System.Diagnostics.ProcessStartInfo(@"diskpart");
            psi.Arguments = " /s \"" + sd + "dpScript.scr\"";
            psi.RedirectStandardOutput = false;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            psi.UseShellExecute = true;

            System.Diagnostics.Process listFiles = System.Diagnostics.Process.Start(psi);

            listFiles.WaitForExit();
            listFiles.Close();
            listFiles.Dispose();
            File.Delete(sd + "dpScript.scr");

        }


        public static string[] Diskpart(string Command)
        {
            string sd = System.IO.Path.GetTempPath();

            File.Delete(sd + "dpScript.scr");       // Delete the script, if it exists
            // Create the script to resize C and make D
            File.AppendAllText(sd + "dpScript.scr", Command + "\r\nexit");

            System.Diagnostics.ProcessStartInfo psi =
              new System.Diagnostics.ProcessStartInfo(@"diskpart");
            psi.Arguments = " /s \"" + sd + "dpScript.scr\"";
            psi.RedirectStandardOutput = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;

            System.Diagnostics.Process listFiles;
            listFiles = System.Diagnostics.Process.Start(psi);
            System.IO.StreamReader myOutput = listFiles.StandardOutput;
            listFiles.WaitForExit();

            var info = new Dictionary<string, string>();
            string output = "";



            File.Delete(sd + "dpScript.scr");
            output = myOutput.ReadToEnd();
            myOutput.Close();
            myOutput.Dispose();

            string[] lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] lines2 = new string[lines.Length - 11];
            int num = 0;
            for (int i = 8; i < (lines.Count() - 3); i++)
            {

                lines2[num] = lines[i];
                num++;
            }
            return lines2;
        }
    }
}
