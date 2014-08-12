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

        public static double Distance(PointF p1, PointF p2) {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
