using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSCommunicationServer.Utilities
{
    class Strings
    {
        public static string FromHexToBinary(string hexValue)
        {
            ulong number = UInt64.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            byte[] bytes = BitConverter.GetBytes(number);

            string binaryString = string.Empty;
            foreach (byte singleByte in bytes)
            {
                binaryString += Convert.ToString(singleByte, 2);
            }

            return binaryString;
        }
        //Convert a hex to a string of a given Little Endian hex 
        public static string FromHexLittleEndianToString(String hexinvertido, int n)
        {
           char[] charArray = System.Text.Encoding.ASCII.GetChars(Strings.FromHex(Strings.Right(hexinvertido, n)));
            Array.Reverse(charArray);
            string c = new string(charArray);
            c = c.Substring(0, c.IndexOf('\0'));
            return c;
        }
        //Return an Array of Byte of a Given Hex
        public static byte[] FromHex(string hex)
        {
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
        //Convert a Hex to a Little Endian Hex  
        public static string Invert(string param,char separator)
        {
            string invert = "";
            string[] separated = param.Split(separator);

            for (int i = separated.Length - 1; i >= 0; i--)
            {
                invert += string.Join("", separated[i]);
            }

            return invert;
        }
       
        public static string Shorten(string param, int n)
        {
            int index = n > param.Length ? 0 : param.Length - n;
            return param.Substring(0, index);
        }

        public static string Left(string param, int length)
        {

            string result = param.Substring(0, length);
            return result;
        }
        public static string Right(string param, int length)
        {

            int value = param.Length - length;
            string result = param.Substring(value, length);
            return result;
        }

        public static string Mid(string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }

        public static string Mid(string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }
    }
}
