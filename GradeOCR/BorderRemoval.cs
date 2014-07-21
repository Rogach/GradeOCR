using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;

namespace GradeOCR {
    public static class BorderRemoval {
        public static Bitmap RemoveBorder(Bitmap b) {
            for (int q = 0; q < 4; q++) {
                b = RemoveTopBorder(b);
                b = ImageUtil.Rotate(b);
            }
            return b;
        }

        public static Bitmap RemoveTopBorder(Bitmap src) {
            bool[] blackRows = FindBlackRows(src);
            int borderWidth = 0;
            for (int q = 0; q < 8; q++) {
                if (blackRows[q]) borderWidth = q + 1;
            }

            Bitmap res = new Bitmap(src.Width, src.Height - borderWidth, PixelFormat.Format32bppArgb);
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);
            g.DrawImage(
                src,
                new Rectangle(0, 0, src.Width, src.Height - borderWidth),
                new Rectangle(0, borderWidth, src.Width, src.Height - borderWidth),
                GraphicsUnit.Pixel
            );
            g.Dispose();

            return res;
        }

        public static bool[] FindBlackRows(Bitmap b) {
            bool[] blackRows = new bool[b.Height];

            int threshold = (int) (b.Width * 0.4);
            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte* ptr = (byte*) bd.Scan0.ToPointer();
                for (int y = 0; y < b.Height; y++) {
                    int c = 0;
                    for (int x = 0; x < b.Width; x++) {
                        if (*(ptr) == 0) c++;
                        ptr += 4;
                    }
                    blackRows[y] = c > threshold;
                }

                b.UnlockBits(bd);
            }

            return blackRows;
        }
    }
}
