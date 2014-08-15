using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace TableOCR {
    public static class CyclicPatternDetector {
        public static bool HasCyclicPatterns(bool[] linePoints, int from, int to, RecognitionOptions options) {
            for (int w = options.cyclicPatternsMinWidth; w <= options.cyclicPatternsMaxWidth; w++) {
                int[] acc = new int[w];
                for (int i = from; i <= to; i++) {
                    if (linePoints[i])
                        acc[(i - from) % w]++;
                }

                int threshold = (to - from) / w / 5;
                double cyclicPatternSize = (double) acc.Where(a => a < threshold).Count() / w;
                if (cyclicPatternSize > 0.2) return true;

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

                    int threshold = (to - from) / w / 5;
                    double cyclicPatternSize = (double) acc.Where(a => a < threshold).Count() / w;

                    for (int i = 0; i < w; i++) {
                        byte v = (byte) (acc[i] * 255 / max);
                        *ptr = v;
                        *(ptr + 1) = v;
                        *(ptr + 2) = v;
                        *(ptr + 3) = 255;
                        ptr += 4;
                    }

                    for (int i = w; i < windowTo + extraWidth; i++) {
                        if (cyclicPatternSize > 0.2) {
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

        public static Bitmap CyclicPatternsInLines(List<Point> edgePoints, List<RawLine> rawLines, RecognitionOptions options) {
            rawLines = rawLines.OrderBy(l => l.yInt).ToList();

            List<Bitmap> images = new List<Bitmap>();

            foreach (var rawLine in rawLines) {
                bool[] linePoints = new bool[options.imageWidth];
                foreach (var pt in edgePoints) {
                    if (Math.Abs(rawLine.yInt - (pt.Y - pt.X * rawLine.k)) < 2) {
                        linePoints[pt.X] = true;
                    }
                }
                var lines = SegmentDetector.GetSegments(linePoints, rawLine);
                if (lines.Count > 0) {
                    images.Add(CyclicPatternsImage(linePoints, lines.First().p1.X, lines.Last().p2.X));
                }
            }

            return ImageUtil.VerticalConcat(images);
        }
    }
}
