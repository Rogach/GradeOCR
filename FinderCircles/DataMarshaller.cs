using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXing.Common.ReedSolomon;

namespace ARCode {

    /*
     * Prepares uint value for storage in the code, and extracts value from processed code.
     * Applies Reed-Solomon error correction algorithms to the data - half of the bits in
     * input image can be destroyed, and value would still be extracted properly.
     */
    public static class DataMarshaller {

        public static bool[] MarshallInt(uint value) {
            int[] byteArray = new int[32];
            byteArray[0] = (byte) (value % 256);
            byteArray[1] = (byte) ((value >> 8) % 256);
            byteArray[2] = (byte) ((value >> 16) % 256);
            byteArray[3] = (byte) ((value >> 24) % 256);

            ReedSolomonEncoder rse = new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256);
            rse.encode(byteArray, 28);

            return UnpackByteArray(byteArray);
        }

        public static uint UnMarshallInt(bool[] bitData) {
            int[] byteArray = PackByteArray(bitData);

            ReedSolomonDecoder rsd = new ReedSolomonDecoder(GenericGF.QR_CODE_FIELD_256);
            if (!rsd.decode(byteArray, 28)) {
                Console.WriteLine("decoding failed");
                //throw new ArgumentException("Reed-Solomon decoding failed when extracting AR-code");
            }

            uint value = 0;
            value += (uint) byteArray[0];
            value += (uint) (byteArray[1]) << 8;
            value += (uint) (byteArray[2]) << 16;
            value += (uint) (byteArray[3]) << 24;

            return value;
        }

        private static bool[] UnpackByteArray(int[] byteArray) {
            bool[] bitData = new bool[byteArray.Length * 8];
            for (int q = 0; q < byteArray.Length; q++) {
                UnpackByte(bitData, q * 8, (byte) byteArray[q]);
            }
            return bitData;
        }

        private static int[] PackByteArray(bool[] bitData) {
            int[] byteArray = new int[bitData.Length / 8];
            for (int q = 0; q < bitData.Length / 8; q++) {
                byteArray[q] = PackByte(bitData, q * 8);
            }
            return byteArray;
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

    }
}
