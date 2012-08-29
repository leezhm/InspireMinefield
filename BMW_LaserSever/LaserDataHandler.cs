using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//
// Handle all Laser data
//
// Written by Leezhm(at)126.com, 1st September, 2011.
//
// Contact : Leezhm(at)126.com
//
// All Right Reserved. Copyright (at) Leezhm(at)126.com.
//
// Last modified by Leezhm(at)126.com on 29th September, 2011.
//

using System.Globalization;       // for NumberStyles
using System.Collections;         // for ArrayList

using System.Drawing;             // for PointF

using System.IO;                                       // for MemoryStream
using System.Runtime.Serialization.Formatters.Binary;  // for BinaryFormatter

namespace BMW_LaserSever
{
    public class AlarmEventArg : System.EventArgs
    {
        public bool Detected { get; set; }

        public AlarmEventArg(bool isDetected) : base()
        {
            Detected = isDetected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="arg"></param>
    public delegate void DetectPersonEventHandler(object sender, AlarmEventArg arg);

    [System.Runtime.InteropServices.GuidAttribute("96A2E954-26AA-4C12-9740-123A762D7E22")]
    public class LaserDataHandler
    {
        /// <summary>
        /// NLog
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Header of Laser Measured Data
        /// </summary>
        private static LaserDataHeader header = new LaserDataHeader();

        // for Debug
        public double xPixelPos = 0;
        public double yPixelPos = 0;

        /// <summary>
        /// Convert the data string to a string array
        /// </summary>
        private string[] dataList = null;

        /// <summary>
        /// Store all the valid data for TUIO
        /// </summary>
        private ArrayList alValidData = new ArrayList();

        public bool DetectPerson { get; set; }

        private System.Windows.Controls.MediaElement Alarm = null;

        /// <summary>
        /// Singleton class
        /// </summary>
        class Nested
        {
            internal static readonly LaserDataHandler instance = new LaserDataHandler();
        }

        LaserDataHandler()
        {
            
        }

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        public static LaserDataHandler Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        public void Close()
        {
        }

        #region Detected Event
        public event DetectPersonEventHandler DetectedPerson;

        public void SendEvent(bool detected)
        {
            if (null != DetectedPerson)
            {
                AlarmEventArg arg = new AlarmEventArg(detected);
                DetectedPerson(this, arg);
            }
        }
        #endregion // Detected Event


        #region Parse all data
        /// <summary>
        /// Split string to an array
        /// </summary>
        /// <param name="msgs"></param>
        public void HandleMeasuredData(ref string msgs)
        {
            if (string.Empty == msgs)
                return;

            dataList = msgs.Split((char)0x20);

            try
            {
                // Set the header
                header.CmdType = dataList[0];
                header.Command = dataList[1];
                header.VersionNumber = UInt16.Parse(dataList[2], NumberStyles.HexNumber);
                header.DeviceNumber = UInt16.Parse(dataList[3], NumberStyles.HexNumber);
                header.SerialNumber = UInt32.Parse(dataList[4], NumberStyles.HexNumber);
                header.DeviceStatus = dataList[5] + dataList[6];
                header.MessageCounter = UInt16.Parse(dataList[7], NumberStyles.HexNumber);
                header.ScanCounter = UInt16.Parse(dataList[8], NumberStyles.HexNumber);
                header.PowerUpDuration = UInt32.Parse(dataList[9], NumberStyles.HexNumber);
                header.TransmissionDuration = UInt32.Parse(dataList[10], NumberStyles.HexNumber);
                header.InputStatus = dataList[11] + dataList[12];
                header.OutputStatus = dataList[13] + dataList[14];
                header.ReservedByteA = UInt16.Parse(dataList[15], NumberStyles.HexNumber);
                header.ScanningFrequency = UInt32.Parse(dataList[16], NumberStyles.HexNumber) / 100;
                header.MeasurementFrequency = UInt32.Parse(dataList[17], NumberStyles.HexNumber) / 100;
                header.NumberEncoders = UInt16.Parse(dataList[18], NumberStyles.HexNumber);
                header.NumberChannels16bit = UInt16.Parse(dataList[19], NumberStyles.HexNumber);
                header.MeasureDataContent = dataList[20];
                header.ScalingFactor = UInt32.Parse(dataList[21], NumberStyles.HexNumber);
                header.ScalingOffset = UInt32.Parse(dataList[22], NumberStyles.HexNumber);
                header.StartingAngle = UInt32.Parse(dataList[23], NumberStyles.HexNumber) / 10000;
                header.AngularStepWidth = UInt16.Parse(dataList[24], NumberStyles.HexNumber);
                header.NumberData = UInt16.Parse(dataList[25], NumberStyles.HexNumber);
            }
            catch (FormatException fexpt)
            {
                logger.Fatal(fexpt.ToString());
                return;
            }

            // print the header
            //logger.Debug(header.PrintMeasuredDataHeader);

            // Handle Data
            HandleDataByFilter(); 

            // finished and clear the string array
            Array.Clear(dataList, 0, dataList.Length);
        }

        /// <summary>
        /// Selected all point in the valid range
        /// </summary>
        private void HandleDataByFilter()
        {
            // obtain the begin and end angle
            uint begin = (uint)Math.Abs(LaserSetting.dataStart - LaserSetting.scanStart) * 10000 / header.AngularStepWidth + 26;
            uint end = (uint)Math.Abs(LaserSetting.dataEnd - LaserSetting.scanStart) * 10000 / header.AngularStepWidth + 26;


            if (end >= dataList.Length || 2500 != header.AngularStepWidth)
            {
                logger.Fatal("Angular Step Width is " + header.AngularStepWidth + " != 2500!!! end(" + end +  ") >= dataList.Length(" + dataList.Length + ")");

                return;
            }

            //logger.Debug("begin = " + begin + "  end = " + end + "  Count = " + dataList.Length + " DataStart=" + LaserSetting.dataStart +
            //             "  ScanStart=" + LaserSetting.scanStart + " AngularStepWidth=" + header.AngularStepWidth);

            double xPos = 0.0f;
            double yPos = 0.0f;
            double distance = 0.0f;

            double radius = LaserSetting.diameter / 2;

            // reset
            DetectPerson = false;

            for (; begin <= end; ++ begin)
            {
                try
                {
                    distance = uint.Parse(dataList[begin], NumberStyles.HexNumber);
                }
                catch (FormatException fexpt)
                {
                    logger.Fatal(fexpt.ToString());
                    return;
                }

                if (LaserSetting.diameter < distance) // less than LaserSetting.diameter
                    continue;

                double angle = (double)(begin - 26) * header.AngularStepWidth / 10000 - Math.Abs(LaserSetting.scanStart);

                double currentAngle = (double)(angle - LaserSetting.angluarCorrection) * Math.PI / 180;


                xPos = (double)(distance * Math.Cos(currentAngle));
                if ((LaserSetting.diameter / 2) < Math.Abs(xPos) || Math.Abs(xPos) <= 0) continue;

                yPos = (double)(distance * Math.Sin(currentAngle));
                if ((LaserSetting.diameter + LaserSetting.offset) < Math.Abs(yPos) || yPos <= LaserSetting.offset) continue;

                if (Math.Pow(radius, 2.0) < (Math.Pow(xPos, 2.0) + Math.Pow(Math.Abs(yPos - LaserSetting.diameter / 2 - LaserSetting.offset), 2.0))) continue;

                #region Except Chair

                if (LaserSetting.leftTopStart <= angle && LaserSetting.leftTopEnd >= angle) continue;
                if (LaserSetting.leftBottomStart <= angle && LaserSetting.leftBottomEnd >= angle) continue;
                if (LaserSetting.rightTopStart <= angle && LaserSetting.rightTopEnd >= angle) continue;
                if (LaserSetting.rightBottomStart <= angle && LaserSetting.rightBottomEnd >= angle) continue;

                #endregion // Except Chair


                logger.Debug("distance = " + distance + " x = " + xPos + "(" + Math.Abs(Math.Sin(2 * currentAngle)) * LaserSetting.diameter / 2 +
                             ")" + "  y = " + yPos + "(" + (Math.Abs(Math.Cos(2 * currentAngle)) + 1) * LaserSetting.diameter / 2 + ")" + "  angular=" + angle);

                DetectPerson = true;
            }

            //
            //
            //
            SendEvent(DetectPerson);
        }

        #endregion // Parse all data
    }
}