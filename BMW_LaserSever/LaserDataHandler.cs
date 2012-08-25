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
    struct Point3D
    {
        public double distance;
        public double xPos;
        public double yPos;

        /// <summary>
        /// Indicate whether is the same point
        /// </summary>
        public int session;

        /// <summary>
        /// avarage angle
        /// </summary>
        public uint angele;

        public bool isUseful;

        public void Clear()
        {
            this.distance = 0;
            this.xPos = 0;
            this.yPos = 0;
            this.session = -1;
            this.angele = 0;

            this.isUseful = false;
        }

        public override string ToString()
        {
            string msg = "Session --> " + this.isUseful + " " + this.session + "  " + this.angele + " "
                         + this.distance + "  " + this.xPos + "  " + this.yPos;

            return msg;
        }
    }

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
            logger.Debug(header.PrintMeasuredDataHeader);

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
                logger.Fatal("Angular Step Width is " + header.AngularStepWidth + " != 2500!!!");

                return;
            }

            //logger.Debug("begin = " + begin + "  end = " + end + "  Count = " + dataList.Length + " DataStart=" + LaserSetting.dataStart +
            //             "  ScanStart=" + LaserSetting.scanStart + " AngularStepWidth=" + header.AngularStepWidth);

            for (; begin <= end; ++ begin)
            {
                Point3D pos = new Point3D();

                try
                {
                    pos.distance = uint.Parse(dataList[begin], NumberStyles.HexNumber);
                }
                catch (FormatException fexpt)
                {
                    logger.Fatal(fexpt.ToString());
                    return;
                }

                if (3200 < pos.distance || 800 > pos.distance) // less than 2000mm
                //if (2400 < pos.distance || 800 > pos.distance) // less than 2000mm
                    continue;

                double angle = (double)(begin - 26) * header.AngularStepWidth / 10000 - Math.Abs(LaserSetting.scanStart);

                double currentAngle = (double)(angle - LaserSetting.angluarCorrection) * Math.PI / 180;


                pos.xPos = (double)(pos.distance * Math.Cos(currentAngle));
                //if (LaserSetting.minWidth > pos.xPos || LaserSetting.maxWidth < pos.xPos) continue;


                pos.yPos = (double)(pos.distance * Math.Sin(currentAngle));
                //if (LaserSetting.minHeight > Math.Abs(pos.yPos) || LaserSetting.maxHeight < Math.Abs(pos.yPos)) continue;
                logger.Debug("x = " + pos.xPos + "y = " + pos.yPos + "  angular=" + angle);

                System.Media.SoundPlayer sndPlayer = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"/ir_begin.wav");
                sndPlayer.PlaySync();
            
            }
        }

        #endregion // Parse all data
    }
}