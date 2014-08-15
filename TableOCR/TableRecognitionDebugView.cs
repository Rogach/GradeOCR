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
            RecognitionParams horizOptions = new RecognitionParams {
                maxAngleFactor = 0.03f,
                houghThreshold = 50
            };

            this.sourceImagePV.Image = sourceImage;

            var lrd = new TableRecognitionDebugObj(sourceImage);

            this.bwImagePV.Image = lrd.bw;
            this.edgePointsPV.Image = EdgeExtraction.DrawPoints(lrd.bw, lrd.horizEdgePoints);
            this.rotBwImagePV.Image = lrd.rotBw;
            this.rotEdgePointsPV.Image = EdgeExtraction.DrawPoints(lrd.rotBw, lrd.vertEdgePoints);

            this.houghPV.Image = lrd.GetHoughDebugImage();
            this.cyclicPatternsPV.Image = lrd.GetCyclicPatternsImage();
            this.filteredLinesPV.Image = lrd.GetFilteredLinesImage();
            this.normalizedLinesPV.Image = lrd.GetNormalizedLinesImage();
            this.tableRecognitionPV.Image = lrd.GetTableRecognitionImage();

            Bitmap recognizedTableImage = new Bitmap(lrd.bw);
            Graphics g = Graphics.FromImage(recognizedTableImage);
            lrd.recognizedTable.DrawTable(g, new Pen(Color.Red, 2));
            g.Dispose();
            this.recognizedTablePV.Image = recognizedTableImage;

            GradeDigestSet digestSet = GradeDigestSet.ReadDefault();
            this.recognizedTablePV.AddDoubleClickListener((pt, e) => {
                lrd.recognizedTable.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                    var gradeRecognition = new GradeRecognitionDebugView(lrd.recognizedTable.GetCellImage(lrd.bw, cell.X, cell.Y), "<gen>", digestSet);
                    gradeRecognition.ShowDialog();
                });
            });
        }
    }
}
