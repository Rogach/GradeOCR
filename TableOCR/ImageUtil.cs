using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace TableOCR {
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
            rotated.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(rotated);
            g.TranslateTransform(0, rotated.Height);
            g.RotateTransform(-90);
            g.DrawImageUnscaled(src, 0, 0);
            g.Dispose();

            return rotated;
        }
    }
}
