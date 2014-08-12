using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using System.Drawing;
using LibUtil;

namespace LineOCR {
    public struct RecognitionParams {
        public float maxAngleFactor;
        public int houghThreshold;
        public int houghWindowWidth;
        public int houghWindowHeight;
        public int width;
        public int height;
        public int minLineLength;
    }

    public static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new LineRecognitionDebugForm(
                    ImageUtil.LoadImage(@"e:\Pronko\prj\Grader\ocr-data\register-test-input\reg019.jpg")));
        }

        public static Bitmap CreateHoughImage(Bitmap src) {
            Bitmap bw = ImageUtil.ToBlackAndWhite(src);
            Bitmap rotBw = ImageUtil.RotateCounterClockwise(bw);
            
            var horizOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 50,
                houghWindowWidth = 20,
                houghWindowHeight = 10, 
                width = bw.Width,
                height = bw.Height,
                minLineLength = bw.Width / 2
            };
            
            var vertOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 100,
                houghWindowWidth = 20,
                houghWindowHeight = 10,
                width = rotBw.Width,
                height = rotBw.Height,
                minLineLength = rotBw.Width / 10
            };
            
            List<Point> horizEdgePoints = ExtractLines.ExtractEdgePoints(bw);
            int[,] horizHough = PseudoHoughTransform.HoughTransform(horizEdgePoints, horizOptions);
            List<Point> horizHoughPeaks = PseudoHoughTransform.FindHoughPeaks(horizHough, horizOptions);
            Bitmap horizHoughPlainImage = PseudoHoughTransform.HoughTransformImage(horizHough);
            Bitmap horizHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(horizHough, horizHoughPeaks);
            List<Line> horizLines = PseudoHoughTransform.ExtractLines(horizEdgePoints, horizHoughPeaks, horizOptions);

            List<Point> vertEdgePoints = ExtractLines.ExtractEdgePoints(rotBw);
            int[,] vertHough = PseudoHoughTransform.HoughTransform(vertEdgePoints, vertOptions);
            List<Point> vertHoughPeaks = PseudoHoughTransform.FindHoughPeaks(vertHough, vertOptions);
            Bitmap vertHoughPlainImage = PseudoHoughTransform.HoughTransformImage(vertHough);
            Bitmap vertHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(vertHough, vertHoughPeaks);
            List<Line> vertLines = PseudoHoughTransform.ExtractLines(vertEdgePoints, vertHoughPeaks, vertOptions);

            Bitmap rawLinesImage = new Bitmap(bw);

            Graphics g = Graphics.FromImage(rawLinesImage);
            Pen p = new Pen(Brushes.Red, 2);
            foreach (var ln in horizLines) {
                g.DrawLine(p, ln.p1, ln.p2);
            }
            foreach (var ln in vertLines) {
                g.DrawLine(p,
                    new Point(bw.Width - 1 - ln.p1.Y, ln.p1.X),
                    new Point(bw.Width - 1 - ln.p2.Y, ln.p2.X));
            }
            g.Dispose();

            return ImageUtil.VerticalConcat(new List<Bitmap> {
                ImageUtil.HorizontalConcat(new List<Bitmap> { rawLinesImage, horizHoughImage, horizHoughPlainImage }),
                ImageUtil.RotateClockwise(vertHoughImage),
                ImageUtil.RotateClockwise(vertHoughPlainImage)
            });
        }
    }
}
