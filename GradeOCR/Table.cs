using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GradeOCR {
    
    public class Table {
        private PointF origin;
        private PointF horizontalNormal;
        private PointF verticalNormal;
        private List<float> rowHeights;
        private float totalHeight;
        private List<float> columnWidths;
        private float totalWidth;

        public Table(List<Line> horizontalLines, List<Line> verticalLines) {
            // find horizontal lines that intersect all vertical ones
            List<Line> hLines = horizontalLines.Where(hln => {
                return verticalLines.All(vln => {
                    PointF i = PointOps.Intersection(hln, vln);
                    return hln.p1.X <= i.X && hln.p2.X >= i.X;
                });
            }).OrderBy(hln => hln.p1.Y).ToList();

            List<Line> vLines = verticalLines.OrderBy(vln => vln.p1.X).ToList();

            origin = PointOps.Intersection(hLines[0], vLines[0]);
            horizontalNormal = PointOps.Normalize(PointOps.FromLine(hLines[0]));
            verticalNormal = PointOps.Normalize(PointOps.FromLine(vLines[0]));

            columnWidths = new List<float>();
            totalWidth = 0;
            for (int q = 1; q < vLines.Count; q++) {
                PointF i = PointOps.Intersection(hLines[0], vLines[q]);
                float w = (i.X - origin.X) / horizontalNormal.X - totalWidth;
                columnWidths.Add(w);
                totalWidth += w;
            }

            rowHeights = new List<float>();
            for (int q = 1; q < hLines.Count; q++) {
                PointF i = PointOps.Intersection(vLines[0], hLines[q]);
                float h = (i.Y - origin.Y) / verticalNormal.Y - totalHeight;
                rowHeights.Add(h);
                totalHeight += h;
            }
        }

        public void DrawTable(Graphics g, Pen p) {
            PointF row = PointOps.Mult(horizontalNormal, totalWidth);
            PointF r = origin;
            g.DrawLine(p, r, PointOps.Add(r, row));
            for (int q = 0; q < rowHeights.Count; q++) {
                r = PointOps.Add(r, PointOps.Mult(verticalNormal, rowHeights[q]));
                g.DrawLine(p, r, PointOps.Add(r, row));
            }

            PointF col = PointOps.Mult(verticalNormal, totalHeight);
            PointF c = origin;
            g.DrawLine(p, c, PointOps.Add(c, col));
            for (int q = 0; q < columnWidths.Count; q++) {
                c = PointOps.Add(c, PointOps.Mult(horizontalNormal, columnWidths[q]));
                g.DrawLine(p, c, PointOps.Add(c, col));
            }
        }

    }
}
