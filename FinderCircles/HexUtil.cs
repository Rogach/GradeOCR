using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;

namespace ARCode {
    public static class HexUtil {
        public static string ToHexString(bool[] data) {
            if (data.Length % 8 != 0) {
                throw new ArgumentException("illegal data length: " + data.Length);
            }
            string res = "";

            byte c = 0;
            for (int q = 0; q < data.Length; q++) {
                if (data[q]) c += (byte) (1 << (q % 8));
                if (q % 8 == 7) {
                    res += Hex2(c) + " ";
                    c = 0;
                }
            }

            return res.Trim();
        }

        public static string Hex2(byte b) {
            return Hex((byte) (b / 16)) + Hex((byte) (b % 16));
        }

        public static string Hex(byte b) {
            if (b < 10) {
                return b.ToString();
            } else {
                return ((char) (((int) 'a') + b - 10)).ToString();
            }
        }
    }
}
