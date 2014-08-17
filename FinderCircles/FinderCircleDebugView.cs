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

namespace FinderCircles {
    public partial class FinderCircleDebugView : Form {
        private PictureView inputImagePV;
        private PictureView noiseImagePV;
        private PictureView houghImagePV;
        private PictureView houghPeakImagePV;

        public FinderCircleDebugView(Bitmap sourceImage) {
            InitializeComponent();

            this.inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);
            this.noiseImagePV = PictureView.InsertIntoPanel(noiseImagePanel);
            this.houghImagePV = PictureView.InsertIntoPanel(houghImagePanel);
            this.houghPeakImagePV = PictureView.InsertIntoPanel(houghPeaksImagePanel);

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    RunOCR(sourceImage);
                });
            });
        }

        private void RunOCR(Bitmap sourceImage) {
            int minPatternRadius = 20;
            int maxPatternRadius = 30;

            Bitmap grayImage = ImageUtil.ToGrayscale(sourceImage);
            this.inputImagePV.Image = grayImage;

            NoiseFilter filter = new FilterSeq(
                new RandomBlots(0.2),
                new RandomNoise(0.2),
                new RandomStripes(0.5, 20)
            );

            Bitmap noiseImage = filter.Apply(sourceImage);
            this.noiseImagePV.Image = noiseImage;

            int[,,] hough = Util.Timed("hough transform", () => 
                CircleHoughTransform.HoughTransform(noiseImage, minPatternRadius, maxPatternRadius ));
            Bitmap houghTransformImage = CircleHoughTransform.HoughTransformImage(hough);
            this.houghImagePV.Image = houghTransformImage;
            List<Point3> peaks = CircleHoughTransform.LocatePeaks(hough);
            foreach (var p in peaks) {
                Console.WriteLine("Peak at {0}x{1}x{2}", p.X, p.Y, p.Z + minPatternRadius);
            }
            this.houghPeakImagePV.Image = CircleHoughTransform.DrawPeaks(houghTransformImage, peaks);
        }
    }
}
