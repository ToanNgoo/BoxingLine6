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
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Threading;

namespace MatchingBarcodeBoxing
{
    public partial class Form1 : Form
    {
        /* D1000 
        D1000 = 0 : Normal
        D1000 = 1 : Không Scan được barcode
        D1000 = 2 : Format code sai tiêu chuẩn
        D1000 = 3 : Code đã boxing
        D1000 = 4 : PCM test NG - không boxing
        D1000 = 5 : PCM test 1 lần NG, 1 lần OK - không boxing        
        */

        OptionDefine.clsCheckTrungInformation checbox = new OptionDefine.clsCheckTrungInformation();

        database dtb = new database();
        clsPLC PLC = new clsPLC();
        ClsExcel Excel = new ClsExcel();
        clsReadData rDt = new clsReadData();
        clsScanner Scaner1;
        clsScanner Scaner2;
        clsSocket socketIns;

        List<string> ListBoxing = new List<string>();
        List<string> ListInfor = new List<string>();
        List<string> ListBarcode = new List<string>();
        List<string> ListTime = new List<string>();
        List<string> ListLot = new List<string>();
        List<string> ListInforPCM = new List<string>();
        List<string> Daily = new List<string>();

        private Thread upload;
        private Thread Ping;
        private Thread inspectionBarcode;
        private Thread mesIns;

        public bool chkUploadCode = false;
        public static string _user;
        string model = String.Empty;        
        int qty_Lot = 0;
        DataTable dt_actual;
        string link_Server = string.Empty;
        string link_PC = string.Empty;
        string User = string.Empty;
        int Lot_PCM = 0;
        int Qty_PO = 0;
        int Qty_PO_actual = 0;        
        string IP_PC = string.Empty;
        string IP_PC1 = string.Empty;
        string _model_MP = string.Empty;
        int STT = 0;
        string pos_cur = string.Empty;       
        int _Cycle = 0;
        public string MES_Connecting = "CANT";
        bool cntIns = false;
        public bool sentt = false;
        public bool _sentt = false;
        enum Error
        {
            EC001,
            EC002,
            EC003
        }

        public Form1()
        {
            InitializeComponent();
            this.ActiveControl = txt_user;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            //label24.Visible = false;
            //tb_barcode1.Visible = false;
            chb_pcmStk.Visible = false;
            //off box
            lbl_MesInp.Enabled = false;
            lbl_MesBx.Enabled = false;             
            //
            this.Icon = Properties.Resources.Business_packingboxes_negocio_2338;
            if (!File.Exists(@Application.StartupPath + @"\Log\Duplicate\Daily.log"))
            {
                checbox.SaveList("", "Daily");
            }

            string[] model = File.ReadAllLines(@Application.StartupPath + "\\Model_FCT.txt");
            cb_FCT6.Items.AddRange(model);

            if (!Directory.Exists(@Application.StartupPath + "\\Manual"))
            {
                Directory.CreateDirectory(@Application.StartupPath + "\\Manual");
            }
            for (int i = 0; i < model.Length; i++)
            {
                if (!Directory.Exists(@Application.StartupPath + "\\Manual\\" + model[i]))
                {
                    Directory.CreateDirectory(@Application.StartupPath + "\\Manual\\" + model[i]);
                }
            }

            cbb_SR1.Items.AddRange(SerialPort.GetPortNames());
            cbb_SR2.Items.AddRange(SerialPort.GetPortNames());
            loadConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);
            Scaner1 = new clsScanner(this);
            Scaner2 = new clsScanner(this);
            Scaner1.COMnum = cbb_SR1.Text;
            Scaner2.COMnum = cbb_SR2.Text;
            Scaner1.ketnoi(lbl_SR1);
            Scaner2.ketnoi(lbl_SR2);

            if (!File.Exists(@Application.StartupPath + "\\Config_Model.txt"))
            {
                File.WriteAllText(@Application.StartupPath + "\\Config_Model.txt", "Main:\r\nCell:\r\nSub:");
            }

            string[] Lot_model = File.ReadAllLines(@Application.StartupPath + "\\Config_Model.txt");

            tb_Main.Text = Lot_model[0].Substring(6);
            tb_Cell.Text = Lot_model[1].Substring(6);
            tb_Sub.Text = Lot_model[2].Substring(5);

            //Mes
            groupBox8.Enabled = false;
            groupBox9.Enabled = false;
            groupBox10.Enabled = false;
            rDt.ReadConfigMES(txt_ipMesInp, txt_linoMesInp, txt_mcidInp, txt_stnidInp, txt_prtInp,
                              txt_ipMesIns, txt_linoMesIns, txt_mcidIns, txt_stnidIns, txt_prtIns, 
                              txt_ipMesBox, txt_linoMesBox, txt_mcidBox, txt_stnidBox, txt_prtBox);
            chb_noMes.Checked = false;
            mesIns = new Thread(new ThreadStart(Connect_MES_Ins));
            mesIns.IsBackground = true;
            mesIns.Start();
            //Connect_MES_Ins();
            un_init();
            OleDbConnection cnn = dtb.GetConnection();
            OleDbConnection cnn2 = dtb.GetConnection2();
            cbb_line.Items.Add("6");
            btn_login.Text = "Đăng nhập";
            btn_Add.Enabled = false;
            checkBox2.Enabled = false;
            checkBox2.Checked = false;
            PLC.thietlap(tb_PLC);
            PLC.ketnoi(lbl_PLC);
            if (cnn != null && cnn2 != null)
            {
                stt_database.ForeColor = Color.Black;
                stt_database.BackColor = Color.Green;
                stt_database.Text = "Database avaiable";
            }
            else
            {
                stt_database.ForeColor = Color.Black;
                stt_database.BackColor = Color.Red;
                stt_database.Text = "Database not avaiable";
            }

            Ping = new Thread(new ThreadStart(ping_Server));
            Ping.Start();
            un_init();
            try
            {
                if (tb_Cycle.Text != "")
                    _Cycle = int.Parse(tb_Cycle.Text);
                else
                    _Cycle = 2;
                tb_Cycle.Text = _Cycle.ToString();
            }
            catch (Exception)
            {
                _Cycle = 2;
                tb_Cycle.Text = _Cycle.ToString();
            }

            PLC.Writeplc(tb_PCready.Text, 1);
            timer1.Enabled = true;
            checbox.LoadList("Daily", ref Daily);
            int _Daily = 0;
            string _timeline = DateTime.Now.Year.ToString() + "-" +DateTime.Now.Month.ToString("00") + "-" +DateTime.Now.Day.ToString("00");
            foreach (string cv in Daily)
            {
                if (cv == _timeline)
                {
                    if (DateTime.Now.Hour > 8)
                    {
                        _Daily++;
                    }
                }
            }

            //if (_Daily == 0)
            //{
            //    Warning = new Thread(new ThreadStart(RequestWarningDay));
            //    Warning.Start();
            //}
            STT = 0;
        }        

        public void Connect_MES_Ins()
        {
            cntIns = true;
            socketIns = new clsSocket(this);
            socketIns.Ip = txt_ipMesIns.Text;
            socketIns.Lineno = txt_linoMesIns.Text;
            socketIns.Mcid = txt_mcidIns.Text;
            socketIns.Stnid1 = txt_stnidIns.Text;
            socketIns.Stnid2 = "";
            socketIns.Stnid3 = "";
            socketIns.Port = int.Parse(txt_prtIns.Text);
            socketIns.Portprocess = "001";
            socketIns.Workerid = "20603021";
            socketIns.start(txt_ipMesIns.Text);
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            string kind = "";

            if (btn_login.Text == "Đăng nhập")
            {
                if (txt_user.Text == "" || txt_pw.Text == "")
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu bị trống!");
                }
                else if (dtb.login_admin(txt_user.Text, txt_pw.Text, kind) == true) // admin đăng nhập
                {
                    _user = txt_user.Text;
                    btn_login.Text = "Đăng xuất";
                    txt_user.Enabled = false;
                    txt_pw.Text = "";
                    txt_pw.Enabled = false;
                    stl_nameUser.Text = dtb.get_name(_user);
                    this.ActiveControl = tb_barcode;
                    checkBox1.Enabled = true;
                    cbb_model.Enabled = true;
                    cbb_line.Enabled = true;
                    cbox_OBA.Enabled = true;
                    checkBox2.Enabled = true;
                    chb_pcmStk.Enabled = true;
                    //init();
                }
                else if (dtb.login_part(txt_user.Text, txt_pw.Text, "PD") == true) // user đăng nhập
                {
                    _user = txt_user.Text;
                    btn_login.Text = "Đăng xuất";
                    txt_user.Enabled = false;
                    txt_pw.Text = "";
                    txt_pw.Enabled = false;
                    stl_nameUser.Text = dtb.get_name(_user);
                    this.ActiveControl = tb_barcode;
                    checkBox1.Enabled = true;
                    cbb_model.Enabled = true;
                    cbb_line.Enabled = true;
                    cbox_OBA.Enabled = true;
                    checkBox2.Enabled = true;
                    chb_pcmStk.Enabled = true;
                    //init();
                }
                else
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu. Hãy thử lại!");
                    txt_pw.Clear();
                    tb_CodeLot.Clear();
                    tb_barcode.Clear();
                    cbb_PO.Items.Clear();
                    cbb_PO.Text = "";
                    cbb_line.Text = "";
                    cbb_model.Text = "";
                    checkBox2.Enabled = false;
                }
            }
            else
            {
                txt_user.Enabled = true;
                txt_pw.Enabled = true;
                tb_barcode.Enabled = false;
                btn_login.Text = "Đăng nhập";
                txt_user.Text = "";
                stl_nameUser.Text = "";
                tb_barcode.Text = "";
                tb_CodeLot.Text = "";
                tb_CurrnentLot.Text = "";
                tb_Qty.Text = "";
                tb_QtyPO.Text = "";
                tb_TotalPO.Text = "";
                cbb_line.Text = "";
                cbb_model.Text = "";
                cbb_PO.Text = "";
                checkBox1.Checked = false;
                cbox_OBA.Checked = false;
                rbt_Day.Checked = false;
                rbt_Night.Checked = false;
                chb_pcmStk.Enabled = false;
                un_init();
            }
        }

        private void cbb_line_TextChanged(object sender, EventArgs e)
        {
            dtb.get_model(cbb_model, cbb_line.Text);
        }
                
        public bool xyLyInfor(string barcode)
        {
            sentt = false;
            _sentt = false;
            chkUploadCode = false;

            int error = 0;
            checbox.LoadList(ref ListInfor, @Application.StartupPath + "\\Log\\Duplicate\\" + _model_MP + ".log");

            foreach (string str in ListInfor)
            {
                string cod = str.Substring(3, 14);
                if (cod.Length == 14 && cod == barcode)
                {
                    ListBarcode.Add(str);
                }
            }

            if (ListBarcode.Count == 0)
            {
                error = 1; // PCM chua test function
            }
            else if (ListBarcode.Count == 1)
            {
                if (ListBarcode[0].Substring(0, 2) == "OK")
                {
                    error = 0;
                }
                else
                {
                    error = 2; // Test 1 times NG                   
                }
            }
            else if (ListBarcode.Count > 1)
            {
                if (ListBarcode[ListBarcode.Count - 1].Substring(0, 2) == "OK" && ListBarcode[ListBarcode.Count - 2].Substring(0, 2) == "OK")
                {
                    error = 0;
                }
                else
                {
                    error = 3; // Chưa đủ điều kiện boxing
                }
            }

            if (error == 0)
            {
                if (cbb_PO.Text.ToUpper().Contains("OBA"))
                {
                    PO_OBA(tb_barcode.Text);
                    return true;
                }
                else
                {
                    if(MES_Connecting == "CAN" && cntIns == true)//On MES
                    {
                        while (sentt == false)
                        {
                            if(!_sentt)
                            {
                                _sentt = true;
                                MesBarcodePCM_Ins();
                            }
                               
                        }
                        
                        if (chkUploadCode == true)//upload code MES OK
                        {
                            PO_run(tb_barcode.Text);
                            chkUploadCode = false;
                            sentt = false;
                            return true;
                        }
                        else
                        {
                            sentt = false;
                            tb_barcode.Text = "";
                            PLC.Writeplc(tb_ReadNG.Text, 1);
                            PLC.Writeplc(tb_ErrorCode.Text, 1);
                            return false;
                        }
                    } 
                    else
                    {
                        MessageBox.Show("Kết nối MES Fail!", "Inspection Process", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        tb_barcode.Text = "";
                        PLC.Writeplc(tb_ReadNG.Text, 1);
                        PLC.Writeplc(tb_ErrorCode.Text, 1);
                        return false;
                    }
                }
            }
            else
            {
                tb_barcode.Text = "";
                PLC.Writeplc(tb_ReadNG.Text, 1);
                if (error == 1)
                {
                    DialogResult r = MessageBox.Show("PCM chưa được test function", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (r == DialogResult.OK)
                        PLC.Writeplc(tb_ErrorCode.Text, 1);
                }
                if (error == 2)
                {
                    DialogResult r = MessageBox.Show("PCM test NG, không thể boxing", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (r == DialogResult.OK)
                        PLC.Writeplc(tb_ErrorCode.Text, 1);
                }
                if (error == 3)
                {
                    DialogResult r = MessageBox.Show("PCM test NG, nhưng chưa test 2 lần OK liên tiếp", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (r == DialogResult.OK)
                        PLC.Writeplc(tb_ErrorCode.Text, 1);
                }
                return false;
            }
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            History history = new History();
            history.Show();
        }

        private void btn_CreateLot_Click(object sender, EventArgs e)
        {
            try
            {
                checbox.LoadList("Lot", ref ListLot);
                if (tb_CodeLot.Text == "")
                {
                    ;
                }
                else if (tb_CodeLot.Text != "")
                {
                    if (!FormatCodeBox(tb_CodeLot.Text, cbb_model.Text))
                    {
                        tb_CodeLot.Text = "";
                        DialogResult rst = MessageBox.Show("Code Box mà bạn vừa nhập không đúng, vui lòng kiểm tra lại", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (checbox.CheckDuplicateInforamation(tb_CodeLot.Text, ListLot))
                        {
                            qty_Lot = 0;
                            tb_Qty.Text = qty_Lot.ToString() + "/" + Lot_PCM.ToString();
                            tb_barcode.Enabled = true;
                            tb_CurrnentLot.Text = tb_CodeLot.Text;
                            checbox.SaveList(tb_CodeLot.Text, "Lot");
                            btn_CreateLot.Enabled = false;
                            this.ActiveControl = tb_barcode;
                            tb_CodeLot.Enabled = false;
                            timer1.Enabled = true;
                            if (PLC.readplc(tb_PosCur.Text) == "1")
                            {
                                check_1 = false;
                            }
                            if (PLC.readplc(tb_PosCur.Text) == "2")
                            {
                                check_2 = false;
                            }
                            if (PLC.readplc(tb_PosCur.Text) == "3")
                            {
                                check_3 = false;
                            }
                        }
                        else
                        {
                            tb_CodeLot.Text = "";
                            DialogResult rst = MessageBox.Show("Box Lot nãy đã tồn tại, thông tin cho Sublead vấn đề này", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    ;
                }
                btn_ClosePO.Enabled = true;
                btn_closeLot.Enabled = true;
            }
            catch (Exception)
            {
                ;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbb_line.Text == "" || cbb_model.Text == "" || (rbt_Day.Checked == false && rbt_Night.Checked == false) || tb_QtyPO.Text == "" || int.Parse(tb_QtyPO.Text) < Lot_PCM)
            {
                if (cbb_model.Text == "")
                {
                    DialogResult r = MessageBox.Show("Bạn chưa chọn model!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (cbb_line.Text == "")
                {
                    DialogResult r = MessageBox.Show("Bạn chưa chọn line!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (rbt_Day.Checked == false && rbt_Night.Checked == false)
                {
                    DialogResult r = MessageBox.Show("Bạn chưa chọn Shift!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (tb_QtyPO.Text == "")
                {
                    DialogResult r = MessageBox.Show("Nhập số lượng boxing cho PO", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (int.Parse(tb_QtyPO.Text) < Lot_PCM)
                {
                    DialogResult r = MessageBox.Show("Số lượng Boxing PO nhỏ hơn so với Boxing Lot", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else
            {
                Qty_PO_actual = 0;
                bool OBA = false;
                Qty_PO = int.Parse(tb_QtyPO.Text);
                string _shift = string.Empty;
                if (rbt_Day.Checked == true)
                    _shift = "Day";
                if (rbt_Night.Checked == true)
                    _shift = "Night";
                DialogResult result;

                if (checkBox1.Checked == true)
                {
                    result = MessageBox.Show("Bạn muốn tạo PO : " + DateTimeActual().Year.ToString("00") + DateTimeActual().Month.ToString("00") + DateTimeActual().Day.ToString("00") + "_" + _shift + "_" + cbb_model.Text + "_" + cbb_line.Text, "Infor", MessageBoxButtons.YesNo);
                    OBA = true;
                }
                else
                {
                    result = MessageBox.Show("Bạn muốn tạo PO-OBA : " + DateTimeActual().Year.ToString("00") + DateTimeActual().Month.ToString("00") + DateTimeActual().Day.ToString("00") + "_" + _shift + "_" + cbb_model.Text + "_" + cbb_line.Text, "Infor", MessageBoxButtons.YesNo);
                    OBA = false;
                }

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    if (dtb.checkPO(cbb_line.Text, cbb_model.Text, DateTimeActual().Year.ToString("00") + DateTimeActual().Month.ToString("00") + DateTimeActual().Day.ToString("00"), _shift) == true)
                    {
                        MakeLog();
                        dtb.CreatePO(cbb_line.Text, cbb_model.Text, DateTimeActual().Year.ToString("00") + DateTimeActual().Month.ToString("00") + DateTimeActual().Day.ToString("00"), _shift, tb_QtyPO.Text, OBA);
                        MessageBox.Show("Tạo PO thành công");
                        cbb_line.Enabled = false;
                        cbb_model.Enabled = false;
                        tb_QtyPO.Enabled = false;
                        rbt_Day.Enabled = false;
                        rbt_Night.Enabled = false;
                        btn_CreatePO.Enabled = false;
                        btn_ClosePO.Enabled = true;
                        btn_closeLot.Enabled = true;
                        STT = 0;
                    }
                    else
                    {
                        DialogResult r = MessageBox.Show("PO đã tồn tại, hãy kiểm tra lại", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    ;
                }
            }
        }

        private void cbb_PO_Click(object sender, EventArgs e)
        {
            cbb_PO.Items.Clear();
            dtb.getPO(cbb_PO, cbb_model.Text, cbb_line.Text);
        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            await Task.Delay(500);
            btn_CreateLot.PerformClick();
            timer1.Enabled = true;
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
            Row["STT"] = STT;
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

        public void MakeLog()
        {
            DataTable tbl = new DataTable();
            DateTime dateTime = DateTime.Now;
            DateTime startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 7, 45, 0);
            DateTime endDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 19, 44, 0);

            DateTime dateTimeNight = DateTime.Now.AddDays(1);
            DateTime startDateTimeNight = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 19, 45, 0);
            DateTime endDateTimeNight = new DateTime(dateTimeNight.Year, dateTimeNight.Month, dateTimeNight.Day, 7, 44, 0);

            if (cbb_PO.Text.ToUpper().Contains("OBA"))
            {
                if (rbt_Day.Checked)
                {
                    if (!Directory.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\OBA\\" + cbb_model.Text);
                    }

                    if (!Directory.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00")))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00"));
                    }
                }

                if (rbt_Night.Checked)
                {

                    if (!Directory.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\OBA\\" + cbb_model.Text);
                    }

                    if (!Directory.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00")))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00"));
                    }
                }
            }
            else
            {
                if (rbt_Day.Checked)
                {
                    if (!Directory.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\Result\\" + cbb_model.Text);
                    }

                    if (!Directory.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00")))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00"));
                    }
                }

                if (rbt_Night.Checked)
                {
                    if (!Directory.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\Result\\" + cbb_model.Text);
                    }

                    if (!Directory.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00")))
                    {
                        Directory.CreateDirectory(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00"));
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListBarcode.Clear();
            ListInfor.Clear();
            try
            {
                if (tb_CurrnentLot.Text == "")
                {
                    MessageBox.Show("Chưa có code Box, Hãy nhập code Box ");
                }
                else
                {
                    if (tb_barcode.Text == "")
                    {
                        ;
                    }
                    else
                    {
                        if (!FormatCodePCM(cbb_PO.Text, tb_barcode.Text))
                        {
                            MessageBox.Show("Code bạn vừa nhập không đúng Format, hãy kiểm tra lại");
                        }
                        else
                        {
                            string barcode = tb_barcode.Text;
                            if (cbb_PO.Text.ToUpper().Contains("OBA"))
                            {

                                checbox.LoadList("INFO_OBA", ref ListBoxing);
                                if (checbox.CheckDuplicateInforamation(barcode, ListBoxing) == false)
                                {
                                    MessageBox.Show("Code này đã được boxing");
                                }
                                else
                                {
                                    xyLyInfor(barcode);
                                    checbox.SaveList(barcode, "INFO_OBA");
                                }
                            }
                            else
                            {
                                checbox.LoadList("INFO", ref ListBoxing);
                                if (checbox.CheckDuplicateInforamation(barcode, ListBoxing) == false)
                                {
                                    MessageBox.Show("Code này đã được boxing");
                                }
                                else
                                {
                                    xyLyInfor(barcode);
                                    checbox.SaveList(barcode, "INFO");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Lỗi CSDL");
            }
        }

        private void ShowData(DataTable dt, DataGridView dgv)
        {
            dgv_result.ClearSelection();
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn STT = new DataGridViewTextBoxColumn();
            STT.DataPropertyName = "STT";
            STT.HeaderText = "STT";
            STT.Name = "STT";
            STT.ReadOnly = true;
            STT.Width = 50;
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
            col_Barcode.Width = 120;
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
            model.Width = 150;
            model.ReadOnly = true;
            model.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns.Add(model);

            DataGridViewTextBoxColumn FU = new DataGridViewTextBoxColumn();
            FU.DataPropertyName = "Time test FU";
            FU.HeaderText = "Time test FU";
            FU.Name = "Time test FU";
            FU.ReadOnly = true;
            FU.Width = 140;
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
            dgv.Rows.Remove(dgv.Rows[0]);
            dgv.Rows.Remove(dgv.Rows[dgv.Rows.Count - 1]);
            dgv.ClearSelection();
        }

        List<string> a = new List<string>();
        private void tb_CurrnentLot_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void tb_Qty_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult rsl;
            rsl = MessageBox.Show("Bạn có muốn kết thúc PO này", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rsl == System.Windows.Forms.DialogResult.Yes)
            {
                DataTable tbl1 = new DataTable();
                if (rbt_Day.Checked)
                {
                    tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                }

                if (rbt_Night.Checked)
                {
                    tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                }

                for (int tmp = 0; tmp < tbl1.Rows.Count; tmp++)
                {
                    if (tbl1.Rows[tmp].ItemArray[1].ToString() == tb_CurrnentLot.Text)
                    {
                        tbl1.Rows[tmp]["Status Lot"] = "Close";
                    }
                }

                tbl1.Rows.Remove(tbl1.Rows[tbl1.Rows.Count - 1]);
                if (rbt_Day.Checked)
                {
                    Excel.Export_CSV_1(tbl1, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot\n");
                }

                if (rbt_Night.Checked)
                {
                    Excel.Export_CSV_1(tbl1, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot\n");
                }
                if (rbt_Day.Checked)
                {
                    tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                }

                if (rbt_Night.Checked)
                {
                    tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                }

                checbox.SaveList(cbb_PO.Text + "\t" + tb_QtyPO.Text + "\t" + Qty_PO_actual.ToString(), "PO");
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
                tb_CodeLot.Clear();
                tb_Qty.Clear();
                tb_CurrnentLot.Clear();
                btn_CreateLot.Enabled = true;
                dtb.delete_PO(cbb_PO.Text);
                STT = 0;
                Qty_PO_actual = 0;
                DialogResult r = MessageBox.Show("PO đã được đóng", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void un_init()
        {
            btn_CreatePO.Enabled = false;
            tb_CodeLot.Enabled = false;
            btn_CreateLot.Enabled = false;
            tb_barcode.Enabled = false;
            btn_ClosePO.Enabled = false;
            btn_closeLot.Enabled = false;
            cbb_PO.Enabled = false;
            rbt_Day.Enabled = false;
            rbt_Night.Enabled = false;
            cbb_line.Enabled = false;
            cbb_model.Enabled = false;
            checkBox1.Enabled = false;
            tb_QtyPO.Enabled = false;
            cbox_OBA.Enabled = false;
            chb_pcmStk.Enabled = false;
        }        

        private DateTime DateTimeActual()
        {
            DateTime dtActual;
            DateTime dateTime = DateTime.Now;
            DateTime startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 7, 45, 0);
            DateTime endDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 19, 44, 0);
            if ((DateTime.Now <= new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 19, 44, 0) && DateTime.Now >= new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 7, 45, 0)) || (DateTime.Now > new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 00, 00, 00) && DateTime.Now < new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 7, 45, 0)))
            {
                dtActual = DateTime.Now;
            }
            else
            {
                dtActual = DateTime.Now.AddDays(1);
            }
            return dtActual;
        }

        private void cbb_PO_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                STT = 0;
                dt_actual = new DataTable();
                bool check = false;
                if (cbb_PO.Text.ToUpper().Contains("OBA"))
                {
                    if (cbb_PO.Text.Contains("Night"))
                    {
                        if (File.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv"))
                        {
                            dt_actual = Excel.ReadCsvFile(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                            check = true;
                        }
                    }

                    if (cbb_PO.Text.Contains("Day"))
                    {
                        if (File.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv"))
                        {
                            dt_actual = Excel.ReadCsvFile(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                            check = true;
                        }
                    }
                }
                else
                {
                    if (cbb_PO.Text.Contains("Night"))
                    {
                        if (File.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv"))
                        {
                            dt_actual = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                            check = true;
                        }
                    }

                    if (cbb_PO.Text.Contains("Day"))
                    {
                        if (File.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv"))
                        {
                            dt_actual = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                            check = true;
                        }
                    }
                }

                if (check)
                {
                    ShowData(dt_actual, dgv_result);
                    int dem = 0;
                    int Qty = 0;
                    List<string> LotBox = new List<string>();
                    List<string> duplicate = new List<string>();
                    List<string> temp = new List<string>();
                    tb_barcode.Enabled = true;
                    tb_CurrnentLot.Text = tb_CodeLot.Text;
                    qty_Lot = 0;
                    tb_Qty.Text = qty_Lot.ToString();
                    btn_CreateLot.Enabled = true;
                    Qty_PO = int.Parse(dtb.get_QtyPO(cbb_PO.Text));
                    STT = dt_actual.Rows.Count;
                    //tb_CodeLot.Enabled = true;
                    foreach (DataRow dr in dt_actual.Rows)
                    {
                        LotBox.Add(dr.ItemArray[2].ToString());
                    }
                    if (LotBox.Count == Qty_PO)
                    {
                        DialogResult r = MessageBox.Show("PO đã boxing đủ, bạn ko thể nhập thêm PCM vào PO này", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tb_CodeLot.Enabled = false;
                        btn_CreateLot.Enabled = false;
                        tb_barcode.Enabled = false;
                    }
                    else
                    {
                        tb_TotalPO.Text = LotBox.Count + "/" + Qty_PO.ToString();
                        Qty_PO_actual = LotBox.Count;
                        duplicate = LotBox.Distinct().ToList();
                        for (int i = 0; i < duplicate.Count; i++)
                        {
                            dem = 0;
                            if (duplicate[i] != "")
                            {
                                foreach (DataRow dr in dt_actual.Rows)
                                {
                                    if (dr.ItemArray[2].ToString() == duplicate[i] && dr.ItemArray[9].ToString().Contains("Open"))
                                    {
                                        dem++;
                                    }
                                }
                                if (dem < Lot_PCM && dem > 0)
                                {
                                    temp.Add(duplicate[i]);
                                    Qty = dem;
                                    break;
                                }
                            }
                        }
                        if (temp.Count >= 1)
                        {
                            DialogResult rst;
                            rst = MessageBox.Show("Hiện tại Lot Boxing " + temp[0] + " chưa đủ số lượng boxing\r\nBạn có muốn boxing tiếp vào Lot này", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (rst == System.Windows.Forms.DialogResult.Yes)
                            {
                                tb_CurrnentLot.Text = temp[0];
                                if (cbb_PO.Text.ToUpper().Contains("MAIN"))
                                {
                                    tb_Qty.Text = Qty.ToString() + "/" + tb_Main.Text;
                                }

                                if (cbb_PO.Text.ToUpper().Contains("CELL"))
                                {
                                    tb_Qty.Text = Qty.ToString() + "/" + tb_Cell.Text;
                                }

                                if (cbb_PO.Text.ToUpper().Contains("SUB"))
                                {
                                    tb_Qty.Text = Qty.ToString() + "/" + tb_Sub.Text;
                                }

                                tb_CodeLot.Enabled = false;
                                qty_Lot = Qty;
                                timer1.Enabled = true;
                            }
                            else
                            {
                                tb_CodeLot.Enabled = true;
                            }
                        }
                        else if (temp.Count == 0)
                        {
                            tb_CodeLot.Enabled = true;
                        }
                    }
                    btn_closeLot.Enabled = true;
                    btn_ClosePO.Enabled = true;
                }
                else
                {
                    Qty_PO = int.Parse(dtb.get_QtyPO(cbb_PO.Text));
                    tb_TotalPO.Text = "0/" + Qty_PO.ToString();
                    tb_CodeLot.Enabled = true;
                    btn_CreateLot.Enabled = true;
                }
            }
            catch (Exception)
            {
                tb_CodeLot.Enabled = true;
            }
        }

        private void cbb_model_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbt_Night.Enabled = true;
            rbt_Day.Enabled = true;
            string[] Lot_model = File.ReadAllLines(@Application.StartupPath + "\\Config_Model.txt");
            if (cbb_model.Text.ToUpper().Contains("MAIN"))
            {
                Lot_PCM = int.Parse(tb_Main.Text);
                _model_MP = "MAIN";
            }

            if (cbb_model.Text.ToUpper().Contains("CELL"))
            {
                Lot_PCM = int.Parse(tb_Cell.Text);
                _model_MP = "CELL";
            }

            if (cbb_model.Text.ToUpper().Contains("SUB"))
            {
                Lot_PCM = int.Parse(tb_Sub.Text);
                _model_MP = "SUB";
            }

            lbl_modelCod.Text = rDt.ReadModelCode(_model_MP);

            if (checkBox1.Checked == true || cbox_OBA.Checked == true)
            {
                tb_QtyPO.Enabled = true;
            }
        }

        private void rbt_Day_CheckedChanged(object sender, EventArgs e)
        {
            btn_CreatePO.Enabled = true;
            cbb_PO.Enabled = true;
        }

        private void rbt_Night_CheckedChanged(object sender, EventArgs e)
        {
            btn_CreatePO.Enabled = true;
            cbb_PO.Enabled = true;
        }

        private void cbb_line_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cbb_model_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cbb_PO_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void btn_closeLot_Click(object sender, EventArgs e)
        {
            DataTable tbl1 = new DataTable();
            if (rbt_Day.Checked)
            {
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }
            if (rbt_Night.Checked)
            {
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            for (int tmp = 0; tmp < tbl1.Rows.Count; tmp++)
            {
                if (tbl1.Rows[tmp].ItemArray[2].ToString() == tb_CurrnentLot.Text)
                {
                    tbl1.Rows[tmp]["Status Lot"] = "Close";
                }
            }
            tbl1.Rows.Remove(tbl1.Rows[tbl1.Rows.Count - 1]);
            if (rbt_Day.Checked)
            {
                Excel.Export_CSV_1(tbl1, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty PO\n");
            }
            if (rbt_Night.Checked)
            {
                Excel.Export_CSV_1(tbl1, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty PO\n");
            }

            if (rbt_Day.Checked)
            {
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }
            if (rbt_Night.Checked)
            {
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            dgv_result.Columns.Clear();
            ShowData(tbl1, dgv_result);
            for (int i = 0; i < dgv_result.Rows.Count; i++)
            {
                if (dgv_result.Rows[i].Cells["Status Lot"].Value.ToString() == "Close")
                {
                    dgv_result.Rows.Remove(dgv_result.Rows[i]);
                    i--;
                }
            }
            tb_CodeLot.Clear();
            tb_Qty.Clear();
            tb_CurrnentLot.Clear();
            btn_CreateLot.Enabled = true;
            tb_CodeLot.Enabled = true;
        }

        private void btn_SaveModel_Click(object sender, EventArgs e)
        {
            File.WriteAllText(@Application.StartupPath + "\\Config_Model.txt", "Main:\t" + tb_Main.Text + "\r\nCell:\t" + tb_Cell.Text + "\r\nSub:\t" + tb_Sub.Text);
        }

        public bool FormatCodeBox(string Code, string model)
        {            
            if (Code.Length != 17)
            {
                return false;
            }
            else
            {
                if (Code.Substring(0, 3).ToUpper() == "VPM")
                {
                    if (Isnumber(Code.Substring(3, 6)))
                    {
                        if (Code.Substring(9, 2) == "06")
                        {
                            if (model.ToUpper().Contains("MAIN"))
                            {
                                if (Code.Substring(12, 2).ToUpper() == "A7")
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
                                if (Code.Substring(12, 2).ToUpper() == "A6")
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
                                if (Code.Substring(12, 2).ToUpper() == "A8")
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
                    if (code.Substring(6, 1).ToUpper() == "V")
                    {
                        if (Isnumber(code.Substring(7, 6)))
                        {
                            if (model.ToUpper().Contains("MAIN"))
                            {
                                if (code.Substring(13, 1).ToUpper() == "R")
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                            else if (model.ToUpper().Contains("CELL"))
                            {
                                if (code.Substring(13, 1).ToUpper() == "P")
                                {
                                    return true;
                                }
                                else
                                    return false;
                            }
                            else if (model.ToUpper().Contains("SUB"))
                            {
                                if (code.Substring(13, 1).ToUpper() == "T")
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                cbb_line.Enabled = true;
                cbb_model.Enabled = true;
                tb_QtyPO.Enabled = true;
                btn_CreatePO.Enabled = true;
                cbox_OBA.Checked = false;
            }
            else if (cbox_OBA.Checked == false && checkBox1.Checked == false)
            {
                tb_QtyPO.Enabled = false;
                btn_CreatePO.Enabled = false;
            }
        }

        private void cbox_OBA_CheckedChanged(object sender, EventArgs e)
        {
            if (cbox_OBA.Checked == true)
            {
                cbb_line.Enabled = true;
                cbb_model.Enabled = true;
                tb_QtyPO.Enabled = true;
                btn_CreatePO.Enabled = true;
                checkBox1.Checked = false;
            }
            else if (cbox_OBA.Checked == false && checkBox1.Checked == false)
            {
                tb_QtyPO.Enabled = false;
                btn_CreatePO.Enabled = false;
            }
        }

        public void PO_run(string barcode)
        {
            string path = string.Empty;
            if (cbb_PO.Text.ToUpper().Contains("OBA"))
            {
                path = @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            else
            {
                path = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            STT++;
            DataTable dt = CreateTable(STT.ToString(), cbb_PO.Text, tb_CurrnentLot.Text, barcode, cbb_line.Text, cbb_model.Text, ListBarcode[0].Substring(18, 19), DateTime.Now.ToString(), stl_nameUser.Text, "Open", Qty_PO.ToString());
            qty_Lot = qty_Lot + 1;
            Qty_PO_actual = Qty_PO_actual + 1;
            tb_TotalPO.Text = Qty_PO_actual.ToString() + "/" + Qty_PO.ToString();
            tb_Qty.Text = qty_Lot.ToString() + "/" + Lot_PCM.ToString();
            DataTable tbl1 = new DataTable();

            if (rbt_Day.Checked)
            {
                MakeLog();
                if (!File.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv"))
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                else
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", true, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }

                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
                for (int i = 0; i < dgv_result.Rows.Count; i++)
                {
                    if (dgv_result.Rows[i].Cells[9].Value.ToString() == "Close")
                    {
                        dgv_result.Rows.Remove(dgv_result.Rows[i]);
                        i--;
                    }
                }
            }

            if (rbt_Night.Checked == true)
            {
                MakeLog();
                if (!File.Exists(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv"))
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                else
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", true, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }

                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
                for (int i = 0; i < dgv_result.Rows.Count; i++)
                {
                    if (dgv_result.Rows[i].Cells[9].Value.ToString() == "Close")
                    {
                        dgv_result.Rows.Remove(dgv_result.Rows[i]);
                        i--;
                    }
                }
            }

            checbox.SaveList(barcode, "INFO");
            if (cbb_model.Text.ToUpper().Contains("MAIN"))
            {
                if (qty_Lot == int.Parse(tb_Main.Text) || Qty_PO_actual == Qty_PO)
                {
                    barcodeModel();
                }
            }

            if (cbb_model.Text.ToUpper().Contains("CELL"))
            {
                if (qty_Lot == int.Parse(tb_Cell.Text) || Qty_PO_actual == Qty_PO)
                {
                    barcodeModel();
                }
            }

            if (cbb_model.Text.ToUpper().Contains("SUB"))
            {
                if (qty_Lot == int.Parse(tb_Sub.Text) || Qty_PO_actual == Qty_PO)
                {
                    barcodeModel();
                }
            }
            tb_barcode.Text = "";
        }

        public void PO_OBA(string barcode)
        {
            string path = string.Empty;
            if (cbb_PO.Text.ToUpper().Contains("OBA"))
            {
                path = @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            else
            {
                path = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            STT++;
            DataTable dt = CreateTable(STT.ToString(), cbb_PO.Text, tb_CurrnentLot.Text, barcode, cbb_line.Text, cbb_model.Text, ListBarcode[0].Substring(18, 19), DateTime.Now.ToString(), stl_nameUser.Text, "Open", Qty_PO.ToString());
            qty_Lot = qty_Lot + 1;
            Qty_PO_actual = Qty_PO_actual + 1;
            tb_TotalPO.Text = Qty_PO_actual.ToString() + "/" + Qty_PO.ToString();
            tb_Qty.Text = qty_Lot.ToString() + "/" + Lot_PCM.ToString();
            DataTable tbl1 = new DataTable();

            if (rbt_Day.Checked)
            {
                MakeLog();
                if (!File.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv"))
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                else
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", true, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
            }

            if (rbt_Night.Checked == true)
            {
                MakeLog();
                if (!File.Exists(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv"))
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                else
                {
                    Excel.Export_CSV(dt, @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", true, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO");
                    checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
                }
                tbl1 = Excel.ReadCsvFile(@Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
            }
            if (cbb_model.Text.ToUpper().Contains("MAIN"))
            {
                if (qty_Lot == int.Parse(tb_Main.Text))
                {
                    barcodeModel();
                }
            }

            if (cbb_model.Text.ToUpper().Contains("CELL"))
            {
                if (qty_Lot == int.Parse(tb_Cell.Text))
                {
                    barcodeModel();
                }
            }

            if (cbb_model.Text.ToUpper().Contains("SUB"))
            {
                if (qty_Lot == int.Parse(tb_Sub.Text))
                {
                    barcodeModel();
                }
            }
            tb_barcode.Text = "";
        }

        public void barcodeModel()
        {
            DataTable tbl1 = new DataTable();
            string path = string.Empty;
            if (cbb_PO.Text.ToUpper().Contains("OBA"))
            {
                path = @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            else
            {
                path = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }

            if (rbt_Day.Checked)
            {
                tbl1 = Excel.ReadCsvFile(path + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            if (rbt_Night.Checked)
            {
                tbl1 = Excel.ReadCsvFile(path + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            for (int tmp = 0; tmp < tbl1.Rows.Count; tmp++)
            {
                if (tbl1.Rows[tmp].ItemArray[2].ToString() == tb_CurrnentLot.Text)
                {
                    tbl1.Rows[tmp]["Status Lot"] = "Close";
                }
            }

            if (rbt_Day.Checked)
            {
                Excel.Export_CSV_1(tbl1, path + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
            }

            if (rbt_Night.Checked)
            {
                Excel.Export_CSV_1(tbl1, path + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
            }

            if (Qty_PO_actual == Qty_PO)
            {
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
                for (int i = 0; i < dgv_result.Rows.Count; i++)
                {
                    if (dgv_result.Rows[i].Cells[9].Value.ToString() == "Close")
                    {
                        dgv_result.Rows.Remove(dgv_result.Rows[i]);
                        i--;
                    }
                }
                string _dttime = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " " + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + "-" + DateTime.Now.Second.ToString("00");
                checbox.SaveList(cbb_PO.Text + "\t" + _dttime + "\t" + tb_QtyPO.Text + "\t" + Qty_PO_actual.ToString(), "PO");
                qty_Lot = 0;
                tb_Qty.Text = qty_Lot.ToString();
                tb_CodeLot.Enabled = false;
                tb_CodeLot.Clear();
                dtb.delete_PO(cbb_PO.Text);
                btn_CreateLot.Enabled = false;
                un_init();
                checkBox1.Enabled = true;
                tb_CodeLot.Clear();
                tb_CurrnentLot.Clear();
                tb_Qty.Clear();
                tb_QtyPO.Clear();
                tb_TotalPO.Clear();
                cbb_line.Text = "";
                cbb_model.Text = "";
                cbb_PO.Text = "";
                cbox_OBA.Enabled = true;
                tb_barcode.Text = "";
                STT = 0;
                Qty_PO_actual = 0;
                MessageBox.Show("PO đã boxing đủ PCM");
            }
            else
            {
                tb_barcode.Text = "";
                dgv_result.Columns.Clear();
                ShowData(tbl1, dgv_result);
                qty_Lot = 0;
                tb_Qty.Text = qty_Lot.ToString();
                tb_CodeLot.Enabled = true;
                tb_CodeLot.Clear();
                btn_CreateLot.Enabled = true;
                tb_CurrnentLot.Clear();
                this.ActiveControl = tb_CodeLot;
                DialogResult r = MessageBox.Show("Lot boxing đã đủ PCM, tạo lot boxing mới", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);                                              
            }
        }

        public bool Ping_IP(string IP)
        {
            Ping myPing = new Ping();
            PingReply reply = myPing.Send(IP, 1000);
            if (reply != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            Qty_PO = int.Parse(tb_QtyPO.Text);
            tb_TotalPO.Text = Qty_PO_actual.ToString() + "/" + Qty_PO.ToString();
            DataTable tbl1 = new DataTable();
            string path = string.Empty;
            if (cbb_PO.Text.ToUpper().Contains("OBA"))
            {
                path = @Application.StartupPath + "\\OBA\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }
            else
            {
                path = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00");
            }

            if (rbt_Day.Checked)
            {
                tbl1 = Excel.ReadCsvFile(path + "_Day.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            if (rbt_Night.Checked)
            {
                tbl1 = Excel.ReadCsvFile(path + "_Night.csv", CreateTable("", "", "", "", "", "", "", "", "", "", ""));
            }

            for (int tmp = 0; tmp < tbl1.Rows.Count; tmp++)
            {               
                tbl1.Rows[tmp]["Qty_PO"] = Qty_PO.ToString();               
            }
            ShowData(tbl1, dgv_result);

            if (rbt_Day.Checked)
            {
                Excel.Export_CSV_1(tbl1, path + "_Day.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
            }

            if (rbt_Night.Checked)
            {
                Excel.Export_CSV_1(tbl1, path + "_Night.csv", false, "STT, PO boxing, Lot boxing, Barcode, Line, Model, Time test FU, Time boxing, PIC boxing, Status Lot, Qty_PO\n");
                checbox.SaveList(cbb_PO.Text + "\t" + tb_CurrnentLot.Text + "\t" + tb_barcode.Text + "\t" + cbb_line.Text + "\t" + cbb_model.Text + "\t" + DateTime.Now.ToString() + "\t" + stl_nameUser.Text + "\t" + Qty_PO.ToString(), "LogTotal");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                tb_QtyPO.Enabled = true;
                btn_Add.Enabled = true;
            }
        }

        bool check_1 = false;
        bool check_2 = false;
        bool check_3 = false;
        bool check_4 = false;
        bool check_5 = false;
        bool check_6 = false;
        bool check_7 = false;
        bool check_8 = false;        
        bool check_upload = true;       
        bool run = false;
        bool stop = false;
        int step = 1;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Minute % _Cycle == 0 && DateTime.Now.Second < 5 && check_upload == true && lbl_Function.BackColor == Color.Green && run == false)
            {
                check_upload = false;
                upload = new Thread(new ThreadStart(upLoadData));
                upload.Start();
            }

            if (PLC.readplc("X11") == "1" && check_upload == false && run == false && stop == false)
            {
                stop = true;
            }

            if (!stop)
            {
                pos_cur = PLC.readplc(tb_PosCur.Text);
                PLC.Writeplc(tb_PCready.Text, 1);
                //label40.Text = PLC.readplc(tb_PosCur.Text);

                switch (step)
                {
                    case 1:
                        if (PLC.readplc(tb_PosCur.Text) == "1" && check_1 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_1 = true;
                            run = true;
                            ReadingCode("1");
                            Thread.Sleep(500);
                            step = 2;
                        }
                        break;
                    case 2:
                        if (PLC.readplc(tb_PosCur.Text) == "2" && check_2 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_2 = true;              
                            ReadingCode("2");
                            Thread.Sleep(500);
                            step = 3;
                        }
                        break;
                    case 3:
                        if (PLC.readplc(tb_PosCur.Text) == "3" && check_3 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_3 = true;               
                            ReadingCode("3");
                            Thread.Sleep(500);
                            step = 4;
                        }
                        break;
                    case 4:
                        if (PLC.readplc(tb_PosCur.Text) == "4" && check_4 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_4 = true;
                            ReadingCode("4");
                            step = 5;
                        }
                        break;
                    case 5:
                        if (PLC.readplc(tb_PosCur.Text) == "5" && check_5 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_5 = true;
                            ReadingCode("5");
                            step = 6;
                        }
                        break;
                    case 6:
                        if (PLC.readplc(tb_PosCur.Text) == "6" && check_6 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_6 = true;
                            ReadingCode("6");
                            step = 7;
                        }
                        break;
                    case 7:
                        if (PLC.readplc(tb_PosCur.Text) == "7" && check_7 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_7 = true;
                            ReadingCode("7");
                            step = 8;
                        }
                        break;
                    case 8:
                        if (PLC.readplc(tb_PosCur.Text) == "8" && check_8 == false && PLC.readplc("M60") == "1" && PLC.readplc(tb_TrayReady.Text) == "1")
                        {
                            check_8 = true;
                            ReadingCode("8");
                        }
                        break;
                    default:
                        break;
                }

                if (PLC.readplc(tb_PosCur.Text) == "11")
                {
                    check_5 = false;
                    check_1 = false;
                    check_2 = false;
                    check_3 = false;
                    check_4 = false;
                    check_6 = false;
                    check_7 = false;
                    check_8 = false;
                    run = false;
                    step = 1;
                }
            }

            if (DateTime.Now.Hour == 20 && DateTime.Now.Minute == 0 && DateTime.Now.Second < 2 && check_PO)
            {
                check_PO = false;
                Warning = new Thread(new ThreadStart(RequestWarningDay));
                Warning.Start();             
            }

            if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0 && DateTime.Now.Second < 2 && check_PO)
            {
                check_PO = false;                
                Warning = new Thread(new ThreadStart(RequestWarningNight));
                Warning.Start();                
            }
        }

        bool check_PO = true;
        int check_CodeLot = 0;
        public void ReadingCode(string vTri)
        {
            if (tb_CurrnentLot.Text == "" && check_CodeLot == 0)
            {
                check_CodeLot = 1;
                MessageBox.Show("Bạn chưa nhập Lot mới, hãy nhập Lot mới để tiếp tục boxing", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tb_CurrnentLot.Text != "" && tb_CodeLot.Enabled == false)
            {
                if (vTri.ToString() == "1")
                {
                    tb_PCMNG.Text = "";
                }

                if (_model_MP == "CELL")
                {
                    if (vTri == "1")
                    {
                        scan2(int.Parse(vTri));
                    }

                    if (vTri == "2")
                    {
                        scan1(int.Parse(vTri));
                    }
                }

                if (_model_MP == "MAIN")
                {
                    scan2(int.Parse(vTri));
                }

                if (_model_MP == "SUB")
                {
                    if (vTri == "1" || vTri == "3" || vTri == "5" || vTri == "7")
                    {
                        scan1(int.Parse(vTri));
                    }
                    if (vTri == "2" || vTri == "4" || vTri == "6" || vTri == "8")
                    {
                        scan2(int.Parse(vTri));
                    }
                }
            }
        }

        public bool checkCode(TextBox tb, int vTri)
        {
            ListBarcode.Clear();
            ListInfor.Clear();
            try
            {
                if (!FormatCodePCM(cbb_PO.Text, tb.Text))
                {
                    tb.Text = "";
                    PLC.Writeplc(tb_ReadNG.Text, 1);
                    DialogResult r = MessageBox.Show("PCM số : " + vTri.ToString() + " không đúng format, hãy kiểm tra lại", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (r == DialogResult.OK)
                        PLC.Writeplc(tb_ErrorCode.Text, 1);
                    return false;
                }
                else
                {
                    string barcode = tb.Text;
                    if (cbb_PO.Text.ToUpper().Contains("OBA"))
                    {
                        checbox.LoadList("INFO_OBA", ref ListBoxing);
                        if (checbox.CheckDuplicateInforamation(barcode, ListBoxing) == false)
                        {
                            tb.Text = "";
                            PLC.Writeplc(tb_ReadNG.Text, 1);
                            DialogResult r = MessageBox.Show("PCM số : " + vTri.ToString() + " có code đã được boxing ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            if (r == DialogResult.OK)
                                PLC.Writeplc(tb_ErrorCode.Text, 1);
                            return false;
                        }
                        else
                        {
                            if (xyLyInfor(barcode))
                            {
                                if (checbox.CheckDuplicateInforamation(tb_CodeLot.Text, ListLot))
                                {
                                    if (tb_CodeLot.Text != "")
                                        checbox.SaveList(tb_CodeLot.Text, "Lot");
                                }
                                else
                                {
                                    ;
                                }
                                return true;
                            }
                            else
                                return false;
                        }
                    }
                    else
                    {
                        checbox.LoadList("INFO", ref ListBoxing);
                        if (checbox.CheckDuplicateInforamation(barcode, ListBoxing) == false)
                        {
                            tb.Text = "";
                            PLC.Writeplc(tb_ReadNG.Text, 1);
                            DialogResult r = MessageBox.Show("PCM số : " + vTri.ToString() + " có code đã được boxing ", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            if (r == DialogResult.OK)
                                PLC.Writeplc(tb_ErrorCode.Text, 1);
                            return false;
                        }
                        else
                        {
                            if (xyLyInfor(barcode))
                                return true;
                            else
                                return false;
                        }
                    }
                    for (int i = 1; i < dgv_result.Rows.Count; i++)
                    {
                        if (i % 15 == 0)
                        {
                            dgv_result.FirstDisplayedScrollingRowIndex = i;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void saveConfig(ComboBox cb_SR1, ComboBox cb_SR2,
                               TextBox tb_IP, TextBox tb_read_SR1, TextBox tb_read_SR2,
                               TextBox tb_readStart, TextBox ready, TextBox tb_OK,
                               TextBox tb_NG, TextBox tb_readEnd, TextBox tb_Model_cur,
                               TextBox tb_Pos_cur, TextBox tb_tray, TextBox tb_err, TextBox link_FCT, TextBox link_Local, TextBox Cycle
                                )
        {
            FileStream FS = new FileStream(Application.StartupPath + @"\Device.ini", FileMode.Create);
            StreamWriter SW = new StreamWriter(FS);

            SW.WriteLine("SR1=" + cb_SR1.Text);
            SW.WriteLine("SR2=" + cb_SR2.Text);
            SW.WriteLine("IP=" + tb_IP.Text);
            SW.WriteLine("Read_SR1=" + tb_ReadingSR1.Text);
            SW.WriteLine("Read_SR2=" + tb_ReadingSR2.Text);
            SW.WriteLine("Read_Start=" + tb_readStart.Text);
            SW.WriteLine("PC Ready=" + ready.Text);
            SW.WriteLine("Read OK=" + tb_OK.Text);
            SW.WriteLine("Read NG=" + tb_NG.Text);
            SW.WriteLine("Read End=" + tb_readEnd.Text);
            SW.WriteLine("Model Cur=" + tb_Model_cur.Text);
            SW.WriteLine("Pos Cur=" + tb_Pos_cur.Text);
            SW.WriteLine("Tray Ready=" + tb_tray.Text);
            SW.WriteLine("Error Code=" + tb_err.Text);
            SW.WriteLine("FCT=" + link_FCT.Text);
            SW.WriteLine("Local=" + tb_Local.Text);
            SW.WriteLine("Cycle=" + Cycle.Text);
            SW.Close();
            FS.Close();
        }

        public void loadConfig(ComboBox cb_SR1, ComboBox cb_SR2,
                               TextBox tb_IP, TextBox tb_read_SR1, TextBox tb_read_SR2,
                               TextBox tb_readStart, TextBox ready, TextBox tb_OK,
                               TextBox tb_NG, TextBox tb_readEnd, TextBox tb_Model_cur,
                               TextBox tb_Pos_cur, TextBox tb_tray, TextBox tb_err, TextBox link_FCT, TextBox link_Local, TextBox Cycle)
        {
            string[] data = null;
            string str;
            FileStream FS = new FileStream(@Application.StartupPath + @"\Device.ini", FileMode.Open);
            StreamReader SR = new StreamReader(FS);
            while (SR.EndOfStream == false)
            {
                str = SR.ReadLine();
                data = str.Split('=');

                switch (data[0])
                {
                    case "SR1":
                        cb_SR1.Text = data[1];
                        break;
                    case "SR2":
                        cb_SR2.Text = data[1];
                        break;
                    case "IP":
                        tb_IP.Text = data[1];
                        break;
                    case "Read_SR1":
                        tb_read_SR1.Text = data[1];
                        break;
                    case "Read_SR2":
                        tb_read_SR2.Text = data[1];
                        break;
                    case "Read_Start":
                        tb_readStart.Text = data[1];
                        break;
                    case "PC Ready":
                        ready.Text = data[1];
                        break;
                    case "Read OK":
                        tb_OK.Text = data[1];
                        break;
                    case "Read NG":
                        tb_NG.Text = data[1];
                        break;
                    case "Read End":
                        tb_readEnd.Text = data[1];
                        break;
                    case "Model Cur":
                        tb_Model_cur.Text = data[1];
                        break;
                    case "Pos Cur":
                        tb_Pos_cur.Text = data[1];
                        break;
                    case "Tray Ready":
                        tb_tray.Text = data[1];
                        break;
                    case "Error Code":
                        tb_err.Text = data[1];
                        break;
                    case "FCT":
                        link_FCT.Text = data[1];
                        break;
                    case "Local":
                        link_Local.Text = data[1];
                        break;
                    case "Cycle":
                        Cycle.Text = data[1];
                        break;
                    default:
                        break;
                }
            }
            SR.Close();
            FS.Close();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            //saveConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);
            PLC.Writeplc(tb_PCready.Text, 0);
            Thread.Sleep(200);
            Scaner1.ngatketnoi();
            Scaner2.ngatketnoi();
            Ping.Abort();
            Application.ExitThread();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string data = Scaner1.Data;
            string data1 = Scaner2.Data;
            string data3 = Scaner2.Data.Substring(0, 14);
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            saveConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);
        }

        public void scan2(int vTri)
        {
            try
            {
                PLC.Writeplc(tb_ReadingSR2.Text, 1);
                Thread.Sleep(1000);
                PLC.Writeplc(tb_ReadingSR2.Text, 0);
                Thread.Sleep(500);
                string code = string.Empty;
                code = Scaner2.Data;
                if (code == "ERROR" || code == "" || code == "12345679" || code.Length < 14)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        PLC.Writeplc(tb_ReadingSR2.Text, 1);
                        Thread.Sleep(500);
                        PLC.Writeplc(tb_ReadingSR2.Text, 0);
                        code = Scaner2.Data;
                        if (code != "ERROR" && code != "" && code != "12345679" && code.Length >= 14)
                        {
                            tb_barcode.Text = code.Substring(0, 14);
                            if (checkCode(tb_barcode, int.Parse(PLC.readplc(tb_PosCur.Text))))
                            {
                                PLC.Writeplc(tb_ReadOK.Text, 1);
                                break;
                            }
                        }
                        if (i == 1)
                        {
                            if (code == "ERROR" || code == "" || code == "12345679" || code.Length < 14)
                            {
                                tb_PCMNG.Text += "PCM" + vTri.ToString() + ",";
                                PLC.Writeplc(tb_ReadNG.Text, 1);
                                DialogResult rst = MessageBox.Show("PCM có code bị lỗi, hãy kiểm tra lại", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                if (rst == DialogResult.OK)
                                {
                                    PLC.Writeplc(tb_ErrorCode.Text, 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    tb_barcode.Text = code.Substring(0, 14);
                    if (checkCode(tb_barcode, int.Parse(PLC.readplc(tb_PosCur.Text))))
                    {
                        Thread.Sleep(200);
                        PLC.Writeplc(tb_ReadOK.Text, 1);
                        check_CodeLot = 0;
                    }
                    else
                    {
                        tb_barcode.Clear();
                        tb_PCMNG.Text += "PCM" + vTri.ToString() + ",";
                    }
                }
            }
            catch (Exception )
            {
                PLC.Writeplc(tb_ReadNG.Text, 1);
                DialogResult rs = MessageBox.Show("Xảy ra lỗi, nhấn reset trên PLC để tiếp tục", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (rs == DialogResult.OK)
                {
                    PLC.Writeplc(tb_ErrorCode.Text, 1);
                }
            }
        }

        public void scan1(int vTri)
        {
            try
            {
                PLC.Writeplc(tb_ReadingSR1.Text, 1);
                Thread.Sleep(1000);
                PLC.Writeplc(tb_ReadingSR1.Text, 0);
                Thread.Sleep(500);
                string code = Scaner1.Data;
                if (code == "ERROR" || code == "" || code == "12345679" || code.Length < 14)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        PLC.Writeplc(tb_ReadingSR1.Text, 1);
                        Thread.Sleep(500);
                        PLC.Writeplc(tb_ReadingSR1.Text, 0);
                        code = Scaner1.Data;
                        tb_barcode.Text = code.Substring(0, 14);
                        if (code != "ERROR" && code != "" && code != "12345679" && code.Length >= 14)
                        {
                            tb_barcode.Text = code.Substring(0, 14);
                            if (checkCode(tb_barcode, int.Parse(PLC.readplc(tb_PosCur.Text))))
                            {
                                PLC.Writeplc(tb_ReadOK.Text, 1);
                                break;
                            }
                        }

                        if (i == 1)
                        {
                            if (code == "ERROR" || code == "" || code == "12345679" || code.Length < 14)
                            {
                                tb_PCMNG.Text += "PCM" + vTri.ToString() + ",";
                                PLC.Writeplc(tb_ReadNG.Text, 1);
                                DialogResult rst = MessageBox.Show("PCM có code bị lỗi, hãy kiểm tra lại", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                if (rst == DialogResult.OK)
                                {
                                    PLC.Writeplc(tb_ErrorCode.Text, 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    tb_barcode.Text = code.Substring(0, 14);
                    if (checkCode(tb_barcode, int.Parse(PLC.readplc(tb_PosCur.Text))))
                    {
                        Thread.Sleep(200);
                        PLC.Writeplc(tb_ReadOK.Text, 1);
                        check_CodeLot = 0;
                    }
                    else
                    {
                        tb_PCMNG.Text += "PCM" + vTri.ToString() + ",";
                        tb_barcode.Clear();
                    }
                }
            }
            catch (Exception)
            {
                DialogResult rs = MessageBox.Show("Xảy ra lỗi, nhấn reset trên PLC để tiếp tục", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (rs == DialogResult.OK)
                {
                    PLC.Writeplc(tb_ErrorCode.Text, 1);
                }
            }
        }

        bool check = true;
        bool mess = true;
        public void ping_Server()
        {
            while (true)
            {
                if (DateTime.Now.Second % 5 == 0 && check == true)
                {
                    try
                    {
                        check = false;
                        if (Directory.Exists(@tb_FCT6.Text))
                        {
                            lbl_Function.BackColor = Color.Green;
                            tb_FCT6.BackColor = Color.Green;
                        }
                        else
                        {
                            lbl_Function.BackColor = Color.Red;
                            tb_FCT6.BackColor = Color.Red;
                            if (mess == true)
                            {
                                mess = false;
                                DialogResult rs = MessageBox.Show("Không thể kết nối đến máy function\r\nThông tin PE, ME để xử lí!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        if (Directory.Exists(@tb_Local.Text))
                        {
                            tb_Local.BackColor = Color.Green;
                        }
                        else
                        {
                            tb_Local.BackColor = Color.Red;
                        }
                    }
                    catch (Exception)
                    {
                        lbl_Function.BackColor = Color.Red;
                        tb_FCT.BackColor = Color.Red;
                    }
                    check = true;
                }
            }        
        }

        List<string> Data;
        List<string> PO;
        List<string> _Model;
        public void WarningBoxing(bool Day_night)// Day = true, Night = false
        {
            PO = new List<string>();
            checbox.LoadList("PO", ref PO);

            _Model = new List<string>();
            

            string[] model = dtb.get_model("6");
            string Model = "";
            string shift = "";
            if(Day_night)
            {
                shift = "Day";
            }
            else
            {
                shift = "Night";
            }

            int Total_Boxing = 0;
            int PCM_Good = 0;
            int k = 0;
            DateTime _timecheck = DateTime.Now;
            for (int tp = 0; tp < model.Length; tp++)
            {
                Total_Boxing = 0;
                PCM_Good = 0;
                string[] compare = new string[5000];
                if (model[tp].ToUpper().Contains("MAIN"))
                {                    
                    Data = new List<string>();
                    checbox.LoadList("MAIN", ref Data);
                    Model = "MAIN";                    
                }

                if (model[tp].ToUpper().Contains("CELL"))
                {
                    Data = new List<string>();
                    checbox.LoadList("CELL", ref Data);
                    Model = "CELL";
                }

                if (model[tp].ToUpper().Contains("SUB"))
                {
                    Data = new List<string>();
                    checbox.LoadList("SUB", ref Data);
                    Model = "SUB";
                }

                string[] time = new string[Data.Count];
                string[] data = new string[Data.Count];
                string[] code = new string[Data.Count];

                for (int i = 0; i < Data.Count; i++)
                {
                    DateTime dt;
                    data[i] = Data[i];
                    string[] tmp = data[i].Split('\t');
                    time[i] = tmp[2];
                    dt = DateTime.Parse(time[i]);               
                    
                    if (dt.AddDays(4).Day == _timecheck.Day || dt.AddDays(3).Day == _timecheck.Day || dt.AddDays(2).Day == _timecheck.Day || dt.AddDays(1).Day == _timecheck.Day || dt.Day == _timecheck.Day)
                    {
                        if (dt.AddDays(4).Day == _timecheck.Day)
                        {
                            if (dt.AddDays(4).Hour >= 8)
                            {
                                compare[k] = data[i];
                                k++;
                            }
                        }

                        if (dt.Day == _timecheck.Day)
                        {
                            if (dt.Hour < 8)
                            {
                                compare[k] = data[i];
                                k++;
                            }
                        }

                        else
                        {
                            compare[k] = data[i];
                            k++;
                        }    
                    }                                    
                }               
                
                for (int i = 0; i < compare.Length; i++)
                {
                    if (compare[i] != "" && compare[i] != null)
                    {
                        string[] tmp = compare[i].Split('\t');
                        code[i] = tmp[1];
                        if (xylycode(compare, code[i]))
                        {
                            PCM_Good++;
                        }
                    }
                }

                foreach (string _data in PO)
                {
                    if (Model == "CELL")
                    {
                        if (_data.ToUpper().Contains("CELL"))
                        {
                            string[] value = _data.Split('\t');
                            DateTime _now = DateTime.Parse(value[1]);
                            if (_now.AddDays(4).Day == _timecheck.Day || _now.AddDays(3).Day == _timecheck.Day || _now.AddDays(2).Day == _timecheck.Day || _now.AddDays(1).Day == _timecheck.Day || _now.Day == _timecheck.Day)
                            {
                                if (_now.AddDays(4).Day == _timecheck.Day)
                                {
                                    if (_now.Hour >= 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else if (_now.Day == _timecheck.Day)
                                {
                                    if (_now.Hour < 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else
                                    Total_Boxing += int.Parse(value[3]);
                            }
                        }
                    }

                    if (Model == "MAIN")
                    {
                        if (_data.ToUpper().Contains("MAIN"))
                        {
                            string[] value = _data.Split('\t');
                            DateTime _now = DateTime.Parse(value[1]);
                            if (_now.AddDays(4).Day == _timecheck.Day || _now.AddDays(3).Day == _timecheck.Day || _now.AddDays(2).Day == _timecheck.Day || _now.AddDays(1).Day == _timecheck.Day || _now.Day == _timecheck.Day)
                            {
                                if (_now.AddDays(4).Day == _timecheck.Day)
                                {
                                    if (_now.Hour >= 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else if (_now.Day == _timecheck.Day)
                                {
                                    if (_now.Hour < 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else
                                    Total_Boxing += int.Parse(value[3]);
                            }
                        }
                    }

                    if (Model == "SUB")
                    {
                        if (_data.ToUpper().Contains("SUB"))
                        {
                            string[] value = _data.Split('\t');
                            DateTime _now = DateTime.Parse(value[1]);
                            if (_now.AddDays(4).Day == _timecheck.Day || _now.AddDays(3).Day == _timecheck.Day || _now.AddDays(2).Day == _timecheck.Day || _now.AddDays(1).Day == _timecheck.Day || _now.Day == _timecheck.Day)
                            {
                                if (_now.AddDays(4).Day == _timecheck.Day)
                                {
                                    if (_now.Hour >= 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else if (_now.Day == _timecheck.Day)
                                {
                                    if (_now.Hour < 8)
                                        Total_Boxing += int.Parse(value[3]);
                                }
                                else
                                    Total_Boxing += int.Parse(value[3]);
                            }
                        }
                    }                        
                }

                if (PCM_Good > 0)
                {
                    if (Total_Boxing / PCM_Good >= 0.9)
                    {

                    }
                    else
                    {
                        checbox.SaveList("Từ ngày " + _timecheck.AddDays(-4).Day.ToString("00") + "-" + _timecheck.AddDays(-4).Month.ToString("00") +
                                         " Đến ngày " + _timecheck.Day.ToString("00") + "-" + _timecheck.Month.ToString("00") +
                                         "\r\nModel " + Model + ": Có PO không thực hiện boxing hoặc boxing không đủ PO", "Error");
                    }
                }               
            }           

            List<string> Error = new List<string>();
            checbox.LoadList("Error", ref Error);
            //listBox_Error.Items.Clear();
            listBox_Error.DataSource = Error;
        }

        public string checkPO(bool Shift, string model) // Day = true,, Night = false
        {
            bool checkPO = false;
            DateTime dt = DateTime.Now;
            List<string> PO = new List<string>();
            checbox.LoadList("PO", ref PO);
            string POname = "";
            string Log = "";
            string data_Return = "";
            if(Shift)
            {
                POname = dt.Year.ToString("00") + dt.Month.ToString("00") + dt.Day.ToString("00") + "_Day_" + model + "_6";
                Log = dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + "_Day.csv";
            }
            else
            {
                POname = dt.Year.ToString("00") + dt.Month.ToString("00") + dt.Day.ToString("00") + "_Night_" + model + "_6";
                Log = dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + "_Night.csv";
            }
            int Qty_Boxed = 0;
            for (int i = 0; i < PO.Count; i++)
            {
                if (PO[i].Contains(POname))
                {
                    string[] tmp = PO[i].Split('\t');
                    Qty_Boxed = int.Parse(tmp[1]);
                    data_Return = Qty_Boxed.ToString();
                    checkPO = true;
                }
                else
                {
                    checkPO = false;
                }
            }

            int PCM = 0;
            if (!checkPO)
            {
                int tmp = 0;
                string link_Log = @Application.StartupPath + "\\Result\\" + model + "\\" + dt.Year.ToString() + "-" + dt.Month.ToString("00");
                string[] File = Directory.GetFiles(link_Log);
                for (int j = 0; j < File.Length; j++)
                {
                    if (File[j] == link_Log + "\\" + Log)
                    {
                        tmp++;
                    }
                }
                if (tmp == 0)
                {
                    return "0";
                }
                else
                {
                    string qtyPO = "";
                    DataTable table = Excel.ReadCsvFile(link_Log + "\\" + Log, CreateTable("", "", "", "", "", "", "", "", "", "", ""));
                    foreach (DataRow dr in table.Rows)
                    {
                        PCM++;
                        qtyPO = dr.ItemArray[10].ToString();
                    }
                    return PCM.ToString() + "-" + qtyPO;
                }
            }
            else
            {
                return data_Return;
            }
        }

        public bool xylycode(string[] data, string code)
        {
            List<string> _code = new List<string>();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != "" && data[i] != null)
                {
                    if (data[i].Contains(code))
                    {
                        _code.Add(data[i]);
                    }
                }               
            }

            if(_code.Count == 1)
            {
                string[] tmp = _code[0].Split('\t');
                string Result = tmp[0];
                if(Result == "OK")
                {
                    return true;   
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string[] Result = new string[_code.Count];
                string[] Code = new string[_code.Count];
                string[] Time = new string[_code.Count];
                for(int i=0; i< _code.Count;i++)
                {
                    string[] tmp = _code[i].Split('\t');
                    Result[i] = tmp[0];
                    Code[i] = tmp[1];
                    Time[i] = tmp[2];
                }

                DateTime newTime = DateTime.Parse(Time[0]);
                if(Time.Length == 2)
                {
                    if(Result[0] == "OK" && Result[1] == "OK")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    string a = "";
                    string b = "";
                    for(int i = 0 ; i< Time.Length;i++)
                    {
                        for(int j = i + 1; j < Time.Length;j++)
                        {
                            if(DateTime.Parse(Time[i]) < DateTime.Parse(Time[j]))
                            {
                                b = Time[i];
                                Time[i] = Time[j];
                                Time[j] = b;
                                a = Result[i];
                                Result[i] = Result[j];
                                Result[j] = a;
                            }
                        }
                    }
                    if(Result[0] == "OK" && Result[1] == "OK")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }                
            }
        }

        public void upLoadData()
        {
            DateTime dt = DateTime.Now;
            try
            {
                string[] model_FCT = File.ReadAllLines(@Application.StartupPath + @"\Model_FCT.txt");
                for (int i = 0; i < model_FCT.Length; i++)
                {
                    try
                    {
                        //if (model_FCT[i].ToUpper().Contains("MAIN"))
                        //{
                        //    File.Delete(@Application.StartupPath + "\\Log\\Duplicate\\MAIN.log");
                        //}

                        if (model_FCT[i].ToUpper().Contains("CELL"))
                        {
                            File.Delete(@Application.StartupPath + "\\Log\\Duplicate\\CELL.log");
                        }

                        //if (model_FCT[i].ToUpper().Contains("SUB"))
                        //{
                        //    File.Delete(@Application.StartupPath + "\\Log\\Duplicate\\SUB.log");
                        //}
                    }
                    catch (Exception)
                    {
                        ;
                    }

                    string link = "";
                    if (lbl_Function.BackColor == Color.Green)
                    {
                        link = tb_FCT6.Text;
                    } 
                    else
                    {
                        link = tb_Local.Text;
                    }
                    string link_1 = link + "\\" + model_FCT[i] + "\\" + dt.Year.ToString() + "-" + dt.Month.ToString("00");
                    string link_2 = "";
                    if (dt.Month == 1)
                        link_2 = link + "\\" + model_FCT[i] + "\\" + dt.AddYears(-1).Year.ToString() + "-12";
                    else
                        link_2 = link + "\\" + model_FCT[i] + "\\" + dt.Year.ToString() + "-" + dt.AddMonths(-1).Month.ToString("00");

                    if (Directory.Exists(@link_1))
                    {
                        string[] File_1 = Directory.GetFiles(@link_1);
                        for (int j = 0; j < File_1.Length; j++)
                        {
                            DataTable table = new DataTable();
                            table = ReadCsvFile(File_1[j]);
                            string[] Total_result = new string[table.Rows.Count];
                            string[] Chanel = new string[table.Rows.Count];
                            string[] testTime = new string[table.Rows.Count];
                            string[] barcode = new string[table.Rows.Count];
                            int k = 0;

                            foreach (DataColumn Dl in table.Columns)
                            {
                                if (Dl.ColumnName.ToString().Contains("Total Result"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            Total_result[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Barcode(SN)"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            barcode[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Test Channel"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            Chanel[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Test Time"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            testTime[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }
                                k++;
                            }
                            for (int tmp = 0; tmp < testTime.Length - 1; tmp++)
                            {
                                if (Total_result[tmp] != null && barcode[tmp] != null && testTime[tmp] != null && Chanel[tmp] != null && Total_result[tmp] != "" && barcode[tmp] != "" && testTime[tmp] != "" && Chanel[tmp] != "" && Total_result[tmp] != "STOP")
                                {
                                    if (model_FCT[i].ToUpper().Contains("CELL"))
                                    {
                                        checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "CELL");
                                    }
                                    //if (model_FCT[i].ToUpper().Contains("MAIN"))
                                    //{
                                    //    checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "MAIN");
                                    //}
                                    //if (model_FCT[i].ToUpper().Contains("SUB"))
                                    //{
                                    //    checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "SUB");
                                    //}
                                }
                            }
                        }
                    }

                    if (Directory.Exists(@link_2))
                    {
                        string[] File_2 = Directory.GetFiles(@link_2);

                        for (int j = 0; j < File_2.Length; j++)
                        {
                            DataTable table = new DataTable();
                            table = ReadCsvFile(File_2[j]);
                            string[] Total_result = new string[table.Rows.Count];
                            string[] Chanel = new string[table.Rows.Count];
                            string[] testTime = new string[table.Rows.Count];
                            string[] barcode = new string[table.Rows.Count];
                            int k = 0;

                            foreach (DataColumn Dl in table.Columns)
                            {
                                if (Dl.ColumnName.ToString().Contains("Total Result"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            Total_result[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Barcode(SN)"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            barcode[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Test Channel"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            Chanel[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }

                                if (Dl.ColumnName.ToString().Contains("Test Time"))
                                {
                                    int temp = 0;
                                    foreach (DataRow dr in table.Rows)
                                    {
                                        if (temp < table.Rows.Count - 1)
                                        {
                                            testTime[temp] = dr.ItemArray[k].ToString();
                                            temp++;
                                        }
                                    }
                                }
                                k++;
                            }
                            for (int tmp = 0; tmp < testTime.Length - 1; tmp++)
                            {
                                if (Total_result[tmp] != null && barcode[tmp] != null && testTime[tmp] != null && Chanel[tmp] != null && Total_result[tmp] != "" && barcode[tmp] != "" && testTime[tmp] != "" && Chanel[tmp] != "" && Total_result[tmp] != "STOP")
                                {
                                    if (model_FCT[i].ToUpper().Contains("CELL"))
                                    {
                                        checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "CELL");
                                    }
                                    //if (model_FCT[i].ToUpper().Contains("MAIN"))
                                    //{
                                    //    checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "MAIN");
                                    //}
                                    //if (model_FCT[i].ToUpper().Contains("SUB"))
                                    //{
                                    //    checbox.SaveList(Total_result[tmp] + "\t" + barcode[tmp] + "\t" + testTime[tmp] + "\t" + Chanel[tmp], "SUB");
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
            stop = false;
            check_upload = true;
        }

        public DataTable ReadCsvFile(string path)
        {
            DataTable dtb = new DataTable();
            try
            {
                string Fulltext;
                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString();
                        string[] rows = Fulltext.Split('\r');

                        for (int i = 5; i < rows.Length; i++)
                        {
                            string[] rowValues = rows[i].Split(',');

                            if (i == 5)
                            {
                                for (int j = 0; j < rowValues.Count(); j++)
                                {
                                    dtb.Columns.Add(rowValues[j]);
                                }
                            }
                            else if (i == 6 || i == 7 || i == 8 || i == 9)
                            {
                                ;
                            }
                            else
                            {
                                DataRow dr = dtb.NewRow();
                                for (int k = 0; k < rowValues.Count(); k++)
                                {
                                    dr[k] = rowValues[k].ToString();
                                }
                                dtb.Rows.Add(dr);
                            }
                        }
                    }
                }
                return dtb;
            }
            catch (Exception)
            {
                return dtb;
            }
        }

        private void btn_LoadManual_Click(object sender, EventArgs e)
        {
            upload = new Thread(new ThreadStart(upLoadData));
            upload.Start();
        }

        private void cb_FCT6_SelectedIndexChanged(object sender, EventArgs e)
        {
            tb_Modelname.Text = cb_FCT6.Text;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            string[] model = new string[3];
            model = File.ReadAllLines(@Application.StartupPath + "\\Model_FCT.txt");
            for (int i = 0; i < model.Length; i++)
            {
                if (model[i] == cb_FCT6.Text)
                {
                    model[i] = tb_Modelname.Text;
                }
            }
            File.WriteAllLines(@Application.StartupPath + "\\Model_FCT.txt", model);
            model = File.ReadAllLines(@Application.StartupPath + "\\Model_FCT.txt");
            cb_FCT6.Items.Clear();
            cb_FCT6.Items.AddRange(model);
            MessageBox.Show("Sửa model thành công");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            upLoadData();
        }

        private void tb_barcode_TextChanged(object sender, EventArgs e)
        {

        }

        public void RequestWarningDay()
        {
            WarningBoxing(true);
            check_PO = true;
        }

        public void RequestWarningNight()
        {
            WarningBoxing(false);
            check_PO = true;
        }

        private Thread Warning;
        private void button4_Click(object sender, EventArgs e)
        {
            Warning = new Thread(new ThreadStart(RequestWarningDay));
            Warning.Start();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            saveConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);      
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveConfig(cbb_SR1, cbb_SR2, tb_PLC, tb_ReadingSR1, tb_ReadingSR2, tb_ReadStart, tb_PCready, tb_ReadOK, tb_ReadNG, tb_ReadEnd, tb_ModelCur, tb_PosCur, tb_TrayReady, tb_ErrorCode, tb_FCT6, tb_Local, tb_Cycle);        
        }

        public int countTime = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            if(MES_Connecting == "CAN" && cntIns == true)
            {
                lbl_MesIns.BackColor = Color.Green;
            }
            else
            {
                lbl_MesIns.BackColor = Color.Red;
            }

            //countTime++;
            //if(countTime == 300)
            //{
            //    CompareLog();
            //    countTime = 0;
            //}
        }

        private void chb_noMes_CheckedChanged(object sender, EventArgs e)
        {
            if(chb_noMes.Checked == false)
            {
                cntIns = true;
            }
            else
            {
                cntIns = false;
            }
        }         
   
        public void MesBarcodePCM_Ins()
        {
            if ((rDt.ChekdoubleCode(tb_barcode.Text, "Code_Inspection_MES") == true)
                 && (rDt.CheckFormatCode(tb_barcode.Text, lbl_modelCod.Text) == true))
            {
                socketIns.Sent_Inspection_Gen2(tb_barcode.Text, listBoxsend);
                Thread.Sleep(500);
            }       
            else
            {
                MessageBox.Show("Sai format/ same code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool chk154 = false;
        private void chk_pasDlb_CheckedChanged(object sender, EventArgs e)
        {
            if(chk_pasDlb.Checked == true)
            {
                chk154 = true;
            }
            else
            {
                chk154 = false;
            }
        }   
        
        public void CompareLog()
        {
            //Boxing logfile
            string strSource = string.Empty;
            if (rbt_Day.Checked)
            {
                strSource = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Day.csv";
            }
            if (rbt_Night.Checked)
            {
                strSource = @Application.StartupPath + "\\Result\\" + cbb_model.Text + "\\" + DateTimeActual().Year.ToString("0000") + "-" + DateTimeActual().Month.ToString("00") + "\\" + DateTimeActual().Month.ToString("00") + "-" + DateTimeActual().Day.ToString("00") + "_Night.csv";
            }
            string[] arrSource = new string[int.Parse(tb_QtyPO.Text)];
            if(File.Exists(strSource) == true)
            {               
                int count_Source = 0;
                StreamReader srSource = new StreamReader(strSource);
                while (srSource.EndOfStream == false)
                {
                    string str1 = srSource.ReadLine();
                    string[] arrStr1 = str1.Split(',');
                    arrSource[count_Source] = arrStr1[3];
                    count_Source++;
                }
                srSource.Close();
            }            

            //Input logfile           
            string strDestination = GetPath();
            string[] arrDestination = new string[int.Parse(tb_QtyPO.Text)];
            if(File.Exists(strDestination) == true)
            {                
                int count_Destination = 0;
                StreamReader srDestination = new StreamReader(strDestination);
                while (srDestination.EndOfStream == false)
                {
                    string str2 = srDestination.ReadLine();
                    string[] arrStr2 = str2.Split('|');
                    string z = arrStr2[1].Substring(4, 2) + "/" + arrStr2[1].Substring(6, 2) + "/" + arrStr2[1].Substring(0, 4) + " " + arrStr2[1].Substring(8, 2) + ":" + arrStr2[1].Substring(10, 2) + ":" + arrStr2[1].Substring(12, 2);
                    int x = DateTime.Compare(Convert.ToDateTime(z), DateTime.Now.AddHours(-6));
                    if (DateTime.Compare(Convert.ToDateTime(z), DateTime.Now.AddHours(-6)) < 0)//input < boxing - 6h
                    {
                        arrDestination[count_Destination] = arrStr2[0];
                        count_Destination++;
                    }
                }
                srDestination.Close();
            }

            //So sánh
            if((File.Exists(strSource) == true) && (File.Exists(strDestination) == true))
            {
                string[] arrAlarm = new string[int.Parse(tb_QtyPO.Text)];
                int count_Alarm = 0;
                for (int i = 0; i < arrDestination.Length; i++)
                {
                    int same = 0;
                    for (int j = 0; j < arrSource.Length; j++)
                    {
                        if (arrDestination[i] == arrSource[j])//input logfile có trong boxing logfile
                        {
                            same++;
                        }

                        if (arrSource[j] == null)
                        {
                            break;
                        }
                    }

                    if (same == 0)//input ko có trong boxing
                    {
                        arrAlarm[count_Alarm] = arrDestination[i];
                        count_Alarm++;
                    }

                    if (arrDestination[i] == null)
                    {
                        break;
                    }
                }

                if (arrAlarm[0] != null)
                {
                    //string toDisply = string.Join("\n", arrAlarm);
                    string toDisply = string.Empty;
                    for (int i = 0; i < arrAlarm.Length; i++)
                    {
                        if (arrAlarm[i] != null)
                        {
                            if (i == 0)
                            {
                                toDisply += arrAlarm[i];
                            }
                            else
                            {
                                toDisply += "\n" + arrAlarm[i];
                            }
                        }

                    }
                    MessageBox.Show("Barcode PCM không FI-FO :\n" + toDisply, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }            
        }  
       
        public string GetPath()
        {
            string str = string.Empty;
            string subStr = string.Empty;
            StreamReader sr = new StreamReader(@Application.StartupPath + "\\LinkInput.txt");
            while(sr.EndOfStream == false)
            {
                subStr = sr.ReadLine();
            }
            sr.Close();
            string dt = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            string molNam = string.Empty;           
            if (cbb_model.Text.Contains("Main"))
            {
                molNam = "Composite Module_main";
            }
            else if (cbb_model.Text.Contains("Cell"))
            {
                molNam = "Cell Module 14S_21700";
            }
            else if (cbb_model.Text.Contains("Sub"))
            {
                molNam = "Composite Module_sub";
            }
            str = subStr + "\\" + molNam + "\\" + dt + "-OnMes.log"; 
            return str;
        }
    }
}
