using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//
// A static class include all laser setting data.
//
// Written by Leezhm(at)126.com, 27th September, 2011.
//
// Contact : Leezhm(at)126.com
//
// All Right Reserved. Copyright (at) Leezhm(at)126.com.
//
// Last modified by Leezhm(at)126.com on 27th September, 2011.
//

using System.Xml;

namespace BMW_LaserSever
{
    public class LaserSetting
    {
        /// <summary>
        /// NLog
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Server
        public static string ipAddress = "192.168.0.245";
        public static uint port = 2112;
        #endregion // Server

        #region Config

        public static double plunIn = 0;

        public static string direction = "";
        public static int timerInterval = 0;

        public static int blobSize = 0;

        /// <summary>
        /// Scan Start Angle
        /// </summary>
        public static double scanStart = 0;

        /// <summary>
        /// Scan End Angle
        /// </summary>
        public static double scanEnd = 0;

        /// <summary>
        /// Data Start Angle
        /// </summary>
        public static double dataStart = 0;

        /// <summary>
        /// Data End Angle
        /// </summary>
        public static double dataEnd = 0;

        /// <summary>
        /// First Data Apliter Angle
        /// </summary>
        public static double firstDataSpliter = 0;

        /// <summary>
        /// Second Data Apliter Angle
        /// </summary>
        public static double secondDataSpliter = 0;

        /// <summary>
        /// Angluar Correction
        /// </summary>
        public static double angluarCorrection = 0;

        /// <summary>
        /// Current Resolution
        /// </summary>
        public static double screenWidth = 0;
        public static double screenHeight = 0;

        /// <summary>
        /// For Rectangle
        /// </summary>
        public static double maxWidth = 0;
        public static double minWidth = 0;
        public static double maxHeight = 0;
        public static double minHeight = 0;

        /// <summary>
        /// Size of Blob
        /// </summary>
        public static double maxBlob = 0;
        public static double minBlob = 0;

        /// <summary>
        /// 
        /// </summary>
        public static Boolean IsCalibration = false;

        #endregion // Congfig

        #region Section
        public struct Section
        {
            public double Angle;
            public double Distance;
        }

        /// <summary>
        /// Store all section information
        /// </summary>
        public static Dictionary<string, Section> sectionDict = new Dictionary<string, Section>();
        #endregion // Section

        /// <summary>
        /// Load Laser System Configuration
        /// </summary>
        public static void InitLaserSetting()
        {
            string xmlPath = System.Environment.CurrentDirectory + "\\SystemConfig.xml";

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                // for ip address
                XmlNode server = xmlDoc.SelectSingleNode("/Luxoom/Laser/Server");
                LaserSetting.ipAddress = server.Attributes["IP"].Value;
                LaserSetting.port = uint.Parse(server.Attributes["Port"].Value);

                // for Setting
                XmlNode setting = xmlDoc.SelectSingleNode("/Luxoom/Laser/Setting");
                LaserSetting.direction = setting.Attributes["Direction"].Value;
                LaserSetting.timerInterval = int.Parse(setting.Attributes["TimerInterval"].Value);
                LaserSetting.blobSize = int.Parse(setting.Attributes["BlobSize"].Value);

                // For Configuration
                XmlNodeList configList = xmlDoc.SelectNodes("/Luxoom/Laser/Setting/Config");
                if (null != configList)
                {
                    foreach (XmlNode config in configList)
                    {
                        // Get the indicated configuration
                        if (LaserSetting.direction == config.Attributes["Name"].Value)
                        {
                            // Scan Setting
                            XmlNode scan = config.SelectSingleNode("Scan");
                            LaserSetting.scanStart = double.Parse(scan.Attributes["Start"].Value);
                            LaserSetting.scanEnd = double.Parse(scan.Attributes["End"].Value);

                            // Data Setting
                            XmlNode data = config.SelectSingleNode("Data");
                            LaserSetting.dataStart = double.Parse(data.Attributes["Start"].Value);
                            LaserSetting.dataEnd = double.Parse(data.Attributes["End"].Value);
                            LaserSetting.firstDataSpliter = double.Parse(data.Attributes["FirstSpliter"].Value);
                            LaserSetting.secondDataSpliter = double.Parse(data.Attributes["SecondSpliter"].Value);
                            LaserSetting.angluarCorrection = double.Parse(data.Attributes["Correction"].Value);

                            // Blob
                            XmlNode blob = config.SelectSingleNode("Blob");
                            LaserSetting.maxBlob = double.Parse(blob.Attributes["Max"].Value);
                            LaserSetting.minBlob = double.Parse(blob.Attributes["Min"].Value);

                            // Window.Rectangel
                            XmlNode rectangle = config.SelectSingleNode("Window/Rectangle");
                            LaserSetting.maxWidth = double.Parse(rectangle.Attributes["MaxWidth"].Value);
                            LaserSetting.minWidth = double.Parse(rectangle.Attributes["MinWidth"].Value);
                            LaserSetting.maxHeight = double.Parse(rectangle.Attributes["MaxHeight"].Value);
                            LaserSetting.minHeight = double.Parse(rectangle.Attributes["MinHeight"].Value);
                            
                            LaserSetting.plunIn = double.Parse(rectangle.Attributes["PlunIn"].Value);

                            // Window.Rectangel
                            XmlNode resolution = config.SelectSingleNode("Window/Resolution");
                            LaserSetting.screenWidth = double.Parse(resolution.Attributes["Width"].Value);
                            LaserSetting.screenHeight = double.Parse(resolution.Attributes["Height"].Value);
                        }
                    }
                }

                #region Section
                XmlNodeList sectionList = xmlDoc.SelectNodes("/Luxoom/Section/City");
                if (null != sectionList)
                {
                    foreach (XmlNode city in sectionList)
                    {
                        Section section = new Section();
                        section.Angle = double.Parse(city.Attributes["Angle"].Value);
                        section.Distance = double.Parse(city.Attributes["Distance"].Value);

                        sectionDict.Add(city.Attributes["Name"].Value, section);
                    }
                }

                // Print
                LaserSetting.Print();

                #endregion // Section
            }
            catch (Exception expt)
            {
                logger.Fatal(expt.ToString());
            }

        }

        public static void SaveLaserSetting()
        {
            string xmlPath = System.Environment.CurrentDirectory + "\\SystemConfig.xml";

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                // for Setting
                XmlNode setting = xmlDoc.SelectSingleNode("/Luxoom/Laser/Setting");
                LaserSetting.direction = setting.Attributes["Direction"].Value;

                // For Configuration
                XmlNodeList configList = xmlDoc.SelectNodes("/Luxoom/Laser/Setting/Config");
                if (null != configList)
                {
                    foreach (XmlNode config in configList)
                    {
                        // Get the indicated configuration
                        if (LaserSetting.direction == config.Attributes["Name"].Value)
                        {
                            // Data Setting
                            XmlElement data = (XmlElement)config.SelectSingleNode("Data");
                            data.SetAttribute("Correction", LaserSetting.angluarCorrection.ToString());

                            // Window.Rectange
                            XmlElement rectangle = (XmlElement)config.SelectSingleNode("Window/Rectangle");

                            // set new value
                            rectangle.SetAttribute("MaxWidth", LaserSetting.maxWidth.ToString());
                            rectangle.SetAttribute("MinWidth", LaserSetting.minWidth.ToString());
                            rectangle.SetAttribute("MaxHeight", LaserSetting.maxHeight.ToString());
                            rectangle.SetAttribute("MinHeight", LaserSetting.minHeight.ToString());
                        }
                    }
                }

                xmlDoc.Save(xmlPath);
            }
            catch (Exception expt)
            {
                logger.Fatal(expt.ToString());
            }
        }

        /// <summary>
        /// Print All Configuration Data
        /// </summary>
        public static void Print()
        {
            string msg = "Laser Scan Server IP --> " + LaserSetting.ipAddress + "\n" +
             "Laser Scan Server Listenning Port --> " + LaserSetting.port + "\n" +
             "Laser Direction --> " + LaserSetting.direction + "\n" + 
             "Timer Interval --> " + LaserSetting.timerInterval + "\n" +
             "Blob Size --> " + LaserSetting.blobSize + "\n" + 
             "Whole Screen --> minWidth " + LaserSetting.minWidth + "  maxWidth " + LaserSetting.maxWidth +
             "  minHeight " + LaserSetting.minHeight + "  maxHeight " + LaserSetting.maxHeight + " --> " +
             "Resolution " + LaserSetting.screenWidth + " X " + LaserSetting.screenHeight + "\n\n" +

             "Start Angleb --> " + LaserSetting.scanStart + "\n" + "End Angle --> " + LaserSetting.scanEnd + "\n" +
             "Valid Start Angle --> " + LaserSetting.dataStart + "\n" + "Valid End Angle --> " + LaserSetting.dataEnd + "\n" +
             "Valid First Spliter Angle --> " + LaserSetting.firstDataSpliter + "\n" + "Valid Second Spliter Angle --> " + LaserSetting.secondDataSpliter + "\n" +
             "Angular Correction --> " + LaserSetting.angluarCorrection + "\n" +
             "Min Blob --> " + LaserSetting.minBlob + "\n" + "Max Blob --> " + LaserSetting.maxBlob;

            logger.Debug(msg);
        }
    }
}
