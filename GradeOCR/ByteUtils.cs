using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GradeOCR {
    public static class ByteUtils {
        public static byte CountBits(ulong i) {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            i = (i + (i >> 4)) & 0x0F0F0F0F0F0F0F0F;
            i = i + (i >> 8);
            i = i + (i >> 16);
            i = i + (i >> 32);
            return (byte) i;
        }

        public static byte SafeReadByte(Stream s) {
            int b = s.ReadByte();
            if (b == -1) {
                throw new Exception("EOF");
            } else {
                return (byte) b;
            }
        }

        public static void WriteUInt(Stream s, uint i) {
            for (int q = 0; q < 4; q++) {
                s.WriteByte((byte) (i >> (8 * q)));
            }
        }

        public static void WriteULong(Stream s, ulong i) {
            for (int q = 0; q < 8; q++) {
                s.WriteByte((byte) (i >> (8 * q)));
            }
        }

        public static uint ReadUInt(Stream s) {
            uint res = 0;
            for (int q = 0; q < 4; q++) {
                uint b = SafeReadByte(s);
                res += (b << (q * 8));
            }
            return res;
        }

        public static ulong ReadULong(Stream s) {
            ulong res = 0;
            for (int q = 0; q < 8; q++) {
                ulong b = SafeReadByte(s);
                res += (b << (q * 8));
            }
            return res;
        }
    }
}
