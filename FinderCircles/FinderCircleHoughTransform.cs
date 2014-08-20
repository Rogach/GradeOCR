using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace ARCode {

    /*
     * Applies Hough transform to given image.
     * 
     * We use finder circle contour as kernel for Hough transform,
     * thus transforming 2d input image into 3d output space where pixel values
     * signify probability of finder circle being present with center at that pixel
     * (3d dimension is finder circle radius).
     * Pixel with highes value is then determined and used as recognized location
     * of finder circle in the image.
     * 
     * As optimization, input image is first severely downscaled (so that Hough transform
     * run in reasonable time). Resulting peaks are then "tuned" against full-scale image,
     * fixing their location with bigger precision.
     */
    public static class FinderCircleHoughTransform {

        /*
         * Convenience function to locate N finder circles with given pattern radius bounds.
         */
        public static List<Point3> LocateFinderCircles(Bitmap img, int minPatternRadius, int maxPatternRadius, int patternCount) {
            int scaleFactor = GetScaleFactor(minPatternRadius);
            Bitmap downscaledImage = ImageScaling.ScaleDown(img, scaleFactor);
            int[,,] hough = HoughTransform(downscaledImage, minPatternRadius / scaleFactor, maxPatternRadius / scaleFactor);
            List<Point3> roughPeaks = LocatePeaks(hough, patternCount, minPatternRadius / scaleFactor).ConvertAll(p =>
                new Point3(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor + minPatternRadius));
            return roughPeaks.ConvertAll(peak => TunePeak(img, minPatternRadius, maxPatternRadius, peak));
        }

        /*
         * "Tune" peak location. Given a peak, re-applies Hough transform in some radius
         * around this peak, and re-determines peak location.
         * This function also downscales input image, though less severely than
         * main transform.
         */
        public static Point3 TunePeak(Bitmap img, int minPatternRadius, int maxPatternRadius, Point3 peak) {
            int scaleFactor = GetScaleFactor(minPatternRadius);
            int tuneScaleFactor = GetTuneScaleFactor(minPatternRadius);

            int wSize = scaleFactor * 2 + maxPatternRadius * 2;
            int wx = peak.X - scaleFactor - maxPatternRadius;
            int wy = peak.Y - scaleFactor - maxPatternRadius;

            // clamp window to main image bounds
            wx = Math.Min(img.Width - wSize, Math.Max(0, wx));
            wy = Math.Min(img.Height - wSize, Math.Max(0, wy));

            Bitmap window = ImageScaling.ScaleDown(img, tuneScaleFactor, new Rectangle(wx, wy, wSize, wSize));
            int[, ,] hough = HoughTransform(window, minPatternRadius / tuneScaleFactor, maxPatternRadius / tuneScaleFactor);

            Point3 tunedPeak = LocatePeak(hough);
            return new Point3(
                tunedPeak.X * tuneScaleFactor + wx, 
                tunedPeak.Y * tuneScaleFactor + wy, 
                tunedPeak.Z * tuneScaleFactor + minPatternRadius);
        }

        /*
         * Get image downscale factor for main transform.
         */
        public static int GetScaleFactor(int minPatternRadius) {
            return (int) Math.Floor((double) minPatternRadius / 10);
        }

        /*
         * Get image downscale factor for peak tuning stage.
         */
        public static int GetTuneScaleFactor(int minPatterRadius) {
            return (int) Math.Floor((double) minPatterRadius / 20);
        }

        /*
         * Run Hough transform against given image.
         * Returns Hough transform array - 1st dimension is pattern radius, 2nd and 3d are Y and X.
         */
        public static int[,,] HoughTransform(Bitmap img, int minPatternRadius, int maxPatternRadius) {
            int imgWidth = img.Width;
            int imgHeight = img.Height;
            int[,,] fullHough = new int[maxPatternRadius - minPatternRadius + 1, imgHeight, imgWidth];

            for (int patternRadius = minPatternRadius; patternRadius <= maxPatternRadius; patternRadius++) {
                int[,] hough = new int[imgHeight + patternRadius * 2, imgWidth + patternRadius * 2];
                List<Point> blackMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != 1 && thisP == 1);
                List<Point> blackPlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == 1 && thisP != 1);
                List<Point> whitePlus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP != -1 && thisP == -1);
                List<Point> whiteMinus = CaclulateOffsetTable(patternRadius, (prevP, thisP) => prevP == -1 && thisP != -1);

                unsafe {
                    BitmapData bd = img.LockBits(ImageLockMode.ReadOnly);
                    byte* ptr = (byte*) bd.Scan0.ToPointer();

                    foreach (var ptBM in blackMinus) {
                        for (int y = 0; y < imgHeight; y++) {
                            for (int x = 0; x < imgWidth; x++) {
                                byte v = *(ptr + 4 * (y * imgWidth + x));
                                hough[patternRadius + y + ptBM.Y, patternRadius + x + ptBM.X] -= v;
                            }
                        }
                    }
                    foreach (var ptBP in blackPlus) {
                        for (int y = 0; y < imgHeight; y++) {
                            for (int x = 0; x < imgWidth; x++) {
                                byte v = *(ptr + 4 * (y * imgWidth + x));
                                hough[patternRadius + y + ptBP.Y, patternRadius + x + ptBP.X] += v;
                            }
                        }
                    }
                    foreach (var ptWP in whitePlus) {
                        for (int y = 0; y < imgHeight; y++) {
                            for (int x = 0; x < imgWidth; x++) {
                                byte v = *(ptr + 4 * (y * imgWidth + x));
                                hough[patternRadius + y + ptWP.Y, patternRadius + x + ptWP.X] += v * 68 / 100;
                            }
                        }
                    }
                    foreach (var ptWM in whiteMinus) {
                        for (int y = 0; y < imgHeight; y++) {
                            for (int x = 0; x < imgWidth; x++) {
                                byte v = *(ptr + 4 * (y * imgWidth + x));
                                hough[patternRadius + y + ptWM.Y, patternRadius + x + ptWM.X] -= v * 68 / 100;
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
                for (int y = 0; y < imgHeight; y++) {
                    for (int x = 0; x < imgWidth; x++) {
                        fullHough[patternRadius - minPatternRadius, y, x] = hough[patternRadius + y, patternRadius + x];
                    }
                }
            }

            return fullHough;
        }

        private static List<Point> CaclulateOffsetTable(int patternRadius, Func<int, int, bool> testPixel) {
            List<Point> pts = new List<Point>();
            PointF center = new PointF(0, 0);
            for (int cy = -patternRadius; cy <= patternRadius; cy++) {
                for (int cx = -patternRadius; cx <= patternRadius; cx++) {
                    PointF p = new PointF(cx, cy);
                    float r = (float) PointOps.Distance(p, center) / patternRadius;
                    PointF prevP = new PointF(cx - 1, cy);
                    float prevR = (float) PointOps.Distance(prevP, center) / patternRadius;
                    if (testPixel(FinderCircleDrawer.GetPixelAtRadius(prevR), FinderCircleDrawer.GetPixelAtRadius(r))) {
                        pts.Add(new Point(cx, cy));
                    }
                }
            }
            return pts;
        }

        /*
         * Locate single peak in Hough transform array.
         */
        public static Point3 LocatePeak(int[,,] hough) {
            int max = int.MinValue;
            int maxX = 0;
            int maxY = 0;
            int maxZ = 0;
            for (int z = 0; z < hough.GetLength(0); z++) {
                for (int y = 0; y < hough.GetLength(1); y++) {
                    for (int x = 0; x < hough.GetLength(2); x++) {
                        if (hough[z, y, x] > max) {
                            max = hough[z, y, x];
                            maxX = x;
                            maxY = y;
                            maxZ = z;
                        }
                    }
                }
            }
            return new Point3(maxX, maxY, maxZ);
        }

        /*
         * Locate several peaks in Hough transform array.
         */
        public static List<Point3> LocatePeaks(int[, ,] hough, int patternCount, int minPatternSize) {
            return LocatePeaks(hough, patternCount, new List<Point3>(), minPatternSize);
        }

        private static List<Point3> LocatePeaks(int[,,] hough, int patternCount, List<Point3> foundPeaks, int minPatternSize) {
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

        /*
         * Draw Hough transform array as grayscale image.
         */
        public static Bitmap HoughTransformImage(int[, ,] hough) {
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

    }
}
