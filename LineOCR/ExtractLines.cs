using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace LineOCR {
    public static class ExtractLines {
        public static readonly float maxAngleFactor = 0.03f;
        public static readonly float minHorizontalLineRatio = 0.5f;
        public static readonly float minVerticalLineRatio = 0.1f;

        public static List<Line> ExtractLineSegments(Bitmap src) {
            List<Line> lineSegments = new List<Line>();

            unsafe {
                BitmapData srcBD = src.LockBits(ImageLockMode.ReadOnly);
                uint* srcPtr = (uint*) srcBD.Scan0.ToPointer();

                // for some reason, calculation of these expression
                // is not moved outside the inner loops
                // execution time horribly suffers
                int srcWidth = src.Width; 
                int srcHeight = src.Height;

                srcPtr += src.Width;

                for (int y = 1; y < srcHeight; y++) {
                    int? startX = null;
                    for (int x = 0; x < srcWidth; x++) {
                        if (*srcPtr == 0xff000000 && *(srcPtr - srcWidth) == 0xffffffff) {
                            if (!startX.HasValue) {
                                startX = x;
                            }
                        } else {
                            if (startX.HasValue) {
                                if (startX.Value + 20 < x) {
                                    lineSegments.Add(new Line(new Point(startX.Value, y), new Point(x - 1, y)));
                                }
                                startX = null;
                            }
                        }
                        srcPtr++;
                    }
                }

                src.UnlockBits(srcBD);
            }

            return lineSegments;
        }

        public static Bitmap DrawLineSegments(Bitmap src, List<Line> lineSegments) {
            Bitmap res = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);

            g.FillRectangle(Brushes.White, new Rectangle(0, 0, res.Width, res.Height));

            List<Line> lines = CombineIntoLongLines(lineSegments, src.Width, 400);
            Console.WriteLine("lines found: " + lines.Count);

            foreach (var ln in lines) {
                g.DrawLine(Pens.Black, ln.p1, ln.p2);
            }

            g.Dispose();
            return res;
        }

        private class LineYComparer : IComparer<Line> {
            public int Compare(Line ln1, Line ln2) {
                return ln1.p1.Y - ln2.p1.Y;
            }
        }

        public static List<Line> CombineIntoLongLines(List<Line> lineSegments, int imageWidth, int minLineLength) {
            List<Line> longLines = new List<Line>();

            SortedSet<Line> unusedSegments = new SortedSet<Line>(new LineYComparer());
            Queue<Line> lineQueue = new Queue<Line>();
            foreach (var seg in lineSegments.OrderBy(ln => ln.p1.X).OrderBy(ln => ln.p1.Y)) {
                unusedSegments.Add(seg);
                lineQueue.Enqueue(seg);
            }

            while (lineQueue.Count > 0) {
                Line seg = lineQueue.Dequeue();
                unusedSegments.Remove(seg);

                Console.WriteLine("seg: " + seg);

                int maxDy = (int) Math.Ceiling(imageWidth * maxAngleFactor);

                List<Line> lines = 
                    unusedSegments.GetViewBetween(
                        new Line(new Point(0, seg.p1.Y - maxDy), new Point(0, seg.p1.Y - maxDy)),
                        new Line(new Point(0, seg.p1.Y + maxDy), new Point(0, seg.p1.Y + maxDy))).ToList();

                var bestMatchOpt = lines.Select(endSeg => {
                    double lineAngle = Math.Atan((double) (endSeg.p2.X - seg.p1.X) / (double) (endSeg.p2.Y - seg.p1.Y));
                    //Console.WriteLine("line angle: " + lineAngle);
                    List<Line> matchingSegs = lines.Where(ln => {
                        double ang1 = Math.Atan((double) (ln.p1.X - seg.p1.X) / (double) (ln.p1.Y - seg.p1.Y));
                        double ang2 = Math.Atan((double) (ln.p2.X - seg.p1.X) / (double) (ln.p2.Y - seg.p1.Y));
                        return ln.p1.X > seg.p2.X && ln.p2.X < endSeg.p1.X && Math.Abs(ang1 - lineAngle) < 0.1 && Math.Abs(ang2 - lineAngle) < 0.1;
                    }).ToList();
                    return new Tuple<Line, double, List<Line>>(endSeg, lineAngle, matchingSegs);
                }).Where(t => {
                    int xlength = t.Item1.p2.X - seg.p1.X;
                    return xlength > minLineLength;
                }).MaxByOption(t => {
                    int xlength = t.Item1.p2.X - seg.p1.X;
                    return (double) ((seg.p2.X - seg.p1.X) + (t.Item1.p2.X - seg.p2.X) + t.Item3.Select(ln => ln.p2.X - ln.p1.X).Sum()) / xlength;
                });

                if (bestMatchOpt.IsEmpty()) {
                    longLines.Add(seg);
                    Console.WriteLine("sole seg");
                } else {
                    var bestMatch = bestMatchOpt.Get();
                    unusedSegments.Remove(bestMatch.Item1);
                    foreach (var ln in bestMatch.Item3) {
                        unusedSegments.Remove(ln);
                    }
                    Console.WriteLine("segments in line: " + bestMatch.Item3.Count);
                    longLines.Add(new Line(seg.p1, bestMatch.Item1.p2));
                }
            }

            return longLines;
            
        }
    }
}
