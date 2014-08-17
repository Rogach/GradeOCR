using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace FinderCircles {
    public static class CircleHoughTransform {
        
        public static int[,] HoughTransform(Bitmap img, int patternRadius) {
            int[,] hough = new int[img.Height + patternRadius * 2, img.Width + patternRadius * 2];
            List<Point> blackMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != 1 && thisP == 1);
            List<Point> blackPlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == 1 && thisP != 1);
            List<Point> whitePlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != -1 && thisP == -1);
            List<Point> whiteMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == -1 && thisP != -1);

            unsafe {
                BitmapData bd = img.LockBits(ImageLockMode.ReadOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < img.Height; y++) {
                    for (int x = 0; x < img.Width; x++) {
                        byte v = *ptr;
                        foreach (var ptBM in blackMinus) {
                            hough[patternRadius + y + ptBM.Y, patternRadius + x + ptBM.X] -= v;
                        }
                        foreach (var ptBP in blackPlus) {
                            hough[patternRadius + y + ptBP.Y, patternRadius + x + ptBP.X] += v;
                        }
                        foreach (var ptWP in whitePlus) {
                            hough[patternRadius + y + ptWP.Y, patternRadius + x + ptWP.X] += v;
                        }
                        foreach (var ptWM in whiteMinus) {
                            hough[patternRadius + y + ptWM.Y, patternRadius + x + ptWM.X] -= v;
                        }

                        ptr += 4;
                    }
                }


                for (int y = 0; y < hough.GetLength(0); y++) {
                    for (int x = 0; x < hough.GetLength(1) - 1; x++) {
                        hough[y, x + 1] += hough[y, x];
                    }
                }

                img.UnlockBits(bd);
            }

            int[,] trimmedHough = new int[img.Height - patternRadius * 2, img.Width - patternRadius * 2];
            for (int y = patternRadius; y < img.Height - patternRadius; y++) {
                for (int x = patternRadius; x < img.Width - patternRadius; x++) {
                    trimmedHough[y - patternRadius, x - patternRadius] = hough[y + patternRadius, x + patternRadius];
                }
            }

            return trimmedHough;
        }

        public static List<Point> CaclulateOffsetTable(int patternRadius, Func<int, int, bool> testPixel) {
            List<Point> pts = new List<Point>();
            PointF center = new PointF(0, 0);
            for (int cy = -patternRadius; cy <= patternRadius; cy++) {
                for (int cx = -patternRadius; cx <= patternRadius; cx++) {
                    PointF p = new PointF(cx, cy);
                    float r = (float) PointOps.Distance(p, center) / patternRadius;
                    PointF prevP = new PointF(cx - 1, cy);
                    float prevR = (float) PointOps.Distance(prevP, center) / patternRadius;
                    if (testPixel(CircleDrawer.GetPixelAtRadius(prevR), CircleDrawer.GetPixelAtRadius(r))) {
                        pts.Add(new Point(cx, cy));
                    }
                }
            }
            return pts;
        }

        public static Bitmap HoughTransformImage(int[,] hough) {
            int max = int.MinValue;
            int min = int.MaxValue;
            foreach (int h in hough) {
                if (h > max) max = h;
                if (h < min) min = h;
            }
            if (max == min) max = min + 1;

            int height = hough.GetLength(0);
            int width = hough.GetLength(1);
            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        byte v = (byte) ((hough[y, x] - min) * 255 / (max - min));
                        *ptr = v;
                        *(ptr + 1) = v;
                        *(ptr + 2) = v;
                        *(ptr + 3) = 255;
                        ptr += 4;
                    }
                }

                res.UnlockBits(bd);
            }
            return res;
        }

        public static List<Point> LocatePeaks(int[,] hough, int patternSize) {
            int max = int.MinValue;
            int maxX = 0;
            int maxY = 0;
            for (int y = 0; y < hough.GetLength(0); y++) {
                for (int x = 0; x < hough.GetLength(1); x++) {
                    if (hough[y, x] > max) {
                        max = hough[y, x];
                        maxX = x;
                        maxY = y;
                    }
                }
            }
            return new List<Point> { new Point(maxX, maxY) };
        }

        public static Bitmap DrawPeaks(Bitmap src, List<Point> peaks) {
            Bitmap res = new Bitmap(src);

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                foreach (var pt in peaks) {
                    *(ptr + pt.Y * src.Width + pt.X) = 0xffff0000;
                }

                res.UnlockBits(bd);
            }

            return res;
        }
    }
}
