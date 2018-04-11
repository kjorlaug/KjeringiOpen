namespace TimerApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstOutbound = new System.Windows.Forms.ListBox();
            this.status = new System.Windows.Forms.StatusStrip();
            this.WorkerThread = new System.ComponentModel.BackgroundWorker();
            this.ctlReader = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.ctlBtn = new System.Windows.Forms.Button();
            this.lstInbound = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstOutbound
            // 
            this.lstOutbound.FormattingEnabled = true;
            this.lstOutbound.ItemHeight = 16;
            this.lstOutbound.Location = new System.Drawing.Point(33, 99);
            this.lstOutbound.Name = "lstOutbound";
            this.lstOutbound.Size = new System.Drawing.Size(548, 340);
            this.lstOutbound.TabIndex = 0;
            // 
            // status
            // 
            this.status.BackColor = System.Drawing.Color.Red;
            this.status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.status.Location = new System.Drawing.Point(0, 459);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1181, 22);
            this.status.TabIndex = 1;
            // 
            // ctlReader
            // 
            this.ctlReader.FormattingEnabled = true;
            this.ctlReader.ItemHeight = 16;
            this.ctlReader.Items.AddRange(new object[] {
            "Serialreader (live)",
            "Testreader"});
            this.ctlReader.Location = new System.Drawing.Point(33, 23);
            this.ctlReader.Name = "ctlReader";
            this.ctlReader.Size = new System.Drawing.Size(206, 20);
            this.ctlReader.TabIndex = 2;
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.ItemHeight = 16;
            this.listBox2.Items.AddRange(new object[] {
            "90 - Damefall",
            "91 - Kleppa",
            "92 - Barneskulen",
            "93 - Hamre",
            "248 - Mål"});
            this.listBox2.Location = new System.Drawing.Point(33, 49);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(206, 20);
            this.listBox2.TabIndex = 3;
            // 
            // ctlBtn
            // 
            this.ctlBtn.Location = new System.Drawing.Point(268, 46);
            this.ctlBtn.Name = "ctlBtn";
            this.ctlBtn.Size = new System.Drawing.Size(109, 23);
            this.ctlBtn.TabIndex = 4;
            this.ctlBtn.Text = "Start";
            this.ctlBtn.UseVisualStyleBackColor = true;
            this.ctlBtn.Click += new System.EventHandler(this.ctlBtn_Click);
            // 
            // lstInbound
            // 
            this.lstInbound.FormattingEnabled = true;
            this.lstInbound.ItemHeight = 16;
            this.lstInbound.Location = new System.Drawing.Point(597, 99);
            this.lstInbound.Name = "lstInbound";
            this.lstInbound.Size = new System.Drawing.Size(548, 340);
            this.lstInbound.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Data inn frå EQ timing";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(594, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Registrert online";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1181, 481);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstInbound);
            this.Controls.Add(this.ctlBtn);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.ctlReader);
            this.Controls.Add(this.status);
            this.Controls.Add(this.lstOutbound);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstOutbound;
        private System.Windows.Forms.StatusStrip status;
        private System.ComponentModel.BackgroundWorker WorkerThread;
        private System.Windows.Forms.ListBox ctlReader;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button ctlBtn;
        private System.Windows.Forms.ListBox lstInbound;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

