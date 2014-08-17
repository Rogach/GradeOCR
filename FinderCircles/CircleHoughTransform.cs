using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace FinderCircles {
    
    public class Point3 {
        public int X;
        public int Y;
        public int Z;
        public Point3(int X, int Y, int Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public override string ToString() {
            return String.Format("[X={0},Y={1},Z={2}]", X, Y, Z);
        }
    }

    public static class CircleHoughTransform {

        public static List<Point3> LocateFinderCircles(Bitmap img, int minPatternRadius, int maxPatternRadius, int patternCount) {
            int scaleFactor = GetScaleFactor(minPatternRadius);
            Bitmap downscaledImage = ImageScaling.ScaleDown(img, scaleFactor);
            int[,,] hough = HoughTransform(downscaledImage, minPatternRadius / scaleFactor, maxPatternRadius / scaleFactor);
            return LocatePeaks(hough, patternCount, minPatternRadius / scaleFactor).ConvertAll(p => 
                new Point3(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor + minPatternRadius));
        }

        public static int GetScaleFactor(int minPatternRadius) {
            return (int) Math.Ceiling((double) minPatternRadius / 10);
        }

        public static int[, ,] HoughTransform(Bitmap img, int minPatternRadius, int maxPatternRadius) {
            return HoughTransform(img, minPatternRadius, maxPatternRadius, 0, 0, img.Width, img.Height);
        }

        public static int[,,] HoughTransform(Bitmap img, int minPatternRadius, int maxPatternRadius, int wX, int wY, int width, int height) {
            int[,,] fullHough = new int[maxPatternRadius - minPatternRadius + 1, height, width];
            for (int patternRadius = minPatternRadius; patternRadius <= maxPatternRadius; patternRadius++) {
                int[,] hough = new int[height + patternRadius * 2, height + patternRadius * 2];
                List<Point> blackMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != 1 && thisP == 1);
                List<Point> blackPlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == 1 && thisP != 1);
                List<Point> whitePlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != -1 && thisP == -1);
                List<Point> whiteMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == -1 && thisP != -1);

                unsafe {
                    BitmapData bd = img.LockBits(ImageLockMode.ReadOnly);
                    byte* ptr;

                    foreach (var ptBM in blackMinus) {
                        ptr = (byte*) bd.Scan0.ToPointer();
                        for (int y = 0; y < height; y++) {
                            for (int x = 0; x < width; x++) {
                                byte v = *ptr;
                                hough[wY + patternRadius + y + ptBM.Y, wX + patternRadius + x + ptBM.X] -= v;
                                ptr += 4;
                            }
                        }
                    }
                    foreach (var ptBP in blackPlus) {
                        ptr = (byte*) bd.Scan0.ToPointer();
                        for (int y = 0; y < height; y++) {
                            for (int x = 0; x < width; x++) {
                                byte v = *ptr;
                                hough[wY + patternRadius + y + ptBP.Y, wX + patternRadius + x + ptBP.X] += v;
                                ptr += 4;
                            }
                        }
                    }
                    foreach (var ptWP in whitePlus) {
                        ptr = (byte*) bd.Scan0.ToPointer();
                        for (int y = 0; y < height; y++) {
                            for (int x = 0; x < width; x++) {
                                byte v = *ptr;
                                hough[wY + patternRadius + y + ptWP.Y, wX + patternRadius + x + ptWP.X] += v * 68 / 100;
                                ptr += 4;
                            }
                        }
                    }
                    foreach (var ptWM in whiteMinus) {
                        ptr = (byte*) bd.Scan0.ToPointer();
                        for (int y = 0; y < height; y++) {
                            for (int x = 0; x < width; x++) {
                                byte v = *ptr;
                                hough[wY + patternRadius + y + ptWM.Y, wX + patternRadius + x + ptWM.X] -= v * 68 / 100;
                                ptr += 4;
                            }
                        }
                    }

                    int houghHeight = hough.GetLength(0);
                    int houghWidth = hough.GetLength(1) - 1;
                    for (int y = 0; y < houghHeight; y++) {
                        for (int x = 0; x < houghWidth; x++) {
                            hough[y, x + 1] += hough[y, x];
                        }
                    }

                    img.UnlockBits(bd);
                }
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        fullHough[patternRadius - minPatternRadius, y, x] = hough[patternRadius + y, patternRadius + x];
                    }
                }
            }

            return fullHough;
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

        public static Bitmap HoughTransformImage(int[,,] hough) {
            int max = int.MinValue;
            int min = int.MaxValue;
            foreach (int h in hough) {
                if (h > max) max = h;
                if (h < min) min = h;
            }
            if (max == min) max = min + 1;

            int zdim = hough.GetLength(0);
            int height = hough.GetLength(1);
            int width = hough.GetLength(2);
            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        byte v = byte.MinValue;
                        for (int z = 0; z < zdim; z++) {
                            byte zv = (byte) ((hough[z, y, x] - min) * 255 / (max - min));
                            if (zv > v) v = zv;
                        }
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

        public static List<Point3> LocatePeaks(int[,,] hough, int patternCount, int minPatternSize) {
            return LocatePeaks(hough, patternCount, new List<Point3>(), minPatternSize);
        }
        public static List<Point3> LocatePeaks(int[,,] hough, int patternCount, List<Point3> foundPeaks, int minPatternSize) {
            int max = int.MinValue;
            int maxX = 0;
            int maxY = 0;
            int maxZ = 0;
            for (int z = 0; z < hough.GetLength(0); z++) {
                for (int y = 0; y < hough.GetLength(1); y++) {
                    for (int x = 0; x < hough.GetLength(2); x++) {
                        Point3 neighbourPeak = foundPeaks.Find(p => PointOps.Distance(x, y, p.X, p.Y) < minPatternSize);
                        if (neighbourPeak == null && hough[z, y, x] > max) {
                            max = hough[z, y, x];
                            maxX = x;
                            maxY = y;
                            maxZ = z;
                        }
                    }
                }
            }
            foundPeaks.Add(new Point3(maxX, maxY, maxZ));
            if (patternCount == 1) {
                return foundPeaks;
            } else {
                return LocatePeaks(hough, patternCount - 1, foundPeaks, minPatternSize);
            }
        }

        public static Bitmap DrawPeaks(Bitmap src, List<Point3> peaks) {
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
