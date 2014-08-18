﻿using System;
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
        private PictureView roughResultImagePV;
        private PictureView tunedResultImagePV;

        public FinderCircleDebugView(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius) {
            InitializeComponent();

            this.inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);
            this.noiseImagePV = PictureView.InsertIntoPanel(noiseImagePanel);
            this.houghImagePV = PictureView.InsertIntoPanel(houghImagePanel);
            this.houghPeakImagePV = PictureView.InsertIntoPanel(houghPeaksImagePanel);
            this.roughResultImagePV = PictureView.InsertIntoPanel(roughResultImagePanel);
            this.tunedResultImagePV = PictureView.InsertIntoPanel(tunedResultImagePanel);

            this.Shown += new EventHandler(delegate {
                Util.NewThread(() => {
                    RunOCR(sourceImage, minPatternRadius, maxPatternRadius);
                });
            });
        }

        private void RunOCR(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius) {
            Bitmap grayImage = ImageUtil.ToGrayscale(sourceImage);
            this.inputImagePV.Image = grayImage;

            Bitmap noiseImage = Program.GetTestNoiseFilter().Apply(sourceImage);
            this.noiseImagePV.Image = noiseImage;

            int scaleFactor = CircleHoughTransform.GetScaleFactor(minPatternRadius);
            Console.WriteLine("scaleFactor = " + scaleFactor);
            Bitmap downscaledImage = ImageScaling.ScaleDown(noiseImage, scaleFactor);

            int[,,] hough = Util.Timed("hough transform", () => 
                CircleHoughTransform.HoughTransform(downscaledImage, minPatternRadius / scaleFactor, maxPatternRadius / scaleFactor));
            Bitmap houghTransformImage = CircleHoughTransform.HoughTransformImage(hough);
            this.houghImagePV.Image = houghTransformImage;
            List<Point3> peaks = CircleHoughTransform.LocatePeaks(hough, 2, minPatternRadius / scaleFactor);
            List<Point3> descaledPeaks = peaks.ConvertAll(p => new Point3(p.X * scaleFactor, p.Y * scaleFactor, p.Z * scaleFactor + minPatternRadius));
            foreach (var p in descaledPeaks) {
                Console.WriteLine("Raw peak at {0}x{1}x{2}", p.X, p.Y, p.Z);
            }
            List<Point3> tunedPeaks = Util.Timed("tune peaks", () =>
                descaledPeaks.ConvertAll(peak => CircleHoughTransform.TunePeak(noiseImage, minPatternRadius, maxPatternRadius, peak)));
            foreach (var p in tunedPeaks) {
                Console.WriteLine("Tuned peak at {0}x{1}x{2}", p.X, p.Y, p.Z);
            }
            this.houghPeakImagePV.Image = CircleHoughTransform.DrawPeaks(houghTransformImage, peaks);
            this.roughResultImagePV.Image = CircleHoughTransform.DrawPeaks(noiseImage, descaledPeaks);
            this.tunedResultImagePV.Image = CircleHoughTransform.DrawPeaks(noiseImage, tunedPeaks);
        }

    }
}
