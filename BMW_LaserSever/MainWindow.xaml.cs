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

namespace LaserServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, TUIO.TuioListener
    {
        /// <summary>
        /// NLog for current system
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Track whether current scrren is full mode
        /// </summary>
        private Boolean isFullScreenMode = false;

        /// <summary>
        /// record current window position
        /// </summary>
        private Point currentPosition = new Point();

        /// <summary>
        /// TUIO Client,Receive TUIO Message on port 3333 with UDP Protocol
        /// </summary>
        private TUIO.TuioClient tuioClient = null;

        private System.Windows.Forms.NotifyIcon icon;

        /// <summary>
        /// the width and height of display
        /// </summary>
        int displayWidth = 0;
        int displayHeight = 0;

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

            // Start TuioHandler
            StartTuioHandler();

            // load basic configuration
            LaserSetting.InitLaserSetting();

            // handle Closing, KeyDown and StateChanged for MainWindow 
            this.Closing += OnMainWindowClosingHandler;
            this.KeyDown += OnMainWindowKeyDownHandler;
            this.StateChanged += OnMainWindowStateChangedHandler;
            this.MouseDown += OnMainWindowMouseDownHandler;

            // auto start
            btnStartClickedHandler(null, null);
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
        private void OnMainWindowMouseDownHandler(object sender, EventArgs args)
        {
            this.Focus();
        }

        /// <summary>
        /// Icon Clicked Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnIconClickedHandler(object sender, EventArgs args)
        {
            if(WindowState.Normal != this.WindowState)
                this.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Closing current application and handle all need to be returned things
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMainWindowClosingHandler(object sender, CancelEventArgs args)
        {
            // unload BSQSim Library
            TuioHandler.UnLoadSystem();

            // close the dispatchertimer
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

            // Close Tuio Client
            if (null != tuioClient)
            {
                tuioClient.disconnect();
                tuioClient = null;
            }

            // save data
            LaserSetting.SaveLaserSetting();

            logger.Info("Closed Laser Server");
            logger.Info("*************************");
            logger.Info("\n");
        }

        /// <summary>
        /// changed current mode for application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMainWindowKeyDownHandler(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Space:
                    {
                        // chaged screen state
                        SetScreenMode();

                        break;
                    }
                case Key.Escape:
                    {
                        // Close current application
                        this.Close();

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Set the icon visible or show in taskbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMainWindowStateChangedHandler(object sender, EventArgs args)
        {
            if (WindowState.Minimized == this.WindowState)
            {
                this.ShowInTaskbar = false;

                if (null != this.icon)
                {
                    this.icon.ShowBalloonTip(500);
                    this.icon.Visible = true;
                }
            }
            else if(WindowState.Normal == this.WindowState)
            {
                this.ShowInTaskbar = true;
                this.icon.Visible = false;
            }
        }

        /// <summary>
        /// Set screen mode
        /// </summary>
        private void SetScreenMode()
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            if (isFullScreenMode)
            {
                // set the SET MODE

                // set windows position
                this.Left = currentPosition.X;
                this.Top = currentPosition.Y;

                // set windows visibility
                this.setModePanel.Visibility = System.Windows.Visibility.Visible;
                this.debugModePanel.Visibility = System.Windows.Visibility.Hidden;

                // reset state of screen
                isFullScreenMode = false;
            }
            else
            {
                // record current window position
                currentPosition.X = this.Left;
                currentPosition.Y = this.Top;

                // set the DEBUG MODE

                // set windows position
                this.Left = 0.0f;
                this.Top = 0.0f;

                // set windows size
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                this.MainPanel.Width = this.debugModePanel.Width = this.Width;
                this.MainPanel.Height = this.debugModePanel.Height = this.Height;

                // set windows visibility
                setModePanel.Visibility = System.Windows.Visibility.Hidden;
                debugModePanel.Visibility = System.Windows.Visibility.Visible;

                // reset state of screen
                isFullScreenMode = true;

                // Track Whether it is calibration
                if (true == chkCalibration.IsChecked)
                {
                    logger.Fatal("Begion Calibration #####################################");
                    LaserSetting.IsCalibration = true;
                }
            }
        }

        /// <summary>
        /// Minimum
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void btnMinModeClickedHandler(object sender, RoutedEventArgs args)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// Close Application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void btnClosedClickedHandler(object sender, RoutedEventArgs args)
        {
            // close the dispatchertimer
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

            // Close Tuio Client
            if (null != tuioClient)
            {
                tuioClient.disconnect();
                tuioClient = null;
            }

            // save data
            LaserSetting.SaveLaserSetting();

            // unload
            TuioHandler.UnLoadSystem();

            this.Close();
        }

        /// <summary>
        /// Start Laser Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void btnStartClickedHandler(object sender, RoutedEventArgs args)
        {
            // Start Laser Data Receiver
            if (!LaserDataReceiver.Instance.InitReceiver())
            {
                MessageBox.Show(@"Error : Can not connect to Laser Measurement System! Please Check \n
                                 if THE LMS is started and connected to your Computer!!!");

                return;
            }

            // Start Timer
            StartDispatherTimer();

            // Start TuioServer
            StartTuioServer();

            // Start TuioClient
            StartTuioClient();
        }

        #endregion // Main Window Events


        #region Tuio Server, Client and Handler
        private void StartTuioServer()
        {
            try
            {
                LaserDataHandler.Instance.InitTuioServer();
            }
            catch (Exception expt)
            {
                logger.Fatal(expt.ToString());
            }
        }

        /// <summary>
        /// Start Tuio Client and receive data
        /// </summary>
        private void StartTuioClient()
        {
            if (tuioClient == null)
            {
                try
                {
                    tuioClient = new TUIO.TuioClient(int.Parse(txtPort.Text));
                    tuioClient.addTuioListener(this);
                    tuioClient.connect();

                    btnStart.Content = "Working.....";
                    lblContent.Content = "Laser Server has working !";
                    
                    logger.Info("Laser Server has working!");

                }
                catch (Exception expt)
                {
                    lblContent.Content = "Can't connect to TUIO host!\nPlease change another port !";
                    tuioClient = null;

                    logger.Info("Laser Server can't connect to TUIO host!" + expt.ToString());
                }
            }
            else
            {
                tuioClient.disconnect();
                tuioClient = null;

                lblContent.Content = "Laser Server has stop working !";

                btnStart.Content = "Start Laser Server";

                logger.Info("Laser Server has stop working!");
            }
        }

        /// <summary>
        /// Start TuioHandler to translate TUIO to WM_Touch
        /// </summary>
        private void StartTuioHandler()
        {
            if (TuioHandler.InitTuioHandler())
            {
                // set touch window size
                this.Width = displayWidth = TuioHandler.SimRange.Width;
                this.Height = displayHeight = TuioHandler.SimRange.Height;

                // handle events for sfInkCanvas
                sfInkCanvas.PreviewTouchUp += (events, args) => { sfInkCanvas.Strokes.Clear(); };
                sfInkCanvas.PreviewMouseLeftButtonUp += (events, args) => { sfInkCanvas.Strokes.Clear(); };

                //// handle Closing, KeyDown and StateChanged for MainWindow 
                //this.Closing += OnMainWindowClosingHandler;
                //this.KeyDown += OnMainWindowKeyDownHandler;
                //this.StateChanged += OnMainWindowStateChangedHandler;
                //this.MouseDown += OnMainWindowMouseDownHandler;

                logger.Info("Event for Laser Server : Done");

                // create the icon object
                if (null == this.icon)
                {
                    try
                    {
                        this.icon = new System.Windows.Forms.NotifyIcon();
                        this.icon.BalloonTipText = "Minimized... Click tray icon to show";
                        this.icon.BalloonTipTitle = "Laser Server";
                        this.icon.Icon = new System.Drawing.Icon("LaserServer.ico");
                        this.icon.Text = "Laser Server";
                        this.icon.Click += new EventHandler(OnIconClickedHandler);
                    }
                    catch (Exception expt)
                    {
                        logger.Fatal(expt.ToString());
                    }
                }

                logger.Info("Icontray Init : Done");
                logger.Info("Laser Server Resolution : " + displayWidth + " X " + displayHeight);
            }
            else
            {
                // init failure
                logger.Info("Error : Can't start Laser Server!");
                logger.Info("***********************************");
                logger.Info("\n");

                // Show Error Message
                MessageBox.Show("Can't start laser Server! Please try again, It need Administrator Acount!");

                // Close Application
                this.Close();
            }
        }

        #region Implement the TuioListener Interface

        public void addTuioObject(TUIO.TuioObject tuioObject) { }
        public void updateTuioObject(TUIO.TuioObject tuioObject) { }
        public void removeTuioObject(TUIO.TuioObject tuioObject) { }
        public void refresh(TUIO.TuioTime timestamp) { }

        public void addTuioCursor(TUIO.TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    TuioHandler.AddTouchDevice((int)tuioCursor.getSessionID(),
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(displayWidth)), 
                                  tuioCursor.getScreenY(Convert.ToInt32(displayHeight))));
                }));
        }

        public void updateTuioCursor(TUIO.TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    TuioHandler.UpdateTouchDevice((int)tuioCursor.getSessionID(),
                        new Point(tuioCursor.getScreenX(Convert.ToInt32(displayWidth)), 
                                  tuioCursor.getScreenY(Convert.ToInt32(displayHeight))));
                }));
        }

        public void removeTuioCursor(TUIO.TuioCursor tuioCursor)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                (Action)(() =>
                {
                    TuioHandler.RemoveTouchDevice((int)tuioCursor.getSessionID());
                }));
        }

        #endregion // Implement the TuioListener Interface

        #endregion // Tuio Server, Client and Handler


        #region for Debug
        public void SetDebugShow()
        {
            if (!LaserSetting.IsCalibration)
            {
                Canvas.SetLeft(TouchPoint, LaserDataHandler.Instance.xPixelPos);
                Canvas.SetTop(TouchPoint, LaserDataHandler.Instance.yPixelPos);

                Pos.Text = "Position(" + LaserDataHandler.Instance.xPixelPos +
                           ",  " + LaserDataHandler.Instance.yPixelPos + ")";
            }
            else
            {
                Canvas.SetLeft(TouchPoint, 1500);
                Canvas.SetTop(TouchPoint, 180);
            }
        }
        #endregion // for Debug

        #region Slider Events
        private void sfMaxWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Max Width
            LaserSetting.maxWidth += (e.NewValue - e.OldValue) / 10;
            logger.Info("LaserSetting.maxWidth = " + LaserSetting.maxWidth);

            //LaserSetting.SaveLaserSetting();
        }

        private void sfMinWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Min Width
            LaserSetting.minWidth += (e.NewValue - e.OldValue) / 10;
            logger.Info("LaserSetting.minWidth = " + LaserSetting.minWidth);
        }

        private void sfMaxHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Max Height
            LaserSetting.maxHeight += (e.NewValue - e.OldValue) / 10;
            logger.Info("LaserSetting.maxHeight = " + LaserSetting.maxHeight);
        }

        private void sfMinHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Min Height
            LaserSetting.minHeight += (e.NewValue - e.OldValue) / 10;
            logger.Info("LaserSetting.minHeight = " + LaserSetting.minHeight);
        }

        private void sfAngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Angle
            LaserSetting.angluarCorrection += (e.NewValue - e.OldValue) / 10;
            logger.Info("LaserSetting.angluarCorrection = " + LaserSetting.angluarCorrection);
        }

        #endregion // Slider Events


    }
}
