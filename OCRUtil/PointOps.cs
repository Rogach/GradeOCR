using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OCRUtil {
    public static class PointOps {
        public static PointF Intersection(Line l1, Line l2) {
            PointF p = l1.p1;
            PointF r = Sub(l1.p2, l1.p1);
            PointF q = l2.p1;
            PointF s = Sub(l2.p2, l2.p1);

            float rXs = CrossProduct(r, s);
            if (rXs == 0) {
                throw new Exception("lines are parallel");
            } else {
                PointF qp = Sub(q, p);
                float t = CrossProduct(qp, s) / rXs;
                float u = CrossProduct(qp, r) / rXs;
                return Add(p, Mult(r, t));
            }
        }

        public static PointF Add(PointF p1, PointF p2) {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointF Sub(PointF p1, PointF p2) {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointF Mult(PointF p, float m) {
            return new PointF(p.X * m, p.Y * m);
        }

        public static float DotProduct(PointF p1, PointF p2) {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        public static float CrossProduct(PointF p1, PointF p2) {
            return p1.X * p2.Y - p1.Y * p2.X;
        }

        public static PointF Normalize(PointF p) {
            float ln = (float) Math.Sqrt(p.X * p.X + p.Y * p.Y);
            return new PointF(p.X / ln, p.Y / ln);
        }

        public static PointF FromLine(Line l) {
            return new PointF(l.p2.X - l.p1.X, l.p2.Y - l.p1.Y);
        }

        public static double Distance(PointF p) {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static double Distance(PointF p1, PointF p2) {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Distance(int x1, int y1, int x2, int y2) {
            int dx = x1 - x2;
            int dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static Point TruncPt(PointF p) {
            return new Point((int) Math.Round(p.X), (int) Math.Round(p.Y));
        }

        public static PointF ToF(this Point p) {
            return new PointF(p.X, p.Y);
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
