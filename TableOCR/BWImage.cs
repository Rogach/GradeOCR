using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace TableOCR {
    public class BWImage {
        public bool[] data;
        public int Width { get; set;}
        public int Height { get; set; }

        public BWImage(Bitmap b) {
            this.Width = b.Width;
            this.Height = b.Height;
            this.data = new bool[Width * Height];

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // image is layed out line-by-line, horizontally
                byte* ptr = (byte*) bd.Scan0.ToPointer();
                for (int q = 0; q < bd.Width * bd.Height; q++) {
                    int gray = (*ptr * 30 + *(ptr + 1) * 59 + *(ptr + 2) * 11) / 100;
                    if (gray < 220) {
                        data[q] = true;
                    }
                    ptr += 4;
                }

                b.UnlockBits(bd);
            }
            
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
