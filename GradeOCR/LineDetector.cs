using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GradeOCR {
    public class LineDetector {
        private static readonly int momentumFactor = 10;

        private int length;
        private bool[] data;
        private int X = 0;

        public LineDetector(int length) {
            this.length = length;
            data = new bool[length];
        }
        public void Advance(bool b) {
            data[X++] = b;
        }
        public void Finish() {
            bool[] transformed = new bool[length];
            // left pass
            int momentum = 0;
            int pixelCount = 0;
            for (int x = 0; x < length; x++) {
                if (data[x]) {
                    pixelCount++;
                    transformed[x] = true;
                } else {
                    if (pixelCount > 0) {
                        momentum += pixelCount / momentumFactor;
                        pixelCount = 0;
                    }
                    if (momentum > 0) {
                        transformed[x] = true;
                        momentum--;
                    }
                }
            }

            // right pass
            momentum = 0;
            pixelCount = 0;
            for (int x = length - 1; x >= 0; x--) {
                if (data[x]) {
                    pixelCount++;
                    transformed[x] = true;
                } else {
                    if (pixelCount > 0) {
                        momentum += pixelCount / momentumFactor;
                        pixelCount = 0;
                    }
                    if (momentum > 0) {
                        transformed[x] = true;
                        momentum--;
                    }
                }
            }
            data = transformed;
        }
        public List<Line> GetLines(Func<int, int> getY) {
            List<Line> lines = new List<Line>();
            int? stt = null;
            for (int x = 0; x < length; x++) {
                if (data[x] && !stt.HasValue) {
                    stt = x;
                } else if (!data[x] && stt.HasValue) {
                    Line l = new Line(new Point(stt.Value, getY(stt.Value)), new Point(x - 1, getY(x - 1)));
                    if (l.Length() > 400) {
                        lines.Add(l);
                    }
                    stt = null;
                }

            }
            return lines;
        }
    }
}
