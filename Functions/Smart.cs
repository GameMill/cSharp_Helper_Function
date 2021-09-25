using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;
using System.Text;

namespace Functions 
{
    [DataContract]
    public class HDD : Core
    {
        private static List<HDD> hDDs = null;
        public static List<HDD> GetHDDs(bool Reset = false)
        {
            if (Reset)
                hDDs = null;
            if (hDDs != null)
                return hDDs;
            // retrieve list of drives on computer (this will return both HDD's and CDROM's and Virtual CDROM's)                    
            var dicDrives = new Dictionary<int, HDD>();
            var wdSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            // extract model and interface information
            foreach (ManagementObject drive in wdSearcher.Get())
            {
                var type = drive["InterfaceType"].ToString().Trim();
                if (type == "IDE" || type == "SCSI")
                {
                    var hdd = new HDD();
                    hdd.Model = drive["Model"].ToString().Trim();
                    hdd.Type = type;
                    hdd.Size = long.Parse(drive["Size"].ToString().Trim()) / 1024 / 1024 / 1024;
                    hdd.PNPDeviceID = drive["PNPDeviceID"].ToString().Trim();
                    dicDrives.Add(int.Parse(drive["Index"].ToString()), hdd);
                }
            }

            var pmsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

            // retrieve hdd serial number
            foreach (ManagementObject drive in pmsearcher.Get())
            {
                if (drive["tag"].ToString().Contains("PHYSICALDRIVE"))
                {
                    var index = int.Parse(drive["tag"].ToString().Trim().Last().ToString());
                    if (dicDrives.ContainsKey(index))
                    {
                        dicDrives[index].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                        dicDrives[index].DriveIndex = index.ToString();
                    }
                    
                }
            }

            

            // get wmi access to hdd 
            var searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive");
            searcher.Scope = new ManagementScope(@"\root\wmi");

            // check if SMART reports the drive is failing
            try
            {
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictStatus");
                foreach (ManagementObject drive in searcher.Get())
                {
                    var Index = -1;
                    try
                    {
                        foreach (var item in dicDrives)
                        {
                            if(drive["InstanceName"].ToString().ToLower().Contains(item.Value.PNPDeviceID.ToLower()))
                            {
                                Index = item.Key;
                                break;
                            }
                        }
                        if (Index != -1)
                        {
                            if (dicDrives.ContainsKey(Index))
                            {
                                dicDrives[Index].IsOK = (bool)drive.Properties["PredictFailure"].Value == false;
                                dicDrives[Index].SmartSupported = true;
                            }
                        }
                        else
                        {
                            //dicDrives[Index].SmartSupported = false;
                        }

                    }
                    catch
                    {
                        dicDrives[Index].SmartSupported = false;
                    }

                }
            }
            catch
            {
            }

            try
            {
                // retrive attribute flags, value worste and vendor data information
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictData");
                foreach (ManagementObject data in searcher.Get())
                {
                    var Index = -1;
                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    foreach (var item in dicDrives)
                    {
                        if(data["InstanceName"].ToString().ToLower().Contains(item.Value.PNPDeviceID.ToLower()))
                        {
                                Index = item.Key;
                                break;
                        }
                    }
                    if (Index == -1 || !dicDrives.ContainsKey(Index))
                        continue;

                    for (int i = 0; i < 30; ++i)
                    {

                        try
                        {
                            int id = bytes[i * 12 + 2];

                            int flags = bytes[i * 12 + 4]; // least significant status byte, +3 most significant byte, but not used so ignored.
                            //bool advisory = (flags & 0x1) == 0x0;
                            bool failureImminent = (flags & 0x1) == 0x1;
                            //bool onlineDataCollection = (flags & 0x2) == 0x2;

                            int value = bytes[i * 12 + 5];
                            int worst = bytes[i * 12 + 6];
                            int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                            if (id == 0) continue;
                            if (HDD.AttributesTamplates.ContainsKey(id))
                            {
                                var attr = new Smart(HDD.AttributesTamplates[id]);
                                attr.Current = value;
                                attr.Worst = worst;
                                attr.Data = vendordata;
                                attr.IsOK = failureImminent == false;
                                dicDrives[Index].Attributes.Add(id, attr);
                            }
                            else
                            {

                            }
                           
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }
                }


                // retreive threshold values foreach attribute
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictThresholds");
                foreach (ManagementObject data in searcher.Get())
                {
                    var Index = -1;
                    foreach (var item in dicDrives)
                    {
                        if (data["InstanceName"].ToString().ToLower().Contains(item.Value.PNPDeviceID.ToLower()))
                        {
                            Index = item.Key;
                            break;
                        }
                    }

                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    for (int i = 0; i < 30; ++i)
                    {
                        try
                        {

                            int id = bytes[i * 12 + 2];
                            int thresh = bytes[i * 12 + 3];
                            if (id == 0) continue;

                            var attr = dicDrives[Index].Attributes[id];
                            attr.Threshold = thresh;
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }

                }
            }
            catch
            {

            }

            hDDs = dicDrives.Values.ToList();
            hDDs.Sort(
                (HDD hd1, HDD hd2) => {
                    return hd1.DriveIndex.CompareTo(hd2.DriveIndex);
                });
            dicDrives = null;
            return hDDs;
        }
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public List<int> IsOKCustomBadMembers
        {
            get
            {
                var BAD = new List<int>();
                if (Attributes.ContainsKey(5) && Attributes[5].Data != 0)
                {
                    BAD.Add(5);
                }
                if (Attributes.ContainsKey(10) && Attributes[10].Data != 0)
                {
                    BAD.Add(10);
                }
                if (Attributes.ContainsKey(184) && Attributes[184].Data != 0)
                {
                    BAD.Add(184);
                }
                if (Attributes.ContainsKey(187) && Attributes[187].Data != 0)
                {
                    BAD.Add(187);
                }
                //if (Attributes.ContainsKey(188) && Attributes[188].Data != 0)
                //{
                //    BAD.Add(188);
                //}
                if (Attributes.ContainsKey(196) && Attributes[196].Data != 0)
                {
                    BAD.Add(189);
                }
                if (Attributes.ContainsKey(197) && Attributes[197].Data != 0)
                {
                    BAD.Add(197);
                }
                if (Attributes.ContainsKey(198) && Attributes[198].Data != 0)
                {
                    BAD.Add(198);
                }
                if (Attributes.ContainsKey(201) && Attributes[201].Data != 0)
                {
                    BAD.Add(201);
                }
                return BAD;
            }
        }
            [DataMember]
        public bool IsOKCustom
        {
            get
            {
                bool _IsOK = true;
                if (Attributes.ContainsKey(5) && Attributes[5].Data != 0)
                {
                    _IsOK = false;
                }
                else if (Attributes.ContainsKey(10) && Attributes[10].Data != 0)
                {
                    _IsOK = false;
                }
                else if(Attributes.ContainsKey(184) && Attributes[184].Data != 0)
                {
                    _IsOK = false;
                }
                else if (Attributes.ContainsKey(187) && Attributes[187].Data != 0)
                {
                    _IsOK = false;
                }
                //else if (Attributes.ContainsKey(188) && Attributes[188].Data != 0)
                //{
                //    _IsOK = false;
                //}
                else if (Attributes.ContainsKey(196) && Attributes[196].Data != 0)
                {
                    _IsOK = false;
                }
                else if (Attributes.ContainsKey(197) && Attributes[197].Data != 0)
                {
                    _IsOK = false;
                }
                else if (Attributes.ContainsKey(198) && Attributes[198].Data != 0)
                {
                    _IsOK = false;
                }
                else if (Attributes.ContainsKey(201) && Attributes[201].Data != 0)
                {
                    _IsOK = false;
                }
                return _IsOK;
            }
            set { }
        }
        [DataMember]
        public bool IsOK { get; set; }

        [DataMember]
        public long Size { get; set; }
        public string PNPDeviceID { get; private set; }
        public bool SmartSupported { get; set; } = false;
        [DataMember]

        public string Model { get; set; }
        [DataMember]

        public string Type { get; set; }
        [DataMember]

        public string Serial { get; set; }
        [DataMember]

        public string DriveIndex { get; set; }

        public bool SupportBiosBoot { get { return Size < 2000; } }

        [IgnoreDataMember]
        public Dictionary<int, Smart> Attributes = new Dictionary<int, Smart>();

        [IgnoreDataMember]
        public static Dictionary<int, string> AttributesTamplates = new Dictionary<int, string>() {
        {1,"read error rate"},
        {2,"Throughput performance"},
        {3,"Spin up time"},
        {4,"Start/Stop count"},
        {5,"Reallocated sector count"},
        {6,"Read channel margin"},
        {7,"Seek error rate"},
        {8,"Seek timer performance"},
        {9,"Power-on hours count"},
        {10,"Spin up retry count"},
        {11,"Calibration retry count"},
        {12,"Power cycle count"},
        {13,"Soft read error rate"},
        {22,"Current Helium Level"},
        {160,"Uncorrectable sector count read or write"},
        {161,"Remaining spare block percentage"},
        {163,"Inital Invalid Blocks"},
        {164,"Total Erase Count"},
        {165,"Maximum Erase Count"},
        {166,"Minimum Erase Count"},
        {167,"Average Erase Count"},
        {168,"Max NAND Erase Count from specification"},
        {169,"Remaining life percentage"},
        {170,"Available Reserved Space"},
        {171,"SSD Program Fail Count"},
        {172,"SSD Erase Fail Count"},
        {173,"SSD Wear Leveling Count"},
        {174,"Unexpected Power Loss Count"},
        {175,"Power Loss Protection Failure"},
        {176,"Erase Fail Count"},
        {177,"Wear Range Delta"},
        {178,"Used Reserved Block Count (Chip)"},
        {179,"Used Reserved Block Count (Total)"},
        {180,"Unused Reserved Block Count Total"},
        {181,"Program Fail Count Total or Non 4K Aligned Access Count"},
        {182,"Erase Fail Count"},
        {183,"SATA Down shift Error Count"},
        {184,"End-to-End error"},
        {185,"Head Stability"},
        {186,"Induced Op Vibration Detection"},
        {187,"Reported Uncorrectable Errors"},
        {188,"Command Timeout"},
        {189,"High Fly Writes"},
        {190,"Temperature Difference from 100"},
        {191,"G-sense error rate"},
        {192,"Power-off retract count"},
        {193,"Load/Unload cycle count"},
        {194,"Temperature"},
        {195,"Hardware ECC recovered"},
        {196,"Reallocation count"},
        {197,"Current pending sector count"},
        {198,"Off-line scan uncorrectable count"},
        {199,"UDMA CRC error rate"},
        {200,"Write error rate"},
        {201,"Soft read error rate"},
        {202,"Data Address Mark errors"},
        {203,"Run out cancel"},
        {204,"Soft ECC correction"},
        {205,"Thermal asperity rate (TAR)"},
        {206,"Flying height"},
        {207,"Spin high current"},
        {208,"Spin buzz"},
        {209,"Off-line seek performance"},
        {210,"Vibration During Write"},
        {211,"Vibration During Write"},
        {212,"Shock During Write"},
        {220,"Disk shift"},
        {221,"G-sense error rate"},
        {222,"Loaded hours"},
        {223,"Load/unload retry count"},
        {224,"Load friction"},
        {225,"Load/Unload cycle count"},
        {226,"Load-in time"},
        {227,"Torque amplification count"},
        {228,"Power-off retract count"},
        {230,"Life Curve Status"},
        {231,"SSD Life Left"},
        {232,"Endurance Remaining"},
        {233,"Media Wear out Indicator"},
        {234,"Average Erase Count AND Maximum Erase Count"},
        {235,"Good Block Count AND System Free Block Count"},
        {240,"Head flying hours"},
        {241,"Lifetime Writes From Host GiB"},
        {242,"Lifetime Reads From Host GiB"},
        {243,"Total LBAs Written Expanded"},
        {244,"Total LBAs Read Expanded"},
        {245,"Cumulative Program NAND Pages"},
        {249,"NAND Writes GiB"},
        {250,"Read error retry rate"},
        {251,"Minimum Spares Remaining"},
        {252,"Newly Added Bad Flash Block"},
        {254,"Free Fall Protection"}
            };

        public override string ToString()
        {
            string Data = "Model: " + Model + "\r\n";
            Data += "S/N: " + Serial + "\r\n";
            Data += "Drive Index: " + Index.ToString() + "\r\n";
            Data += "Drive Type: " + Type + "\r\n";
            Data += "Drive Is OK: " + ((IsOKCustom) ? "True" : "False") + "\r\n";
            Data += "Drive Supports Smart: " + ((SmartSupported) ? "True" : "False") + "\r\n";
            Data += "Smart Attributes\r\n--------------------------------------------------------------------------------------------------------\r\n";
            Data += String.Format("|{0,5}|{1,34}|{2,10}|{3,10}|{4,10}|{5,10}|{6,8}|{7,8}|\r\n", "index", "Attribute", "Data", "Current", "Worst", "Threshold", "IsOK", "HasData");
            Data += "--------------------------------------------------------------------------------------------------------\r\n";
            foreach (var item in Attributes)
            {
                Data += String.Format("|{0,5}|{1,34}|{2,10}|{3,10}|{4,10}|{5,10}|{6,8}|{7,8}|\r\n", "0x" + item.Key.ToString("X"), item.Value.Attribute, item.Value.Data, item.Value.Current, item.Value.Worst, item.Value.Threshold, item.Value.IsOK, item.Value.HasData);
            }
            Data += "--------------------------------------------------------------------------------------------------------\r\n";

            return Data;
        }

        public override void DisplayDebug()
        {
            throw new NotImplementedException();
        }

        public override void DisplayDebugWithBuffer()
        {
            throw new NotImplementedException();
        }

        public class Smart
        {
            public bool HasData
            {
                get
                {
                    if (Current == 0 && Worst == 0 && Threshold == 0 && Data == 0)
                        return false;
                    return true;
                }
            }
            public string Attribute { get; set; }
            public int Current { get; set; }
            public int Worst { get; set; }
            public int Threshold { get; set; }
            public int Data { get; set; }
            public bool IsOK { get; set; }

            public Smart()
            {

            }

            public Smart(string attributeName)
            {
                this.Attribute = attributeName;
            }
        }

    }
}
