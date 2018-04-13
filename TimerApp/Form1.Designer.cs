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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lstOutbound = new System.Windows.Forms.ListBox();
            this.status = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.ctlBtn = new System.Windows.Forms.Button();
            this.lstInbound = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ctlReader = new System.Windows.Forms.ComboBox();
            this.ctlBox = new System.Windows.Forms.ComboBox();
            this.ctlCom = new System.Windows.Forms.ComboBox();
            this.readWorker = new System.ComponentModel.BackgroundWorker();
            this.writeWorker = new System.ComponentModel.BackgroundWorker();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstOutbound
            // 
            this.lstOutbound.FormattingEnabled = true;
            this.lstOutbound.ItemHeight = 16;
            this.lstOutbound.Location = new System.Drawing.Point(33, 99);
            this.lstOutbound.Name = "lstOutbound";
            this.lstOutbound.Size = new System.Drawing.Size(286, 340);
            this.lstOutbound.TabIndex = 0;
            // 
            // status
            // 
            this.status.BackColor = System.Drawing.Color.Transparent;
            this.status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.toolStripSplitButton1});
            this.status.Location = new System.Drawing.Point(0, 455);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(670, 26);
            this.status.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 21);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.BackColor = System.Drawing.Color.Red;
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(39, 24);
            this.toolStripSplitButton1.Text = "statusButton";
            // 
            // ctlBtn
            // 
            this.ctlBtn.Location = new System.Drawing.Point(512, 20);
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
            this.lstInbound.Location = new System.Drawing.Point(335, 99);
            this.lstInbound.Name = "lstInbound";
            this.lstInbound.Size = new System.Drawing.Size(286, 340);
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
            this.label2.Location = new System.Drawing.Point(332, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "Registrert online";
            // 
            // ctlReader
            // 
            this.ctlReader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ctlReader.FormattingEnabled = true;
            this.ctlReader.Items.AddRange(new object[] {
            "Serialreader (live)",
            "Testreader"});
            this.ctlReader.Location = new System.Drawing.Point(33, 19);
            this.ctlReader.Name = "ctlReader";
            this.ctlReader.Size = new System.Drawing.Size(157, 24);
            this.ctlReader.TabIndex = 8;
            // 
            // ctlBox
            // 
            this.ctlBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ctlBox.FormattingEnabled = true;
            this.ctlBox.Items.AddRange(new object[] {
            "90 - Damefall",
            "91 - Kleppa",
            "92 - Barneskulen",
            "93 - Hamre",
            "248 - Mål"});
            this.ctlBox.Location = new System.Drawing.Point(215, 19);
            this.ctlBox.Name = "ctlBox";
            this.ctlBox.Size = new System.Drawing.Size(121, 24);
            this.ctlBox.TabIndex = 9;
            // 
            // ctlCom
            // 
            this.ctlCom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ctlCom.FormattingEnabled = true;
            this.ctlCom.Location = new System.Drawing.Point(359, 20);
            this.ctlCom.Name = "ctlCom";
            this.ctlCom.Size = new System.Drawing.Size(125, 24);
            this.ctlCom.TabIndex = 10;
            // 
            // writeWorker
            // 
            this.writeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.writeWorker_DoWork);
            this.writeWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.writeWorker_RunWorkerCompleted);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 481);
            this.Controls.Add(this.ctlCom);
            this.Controls.Add(this.ctlBox);
            this.Controls.Add(this.ctlReader);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstInbound);
            this.Controls.Add(this.ctlBtn);
            this.Controls.Add(this.status);
            this.Controls.Add(this.lstOutbound);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstOutbound;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.Button ctlBtn;
        private System.Windows.Forms.ListBox lstInbound;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ComboBox ctlReader;
        private System.Windows.Forms.ComboBox ctlBox;
        private System.Windows.Forms.ComboBox ctlCom;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.ComponentModel.BackgroundWorker readWorker;
        private System.ComponentModel.BackgroundWorker writeWorker;
    }
}

