using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace TableOCR {
    public static class EdgeExtraction {

        public static List<Point> ExtractEdgePoints(Bitmap src) {
            List<Point> edgePoints = new List<Point>();

            unsafe {
                BitmapData bd = src.LockBits(ImageLockMode.ReadOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                // for some reason, calculation of these expression
                // is not moved outside the inner loops
                // execution time horribly suffers
                int srcWidth = src.Width;
                int srcHeight = src.Height;

                ptr += srcWidth;
                for (int y = 1; y < srcHeight; y++) {
                    for (int x = 0; x < srcWidth; x++) {
                        if (*ptr == 0xff000000 && *(ptr - srcWidth) == 0xffffffff) {
                            edgePoints.Add(new Point(x, y));
                        }
                        ptr++;
                    }
                }

                src.UnlockBits(bd);
            }

            return edgePoints;
        }

        public static Bitmap DrawPoints(Bitmap src, List<Point> points) {
            Bitmap res = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, res.Width, res.Height));

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                IEnumerable<Point> orderedPoints = points.OrderBy(pt => pt.X).OrderBy(pt => pt.Y);
                foreach (var pt in orderedPoints) {
                    *(ptr + pt.Y * src.Width + pt.X) = 0xff000000;
                }

                res.UnlockBits(bd);
            }

            return res;
        }

    }
}
