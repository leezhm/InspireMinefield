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

namespace LaserServer
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


        private static int SessionID = -1;

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
        /// store all valid point in every scan
        /// </summary>
        private Dictionary<int, Point3D> md = new Dictionary<int, Point3D>();

        private Dictionary<int, Point3D> mdKeyPoints = new Dictionary<int, Point3D>();

        /// <summary>
        /// a copy pf md
        /// </summary>
        private Dictionary<int, Point3D> mdCashe = new Dictionary<int, Point3D>();

        private Dictionary<int, TouchPoint> tp = new Dictionary<int, TouchPoint>();
        private Dictionary<int, TouchPoint> tpCashe = new Dictionary<int, TouchPoint>();

        /// <summary>
        /// Indicate whether there are some blobs
        /// </summary>
        private Boolean isContainTuioBlob = false;

        private int CurrentDataSerial = 0;
        private int BeginSelectedDataSerial = 0;
        private Boolean IsBeginSelected = false;

        //private int latestDistanceFilter = 0;

        /// <summary>
        /// for Tuio Server
        /// </summary>
        private TuioServer tuioServer ;

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

        public void InitTuioServer()
        {
            if(null == tuioServer)
                tuioServer = new TuioServer();
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
            if (null != tuioServer)
            {
                tuioServer.Close();
                tuioServer = null;
            }

            isContainTuioBlob = false;
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
            //logger.Debug(header.PrintMeasuredDataHeader);

            //md.Clear();

            // Handle Data
            HandleDataByFilter(); 
            
            ++ CurrentDataSerial;

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

            this.isContainTuioBlob = false;

            if (end >= dataList.Length || 2500 != header.AngularStepWidth)
            {
                logger.Fatal("Angular Step Width is " + header.AngularStepWidth + " != 2500!!!");

                return;
            }

            //logger.Debug("begin = " + begin + "  end = " + end + "  Count = " + dataList.Length + " DataStart=" + LaserSetting.dataStart +
            //             "  ScanStart=" + LaserSetting.scanStart + " AngularStepWidth=" + header.AngularStepWidth);

            #region Parse All Data One time
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
                if (LaserSetting.minWidth > pos.xPos || LaserSetting.maxWidth < pos.xPos) continue;


                pos.yPos = (double)(pos.distance * Math.Sin(currentAngle));
                if (LaserSetting.minHeight > Math.Abs(pos.yPos) || LaserSetting.maxHeight < Math.Abs(pos.yPos)) continue;
                //logger.Debug("x = " + pos.xPos + "y = " + pos.yPos + "  angular=" + angle);

                pos.session = -1;
                pos.angele = begin;
                pos.isUseful = false;

                //logger.Trace("Current = " + CurrentDataSerial + "  Screen --> " + begin);
                md.Add((int)begin, pos);
            
            }

            // Create Tuio Touch Point
            CreateTuioTouchPoint();
            #endregion //  Parse All Data One time

            if (!isContainTuioBlob)
            {
                if (0 == tp.Count)
                    return;

                // remove blob
                foreach (int no in tp.Keys)
                {
                    tuioServer.DeleteTouchCursor(no);
                }

                // Send TUIO Messages
                tuioServer.SendTuioMessages();

                // clear 
                tp.Clear();
            }
        }

        /// <summary>
        /// Create Tuio Touch Point
        /// </summary>
        private void CreateTuioTouchPoint()
        {
            if (0 == md.Count)
                return;

            // get all keys
            int[] keys = new int[md.Keys.Count];
            int i = 0;
            foreach (int j in md.Keys)
            {
                keys[i ++] = j;
            }

            Point3D p = new Point3D();

            int tmpKey = keys[0];
            double tmpDistance = md[keys[0]].distance;

            // Begin Selected touch point
            if (!IsBeginSelected)
            {
                IsBeginSelected = true;
                BeginSelectedDataSerial = CurrentDataSerial;
                //latestDistanceFilter = 0;
            }

            for(i = 0; i < keys.Length; ++ i)
            {
                // Miniumn point count
                int pointCount = (int)((LaserSetting.minBlob * 720.0) / (Math.PI * md[keys[i]].distance));

                //logger.Debug("Point Count == " + pointCount);

                if (pointCount >= Math.Abs(keys[i] - tmpKey))
                {
                    if (tmpDistance > md[keys[i]].distance)
                    {
                        tmpDistance = md[keys[i]].distance;
                        tmpKey = keys[i];

                        ///logger.Debug("Track tmpKey = " + tmpKey);
                    }
                }
                else
                {
                    if (i < keys.Length - 1)
                    {
                        // update the session for first point
                        p.distance = md[tmpKey].distance;
                        p.xPos = md[tmpKey].xPos;
                        p.yPos = md[tmpKey].yPos;
                        p.angele = md[tmpKey].angele;
                        p.session = md[tmpKey].session;
                        //p.isUseful = true;

                        md[tmpKey] = p;

                        #region Statistic Touch Point
                        //logger.Debug("@@@@@ CurrentDataSerial = " + CurrentDataSerial + "  BeginSelectedDataSerial =" + BeginSelectedDataSerial);
                        if (1 >= (CurrentDataSerial - BeginSelectedDataSerial))
                        {
                            ++CurrentDataSerial;

                            if (LaserSetting.blobSize <= mdKeyPoints.Count)
                            {
                                // Cout
                                //foreach (int key in mdKeyPoints.Keys)
                                //{
                                //    logger.Debug("key --> " + key + "  data --> " + mdKeyPoints[key].ToString());
                                //}

                                // CreateTouchPoint
                                SelectedTouchPoint();

                                // Clear and restart
                                mdKeyPoints.Clear();

                                // reset BeginDataSerial
                                BeginSelectedDataSerial = 0;
                                IsBeginSelected = false;
                            }

                            mdKeyPoints[CurrentDataSerial] = p;

                            BeginSelectedDataSerial = CurrentDataSerial;
                        }
                        else
                        {
                            // Clear and restart
                            mdKeyPoints.Clear();

                            // reset BeginDataSerial
                            BeginSelectedDataSerial = 0;
                            IsBeginSelected = false;
                        }

                        #endregion // Statistic Touch Point

                        p.Clear();

                        ////logger.Debug("Current Miniumn Point (1) --> " + tmpKey + " " + tmpDistance + " " + md[tmpKey].ToString() + "\n");

                        tmpKey = keys[i];
                        tmpDistance = md[tmpKey].distance;
                    }
                }
            }

            // update the session for first point
            p.distance = md[tmpKey].distance;
            p.xPos = md[tmpKey].xPos;
            p.yPos = md[tmpKey].yPos;
            p.angele = md[tmpKey].angele;
            p.session = md[tmpKey].session;
            //p.isUseful = true;

            md[tmpKey] = p;

            #region Statistic Touch Point
            //logger.Debug("@@@@@ CurrentDataSerial = " + CurrentDataSerial + "  BeginSelectedDataSerial =" + BeginSelectedDataSerial);
            if (1 >= (CurrentDataSerial - BeginSelectedDataSerial))
            {
                if (LaserSetting.blobSize <= mdKeyPoints.Count)
                {
                    // CreateTouchPoint
                    SelectedTouchPoint();

                    // Clear and restart
                    mdKeyPoints.Clear();

                    // reset BeginDataSerial
                    BeginSelectedDataSerial = 0;
                    IsBeginSelected = false;
                }

                mdKeyPoints[CurrentDataSerial] = p;
                BeginSelectedDataSerial = CurrentDataSerial;
            }
            else
            {
                // Clear and restart
                mdKeyPoints.Clear();

                // reset BeginDataSerial
                BeginSelectedDataSerial = 0;
                IsBeginSelected = false;
            }

            #endregion // Statistic Touch Point

            p.Clear();

            /////logger.Debug("Current Miniumn Point (2) --> " + tmpKey + " " + tmpDistance + " " + md[tmpKey].ToString() + "\n");

            md.Clear();
        }

        private void CreateAndUpdateTouchPoint()
        {
            double x = 0;
            double y = 0;

            // for screen
            double width = LaserSetting.maxWidth + Math.Abs(LaserSetting.minWidth);
            double height = LaserSetting.maxHeight - LaserSetting.minHeight;

            //logger.Debug("width = " + width + "  height = " + height + "  --------------------->  ");

            // get all keys
            int[] keys = new int[mdKeyPoints.Keys.Count];
            int i = 0;
            foreach (int j in mdKeyPoints.Keys)
            {
                keys[i++] = j;
            }

            for(i = 0; i < keys.Length; ++ i)
            {
                if (mdKeyPoints[keys[i]].isUseful)
                {
                    //logger.Debug("TouchPoint --> " + mdKeyPoints[keys[i]].ToString());

                    if ("RightToLeft" == LaserSetting.direction)
                    {
                        xPixelPos = (double)((((mdKeyPoints[keys[i]].xPos + Math.Abs(LaserSetting.minWidth)) + width) * LaserSetting.screenWidth) / width);
                        x = (double)((mdKeyPoints[keys[i]].xPos + Math.Abs(LaserSetting.minWidth)) / width);
                    }
                    else if ("LeftToRight" == LaserSetting.direction)
                    {
                        if (0 >= mdKeyPoints[keys[i]].xPos)
                        {
                            xPixelPos = (double)(((width + LaserSetting.maxWidth + Math.Abs(mdKeyPoints[keys[i]].xPos)) * LaserSetting.screenWidth) / width);
                            x = (double)(((Math.Abs(mdKeyPoints[keys[i]].xPos)) + LaserSetting.maxWidth) / width);
                        }
                        else
                        {
                            xPixelPos = (double)(((width + (LaserSetting.maxWidth - Math.Abs(mdKeyPoints[keys[i]].xPos))) * LaserSetting.screenWidth) / width);
                            x = (double)((LaserSetting.maxWidth - (Math.Abs(mdKeyPoints[keys[i]].xPos))) / width);
                        }
                    }

                    yPixelPos = (double)(((mdKeyPoints[keys[i]].yPos - LaserSetting.minHeight) * LaserSetting.screenHeight) / height);

                    y = (Math.Abs(mdKeyPoints[keys[i]].yPos) - LaserSetting.minHeight) / height;

                    //logger.Debug("-----------New Point--------------->" + md[keys[i]].session + "  " + x + "  " + y);

                    HandleTuioMessage(mdKeyPoints[keys[i]].session, x, y);

                    x = y = 0;
                }
            } // foreach
        }

        private void HandleTuioMessage(int sid, double x, double y)
        {
            if (0 == tp.Count) // add cursor
            {

                tp.Add(sid, new TouchPoint(sid, x, y));

                tuioServer.AddTouchCursor(sid, new PointF((float)x, (float)y), new PointF(0, 0));

                alValidData.Add(sid);
            }
            else
            {
                Boolean isFound = false;

                foreach (int id in tp.Keys)
                {
                    if (id == sid) // update cursor
                    {
                        // speed

                        //logger.Debug(angleid + " --> " + dictOriginalData[id].AngleID);

                        tuioServer.UpdateTouchCursor(sid, new PointF((float)x, (float)y), new PointF(0, 0));

                        alValidData.Add(sid);

                        isFound = true;
                    }
                }

                if (!isFound)
                {
                    tpCashe.Add(sid, new TouchPoint(sid, x, y));

                    tuioServer.AddTouchCursor(sid, new PointF((float)x, (float)y), new PointF(0, 0));

                    alValidData.Add(sid);

                    isFound = false;
                }
            }
        }

        private void SelectedTouchPoint()
        {
            double distance = 200000.0f;
            int usedKey = 0;

            ++ SessionID;

            foreach (int key in mdKeyPoints.Keys)
            {
                if (distance > mdKeyPoints[key].distance)
                {
                    distance = mdKeyPoints[key].distance;
                    usedKey = key;
                }

                //logger.Debug("******* " + mdKeyPoints[key].distance + " ************");
            }

            double angle = (double)(mdKeyPoints[usedKey].angele - 26) * header.AngularStepWidth / 10000 - Math.Abs(LaserSetting.scanStart);

            /// 
            /// Track Whether It is valiad for screen
            ///
            if (LaserSetting.minWidth > mdKeyPoints[usedKey].xPos || LaserSetting.maxWidth < mdKeyPoints[usedKey].xPos) return;
            if (LaserSetting.minHeight > Math.Abs(mdKeyPoints[usedKey].yPos) || LaserSetting.maxHeight < Math.Abs(mdKeyPoints[usedKey].yPos)) return;

            

            // update the session for first point
            Point3D p = new Point3D();
            p.distance = mdKeyPoints[usedKey].distance;
            p.xPos = mdKeyPoints[usedKey].xPos;
            p.yPos = mdKeyPoints[usedKey].yPos;
            p.session = SessionID;
            p.angele = mdKeyPoints[usedKey].angele;
            p.isUseful = true;

            mdKeyPoints[usedKey] = p;

            //logger.Debug("Selected Touch Point --> " + p.distance + "  " + p.angele + "  " + latestDistanceFilter);

            p.Clear();

            #region Send Tuio Message
            CreateAndUpdateTouchPoint();

            foreach (int id in tpCashe.Keys)
            {
                tp.Add(id, tpCashe[id]);
            }
            // clear after merge
            tpCashe.Clear();

            // tmp varilable
            ArrayList al = new ArrayList();

            // remove blob

            if (0 != alValidData.Count)
            {
                foreach (int  no in tp.Keys)
                {
                    if (!alValidData.Contains(no)) // remove
                    {
                        tuioServer.DeleteTouchCursor(no);

                        al.Add(no);
                    }

                    //logger.Debug(dictOriginalData[no].ToString());
                }
            }

            // update the dicOriginalData 
            foreach (int i in al)
            {
                tp.Remove(i);
            }

            // clear 
            al.Clear();
            alValidData.Clear();

            tuioServer.SendTuioMessages();
            #endregion // Send Tuio Message

            // need to updata tuio message
            if (!this.isContainTuioBlob)
                this.isContainTuioBlob = true;
        }

        #endregion // Parse all data
    }
}
