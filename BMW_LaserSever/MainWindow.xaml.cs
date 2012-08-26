using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//
// Main Class for Laser Server
//
// Written by Leezhm(at)126.com, 29th August, 2011.
//
// Contact : Leezhm(at)126.com
//
// All Right Reserved. Copyright (at) Leezhm(at)126.com.
//
// Last modified by Leezhm(at)126.com on 4th September, 2011.
//

using System.Threading;
using System.Windows.Threading;      // for DispatcherTimer

using System.ComponentModel;         // for CancelEventArgs
      
using System.Configuration;          // for ConfigurationManager
using System.Collections;            // for ArrayList

namespace BMW_LaserSever
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// NLog for current system
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region  for server
        /// <summary>
        /// Timer for received data from Laser Scan
        /// </summary>
        private DispatcherTimer dataTimer = null;
        #endregion // for server

        public MainWindow()
        {
            logger.Info("Start Laser Server");

            InitializeComponent();

            logger.Info("Init Laser Server");

            // handle Closing, KeyDown and StateChanged for MainWindow 
            this.Closing += OnMainWindowClosingHandler;

            // load basic configuration
            LaserSetting.InitLaserSetting();

            //
            InitInterface();

            LaserDataReceiver.Instance.InitReceiver();

            StartDispatherTimer();

            LaserDataHandler.Instance.DetectedPerson += Instance_DetectedPerson;
        }

        void Instance_DetectedPerson(object sender, AlarmEventArg arg)
        {
            //logger.Debug("Hello, " + arg.Detected.ToString());

            alarm.Volume = arg.Detected ? 100 : 0;
        }

        /// <summary>
        /// Start timer and receive data
        /// </summary>
        private void StartDispatherTimer()
        {
            // setup timer
            if (null == dataTimer)
            {
                dataTimer = new DispatcherTimer();
                dataTimer.Tick += LaserDataReceiver.Instance.StartReceivedData;
                dataTimer.Interval = new TimeSpan(0, 0, 0, 0, LaserSetting.timerInterval);

                dataTimer.Start();
            }
        }

        #region Main Window Events
        /// <summary>
        /// Closing current application and handle all need to be returned things
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMainWindowClosingHandler(object sender, CancelEventArgs args)
        {
            // close the dispatcher timer
            if (null != dataTimer && dataTimer.IsEnabled)
            {
                // stop
                dataTimer.Stop();

                dataTimer = null;
            }

            // Close the Laser Data Receiver
            LaserDataReceiver.Instance.Close();

            // Close the Laser Data Handler
            LaserDataHandler.Instance.Close();

            // save data
            LaserSetting.SaveLaserSetting();

            logger.Info("Closed Laser Server");
            logger.Info("*************************");
            logger.Info("\n");
        }
        #endregion // Main Window Events

        void InitInterface()
        {
            laserIP.Content = "Laser IP : " + LaserSetting.ipAddress;
            laserPort.Content = "Port : " + LaserSetting.port.ToString();
            laserDirection.Content = "Laser Direction : " + LaserSetting.direction;
            laserStart.Content = "Scan Start : " + LaserSetting.scanStart.ToString();
            laserEnd.Content = "Scan End : " + LaserSetting.scanEnd.ToString();

            timerInterval.Content = "Time Interval : " + LaserSetting.timerInterval.ToString();

            txtDiameter.Text = LaserSetting.diameter.ToString();

            leftTopStart.Text = LaserSetting.leftTopStart.ToString();
            leftTopEnd.Text = LaserSetting.leftTopEnd.ToString();

            leftBottomStart.Text = LaserSetting.leftBottomStart.ToString();
            leftBottomEnd.Text = LaserSetting.leftBottomEnd.ToString();

            rightTopStart.Text = LaserSetting.rightTopStart.ToString();
            rightTopEnd.Text = LaserSetting.rightTopEnd.ToString();

            rightBottomStart.Text = LaserSetting.rightBottomStart.ToString();
            rightBottomEnd.Text = LaserSetting.rightBottomEnd.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            LaserSetting.diameter = double.Parse(txtDiameter.Text);

            LaserSetting.leftTopStart = double.Parse(leftTopStart.Text);
            LaserSetting.leftTopEnd = double.Parse(leftTopEnd.Text);

            LaserSetting.leftBottomStart = double.Parse(leftBottomStart.Text);
            LaserSetting.leftBottomEnd = double.Parse(leftBottomEnd.Text);

            LaserSetting.rightTopStart = double.Parse(rightTopStart.Text);
            LaserSetting.rightTopEnd = double.Parse(rightTopEnd.Text);

            LaserSetting.rightBottomStart = double.Parse(rightBottomStart.Text);
            LaserSetting.rightBottomEnd = double.Parse(rightBottomEnd.Text);

            LaserSetting.SaveLaserSetting();
        }
    }
}
