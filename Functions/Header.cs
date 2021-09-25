using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class Header : Core
    {
        public static string Sign = "-";

        public static void New(string Header)
        {
            New(Header, Console.WindowWidth);
        }

        public static void New(string Header, ref DoubleBuffer.buffer buffer)
        {
            if (buffer == null)
                New(Header);
            else
                New(Header, Console.WindowWidth, ref buffer);
        }

        public static void New(string Header,int Width)
        {
            Console.Clear();
            int Line_length = Width;
            int Text_length = Header.Length;
            string[] Text = Header.Split( new[] { Environment.NewLine }, StringSplitOptions.None);


            for (int i = 0; i < Line_length; i++) { Console.Write(Sign); }

            foreach (var Line in Text)
            {
                int Text_Line_length = ((Width - Line.Length) - 4) / 2;
                int Test_Text_Line_length = Text_Line_length + Text_Line_length + 4 + Text_length;

                for (int i = 0; i < Text_Line_length; i++) { Console.Write(Sign); }

                Console.Write("| "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(Line); Console.ForegroundColor = ConsoleColor.White; Console.Write(" |");
                if (Test_Text_Line_length == (Width - 1)) { Console.Write(Sign); }

                for (int i = 0; i < Text_Line_length; i++) { Console.Write(Sign); }

            }




            for (int i = 0; i < Line_length; i++) { Console.Write(Sign); }
            Console.WriteLine();
        }

        public static void New(string Header, int Width, ref DoubleBuffer.buffer buffer)
        {
            int Line_length = Width;

            string Line = new String(Sign[0], Line_length);
            string[] Text = Header.Split(new[] { Environment.NewLine }, StringSplitOptions.None);


            buffer.Draw(Line, 0, 0, 15); buffer.Draw(Line, 0, 2, 15);
            int h = 1;
            foreach (var Line2 in Text)
            {
                int Text_length = Line2.Length;

                int Text_Line_length = ((Width - Line2.Length) - 4) / 2;
                int Test_Text_Line_length = Text_Line_length + Text_Line_length + 4 + Text_length;
                string Small_Line = new String(Sign[0], Text_Line_length);


                buffer.Draw(Small_Line + "| ", 0, h, 15);

                buffer.Draw(Line2, Text_Line_length + 2, h, 14);


                buffer.Draw(" |" + Small_Line, ((Text_Line_length + Text_length) + 2), h, 15);

                if (Test_Text_Line_length == (Width - 1)) { buffer.Draw(Sign, 100, h, 15); }
                h++;
            }
            buffer.Draw(Line, 0, 0, 15); buffer.Draw(Line, 0, h, 15);
            buffer.Row = h + 2;

            buffer.Print();



        }

        public static void Line()
        {
            for (int i = 0; i < Console.WindowWidth; i++) { Console.Write(Sign); }
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
