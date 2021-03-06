﻿using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace ShieldBox
{
    public class BpShieldBox
    {
        private readonly SerialPort _serial = null;
        private readonly object _sendLock = new object();
        private const string CmdEnding = "\r";
        private string _response;
        private int _id;

        public BpShieldBox(int id, string serialPortName, int serialBaudRate = 9600,
            Parity serialParity = Parity.None, int serialDataBit = 8,
            StopBits serialStopBits = StopBits.One)
        {
            _serial = new SerialPort(serialPortName, serialBaudRate, serialParity,
                    serialDataBit, serialStopBits)
                { ReadTimeout = 1000};
            _id = id;
        }

        public void Start()
        {
            _serial.Open();
            _serial.DataReceived += _serial_DataReceived;
        }

        private void _serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _response += _serial.ReadExisting();
        }

        public void Stop()
        {
            _serial.Close();
            _serial.Dispose();
        }

        public void SendCmd(Command command)
        {
            lock (_sendLock)
            {
                string cmd = command + CmdEnding;
                _serial.Write(cmd);
            }            
        }

        private void Delay(int milliSecond)
        {
            Thread.Sleep(milliSecond);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">Takes less than 3 sec to open</param>
        public void OpenBox(int timeout = 10000)
        {
            SendCmd(Command.OPEN);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != Response.OpenSuccessful)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    //_response = String.Empty;
                    throw new Exception("OpenBox " + _id + " timeout");
                }
                Delay(100);
            }

            long sec = stopwatch.ElapsedMilliseconds;
            _response = String.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"> Takes less than 3 sec close</param>
        public void CloseBox(int timeout = 10000)
        {
            SendCmd(Command.CLOSE);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_response != Response.CloseSuccessful)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    //_response = String.Empty;
                    throw new Exception("CloseBox " + _id + " timeout");
                }
                Delay(100);
            }
            long sec = stopwatch.ElapsedMilliseconds;
            _response = String.Empty;
        }

        public Task<bool> CloseBoxAsync()
        {
            return Task.Factory.StartNew(() => true);
        }

        public Task<bool> OpenBoxAsync()
        {
            return Task.Factory.StartNew(() => true);
        }
    }
}
