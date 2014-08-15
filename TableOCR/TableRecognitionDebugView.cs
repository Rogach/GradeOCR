using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using LibUtil;
using GradeOCR;

namespace TableOCR {
    public partial class TableRecognitionDebugView : Form {
        private PictureView sourceImagePV;
        private PictureView bwImagePV;
        private PictureView edgePointsPV;
        private PictureView rotBwImagePV;
        private PictureView rotEdgePointsPV;
        private PictureView houghPV;
        private PictureView cyclicPatternsPV;
        private PictureView filteredLinesPV;
        private PictureView normalizedLinesPV;
        private PictureView tableRecognitionPV;
        private PictureView recognizedTablePV;

        public TableRecognitionDebugView(Bitmap sourceImage) {
            InitializeComponent();

            sourceImagePV = PictureView.InsertIntoPanel(sourceImagePanel);
            bwImagePV = PictureView.InsertIntoPanel(bwImagePanel);
            edgePointsPV = PictureView.InsertIntoPanel(edgePointsPanel);
            houghPV = PictureView.InsertIntoPanel(houghPanel);
            cyclicPatternsPV = PictureView.InsertIntoPanel(cyclicPatternsPanel);
            filteredLinesPV = PictureView.InsertIntoPanel(filteredLinesPanel);
            normalizedLinesPV = PictureView.InsertIntoPanel(normalizedLinesPanel);
            tableRecognitionPV = PictureView.InsertIntoPanel(tableRecognitionPanel);
            recognizedTablePV = PictureView.InsertIntoPanel(recognizedTablePanel);
            rotBwImagePV = PictureView.InsertIntoPanel(rotBwImagePanel);
            rotEdgePointsPV = PictureView.InsertIntoPanel(rotEdgePointsPanel);

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    RunOCR(sourceImage);
                });
            });
        }

        private void RunOCR(Bitmap sourceImage) {
            this.sourceImagePV.Image = sourceImage;

            Bitmap bw = ImageUtil.ToBlackAndWhite(sourceImage);
            this.bwImagePV.Image = bw;
            Bitmap rotBw = ImageUtil.RotateCounterClockwise(bw);
            this.rotBwImagePV.Image = rotBw;

            var horizOptions = RecognitionOptions.HorizontalOptions();
            horizOptions.imageWidth = bw.Width;
            horizOptions.imageHeight = bw.Height;

            var vertOptions = RecognitionOptions.VerticalOptions();
            vertOptions.imageWidth = rotBw.Width;
            vertOptions.imageHeight = rotBw.Height;

            List<Point> horizEdgePoints = EdgePointExtraction.ExtractEdgePoints(bw);
            this.edgePointsPV.Image = EdgePointExtraction.DrawPoints(bw, horizEdgePoints);
            int[,] horizHough = PseudoHoughTransform.HoughTransform(horizEdgePoints, horizOptions);
            List<Point> horizHoughPeaks = PseudoHoughTransform.FindHoughPeaks(horizHough, horizOptions);
            Bitmap horizHoughPlainImage = PseudoHoughTransform.HoughTransformImage(horizHough);
            Bitmap horizHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(horizHough, horizHoughPeaks);
            List<RawLine> horizRawLines = PseudoHoughTransform.ExtractRawLines(horizHoughPeaks, horizOptions);
            List<Line> horizLines = LineFilter.FilterLines(horizEdgePoints, horizRawLines, horizOptions);

            List<Point> vertEdgePoints = EdgePointExtraction.ExtractEdgePoints(rotBw);
            this.rotEdgePointsPV.Image = EdgePointExtraction.DrawPoints(rotBw, vertEdgePoints);
            int[,] vertHough = PseudoHoughTransform.HoughTransform(vertEdgePoints, vertOptions);
            List<Point> vertHoughPeaks = PseudoHoughTransform.FindHoughPeaks(vertHough, vertOptions);
            Bitmap vertHoughPlainImage = PseudoHoughTransform.HoughTransformImage(vertHough);
            Bitmap vertHoughImage = PseudoHoughTransform.HoughTransformImageWithPeaks(vertHough, vertHoughPeaks);
            List<RawLine> vertRawLines = PseudoHoughTransform.ExtractRawLines(vertHoughPeaks, vertOptions);
            this.cyclicPatternsPV.Image = CyclicPatternDetector.CyclicPatternsInLines(vertEdgePoints, vertRawLines, vertOptions);
            
            RecognitionOptions vertNoFilterOptions = vertOptions;
            vertNoFilterOptions.detectCyclicPatterns = false;
            List<Line> vertUnfilteredLines = LineFilter.FilterLines(vertEdgePoints, vertRawLines, vertNoFilterOptions);
            List<Line> vertLines = LineFilter.FilterLines(vertEdgePoints, vertRawLines, vertOptions);

            Bitmap rawLinesImage = DrawLines(bw, horizLines, vertUnfilteredLines, 2);
            Bitmap filteredLinesImage = DrawLines(bw, horizLines, vertLines, 4);
            this.filteredLinesPV.Image = filteredLinesImage;

            this.houghPV.Image = ImageUtil.VerticalConcat(new List<Bitmap> {
                ImageUtil.HorizontalConcat(new List<Bitmap> { rawLinesImage, horizHoughImage, horizHoughPlainImage }),
                ImageUtil.RotateClockwise(vertHoughImage),
                ImageUtil.RotateClockwise(vertHoughPlainImage)
            });

            var lnorm = new LineNormalization(horizLines, vertLines, sourceImage);
            Bitmap normalizedLinesImage = DrawLines(bw, lnorm.normHorizLines, lnorm.normVertLines, 2);
            this.normalizedLinesPV.Image = normalizedLinesImage;
            var tb = TableBuilder.NewBuilder(lnorm);
            Bitmap tableRecognitionImage = tb.DebugImage(bw);
            this.tableRecognitionPV.Image = tableRecognitionImage;
            Option<Table> recognizedTable = tb.table;
            
            recognizedTable.ForEach(table => {
                Bitmap recognizedTableImage = new Bitmap(bw);
                Graphics g = Graphics.FromImage(recognizedTableImage);
                table.DrawTable(g, new Pen(Color.Red, 2));
                g.Dispose();
                this.recognizedTablePV.Image = recognizedTableImage;
            });
            if (recognizedTable.IsEmpty()) {
                Console.WriteLine("no table was recognized");
            }

            GradeDigestSet digestSet = GradeDigestSet.ReadDefault();
            this.recognizedTablePV.AddDoubleClickListener((pt, e) => {
                recognizedTable.ForEach(table => {
                    table.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                        var gradeRecognition = new GradeRecognitionDebugView(table.GetCellImage(bw, cell.X, cell.Y), "<gen>", digestSet);
                        gradeRecognition.ShowDialog();
                    });
                });
            });
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
                    new PointF(src.Width - 1 - ln.p1.Y, ln.p1.X),
                    new PointF(src.Width - 1 - ln.p2.Y, ln.p2.X));
            }
            g.Dispose();

            return res;
        }
    }
}
