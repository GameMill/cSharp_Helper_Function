using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace Functions
{
    public class Battery
    {

        public static int Main_Powered = 1; 
        public static int Battery_Pent = -1;
        public static int batteryerror = -1;
        private static Dictionary<string, int> Get()
        {
            Dictionary<string, int> Return = new Dictionary<string, int>();

            try
            {
                System.Management.ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject mo in searcher.Get())
                { 
                    int BatteryStatus = int.Parse(mo["BatteryStatus"].ToString());
                    int EstimatedChargeRemaining = int.Parse(mo["EstimatedChargeRemaining"].ToString());
                    if (BatteryStatus > 1)
                    { Main_Powered = 1; }
                    else
                    { Main_Powered = 0; }

                    Battery_Pent = EstimatedChargeRemaining;
                    batteryerror = 0;
                    break;
                }
            }
            catch (Exception)
            {
                batteryerror = 1;
            }
            Return["Main_Powered"] = Main_Powered;
            Return["Battery_Pent"] = Battery_Pent;
            Return["Battery_Error"] = batteryerror;
            return Return;
        }

        public static void Wail_For_Charger()
        {
            Get();
            while (Main_Powered < 1 && batteryerror != 1) {  Functions.Header.New("Please Plug in The Charger & Press Any Key"); Console.ReadKey(); Get(); }
            Console.Clear();
        }

        public static bool Is_Main_Powered()
        {
            Get();
            return (Main_Powered == 1);
        }
    }
}
