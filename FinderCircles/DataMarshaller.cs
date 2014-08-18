using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCode {
    public static class DataMarshaller {

        public static bool[] MarshallInt(uint value) {
            bool[] data = new bool[256];
            
            // calculate and write checksum
            for (int q = 0; q < 32; q += 8) {
                UnpackByte(data, q, UIntChecksum(value));
            }
            int packedChecksum = PackByte(data, 0);
            
            for (int q = 32; q < 256; q += 32) {
                UnpackUInt(data, q, value);
            }

            return data;
        }

        private static void UnpackByte(bool[] data, int offset, byte value) {
            for (int q = 0; q < 8; q++) {
                data[offset + q] = ((value >> q) & 1) == 1;
            }
        }

        private static byte PackByte(bool[] data, int offset) {
            byte value = 0;
            for (int q = 7; q >= 0; q--) {
                value <<= 1;
                if (data[offset + q]) value++;
            }
            return value;
        }

        private static void UnpackUInt(bool[] data, int offset, uint value) {
            for (int q = 0; q < 32; q++) {
                data[offset + q] = ((value >> q) & 1) == 1;
            }
        }

        private static uint PackUInt(bool[] data, int offset) {
            uint value = 0;
            for (int q = 31; q >= 0; q--) {
                value <<= 1;
                if (data[offset + q]) value++;
            }
            return value;
        }

        private static byte UIntChecksum(uint value) {
            return (byte) (((byte) value) + ((byte) (value >> 8)) + ((byte) (value >> 16)) + ((byte) (value >> 24)));
        }

        public static uint UnMarshallInt(bool[] data) {
            byte checksum = PackByte(ReadRedundantData(data, 0, 8, 4), 0);
            uint value = PackUInt(ReadRedundantData(data, 32, 32, 7), 0);
            if (checksum != UIntChecksum(value))
                Console.WriteLine("Checksum error when decoding AR-code");
            return value;
        }

        private static bool[] ReadRedundantData(bool[] data, int offset, int length, int copies) {
            bool[] res = new bool[length];
            for (int q = 0; q < length; q++) {
                int pos = 0;
                int neg = 0;
                for (int w = 0; w < copies; w++) {
                    if (data[offset + w * length + q]) {
                        pos++;
                    } else {
                        neg++;
                    }
                }
                res[q] = pos >= neg;
            }
            return res;
        }
    }
}
