using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;
using System.Drawing.Imaging;

namespace ARCode {
    public class DataMatrixExtraction {

        private Bitmap sourceImage;
        private FinderPatternPair fpp;

        double angX;
        PointF topLeft;
        PointF bottomLeft;
        PointF topRight;
        PointF bottomRight;

        public Bitmap rotatedMatrix;
        public bool[] extractedData;

        public DataMatrixExtraction(Bitmap sourceImage, FinderPatternPair fpp) {
            this.sourceImage = sourceImage;
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

            int[,] cellSum = new int[DataMatrixDrawer.rowCount, DataMatrixDrawer.columnCount];
            unsafe {
                BitmapData bd = rotatedMatrix.LockBits(ImageLockMode.ReadOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                for (int y = 0; y < rotatedMatrix.Height; y++) {
                    for (int x = 0; x < rotatedMatrix.Width; x++) {
                        int cx = (int) Math.Floor((float) x * DataMatrixDrawer.columnCount / rotatedMatrix.Width);
                        int cy = (int) Math.Floor((float) y * DataMatrixDrawer.rowCount / rotatedMatrix.Height);
                        cellSum[cy, cx] += *ptr;
                        ptr += 4;
                    }
                }

                rotatedMatrix.UnlockBits(bd);
            }
            int threshold = (rotatedMatrix.Width * rotatedMatrix.Height) / (DataMatrixDrawer.columnCount * DataMatrixDrawer.rowCount) / 2 * 256;
            extractedData = new bool[DataMatrixDrawer.rowCount * DataMatrixDrawer.columnCount];
            for (int y = 0; y < DataMatrixDrawer.rowCount; y++) {
                for (int x = 0; x < DataMatrixDrawer.columnCount; x++) {
                    extractedData[x * DataMatrixDrawer.rowCount + y] = cellSum[y, x] < threshold;
                }
            }
        }

        public Bitmap PositioningDebugImage() {
            Bitmap res = new Bitmap(sourceImage);

            Graphics g = Graphics.FromImage(res);
            Pen gp = new Pen(Color.Green, 1);
            g.DrawLine(gp, topLeft, bottomLeft);
            g.DrawLine(gp, bottomLeft, bottomRight);
            g.DrawLine(gp, bottomRight, topRight);
            g.DrawLine(gp, topRight, topLeft);
            g.Dispose();

            return res;
        }
    }
}
