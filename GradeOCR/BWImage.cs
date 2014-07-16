using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public class BWImage {
        private bool[] data;
        public int Width { get; set;}
        public int Height { get; set; }

        public BWImage(Bitmap b) {
            //b = ToSimpleBitmap(b);
            ImageUtil.AssertImageFormat(b);

            this.Width = b.Width;
            this.Height = b.Height;
            this.data = new bool[Width * Height];

            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe {
                // image is layed out line-by-line, horizontally
                byte* scan0 = (byte*) bd.Scan0.ToPointer();
                for (int q = 0; q < bd.Width * bd.Height; q++) {
                    if (*scan0 < 250) {
                        data[q] = true;
                    }
                    scan0++;
                }
            }

            b.UnlockBits(bd);
        }

        public static Bitmap ToSimpleBitmap(Bitmap b) {
            Bitmap result = new Bitmap(b.Width, b.Height, PixelFormat.Format8bppIndexed);
            Graphics g = Graphics.FromImage(result);
            g.DrawImageUnscaled(b, new Point(0, 0));
            g.Dispose();
            return result;
        }

        public bool Pixel(int x, int y) {
            return data[y * Width + x];
        }

    }
}
