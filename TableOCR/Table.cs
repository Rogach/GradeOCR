﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using LibUtil;
using System.Drawing.Imaging;
using OCRUtil;

namespace TableOCR {
    
    /*
     * Contains information about recognized table.
     * Carries row/column dimensions, table origin, table rotation.
     */
    public class Table {
        /* Top-left table point */
        public PointF origin;
        /* Normal vector (oriented in row direction) */
        public PointF horizontalNormal;
        /* Normal vector (oriented in column direction) */
        public PointF verticalNormal;
        
        public List<float> rowHeights;
        public float totalHeight;
        public List<float> columnWidths;
        public float totalWidth;

        /*
         * Draw a table on provided graphics canvas with provided pen.
         */
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

        private bool HasCellAt(int x, int y) {
            return x < columnWidths.Count && y < rowHeights.Count;
        }

        /* 
         * Locates top-left cell corner at `x` column and `y` row.
         */
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

        /*
         * Returns contour for cell at `x` column and `y` row.
         */ 
        public Option<GraphicsPath> GetCellContour(int x, int y) {
            if (HasCellAt(x, y)) {
                var gp = new GraphicsPath();
                gp.AddPolygon(
                    new PointF[] { 
                        GetTopLeftCellCorner(x, y),
                        GetTopLeftCellCorner(x + 1, y),
                        GetTopLeftCellCorner(x + 1, y + 1),
                        GetTopLeftCellCorner(x, y + 1)
                    }
                );
                return new Some<GraphicsPath>(gp);
            } else {
                return new None<GraphicsPath>();
            }
        }

        /*
         * Calculates row/column for given coordinates.
         * If given coordinates are outside the table, returns None.
         */
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

        /*
         * Extracts cell contents image from provided image in cell
         * of `x` column, `y` row.
         */
        public Option<Bitmap> GetCellImage(Bitmap img, int x, int y) {
            if (HasCellAt(x, y)) {
                int padding = 1;

                int w = (int) Math.Floor(columnWidths[x]);
                int h = (int) Math.Floor(rowHeights[y]);
                float ang = (float) (Math.Atan(horizontalNormal.Y / horizontalNormal.X) / Math.PI * 180);

                Bitmap cell = new Bitmap(w - padding * 2, h - padding * 2, PixelFormat.Format32bppArgb);
                PointF pt = GetTopLeftCellCorner(x, y);
                Graphics g = Graphics.FromImage(cell);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, cell.Width, cell.Height));
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                g.RotateTransform(-ang);
                g.TranslateTransform(0, -10);
                g.DrawImage(
                    img,
                    new RectangleF(0, 0, w + 10, h + 20),
                    new RectangleF((float) Math.Floor(pt.X + padding), (float) Math.Floor(pt.Y - 10 + padding), w + 10, h + 20),
                    GraphicsUnit.Pixel);
                g.Dispose();
                return new Some<Bitmap>(cell);
            } else {
                return new None<Bitmap>();
            }
        }

    }
}
