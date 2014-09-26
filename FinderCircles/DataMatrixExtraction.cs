using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace ARCode {

    public class DataMatrixExtraction {

        private FinderPatternPair fpp;

        double angX;
        PointF topLeft;
        PointF bottomLeft;
        PointF topRight;
        PointF bottomRight;

        public Bitmap rotatedMatrix;
        public bool[] extractedData;

        public DataMatrixExtraction(Bitmap sourceImage, FinderPatternPair fpp) {
            this.fpp = fpp;

            PointF p1 = fpp.p1.X < fpp.p2.X ? fpp.p1.ToF() : fpp.p2.ToF();
            PointF p2 = fpp.p1.X < fpp.p2.X ? fpp.p2.ToF() : fpp.p1.ToF();
            PointF vect = PointOps.Mult(PointOps.Sub(p2, p1), 0.1f);
            float unit = (float) PointOps.Distance(vect);

            PointF normX = PointOps.Normalize(vect);
            angX = Math.Atan2(normX.Y, normX.X);
            double angY = angX - Math.PI / 2;
            PointF normY = new PointF((float) Math.Cos(angY), (float) Math.Sin(angY));

            topLeft = PointOps.Add(p1, PointOps.Add(PointOps.Mult(normX, unit), PointOps.Mult(normY, unit)));
            bottomLeft = PointOps.Add(p1, PointOps.Sub(PointOps.Mult(normX, unit), PointOps.Mult(normY, unit)));
            topRight = PointOps.Add(p1, PointOps.Add(PointOps.Mult(normX, unit * 9), PointOps.Mult(normY, unit)));
            bottomRight = PointOps.Add(p1, PointOps.Sub(PointOps.Mult(normX, unit * 9), PointOps.Mult(normY, unit)));

            rotatedMatrix = new Bitmap((int) Math.Ceiling(unit * 8), (int) Math.Ceiling(unit * 2), PixelFormat.Format32bppArgb);
            Graphics rotG = Graphics.FromImage(rotatedMatrix);
            rotG.RotateTransform((float) (-angX * 180 / Math.PI));

            float diagonal = (float) PointOps.Distance(p1, p2);
            rotG.DrawImage(
                sourceImage,
                new RectangleF(-diagonal, -diagonal, diagonal * 2, diagonal * 2),
                new RectangleF(topLeft.X - diagonal, topLeft.Y - diagonal, diagonal * 2, diagonal * 2),
                GraphicsUnit.Pixel);
            rotG.Dispose();

            int[] cellSum = new int[DataMatrixDrawer.rowCount * DataMatrixDrawer.columnCount];
            unsafe {
                BitmapData bd = rotatedMatrix.LockBits(ImageLockMode.ReadOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < rotatedMatrix.Height; y++) {
                    for (int x = 0; x < rotatedMatrix.Width; x++) {
                        int cx = (int) Math.Floor((float) x * DataMatrixDrawer.columnCount / rotatedMatrix.Width);
                        int cy = (int) Math.Floor((float) y * DataMatrixDrawer.rowCount / rotatedMatrix.Height);
                        cellSum[cy * DataMatrixDrawer.columnCount + cx] += *ptr;
                        ptr += 4;
                    }
                }

                rotatedMatrix.UnlockBits(bd);
            }
            double threshold = ValueClustering.DivThreshold(cellSum);
            extractedData = new bool[DataMatrixDrawer.rowCount * DataMatrixDrawer.columnCount];
            for (int q = 0; q < DataMatrixDrawer.rowCount * DataMatrixDrawer.columnCount; q++) {
                extractedData[q] = cellSum[q] < threshold;
            }
        }

        public void DrawPositioningDebug(Bitmap img) {
            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.HighQuality;
            Pen gp = new Pen(Color.Green, 2);
            g.DrawLine(gp, topLeft, bottomLeft);
            g.DrawLine(gp, bottomLeft, bottomRight);
            g.DrawLine(gp, bottomRight, topRight);
            g.DrawLine(gp, topRight, topLeft);
            g.Dispose();

        }

        public Bitmap RecognitionDebugImage() {
            Bitmap res = new Bitmap(rotatedMatrix);

            float cellWidth = (float) res.Width / DataMatrixDrawer.columnCount;
            float cellHeight = (float) res.Height / DataMatrixDrawer.rowCount;

            Graphics g = Graphics.FromImage(res);
            for (int y = 0; y < DataMatrixDrawer.rowCount; y++) {
                for (int x = 0; x < DataMatrixDrawer.columnCount; x++) {
                    var gp = new GraphicsPath();
                    gp.AddPolygon(new PointF[] { 
                        new PointF(cellWidth * x, cellHeight * y),
                        new PointF(cellWidth * (x + 1), cellHeight * y),
                        new PointF(cellWidth * (x + 1), cellHeight * (y + 1)),
                        new PointF(cellWidth * x, cellHeight * (y + 1))
                    });
                    Color cellColor = extractedData[y * DataMatrixDrawer.columnCount + x] ? Color.Blue : Color.Red;
                    g.FillPath(new SolidBrush(Color.FromArgb(50, cellColor)), gp);
                }
            }
            g.Dispose();

            return res;
        }
    }
}
