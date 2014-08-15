using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;
using TableOCR;

namespace TableOCR {

    /*
     * Pseudo-Hough transform
     * Inspired by real Hough transform, that takes an image
     * and by examining neighbourhoods of each point determines the probability
     * of line being present with given angle and distance from center.
     * 
     * For each tested angle, this algorithm extends a line from each point,
     * find Y-Intersect value (y at x == 0), and counts lines for each Y-Intersect value.
     * Y-Intersect at given angle with biggest resulting count has biggest probability of
     * line passing through it.
     * 
     * Essentially, we are transforming an image from X/Y space to angle/distance-from-origin space (A/DO space).
     * (HoughTransformImage() and HoughTransformImageWithPeaks() 
     * offer visualization of A/DO space)
     * 
     * Location of lines can be done via finding local peaks in A/DO space.
     * 
     * In this class, A/DO space is represented as simple 2d integer array.
     */
    public static class PseudoHoughTransform {

        /*
         * Convenience function to recognize set of lines in points.
         */ 
        public static List<RawLine> RecognizeLines(List<Point> points, RecognitionOptions options) {
            int[,] hough = HoughTransform(points, options);
            List<Point> houghPeaks = FindHoughPeaks(hough, options);
            return ExtractRawLines(houghPeaks, options);
        }

        /*
         * Main function, that performs Pseudo-Hough transform as described above.
         * Returns A/DO space array.
         */
        public static int[,] HoughTransform(List<Point> points, RecognitionOptions options) {
            double[] angleMap = GetAngleMap(options);
            int[,] hough = new int[angleMap.Length, options.imageHeight];

            for (int a = 0; a < angleMap.Length; a++) {
                foreach (Point pt in points) {
                    int y = (int) Math.Round(pt.Y - pt.X * angleMap[a]);
                    if (y >= 0 && y < options.imageHeight) hough[a, y]++;
                }
            }

            return hough;
        }

        /*
         * Locate peaks in A/DO space array.
         * For each point, searches some narrow window of points around it,
         * and if there is no point bigger than current point, records it as a peak.
         * If there are several points with same value, records their average as a peak.
         */
        public static List<Point> FindHoughPeaks(int[,] hough, RecognitionOptions options) {
            int width = hough.GetLength(0);
            int height = hough.GetLength(1);

            int maxHough = 0;
            foreach (var h in hough) {
                if (h > maxHough) maxHough = h;
            }

            List<Point> thresholdedPoints = new List<Point>();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if ((double) hough[x, y] / maxHough > options.houghThreshold)
                        thresholdedPoints.Add(new Point(x, y));
                }
            }

            List<Point> peaks = new List<Point>();
            HashSet<Point> usedPoints = new HashSet<Point>();
            foreach (Point pt in thresholdedPoints) {
                if (!usedPoints.Contains(pt)) {
                    List<Point> adjPoints =
                        thresholdedPoints
                        .Where(p => 
                            Math.Abs(p.X - pt.X) < options.houghWindowWidth && 
                            Math.Abs(p.Y - pt.Y) < options.houghWindowHeight).ToList();

                    int maxAdj = adjPoints.Select(p => hough[p.X, p.Y]).Max();
                    if (hough[pt.X, pt.Y] == maxAdj) {
                        List<Point> adjPeaks = adjPoints.Where(p => hough[p.X, p.Y] == maxAdj).ToList();
                        int avgX = (int) Math.Round(adjPeaks.Select(p => p.X).Average());
                        int avgY = (int) Math.Round(adjPeaks.Select(p => p.Y).Average());
                        peaks.Add(new Point(avgX, avgY));
                        foreach (var adj in adjPoints) {
                            usedPoints.Add(adj);
                        }
                    }
                }
            }
            return peaks;
        }

        /*
         * Extract raw lines from peaks in A/DO space.
         */
        public static List<RawLine> ExtractRawLines(List<Point> houghPeaks, RecognitionOptions options) {
            double[] angleMap = GetAngleMap(options);
            List<RawLine> lines = new List<RawLine>();
            foreach (var pt in houghPeaks) {
                lines.Add(new RawLine { yInt = pt.Y, k = angleMap[pt.X] });
            }
            return lines;
        }

        /*
         * Get array of angles that we want to test.
         * Creates array of angles, that differ by 1px at distance of image width.
         */
        private static double[] GetAngleMap(RecognitionOptions options) {
            int maxDy = (int) Math.Ceiling(options.imageWidth * options.maxAngleFactor);
            double[] angleMap = new double[maxDy * 2 + 1];
            for (int dy = -maxDy; dy <= maxDy; dy++) {
                angleMap[maxDy + dy] = (double) dy / (double) options.imageWidth;
            }
            return angleMap;
        }

        /*
         * Visualizes A/DO space array in an image 
         * (black pixels - zero probability of line presence, white - maximum probability)
         */
        public static Bitmap HoughTransformImage(int[,] hough) {
            int maxHough = 0;
            foreach (var h in hough) {
                if (h > maxHough) maxHough = h;
            }

            int resWidth = hough.GetLength(0);
            int resHeight = hough.GetLength(1);
            Bitmap res = new Bitmap(resWidth, resHeight, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < resHeight; y++) {
                    for (int x = 0; x < resWidth; x++) {
                        byte v = (byte) (hough[x, y] * 255 / maxHough);
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

        /*
         * Draw peaks in A/DO space as red dots over visualization offered by HoughTransformImage().
         */
        public static Bitmap HoughTransformImageWithPeaks(int[,] hough, List<Point> peaks) {
            Bitmap img = HoughTransformImage(hough);
            Graphics g = Graphics.FromImage(img);

            foreach (var pt in peaks) {
                g.FillEllipse(Brushes.Red, new Rectangle(pt.X - 4, pt.Y - 4, 8, 8));
            }

            g.Dispose();
            return img;
        }

    }
}
