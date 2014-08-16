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
            int[,] hough = new int[img.Width - patternRadius * 2, img.Height - patternRadius * 2];

            unsafe {
                BitmapData bd = img.LockBits(ImageLockMode.ReadOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = patternRadius; y < img.Height - patternRadius; y++) {
                    for (int x = patternRadius; x < img.Width - patternRadius; x++) {
                        int hy = y - patternRadius;
                        int hx = x - patternRadius;
                        hough[hx, hy] += ScoreAtPoint(ptr, img, x, y, patternRadius);
                    }
                }

                img.UnlockBits(bd);
            }

            return hough;
        }

        private static unsafe int ScoreAtPoint(byte* ptr, Bitmap img, int x, int y, int patternRadius) {
            int score = 0;
            PointF center = new PointF(x, y);
            for (int cy = y - patternRadius; cy <= y + patternRadius; cy++) {
                for (int cx = x - patternRadius; cx <= x + patternRadius; cx++) {
                    PointF p = new PointF(cx, cy);
                    float r = (float) PointOps.Distance(p, center) / patternRadius;
                    byte v = *(ptr + 4 * (cy * img.Width + cx));

                    if (r < 3f / 9) {
                        score -= v;
                    } else if (r < 5f / 9) {
                        score += v;
                    } else if (r < 7f / 9) {
                        score -= v;
                    } else if (r < 1) {
                        score += v;
                    }
                }
            }
            return score;
        }

        public static Bitmap HoughTransformImage(int[,] hough) {
            int max = int.MinValue;
            int min = int.MaxValue;
            foreach (int h in hough) {
                if (h > max) max = h;
                if (h < min) min = h;
            }

            int width = hough.GetLength(0);
            int height = hough.GetLength(1);
            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        byte v = (byte) ((hough[x, y] - min) * 255 / (max - min));
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
            for (int y = 0; y < hough.GetLength(1); y++) {
                for (int x = 0; x < hough.GetLength(0); x++) {
                    if (hough[x, y] > max) {
                        max = hough[x, y];
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
