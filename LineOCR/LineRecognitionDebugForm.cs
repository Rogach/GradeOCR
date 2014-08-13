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

namespace LineOCR {
    public partial class LineRecognitionDebugForm : Form {
        private PictureView sourceImagePV;
        private PictureView bwImagePV;
        private PictureView edgePointsPV;
        private PictureView houghPV;
        private PictureView cyclicPatternsPV;
        private PictureView filteredLinesPV;
        private PictureView normalizedLinesPV;
        private PictureView tableRecognitionPV;
        private PictureView recognizedTablePV;

        public LineRecognitionDebugForm(Bitmap sourceImage) {
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

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    RunOCR(sourceImage);
                });
            });
        }

        private void RunOCR(Bitmap sourceImage) {
            RecognitionParams horizOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 50
            };

            this.sourceImagePV.Image = sourceImage;

            Bitmap bwImage = Util.Timed("to bw image", () => ImageUtil.ToBlackAndWhite(sourceImage));
            this.bwImagePV.Image = bwImage;

            List<Point> edgePoints = EdgeExtraction.ExtractEdgePoints(bwImage);
            Console.WriteLine("extracted {0} edge points", edgePoints.Count);
            Bitmap edgePointsImage = EdgeExtraction.DrawPoints(bwImage, edgePoints);
            
            this.edgePointsPV.Image = edgePointsImage;
            this.edgePointsPV.AddDoubleClickListener((p, e) => {
                Console.WriteLine(p);
            });

            Util.Timed("hough image", () => {
                var lrd = new LineRecognitionDebugObj(sourceImage);
                this.houghPV.Image = lrd.GetHoughDebugImage();
                this.cyclicPatternsPV.Image = lrd.GetCyclicPatternsImage();
                this.filteredLinesPV.Image = lrd.GetFilteredLinesImage();
                this.normalizedLinesPV.Image = lrd.GetNormalizedLinesImage();
                this.tableRecognitionPV.Image = lrd.GetTableRecognitionImage();

                Bitmap recognizedTableImage = new Bitmap(bwImage);
                Graphics g = Graphics.FromImage(recognizedTableImage);
                lrd.recognizedTable.DrawTable(g, new Pen(Color.Red, 4));
                g.Dispose();
                this.recognizedTablePV.Image = recognizedTableImage;

                GradeDigestSet digestSet = GradeDigestSet.ReadDefault();
                this.recognizedTablePV.AddDoubleClickListener((pt, e) => {
                    lrd.recognizedTable.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                        var gradeRecognition = new GradeRecognitionDebugView(lrd.recognizedTable.GetCellImage(bwImage, cell.X, cell.Y), "<gen>", digestSet);
                        gradeRecognition.ShowDialog();
                    });
                });
            });
        }
    }
}
