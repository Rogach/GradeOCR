using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using LibUtil;

namespace LineOCR {
    public partial class LineRecognitionDebugForm : Form {
        private PictureView sourceImagePV;
        private PictureView bwImagePV;
        private PictureView segmentsPV;
        private PictureView linesPV;

        public LineRecognitionDebugForm(Bitmap sourceImage) {
            InitializeComponent();

            sourceImagePV = PictureView.InsertIntoPanel(sourceImagePanel);
            bwImagePV = PictureView.InsertIntoPanel(bwImagePanel);
            segmentsPV = PictureView.InsertIntoPanel(segmentsPanel);
            linesPV = PictureView.InsertIntoPanel(linesPanel);

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    RunOCR(sourceImage);
                });
            });
        }

        private void RunOCR(Bitmap sourceImage) {
            this.sourceImagePV.Image = sourceImage;

            Bitmap bwImage = Util.Timed("to bw image", () => ImageUtil.ToBlackAndWhite(sourceImage));
            this.bwImagePV.Image = bwImage;

            List<Line> lineSegments = Util.Timed("line segment extraction", () => ExtractLines.ExtractLineSegments(bwImage));
            Console.WriteLine("found {0} line segments", lineSegments.Count);

            Bitmap bwPairsImage = ExtractLines.DrawLineSegments(bwImage, lineSegments);
            this.segmentsPV.Image = bwPairsImage;
        }
    }
}
