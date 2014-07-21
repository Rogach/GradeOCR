using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public class GradeDigest {
        public static readonly int digestSize = 32;

        public bool[] data = new bool[digestSize * digestSize];
        public byte grade = 0;

        public GradeDigest() { }

        public static GradeDigest FromImage(Bitmap src) {
            if (src.Width != digestSize || src.Height != digestSize) {
                throw new Exception(String.Format(
                    "Wrong digest image dimension: required {0}x{1}, got {2}x{3}", 
                    digestSize, digestSize, src.Width, src.Height));
            }
            GradeDigest gd = new GradeDigest();

            unsafe {
                BitmapData bd = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int q = 0; q < src.Width * src.Height; q++) {
                    gd.data[q] = *ptr == 0;
                    ptr += 4;
                }

                src.UnlockBits(bd);
            }

            return gd;
        }

        public Bitmap DigestImage() {
            Bitmap res = new Bitmap(digestSize, digestSize, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData bd = res.LockBits(new Rectangle(0, 0, digestSize, digestSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                for (int q = 0; q < data.Length; q++) {
                    *(ptr++) = data[q] ? 0xff000000 : 0xffffffff;
                }

                res.UnlockBits(bd);
            }

            return res;
        }
    }
}
