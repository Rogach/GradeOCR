using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;
using TableOCR;

namespace LineOCR {
    public struct RawLine {
        public int yInt;
        public double k;
    }

    public static class PseudoHoughTransform {

        public static double[] GetAngleMap(RecognitionParams options) {
            int maxDy = (int) Math.Ceiling(options.width * options.maxAngleFactor);
            double[] angleMap = new double[maxDy * 2 + 1];
            for (int dy = -maxDy; dy <= maxDy; dy++) {
                angleMap[maxDy + dy] = (double) dy / (double) options.width;
            }
            return angleMap;
        }

        public static int[,] HoughTransform(List<Point> points, RecognitionParams options) {
            double[] angleMap = GetAngleMap(options);
            int[,] hough = new int[angleMap.Length, options.height];

            for (int a = 0; a < angleMap.Length; a++) {
                foreach (Point pt in points) {
                    int y = (int) Math.Round(pt.Y - pt.X * angleMap[a]);
                    if (y >= 0 && y < options.height) hough[a, y]++;
                }
            }

            return hough;
        }

        public static List<Point> FindHoughPeaks(int[,] hough, RecognitionParams options) {
            int width = hough.GetLength(0);
            int height = hough.GetLength(1);

            int maxHough = 0;
            foreach (var h in hough) {
                if (h > maxHough) maxHough = h;
            }

            List<Point> thresholdedPoints = new List<Point>();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (hough[x, y] * 255 / maxHough > options.houghThreshold)
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

        public static List<RawLine> ExtractRawLines(List<Point> houghPeaks, RecognitionParams options) {
            double[] angleMap = GetAngleMap(options);
            List<RawLine> lines = new List<RawLine>();
            foreach (var pt in houghPeaks) {
                lines.Add(new RawLine { yInt = pt.Y, k = angleMap[pt.X] });
            }
            return lines;
        }

        public static List<Line> FormLines(List<Point> edgePoints, List<RawLine> rawLines, RecognitionParams options) {
            List<Line> lines = new List<Line>();

            foreach (var rawLine in rawLines) {
                bool[] linePoints = new bool[options.width];
                foreach (var pt in edgePoints) {
                    if (Math.Abs(rawLine.yInt - (pt.Y - pt.X * rawLine.k)) < 2) {
                        linePoints[pt.X] = true;
                    }
                }
                var ld = new SimpleLineDetector(linePoints);
                lines.AddRange(ld.GetLines(x => (int) Math.Round(rawLine.yInt + x * rawLine.k)));
            }

            return lines;
        }

        public static List<Line> ExtractLines(List<Point> edgePoints, List<Point> houghPeaks, RecognitionParams options) {
            return FormLines(edgePoints, ExtractRawLines(houghPeaks, options), options);
            //double[] angleMap = GetAngleMap(options);
            //List<Line> lines = new List<Line>();
            //foreach (var pt in houghPeaks) {
            //    RawLine raw = new RawLine { yInt = pt.Y, k = angleMap[pt.X] };
            //    lines.Add(
            //        new Line(
            //            new Point(0, raw.yInt),
            //            new Point(options.width - 1, raw.yInt + (int) Math.Round(raw.k * options.width))));
            //}
            //return lines;
        }

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
