using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Functions
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;

        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    };


    public class NativeMethods
    {
        // Affirms that the platform invoke
        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(
          string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(
              ref DEVMODE devMode, int flags);

        // Control to change the screen resolution.
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;

        // Constant definition control to change direction
        public const int DMDO_DEFAULT = 0;
        public const int DMDO_90 = 1;
        public const int DMDO_180 = 2;
        public const int DMDO_270 = 3;
    }
    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class Screen
    {
        public static Size GetMaximumScreenSizePrimary()
        {
            var scope = new System.Management.ManagementScope();
            var q = new System.Management.ObjectQuery("SELECT * FROM CIM_VideoControllerResolution");

            using (var searcher = new System.Management.ManagementObjectSearcher(scope, q))
            {
                var results = searcher.Get();
                UInt32 maxHResolution = 0;
                UInt32 maxVResolution = 0;

                foreach (var item in results)
                {
                    if ((UInt32)item["HorizontalResolution"] > maxHResolution)
                        maxHResolution = (UInt32)item["HorizontalResolution"];

                    if ((UInt32)item["VerticalResolution"] > maxVResolution)
                        maxVResolution = (UInt32)item["VerticalResolution"];
                }

                return new Size { Width = Convert.ToInt32(maxHResolution), Height = Convert.ToInt32(maxVResolution) };

            }
        }
        public static void ChangeResolution(int width, int height)
        {
            // Initialize the DEVMODE structure
            DEVMODE devmode = new DEVMODE();
            devmode.dmDeviceName = new String(new char[32]);
            devmode.dmFormName = new String(new char[32]);
            devmode.dmSize = (short)Marshal.SizeOf(devmode);

            if (0 != NativeMethods.EnumDisplaySettings(null, NativeMethods.ENUM_CURRENT_SETTINGS, ref devmode))
            {
                devmode.dmPelsWidth = width;
                devmode.dmPelsHeight = height;

                // Change the screen resolution
                int iRet = NativeMethods.ChangeDisplaySettings(ref devmode, NativeMethods.CDS_TEST);

                if (iRet == NativeMethods.DISP_CHANGE_FAILED)
                {
                    throw new Exception("Can`t change display settings");
                }
                else
                {
                    iRet = NativeMethods.ChangeDisplaySettings(ref devmode, NativeMethods.CDS_UPDATEREGISTRY);

                    switch (iRet)
                    {
                        // Successful change
                        case NativeMethods.DISP_CHANGE_SUCCESSFUL:
                            {
                                break;
                            }
                        case NativeMethods.DISP_CHANGE_RESTART:
                            {
                                throw new Exception("You need to restart your computer settings to take effect");
                            }
                        default:
                            {
                                throw new Exception("Change the screen resolution failed");
                            }
                    }
                }
            }
        }
    }
}
