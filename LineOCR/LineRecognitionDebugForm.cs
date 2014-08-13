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

namespace LineOCR {
    public partial class LineRecognitionDebugForm : Form {
        private PictureView sourceImagePV;
        private PictureView bwImagePV;
        private PictureView edgePointsPV;
        private PictureView houghPV;
        private PictureView cyclicPatternsPV;
        private PictureView filteredLinesPV;

        public LineRecognitionDebugForm(Bitmap sourceImage) {
            InitializeComponent();

            sourceImagePV = PictureView.InsertIntoPanel(sourceImagePanel);
            bwImagePV = PictureView.InsertIntoPanel(bwImagePanel);
            edgePointsPV = PictureView.InsertIntoPanel(edgePointsPanel);
            houghPV = PictureView.InsertIntoPanel(houghPanel);
            cyclicPatternsPV = PictureView.InsertIntoPanel(cyclicPatternsPanel);
            filteredLinesPV = PictureView.InsertIntoPanel(filteredLinesPanel);

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
            });
        }
    }
}
