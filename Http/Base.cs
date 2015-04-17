using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http
{
    public class Base
    {
        public static String NewLine = "\r\n";
        public static byte[] NewLineBytes = Encoding.UTF8.GetBytes(NewLine);

        public static String DoubleNewLine = NewLine + NewLine;
        public static byte[] DoubleNewLineBytes = Encoding.UTF8.GetBytes(DoubleNewLine);
    }
}
