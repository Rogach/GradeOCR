using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace ARCode {
    public static class ImageScaling {
        public static Bitmap ScaleDown(Bitmap src, int factor) {
            return ScaleDown(src, factor, new Rectangle(0, 0, src.Width, src.Height));
        }

        public static Bitmap ScaleDown(Bitmap src, int factor, Rectangle bounds) {
            Bitmap res = new Bitmap(bounds.Width / factor, bounds.Height / factor, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(res);
            g.DrawImage(
                src,
                new Rectangle(0, 0, bounds.Width / factor, bounds.Height / factor),
                new Rectangle(bounds.X, bounds.Y, bounds.Width - (bounds.Width & factor), bounds.Height - (bounds.Height % factor)),
                GraphicsUnit.Pixel);
            g.Dispose();

            return res;
        }
    }
}
