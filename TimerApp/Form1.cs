using EmitReaderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

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
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.status.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetStatusText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.status.Text = text;
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

        private void ctlBtn_Click(object sender, EventArgs e)
        {
            EmitReader = new TestReader(new List<int>() { 4481 }, new List<int>() { 90, 91, 92, 93, 248 });
            EmitWorker = new SignalWorker()
            {
                BoxId = "90",
                Name = "",
                Hub = ConfigurationManager.AppSettings["hub"]
            };

            EmitReader.DataReceived += Reader_DataReceived;
            EmitReader.Port = ConfigurationManager.AppSettings["com"];
            EmitReader.BoxId = "90";

            EmitWorker.StatusChange += EmitWorker_StatusChange;
            EmitWorker.LogEntry += EmitWorker_LogEntry;

            Thread thread = new Thread(new ThreadStart(EmitWorker.StartWork));
            thread.Start();

            EmitReader.Start();
        }
    }
}
