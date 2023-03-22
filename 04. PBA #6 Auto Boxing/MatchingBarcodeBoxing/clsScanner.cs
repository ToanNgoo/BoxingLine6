using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace MatchingBarcodeBoxing
{
    public class clsScanner
    {
        public event SerialDataReceivedEventHandler Datareceived;
        SerialPort Scanner;
        Form1 _frm;
        //clsdataconvert dataconvert;

        private string _data;

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private string _COMnum;

        public string COMnum
        {
            get { return _COMnum; }
            set { _COMnum = value; }
        }

        public clsScanner(Form1 frm)
        {
            _frm = frm;
            Scanner = new SerialPort();
        }

        public bool ketnoi(ToolStripLabel lb)
        {
            try
            {
                Scanner.PortName = _COMnum;
                Scanner.BaudRate = 9600;
                Scanner.DataBits = 8;
                Scanner.ReadBufferSize = 1024;
                Scanner.WriteBufferSize = 512;
                Scanner.Parity = Parity.None;
                Scanner.DtrEnable = true;
                Scanner.DataReceived += Scanner_DataReceived;
                Scanner.Open();
                Scanner.Write("LON\r\n");
                Scanner.Write("LOFF\r\n");
                //Scanner.ReadExisting();
                lb.BackColor = Color.Green;
                return true;
            }
            catch (Exception)
            {
                Scanner.Close();
                lb.BackColor = Color.Red;
                return false;
            }

        }

        public void ngatketnoi()
        {
            try
            {
                Scanner.Close();
            }
            catch (Exception)
            {

            }
        }

        private void Scanner_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(300);
            try
            {
                _data = "";
                //_frm.tb_barcode1.Text = "";
                _data = Scanner.ReadLine();
                _frm.tb_barcode1.Text = _data;

                Scanner.DiscardInBuffer();
                Scanner.DiscardOutBuffer();
                if (Datareceived != null)
                {
                    Datareceived(this, e);
                }               
             
            }
            catch (Exception)
            {
                //throw;
            }
        }

        
    }
}
