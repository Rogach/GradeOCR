using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace LineOCR {
    public static class VerticalArtifactRemoval {

        public static List<Line> RemoveArtifactLines(List<Line> lines, RecognitionParams options) {
            int median1 = lines.Select(ln => ln.p1.X).OrderBy(x => x).ToList()[lines.Count / 2];
            int median2 = lines.Select(ln => ln.p2.X).OrderBy(x => x).ToList()[lines.Count / 2];
            return lines.Where(ln =>
                Math.Abs(ln.p1.X - median1) < options.verticalDisparityThreshold &&
                Math.Abs(ln.p2.X - median2) < options.verticalDisparityThreshold).ToList();
        }

        private static readonly int imageWidth = 200;

        public static Bitmap LengthMap(List<Line> lines, RecognitionParams options) {
            Bitmap res = new Bitmap(imageWidth, options.height, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(res);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, res.Width, res.Height));
            g.Dispose();

            List<int> x1s = lines.Select(ln => ln.p1.X).OrderBy(x => x).ToList();
            List<int> x2s = lines.Select(ln => ln.p2.X).OrderBy(x => x).ToList();

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                DrawPoints(ptr, x1s, options);
                DrawPoints(ptr, x2s, options);

                res.UnlockBits(bd);
            }

            return res;
        }

        private static unsafe void DrawPoints(uint* ptr, List<int> points, RecognitionParams options) {
            int median = points[points.Count / 2];

            foreach (int pt in points)
                for (int x = 0; x < imageWidth; x++)
                    *(ptr + pt * imageWidth + x) = 0xff000000;

            for (int x = 0; x < imageWidth; x++)
                *(ptr + median * imageWidth + x) = 0xff00ff00;

            if (median - options.verticalDisparityThreshold >= 0)
                for (int x = 0; x < imageWidth; x++)
                    *(ptr + (median - options.verticalDisparityThreshold) * imageWidth + x) = 0xffff0000;

            if (median + options.verticalDisparityThreshold < options.height)
                for (int x = 0; x < imageWidth; x++)
                    *(ptr + (median + options.verticalDisparityThreshold) * imageWidth + x) = 0xffff0000;
        }
    }
}