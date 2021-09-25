using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Functions
{
    public class Drives : Core
    {
        static Dictionary<string, Dictionary<string, string>> HD_Drives = new Dictionary<string, Dictionary<string, string>>();
        static Dictionary<int, string> DVD_Drives = new Dictionary<int, string>();

        private static bool Is_Init = false;

        public static void init(bool Override)
        {
            Is_Init = false;
            init();
        }
        public static void init()
        {
            if (Is_Init == false)
            {
                HD_Drives = new Dictionary<string, Dictionary<string, string>>();
                DVD_Drives = new Dictionary<int, string>();
                Is_Init = true;
                char[] Char = { ' ', '#', ',', ':' };
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"));
                foreach (ManagementObject item in searcher.Get())
                {
                    foreach (ManagementObject b in item.GetRelated("Win32_DiskPartition"))
                    {
                        var Disk = item["DeviceID"].ToString().Trim(Char).ToLower();
                        HD_Drives[Disk] = new Dictionary<string, string>();
                        HD_Drives[Disk]["Letter"] = Disk;
                        HD_Drives[Disk]["Disk_Number"] = b["DeviceID"].ToString().Substring(6, 3).Trim(Char);
                        HD_Drives[Disk]["Partition_Number"] = b["DeviceID"].ToString().Substring(20).Trim(Char);
                        // Console.WriteLine( + " " + b["DeviceID"]);
                    }
                }
                foreach (string item in Functions.CMD.Diskpart("list volume"))
                {
                    if (HD_Drives.ContainsKey(item.Substring(14, 3).Trim(Char).ToLower()))
                    {
                        HD_Drives[item.Substring(14, 3).Trim(Char).ToLower()]["Volume_Number"] = item.Substring(8, 3).Trim(Char);
                    }
                }
                searcher.Dispose();

                searcher = new ManagementObjectSearcher(new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DriveType=5"));
                int num = 0;
                foreach (ManagementObject item in searcher.Get())
                {
                    DVD_Drives[num] = item["DeviceID"].ToString().Trim(Char).ToLower();
                }
                searcher.Dispose();
            }
        }

        public static Dictionary<string, string> Get_HD_By_File(string File)
        {
            init();
            foreach (var Key in HD_Drives.Keys)
            {
                if (System.IO.File.Exists(HD_Drives[Key]["Letter"] + @":\" + File))
	            {
                    return HD_Drives[Key];
	            }
            }
            return new Dictionary<string, string>();
        }

        public static Dictionary<string, string> Get_HD_By_Folder(string Folder)
        {
            init();
            foreach (var Key in HD_Drives.Keys)
            {
                if (System.IO.Directory.Exists(HD_Drives[Key]["Letter"] + @":\" + Folder))
	            {
                    return HD_Drives[Key];
	            }
            }
            return new Dictionary<string, string>();
        }

        public static Dictionary<string, string> Get_HD_Drive(string Drive_Letter)
        {
            init();
            return HD_Drives[Drive_Letter];
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
