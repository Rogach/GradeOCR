using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public class GradeDigest {
        public static readonly int digestSize = 32;

        public ulong[] data = new ulong[digestSize * digestSize / 64];
        public byte grade = 0;

        public GradeDigest() { }

        public static GradeDigest FromImage(Bitmap src) {
            if (src.Width != digestSize || src.Height != digestSize) {
                throw new Exception(String.Format(
                    "Wrong digest image dimension: required {0}x{1}, got {2}x{3}", 
                    digestSize, digestSize, src.Width, src.Height));
            }
            GradeDigest gd = new GradeDigest();

            bool[] bitData = new bool[digestSize * digestSize];
            unsafe {
                BitmapData bd = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int q = 0; q < src.Width * src.Height; q++) {
                    bitData[q] = *ptr == 0;
                    ptr += 4;
                }

                src.UnlockBits(bd);
            }
            gd.data = PackBits(bitData);

            return gd;
        }

        public Bitmap DigestImage() {
            Bitmap res = new Bitmap(digestSize, digestSize, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData bd = res.LockBits(new Rectangle(0, 0, digestSize, digestSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                for (int q = 0; q < data.Length * 64; q++) {
                    *(ptr++) = ((data[q / 64] & ((ulong) 1 << (q % 64))) != 0) ? 0xff000000 : 0xffffffff;
                }

                res.UnlockBits(bd);
            }

            return res;
        }

        public static ulong[] PackBits(bool[] bits) {
            if (bits.Length % 64 != 0) {
                throw new Exception("PackBits() input array should have size divisible by 64. Got size: " + bits.Length);
            }
            ulong[] longs = new ulong[bits.Length / 64];
            for (int q = 0; q < bits.Length; q++) {
                if (bits[q])
                    longs[q / 64] += ((ulong) 1 << (q % 64));
            }
            return longs;
        }
    }
}
