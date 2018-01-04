/*
Copyright GHI Electronics, LLC 2015

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using GHI.Pins;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;

namespace BrainpadLibrary
{
    /// <summary>
    /// The BrainPad class used with GHI Electronics's BrainPad.
    /// </summary>
    public static class BrainPad
    {
        /// <summary>
        /// A constant value that is always true for endless looping.
        /// </summary>
        public const bool Looping = true;

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(string message)
        {
            Microsoft.SPOT.Debug.Print(message);
        }

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(int message)
        {
            WriteDebugMessage(message.ToString());
        }

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteDebugMessage(double message)
        {
            WriteDebugMessage(message.ToString());
        }

        /// <summary>
        /// Represents a color made up of red, green, and blue.
        /// </summary>
        public class Color
        {
            /// <summary>
            /// The amount of red.
            /// </summary>
            public byte R { get; set; }

            /// <summary>
            /// The amount of green.
            /// </summary>
            public byte G { get; set; }

            /// <summary>
            /// The amount of blue.
            /// </summary>
            public byte B { get; set; }

            /// <summary>
            /// The color in 565 format.
            /// </summary>
            public ushort As565
            {
                get
                {
                    return (ushort)(((this.R & 0x1F) << 11) | ((this.G & 0x3F) << 5) | (this.B & 0x1F));
                }
            }

            /// <summary>
            /// Constructs a new instance with the given levels.
            /// </summary>
            public Color()
                : this(0, 0, 0)
            {

            }

            /// <summary>
            /// Constructs a new instance with the given levels.
            /// </summary>
            /// <param name="red">The amount of red.</param>
            /// <param name="green">The amount of green.</param>
            /// <param name="blue">The amount of blue.</param>
            public Color(byte red, byte green, byte blue)
            {
                this.R = red;
                this.G = green;
                this.B = blue;
            }

            /// <summary>
            /// A predefined color for black.
            /// </summary>
            public static Color Black = new Color(0, 0, 0);
            /// <summary>
            /// A predefined color for white.
            /// </summary>
            public static Color White = new Color(255, 255, 255);
            /// <summary>
            /// A predefined color for red.
            /// </summary>
            public static Color Red = new Color(255, 0, 0);
            /// <summary>
            /// A predefined color for green.
            /// </summary>
            public static Color Green = new Color(0, 255, 0);
            /// <summary>
            /// A predefined color for blue.
            /// </summary>
            public static Color Blue = new Color(0, 0, 255);
            /// <summary>
            /// A predefined color for yellow.
            /// </summary>
            public static Color Yellow = new Color(255, 255, 0);
            /// <summary>
            /// A predefined color for cyan.
            /// </summary>
            public static Color Cyan = new Color(0, 255, 255);
            /// <summary>
            /// A predefined color for magneta.
            /// </summary>
            public static Color Magneta = new Color(255, 0, 255);
        }

        /// <summary>
        /// Represents an image that can be reused.
        /// </summary>
        public class Image
        {
            public int Width { get; private set; }
            public int Height { get; private set; }
            public byte[] Pixels { get; private set; }

            /// <summary>
            /// Constructs a new image with the given dimensions.
            /// </summary>
            /// <param name="width">The image width.</param>
            /// <param name="height">The image height.</param>
            public Image(int width, int height)
            {
                if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
                if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

                this.Width = width;
                this.Height = height;
                this.Pixels = new byte[width * height * 2];
            }

            /// <summary>
            /// Sets the given pixel to the given color.
            /// </summary>
            /// <param name="x">The x coordinate of the pixel to set.</param>
            /// <param name="y">The y coordinate of the pixel to set.</param>
            /// <param name="color">The color to set the pixel to.</param>
            public void SetPixel(int x, int y, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (x > this.Width) throw new ArgumentOutOfRangeException("x", "x must not exceed Width.");
                if (y > this.Height) throw new ArgumentOutOfRangeException("y", "y must not exceed Height.");

                this.Pixels[(x * Width + y) * 2 + 0] = (byte)(color.As565 >> 8);
                this.Pixels[(x * Width + y) * 2 + 1] = (byte)(color.As565 >> 0);
            }
        }

        /// <summary>
        /// Provices access to the DC motor on the Brainpad.
        /// </summary>
        public static class DcMotor
        {
            private static PWM output;
            private static bool started;

            static DcMotor()
            {
                started = false;
                output = new PWM(Peripherals.DcMotor, 2175, 0, false);
            }

            /// <summary>
            /// Sets the speed of the DC Motor.
            /// </summary>
            /// <param name="speed">A value between 0 (off) and 1 (max speed).</param>
            public static void SetSpeed(double speed)
            {
                if (speed > 1 | speed < 0) throw new ArgumentOutOfRangeException("speed", "speed must be between 0 and 1.");

                if (speed == 1.0)
                    speed = 0.99;

                if (started)
                    output.Stop();

                output.DutyCycle = speed;
                output.Start();

                started = true;
            }

            public static void Stop()
            {
                if (started)
                {
                    output.Stop();

                    started = false;
                }
            }
        }

        /// <summary>
        /// Provides access to the servo motor on the BrainPad.
        /// </summary>
        public static class ServoMotor
        {
            private static PWM output;
            private static bool started;

            static ServoMotor()
            {
                output = new PWM(Peripherals.ServoMotor, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
                started = false;
            }

            /// <summary>
            /// Sets the position of the Servo Motor.
            /// </summary>
            /// <param name="position">The position of the servo between 0 and 180 degrees.</param>
            public static void SetPosition(int position)
            {
                if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");

                Stop();

                output.Period = 20000;
                output.Duration = (uint)(1000.0 + ((position / 180.0) * 1000.0));

                Start();
            }

            /// <summary>
            /// Stops the servo motor.
            /// </summary>
            public static void Stop()
            {
                if (started)
                {
                    output.Stop();

                    started = false;
                }
            }

            /// <summary>
            /// Starts the servo motor.
            /// </summary>
            public static void Start()
            {
                if (!started)
                {
                    output.Start();

                    started = true;
                }
            }
        }

        /// <summary>
        /// Provides access to the light bulb on the BrainPad.
        /// </summary>
        public static class LightBulb
        {
            private static PWM red;
            private static PWM green;
            private static PWM blue;
            private static bool started;

            static LightBulb()
            {
                started = false;
                red = new PWM(Peripherals.LightBulb.Red, 10000, 1, false);
                green = new PWM(Peripherals.LightBulb.Green, 10000, 1, false);
                blue = new PWM(Peripherals.LightBulb.Blue, 10000, 1, false);
            }

            /// <summary>
            /// Sets the color of the light bulb.
            /// </summary>
            /// <param name="color">The color to set the light bulb to.</param>
            public static void SetColor(Color color)
            {
                SetColor(color.R / 255.0, color.G / 255.0, color.B / 255.0);
            }

            /// <summary>
            /// Sets the color of the light bulb.
            /// </summary>
            /// <param name="r">The red value of the color between 0 (fully off) and 1 (fully on).</param>
            /// <param name="g">The green value of the color between 0 (fully off) and 1 (fully on).</param>
            /// <param name="blue">The blue value of the color between 0 (fully off) and 1 (fully on).</param>
            public static void SetColor(double r, double g, double b)
            {
                if (r < 0 || r > 1) throw new ArgumentOutOfRangeException("red", "red must be between zero and one.");
                if (g < 0 || g > 1) throw new ArgumentOutOfRangeException("green", "green must be between zero and one.");
                if (b < 0 || b > 1) throw new ArgumentOutOfRangeException("blue", "blue must be between zero and one.");

                if (started)
                    TurnOff();

                red.DutyCycle = r;
                green.DutyCycle = g;
                blue.DutyCycle = b;

                TurnOn();
            }

            /// <summary>
            /// Turns on the light bulb.
            /// </summary>
            public static void TurnOn()
            {
                if (!started)
                {
                    red.Start();
                    green.Start();
                    blue.Start();

                    started = true;
                }
            }

            /// <summary>
            /// Turns off the light bulb.
            /// </summary>
            public static void TurnOff()
            {
                if (started)
                {
                    red.Stop();
                    green.Stop();
                    blue.Stop();

                    started = false;
                }
            }
        }

        /// <summary>
        /// Provides access to the buzzer on the BrainPad.
        /// </summary>
        public static class Buzzer
        {
            private static PWM output;
            private static OutputPort volumeControl;
            private static bool started;

            static Buzzer()
            {
                started = false;
                output = new PWM(Peripherals.Buzzer, 500, 0.5, false);
                volumeControl = new OutputPort(Peripherals.BuzzerVolumeController, false);
            }

            /// <summary>
            /// Notes the Buzzer can play.
            /// </summary>
            public enum Note
            {
                A = 880,
                ASharp = 932,
                B = 988,
                C = 1047,
                CSharp = 1109,
                D = 1175,
                DSharp = 1244,
                E = 1319,
                F = 1397,
                FSharp = 1480,
                G = 1568,
                GSharp = 1661,
            }

            /// <summary>
            /// Volumes the Buzzer can play at.
            /// </summary>
            public enum Volume
            {
                /// <summary>
                /// The default louder volume.
                /// </summary>
                Loud,
                /// <summary>
                /// A quieter volume.
                /// </summary>
                Quiet
            }

            /// <summary>
            /// Sets the volume.
            /// </summary>
            /// <param name="volume">The volume to play at.</param>
            public static void SetVolume(Volume volume)
            {
                volumeControl.Write(volume == Volume.Quiet);
            }

            /// <summary>
            /// Plays the given note.
            /// </summary>
            /// <param name="note">The note to play.</param>
            public static void PlayNote(Note note)
            {
                PlayFrequency((int)note);
            }

            /// <summary>
            /// Plays a given frequency.
            /// </summary>
            /// <param name="frequency">The frequency to play.</param>
            public static void PlayFrequency(int frequency)
            {
                Stop();

                if (frequency > 0)
                {
                    output.Frequency = frequency;
                    output.Start();

                    started = true;
                }
            }

            /// <summary>
            /// Stops any note or frequency currently playing.
            /// </summary>
            public static void Stop()
            {
                if (started)
                {
                    output.Stop();

                    started = false;
                }
            }
        }

        /// <summary>
        /// Provices access to the buttons on the BrainPad.
        /// </summary>
        public static class Button
        {
            private static InterruptPort[] ports;

            /// <summary>
            /// The avilable buttons.
            /// </summary>
            public enum DPad
            {
                Up = 0,
                Down,
                Left,
                Right
            }

            /// <summary>
            /// The button state.
            /// </summary>
            public enum State
            {
                Pressed,
                NotPressed
            }

            static Button()
            {
                ports = new InterruptPort[] {
                new InterruptPort(Peripherals.Button.Up, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth),
                new InterruptPort(Peripherals.Button.Down, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth),
                new InterruptPort(Peripherals.Button.Left, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth),
                new InterruptPort(Peripherals.Button.Right, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth)
            };

                foreach (var p in ports)
                    p.OnInterrupt += (a, b, c) => OnInterrupt(p, b != 0);
            }

            /// <summary>
            /// Is the down button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsDownPressed()
            {
                return IsPressed(DPad.Down);
            }

            /// <summary>
            /// Is the up button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsUpPressed()
            {
                return IsPressed(DPad.Up);
            }

            /// <summary>
            /// Is the left button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsLeftPressed()
            {
                return IsPressed(DPad.Left);
            }

            /// <summary>
            /// Is the right button pressed.
            /// </summary>
            /// <returns>Whether or not it is pressed.</returns>
            public static bool IsRightPressed()
            {
                return IsPressed(DPad.Right);
            }

            /// <summary>
            /// The signature of all button events.
            /// </summary>
            /// <param name="button">The button in question.</param>
            /// <param name="state">The new button state.</param>
            public delegate void ButtonEventHandler(DPad button, State state);

            /// <summary>
            /// The event raised when a button is pressed.
            /// </summary>
            public static event ButtonEventHandler ButtonPressed;

            /// <summary>
            /// The event raised when a button is released.
            /// </summary>
            public static event ButtonEventHandler ButtonReleased;

            /// <summary>
            /// The event raised when a button changes state.
            /// </summary>
            public static event ButtonEventHandler ButtonChanged;

            /// <summary>
            /// Checks if a button is pressed.
            /// </summary>
            /// <param name="button">The button to check.</param>
            /// <returns>Whether or not it was pressed.</returns>
            public static bool IsPressed(DPad button)
            {
                return !ports[(int)button].Read();
            }

            private static void OnInterrupt(InterruptPort port, bool state)
            {
                var button = (DPad)(-1);

                for (var i = 0; i < ports.Length; i++)
                    if (ports[i] == port)
                        button = (DPad)i;

                var e1 = ButtonPressed;
                var e2 = ButtonReleased;
                var e3 = ButtonChanged;

                if (e1 != null && !state)
                {
                    e1(button, State.Pressed);
                }
                else if (e2 != null && state)
                {
                    e2(button, State.NotPressed);
                }

                if (e3 != null)
                    e3(button, state ? State.NotPressed : State.Pressed);
            }
        }

        /// <summary>
        /// Provides access to the traffic light on the BrainPad.
        /// </summary>
        public static class TrafficLight
        {
            private static PWM red;
            private static PWM yellow;
            private static PWM green;

            static TrafficLight()
            {
                red = new PWM(Peripherals.TrafficLight.Red, 200, 1.0, false);
                yellow = new PWM(Peripherals.TrafficLight.Yellow, 200, 1.0, false);
                green = new PWM(Peripherals.TrafficLight.Green, 200, 1.0, false);
            }

            /// <summary>
            /// Turns the red light on.
            /// </summary>
            public static void TurnRedLightOn()
            {
                TurnColorOn(Color.Red);
            }

            /// <summary>
            /// Turns the red light off.
            /// </summary>
            public static void TurnRedLightOff()
            {
                TurnColorOff(Color.Red);
            }

            /// <summary>
            /// Turns the yellow light on.
            /// </summary>
            public static void TurnYellowLightOn()
            {
                TurnColorOn(Color.Yellow);
            }

            /// <summary>
            /// Turns the yellow light off.
            /// </summary>
            public static void TurnYellowLightOff()
            {
                TurnColorOff(Color.Yellow);
            }

            /// <summary>
            /// Turns the green light on.
            /// </summary>
            public static void TurnGreenLightOn()
            {
                TurnColorOn(Color.Green);
            }

            /// <summary>
            /// Turns the green light off.
            /// </summary>
            public static void TurnGreenLightOff()
            {
                TurnColorOff(Color.Green);
            }

            /// <summary>
            /// Turns off all three lights.
            /// </summary>
            public static void TurnOffAllLights()
            {
                TurnColorOff(Color.Green);
                TurnColorOff(Color.Red);
                TurnColorOff(Color.Yellow);
            }

            /// <summary>
            /// Turns on a color.
            /// </summary>
            /// <param name="color">The color to turn on.</param>
            public static void TurnColorOn(BrainPad.Color color)
            {
                SetColor(color, 1.0);
            }

            /// <summary>
            /// Turns off a color.
            /// </summary>
            /// <param name="color">The color to turn off.</param>
            public static void TurnColorOff(BrainPad.Color color)
            {
                SetColor(color, 0.0);
            }

            private static void SetColor(BrainPad.Color color, double level)
            {
                if (color.R != 0 && color.G == 0 && color.B == 0)
                {
                    red.Stop();
                    red.DutyCycle = level;
                    red.Start();
                }
                else if (color.R != 0 && color.G != 0 && color.B == 0)
                {
                    yellow.Stop();
                    yellow.DutyCycle = level;
                    yellow.Start();
                }
                else if (color.R == 0 && color.G != 0 && color.B == 0)
                {
                    green.Stop();
                    green.DutyCycle = level;
                    green.Start();
                }
                else
                {
                    throw new ArgumentException("This color isn't valid.", "color");
                }
            }
        }

        /// <summary>
        /// Provides access to the light sensor on the BrainPad.
        /// </summary>
        public static class LightSensor
        {
            private static AnalogInput input;

            static LightSensor()
            {
                input = new AnalogInput(Peripherals.LightSensor);
            }

            /// <summary>
            /// Reads the light level.
            /// </summary>
            /// <returns>The light level.</returns>
            public static double ReadLightLevel()
            {
                return input.Read();
            }
        }

        /// <summary>
        /// Provides access to the temperature sensor on the BrainPad.
        /// </summary>
        public static class TemperatureSensor
        {
            private static AnalogInput input;

            static TemperatureSensor()
            {
                input = new AnalogInput(Peripherals.TemperatureSensor);
            }

            /// <summary>
            /// Reads the temperature.
            /// </summary>
            /// <returns>The temperature in celsius.</returns>
            public static double ReadTemperature()
            {
                double sum = 0;

                for (var i = 0; i < 10; i++)
                    sum += input.Read();

                sum /= 10.0;

                return (sum * 3300.0 - 450.0) / 19.5;
            }
        }

        /// <summary>
        /// Provices access to the accelerometer on the BrainPad.
        /// </summary>
  /*      public static class Accelerometer
        {
            private static I2C device;
            private static byte[] buffer;

            static Accelerometer()
            {
                buffer = new byte[2];

                device = new I2C(0x1C);
                device.WriteRegister(0x2A, 0x01);
            }

            private static double ReadAxis(byte register)
            {
                device.ReadRegisters(register, buffer);

                var value = (double)(buffer[0] << 2 | buffer[1] >> 6);

                if (value > 511.0)
                    value -= 1024.0;

                return value / 256.0;
            }

            /// <summary>
            /// Reads the acceleration on the x axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadX()
            {
                return ReadAxis(0x01);
            }

            /// <summary>
            /// Reads the acceleration on the y axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadY()
            {
                return ReadAxis(0x03);
            }

            /// <summary>
            /// Reads the acceleration on the z axis.
            /// </summary>
            /// <returns>The acceleration.</returns>
            public static double ReadZ()
            {
                return ReadAxis(0x05);
            }
        }
*/
        /// <summary> 
        /// Controls the display on the BrainPad.
        /// </summary>
        public static class Display
        {
            private static SPI spi;
            private static OutputPort controlPin;
            private static OutputPort resetPin;
            private static OutputPort backlightPin;

            private static byte[] buffer1;
            private static byte[] buffer2;
            private static byte[] buffer4;
            private static byte[] clearBuffer;
            private static byte[] characterBuffer1;
            private static byte[] characterBuffer2;
            private static byte[] characterBuffer4;

            private const byte ST7735_MADCTL = 0x36;
            private const byte MADCTL_MY = 0x80;
            private const byte MADCTL_MX = 0x40;
            private const byte MADCTL_MV = 0x20;
            private const byte MADCTL_BGR = 0x08;

            /// <summary>
            /// The width of the display in pixels.
            /// </summary>
            public const int Width = 160;

            /// <summary>
            /// The height of the display in pixels.
            /// </summary>
            public const int Height = 128;

            static Display()
            {
                buffer1 = new byte[1];
                buffer2 = new byte[2];
                buffer4 = new byte[4];
                clearBuffer = new byte[160 * 2 * 16];
                characterBuffer1 = new byte[80];
                characterBuffer2 = new byte[320];
                characterBuffer4 = new byte[1280];

                controlPin = new OutputPort(Peripherals.Display.Control, false);
                resetPin = new OutputPort(Peripherals.Display.Reset, false);
                backlightPin = new OutputPort(Peripherals.Display.Backlight, true);
                spi = new SPI(new SPI.Configuration(Peripherals.Display.ChipSelect, false, 0, 0, false, true, 12000, Peripherals.Display.SpiModule));

                Reset();

                WriteCommand(0x11); //Sleep exit 
                Thread.Sleep(120);

                // ST7735R Frame Rate
                WriteCommand(0xB1);
                WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
                WriteCommand(0xB2);
                WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
                WriteCommand(0xB3);
                WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
                WriteData(0x01); WriteData(0x2C); WriteData(0x2D);

                WriteCommand(0xB4); // Column inversion 
                WriteData(0x07);

                // ST7735R Power Sequence
                WriteCommand(0xC0);
                WriteData(0xA2); WriteData(0x02); WriteData(0x84);
                WriteCommand(0xC1); WriteData(0xC5);
                WriteCommand(0xC2);
                WriteData(0x0A); WriteData(0x00);
                WriteCommand(0xC3);
                WriteData(0x8A); WriteData(0x2A);
                WriteCommand(0xC4);
                WriteData(0x8A); WriteData(0xEE);

                WriteCommand(0xC5); // VCOM 
                WriteData(0x0E);

                WriteCommand(0x36); // MX, MY, RGB mode
                WriteData(MADCTL_MX | MADCTL_MY | MADCTL_BGR);

                // ST7735R Gamma Sequence
                WriteCommand(0xe0);
                WriteData(0x0f); WriteData(0x1a);
                WriteData(0x0f); WriteData(0x18);
                WriteData(0x2f); WriteData(0x28);
                WriteData(0x20); WriteData(0x22);
                WriteData(0x1f); WriteData(0x1b);
                WriteData(0x23); WriteData(0x37); WriteData(0x00);

                WriteData(0x07);
                WriteData(0x02); WriteData(0x10);
                WriteCommand(0xe1);
                WriteData(0x0f); WriteData(0x1b);
                WriteData(0x0f); WriteData(0x17);
                WriteData(0x33); WriteData(0x2c);
                WriteData(0x29); WriteData(0x2e);
                WriteData(0x30); WriteData(0x30);
                WriteData(0x39); WriteData(0x3f);
                WriteData(0x00); WriteData(0x07);
                WriteData(0x03); WriteData(0x10);

                WriteCommand(0x2a);
                WriteData(0x00); WriteData(0x00);
                WriteData(0x00); WriteData(0x7f);
                WriteCommand(0x2b);
                WriteData(0x00); WriteData(0x00);
                WriteData(0x00); WriteData(0x9f);

                WriteCommand(0xF0); //Enable test command  
                WriteData(0x01);
                WriteCommand(0xF6); //Disable ram power save mode 
                WriteData(0x00);

                WriteCommand(0x3A); //65k mode 
                WriteData(0x05);

                // Rotate
                WriteCommand(ST7735_MADCTL);
                WriteData(MADCTL_MV | MADCTL_MY);

                WriteCommand(0x29); //Display on
                Thread.Sleep(50);

                Clear();
            }

            private static void WriteData(byte[] data)
            {
                controlPin.Write(true);
                spi.Write(data);
            }

            private static void WriteCommand(byte command)
            {
                buffer1[0] = command;
                controlPin.Write(false);
                spi.Write(buffer1);
            }

            private static void WriteData(byte data)
            {
                buffer1[0] = data;
                controlPin.Write(true);
                spi.Write(buffer1);
            }

            private static void Reset()
            {
                resetPin.Write(false);
                Thread.Sleep(300);
                resetPin.Write(true);
                Thread.Sleep(1000);
            }

            private static void SetClip(int x, int y, int width, int height)
            {
                WriteCommand(0x2A);

                controlPin.Write(true);
                buffer4[1] = (byte)x;
                buffer4[3] = (byte)(x + width - 1);
                spi.Write(buffer4);

                WriteCommand(0x2B);
                controlPin.Write(true);
                buffer4[1] = (byte)y;
                buffer4[3] = (byte)(y + height - 1);
                spi.Write(buffer4);
            }

            /// <summary>
            /// Clears the Display.
            /// </summary>
            public static void Clear()
            {
                SetClip(0, 0, 160, 128);
                WriteCommand(0x2C);

                for (var i = 0; i < 128 / 16; i++)
                    WriteData(clearBuffer);
            }

            /// <summary>
            /// Draws an image.
            /// </summary>
            /// <param name="data">The image as a byte array.</param>
            public static void DrawImage(byte[] data)
            {
                if (data == null) throw new ArgumentNullException("data");
                if (data.Length == 0) throw new ArgumentException("data.Length must not be zero.", "data");

                WriteCommand(0x2C);
                WriteData(data);
            }

            /// <summary>
            /// Draws an image at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="image">The image to draw.</param>
            public static void DrawImage(int x, int y, Image image)
            {
                if (image == null) throw new ArgumentNullException("image");
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                SetClip(x, y, image.Width, image.Height);
                DrawImage(image.Pixels);
            }

            /// <summary>
            /// Draws a filled rectangle.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="width">The width of the rectangle.</param>
            /// <param name="height">The height of the rectangle.</param>
            /// <param name="color">The color to draw.</param>
            public static void DrawFilledRectangle(int x, int y, int width, int height, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
                if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

                SetClip(x, y, width, height);

                var data = new byte[width * height * 2];
                for (var i = 0; i < data.Length; i += 2)
                {
                    data[i] = (byte)((color.As565 >> 8) & 0xFF);
                    data[i + 1] = (byte)((color.As565 >> 0) & 0xFF);
                }

                DrawImage(data);
            }

            /// <summary>
            /// Turns the backlight on.
            /// </summary>
            public static void TurnOn()
            {
                backlightPin.Write(true);
            }

            /// <summary>
            /// Turns the backlight off.
            /// </summary>
            public static void TurnOff()
            {
                backlightPin.Write(false);
            }

            /// <summary>
            /// Draws a pixel.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="color">The color to draw.</param>
            public static void SetPixel(int x, int y, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                SetClip(x, y, 1, 1);

                buffer2[0] = (byte)(color.As565 >> 8);
                buffer2[1] = (byte)(color.As565 >> 0);

                DrawImage(buffer2);
            }

            /// <summary>
            /// Draws a line.
            /// </summary>
            /// <param name="x">The x coordinate to start drawing at.</param>
            /// <param name="y">The y coordinate to start drawing at.</param>
            /// <param name="x1">The ending x coordinate.</param>
            /// <param name="y1">The ending y coordinate.</param>
            /// <param name="color">The color to draw.</param>
            public static void DrawLine(int x0, int y0, int x1, int y1, Color color)
            {
                if (x0 < 0) throw new ArgumentOutOfRangeException("x0", "x0 must not be negative.");
                if (y0 < 0) throw new ArgumentOutOfRangeException("y0", "y0 must not be negative.");
                if (x1 < 0) throw new ArgumentOutOfRangeException("x1", "x1 must not be negative.");
                if (y1 < 0) throw new ArgumentOutOfRangeException("y1", "y1 must not be negative.");

                var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
                int t, dX, dY, yStep, error;

                if (steep)
                {
                    t = x0;
                    x0 = y0;
                    y0 = t;
                    t = x1;
                    x1 = y1;
                    y1 = t;
                }

                if (x0 > x1)
                {
                    t = x0;
                    x0 = x1;
                    x1 = t;

                    t = y0;
                    y0 = y1;
                    y1 = t;
                }

                dX = x1 - x0;
                dY = System.Math.Abs(y1 - y0);

                error = (dX / 2);

                if (y0 < y1)
                {
                    yStep = 1;
                }
                else
                {
                    yStep = -1;
                }

                for (; x0 < x1; x0++)
                {
                    if (steep)
                    {
                        SetPixel(y0, x0, color);
                    }
                    else
                    {
                        SetPixel(x0, y0, color);
                    }

                    error -= dY;

                    if (error < 0)
                    {
                        y0 += (byte)yStep;
                        error += dX;
                    }
                }
            }

            /// <summary>
            /// Draws a circle.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="r">The radius of the circle.</param>
            /// <param name="color">The color to draw.</param>
            public static void DrawCircle(int x, int y, int r, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (r <= 0) throw new ArgumentOutOfRangeException("radius", "radius must be positive.");

                int f = 1 - r;
                int ddFX = 1;
                int ddFY = -2 * r;
                int dX = 0;
                int dY = r;

                SetPixel(x, y + r, color);
                SetPixel(x, y - r, color);
                SetPixel(x + r, y, color);
                SetPixel(x - r, y, color);

                while (dX < dY)
                {
                    if (f >= 0)
                    {
                        dY--;
                        ddFY += 2;
                        f += ddFY;
                    }

                    dX++;
                    ddFX += 2;
                    f += ddFX;

                    SetPixel(x + dX, y + dY, color);
                    SetPixel(x - dX, y + dY, color);
                    SetPixel(x + dX, y - dY, color);
                    SetPixel(x - dX, y - dY, color);

                    SetPixel(x + dY, y + dX, color);
                    SetPixel(x - dY, y + dX, color);
                    SetPixel(x + dY, y - dX, color);
                    SetPixel(x - dY, y - dX, color);
                }
            }

            /// <summary>
            /// Draws a rectangle.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="width">The width of the rectangle.</param>
            /// <param name="height">The height of the rectangle.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawRectangle(int x, int y, int width, int height, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (width < 0) throw new ArgumentOutOfRangeException("width", "width must not be negative.");
                if (height < 0) throw new ArgumentOutOfRangeException("height", "height must not be negative.");

                for (var i = x; i < x + width; i++)
                {
                    SetPixel(i, y, color);
                    SetPixel(i, y + height - 1, color);
                }

                for (var i = y; i < y + height; i++)
                {
                    SetPixel(x, i, color);
                    SetPixel(x + width - 1, i, color);
                }
            }

            static byte[] font = new byte[95 * 5] {
            0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
            0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
            0x00, 0x07, 0x00, 0x07, 0x00, /* " */
            0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
            0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
            0x23, 0x13, 0x08, 0x64, 0x62, /* % */
            0x36, 0x49, 0x55, 0x22, 0x20, /* & */
            0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
            0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
            0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
            0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
            0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
            0x50, 0x30, 0x00, 0x00, 0x00, /* , */
            0x08, 0x08, 0x08, 0x08, 0x08, /* - */
            0x00, 0x60, 0x60, 0x00, 0x00, /* . */
            0x20, 0x10, 0x08, 0x04, 0x02, /* / */
            0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
            0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
            0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
            0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
            0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
            0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
            0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
            0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
            0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
            0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
            0x00, 0x36, 0x36, 0x00, 0x00, /* : */
            0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
            0x08, 0x14, 0x22, 0x41, 0x00, /* < */
            0x14, 0x14, 0x14, 0x14, 0x14, /* = */
            0x00, 0x41, 0x22, 0x14, 0x08, /* > */
            0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
            0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
            0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
            0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
            0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
            0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
            0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
            0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
            0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
            0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
            0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
            0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
            0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
            0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
            0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
            0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
            0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
            0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
            0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
            0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
            0x26, 0x49, 0x49, 0x49, 0x32, /* S */
            0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
            0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
            0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
            0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
            0x63, 0x14, 0x08, 0x14, 0x63, /* X */
            0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
            0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
            0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
            0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
            0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
            0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
            0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
            0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
            0x20, 0x54, 0x54, 0x54, 0x78, /* a */
            0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
            0x38, 0x44, 0x44, 0x44, 0x44, /* c */
            0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
            0x38, 0x54, 0x54, 0x54, 0x18, /* e */
            0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
            0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
            0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
            0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
            0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
            0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
            0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
            0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
            0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
            0x38, 0x44, 0x44, 0x44, 0x38, /* o */
            0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
            0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
            0x7c, 0x08, 0x04, 0x04, 0x00, /* r */
            0x48, 0x54, 0x54, 0x54, 0x24, /* s */
            0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
            0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
            0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
            0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
            0x44, 0x28, 0x10, 0x28, 0x44, /* x */
            0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
            0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
            0x08, 0x36, 0x41, 0x41, 0x00, /* { */
            0x00, 0x00, 0x77, 0x00, 0x00, /* | */
            0x00, 0x41, 0x41, 0x36, 0x08, /* } */
            0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

            private static void DrawLetter(int x, int y, char letter, Color color, int scaleFactor)
            {
                var index = 5 * (letter - 32);
                var upper = (byte)(color.As565 >> 8);
                var lower = (byte)(color.As565 >> 0);
                var characterBuffer = scaleFactor == 1 ? characterBuffer1 : (scaleFactor == 2 ? characterBuffer2 : characterBuffer4);

                var i = 0;

                for (var j = 1; j <= 64; j *= 2)
                {
                    for (var k = 0; k < scaleFactor; k++)
                    {
                        for (var l = 0; l < 5; l++)
                        {
                            for (var m = 0; m < scaleFactor; m++)
                            {
                                var show = (font[index + l] & j) != 0;

                                characterBuffer[i++] = show ? upper : (byte)0x00;
                                characterBuffer[i++] = show ? lower : (byte)0x00;
                            }
                        }
                    }
                }

                SetClip(x, y, 5 * scaleFactor, 8 * scaleFactor);
                DrawImage(characterBuffer);
            }

            /// <summary>
            /// Draws a letter at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="letter">The letter to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawLetter(int x, int y, char letter, Color color)
            {
                if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawLetter(x, y, letter, color, 1);
            }

            /// <summary>
            /// Draws a large letter at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="letter">The letter to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawLargeLetter(int x, int y, char letter, Color color)
            {
                if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawLetter(x, y, letter, color, 2);
            }

            /// <summary>
            /// Draws an extra large letter at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="letter">The letter to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawExtraLargeLetter(int x, int y, char letter, Color color)
            {
                if (letter > 126 || letter < 32) throw new ArgumentOutOfRangeException("letter", "This letter cannot be drawn.");
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawLetter(x, y, letter, color, 4);
            }

            /// <summary>
            /// Draws text at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="text">The string to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawText(int x, int y, string text, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (text == null) throw new ArgumentNullException("data");

                for (var i = 0; i < text.Length; i++)
                    DrawLetter(x + i * 6, y, text[i], color, 1);
            }

            /// <summary>
            /// Draws large text at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="text">The string to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawLargeText(int x, int y, string text, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (text == null) throw new ArgumentNullException("data");

                for (var i = 0; i < text.Length; i++)
                    DrawLetter(x + i * 6 * 2, y, text[i], color, 2);
            }

            /// <summary>
            /// Draws extra large text at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="text">The string to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawExtraLargeText(int x, int y, string text, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");
                if (text == null) throw new ArgumentNullException("data");

                for (var i = 0; i < text.Length; i++)
                    DrawLetter(x + i * 6 * 4, y, text[i], color, 4);
            }

            /// <summary>
            /// Draws a number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawNumber(int x, int y, double number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawText(x, y, number.ToString("N2"), color);
            }

            /// <summary>
            /// Draws a large number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawLargeNumber(int x, int y, double number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawLargeText(x, y, number.ToString("N2"), color);
            }

            /// <summary>
            /// Draws an extra large number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawExtraLargeNumber(int x, int y, double number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawExtraLargeText(x, y, number.ToString("N2"), color);
            }

            /// <summary>
            /// Draws a number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawNumber(int x, int y, long number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawText(x, y, number.ToString("N0"), color);
            }

            /// <summary>
            /// Draws a large number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawLargeNumber(int x, int y, long number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawLargeText(x, y, number.ToString("N0"), color);
            }

            /// <summary>
            /// Draws an extra large number at the given location.
            /// </summary>
            /// <param name="x">The x coordinate to draw at.</param>
            /// <param name="y">The y coordinate to draw at.</param>
            /// <param name="number">The number to draw.</param>
            /// <param name="color">The color to use.</param>
            public static void DrawExtraLargeNumber(int x, int y, long number, Color color)
            {
                if (x < 0) throw new ArgumentOutOfRangeException("x", "x must not be negative.");
                if (y < 0) throw new ArgumentOutOfRangeException("y", "y must not be negative.");

                DrawExtraLargeText(x, y, number.ToString("N0"), color);
            }
        }

        /// <summary>
        /// Tells the BrainPad to wait.
        /// </summary>
        public static class Wait
        {
            /// <summary>
            /// Tells the BrainPad to wait for the given number of seconds.
            /// </summary>
            /// <param name="seconds">The number of seconds to wait.</param>
            public static void Seconds(double seconds)
            {
                if (seconds < 0.0) throw new ArgumentOutOfRangeException("seconds", "seconds must not be negative.");

                Thread.Sleep((int)(seconds * 1000));
            }

            /// <summary>
            /// Tells the BrainPad to wait for the given number of milliseconds.
            /// </summary>
            /// <param name="milliseconds">The number of milliseconds to wait.</param>
            public static void Milliseconds(double milliseconds)
            {
                if (milliseconds < 0.0 && milliseconds != -1.0) throw new ArgumentOutOfRangeException("milliseconds", "milliseconds must not be negative.");

                Thread.Sleep((int)milliseconds);
            }
        }

        /// <summary>
        /// Board definition for the BrainPad expansion.
        /// </summary>
        public static class Expansion
        {
            /// <summary>GPIO definitions.</summary>
            public static class Gpio
            {
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA7 = G30.Gpio.PA7;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA6 = G30.Gpio.PA6;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PC3 = G30.Gpio.PC3;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PB3 = G30.Gpio.PB3;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PB4 = G30.Gpio.PB4;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PB5 = G30.Gpio.PB5;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA3 = G30.Gpio.PA3;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA2 = G30.Gpio.PA2;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA10 = G30.Gpio.PA10;
                /// <summary>GPIO pin.</summary>
                public const Cpu.Pin PA9 = G30.Gpio.PA9;
            }

            /// <summary>Analog input definitions.</summary>
            public static class AnalogInput
            {
                /// <summary>Analog channel.</summary>
                public const Cpu.AnalogChannel PA7 = G30.AnalogInput.PA7;
                /// <summary>Analog channel.</summary>
                public const Cpu.AnalogChannel PA6 = G30.AnalogInput.PA6;
                /// <summary>Analog channel.</summary>
                public const Cpu.AnalogChannel PC3 = G30.AnalogInput.PC3;
                /// <summary>Analog channel.</summary>
                public const Cpu.AnalogChannel PA3 = G30.AnalogInput.PA3;
                /// <summary>Analog channel.</summary>
                public const Cpu.AnalogChannel PA2 = G30.AnalogInput.PA2;
            }

            /// <summary>PWM output definitions.</summary>
            public static class PwmOutput
            {
                /// <summary>PWM channel.</summary>
                public const Cpu.PWMChannel PA3 = G30.PwmOutput.PA3;
                /// <summary>PWM channel.</summary>
                public const Cpu.PWMChannel PA2 = G30.PwmOutput.PA2;
                /// <summary>PWM channel.</summary>
                public const Cpu.PWMChannel PA10 = G30.PwmOutput.PA10;
                /// <summary>PWM channel.</summary>
                public const Cpu.PWMChannel PA9 = G30.PwmOutput.PA9;
            }
        }

        /// <summary>
        /// Board definition for the BrainPad peripherals.
        /// </summary>
        public static class Peripherals
        {
            /// <summary>
            /// The onboard display.
            /// </summary>
            public static class Display
            {
                /// <summary>The SPI chip select.</summary>
                public const Cpu.Pin ChipSelect = G30.Gpio.PB12;
                /// <summary>The control pin.</summary>
                public const Cpu.Pin Control = G30.Gpio.PC5;
                /// <summary>The reset pin.</summary>
                public const Cpu.Pin Reset = G30.Gpio.PC4;
                /// <summary>The backlight pin.</summary>
                public const Cpu.Pin Backlight = G30.Gpio.PA4;
                /// <summary>The spi module.</summary>
                public const SPI.SPI_module SpiModule = G30.SpiBus.Spi2;
            }

            /// <summary>
            /// The onboard traffic light LEDs.
            /// </summary>
            public static class TrafficLight
            {
                /// <summary>
                /// The red LED.
                /// </summary>
                public const Cpu.PWMChannel Red = G30.PwmOutput.PA1;
                /// <summary>
                /// The yellow LED.
                /// </summary>
                public const Cpu.PWMChannel Yellow = G30.PwmOutput.PC6;
                /// <summary>
                /// The green LED.
                /// </summary>
                public const Cpu.PWMChannel Green = G30.PwmOutput.PB9;
            }

            /// <summary>
            /// The onboard buttons.
            /// </summary>
            public static class Button
            {
                /// <summary>
                /// The up button.
                /// </summary>
                public const Cpu.Pin Up = G30.Gpio.PA15;
                /// <summary>
                /// The down button.
                /// </summary>
                public const Cpu.Pin Down = G30.Gpio.PC13;
                /// <summary>
                /// The left button.
                /// </summary>
                public const Cpu.Pin Left = G30.Gpio.PB10;
                /// <summary>
                /// The right button.
                /// </summary>
                public const Cpu.Pin Right = G30.Gpio.PA5;
            }

            /// <summary>
            /// The onboard touch pads.
            /// </summary>
            public static class TouchPad
            {
                /// <summary>
                /// The left pad.
                /// </summary>
                public const Cpu.Pin Left = G30.Gpio.PC0;
                /// <summary>
                /// The middle pad.
                /// </summary>
                public const Cpu.Pin Middle = G30.Gpio.PC1;
                /// <summary>
                /// The right pad.
                /// </summary>
                public const Cpu.Pin Right = G30.Gpio.PC2;
            }

            /// <summary>
            /// The onboard light bulb LEDs.
            /// </summary>
            public static class LightBulb
            {
                /// <summary>
                /// The red LED.
                /// </summary>
                public const Cpu.PWMChannel Red = G30.PwmOutput.PC9;
                /// <summary>
                /// The green LED.
                /// </summary>
                public const Cpu.PWMChannel Green = G30.PwmOutput.PC8;
                /// <summary>
                /// The blue LED.
                /// </summary>
                public const Cpu.PWMChannel Blue = G30.PwmOutput.PC7;
            }

            /// <summary>
            /// The temperature sensor analog input.
            /// </summary>
            public const Cpu.AnalogChannel TemperatureSensor = G30.AnalogInput.PB0;

            /// <summary>
            /// The light sensor analog input.
            /// </summary>
            public const Cpu.AnalogChannel LightSensor = G30.AnalogInput.PB1;

            /// <summary>
            /// The buzzer pwm output.
            /// </summary>
            public const Cpu.PWMChannel Buzzer = G30.PwmOutput.PB8;

            /// <summary>
            /// The buzzer volume control output.
            /// </summary>
            public const Cpu.Pin BuzzerVolumeController = G30.Gpio.PC12;

            /// <summary>
            /// The DC motor pwm output.
            /// </summary>
            public const Cpu.PWMChannel DcMotor = G30.PwmOutput.PA0;

            /// <summary>
            /// The servo motor pwm output.
            /// </summary>
            public const Cpu.PWMChannel ServoMotor = G30.PwmOutput.PA8;
        }
/*
        private class I2C
        {
            private I2CDevice device;
            private byte[] buffer2;
            private byte[] buffer1;

            public I2C(byte address)
            {
                if (address > 0x7F) throw new ArgumentOutOfRangeException("address");

                this.device = new I2CDevice(new I2CDevice.Configuration(address, 400));
                this.buffer1 = new byte[1];
                this.buffer2 = new byte[2];
            }

            public void WriteRegister(byte register, byte value)
            {
                this.buffer2[0] = register;
                this.buffer2[1] = value;

                this.device.Execute(new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(this.buffer2) }, 1000);
            }

            public byte ReadRegister(byte register)
            {
                this.buffer1[0] = register;

                this.device.Execute(new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(this.buffer1), I2CDevice.CreateReadTransaction(this.buffer1) }, 1000);

                return this.buffer1[0];
            }

            public byte[] ReadRegisters(byte register, int count)
            {
                if (count < 0) throw new ArgumentOutOfRangeException("count");

                var buffer = new byte[count];

                this.ReadRegisters(register, buffer);

                return buffer;
            }

            public void ReadRegisters(byte register, byte[] buffer)
            {
                this.buffer1[0] = register;

                this.device.Execute(new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(this.buffer1), I2CDevice.CreateReadTransaction(buffer) }, 1000);
            }
        }
*/
        /// <summary>
        /// Legacy drivers for older hardware.
        /// </summary>
        public static class Legacy
        {
            /// <summary>
            /// Provides access to the touch pads on the BrainPad. The contents of this class are commented out. To use this class, uncomment the below code and add a reference to GHI.Hardware.
            /// </summary>
            public static class TouchPad
            {
                //private static PulseFeedback[] pins;
                //private static long[] thresholds;
                //
                //static TouchPad() {
                //	thresholds = new long[] { 130, 130, 130 };
                //	pins = new PulseFeedback[] {
                //		new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, Peripherals.TouchPad.Left),
                //		new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, Peripherals.TouchPad.Middle),
                //		new PulseFeedback(PulseFeedback.Mode.DrainDuration, true, 10, Peripherals.TouchPad.Right)
                //	};
                //}
                //
                ///// <summary>
                ///// The available touch pads.
                ///// </summary>
                //public enum Pad {
                //	/// <summary>
                //	/// The left pad.
                //	/// </summary>
                //	Left,
                //	/// <summary>
                //	/// The middle pad.
                //	/// </summary>
                //	Middle,
                //	/// <summary>
                //	/// The right pad.
                //	/// </summary>
                //	Right
                //}
                //
                ///// <summary>
                ///// Determines whether or not the left pad is touched.
                ///// </summary>
                ///// <returns>Whether or not the pad is touched.</returns>
                //public static bool IsLeftTouched() {
                //	return IsTouched(Pad.Left);
                //}
                //
                ///// <summary>
                ///// Determines whether or not the middle pad is touched.
                ///// </summary>
                ///// <returns>Whether or not the pad is touched.</returns>
                //public static bool IsMiddleTouched() {
                //	return IsTouched(Pad.Middle);
                //}
                //
                ///// <summary>
                ///// Determines whether or not the right pad is touched.
                ///// </summary>
                ///// <returns>Whether or not the pad is touched.</returns>
                //public static bool IsRightTouched() {
                //	return IsTouched(Pad.Right);
                //}
                //
                ///// <summary>
                ///// Determines whether or not the given pad is touched.
                ///// </summary>
                ///// <param name="pad">The pad to check.</param>
                ///// <returns>Whether or not the pad is touched.</returns>
                //public static bool IsTouched(Pad pad) {
                //	return pins[(int)pad].Read() > thresholds[(int)pad];
                //}
                //
                ///// <summary>
                ///// Sets the threshold beyond which a touch should be detected.
                ///// </summary>
                ///// <param name="pad">The pad to set the threshold for.</param>
                ///// <param name="threshold">The threshold value to set.</param>
                //public static void SetThreshold(Pad pad, long threshold) {
                //	if (threshold <= 0) throw new ArgumentOutOfRangeException("threshold", "threshold must be positive.");
                //
                //	thresholds[(int)pad] = threshold;
                //}
            }

            /// <summary>
            /// Board definition for the BrainPad expansion.
            /// </summary>
            public static class Expansion
            {
                /// <summary>GPIO definitions.</summary>
                public static class Gpio
                {
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E1 = G30.Gpio.PA7;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E2 = G30.Gpio.PA6;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E3 = G30.Gpio.PC3;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E4 = G30.Gpio.PB3;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E5 = G30.Gpio.PB4;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E6 = G30.Gpio.PB5;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E9 = G30.Gpio.PA3;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E10 = G30.Gpio.PA2;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E11 = G30.Gpio.PA10;
                    /// <summary>GPIO pin.</summary>
                    public const Cpu.Pin E12 = G30.Gpio.PA9;
                }

                /// <summary>Analog input definitions.</summary>
                public static class AnalogInput
                {
                    /// <summary>Analog channel.</summary>
                    public const Cpu.AnalogChannel E1 = G30.AnalogInput.PA7;
                    /// <summary>Analog channel.</summary>
                    public const Cpu.AnalogChannel E2 = G30.AnalogInput.PA6;
                    /// <summary>Analog channel.</summary>
                    public const Cpu.AnalogChannel E3 = G30.AnalogInput.PC3;
                    /// <summary>Analog channel.</summary>
                    public const Cpu.AnalogChannel E9 = G30.AnalogInput.PA3;
                    /// <summary>Analog channel.</summary>
                    public const Cpu.AnalogChannel E10 = G30.AnalogInput.PA2;
                }

                /// <summary>PWM output definitions.</summary>
                public static class PwmOutput
                {
                    /// <summary>PWM channel.</summary>
                    public const Cpu.PWMChannel E9 = G30.PwmOutput.PA3;
                    /// <summary>PWM channel.</summary>
                    public const Cpu.PWMChannel E10 = G30.PwmOutput.PA2;
                    /// <summary>PWM channel.</summary>
                    public const Cpu.PWMChannel E11 = G30.PwmOutput.PA10;
                    /// <summary>PWM channel.</summary>
                    public const Cpu.PWMChannel E12 = G30.PwmOutput.PA9;
                }
            }
        }
    }
}