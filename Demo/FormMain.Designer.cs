namespace Demo
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboPort = new System.Windows.Forms.ComboBox();
            this.checkHex = new System.Windows.Forms.CheckBox();
            this.checkWordWrap = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxRaw = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listViewData = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.comboBaudrate = new System.Windows.Forms.ComboBox();
            this.buttonConnect = new Demo.NoFocusedButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Port";
            // 
            // comboPort
            // 
            this.comboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboPort.FormattingEnabled = true;
            this.comboPort.Location = new System.Drawing.Point(54, 17);
            this.comboPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.comboPort.Name = "comboPort";
            this.comboPort.Size = new System.Drawing.Size(102, 25);
            this.comboPort.TabIndex = 1;
            // 
            // checkHex
            // 
            this.checkHex.AutoSize = true;
            this.checkHex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkHex.Location = new System.Drawing.Point(113, 52);
            this.checkHex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.checkHex.Name = "checkHex";
            this.checkHex.Size = new System.Drawing.Size(122, 21);
            this.checkHex.TabIndex = 5;
            this.checkHex.Text = "Hex for raw data";
            this.checkHex.UseVisualStyleBackColor = true;
            // 
            // checkWordWrap
            // 
            this.checkWordWrap.AutoSize = true;
            this.checkWordWrap.Checked = true;
            this.checkWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkWordWrap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkWordWrap.Location = new System.Drawing.Point(250, 52);
            this.checkWordWrap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.checkWordWrap.Name = "checkWordWrap";
            this.checkWordWrap.Size = new System.Drawing.Size(89, 21);
            this.checkWordWrap.TabIndex = 6;
            this.checkWordWrap.Text = "WordWrap";
            this.checkWordWrap.UseVisualStyleBackColor = true;
            this.checkWordWrap.CheckedChanged += new System.EventHandler(this.CheckWordWrap_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBoxRaw);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox1.Location = new System.Drawing.Point(7, 54);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(566, 203);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Raw data view";
            // 
            // textBoxRaw
            // 
            this.textBoxRaw.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRaw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxRaw.Location = new System.Drawing.Point(7, 30);
            this.textBoxRaw.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBoxRaw.Multiline = true;
            this.textBoxRaw.Name = "textBoxRaw";
            this.textBoxRaw.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxRaw.Size = new System.Drawing.Size(552, 163);
            this.textBoxRaw.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.listViewData);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.groupBox2.Location = new System.Drawing.Point(7, 265);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(566, 214);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Analyze result event log";
            // 
            // listViewData
            // 
            this.listViewData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewData.FullRowSelect = true;
            this.listViewData.GridLines = true;
            this.listViewData.HideSelection = false;
            this.listViewData.Location = new System.Drawing.Point(7, 28);
            this.listViewData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listViewData.MultiSelect = false;
            this.listViewData.Name = "listViewData";
            this.listViewData.Size = new System.Drawing.Size(552, 176);
            this.listViewData.TabIndex = 0;
            this.listViewData.UseCompatibleStateImageBehavior = false;
            this.listViewData.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Protocol";
            this.columnHeader1.Width = 135;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Update Reason";
            this.columnHeader3.Width = 200;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Event Time";
            this.columnHeader4.Width = 100;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(163, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Baudrate";
            // 
            // comboBaudrate
            // 
            this.comboBaudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBaudrate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBaudrate.FormattingEnabled = true;
            this.comboBaudrate.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "115200"});
            this.comboBaudrate.Location = new System.Drawing.Point(232, 17);
            this.comboBaudrate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.comboBaudrate.Name = "comboBaudrate";
            this.comboBaudrate.Size = new System.Drawing.Size(140, 25);
            this.comboBaudrate.TabIndex = 2;
            // 
            // buttonConnect
            // 
            this.buttonConnect.BackColor = System.Drawing.SystemColors.Control;
            this.buttonConnect.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.buttonConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Khaki;
            this.buttonConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConnect.ForeColor = System.Drawing.Color.Blue;
            this.buttonConnect.Location = new System.Drawing.Point(380, 11);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(202, 40);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Tag = "OffLine";
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = false;
            this.buttonConnect.Click += new System.EventHandler(this.ButtonConnect_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 496);
            this.Controls.Add(this.comboBaudrate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.checkWordWrap);
            this.Controls.Add(this.checkHex);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.comboPort);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(603, 511);
            this.Name = "Form1";
            this.Text = "KoboldCom Library Demo Usage";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboPort;
        private System.Windows.Forms.CheckBox checkHex;
        private System.Windows.Forms.CheckBox checkWordWrap;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxRaw;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView listViewData;
        private NoFocusedButton buttonConnect;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBaudrate;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}

