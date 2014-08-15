using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.Drawing;

namespace TableOCR {

    public class SegmentDetector {
        private static readonly int stopLength = 20;
        private static readonly int minSegmentLength = 100;

        /*
         * Detect line segments in a sequence of black and white pixels.
         * Sequence of black/white pixels is represented by simple boolean array (black == true).
         * Algorithm is very simple - extract sequences of points, that have distance to adjacent
         * point less than `stopLength`. Retains only segments bigger than `minSegmentLength`.
         */
        public static List<Line> GetSegments(bool[] data, RawLine rawLine) {
            Func<int, Point> pointAtX = x => new Point(x, (int) Math.Round(rawLine.yInt + x * rawLine.k));

            List<Line> segments = new List<Line>();
            int? sttX = null;
            int? endX = null;
            for (int x = 0; x < data.Length; x++) {
                if (data[x]) {
                    if (!sttX.HasValue) {
                        sttX = x;
                        endX = null;
                    } else {
                        endX = x;
                    }
                } else {
                    if (endX.HasValue) {
                        if (endX.Value + stopLength < x) {
                            var ln = new Line(pointAtX(sttX.Value), pointAtX(endX.Value));
                            if (ln.Length() > minSegmentLength) segments.Add(ln);
                            sttX = null;
                            endX = null;
                        }
                    } else {
                        if (sttX.HasValue) {
                            if (sttX.Value + stopLength < x) {
                                sttX = null;
                                endX = null;
                            }
                        }
                    }
                }
            }
            if (sttX.HasValue && endX.HasValue) {
                var ln = new Line(pointAtX(sttX.Value), pointAtX(endX.Value));
                if (ln.Length() > minSegmentLength) segments.Add(ln);
            }

            return segments;
        }

        /*
         * Forms continuos line from set of segment.
         * Discards segments too far from main body, if needed.
         */
        public static Line GetSolidLine(List<Line> segments) {
            Line totalLine = new Line(segments.First().p1, segments.Last().p2);
            if (segments.Sum(ln => ln.Length()) / totalLine.Length() > 0.7) {
                return totalLine;
            } else {
                if (segments.First().Length() > segments.Last().Length()) {
                    return GetSolidLine(segments.GetRange(0, segments.Count - 1));
                } else {
                    return GetSolidLine(segments.Skip(1).ToList());
                }
            }
        }

    }
}
