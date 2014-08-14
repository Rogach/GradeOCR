using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibUtil;
using OCRUtil;
using TableOCR;

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
        List<Line> vertUnfilteredLines;
        List<Line> vertLines;

        Bitmap rawLinesImage;
        Bitmap filteredLinesImage;
        Bitmap normalizedLinesImage;
        Bitmap tableRecognitionImage;

        public Table recognizedTable;

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
                detectCyclicPatterns = false
            };

            vertOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 100,
                houghWindowWidth = 20,
                houghWindowHeight = 10,
                width = rotBw.Width,
                height = rotBw.Height,
                minLineLength = rotBw.Width / 10,
                detectCyclicPatterns = true,
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

            RecognitionParams vertNoFilterOptions = vertOptions;
            vertNoFilterOptions.detectCyclicPatterns = false;
            vertUnfilteredLines = LineFilter.ExtractLines(vertEdgePoints, vertRawLines, vertNoFilterOptions);
            vertLines = LineFilter.ExtractLines(vertEdgePoints, vertRawLines, vertOptions);

            rawLinesImage = DrawLines(bw, horizLines, vertUnfilteredLines, 2);
            filteredLinesImage = DrawLines(bw, horizLines, vertLines, 4);

            var lnorm = new LineNormalization(horizLines, vertLines, src);
            normalizedLinesImage = DrawLines(bw, lnorm.normHorizLines, lnorm.normVertLines, 2);
            var tb = new TableBuilder(lnorm);
            tableRecognitionImage = tb.DebugImage(bw);
            recognizedTable = tb.table;
        }

        private Bitmap DrawLines(Bitmap src, List<Line> horizLines, List<Line> vertLines, int lineWidth) {
            return DrawLines(src, horizLines.Select(ln => new LineF(ln)).ToList(), vertLines.Select(ln => new LineF(ln)).ToList(), lineWidth);
        }

        private Bitmap DrawLines(Bitmap src, List<LineF> horizLines, List<LineF> vertLines, int lineWidth) {
            Bitmap res = new Bitmap(src);

            Graphics g = Graphics.FromImage(res);
            Pen p = new Pen(Brushes.Red, lineWidth);
            foreach (var ln in horizLines) {
                g.DrawLine(p, ln.p1, ln.p2);
            }
            foreach (var ln in vertLines) {
                g.DrawLine(p,
                    new PointF(bw.Width - 1 - ln.p1.Y, ln.p1.X),
                    new PointF(bw.Width - 1 - ln.p2.Y, ln.p2.X));
            }
            g.Dispose();

            return res;
        }

        public Bitmap GetAggregateImage() {
            return ImageUtil.HorizontalConcat(new List<Bitmap> { 
                GetHoughDebugImage(),
                GetCyclicPatternsImage(),
                GetFilteredLinesImage()
            });
        }

        public Bitmap GetRawLinesImage() {
            return rawLinesImage;
        }

        public Bitmap GetFilteredLinesImage() {
            return filteredLinesImage;
        }

        public Bitmap GetNormalizedLinesImage() {
            return normalizedLinesImage;
        }

        public Bitmap GetTableRecognitionImage() {
            return tableRecognitionImage;
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
