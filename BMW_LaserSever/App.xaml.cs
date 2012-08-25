using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using System.Windows.Threading;      // for

namespace LaserServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// NLog
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private void App_DispatcherUnhandledExceptionEventHandler(object sender, DispatcherUnhandledExceptionEventArgs arg)
        {
            logger.Fatal(arg.ToString() + " --> " + arg.Exception.ToString());
        }
    }
}
