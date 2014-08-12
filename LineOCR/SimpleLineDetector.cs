using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.Drawing;

namespace LineOCR {
    public class SimpleLineDetector {
        private static readonly int stopLength = 20;
        private static readonly int minSegmentLength = 100;

        private bool[] data;

        public SimpleLineDetector(bool[] data) {
            this.data = data;
        }

        public List<Line> GetLines(Func<int, int> getY) {
            Func<int, Point> pointAtX = x => new Point(x, getY(x));

            List<Line> lines = new List<Line>();
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
                            if (ln.Length() > minSegmentLength) lines.Add(ln);
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
                if (ln.Length() > minSegmentLength) lines.Add(ln);
            }

            return lines;
        }

    }
}
