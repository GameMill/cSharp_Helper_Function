using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Functions
{
    public class Copy_file
    {
        public static string srcName;
        public static string destName;
        public static long fileLen;
        private static System.ComponentModel.BackgroundWorker backgroundWorker1;
        public static bool Is_Done = false;
        public static void XCopy(string SrcName, string DestName)
        {
            srcName = SrcName; destName = DestName;
            Is_Done = false;
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            Functions.Progress_Bar.Start();

            FileInfo sourceFile = new FileInfo(srcName);
            if (!sourceFile.Exists)
            {
                throw new NotImplementedException();
            }
            fileLen = sourceFile.Length;

            FileInfo destFile = new FileInfo(destName);
            if (destFile.Exists)
            {
                destFile.Delete();
            }
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerAsync();
            while (Is_Done == false) { }
            Functions.Progress_Bar.End();
        }

        private static void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Functions.Progress_Bar.Set_Progress(e.ProgressPercentage);
        }
        static void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int buflen = (1024 * 16);
            byte[] buf = new byte[buflen];
            long totalBytesRead = 0;
            double pctDone = 0;
            string msg = "";
            int numReads = 0; 

            using (FileStream sourceStream =
              new FileStream(srcName, FileMode.Open))
            {
                using (FileStream destStream =
                    new FileStream(destName, FileMode.CreateNew))
                {
                    while (true)
                    {
                        numReads++;
                        int bytesRead = sourceStream.Read(buf, 0, buflen);
                        if (bytesRead == 0) break;
                        destStream.Write(buf, 0, bytesRead);

                        totalBytesRead += bytesRead;
                        if (numReads % 10 == 0)
                        {
                            pctDone = (double)
                                ((double)totalBytesRead / (double)fileLen);
                            msg = string.Format("{0}",
                                     (int)(pctDone * 100));
                            backgroundWorker1.ReportProgress(int.Parse(msg));

                        }

                        if (bytesRead < buflen) break;

                    }
                }
            }
            Is_Done = true;
        }
    }
}
