using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OCRUtil {
    public class LineF {
        public PointF p1;
        public PointF p2;

        public LineF(PointF p1, PointF p2) {
            this.p1 = p1;
            this.p2 = p2;
        }

        public LineF(Line ln) {
            this.p1 = ln.p1;
            this.p2 = ln.p2;
        }

        public double Length() {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString() {
            return String.Format("Line({0} -> {1})", p1, p2);
        }
    }
}
