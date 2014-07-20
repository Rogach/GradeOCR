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

        public static Bitmap RemoveTopBorder(Bitmap b) {
            bool[] blackRows = FindBlackRows(b);
            int borderWidth = 0;
            for (int q = 0; q < 8; q++) {
                if (blackRows[q]) borderWidth = q + 1;
            }

            Bitmap withoutBorder = new Bitmap(b.Width, b.Height - borderWidth, PixelFormat.Format32bppArgb);
            withoutBorder.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            Graphics g = Graphics.FromImage(withoutBorder);
            g.DrawImage(
                b,
                new Rectangle(0, 0, b.Width, b.Height - borderWidth),
                new Rectangle(0, borderWidth, b.Width, b.Height - borderWidth),
                GraphicsUnit.Pixel
            );
            g.Dispose();
            return withoutBorder;
        }

        public static bool[] FindBlackRows(Bitmap b) {
            bool[] blackRows = new bool[b.Height];

            int threshold = (int) (b.Width * 0.3);
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
