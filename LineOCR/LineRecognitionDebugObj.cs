using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibUtil;
using OCRUtil;

namespace LineOCR {
    public class LineRecognitionDebugObj {
        private Bitmap src;

        public Bitmap bw;
        public Bitmap rotBw;

        public RecognitionParams horizOptions;
        public RecognitionParams vertOptions;

        List<Point> horizEdgePoints;
        int[,] horizHough;
        List<Point> horizHoughPeaks;
        Bitmap horizHoughPlainImage;
        Bitmap horizHoughImage;
        List<RawLine> horizRawLines;
        List<Line> horizLines;

        List<Point> vertEdgePoints;
        int[,] vertHough;
        List<Point> vertHoughPeaks;
        Bitmap vertHoughPlainImage;
        Bitmap vertHoughImage;
        List<RawLine> vertRawLines;
        List<Line> vertLines;

        Bitmap rawLinesImage;

        public LineRecognitionDebugObj(Bitmap src) {
            this.src = src;

            bw = ImageUtil.ToBlackAndWhite(src);
            rotBw = ImageUtil.RotateCounterClockwise(bw);

            horizOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 50,
                houghWindowWidth = 20,
                houghWindowHeight = 10,
                width = bw.Width,
                height = bw.Height,
                minLineLength = bw.Width / 2,
                cyclicPatternsMinWidth = 10,
                cyclicPatternsMaxWidth = 100
            };

            vertOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 100,
                houghWindowWidth = 20,
                houghWindowHeight = 10,
                width = rotBw.Width,
                height = rotBw.Height,
                minLineLength = rotBw.Width / 10,
                cyclicPatternsMinWidth = 10,
                cyclicPatternsMaxWidth = 100
            };

            horizEdgePoints = EdgeExtraction.ExtractEdgePoints(bw);
            horizHough = PseudoHoughTransform.HoughTransform(horizEdgePoints, horizOptions);
            horizHoughPeaks = PseudoHoughTransform.FindHoughPeaks(horizHough, horizOptions);
            horizHoughPlainImage = PseudoHoughTransform.HoughTransformImage(horizHough);
            horizHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(horizHough, horizHoughPeaks);
            horizRawLines = PseudoHoughTransform.ExtractRawLines(horizHoughPeaks, horizOptions);
            horizLines = LineFilter.ExtractLines(horizEdgePoints, horizRawLines, horizOptions);

            vertEdgePoints = EdgeExtraction.ExtractEdgePoints(rotBw);
            vertHough = PseudoHoughTransform.HoughTransform(vertEdgePoints, vertOptions);
            vertHoughPeaks = PseudoHoughTransform.FindHoughPeaks(vertHough, vertOptions);
            vertHoughPlainImage = PseudoHoughTransform.HoughTransformImage(vertHough);
            vertHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(vertHough, vertHoughPeaks);
            vertRawLines = PseudoHoughTransform.ExtractRawLines(vertHoughPeaks, vertOptions);
            vertLines = LineFilter.ExtractLines(vertEdgePoints, vertRawLines, vertOptions);

            rawLinesImage = new Bitmap(bw);

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
        }

        public Bitmap GetAggregateImage() {
            return ImageUtil.HorizontalConcat(new List<Bitmap> { 
                GetHoughDebugImage(), GetCyclicPatternsImage()
            });
        }

        public Bitmap GetHoughDebugImage() {
            return ImageUtil.VerticalConcat(new List<Bitmap> {
                ImageUtil.HorizontalConcat(new List<Bitmap> { rawLinesImage, horizHoughImage, horizHoughPlainImage }),
                ImageUtil.RotateClockwise(vertHoughImage),
                ImageUtil.RotateClockwise(vertHoughPlainImage)
            });
        }

        public Bitmap GetCyclicPatternsImage() {
            return LineFilter.CyclicPatternsInLines(vertEdgePoints, vertRawLines, vertOptions);
        }
    }
}
