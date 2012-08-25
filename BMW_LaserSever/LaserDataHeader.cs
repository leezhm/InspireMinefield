using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//
// Define the data class that include all data header
//
// Written by Leezhm(at)126.com, 29th August, 2011.
//
// Contact : Leezhm@126.com
//
// All Right Reserved. Copyright (at) Leezhm(at)126.com.
//
// Last modified by Leezhm(at)126.com on 29th August, 2011.
//


namespace BMW_LaserSever
{
    class LaserDataHeader
    {
        public LaserDataHeader()
        {
        }

        /// <summary>
        /// Type of Command (3bytes)
        /// </summary>
        private string cmdType = string.Empty;
        public string CmdType
        {
            get
            {
                return this.cmdType;
            }

            set
            {
                if (value != this.command)
                    this.cmdType = value;
            }

        }

        /// <summary>
        /// Command (11 bytes)
        /// </summary>
        private string command = string.Empty;
        public string Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (value != this.command)
                    this.command = value;
            }
        }

        /// <summary>
        /// Version Number (2 bytes)
        /// </summary>
        private UInt16 versionNumber = 0;
        public UInt16 VersionNumber
        {
            get
            {
                return this.versionNumber;
            }

            set
            {
                if (value != this.versionNumber)
                    this.versionNumber = value;
            }
        }

        /// <summary>
        /// Device Number (2 bytes)
        /// </summary>
        private UInt16 deviceNumber = 0;
        public UInt16 DeviceNumber
        {
            get
            {
                return this.deviceNumber;
            }

            set
            {
                if (value != this.deviceNumber)
                    this.deviceNumber = value;
            }
        }

        /// <summary>
        /// Serial Number (4 bytes)
        /// </summary>
        private UInt32 serialNumber = 0;
        public UInt32 SerialNumber
        {
            get
            {
                return serialNumber;
            }

            set
            {
                if (value != this.serialNumber)
                    this.serialNumber = value;
            }
        }
        
        /// <summary>
        /// Device Status (Please Attention 2 x 2*(UInt16))
        /// Use a string to store it
        /// </summary>
        private string deviceStatus = string.Empty;
        public string DeviceStatus
        {
            get
            {
                return this.deviceStatus;
            }

            set
            {
                if (value != this.deviceStatus)
                    this.deviceStatus = value;
            }
        }

        /// <summary>
        /// Message Counter (2 byte, 0 ~ 65535)
        /// </summary>
        private UInt16 messageCounter = 0;
        public UInt16 MessageCounter
        {
            get
            {
                return this.messageCounter;
            }

            set
            {
                if (value != this.messageCounter)
                    this.messageCounter = value;
            }
        }

        /// <summary>
        /// Scan Counter (2 bytes 0 ~ 65535)
        /// </summary>
        private UInt16 scanCounter = 0;
        public UInt16 ScanCounter
        {
            get
            {
                return this.scanCounter;
            }

            set
            {
                if (value != this.scanCounter)
                    this.scanCounter = value;
            }
        }

        /// <summary>
        /// Power Up Duration ( 4byttes, 0 ~ 68,719,476,735)
        /// </summary>
        private UInt32 powerUpDuration = 0;
        public UInt32 PowerUpDuration
        {
            get
            {
                return this.powerUpDuration;
            }

            set
            {
                if (value != this.powerUpDuration)
                    this.powerUpDuration = value;
            }
        }

        /// <summary>
        /// Transmission Duration (4 bytes, 0 ~ 68,719,476,735)
        /// </summary>
        private UInt32 transmissionDuration = 0;
        public UInt32 TransmissionDuration
        {
            get
            {
                return this.transmissionDuration;
            }

            set
            {
                if (value != this.transmissionDuration)
                    this.transmissionDuration = value;
            }
        }

        /// <summary>
        /// Input Status (Please Attention 2 x 2*(UInt16))
        /// Use a string to store it
        /// </summary>
        private string inputStatus = string.Empty;
        public string InputStatus
        {
            get
            {
                return this.inputStatus;
            }

            set
            {
                if (value != this.inputStatus)
                    this.inputStatus = value;
            }
        }

        /// <summary>
        /// Output Status (Please Attention 2 x 2*(UInt16))
        /// Use a string to store it
        /// </summary>
        private string outputStatus = string.Empty;
        public string OutputStatus
        {
            get
            {
                return this.outputStatus;
            }

            set
            {
                if (value != this.outputStatus)
                    this.outputStatus = value;
            }
        }

        /// <summary>
        /// Reserved Byte A
        /// </summary>
        private UInt16 reservedByteA = 0;
        public UInt16 ReservedByteA
        {
            get
            {
                return this.reservedByteA;
            }

            set
            {
                if (value != this.reservedByteA)
                    this.reservedByteA = value;
            }
  
        }

        /// <summary>
        /// Scanning Frequency (4bytes 25Hz = 2500 , 50Hz = 5000)
        /// information 1/100Hz
        /// </summary>
        private UInt32 scanningFrequency;
        public UInt32 ScanningFrequency
        {
            get
            {
                return this.scanningFrequency;
            }

            set
            {
                if (value != this.scanningFrequency)
                    this.scanningFrequency = value;
            }
        }

        /// <summary>
        /// Measurement Frequency (4bytes)
        /// Frequency between two separate measurements in 100Hz
        /// </summary>
        private UInt32 measurementFrequency;
        public UInt32 MeasurementFrequency
        {
            get
            {
                return this.measurementFrequency;
            }

            set
            {
                if (value != this.measurementFrequency)
                    this.measurementFrequency = value;
            }
        }

        /// <summary>
        /// Defines the number of encoders from which data are output
        /// 0 means no encode (2 bytes)
        /// </summary>
        private UInt16 numberEncoders = 0;
        public UInt16 NumberEncoders
        {
            get
            {
                return this.numberEncoders;
            }

            set
            {
                if (value != this.numberEncoders)
                    this.numberEncoders = value;
            }
        }

        /// <summary>
        /// Defines the number of 16-bit output channels on which measured data
        /// are output. (2 bytes)
        /// </summary>
        private UInt16 numberChannels16bit;
        public UInt16 NumberChannels16bit
        {
            get
            {
                return this.numberChannels16bit;
            }

            set
            {
                if (value != this.numberChannels16bit)
                    this.numberChannels16bit = value;
            }
        }

        /// <summary>
        /// The message part defines the contents of the output channel.(5 bytes)
        /// </summary>
        private string measuredDataContent = string.Empty;
        public string MeasureDataContent
        {
            get
            {
                return this.measuredDataContent;
            }

            set
            {
                if (value != this.measuredDataContent)
                    this.measuredDataContent = value;
            }
        }

        /// <summary>
        /// Scaling Factor (4 bytes)
        /// </summary>
        private UInt32 scalingFactor = 0;
        public UInt32 ScalingFactor
        {
            get
            {
                return this.scalingFactor;
            }

            set
            {
                if (value != this.scalingFactor)
                    this.scalingFactor = value;
            }
        }

        /// <summary>
        /// For the LMS always 0(Real 4 bytes)
        /// </summary>
        private UInt32 scalingOffset = 0;
        public UInt32 ScalingOffset
        {
            get
            {
                return this.scalingOffset;
            }

            set
            {
                if (value != this.scalingOffset)
                    this.scalingOffset = value;
            }
        }

        /// <summary>
        /// information 1/10,000 degree (4bytes)
        /// </summary>
        private UInt32 startingAngle = 0;
        public UInt32 StartingAngle
        {
            get
            {
                return this.startingAngle;
            }

            set
            {
                if (value != this.startingAngle)
                    this.startingAngle = value;
            }
        }

        /// <summary>
        /// information 1/10,000 degree (2bytes)
        /// </summary>
        private UInt16 angularStepWidth;
        public UInt16 AngularStepWidth
        {
            get
            {
                return this.angularStepWidth;
            }

            set
            {
                if (value != this.angularStepWidth)
                    this.angularStepWidth = value;
            }
        }

        /// <summary>
        /// Defines the number of items of measured data output(2 bytes)
        /// (0 ~ 10820)
        /// </summary>
        private UInt16 numberData;
        public UInt16 NumberData
        {
            get
            {
                return this.numberData;
            }

            set
            {
                if (value != this.numberData)
                    this.numberData = value;
            }
        }

        /// <summary>
        /// Print the info of header
        /// </summary>
        /// <returns></returns>
        public string PrintMeasuredDataHeader()
        {
            string FormatMsgs = "Command Type          " + this.CmdType + "\n" +
                                "Command               " + this.Command + "\n" +
                                "Version Number        " + this.VersionNumber + "\n" +
                                "Device Number         " + this.DeviceNumber + "\n" +
                                "Serial Number         " + this.SerialNumber + "\n" +
                                "Device Status         " + this.DeviceStatus + "\n" +
                                "Message Counter       " + this.MessageCounter + "\n" +
                                "Scan Counter          " + this.ScanCounter + "\n" +
                                "Power Up Duration     " + this.PowerUpDuration + "\n" +
                                "Transmission Duration " + this.TransmissionDuration + "\n" +
                                "Input  Status         " + this.InputStatus + "\n" +
                                "Output Status         " + this.OutputStatus + "\n" +
                                "Reserved ByteA        " + this.ReservedByteA + "\n" +
                                "Scanning Frequency    " + this.ScanningFrequency + "\n" +
                                "Measurement Frequency " + this.MeasurementFrequency + "\n" +
                                "Number Encoders       " + this.NumberEncoders + "\n" +
                                "Number Channels 16Bit " + this.NumberChannels16bit + "\n" +
                                "Measure Data Content  " + this.MeasureDataContent + "\n" +
                                "Scaling Factor        " + this.ScalingFactor + "\n" +
                                "Scaling Offset        " + this.ScalingOffset + "\n" +
                                "Starting Angle        " + this.StartingAngle + "\n" +
                                "Angular Step Width    " + this.AngularStepWidth + "\n" +
                                "Number Data           " + this.NumberData + "\n";

            return FormatMsgs;
        }
    }
}
