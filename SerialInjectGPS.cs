﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MissionPlanner.Comms;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;

namespace MissionPlanner
{
    public partial class SerialInjectGPS : Form
    {
        private static readonly ILog log =
        LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal static SerialPort comPort = new SerialPort();
        private System.Threading.Thread t12;
        private static bool threadrun = false;

        public SerialInjectGPS()
        {
            InitializeComponent();

            CMB_serialport.Items.AddRange(SerialPort.GetPortNames());

            if (threadrun)
            {
                BUT_connect.Text = Strings.Stop;
            }

            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
        }

        private void BUT_connect_Click(object sender, EventArgs e)
        {
            if (comPort.IsOpen)
            {
                threadrun = false;
                comPort.Close();
                BUT_connect.Text = Strings.Connect;
            }
            else
            {
                try
                {
                    comPort.PortName = CMB_serialport.Text;
                }
                catch
                {
                    CustomMessageBox.Show(Strings.InvalidPortName);
                    return;
                }
                try
                {
                    comPort.BaudRate = int.Parse(CMB_baudrate.Text);
                }
                catch
                {
                    CustomMessageBox.Show(Strings.InvalidBaudRate);
                    return;
                }
                try
                {
                    comPort.Open();
                }
                catch
                {
                    CustomMessageBox.Show("Error Connecting\nif using com0com please rename the ports to COM??");
                    return;
                }

                t12 = new System.Threading.Thread(new System.Threading.ThreadStart(mainloop))
                {
                    IsBackground = true,
                    Name = "Nmea output"
                };
                t12.Start();
            }
        }

        private void mainloop()
        {
            threadrun = true;
            int counter = 0;
            while (threadrun)
            {
                try
                {
                    // limit to 128 byte packets
                    byte[] buffer = new byte[128];

                    while (comPort.BytesToRead > 0)
                    {
                        int read = comPort.Read(buffer, 0, Math.Min(buffer.Length, comPort.BytesToRead));

                        MainV2.comPort.InjectGpsData(buffer, (byte)read);
                    }

                    System.Threading.Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }
    }
}