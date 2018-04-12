using EmitReaderLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;

namespace TimerApp
{
    public partial class Form1 : Form
    {
        delegate void StringArgReturningVoidDelegate(string text);
        delegate void ColorArgReturningVoidDelegate(Color c);

        public Form1()
        {
            InitializeComponent();
        }

        protected IEmitReader EmitReader;
        protected IWorker EmitWorker;

        private void SetStatusText(string text)
        {
            if (this.status.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetStatusText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblStatus.Text = text;
            }
        }
        private void SetStatusColor(Color c)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.status.InvokeRequired)
            {
                ColorArgReturningVoidDelegate d = new ColorArgReturningVoidDelegate(SetStatusColor);
                this.Invoke(d, new object[] { c });
            }
            else
            {
                this.status.BackColor = c;
            }
        }
        private void AddInboundItem(string text)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.lstInbound.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddInboundItem);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lstInbound.Items.Insert(0, text);
            }
        }
        private void AddOutboundItem(string text)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.lstOutbound.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddOutboundItem);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lstOutbound.Items.Insert(0, text);
            }
        }

        private void EmitWorker_StatusChange(object sender, KeyValuePair<Color, string> e)
        {
            SetStatusText(e.Value);
            SetStatusColor(e.Key);
        }

        private void Reader_DataReceived(object sender, EmitDataRecievedEventArgs e)
        {
            AddOutboundItem(e.Data.Id.ToString() + " " + e.Data.Time.ToLongTimeString());
            EmitWorker.ProcessData(e.Data);
        }

        private void EmitWorker_LogEntry(object sender, LogEventArgs e)
        {
            AddInboundItem(e.Data);
        }

        private void ToggleGui(Boolean enabled)
        {
            ctlBtn.Enabled = enabled;
            ctlReader.Enabled = enabled;
            ctlBox.Enabled = enabled;
            ctlCom.Enabled = enabled;
        }

        private void ctlBtn_Click(object sender, EventArgs e)
        {
            ToggleGui(false);

            // Selected boxId and name
            String boxId = ctlBox.SelectedItem.ToString().Split('-')[0].Trim();
            String boxName = ctlBox.SelectedItem.ToString().Split('-')[1].Trim();

            EmitWorker = new SignalWorker()
            {
                BoxId = boxId,
                Name = boxName,
                Hub = ConfigurationManager.AppSettings["hub"]
            };

            // Live or test reader?
            if (ctlReader.SelectedIndex == 1)
                EmitReader = new TestReader(new List<int>() { 4481 }, new List<int>() { int.Parse(boxId) });
            else
            {
                EmitReader = new UsbSerialReader() {
                    BoxId = boxId,
                    Port = ctlCom.SelectedIndex >= 0 ? ctlCom.SelectedItem.ToString() : ConfigurationManager.AppSettings["com"]
                };
            }

            EmitReader.DataReceived += Reader_DataReceived;

            EmitWorker.StatusChange += EmitWorker_StatusChange;
            EmitWorker.LogEntry += EmitWorker_LogEntry;


            Thread thread = new Thread(new ThreadStart(EmitWorker.StartWork));
            thread.Start();

            EmitReader.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ctlReader.SelectedIndex = 0;
            ctlBox.SelectedIndex = 0;

            // Iterate and populate with serial ports
            foreach (String sp in SerialPort.GetPortNames())
                ctlCom.Items.Add(sp);

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["com"]) && ctlCom.Items.IndexOf(ConfigurationManager.AppSettings["com"]) >= 0)
                ctlCom.SelectedIndex = ctlCom.Items.IndexOf(ConfigurationManager.AppSettings["com"]);
            else if (ctlCom.Items.Count > 0)
                ctlCom.SelectedIndex = 0;
        }
    }
}
