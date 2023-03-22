using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;

namespace MatchingBarcodeBoxing
{
    public partial class History : Form
    {
        
        database dtb = new database();
        ClsExcel Excel = new ClsExcel();
        OptionDefine.clsCheckTrungInformation check = new OptionDefine.clsCheckTrungInformation();
        List<string> List = new List<string>();
        List<string> List_temp = new List<string>();
        public History()
        {
            InitializeComponent();
        }

        private void History_Load(object sender, EventArgs e)
        {
            cbb_line.Items.Add("6");
            cbb_shift.Items.Add("Day");
            cbb_shift.Items.Add("Night");
            cbb_Model_1.Items.Clear();
            dtb.get_model(cbb_Model_1, "6");
        }

        private void cbb_line_TextChanged(object sender, EventArgs e)
        {
            dtb.get_model(cbb_model, cbb_line.Text);
        }

        private void btn_exe_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbb_shift.Text == "" || cbb_line.Text == "" || cbb_model.Text == "")
                {
                    MessageBox.Show("Hãy nhập đầy đủ thông tin");
                }
                else
                {
                    string month = dt_boxing.Text.Substring(0, 2);
                    string day = dt_boxing.Text.Substring(3, 2);
                    string year = dt_boxing.Text.Substring(6, 4);

                    string filePath = Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + dt_boxing.Value.Year.ToString("0000") + "-" + dt_boxing.Value.Month.ToString("00") + "\\" + dt_boxing.Value.Month.ToString("00") + "-" + dt_boxing.Value.Day.ToString("00") + "_" + cbb_shift.Text + ".csv";
                    if (File.Exists(filePath))
                    {
                        DataTable historyTb = new DataTable();
                        historyTb = Excel.ReadCsvFile(filePath, CreateTable("","", "", "", "", "", "", "", "", "", ""));
                        ShowData(historyTb, dgv_boxing);
                        //dgv_boxing.DataSource = historyTb.DefaultView;
                        txt_total.Text = dgv_boxing.Rows.Count.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy lịch sử");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private DataTable ReadCsvFile(string filePath)
        {
            DataTable tbFile = new DataTable();
            string Fulltext;

            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString();
                    string[] rows = Fulltext.Split('\n');

                    for (int i = 0; i < rows.Length - 1; i++)
                    {
                        string[] rowValues = rows[i].Split(',');

                        if (i == 0)
                        {
                            for (int j = 0; j < rowValues.Length; j++)
                            {
                                tbFile.Columns.Add(rowValues[j]);
                            }
                        }
                        else
                        {

                            DataRow dr = tbFile.NewRow();
                            for (int k = 0; k < rowValues.Length; k++)
                            {
                                dr[k] = rowValues[k];
                            }

                            tbFile.Rows.Add(dr);
                        }

                    }
                }

            }

            return tbFile;
        }

        public DataTable CreateTable(string STT, string PO_name, string Lot, string barcode, string line, string model, string time_FU, string time_Box, string PIC, string status, string Qty_PO)
        {
            DataTable TableExcel = new DataTable();
            DataColumn column;
            DataRow Row;

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "STT";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "PO boxing";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Lot boxing";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Barcode";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Line";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Model";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Time test FU";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Time Boxing";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "PIC boxing";
            TableExcel.Columns.Add(column);


            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Status Lot";
            TableExcel.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = "Qty_PO";
            TableExcel.Columns.Add(column);

            Row = TableExcel.NewRow();
            Row["PO boxing"] = PO_name;
            Row["Lot boxing"] = Lot;
            Row["Barcode"] = barcode;
            Row["Line"] = line;
            Row["Model"] = model;
            Row["Time test FU"] = time_FU;
            Row["Time Boxing"] = time_Box;
            Row["PIC boxing"] = PIC;
            Row["Status Lot"] = status;
            Row["Qty_PO"] = Qty_PO;

            TableExcel.Rows.Add(Row);
            return TableExcel;
        }

        private void dgv_boxing_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            dgv_boxing.ColumnHeadersDefaultCellStyle.Font = new Font(dgv_boxing.Font, FontStyle.Bold);
            dgv_boxing.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_boxing.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void ShowData(DataTable dt, DataGridView dgv)
        {
            dgv.ClearSelection();
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn STT = new DataGridViewTextBoxColumn();
            STT.DataPropertyName = "STT";
            STT.HeaderText = "STT";
            STT.Name = "STT";
            STT.ReadOnly = true;
            STT.Width = 20;
            STT.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(STT);

            DataGridViewTextBoxColumn PO_boxing = new DataGridViewTextBoxColumn();
            PO_boxing.DataPropertyName = "PO boxing";
            PO_boxing.HeaderText = "PO boxing";
            PO_boxing.Name = "PO boxing";
            PO_boxing.ReadOnly = true;
            PO_boxing.Width = 120;
            PO_boxing.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(PO_boxing);


            DataGridViewTextBoxColumn Lot_boxing = new DataGridViewTextBoxColumn();
            Lot_boxing.DataPropertyName = "Lot boxing";
            Lot_boxing.HeaderText = "Lot boxing";
            Lot_boxing.Name = "Lot boxing";
            Lot_boxing.ReadOnly = true;
            Lot_boxing.Width = 140;
            Lot_boxing.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(Lot_boxing);

            DataGridViewTextBoxColumn col_Barcode = new DataGridViewTextBoxColumn();
            col_Barcode.DataPropertyName = "Barcode";
            col_Barcode.HeaderText = "Barcode";
            col_Barcode.Name = "Barcode";
            col_Barcode.ReadOnly = true;
            col_Barcode.Width = 110;
            col_Barcode.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(col_Barcode);

            DataGridViewTextBoxColumn Line = new DataGridViewTextBoxColumn();
            Line.DataPropertyName = "Line";
            Line.HeaderText = "Line";
            Line.Name = "Line";
            Line.ReadOnly = true;
            Line.Width = 45;
            Line.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(Line);

            DataGridViewTextBoxColumn model = new DataGridViewTextBoxColumn();
            model.DataPropertyName = "Model";
            model.HeaderText = "Model";
            model.Name = "Model";
            model.Width = 140;
            model.ReadOnly = true;
            model.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(model);

            DataGridViewTextBoxColumn FU = new DataGridViewTextBoxColumn();
            FU.DataPropertyName = "Time test FU";
            FU.HeaderText = "Time test FU";
            FU.Name = "Time test FU";
            FU.ReadOnly = true;
            FU.Width = 150;
            FU.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(FU);

            DataGridViewTextBoxColumn Time_boxing = new DataGridViewTextBoxColumn();
            Time_boxing.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Time_boxing.DataPropertyName = "Time boxing";
            Time_boxing.HeaderText = "Time boxing";
            Time_boxing.Name = "Time boxing";
            Time_boxing.Width = 150;
            Time_boxing.ReadOnly = true;
            dgv.Columns.Add(Time_boxing);

            DataGridViewTextBoxColumn PIC = new DataGridViewTextBoxColumn();
            PIC.DataPropertyName = "PIC boxing";
            PIC.HeaderText = "PIC boxing";
            PIC.Name = "PIC boxing";
            PIC.Width = 100;
            PIC.ReadOnly = true;
            PIC.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(PIC);

            DataGridViewTextBoxColumn Status = new DataGridViewTextBoxColumn();
            Status.DataPropertyName = "Status Lot";
            Status.HeaderText = "Status Lot";
            Status.Name = "Status Lot";
            Status.Width = 100;
            Status.ReadOnly = true;
            PIC.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(Status);

            DataGridViewTextBoxColumn Qty_PO = new DataGridViewTextBoxColumn();
            Qty_PO.DataPropertyName = "Qty_PO";
            Qty_PO.HeaderText = "Qty_PO";
            Qty_PO.Name = "Qty_PO";
            Qty_PO.Width = 80;
            Qty_PO.ReadOnly = true;
            PIC.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(Qty_PO);
            dgv.DataSource = dt;
           // dgv.Rows.Remove(dgv.Rows[0]);
            //dgv.Rows.Remove(dgv.Rows[dgv.Rows.Count - 1]);
            dgv.ClearSelection();
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            if (tb_CodePCM.Text == "")
            {
                MessageBox.Show("Nhập code PCM hoặc code Box của PCM để tìm dữ liệu");
            }
            else
            {
                if (!FormatCodeBox(tb_CodePCM.Text, cbb_Model_1.Text) && !FormatCodePCM(cbb_Model_1.Text, tb_CodePCM.Text))
                {
                    MessageBox.Show("Format code bạn vừa nhập không đúng, Vui lòng kiểm tra lại");
                }
                else if (FormatCodeBox(tb_CodePCM.Text, cbb_Model_1.Text))
                {
                    string[] Data;
                    List.Clear();
                    check.LoadList("LogToTal", ref List);
                    for (int i = 0; i < List.Count; i++)
                    {
                        Data = List[i].Split('\t');
                        if (Data[1] == tb_CodePCM.Text)
                        {
                            string path = string.Empty;
                            string Year = Data[0].Substring(0, 4);
                            string Month = Data[0].Substring(4, 2);
                            string Day = Data[0].Substring(6, 2);
                            if (Data[0].Contains("Day"))
                            {
                                path = @Application.StartupPath + "\\Result\\" + cbb_Model_1.Text + "\\" + Year + "-" + Month + "\\" + Month + "-" + Day + "_Day.csv";                            
                            }

                            if (Data[0].Contains("Night"))
                            {
                                path = @Application.StartupPath + "\\Result\\" + cbb_Model_1.Text + "\\" + Year + "-" + Month + "\\" + Month + "-" + Day + "_Night.csv";
                            }
                            DataTable dt = Excel.ReadCsvFile(path, CreateTable("" ,"", "", "", "", "", "", "", "", "", ""));
                            for (int j = 0; j < dt.Rows.Count;j++)
                            {
                                if (dt.Rows[j].ItemArray[2].ToString() != tb_CodePCM.Text )
                                {
                                    dt.Rows.Remove(dt.Rows[j]);
                                }
                            }
                            dgv_Show.Columns.Clear();
                            ShowData(dt, dgv_Show);
                        }                       
                    }         
                }
                else if (FormatCodePCM(cbb_Model_1.Text, tb_CodePCM.Text))
                {
                    string[] Data;
                    List.Clear();
                    check.LoadList("LogToTal", ref List);
                    for (int i = 0; i < List.Count; i++)
                    {
                        Data = List[i].Split('\t');
                        if (Data[2] == tb_CodePCM.Text)
                        {
                            string path = string.Empty;
                            string Year = Data[0].Substring(0, 4);
                            string Month = Data[0].Substring(4, 2);
                            string Day = Data[0].Substring(6, 2);
                            if (Data[0].Contains("Day"))
                            {
                                path = @Application.StartupPath + "\\Result\\" + cbb_Model_1.Text + "\\" + Year + "-" + Month + "\\" + Month + "-" + Day + "_Day.csv";
                            }

                            if (Data[0].Contains("Night"))
                            {
                                path = @Application.StartupPath + "\\Result\\" + cbb_Model_1.Text + "\\" + Year + "-" + Month + "\\" + Month + "-" + Day + "_Night.csv";
                            }
                            DataTable dt = Excel.ReadCsvFile(path, CreateTable("","", "", "", "", "", "", "", "", "", ""));
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {                               
                                if (dt.Rows[j].ItemArray[3].ToString() != tb_CodePCM.Text)
                                {
                                    dt.Rows.Remove(dt.Rows[j]);
                                    j = j - 1;
                                }
                            }
                            dgv_Show.Columns.Clear();
                            ShowData(dt, dgv_Show);
                        }
                    }         
                }
            }
        }

        public bool FormatCodeBox(string Code, string model)
        {            
            if (Code.Length != 17)
            {
                return false;
            }
            else
            {
                if (Code.Substring(0, 3) == "VPM")
                {
                    if (Isnumber(Code.Substring(3, 6)))
                    {
                        if (Code.Substring(9, 2) == "06")
                        {
                            if (model.ToUpper().Contains("MAIN"))
                            {
                                if (Code.Substring(12, 2) == "A7")
                                {
                                    if (Isnumber(Code.Substring(14, 3)))
                                        return true;
                                    else
                                        return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (model.ToUpper().Contains("CELL"))
                            {
                                if (Code.Substring(12, 2) == "A6")
                                {
                                    if (Isnumber(Code.Substring(14, 3)))
                                        return true;
                                    else return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if (model.ToUpper().Contains("SUB"))
                            {
                                if (Code.Substring(12, 2) == "A8")
                                {
                                    if (Isnumber(Code.Substring(14, 3)))
                                        return true;
                                    else return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool FormatCodePCM(string model, string code)
        {
            if (code.Length != 14)
            {
                return false;
            }
            else
            {
                //210312V005710R
                if (Isnumber(code.Substring(0, 5)))
                {
                    if (code.Substring(6, 1) == "V")
                    {
                        if (Isnumber(code.Substring(7, 6)))
                        {
                            if (model.ToUpper().Contains("MAIN"))
                            {
                                if (code.Substring(13, 1) == "R")
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                            else if (model.ToUpper().Contains("CELL"))
                            {
                                if (code.Substring(13, 1) == "P")
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                            else if (model.ToUpper().Contains("SUB"))
                            {
                                if (code.Substring(13, 1) == "T")
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public bool Isnumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }     


    }
}
