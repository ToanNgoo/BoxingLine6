using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingBarcodeBoxing
{
    class clsConvertData
    {
        public string removenull(string str)
        {
            if (str.Length == 0) return "";
            string data = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (str.Substring(i, 1) != "\0")
                {
                    data = data + str.Substring(i, 1);
                }
            }
            return data;
        }

        public string insert_Blank_Left(string str, int len)
        {
            if (str.Length >= len) return str;
            while (str.Length != len)
            {
                str = " " + str;
            }
            return str;
        }

        public string insert_Blank_Right(string str, int len)
        {
            if (str.Length >= len) return str;
            while (str.Length != len)
            {
                str = str + " ";
            }
            return str;
        }
    }
}
