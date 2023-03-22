using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.OleDb;
using System.IO;

namespace MatchingBarcodeBoxing
{
    public class database
    {
        public string constr = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source = " + Application.StartupPath + @"\Database.mdb";
        public string user;
        public OleDbConnection GetConnection()
        {
            OleDbConnection con = new OleDbConnection(constr);
            con.Open();
            return con;
        }

        public void delete(string sql_delete)
        {
            OleDbConnection cnn = GetConnection();

            OleDbCommand cmd = new OleDbCommand(sql_delete, cnn);
            cmd.ExecuteNonQuery();

            cnn.Close();
        }

        public DataTable getData(string str)
        {
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter(str, constr);
            da.Fill(dt);

            return dt;
        }

        public bool login_part(string user, string pass, string part)
        {
            string str = "select Account, Password, Kind from LOGIN where Account = '" + user + "' and Password = '" +
                pass + "' and Part = '" + part + "'";

            DataTable dt = new DataTable();
            dt = getData(str);

            if (dt.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool login_admin(string user, string pass, string kind)
        {
            string str = "select Account, Password, Kind from LOGIN where Account = '" + user + "' and Password = '" + pass + "'";

            DataTable dt = new DataTable();
            dt = getData(str);

            foreach (DataRow dr in dt.Rows)
            {
                kind = dr.ItemArray[2].ToString();
            }

            if (dt.Rows.Count != 0 && kind == "admin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string get_name(string account)
        {
            string name = "";
            string str = "select FullName from LOGIN where Account = '" + account + "' ";
            DataTable dt = new DataTable();
            dt = getData(str);

            // Add users vào comboBox
            foreach (DataRow dr in dt.Rows)
            {
                name = dr.ItemArray[0].ToString();
            }
            return name;
        }

        public string get_QtyPO(string PO)
        {
            string name = "";
            string str = "Select Qty from PO where PO_name = '" + PO + "' ";
            DataTable dt = new DataTable();
            dt = getData(str);

            // Add users vào comboBox
            foreach (DataRow dr in dt.Rows)
            {
                name = dr.ItemArray[0].ToString();
            }
            return name;
        }

        public bool checkBarcode(string barcode)
        {
            string str = "select * from BARCODE_BOXING where BarcodeFu = '" + barcode + "'";

            DataTable dt = new DataTable();
            dt = getData(str);

            foreach (DataRow dr in dt.Rows)
            {
                if (dr.ItemArray[4].ToString() != "")
                {
                    return true;
                }
            }
            return false;
        }

        public DataTable getBarCode2(string barcode)
        {
            string str = "select * from BARCODE_BOXING where BarcodeBoxing = '" + barcode + "'";

            return getData(str);
        }

        public bool TimeBetween(DateTime time, DateTime startDateTime, DateTime endDateTime)
        {
            // get TimeSpan
            TimeSpan start = new TimeSpan(startDateTime.Hour, startDateTime.Minute, 0);
            TimeSpan end = new TimeSpan(endDateTime.Hour, endDateTime.Minute, 0);

            // convert datetime to a TimeSpan
            TimeSpan now = time.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }

        public string find_shift()
        {
            string shift;
            DateTime dateTime = DateTime.Now;

            DateTime startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 8, 0, 0);
            DateTime endDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 20, 0, 0);

            if (TimeBetween(dateTime, startDateTime, endDateTime))
            {
                shift = "Day";
            }
            else
            {
                shift = "Night";
            }
            return shift;
        }

        public DateTime find_day()
        {
            DateTime dateTime = DateTime.Now;

            DateTime startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 20, 0, 1);
            DateTime endDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);

            TimeSpan start = new TimeSpan(startDateTime.Hour, startDateTime.Minute, startDateTime.Second);
            TimeSpan end = new TimeSpan(endDateTime.Hour, endDateTime.Minute, endDateTime.Second);

            // convert datetime to a TimeSpan
            TimeSpan now = dateTime.TimeOfDay;

            if (start <= now && now <= end)
            {
                dateTime = DateTime.Now.AddDays(1);
            }
            else
            {
                dateTime = DateTime.Now;
            }
            return dateTime;
        }

        public DataTable loadBarCode(string model, bool getDate = true)
        {
            DateTime dateTime = DateTime.Now;

            DateTime startDateTimeDay = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 8, 0, 0);
            DateTime endDateTimeDay = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 20, 0, 0);

            DateTime startDateTimeNight = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 20, 0, 1);
            DateTime endDateTimeNight = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);

            DateTime dt = DateTime.Now.AddDays(-1);
            DateTime startDateTimeNight_2 = new DateTime(dt.Year, dt.Month, dt.Day, 20, 0, 1);
            DateTime endDateTimeNight_2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 7, 59, 00);

            string dtFrom;
            string dtTo;

            if (find_shift() == "Day")
            {
                dtFrom = startDateTimeDay.ToString("yyyy-MM-dd HH:mm:ss");
                dtTo = endDateTimeDay.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                if (dateTime >= startDateTimeNight && dateTime <= endDateTimeNight)
                {
                    dtFrom = startDateTimeNight.ToString("yyyy-MM-dd HH:mm:ss");
                    dtTo = endDateTimeNight.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    dtFrom = startDateTimeNight_2.ToString("yyyy-MM-dd HH:mm:ss");
                    dtTo = endDateTimeNight_2.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }


            //if (dateTime.Month > 0 && dateTime.Month < 10 && dateTime.Day > 0 && dateTime.Day < 10)
            //{
            //    dtFrom = dateTime.Year + "-0" + dateTime.Month + "-0" + dateTime.Day + " 00:00:01";
            //    dtTo = dateTime.Year + "-0" + dateTime.Month + "-0" + dateTime.Day + " 23:59:59";
            //}
            //else if (dateTime.Month > 0 && dateTime.Month < 10 && dateTime.Day >= 10)
            //{
            //    dtFrom = dateTime.Year + "-0" + dateTime.Month + "-" + dateTime.Day + " 00:00:01";
            //    dtTo = dateTime.Year + "-0" + dateTime.Month + "-" + dateTime.Day + " 23:59:59";
            //}
            //else if (dateTime.Day > 0 && dateTime.Day < 10 && dateTime.Month >= 10)
            //{
            //    dtFrom = dateTime.Year + "-" + dateTime.Month + "-0" + dateTime.Day + " 00:00:01";
            //    dtTo = dateTime.Year + "-" + dateTime.Month + "-0" + dateTime.Day + " 23:59:59";
            //}
            //else
            //{
            //    dtFrom = dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day + " 00:00:01";
            //    dtTo = dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day + " 23:59:59";
            //}

            string str = "";

            if (getDate)
            {
                str = "select * from BARCODE_BOXING where Model = '" + model + "' and TimeBoxing >= '"
                    + dtFrom + "' and TimeBoxing <= '" + dtTo + "' order by TimeBoxing DESC";
            }
            else
            {
                str = "select * from BARCODE_BOXING where Model = '" + model + "' and BarcodeBoxing = '" + "" + "'";
            }

            return getData(str);
        }

        public DataTable getBarCode(string model)
        {
            string str = "select BarcodeFu from BARCODE_BOXING where Model = '" + model + "'";

            return getData(str);
        }

        public void get_model(ComboBox cbb, string line)
        {
            string str = "select Model from MODEL where Line = '" + line + "'";
            DataTable dt = new DataTable();
            dt = getData(str);

            // Add users vào comboBox
            foreach (DataRow dr in dt.Rows)
            {
                if (!cbb.Items.Contains(dr.ItemArray[0].ToString()))
                {
                    cbb.Items.Add(dr.ItemArray[0].ToString());
                }
            }
        }

        public string[] get_model(string line)
        {
            string str = "select Model from MODEL where Line = '" + line + "'";
            DataTable dt = new DataTable();
            dt = getData(str);
            string[] data = new string[3];
            int tmp = 0;
            // Add users vào comboBox
            foreach (DataRow dr in dt.Rows)
            {
                data[tmp] = dr.ItemArray[0].ToString();
                tmp++;
            }
            return data;
        }

        public void getPO(ComboBox cbb, string model, string line)
        {
            string str = "Select PO_name from PO";
            DataTable dt = new DataTable();
            dt = getData(str);

            // Add users vào comboBox
            foreach (DataRow dr in dt.Rows)
            {
                if (!cbb.Items.Contains(dr.ItemArray[0].ToString()))
                {
                    cbb.Items.Add(dr.ItemArray[0].ToString());
                }
            }
        }

        public void delete_PO(string PO)
        {
            string cmd = "Delete from PO where PO_name = '" + PO + "'";
            delete(cmd);
        }

        public void CreatePO(string Line, string model, string date, string shift , string Qty, bool OBA)
        {
            try
            {
                OleDbConnection cnn = new OleDbConnection(constr);
                cnn.Open();
                string str;
                if (!OBA)
                {
                     str = "INSERT INTO PO VALUES ('" + "OBA_" + date + "_" + shift + "_" + model + "_" + Line + "', '" + model + "','" + Line + "','" + Qty + "')";                
                }
                else
                    str = "INSERT INTO PO VALUES ('" + date + "_" + shift + "_" + model + "_" + Line + "', '" + model + "','" + Line + "','" + Qty + "')";                
                OleDbCommand cmd = new OleDbCommand(str, cnn);
                cmd.ExecuteNonQuery();
                cnn.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Không tạo được PO");
            }
        }

        public bool checkPO(string Line, string model, string date, string shift)
        {
            try
            {
                string _str = "Select *from PO where PO_name = '" + date + "_" + shift + "_" + model + "_" + Line + "'";
                DataTable dt = getData(_str);
                if (dt.Rows.Count != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch(Exception)
            {
                return false;
            }            
        }

        public DataTable GetData2(string str)
        {
            DataTable dta1 = new DataTable();
            OleDbDataAdapter da1 = new OleDbDataAdapter(str, @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source =" + Application.StartupPath + @"\MesMsg.mdb");
            da1.Fill(dta1);
            return dta1;
        }

        public OleDbConnection GetConnection2()
        {
            OleDbConnection con = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source =" + Application.StartupPath + @"\MesMsg.mdb");
            con.Open();
            return con;
        }

        public string GetMesErr(string errCode)
        {
            string str = string.Empty;
            string strSle = "Select Info_en from Message where Msg_ID = '" + errCode + "'";

            DataTable dt = new DataTable();
            dt = GetData2(strSle);

            foreach (DataRow dtr in dt.Rows)
            {
                str = dtr.ItemArray[0].ToString();
            }
            return str;
        }        
    }
}
