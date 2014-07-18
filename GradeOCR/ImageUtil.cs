using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class ImageUtil {
        public static Bitmap ToStdFormat(Bitmap b) {
            Bitmap res = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
            res.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            Graphics g = Graphics.FromImage(res);
            g.DrawImageUnscaled(b, 0, 0);
            g.Dispose();
            return res;
        }

        public static Bitmap ToBlackAndWhite(Bitmap b) {
            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe {
                byte* ptr = (byte*) bd.Scan0.ToPointer();
                for (int q = 0; q < bd.Width * bd.Height; q++) {
                    int gray = (*ptr * 30 + *(ptr + 1) * 59 + *(ptr + 2) * 11) / 100;
                    if (gray < 220) {
                        *ptr = 0;
                        *(ptr + 1) = 0;
                        *(ptr + 2) = 0;
                    } else {
                        *ptr = 255;
                        *(ptr + 1) = 255;
                        *(ptr + 2) = 255;
                    }
                    ptr += 4;
                }
            }
            b.UnlockBits(bd);

            return b;
        }

        public static Bitmap Rotate(Bitmap src) {
            Bitmap rotated = new Bitmap(src.Height, src.Width, PixelFormat.Format32bppArgb);

            BitmapData srcBD = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData rotBD = rotated.LockBits(new Rectangle(0, 0, rotated.Width, rotated.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe {
                uint* srcPtr = (uint*) srcBD.Scan0.ToPointer();
                uint* rotPtr = (uint*) rotBD.Scan0.ToPointer();

                for (int y = 0; y < src.Height; y++) {
                    for (int x = 0; x < src.Width; x++) {
                        *(rotPtr + (src.Width - 1 - x) * src.Height + y) = *(srcPtr++);
                    }
                }
            }

            src.UnlockBits(srcBD);
            rotated.UnlockBits(rotBD);

            return rotated;
        }
    }
}
