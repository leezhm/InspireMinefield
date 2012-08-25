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

            LaserDataReceiver.Instance.InitReceiver();

            StartDispatherTimer();   
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
    }
}
