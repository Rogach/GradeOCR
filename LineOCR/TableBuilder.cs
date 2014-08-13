using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;

namespace LineOCR {
    public class TableBuilder {
        public static readonly int sideEgdeThreshold = 100;

        public PointF horizNormal;
        public PointF vertNormal;
        public PointF invHorizNormal;
        public PointF invVertNormal;

        List<Line> rowLines;

        public Line leftEdge;
        public Line rightEdge;

        List<RowInfo> rows;

        public TableBuilder(LineNormalization lnorm) {
            double angle = lnorm.angle;
            
            horizNormal = new PointF((float) Math.Cos(angle), (float) Math.Sin(angle));
            vertNormal = new PointF((float) -Math.Sin(angle), (float) Math.Cos(angle));
            invHorizNormal = new PointF((float) Math.Cos(angle), (float) -Math.Sin(angle));
            invVertNormal = new PointF((float) Math.Sin(angle), (float) Math.Cos(angle));

            List<Line> horizLines = lnorm.normHorizLines;
            List<Line> vertLines = lnorm.normRotVertLines;

            List<float> allLeftEndPoints = horizLines.Select(ln => TableX(ln.p1)).OrderBy(x => x).ToList();
            List<float> allRightEndPoints = horizLines.Select(ln => TableX(ln.p2)).OrderBy(x => x).ToList();

            float leftMedian = allLeftEndPoints[allLeftEndPoints.Count / 2];
            float rightMedian = allRightEndPoints[allRightEndPoints.Count / 2];

            rowLines =
                horizLines
                .Where(ln => Math.Abs(TableX(ln.p1) - leftMedian) < sideEgdeThreshold)
                .Where(ln => Math.Abs(TableX(ln.p2) - rightMedian) < sideEgdeThreshold)
                .OrderBy(ln => TableY(ln.p1)).ToList();

            List<Point> leftEndPoints = rowLines.Select(ln => ln.p1).ToList();
            List<Point> rightEndPoints = rowLines.Select(ln => ln.p2).ToList();

            float leftX = leftEndPoints.Select(pt => TableX(pt)).Average();
            float rightX = rightEndPoints.Select(pt => TableX(pt)).Average();

            PointF leftEdgeTop = new PointF(leftX, TableY(leftEndPoints.First()));
            PointF leftEdgeBottom = new PointF(leftX, TableY(leftEndPoints.Last()));
            leftEdge = new Line(PointOps.TruncPt(ToPicture(leftEdgeTop)), PointOps.TruncPt(ToPicture(leftEdgeBottom)));

            PointF rightEdgeTop = new PointF(rightX, TableY(rightEndPoints.First()));
            PointF rightEdgeBottom = new PointF(rightX, TableY(rightEndPoints.Last()));
            rightEdge = new Line(PointOps.TruncPt(ToPicture(rightEdgeTop)), PointOps.TruncPt(ToPicture(rightEdgeBottom)));

            rows = new List<RowInfo>();
            for (int r = 0; r < rowLines.Count - 1; r++) {
                List<float> dividers = new List<float>();
                Line topLine = rowLines[r];
                Line bottomLine = rowLines[r + 1];
                foreach (var ln in vertLines) {
                    if (Math.Abs(TableX(ln.p1) - leftX) > 10 && Math.Abs(TableX(ln.p1) - rightX) > 10) {
                        if (TableY(ln.p1) - 5 <= TableY(topLine.p1) && TableY(ln.p2) + 5 >= TableY(bottomLine.p1)) {
                            dividers.Add((TableX(ln.p1) + TableX(ln.p2)) / 2);
                        }
                    }
                }
                rows.Add(new RowInfo { topLine = topLine, bottomLine = bottomLine, dividers = dividers });
            }
        }

        private class RowInfo {
            public Line topLine { get; set; }
            public Line bottomLine { get; set; }
            public List<float> dividers { get; set; }
        }

        

        public float TableX(PointF p) {
            return PointOps.DotProduct(p, horizNormal);
        }

        public float TableY(PointF p) {
            return PointOps.DotProduct(p, vertNormal);
        }

        public PointF ToTable(PointF p) {
            return new PointF(TableX(p), TableY(p));
        }

        public float PictureX(PointF p) {
            return PointOps.DotProduct(p, invHorizNormal);
        }

        public float PictureY(PointF p) {
            return PointOps.DotProduct(p, invVertNormal);
        }

        public PointF ToPicture(PointF p) {
            return new PointF(PictureX(p), PictureY(p));
        }

        public Bitmap DebugImage(Bitmap bw) {
            Bitmap res = new Bitmap(bw);

            Graphics g = Graphics.FromImage(res);

            g.DrawLine(new Pen(Color.Green, 4), leftEdge.p1, leftEdge.p2);
            g.DrawLine(new Pen(Color.Green, 4), rightEdge.p1, rightEdge.p2);
            foreach (var row in rowLines) {
                g.DrawLine(new Pen(Color.Red, 4), row.p1, row.p2);
            }

            foreach (var row in rows) {
                foreach (float d in row.dividers) {
                    g.DrawLine(new Pen(Color.Blue, 4),
                        PointOps.TruncPt(ToPicture(new PointF(d, ToTable(row.topLine.p1).Y))),
                        PointOps.TruncPt(ToPicture(new PointF(d, ToTable(row.bottomLine.p1).Y))));
                }
            }

            g.Dispose();

            return res;
        }
    }
}
