namespace MatchingBarcodeBoxing
{
    partial class History
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbb_shift = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbb_line = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_exe = new System.Windows.Forms.Button();
            this.cbb_model = new System.Windows.Forms.ComboBox();
            this.dt_boxing = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txt_total = new System.Windows.Forms.TextBox();
            this.dgv_boxing = new System.Windows.Forms.DataGridView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgv_Show = new System.Windows.Forms.DataGridView();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbb_Model_1 = new System.Windows.Forms.ComboBox();
            this.btn_Export = new System.Windows.Forms.Button();
            this.tb_CodePCM = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_boxing)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Show)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbb_shift);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbb_line);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btn_exe);
            this.groupBox1.Controls.Add(this.cbb_model);
            this.groupBox1.Controls.Add(this.dt_boxing);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1213, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Nhập thông tin";
            // 
            // cbb_shift
            // 
            this.cbb_shift.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_shift.FormattingEnabled = true;
            this.cbb_shift.Location = new System.Drawing.Point(283, 30);
            this.cbb_shift.Name = "cbb_shift";
            this.cbb_shift.Size = new System.Drawing.Size(96, 24);
            this.cbb_shift.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(238, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 16);
            this.label5.TabIndex = 7;
            this.label5.Text = "Shift :";
            // 
            // cbb_line
            // 
            this.cbb_line.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_line.FormattingEnabled = true;
            this.cbb_line.Location = new System.Drawing.Point(453, 29);
            this.cbb_line.Name = "cbb_line";
            this.cbb_line.Size = new System.Drawing.Size(96, 24);
            this.cbb_line.TabIndex = 6;
            this.cbb_line.TextChanged += new System.EventHandler(this.cbb_line_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(408, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Line :";
            // 
            // btn_exe
            // 
            this.btn_exe.BackColor = System.Drawing.Color.PaleGreen;
            this.btn_exe.Location = new System.Drawing.Point(829, 21);
            this.btn_exe.Name = "btn_exe";
            this.btn_exe.Size = new System.Drawing.Size(89, 37);
            this.btn_exe.TabIndex = 4;
            this.btn_exe.Text = "EXECUTE";
            this.btn_exe.UseVisualStyleBackColor = false;
            this.btn_exe.Click += new System.EventHandler(this.btn_exe_Click);
            // 
            // cbb_model
            // 
            this.cbb_model.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_model.FormattingEnabled = true;
            this.cbb_model.Location = new System.Drawing.Point(632, 27);
            this.cbb_model.Name = "cbb_model";
            this.cbb_model.Size = new System.Drawing.Size(170, 24);
            this.cbb_model.TabIndex = 3;
            // 
            // dt_boxing
            // 
            this.dt_boxing.CustomFormat = "MM-dd-yyyy";
            this.dt_boxing.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dt_boxing.Location = new System.Drawing.Point(107, 31);
            this.dt_boxing.Name = "dt_boxing";
            this.dt_boxing.Size = new System.Drawing.Size(110, 22);
            this.dt_boxing.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(574, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Model :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Date Boxing :";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.dgv_boxing);
            this.groupBox2.Location = new System.Drawing.Point(6, 82);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1213, 535);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Hiển thị thông tin";
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.groupBox3.Controls.Add(this.txt_total);
            this.groupBox3.Location = new System.Drawing.Point(1119, 21);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(83, 62);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tổng";
            // 
            // txt_total
            // 
            this.txt_total.BackColor = System.Drawing.Color.White;
            this.txt_total.Location = new System.Drawing.Point(12, 26);
            this.txt_total.Name = "txt_total";
            this.txt_total.ReadOnly = true;
            this.txt_total.Size = new System.Drawing.Size(60, 22);
            this.txt_total.TabIndex = 1;
            // 
            // dgv_boxing
            // 
            this.dgv_boxing.AllowUserToAddRows = false;
            this.dgv_boxing.AllowUserToDeleteRows = false;
            this.dgv_boxing.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgv_boxing.BackgroundColor = System.Drawing.SystemColors.Info;
            this.dgv_boxing.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_boxing.Location = new System.Drawing.Point(7, 17);
            this.dgv_boxing.Name = "dgv_boxing";
            this.dgv_boxing.ReadOnly = true;
            this.dgv_boxing.Size = new System.Drawing.Size(1200, 512);
            this.dgv_boxing.TabIndex = 0;
            this.dgv_boxing.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_boxing_CellFormatting);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1233, 652);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1225, 623);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "P1101";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox5);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1225, 623);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "P1102";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgv_Show
            // 
            this.dgv_Show.BackgroundColor = System.Drawing.SystemColors.Info;
            this.dgv_Show.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Show.Location = new System.Drawing.Point(7, 17);
            this.dgv_Show.Name = "dgv_Show";
            this.dgv_Show.RowTemplate.Height = 23;
            this.dgv_Show.Size = new System.Drawing.Size(1200, 512);
            this.dgv_Show.TabIndex = 4;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.cbb_Model_1);
            this.groupBox4.Controls.Add(this.btn_Export);
            this.groupBox4.Controls.Add(this.tb_CodePCM);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1213, 70);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Nhập thông tin";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 16);
            this.label7.TabIndex = 4;
            this.label7.Text = "Model";
            // 
            // cbb_Model_1
            // 
            this.cbb_Model_1.FormattingEnabled = true;
            this.cbb_Model_1.Location = new System.Drawing.Point(76, 31);
            this.cbb_Model_1.Name = "cbb_Model_1";
            this.cbb_Model_1.Size = new System.Drawing.Size(170, 24);
            this.cbb_Model_1.TabIndex = 3;
            // 
            // btn_Export
            // 
            this.btn_Export.Location = new System.Drawing.Point(577, 15);
            this.btn_Export.Name = "btn_Export";
            this.btn_Export.Size = new System.Drawing.Size(115, 46);
            this.btn_Export.TabIndex = 1;
            this.btn_Export.Text = "Export Data";
            this.btn_Export.UseVisualStyleBackColor = true;
            this.btn_Export.Click += new System.EventHandler(this.btn_Export_Click);
            // 
            // tb_CodePCM
            // 
            this.tb_CodePCM.Location = new System.Drawing.Point(382, 31);
            this.tb_CodePCM.Name = "tb_CodePCM";
            this.tb_CodePCM.Size = new System.Drawing.Size(170, 22);
            this.tb_CodePCM.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(294, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 16);
            this.label6.TabIndex = 2;
            this.label6.Text = "Code PCM";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.dgv_Show);
            this.groupBox5.Location = new System.Drawing.Point(6, 82);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1213, 535);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Hiển thị thông tin";
            // 
            // History
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1241, 654);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "History";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "History";
            this.Load += new System.EventHandler(this.History_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_boxing)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Show)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbb_model;
        private System.Windows.Forms.DateTimePicker dt_boxing;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_exe;
        private System.Windows.Forms.DataGridView dgv_boxing;
        private System.Windows.Forms.ComboBox cbb_line;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txt_total;
        private System.Windows.Forms.ComboBox cbb_shift;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btn_Export;
        private System.Windows.Forms.TextBox tb_CodePCM;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbb_Model_1;
        private System.Windows.Forms.DataGridView dgv_Show;
        private System.Windows.Forms.GroupBox groupBox5;
    }
}