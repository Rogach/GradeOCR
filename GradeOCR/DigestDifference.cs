using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class DigestDifference {
        public static Bitmap GenerateDifferenceImage(GradeDigest gd1, GradeDigest gd2) {
            Bitmap res = new Bitmap(GradeDigest.digestSize, GradeDigest.digestSize, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData bd = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                for (int q = 0; q < gd1.data.Length; q++) {
                    if (gd1.data[q] && gd2.data[q]) {
                        *(ptr++) = 0xff00ff00;
                    } else if (gd1.data[q] && !gd2.data[q]) {
                        *(ptr++) = 0xffff0000;
                    } else if (!gd1.data[q] && gd2.data[q]) {
                        *(ptr++) = 0xff0000ff;
                    } else {
                        *(ptr++) = 0xffffffff;
                    }
                }

                res.UnlockBits(bd);
            }

            return res;
        }
    }
}
