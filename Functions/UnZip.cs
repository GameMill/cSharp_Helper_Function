using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public class Zip
    {
        public static void UnZip(string File,string Dir)
        {
            Ionic.Zip.ZipFile zip2 = Ionic.Zip.ZipFile.Read(File);
            zip2.ExtractProgress += zip1_ExtractProgress;
            Functions.Progress_Bar.Start(); Functions.Progress_Bar.Set_Progress(0, new string[] { "Loading Files" });
            zip2.ExtractAll(Dir);
            Functions.Progress_Bar.End();
        }
        public static void zip1_ExtractProgress(object sender, Ionic.Zip.ExtractProgressEventArgs e)
        {
            Functions.Progress_Bar.Set_Progress(Get_Percentage(e.TotalBytesToTransfer, e.BytesTransferred));
        }

        private static int Get_Percentage(long total, long transferred)
        {
            if (transferred != 0)
            {
                return Convert.ToInt32(transferred * 100 / total);
            }
            return 0;
        }
    }
}
