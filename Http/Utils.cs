using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sin.Http
{
    public static class Utils
    {
        public static int IndexOfBytes(this List<byte> bytes, byte[] bts, int start = 0)
        {
            int max = bytes.Count - bts.Length;
            for (int i = start; i <= max; ++i)
            {
                if (bytes[i] == bts[0])
                {
                    bool flag = true;
                    for (int n = 1; n < bts.Length; ++n)
                    {
                        if (bytes[i + n] != bts[n])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
