#define LCD

using System.Threading;
using Microsoft.SPOT;
using testMicroToolsKit.Hardware.IO;
using Microtoolskit.Hardware.IO;
using Microtoolskit.Hardware.Displays;


namespace FezPanda
{
    public class Program
    {
        public static void Main()
        {
            byte Nb = 3; // Channels on test in single mode
            bool commut = true;

            MCP342x can = new MCP342x();
            PCF8574 leds = new PCF8574();
#if LCD
            ELCD162 lcd = new ELCD162("com1");
            lcd.Init(); lcd.ClearScreen(); lcd.CursorOff();
#endif

            // One Shot Conversion
#if LCD
            lcd.PutString("One Shot Conv.");
            Thread.Sleep(2000);
#endif
            can.Mode = MCP342x.ConversionMode.OneShot;
            double resolution = Resolution(can.Resolution);
            double gain = System.Math.Pow(2, (byte)can.Gain);

            for (int i = 0; i < Nb; i++)
            {

                try
                {
                    Debug.Print("Single on channel " + (can.CHannel + 1) + " => Tension= " + can.ReadVolts().ToString("F2") +
                        "   " + "Resol: " + resolution + "-bit " + "Gain: " + gain);
                    can.CHannel = (MCP342x.Channel)(i + 1);
                }
                catch (System.IO.IOException ex)
                {
#if LCD
                    lcd.ClearScreen(); lcd.SetCursor(0, 0); lcd.PutString(ex.Message);
#else 
                    Debug.Print(ex.Message);
#endif
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }

            // Continuous Conversion mode 
#if LCD
            lcd.ClearScreen();
            lcd.PutString("Continuous Conv.");
#endif
            can.Mode = MCP342x.ConversionMode.Continuous;
            byte j = 1;

            while (true)
            {
                if (commut)
                {
                    try
                    {
                        leds.Write(0xF0);
                    }
                    catch (System.IO.IOException ex)
                    {
#if LCD
                        lcd.ClearScreen(); lcd.SetCursor(0, 1); lcd.PutString(ex.Message); Thread.Sleep(1000);
#else
                        Debug.Print(ex.Message);
#endif
                    }
                    finally
                    {
                        commut = false;
                        can.CHannel = (MCP342x.Channel)(j - 1); j = 2;
                    }
                }
                else
                {
                    try
                    {
                        leds.Write(0x0F);
                    }
                    catch (System.IO.IOException ex)
                    {
#if LCD
                        lcd.ClearScreen(); lcd.SetCursor(0, 1); lcd.PutString(ex.Message); Thread.Sleep(1000);
#else
                        Debug.Print(ex.Message);
#endif
                    }
                    finally
                    {
                        commut = true;
                        can.CHannel = (MCP342x.Channel)(j - 1); j = 1;
                    }
                }


                try
                {
#if LCD
                    lcd.ClearScreen();
                    lcd.SetCursor(0, 0); lcd.PutString("Ch" + (can.CHannel + 1) + " U=" + can.ReadVolts().ToString("F2") + "V");
#else
                    Debug.Print("Continuous on channel " + (can.CHannel + 1) + " =>Tension= " + can.ReadVolts().ToString("F2") + "   " +
                    "Resol: " + resolution + "-bit " + "Gain: " + gain);
#endif
                }
                catch (System.IO.IOException ex)
                {
#if LCD
                    lcd.ClearScreen(); lcd.SetCursor(0, 0); lcd.PutString(ex.Message);
#else
                    Debug.Print(ex.Message);
#endif
                }
                finally
                {
                    Thread.Sleep(1000);
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
    }
}
