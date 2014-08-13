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
        public List<Line> normHorizLines;
        public List<Line> normVertLines;
        public List<Line> normRotVertLines;

        public LineNormalization(List<Line> horizLines, List<Line> vertLines, Bitmap src) {
            List<double> angles = new List<double>();
            angles.AddRange(horizLines.Select(ln => LineAngle(ln)));
            angles.AddRange(vertLines.Select(ln => LineAngle(ln)));
            angles.Sort();

            angle = angles[angles.Count / 2];

            normHorizLines = horizLines.Select(ln => RotateLine(ln, LineAngle(ln) - angle)).ToList();
            normVertLines = vertLines.Select(ln => RotateLine(ln, LineAngle(ln) - angle)).ToList();
            normRotVertLines = normVertLines.Select(ln =>
                new Line(new Point(src.Width - 1 - ln.p1.Y, ln.p1.X), new Point(src.Width - 1 - ln.p2.Y, ln.p2.X))).ToList();
        }

        private static double LineAngle(Line ln) {
            return Math.Atan2(ln.p2.Y - ln.p1.Y, ln.p2.X - ln.p1.X);
        }

        private static Line RotateLine(Line ln, double ang) {
            Point center = PointOps.TruncPt(PointOps.Mult(PointOps.Add(ln.p1, ln.p2), 0.5f));
            return RotateLineAroundPoint(ln, center, ang);
        }

        private static Line RotateLineAroundPoint(Line ln, Point pt, double ang) {
            return new Line(
                RotatePoint(ln.p1, pt, ang),
                RotatePoint(ln.p2, pt, ang));
        }

        private static Point RotatePoint(Point pt, Point center, double ang) {
            double prevAngle = Math.Atan2(pt.Y - center.Y, pt.X - center.X);
            double newAngle = prevAngle + ang;
            double distance = PointOps.Distance(pt, center);
            PointF rotatedOffset = new PointF((float) Math.Round(Math.Cos(newAngle) * distance), (float) Math.Round(Math.Sin(newAngle) * distance));
            return PointOps.TruncPt(PointOps.Add(center, rotatedOffset));
        }
    }
}
