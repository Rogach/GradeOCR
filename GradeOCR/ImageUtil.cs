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
                byte* ptr = (byte*) bd.Scan0.ToPointer();
                for (int q = 0; q < bd.Width * bd.Height; q++) {
                    if (*ptr < 200) {
                        *ptr = 0;
                    } else {
                        *ptr = 255;
                    }
                    ptr++;
                }
            }
            b.UnlockBits(bd);

            return b;
        }

        public static Bitmap Rotate(Bitmap src) {
            AssertImageFormat(src);

            Bitmap rotated = new Bitmap(src.Height, src.Width, PixelFormat.Format8bppIndexed);
            rotated.Palette = src.Palette;

            BitmapData srcBD = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData rotBD = rotated.LockBits(new Rectangle(0, 0, rotated.Width, rotated.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            unsafe {
                byte* srcPtr = (byte*) srcBD.Scan0.ToPointer();
                byte* rotPtr = (byte*) rotBD.Scan0.ToPointer();

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
