using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace MatchingBarcodeBoxing
{
    class clsReadData
    {
        public void SaveCode(string inp_Cod, string namFile)
        {
            FileStream fs = new FileStream(@Application.StartupPath + "\\Log\\Duplicate\\" + namFile + ".log", FileMode.Append);           
            StreamWriter sw = new StreamWriter(fs);            
            sw.WriteLine(inp_Cod);            
            sw.Close();
            fs.Close();
        }

        public bool ChekdoubleCode(string inp_Cod, string namFile)
        {
            if (File.Exists(@Application.StartupPath + "\\Log\\Duplicate\\" + namFile + ".log"))
            {
                int same = 0;
                StreamReader sr = new StreamReader(@Application.StartupPath + "\\Log\\Duplicate\\" + namFile + ".log");
                while (sr.EndOfStream == false)
                {
                    string strRead = sr.ReadLine();
                    if (strRead == inp_Cod)
                    {
                        same++;
                    }
                }
                sr.Close();

                if (same > 0)//trung
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public void ReadConfigMES(TextBox ipInp, TextBox linenoInp, TextBox mcidInp, TextBox stnidInp, TextBox portInp,
                                  TextBox ipIns, TextBox linenoIns, TextBox mcidIns, TextBox stnidIns, TextBox portIns,
                                  TextBox ipBx, TextBox linenoBx, TextBox mcidBx, TextBox stnidBx, TextBox portBx)
        {
            StreamReader sr = new StreamReader(@Application.StartupPath + "\\MesSetting.ini");
            while (sr.EndOfStream == false)
            {
                string[] str = sr.ReadLine().Split('=');
                switch (str[0])
                {
                    case "IP_Inp":
                        ipInp.Text = str[1];
                        break;
                    case "LineNo_Inp":
                        linenoInp.Text = str[1];
                        break;
                    case "MCID_Inp":
                        mcidInp.Text = str[1];
                        break;
                    case "StnID_Inp":
                        stnidInp.Text = str[1];
                        break;
                    case "Port_Inp":
                        portInp.Text = str[1];
                        break;
                    case "IP_Ins":
                        ipIns.Text = str[1];
                        break;
                    case "LineNo_Ins":
                        linenoIns.Text = str[1];
                        break;
                    case "MCID_Ins":
                        mcidIns.Text = str[1];
                        break;
                    case "StnID_Ins":
                        stnidIns.Text = str[1];
                        break;
                    case "Port_Ins":
                        portIns.Text = str[1];
                        break;
                    case "IP_Box":
                        ipBx.Text = str[1];
                        break;
                    case "LineNo_Box":
                        linenoBx.Text = str[1];
                        break;
                    case "MCID_Box":
                        mcidBx.Text = str[1];
                        break;
                    case "StnID_Box":
                        stnidBx.Text = str[1];
                        break;
                    case "Port_Box":
                        portBx.Text = str[1];
                        break;
                    default:
                        break;
                }
            }
            sr.Close();
        }

        public string ReadModelCode(string modelNam)
        {
            string modelCode = string.Empty;
            StreamReader sr = new StreamReader(@Application.StartupPath + "\\Model_Code.txt");
            while(sr.EndOfStream == false)
            {
                string str = sr.ReadLine();
                string[] arrStr = str.Split(':');
                if(arrStr[0].Contains(modelNam))
                {
                    modelCode = arrStr[1];
                }
            }
            return modelCode;
        }

        public bool CheckFormatCode(string code, string codeMol)
        {
            if (code.Length == 14)
            {
                int err = 0;
                string serailNo = code.Substring(7, 5);
                string molCod = code.Substring(code.Length - 1, 1);               
                if (molCod != codeMol)
                {
                    err++;
                }
                else if(int.Parse(serailNo) > 65534)
                {
                    err++;
                }
                else
                {
                    
                }  

                if(err == 0)
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
                return false;
            }
        }

        public bool CheckFormatCode(string code)
        {
            if(code.Length == 17)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void del_filLog(string namFil, string cod, int dong)
        {
            string strpth = @Application.StartupPath + "\\Log\\Duplicate\\" + namFil + ".log";
            if (File.Exists(strpth))
            {
                try
                {
                    string oldText = string.Empty;
                    string ntext = string.Empty;
                    FileStream fs = new FileStream(strpth, FileMode.Open);
                    StreamReader sr = new StreamReader(fs);
                    int i = 0;
                    while ((oldText = sr.ReadLine()) != null)
                    {
                        if (i < dong)
                        {
                            if (oldText.Contains(cod) == false)
                            {
                                ntext += oldText + Environment.NewLine;
                            }
                            else
                            {
                                i++;
                            }
                        }
                        else
                        {
                            ntext += oldText + Environment.NewLine;
                        }
                    }
                    sr.Close();
                    fs.Close();
                    File.WriteAllText(strpth, ntext);
                }
                catch (Exception)
                {
                    MessageBox.Show("Xảy ra lỗi xóa dữ liệu trong file " + namFil + ".log!", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }            
        }
    }
}
