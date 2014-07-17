using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GradeOCR {
    public class LineDetector {
        private int length;
        private static readonly int slidingWindowLength = 301;
        private static readonly int halfwindow = slidingWindowLength / 2;
        private static readonly double slidingWindowThreshold = 0.3;
        private static readonly int threshold = (int) Math.Ceiling(slidingWindowLength * slidingWindowThreshold);

        private int[] data;
        private int x = 0;

        public LineDetector(int length) {
            this.length = length;
            data = new int[length + slidingWindowLength - 1];
        }
        public void Advance(bool b) {
            if (b) {
                data[x] += 1;
                data[x + slidingWindowLength] -= 1;
            }
            data[x + 1] += data[x];
            x++;
        }
        public void Finish() {
            while (x < data.Length - 1) {
                data[x + 1] += data[x];
                x++;
            }
        }
        public List<Line> GetLines(Func<int, int> getY) {
            List<Line> lines = new List<Line>();
            int? stt = null;
            for (int x = 0; x < length; x++) {
                int dx = halfwindow + x;
                if (data[dx] >= threshold && !stt.HasValue) {
                    stt = x;
                } else if (data[dx] < threshold && stt.HasValue) {
                    lines.Add(new Line(new Point(stt.Value, getY(stt.Value)), new Point(x - 1, getY(x - 1))));
                    stt = null;
                }
            }
            return lines;
        }
    }
}
