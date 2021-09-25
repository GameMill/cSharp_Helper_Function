using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functions
{
    public class Progress_BarV2 : Core
    {
        private static int width = Console.WindowWidth;
        private static int height = Console.WindowHeight;
       // private static int Use_Mini_Progress;
        public static void Start()
        {
            /*
            //Last = -1;
            if (Console.LargestWindowWidth < 104)
            {
                if (Console.LargestWindowWidth > 100)
                {
        //            Use_Mini_Progress = 2;
                    Console.WindowWidth = 100;
                    New_Buffer = new DoubleBuffer.buffer(100, 20, 100, 20);
                }
                else
                {
        //            Use_Mini_Progress = 1;
                    Console.WindowWidth = 52;
                    New_Buffer = new DoubleBuffer.buffer(52, 20, 52, 20);
                }
            }
            else
            {
        //        Use_Mini_Progress = 0;
                Console.WindowWidth = 104;
                New_Buffer = new DoubleBuffer.buffer(104, 20, 104, 20);
            }
            Console.WindowHeight = 20;*/
            New_Buffer = new DoubleBuffer.buffer(Console.WindowWidth, Console.WindowHeight, Console.WindowWidth, Console.WindowHeight);

        }

        public static void Start(ref DoubleBuffer.buffer buffer)
        {
            New_Buffer = buffer;
        }



        public static void Set_Progress(int Pent)
        {
            string[] Extra = { };
            Set_Progress(Pent, Extra);
        }

        //static int Last = -1;
        public static DoubleBuffer.buffer New_Buffer;
        public static void Set_Progress(int Pent, string[] Extra_Info)
        {
            short YellowColor = DoubleBuffer.buffer.GetColor(ConsoleColor.Red, ConsoleColor.Red); 
            short RedColor = DoubleBuffer.buffer.GetColor(ConsoleColor.Yellow, ConsoleColor.Yellow);
            var WindowWidth = Console.WindowWidth - 1;

            int Pent2 = ((WindowWidth) / 100) * Pent;
            var Width = (WindowWidth) - 4;
            Header.New("Progress: " + Pent + "%", ref New_Buffer);
            string Line = new String('#', WindowWidth+1);
            string Pren_Line = new String('#', Pent2);

            New_Buffer.Draw(Line, 0, 5, YellowColor);
            New_Buffer.Draw("#", 0, 6, YellowColor); New_Buffer.Draw(Pren_Line, 1, 6, RedColor); New_Buffer.Draw("#", WindowWidth, 6, YellowColor);
            New_Buffer.Draw("#", 0, 7, YellowColor); New_Buffer.Draw(Pren_Line, 1, 7, RedColor); New_Buffer.Draw("#", WindowWidth, 7, YellowColor);
            New_Buffer.Draw("#", 0, 8, YellowColor); New_Buffer.Draw(Pren_Line, 1, 8, RedColor); New_Buffer.Draw("#", WindowWidth, 8, YellowColor);
            New_Buffer.Draw("#", 0, 9, YellowColor); New_Buffer.Draw(Pren_Line, 1, 9, RedColor); New_Buffer.Draw("#", WindowWidth, 9, YellowColor);
            New_Buffer.Draw("#", 0, 10, YellowColor); New_Buffer.Draw(Pren_Line, 1, 10, RedColor); New_Buffer.Draw("#", WindowWidth, 10, YellowColor);
            New_Buffer.Draw(Line, 0, 11, YellowColor);

            int num = 13;
            foreach (var item in Extra_Info)
            {
                New_Buffer.Draw(item, 0, num, 15);
                num++;
            }
            New_Buffer.Print();

            /*
            if (Pent != Last)
            {
                Last = Pent;
                if (Use_Mini_Progress == 1)
                {
                    var Pent2 = Pent / 2;
                    Header.New("Progress: " + Pent + "%", 52, ref New_Buffer);
                    string Line = new String('#', 50);
                    string Pren_Line = new String('#', Pent2);
                    //Pren_Line += new String(' ', (50 - Pent2));


                    New_Buffer.Draw("#" + Line + "#", 0, 5, YellowColor);
                    New_Buffer.Draw("#", 0, 6, YellowColor); New_Buffer.Draw(Pren_Line, 1, 6, RedColor); New_Buffer.Draw("#", 51, 6, YellowColor);
                    New_Buffer.Draw("#", 0, 7, YellowColor); New_Buffer.Draw(Pren_Line, 1, 7, RedColor); New_Buffer.Draw("#", 51, 7, YellowColor);
                    New_Buffer.Draw("#", 0, 8, YellowColor); New_Buffer.Draw(Pren_Line, 1, 8, RedColor); New_Buffer.Draw("#", 51, 8, YellowColor);
                    New_Buffer.Draw("#", 0, 9, YellowColor); New_Buffer.Draw(Pren_Line, 1, 9, RedColor); New_Buffer.Draw("#", 51, 9, YellowColor);
                    New_Buffer.Draw("#", 0, 10, YellowColor); New_Buffer.Draw(Pren_Line, 1, 10, RedColor); New_Buffer.Draw("#", 51, 10, YellowColor);
                    New_Buffer.Draw("#" + Line + "#", 0, 11, YellowColor);

                    int num = 13;
                    foreach (var item in Extra_Info)
                    {
                        New_Buffer.Draw(item, 0, num, 15);
                        num++;
                    }
                }
                else
                {

                    string Line = new String('#', 100);
                    string Pren_Line = new String('#', Pent);
                    //Pren_Line += new String(' ', (100 - Pent));

                    if (Use_Mini_Progress == 2)
                    {
                        Header.New("Progress: " + Pent + "%", 100, ref New_Buffer);
                        New_Buffer.Draw(Line, 0, 5, YellowColor);
                        New_Buffer.Draw(Pren_Line, 0, 6, RedColor);
                        New_Buffer.Draw(Pren_Line, 0, 7, RedColor);
                        New_Buffer.Draw(Pren_Line, 0, 8, RedColor);
                        New_Buffer.Draw(Pren_Line, 0, 9, RedColor);
                        New_Buffer.Draw(Pren_Line, 0, 10, RedColor);
                        New_Buffer.Draw(Line, 0, 11, YellowColor);
                    }
                    else
                    {
                        Header.New("Progress: " + Pent + "%", 104, ref New_Buffer);
                        New_Buffer.Draw("##" + Line + "##", 0, 5, YellowColor);
                        New_Buffer.Draw("##", 0, 6, YellowColor); New_Buffer.Draw(Pren_Line, 2, 6, RedColor); New_Buffer.Draw("##", 102, 6, YellowColor);
                        New_Buffer.Draw("##", 0, 7, YellowColor); New_Buffer.Draw(Pren_Line, 2, 7, RedColor); New_Buffer.Draw("##", 102, 7, YellowColor);
                        New_Buffer.Draw("##", 0, 8, YellowColor); New_Buffer.Draw(Pren_Line, 2, 8, RedColor); New_Buffer.Draw("##", 102, 8, YellowColor);
                        New_Buffer.Draw("##", 0, 9, YellowColor); New_Buffer.Draw(Pren_Line, 2, 9, RedColor); New_Buffer.Draw("##", 102, 9, YellowColor);
                        New_Buffer.Draw("##", 0, 10, YellowColor); New_Buffer.Draw(Pren_Line, 2, 10, RedColor); New_Buffer.Draw("##", 102, 10, YellowColor);
                        New_Buffer.Draw("##" + Line + "##", 0, 11, YellowColor);
                    }


                    int num = 13;
                    foreach (var item in Extra_Info)
                    {
                        New_Buffer.Draw(item, 0, num, 15);
                        num++;
                    }
                }
                */

            New_Buffer.Print();
            }

        public static void End()
        {
            //Last = -1;
            New_Buffer.Clear();
            Console.WindowWidth = width;
            Console.WindowHeight = height;
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
