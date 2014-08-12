using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using LibUtil;
using System.Drawing;
using System.Drawing.Imaging;

namespace LineOCR {
    public static class LineFilter {

        public static List<Line> ExtractLines(List<Point> edgePoints, List<RawLine> rawLines, RecognitionParams options) {
            List<Line> lines = new List<Line>();

            foreach (var rawLine in rawLines) {
                bool[] linePoints = new bool[options.width];
                foreach (var pt in edgePoints) {
                    if (Math.Abs(rawLine.yInt - (pt.Y - pt.X * rawLine.k)) < 2) {
                        linePoints[pt.X] = true;
                    }
                }
                var ld = new SimpleLineDetector(linePoints);
                var segments = ld.GetLines(x => (int) Math.Round(rawLine.yInt + x * rawLine.k));
                if (segments.Count > 0) {
                    if (!HasCyclicPatterns(linePoints, segments.First().p1.X, segments.Last().p2.X, options))
                        lines.Add(new Line(segments.First().p1, segments.Last().p2));
                }
            }

            return lines;
        }

        public static bool HasCyclicPatterns(bool[] linePoints, int from, int to, RecognitionParams options) {
            for (int w = options.cyclicPatternsMinWidth; w <= options.cyclicPatternsMaxWidth; w++) {
                int[] acc = new int[w];
                for (int i = from; i <= to; i++) {
                    if (linePoints[i])
                        acc[(i - from) % w]++;
                }
                int max = acc.Max();
                int sum = acc.Sum();
                double ratio = (double) sum / (max * acc.Length);
                if (ratio < 0.5) return true;
            }
            return false;
        }

        public static Bitmap CyclicPatternsImage(bool[] linePoints, int from, int to) {
            int windowFrom = 10;
            int windowTo = 100;
            int extraWidth = 50;
            Bitmap res = new Bitmap(windowTo + extraWidth, windowTo - windowFrom + 1, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int w = windowFrom; w <= windowTo; w++) {
                    int[] acc = new int[w];
                    for (int i = from; i <= to; i++) {
                        if (linePoints[i]) {
                            acc[(i - from) % w]++;
                        }
                    }
                    int max = acc.Max();
                    int sum = acc.Sum();
                    double ratio = (double) sum / (max * acc.Length);
                    for (int i = 0; i < w; i++) {
                        byte v = (byte) (acc[i] * 255 / max);
                        *ptr = v;
                        *(ptr + 1) = v;
                        *(ptr + 2) = v;
                        *(ptr + 3) = 255;
                        ptr += 4;
                    }
                    
                    for (int i = w; i < windowTo + extraWidth; i++) {
                        if (ratio < 0.5) {
                            *ptr = 0;
                            *(ptr + 1) = 0;
                            *(ptr + 2) = 255;
                            *(ptr + 3) = 255;
                        }
                        ptr += 4;
                    }
                }

                res.UnlockBits(bd);
            }

            return res;
        }

        public static Bitmap CyclicPatternsInLines(List<Point> edgePoints, List<RawLine> rawLines, RecognitionParams options) {
            rawLines = rawLines.OrderBy(l => l.yInt).ToList();

            List<Bitmap> images = new List<Bitmap>();

            foreach (var rawLine in rawLines) {
                bool[] linePoints = new bool[options.width];
                foreach (var pt in edgePoints) {
                    if (Math.Abs(rawLine.yInt - (pt.Y - pt.X * rawLine.k)) < 2) {
                        linePoints[pt.X] = true;
                    }
                }
                var ld = new SimpleLineDetector(linePoints);
                var lines = ld.GetLines(x => (int) Math.Round(rawLine.yInt + x * rawLine.k));
                if (lines.Count > 0) {
                    images.Add(CyclicPatternsImage(linePoints, lines.First().p1.X, lines.Last().p2.X));
                }
            }

            return ImageUtil.VerticalConcat(images);
        }
    }
}
