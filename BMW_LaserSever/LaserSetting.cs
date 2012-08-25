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
        /// Angluar Correction
        /// </summary>
        public static double angluarCorrection = 0;

        public static double diameter = 0;

        #endregion // Congfig

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

                            LaserSetting.diameter = double.Parse(data.Attributes["diameter"].Value);
                            LaserSetting.dataStart = double.Parse(data.Attributes["Start"].Value);
                            LaserSetting.dataEnd = double.Parse(data.Attributes["End"].Value);
                            LaserSetting.angluarCorrection = double.Parse(data.Attributes["Correction"].Value);
                        }
                    }
                }
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
                            data.SetAttribute("diameter", LaserSetting.diameter.ToString());
                            data.SetAttribute("Correction", LaserSetting.angluarCorrection.ToString());
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

             "Start Angleb --> " + LaserSetting.scanStart + "\n" + "End Angle --> " + LaserSetting.scanEnd + "\n" +
             "Valid Start Angle --> " + LaserSetting.dataStart + "\n" + "Valid End Angle --> " + LaserSetting.dataEnd + "\n" +
             "Angular Correction --> " + LaserSetting.angluarCorrection + "\n";

            logger.Debug(msg);
        }
    }
}
