using System;
using System.Threading;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace testMicroToolsKit
{
    namespace Hardware
    {
        namespace IO
        {
            /// <summary>
            /// A Class to manage the MicroChip MCP342x, 18-Bit Multi-Channel delta-sigma ADC series with
            /// I²C Interface and On-Board Reference
            /// </summary>
            /// <remarks>
            /// Can be used with MCP3422/3/4.
            /// </remarks>
            public class MCP342x
            {
                const UInt16 TIME_TO_LEAVE_STANDBY_MODE = 200;

                /// <summary>
                /// Resolution Selection affects convertion time (more bits, slower conversion)
                /// </summary>
                public enum SampleRate : byte
                {
                    /// <summary>
                    /// 240 SPS (By default on power-up)
                    /// </summary>
                    TwelveBits,
                    /// <summary>
                    /// 60 SPS
                    /// </summary>
                    FourteenBits,
                    /// <summary>
                    /// 15 SPS
                    /// </summary>
                    SixteenBits,
                    /// <summary>
                    /// 3.75 SPS
                    /// </summary>
                    EighteenBits
                }
                /// <summary>
                /// Use One-Shot if very low power consuption is required
                /// </summary>
                public enum ConversionMode : byte
                {
                    /// <summary>
                    /// The device performs a single conversion and enters a low current standby mode automatically until it
                    /// receives another conversion command. This reduces current consumption greatly during idle periods.
                    /// </summary>
                    OneShot,
                    /// <summary>
                    /// The conversion takes place continuously at the set conversion speed. See Resolution. (By default on power-up)
                    /// </summary>
                    Continuous
                }
                /// <summary>
                /// MCP3422 and MCP3423 devices have two differential input channels and the MCP3424 has four differential input channels
                /// </summary>
                public enum Channel : byte
                {
                    /// <summary>
                    /// MCP3422/3/4 (By default on power-up)
                    /// </summary>
                    Ch1,
                    /// <summary>
                    /// MCP3422/3/4
                    /// </summary>
                    Ch2,
                    /// <summary>
                    /// MCP3424
                    /// </summary>
                    Ch3,
                    /// <summary>
                    /// MCP3424
                    /// </summary>
                    Ch4
                }
                /// <summary>
                /// Selects the gain factor for the input signal before the analog-to-digital conversion takes place
                /// </summary>
                public enum PGA_Gain : byte
                {
                    /// <summary>
                    /// By default on power-up Gain = x1
                    /// </summary>
                    x1,
                    x2,
                    x4,
                    x8
                }
                /// <summary>
                ///  MCP342x configuration : SLave Address and Frequency
                /// </summary>
                I2CDevice.Configuration _config;

                I2CDevice _i2cBus = null;

                /// <summary>
                /// 7-bit Slave Adress
                /// </summary>
                UInt16 _sla;

                UInt16 _transactionTimeOut = 1000;

                // By default on power-up
                PGA_Gain _gain = PGA_Gain.x1;
                Channel _channel = Channel.Ch1;
                ConversionMode _conversionMode = ConversionMode.Continuous; // default
                SampleRate _resolution = SampleRate.TwelveBits;

                // Used to calcul voltage 
                bool _configRegisterOk = true;
                bool _endOfConversion = false;
                bool _sampleError = false;
                double[] _lsbValues = new double[] { 0.001, 0.00025, 0.0000625, 0.000015625 };
                int[] conversionTimeMilliSecs = new int[] { 5, 20, 70, 270 };
                double _lsbVolts = 0.001; // resolution = 12-bit 
                double _gainDivisor = 1.0;
                Int32 _maxValue;

                // Used to read raw data
                Int32 _dataMask = 0x0fff; // resolution = 12-bit 
                Timer timer = null;

                /// <summary>
                /// Basic Constructor - By default on power-up (Channel = CH1, Resolution = 12-bit, Conversion = Continuous, Gain = 1)
                /// </summary>
                /// <param name="SLA">7 bits Slave Address 0x68, 0x6A, 0x6C or 0x6E (0x68 by default)</param>
                /// <param name="Frequency">Standard : 100kHz to fast 400kHz or high-speed (3.4MHz) (100kHz by default)</param>
                public MCP342x(UInt16 SLA = 0x68, Int16 Frequency = 100)
                {
                    _sla = SLA;
                    _config = new I2CDevice.Configuration(SLA, Frequency);
                }

                /// <summary>
                /// Write value to configuration register
                /// </summary>
                /// <param name="value">RDY C1 C0 /O_C S1 S0 G1 G0 = 00010000 (default)</param>
                public void ConfigDevice(byte value)
                {
                    writeConfRegister(value);                 
                }

                /// <summary>
                /// Get or Set Sample Rate Selection Bit
                /// </summary>
                /// <remarks>
                /// Get :  0 => 240 SPS (12 bits) (Default), 1 => 60 SPS (14 bits), 2 => 15 SPS (16 bits), 3 => 3.75 SPS (18 bits)
                /// </remarks>
                public SampleRate Resolution
                {
                    get
                    {
                        return _resolution; 
                    }

                    set
                    {
                        _resolution = value;
                        _dataMask = (1 << (12 + (int)_resolution * 2)) - 1;
                        _maxValue = 1 << (11 + (int)_resolution * 2);
                        _lsbVolts = _lsbValues[(ushort)_resolution];
                        _configRegisterOk = false;
                    }                
                }

                /// <summary>
                /// Get or Set PGA Gain Selection Bits
                /// </summary>
                /// <remarks>
                /// Get :  0 => x1 (Default), 1 => x2, 2 => x4, 3 => x8
                /// </remarks>
                public PGA_Gain Gain
                {
                    get
                    {
                        return _gain;
                    }
                    set
                    {
                        _gain = value;
                        _gainDivisor = System.Math.Pow(2.0, (double)_gain);
                        _configRegisterOk = false;
                    }
                }

                /// <summary>
                /// Get or Set Channel Selection Bits
                /// </summary>
                /// <remarks>
                /// MCP3422 and MCP3423 : CH1 or CH2 - MCP3424 : CH1, CH2, CH3 or CH4
                /// </remarks>
                public Channel CHannel
                {
                    get
                    {
                        return _channel;
                    }
                    set
                    {
                        _channel = value;
                        _configRegisterOk = false;
                    }
                }
                
                /// <summary>
                /// Get or Set Conversion Mode
                /// </summary>
                /// <remarks>
                /// One-Shot Conversion mode or Continuous Conversion mode
                /// </remarks>
                public ConversionMode Mode
                {
                    get
                    {
                        return _conversionMode;
                    }
                    set
                    {
                        _conversionMode = value;
                        _configRegisterOk = false;
                    }
                }
                /// <summary>
                /// Time before System IO Exception if transaction failed
                /// </summary>
                /// <remarks>
                /// 1000ms by default
                /// </remarks>
                public UInt16 TransactionTimeOut
                {
                    get
                    {
                        return _transactionTimeOut;
                    }
                    set
                    {
                        _transactionTimeOut = value;
                    }
                }

                /// <summary>
                /// Reads the selected input channel and converts to voltage units
                /// </summary>
                /// <returns>Voltage representated as a double-precision real</returns>
                public double ReadVolts()
                {
                    double dataRaw = 0;
                    byte value = 0x10;

                    if (!_configRegisterOk)
                    {
                        value = (byte)((ushort)_gain +
                                           ((ushort)_resolution << 2) +
                                           ((ushort)_conversionMode << 4) +
                                           ((ushort)_channel << 5));
                        ConfigDevice(value);
                        _configRegisterOk = true;
                    }

                    if (_conversionMode == ConversionMode.OneShot)
                    {
                        value |= 0x80; // Start a single conversion
                        ConfigDevice(value);
                        timer = new Timer(new TimerCallback(noSample), null, TIME_TO_LEAVE_STANDBY_MODE, -1);
                    }
                    else
                    {
                        timer = new Timer(new TimerCallback(noSample), null, conversionTimeMilliSecs[(ushort)_resolution], -1);
                    }                    

                    if (_resolution == SampleRate.EighteenBits)
                    {
                        do
                        {
                            dataRaw = dataReadRaw(4); // Read three data bytes plus the data register
                        }
                        while (!_endOfConversion && !_sampleError);
                    }
                    else
                    {
                        do
                        {
                            dataRaw = dataReadRaw(3); // Read two data bytes plus the data register 
                        } while (!_endOfConversion && !_sampleError);  
                    }

                    if (_sampleError)
                    {
                        timer.Dispose(); _sampleError = false;
                        throw new System.IO.IOException("No sample on " + _sla);
                    }
                    timer.Dispose();  _endOfConversion = false;

                    if (dataRaw > _maxValue)
                    {
                        dataRaw -= _maxValue * 2;
                    }
                    return (dataRaw * _lsbVolts) / _gainDivisor;
                }
               void noSample(object state)
                {
                    _sampleError = true;                 
                }

                /// <summary>
                /// Reads the raw counts from the ADC
                /// </summary>
                /// <param name="number">Number of bytes to read (four if 18-bit conversion else three) </param>
                /// <returns>Result of 12, 14, 16 or 18-bit conversion</returns>
                double dataReadRaw(byte number)
                {
                    Int32 data = 0;
                    byte[] inbuffer = new byte[number];
                    I2CDevice.I2CReadTransaction readTransaction = I2CDevice.CreateReadTransaction(inbuffer);
                    I2CDevice.I2CTransaction[] xAction = new I2CDevice.I2CTransaction[] { readTransaction };

                    _i2cBus = new I2CDevice(_config);
                    int transferred = _i2cBus.Execute(xAction, _transactionTimeOut);
                    _i2cBus.Dispose();

                    if (transferred < inbuffer.Length)
                    {
                        throw new System.IO.IOException("Error i2cBus " + _sla);
                    }
                    else if ((inbuffer[number - 1] & 0x80) == 0) // End of conversion
                    {
                        if (_resolution == SampleRate.EighteenBits)
                        {
                            data = (((Int32)inbuffer[0]) << 16) + (((Int32)inbuffer[1]) << 8) + (Int32)inbuffer[2];
                        }
                        else
                        {
                            data = (((Int32)inbuffer[0]) << 8) + (Int32)inbuffer[1];
                        }
                        _endOfConversion = true; return data &= _dataMask;
                    }
                    else
                    {
                        _endOfConversion = false;
                        return 0; // return something
                    }
                }

                void writeConfRegister(byte value)
                {
                    
                    byte[] outbuffer = { value };
                    I2CDevice.I2CWriteTransaction writeTransaction = I2CDevice.CreateWriteTransaction(outbuffer);
                    I2CDevice.I2CTransaction[] xAction = new I2CDevice.I2CTransaction[] { writeTransaction };

                    _i2cBus = new I2CDevice(_config);
                    int transferred = _i2cBus.Execute(xAction, _transactionTimeOut);
                    _i2cBus.Dispose();
                    if (transferred < outbuffer.Length)
                        throw new System.IO.IOException("Error i2cBus " + _sla);
                }
            }

        }
    }
}
