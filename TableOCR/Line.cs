using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace TableOCR {
    public class Line {
        public Point p1;
        public Point p2;

        public Line(Point p1, Point p2) {
            this.p1 = p1;
            this.p2 = p2;
        }

        public double Length() {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public double Tangent() {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;
            return (double) dy / (double) dx;
        }

        public int Y_atZero() {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double k = dy / dx;
            double a = p1.Y - k * p1.X;
            return (int) Math.Round(a);
        }

        public override string ToString() {
            return String.Format("Line({0} -> {1})", p1, p2);
        }
    }
}
