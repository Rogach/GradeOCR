using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;

namespace TableOCR {

    /*
     * For given set of vertical and horizontal lines,
     * rotates lines so that they all have the same angle
     * (so afterwards we can assume that table, rows and columns are strictly rectangular)
     */
    public class LineNormalization {

        public double angle;
        public List<LineF> normHorizLines;
        public List<LineF> normVertLines;
        public List<LineF> normRotVertLines;

        public LineNormalization(List<Line> horizLines, List<Line> vertLines, Bitmap src)
            : this(horizLines.Select(ln => new LineF(ln)).ToList(), vertLines.Select(ln => new LineF(ln)).ToList(), src) { }

        public LineNormalization(List<LineF> horizLines, List<LineF> vertLines, Bitmap src) {
            List<double> angles = new List<double>();
            angles.AddRange(horizLines.Select(ln => PointOps.LineAngle(ln)));
            angles.AddRange(vertLines.Select(ln => PointOps.LineAngle(ln)));
            angles.Sort();

            angle = angles[angles.Count / 2];

            normHorizLines = horizLines.Select(ln => PointOps.RotateLine(ln, angle - PointOps.LineAngle(ln))).ToList();
            normVertLines = vertLines.Select(ln => PointOps.RotateLine(ln, angle - PointOps.LineAngle(ln))).ToList();
            normRotVertLines = normVertLines.Select(ln =>
                new LineF(new PointF(src.Width - 1 - ln.p1.Y, ln.p1.X), new PointF(src.Width - 1 - ln.p2.Y, ln.p2.X))).ToList();
        }
    }
}
