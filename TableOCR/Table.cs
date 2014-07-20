using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using LibUtil;
using System.Drawing.Imaging;

namespace TableOCR {
    
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

        public PointF GetTopLeftCellCorner(int x, int y) {
            PointF p = origin;
            for (int q = 0; q < x; q++) {
                p = PointOps.Add(p, PointOps.Mult(horizontalNormal, columnWidths[q]));
            }
            for (int q = 0; q < y; q++) {
                p = PointOps.Add(p, PointOps.Mult(verticalNormal, rowHeights[q]));
            }
            return p;
        }

        public GraphicsPath GetCellContour(int x, int y) {
            var gp = new GraphicsPath();
            gp.AddPolygon(
                new PointF[] { 
                    GetTopLeftCellCorner(x, y),
                    GetTopLeftCellCorner(x + 1, y),
                    GetTopLeftCellCorner(x + 1, y + 1),
                    GetTopLeftCellCorner(x, y + 1)
                }
            );
            return gp;
        }

        public Option<Point> GetCellAtPoint(float px, float py) {
            px -= origin.X;
            py -= origin.Y;
            float hx = horizontalNormal.X;
            float hy = horizontalNormal.Y;
            float vx = verticalNormal.X;
            float vy = verticalNormal.Y;
            float v = (py * hx - px * hy) / (hx * vy - vx * hy);
            float h = (px - v * vx) / vy;

            if (v < 0 || h < 0 || v >= totalHeight || h >= totalWidth) {
                return new None<Point>();
            } else {
                int col = 0;
                while (h > columnWidths[col]) {
                    h -= columnWidths[col];
                    col++;
                }
                int row = 0;
                while (v > rowHeights[row]) {
                    v -= rowHeights[row];
                    row++;
                }
                return new Some<Point>(new Point(col, row));
            }
        }

        public Bitmap GetCellImage(Bitmap img, int x, int y) {
            int padding = 1;

            int w = (int) Math.Floor(columnWidths[x]);
            int h = (int) Math.Floor(rowHeights[y]);
            float ang = (float) (Math.Atan(horizontalNormal.Y / horizontalNormal.X) / Math.PI * 180);
            
            Bitmap cell = new Bitmap(w - padding * 2, h - padding * 2, PixelFormat.Format32bppArgb);
            PointF pt = GetTopLeftCellCorner(x, y);
            Graphics g = Graphics.FromImage(cell);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.RotateTransform(-ang);
            g.DrawImage(
                img,
                new RectangleF(0, 0, w + 10, h + 10),
                new RectangleF((float) Math.Floor(pt.X + padding), (float) Math.Floor(pt.Y + padding), w + 10, h + 10),
                GraphicsUnit.Pixel);
            g.Dispose();
            return cell;
        }

    }
}
