using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;

namespace LineOCR {
    public class LineNormalization {

        public double angle;
        public List<LineF> normHorizLines;
        public List<LineF> normVertLines;
        public List<LineF> normRotVertLines;

        public LineNormalization(List<Line> horizLines, List<Line> vertLines, Bitmap src)
            : this(horizLines.Select(ln => new LineF(ln)).ToList(), vertLines.Select(ln => new LineF(ln)).ToList(), src) { }

        public LineNormalization(List<LineF> horizLines, List<LineF> vertLines, Bitmap src) {
            List<double> angles = new List<double>();
            angles.AddRange(horizLines.Select(ln => LineAngle(ln)));
            angles.AddRange(vertLines.Select(ln => LineAngle(ln)));
            angles.Sort();

            angle = angles[angles.Count / 2];

            normHorizLines = horizLines.Select(ln => RotateLine(ln, angle - LineAngle(ln))).ToList();
            normVertLines = vertLines.Select(ln => RotateLine(ln, angle - LineAngle(ln))).ToList();
            normRotVertLines = normVertLines.Select(ln =>
                new LineF(new PointF(src.Width - 1 - ln.p1.Y, ln.p1.X), new PointF(src.Width - 1 - ln.p2.Y, ln.p2.X))).ToList();
        }

        public static double LineAngle(LineF ln) {
            return Math.Atan2(ln.p2.Y - ln.p1.Y, ln.p2.X - ln.p1.X);
        }

        public static LineF RotateLine(LineF ln, double ang) {
            PointF center = PointOps.Mult(PointOps.Add(ln.p1, ln.p2), 0.5f);
            return RotateLineAroundPoint(ln, center, ang);
        }

        public static LineF RotateLineAroundPoint(LineF ln, PointF pt, double ang) {
            return new LineF(
                RotatePoint(ln.p1, pt, ang),
                RotatePoint(ln.p2, pt, ang));
        }

        public static PointF RotatePoint(PointF pt, PointF center, double ang) {
            double prevAngle = Math.Atan2(pt.Y - center.Y, pt.X - center.X);
            double newAngle = prevAngle + ang;
            double distance = PointOps.Distance(pt, center);
            PointF rotatedOffset = new PointF((float) (Math.Cos(newAngle) * distance), (float) (Math.Sin(newAngle) * distance));
            return PointOps.Add(center, rotatedOffset);
        }
    }
}
