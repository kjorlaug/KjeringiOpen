using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EmitReaderLib.Model;

namespace EmitReaderLib
{
    public class UsbSerialReader : IEmitReader
    {
        private SerialPort Port { get; set; }
        private StringBuilder buffer = new StringBuilder();

        protected String ComPortNo { get; set; }

        public event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        public UsbSerialReader()
        {
            ComPortNo = ConfigurationManager.AppSettings["com"];
        }

        public void Start()
        {
            if (Port == null)
                Port = new SerialPort(ComPortNo, 115200);

            //Port.ReceivedBytesThreshold = 200;

            Port.DataReceived += ProcessDataReceived;

            if (!Port.IsOpen)
                Port.Open();
        }

        public void Stop()
        {
            if (Port.IsOpen)
                Port.Close();
        }

        protected void ProcessDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            String data = sp.ReadExisting();

            buffer.Append(data);

            // Only check if newline in data
            if (data.IndexOf('\n') < 0)
                return;

            Console.WriteLine(DateTime.Now.ToLongTimeString() + ": COM checking");
            String checkString = buffer.ToString();

            while (checkString.IndexOf('\n') > 0)
            {
                String indata = checkString.Substring(0, checkString.IndexOf('\n'));
                checkString = checkString.Substring(checkString.IndexOf('\n') + 1);
                buffer.Clear().Append(checkString);

                if (indata.IndexOf("M") >= 0 || indata.IndexOf("emiTag") >= 0)
                {
                    EmitData d = new EmitData();
                    foreach (String s in indata.Split('\t'))
                    {
                        if (s.Length > 0)
                        {
                            switch (s.Substring(0, 1))
                            {
                                case "N":
                                    d.Id = int.Parse(s.Substring(1));
                                    break;
                                case "W":
                                case "E":
                                    d.Time = DateTime.ParseExact(s.Substring(1), "HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "V":
                                    d.Voltage = double.Parse(s.Substring(1, 3));
                                    break;
                                case "C":
                                    d.BoxId = GetBoxId(s.Substring(1));
                                    break;
                                case "S":
                                    d.Chip = int.Parse(s.Substring(1));
                                    break;
                            }
                        }
                    }
                    EventHandler<EmitDataRecievedEventArgs> handler = DataReceived;

                    if (handler != null && d.Id > 0)
                        handler(this, new EmitDataRecievedEventArgs(d));
                }
            }
        }

        private static int GetBoxId(string boxIdFromDevice)
        {
            var boxIdSetting = ConfigurationManager.AppSettings["BoxId"];
            
            int parseResult;
            if (string.IsNullOrWhiteSpace(boxIdSetting) || !int.TryParse(boxIdSetting, out parseResult))
            {
                return int.Parse(boxIdFromDevice);
            }

            return int.Parse(boxIdSetting);
        }
    }
}
