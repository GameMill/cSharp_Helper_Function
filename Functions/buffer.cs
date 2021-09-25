using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Functions;

namespace DoubleBuffer
{
    
   
    public class Current_Buffer
    {
         public static readonly Dictionary<string, int> Color
                = new Dictionary<string, int>
             {
                 { "Black", 1 },
                 { "DarkBlue", 2 },
                 { "DarkGreen", 3 },
                 { "DarkCyan", 4 },
                 { "DarkRed", 5 },
                 { "DarkMagenta", 6 },
                 { "DarkYellow", 7 },
                 { "DarkGray", 8 },
                 { "Blue", 9 },
                 { "Green", 10 },
                 { "Cyan", 11 },
                 { "Red", 12 },
                 { "Magenta", 13 },
                 { "Yellow", 14 },
                 { "White", 15 },
             };
    
        public static buffer Current_buffer;

        public static void New_Buffer()
        {
            Current_buffer = new buffer(Console.WindowWidth, Console.WindowHeight, Console.WindowWidth, Console.WindowHeight);
        }
    }


    public class buffer  : Core
    {
        private int width;
        private int height;
        private int windowWidth;
        private int windowHeight;
        private SafeFileHandle h;
        private CharInfo[] buf;
        private SmallRect rect;

        int row = 0;
        int col = 0;
        short color = 15;
        ConsoleColor FGColor = ConsoleColor.White;
        ConsoleColor BGColor = ConsoleColor.Black;



        public void WriteLine(string Text)
        {
            Draw(Text, col, row, (short)color);
            row++;
            col = 0;
        }

        public void WriteLine()
        {
            WriteLine(new String(' ', Console.WindowWidth), col, row, (short)color);
        }

        public void WriteLineWithCursor(string Text)
        {
            Console.SetCursorPosition(Text.Length, row);
            this.WriteLine(Text);
        }
        public void WriteLineWithCursor(string Format, params object[] arg)
        {
            WriteLineWithCursor(string.Format(Format, arg));

        }

        public void WriteLine(string Format,params object[] arg)
        {
            WriteLine(string.Format(Format, arg));
        }

        public void SetForgroundColor(ConsoleColor color)
        {
            if (FGColor == color)
            {
                this.FGColor = color;
                this.color = GetColor(BGColor, FGColor);
                
            }
        }
        public void SetBackgroundColor(ConsoleColor color)
        {
            if (FGColor == color)
            {
                this.BGColor = color;
                this.color = GetColor(BGColor, FGColor);
            }
        }


        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);


        public int Row { get { return row; } set { row = value; } }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            private short X;
            private short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)]
            public char UnicodeChar;
            [FieldOffset(0)]
            public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)]
            public CharUnion Char;
            [FieldOffset(2)]
            public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            private short Left;
            private short Top;
            private short Right;
            private short Bottom;
            public void setDrawCord(short l, short t)
            {
                Left = l;
                Top = t;
            }
            public void setWindowSize(short r, short b)
            {
                Right = r;
                Bottom = b;
            }
        }

        /// <summary>
        /// Consctructor class for the buffer. Pass in the width and height you want the buffer to be.
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// 
        public buffer()
        {
            buffer2(Console.WindowWidth, Console.WindowHeight, Console.WindowWidth, Console.WindowHeight);
        }
        public buffer(int Width, int Height, int wWidth, int wHeight)
        {
            buffer2(Width, Height, wWidth, wHeight);
        }


        private void buffer2(int Width, int Height, int wWidth, int wHeight) // Create and fill in a multideminsional list with blank spaces.
        {
            Console.CursorVisible = false;
            if (Width > wWidth || Height > wHeight)
            {
                throw new System.ArgumentException("The buffer width and height can not be greater than the window width and height.");
            }
            h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            width = Width;
            height = Height;
            windowWidth = wWidth;
            windowHeight = wHeight;
            buf = new CharInfo[width * height];
            rect = new SmallRect();
            rect.setDrawCord(0, 0);
            rect.setWindowSize((short)windowWidth, (short)windowHeight);
            Clear();
        }
        /// <summary>
        /// This method draws any text to the buffer with given color.
        /// To chance the color, pass in a value above 0. (0 being black text, 15 being white text).
        /// Put in the starting width and height you want the input string to be.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="attribute"></param>
        public void Draw(String str, int Width, int Height, short attribute) //Draws the image to the buffer
        {
            if (Width > windowWidth - 1 || Height > windowHeight - 1)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if (str != null)
            {
                Char[] temp = str.ToCharArray();
                int tc = 0;
                foreach (Char le in temp)
                {
                    buf[(Width + tc) + (Height * width)].Char.AsciiChar = (byte)(int)le; //Height * width is to get to the correct spot (since this array is not two dimensions).
                    if (attribute != 0)
                        buf[(Width + tc) + (Height * width)].Attributes = attribute;
                    tc++;
                }
            }
        }

        public static short GetColor(ConsoleColor BG, ConsoleColor FG)
        {
            int BGColor = ((int)BG * 16);
            BGColor += (int)FG;
            return (short)BGColor;
        }

        public
        enum
        Color
        {
            Black = 0,
            DarkBlue = 1,
            DarkGreen = 2,
            DarkCyan = 3,
            DarkRed = 4,
            DarkMagenta = 5,
            DarkYellow = 6,
            Gray = 7,
            DarkGray = 8,
            Blue = 9,
            Green = 10,
            Cyan = 11,
            Red = 12,
            Magenta = 13,
            Yellow = 14,
            White = 15,
            
        }
        /// <summary>
        /// Prints the buffer to the screen.
        /// </summary>
        public void Print() //Paint the image
        {
            if (!h.IsInvalid)
            {

                bool b = WriteConsoleOutput(h, buf, new Coord((short)width, (short)height), new Coord((short)0, (short)0), ref rect);
            }
        }
        /// <summary>
        /// Clears the buffer and resets all character values back to 32, and attribute values to 1.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i].Attributes = 1;
                buf[i].Char.AsciiChar = 32;
            }
            col = 0;
            row = 0;
        }
        /// <summary>
        /// Pass in a buffer to change the current buffer.
        /// </summary>
        /// <param name="b"></param>
        public void setBuf(CharInfo[] b)
        {
            if (b == null)
            {
                throw new System.ArgumentNullException();
            }

            buf = b;
        }

        /// <summary>
        /// Set the x and y cordnants where you wish to draw your buffered image.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void setDrawCord(short x, short y)
        {
            rect.setDrawCord(x, y);
        }

        /// <summary>
        /// Clear the designated row and make all attribues = 1.
        /// </summary>
        /// <param name="row"></param>
        public void clearRow(int row)
        {
            for (int i = (row * width); i < ((row * width + width)); i++)
            {
                if (row > windowHeight - 1)
                {
                    throw new System.ArgumentOutOfRangeException();
                }
                buf[i].Attributes = 0;
                buf[i].Char.AsciiChar = 32;
            }
        }

        /// <summary>
        /// Clear the designated column and make all attribues = 1.
        /// </summary>
        /// <param name="col"></param>
        public void clearColumn(int col)
        {
            if (col > windowWidth - 1)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            for (int i = col; i < windowHeight * windowWidth; i += windowWidth)
            {
                buf[i].Attributes = 0;
                buf[i].Char.AsciiChar = 32;
            }
        }

        /// <summary>
        /// This function return the character and attribute at given location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// byte character
        /// byte attribute
        /// </returns>
        public KeyValuePair<byte, byte> getCharAt(int x, int y)
        {
            if (x > windowWidth || y > windowHeight)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            return new KeyValuePair<byte, byte>((byte)buf[((y * width + x))].Char.AsciiChar, (byte)buf[((y * width + x))].Attributes);
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
