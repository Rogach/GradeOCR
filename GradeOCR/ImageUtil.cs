using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class ImageUtil {
        public static Bitmap ToBlackAndWhite(Bitmap b) {
            AssertImageFormat(b);

            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            unsafe {
                byte* scan0 = (byte*) bd.Scan0.ToPointer();
                for (int q = 0; q < bd.Width * bd.Height; q++) {
                    if (*scan0 < 200) {
                        *scan0 = 0;
                    } else {
                        *scan0 = 255;
                    }
                    scan0++;
                }
            }
            b.UnlockBits(bd);

            return b;
        }

        public static void AssertImageFormat(Image img) {
            if (img.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed) {
                throw new Exception("Unsupported pixel format: " + img.PixelFormat);
            }
            Color prev = img.Palette.Entries.First();
            foreach (var c in img.Palette.Entries) {
                if (c.A != 255) {
                    throw new Exception("Unsupported alpha values in image");
                }
                if (c.R < prev.R || c.G < prev.G || c.B < prev.B) {
                    throw new Exception("Unsupported palette in image");
                }
                prev = c;
            }
        }
    }
}
