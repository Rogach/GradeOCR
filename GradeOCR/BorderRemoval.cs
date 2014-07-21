using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;

namespace GradeOCR {
    public static class BorderRemoval {
        public static readonly double blackThreshold = 0.6;

        public static Bitmap RemoveBorder(Bitmap src) {
            bool[] hBlack = FindBlackHorizontalLines(src);
            bool[] vBlack = FindBlackVerticalLines(src);

            int topBorderWidth = 0;
            int bottomBorderWidth = 0;
            int leftBorderWidth = 0;
            int rightBorderWidth = 0;

            for (int q = 0; q < 6; q++) {
                if (hBlack[q]) topBorderWidth = q + 1;
                if (hBlack[src.Height - 1 - q]) bottomBorderWidth = q + 1;
                if (vBlack[q]) leftBorderWidth = q + 1;
                if (vBlack[src.Width - 1 - q]) rightBorderWidth = q + 1;
            }

            Bitmap res = new Bitmap(
                src.Width - leftBorderWidth - rightBorderWidth, 
                src.Height - topBorderWidth - bottomBorderWidth,
                PixelFormat.Format32bppArgb
            );
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);
            g.DrawImage(
                src,
                new Rectangle(0, 0, res.Width, res.Height),
                new Rectangle(leftBorderWidth, topBorderWidth, res.Width, res.Height),
                GraphicsUnit.Pixel
            );
            g.Dispose();

            return res;
        }

        public static bool[] FindBlackHorizontalLines(Bitmap b) {
            bool[] blackLines = new bool[b.Height];

            int threshold = (int) (b.Width * blackThreshold);
            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte* ptr = (byte*) bd.Scan0.ToPointer();
                for (int y = 0; y < b.Height; y++) {
                    int c = 0;
                    for (int x = 0; x < b.Width; x++) {
                        if (*(ptr) == 0) c++;
                        ptr += 4;
                    }
                    blackLines[y] = c > threshold;
                }

                b.UnlockBits(bd);
            }

            return blackLines;
        }

        public static bool[] FindBlackVerticalLines(Bitmap b) {
            bool[] blackLines = new bool[b.Width];

            int threshold = (int) (b.Height * blackThreshold);

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                int[] tally = new int[b.Width];
                for (int y = 0; y < b.Height; y++) {
                    for (int x = 0; x < b.Width; x++) {
                        if (*(ptr) == 0) tally[x]++;
                        ptr += 4;
                    }
                }

                for (int x = 0; x < b.Width; x++) {
                    blackLines[x] = tally[x] > threshold;
                }

                b.UnlockBits(bd);
            }

            return blackLines;
        }
    }
}
