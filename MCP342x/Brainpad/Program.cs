using BrainpadLibrary;
//using testMicroToolsKit.Hardware.IO; // Source code
using Microtoolskit.Hardware.IO;  // Nuget
using System.Threading;

namespace Brainpad
{
    public class Program
    {
        public static void Main()
        {
            byte Nb = 4; // Channels on MCP342x
           
            MCP342x can = new MCP342x();
            PCF8574 leds = new PCF8574();

            can.Gain = MCP342x.PGA_Gain.x8;
            can.Mode = MCP342x.ConversionMode.OneShot;
            can.Resolution = MCP342x.SampleRate.FourteenBits;

            double resolution = Resolution(can.Resolution);
            double gain = System.Math.Pow(2, (byte)can.Gain);

            // First message on graphic display
            BrainPad.Display.DrawLargeText(10, 10, "BrainPad", BrainPad.Color.Yellow);
            BrainPad.Display.DrawLargeText(25, 30, "MCP342x", BrainPad.Color.Yellow);

            // 1. One Shot Conversion
            BrainPad.Display.DrawText(5, 50, "One Shot Conversion", BrainPad.Color.Yellow);
            BrainPad.Display.DrawText(5, 60, "n:" + resolution + "-bit" + " Gain:" + gain + " sample:5s", BrainPad.Color.Yellow);

            for (int i = 0; i < Nb; i++)
            {
                try
                {
                    var voltage = can.ReadVolts(); var temperature = voltage / 0.01;
                    BrainPad.Display.DrawText(5, 80, "On channel " + (can.CHannel + 1), BrainPad.Color.Yellow);
                    BrainPad.Display.DrawText(10, 90, "=> Voltage = " + voltage.ToString("F2") + "V", BrainPad.Color.Yellow);
                    BrainPad.Display.DrawText(10, 100, "=> Temperature = " + temperature.ToString("F1") + "degC", BrainPad.Color.Yellow);
                    can.CHannel = (MCP342x.Channel)(i + 1);
                }
                catch (System.IO.IOException ex)
                {
                    displayErrorMessage(5, 120, ex);
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }

            // 2. Clear screen
            BrainPad.Display.Clear();
            BrainPad.Display.DrawLargeText(10, 10, "BrainPad", BrainPad.Color.Yellow);
            BrainPad.Display.DrawLargeText(25, 30, "MCP342x", BrainPad.Color.Yellow);

            // 3. Continuous Conversion mode 
            BrainPad.Display.DrawText(5, 50, "Continuous Conversion", BrainPad.Color.Yellow);
            BrainPad.Display.DrawText(5, 60, "n:" + resolution + "-bit" + " Gain:" + gain + " sample:2s", BrainPad.Color.Yellow);

            can.Mode = MCP342x.ConversionMode.Continuous;
            uint state = 1;

            while (true)
            {
                switch (state)
                {
                    case 1:
                        try
                        {
                            leds.Write(0xFE);
                        }
                        catch (System.IO.IOException ex)
                        {
                            displayErrorMessage(5, 120, ex);
                        }
                        finally
                        {
                            can.CHannel = MCP342x.Channel.Ch1; state = 2;
                        }
                        break;

                    case 2:
                        try
                        {
                            leds.Write(0xFD);
                        }
                        catch (System.IO.IOException ex)
                        {
                            displayErrorMessage(5, 120, ex);
                        }
                        finally
                        {
                            can.CHannel = MCP342x.Channel.Ch2; state = 3;
                        }
                        break;

                    case 3:
                        try
                        {
                            leds.Write(0xFB);
                        }
                        catch (System.IO.IOException ex)
                        {
                            displayErrorMessage(5, 120, ex);
                        }
                        finally
                        {
                            can.CHannel = MCP342x.Channel.Ch3; state = 4;
                        }
                        break;

                    case 4:
                        try
                        {
                            leds.Write(0xF7);
                        }
                        catch (System.IO.IOException ex)
                        {
                            displayErrorMessage(5, 120, ex);
                        }
                        finally
                        {
                            can.CHannel = MCP342x.Channel.Ch4; state = 1;
                        }
                        break;
                }

                try
                {
                    var voltage = can.ReadVolts(); var temperature = voltage / 0.01;
                    BrainPad.Display.DrawText(5, 80, "On channel " + (can.CHannel + 1), BrainPad.Color.Yellow);
                    BrainPad.Display.DrawText(10, 90, "=> Voltage = " + voltage.ToString("F2") + "V", BrainPad.Color.Yellow);
                    BrainPad.Display.DrawText(10, 100, "=> Temperature = " + temperature.ToString("F1") + "degC", BrainPad.Color.Yellow);
                }
                catch (System.IO.IOException ex)
                {
                    displayErrorMessage(5, 120, ex);
                }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }

        static byte Resolution(MCP342x.SampleRate res)
        {
            byte value = 0;

            switch (res)
            {
                case MCP342x.SampleRate.TwelveBits: value = 12; break;
                case MCP342x.SampleRate.FourteenBits: value = 14; break;
                case MCP342x.SampleRate.SixteenBits: value = 16; break;
                case MCP342x.SampleRate.EighteenBits: value = 18; break;
            }
            return value;
        }
        static void displayErrorMessage(int x, int y, System.IO.IOException ex)
        {
            BrainPad.Display.DrawText(x, y, ex.Message, BrainPad.Color.Red);
            Thread.Sleep(2000);
            BrainPad.Display.DrawText(5, 120, "                          ", BrainPad.Color.Red);
        }
    }
}
