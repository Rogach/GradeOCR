using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class WhitespaceCropper {
        public static Bitmap CropWhitespace(Bitmap src) {
            bool[] hWhite = FindHorizontalWhiteLines(src);
            bool[] vWhite = FindVerticalWhiteLines(src);

            int topCropWidth = 0;
            int bottomCropWidth = 0;
            int leftCropWidth = 0;
            int rightCropWidth = 0;

            for (int q = 0; q < src.Width; q++) {
                if (!vWhite[q]) break;
                leftCropWidth++;
            }
            for (int q = src.Width - 1; q >= 0; q--) {
                if (!vWhite[q]) break;
                rightCropWidth++;
            }
            for (int q = 0; q < src.Height; q++) {
                if (!hWhite[q]) break;
                topCropWidth++;
            }
            for (int q = src.Height - 1; q >= 0; q--) {
                if (!hWhite[q]) break;
                bottomCropWidth++;
            }

            Bitmap res = new Bitmap(
                src.Width - leftCropWidth - rightCropWidth,
                src.Height - topCropWidth - bottomCropWidth,
                PixelFormat.Format32bppArgb
            );
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);
            g.DrawImage(
                src,
                new Rectangle(0, 0, res.Width, res.Height),
                new Rectangle(leftCropWidth, topCropWidth, res.Width, res.Height),
                GraphicsUnit.Pixel
            );
            g.Dispose();

            return res;
        }

        private static bool[] FindHorizontalWhiteLines(Bitmap b) {
            bool[] whiteLines = new bool[b.Height];

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();
                
                for (int y = 0; y < b.Height; y++) {
                    whiteLines[y] = true;
                    for (int x = 0; x < b.Width; x++) {
                        whiteLines[y] &= *ptr == 255;
                        ptr += 4;
                    }
                }
                
                b.UnlockBits(bd);
            }

            return whiteLines;
        }

        private static bool[] FindVerticalWhiteLines(Bitmap b) {
            bool[] whiteLines = new bool[b.Width];
            for (int x = 0; x < b.Width; x++) {
                whiteLines[x] = true;
            }

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < b.Height; y++) {
                    for (int x = 0; x < b.Width; x++) {
                        whiteLines[x] &= *ptr == 255;
                        ptr += 4;
                    }
                }

                b.UnlockBits(bd);
            }

            return whiteLines;
        }
    }
}
