#define LCD

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

using testMicroToolsKit.Hardware.IO;
using Microtoolskit.Hardware.IO;
using Microtoolskit.Hardware.Displays;

namespace Netduino
{
    public class Program
    {
        public static void Main()
        {
            MCP342x can = new MCP342x();
            PCF8574 leds = new PCF8574();
#if LCD
            ELCD162 lcd = new ELCD162();
            lcd.Init(); lcd.ClearScreen(); lcd.CursorOff();
#endif
            bool commut = false;

            // One Shot Conversion
#if LCD
            lcd.PutString("One Shot Conv.");
            Thread.Sleep(2000);
#endif
            for (int i = 1; i < 4; i++)
            {
                can.Mode = MCP342x.ConversionMode.OneShot;
                double resolution = Resolution(can.Resolution);
                double gain = System.Math.Pow(2, (byte)can.Gain);

                try
                {
                    Debug.Print("Single on channel " + i + " => Tension= " + can.ReadVolts().ToString("F2") + "   " + "Resol: " + resolution + "-bit " + "Gain: " + gain);
                }
                catch (System.IO.IOException ex)
                {
#if LCD
                    lcd.ClearScreen(); lcd.PutString(ex.Message);
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
            lcd.ClearScreen(); lcd.PutString("Continuous Conv.");
#endif
            can.Mode = MCP342x.ConversionMode.Continuous;

            while (true)
            {
                byte j = 0;
                double resolution = Resolution(can.Resolution);
                double gain = System.Math.Pow(2, (byte)can.Gain);

                if (commut)
                {
                    try
                    {
                        leds.Write(0xF0);
                    }
                    catch (System.IO.IOException ex)
                    {
#if LCD
                        lcd.ClearScreen(); lcd.PutString(ex.Message);
#else
                            Debug.Print(ex.Message);
#endif
                    }
                    finally
                    {
                        commut = false;
                        can.CHannel = MCP342x.Channel.Ch1; j = 1;
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
                        lcd.ClearScreen(); lcd.PutString(ex.Message);
#else
                            Debug.Print(ex.Message);
#endif
                    }
                    finally
                    {
                        commut = true;
                        can.CHannel = MCP342x.Channel.Ch2; j = 2;
                    }
                }


            try
            {
                Debug.Print("Continuous on channel " + j + " =>Tension= " + can.ReadVolts().ToString("F2") + "   " + "Resol: " + resolution + "-bit " + "Gain: " + gain);
            }
            catch (System.IO.IOException ex)
            {
#if LCD
                lcd.ClearScreen(); lcd.PutString(ex.Message);
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
