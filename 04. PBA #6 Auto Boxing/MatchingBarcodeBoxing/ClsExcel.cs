using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;

using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
namespace MatchingBarcodeBoxing
{
    class ClsExcel
    {
        public void Export_CSV(DataTable dt, string path, bool check, string ColumnName)
        {
            StringBuilder sb = new StringBuilder();

            if (check == false)
            {
                sb.Append(ColumnName);
            }
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.ItemArray[0].ToString() != "")
                {
                    foreach (DataColumn dc in dt.Columns)
                        sb.Append(FormatCSV(dr[dc.ColumnName].ToString()) + ",");
                    sb.Remove(sb.Length - 1, 1);
                    sb.AppendLine();
                }
                
            }
            if (check == false)
            {               
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            else
                File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public void Export_CS(DataTable dt, string path, string ColumnName)
        {
            StringBuilder sb = new StringBuilder();           
            sb.Append(ColumnName);
            
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn dc in dt.Columns)
                    sb.Append(FormatCSV(dr[dc.ColumnName].ToString()) + ",");
                sb.Remove(sb.Length - 1, 1);
                sb.AppendLine();
            }            
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            
        }

        public void Export_CSV_1(DataTable dt, string path, bool check, string ColumnName)
        {
            StringBuilder sb = new StringBuilder();

            if (check == false)
            {
                sb.Append(ColumnName);
            }
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.ItemArray[0].ToString() != "")
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        sb.Append(FormatCSV(dr[dc.ColumnName].ToString()) + ",");
                        
                    }                        
                    
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append('\n');
                }
                
            }
            if (check == false)
            {               
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            }
            else
                File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
        }

        //public void Export_CS(DataTable dt, string path, string ColumnName)
        ////{
        //    StringBuilder sb = new StringBuilder();           
        //    sb.Append(ColumnName);
            
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        foreach (DataColumn dc in dt.Columns)
        //            sb.Append(FormatCSV(dr[dc.ColumnName].ToString()) + ",");
        //        sb.Remove(sb.Length - 1, 1);
        //       // sb.AppendLine();
        //    }            
        //        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            
        //}
        public static string FormatCSV(string input)
        {
            try
            {
                if (input == null)
                    return string.Empty;

                bool containsQuote = false;
                bool containsComma = false;
                int len = input.Length;
                for (int i = 0; i < len && (containsComma == false || containsQuote == false); i++)
                {
                    char ch = input[i];
                    if (ch == '"')
                        containsQuote = true;
                    else if (ch == ',')
                        containsComma = true;
                }

                if (containsQuote && containsComma)
                    input = input.Replace("\"", "\"\"");

                if (containsComma)
                    return "\"" + input + "\"";
                else
                    return input;
            }
            catch
            {
                throw;
            }
        }

        public DataTable ReadCsvFile(string path, DataTable dtb)
        {
           // DataTable dtb = new DataTable();
            string Fulltext;
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString();
                    string[] rows = Fulltext.Split('\n');

                    for (int i = 0; i < rows.Length; i++)
                    {
                        string[] rowValues = rows[i].Split(',');

                        if (i == 0)
                        {
                            //for (int j = 0; j < rowValues.Count(); j++)
                            //{
                            //    dtb.Columns.Add(rowValues[j]);
                            //}
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

        public DataTable ReadCsvFile_1(string path, DataTable dtb)
        {
            // DataTable dtb = new DataTable();
            string Fulltext;
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString();
                    string[] rows = Fulltext.Split('\n');

                    for (int i = 0; i < rows.Length; i++)
                    {
                        string[] rowValues = rows[i].Split(',');

                        if (i == 0)
                        {
                            for (int j = 0; j < rowValues.Count(); j++)
                            {
                                dtb.Columns.Add(rowValues[j]);
                            }
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

        public void createlog(string path, DataGridView dtvspec, int count)
        {
            string pathlocal1;
            string data1 = "";

            pathlocal1 = path;

            if (!File.Exists(pathlocal1))
            {
                
            }
            FileStream fslocal1 = new FileStream(pathlocal1, FileMode.Append);
            StreamWriter swlocal1 = new StreamWriter(fslocal1, System.Text.Encoding.UTF8);          

            for (int irow = 0; irow <= (dtvspec.RowCount + 1); irow++)
            {
                if (irow < dtvspec.RowCount)
                {
                    data1 = dtvspec.Rows[irow].Cells[2].Value.ToString() + "\t";
                    swlocal1.Write(data1 + ",");
                }
                else if (irow == dtvspec.RowCount)
                {
                    data1 = "Ket Qua\t";
                    swlocal1.Write(data1 + ",");
                }
                else
                {
                    
                }

            }
            swlocal1.WriteLine("");           

            for (int irow = 0; irow < dtvspec.RowCount - 1; irow++)
            {
                data1 = dtvspec.Rows[irow].Cells[4].Value.ToString() + " ~ " + dtvspec.Rows[irow].Cells[5].Value.ToString() + "\t";
                swlocal1.Write(data1 + ",");
            }
            swlocal1.WriteLine(data1);           
            swlocal1.Flush();
            swlocal1.Close();        
            fslocal1.Close();      
        }

        public void createlog(string path, DataTable table, int count)
        {
            string pathlocal1;
            string data1 = "";

            pathlocal1 = path;

            if (!File.Exists(pathlocal1))
            {
                FileStream fslocal1 = new FileStream(pathlocal1, FileMode.Append);
                StreamWriter swlocal1 = new StreamWriter(fslocal1, System.Text.Encoding.UTF8);
                foreach (DataColumn dtcl in table.Columns)
                {
                    data1 = dtcl.ColumnName + "\t";
                    swlocal1.Write(data1 + ",");
                }
                foreach (DataRow dr in table.Rows)
                {
                    foreach (string item in dr.ItemArray)
                    {
                        data1 = item + "\t";
                        swlocal1.Write(data1 + ",");
                    }
                }
                swlocal1.WriteLine("");
                swlocal1.WriteLine(data1);
                swlocal1.Flush();
                swlocal1.Close();
                fslocal1.Close();
            }
            else
            {
                FileStream fslocal1 = new FileStream(pathlocal1, FileMode.Append);
                StreamWriter swlocal1 = new StreamWriter(fslocal1, System.Text.Encoding.UTF8);                
                foreach (DataRow dr in table.Rows)
                {
                    foreach (string item in dr.ItemArray)
                    {
                        data1 = item + "\t";
                        swlocal1.Write(data1 + ",");
                    }
                    swlocal1.WriteLine("");
                }
                //swlocal1.WriteLine("");
                //swlocal1.WriteLine(data1);
                swlocal1.Flush();
                swlocal1.Close();
                fslocal1.Close();
            }
        }
    }
}
