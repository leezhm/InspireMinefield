using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//
// Receive data from laser and Handle it
//
// Written by Leezhm(at)126.com, 1st September, 2011.
//
// Contact : Leezhm(at)126.com
//
// All Right Reserved. Copyright (at) Leezhm(at)126.com.
//
// Last modified by Leezhm(at)126.com on 27th September, 2011.
//

using System.Net;
using System.Net.Sockets;

using System.Windows;

namespace BMW_LaserSever
{
    public sealed class LaserDataReceiver
    {
        /// <summary>
        /// NLog
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Singleton class
        /// </summary>
        class Nested
        {
            internal static readonly LaserDataReceiver instance = new LaserDataReceiver();
        }

        // current client
        private TcpClient dataClient = null;
        private NetworkStream dataStream = null;

        // Track whether current client begain to receive data
        private Boolean isStartReceived = false;

        /// <summary>
        /// internal constructor
        /// </summary>
        LaserDataReceiver()
        {

        }

        /// <summary>
        /// obtain the singleton object
        /// </summary>
        public static LaserDataReceiver Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        /// <summary>
        /// Init Receiver (tcp connected and so on)
        /// </summary>
        public bool InitReceiver()
        {
            // Connect to a remote device.
            try
            {
                // Create a TCP/IP socket.
                // set up tcp connected
                if (null == dataClient)
                {
                    dataClient = new TcpClient(LaserSetting.ipAddress, (int)LaserSetting.port);
                }
            }
            catch (Exception expt)
            {
                logger.Fatal(expt.ToString());

                return false;
            }

            return true;
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
            if (null != dataClient)
            {
                // closing
                dataClient.Close();

                dataClient = null;
            }

            isStartReceived = false;
        }

        /// <summary>
        /// Receive data and handle it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void StartReceivedData(object sender, EventArgs args)
        {
            if (isStartReceived)
            {
                // Receive the TcpServer.response.
                try
                {
                    // Buffer to store the response bytes.
                    Byte[] data = new Byte[6000];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = dataStream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    LaserDataHandler.Instance.HandleMeasuredData(ref responseData);
                }
                catch (Exception expt)
                {
                    logger.Fatal(expt.ToString());
                }

                isStartReceived = false;
            }
            else
            {
                string cmd = "sRN LMDscandata";
                SendCommand(cmd);
            }
        }

        #region Commands for Laser Scan
        /// <summary>
        /// Send original command to Laser Scan
        /// </summary>
        /// <param name="originalCmd">without begin and end char</param>
        private void SendCommand(string originalCmd)
        {
            if (string.Empty == originalCmd)
                return;

            char stx = (char)0x02;
            char etx = (char)0x03;

            // with stx and etx
            string command = stx + originalCmd + etx;

            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(command);

                // Get a client stream for reading and writing.
                if (null == dataStream)
                    dataStream = dataClient.GetStream();

                // Send the message to the connected TcpServer. 
                dataStream.Write(data, 0, data.Length);
            }
            catch (Exception expt)
            {
                logger.Fatal(expt.ToString());
            }

            // set markbit true
            isStartReceived = true;
        }
        #endregion // Commands for Laser Scan
    }
}
