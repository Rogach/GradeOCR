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
using System.Drawing.Imaging;

namespace ARCode {
    public partial class FinderCircleDebugView : Form {
        private PictureView inputImagePV;
        private PictureView noiseImagePV;
        private PictureView houghImagePV;
        private PictureView houghPeakImagePV;
        private PictureView peakResultImagePV;
        private PictureView dataMatrixLocationPV;
        private PictureView rotatedDataMatrixPV;
        private PictureView recognizedDataMatrixPV;

        public FinderCircleDebugView(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius, uint inputValue) {
            InitializeComponent();

            this.inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);
            this.noiseImagePV = PictureView.InsertIntoPanel(noiseImagePanel);
            this.houghImagePV = PictureView.InsertIntoPanel(houghImagePanel);
            this.houghPeakImagePV = PictureView.InsertIntoPanel(houghPeaksImagePanel);
            this.peakResultImagePV = PictureView.InsertIntoPanel(peakResultImagePanel);
            this.dataMatrixLocationPV = PictureView.InsertIntoPanel(dataMatrixLocationPanel);
            this.rotatedDataMatrixPV = PictureView.InsertIntoPanel(rotatedDataMatrixPanel);
            this.recognizedDataMatrixPV = PictureView.InsertIntoPanel(recognizedDataMatrixPanel);

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    Util.Timed("full AR-code OCR", () => {
                        RunOCR(sourceImage, minPatternRadius, maxPatternRadius, inputValue);
                    });
                });
            });
        }

        private void RunOCR(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius, uint inputValue) {
            this.inputDataLabel.Text = inputValue.ToString();

            Bitmap grayImage = ImageUtil.ToGrayscale(sourceImage);
            this.inputImagePV.Image = grayImage;

            Bitmap noiseImage = sourceImage;
            this.noiseImagePV.Image = noiseImage;

            int scaleFactor = FinderCircleHoughTransform.GetScaleFactor(minPatternRadius);
            Console.WriteLine("scaleFactor = " + scaleFactor);
            Bitmap downscaledImage = ImageScaling.ScaleDown(noiseImage, scaleFactor);

            int[,,] hough = Util.Timed("hough transform", () => 
                FinderCircleHoughTransform.HoughTransform(downscaledImage, minPatternRadius / scaleFactor, maxPatternRadius / scaleFactor));
            Bitmap houghTransformImage = FinderCircleHoughTransform.HoughTransformImage(hough);
            this.houghImagePV.Image = houghTransformImage;
            List<Point3> peaks = FinderCircleHoughTransform.LocatePeaks(hough, 2, minPatternRadius / scaleFactor);
            List<Point3> descaledPeaks = peaks.ConvertAll(p => new Point3(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor + minPatternRadius));
            foreach (var p in descaledPeaks) {
                Console.WriteLine("Raw peak at {0}x{1}x{2}", p.X, p.Y, p.Z);
            }
            List<Point3> tunedPeaks = Util.Timed("tune peaks", () =>
                descaledPeaks.ConvertAll(peak => FinderCircleHoughTransform.TunePeak(noiseImage, minPatternRadius, maxPatternRadius, peak)));
            foreach (var p in tunedPeaks) {
                Console.WriteLine("Tuned peak at {0}x{1}x{2}", p.X, p.Y, p.Z);
            }
            Bitmap houghPeaksImage = new Bitmap(houghTransformImage);
            DrawPeaks(houghPeaksImage, peaks, Color.Red);
            this.houghPeakImagePV.Image = houghPeaksImage;

            Bitmap resultPeaksImage = new Bitmap(noiseImage);
            DrawPeaks(resultPeaksImage, descaledPeaks, Color.Red);
            DrawPeaks(resultPeaksImage, tunedPeaks, Color.Green);
            this.peakResultImagePV.Image = resultPeaksImage;

            FinderPatternPair fpp = new FinderPatternPair();
            fpp.p1 = new Point(tunedPeaks[0].X, tunedPeaks[0].Y);
            fpp.size1 = tunedPeaks[0].Z;
            fpp.p2 = new Point(tunedPeaks[1].X, tunedPeaks[1].Y);
            fpp.size2 = tunedPeaks[1].Z;

            DataMatrixExtraction dme = new DataMatrixExtraction(noiseImage, fpp);
            dataMatrixLocationPV.Image = dme.PositioningDebugImage();
            rotatedDataMatrixPV.Image = dme.rotatedMatrix;
            recognizedDataMatrixPV.Image = dme.RecognitionDebugImage();
            Option<uint> extractedCode = DataMarshaller.UnMarshallInt(dme.extractedData);
            outputDataLabel.Text = extractedCode.Map(c => c.ToString()).GetOrElse("none");
        }


        public static void DrawPeaks(Bitmap src, List<Point3> peaks, Color c) {
            unsafe {
                BitmapData bd = src.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                foreach (var pt in peaks) {
                    byte* p = ptr + 4 * (pt.Y * src.Width + pt.X);
                    *p = c.B;
                    *(p + 1) = c.G;
                    *(p + 2) = c.R;
                    *(p + 3) = c.A;
                }

                src.UnlockBits(bd);
            }
        }
    }
}
