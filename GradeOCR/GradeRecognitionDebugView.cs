﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using LibUtil;
using OCRUtil;

namespace GradeOCR {
    public partial class GradeRecognitionDebugView : Form {
        public PictureView inputImagePV;
        public PictureView removeBorderPV;
        public PictureView noiseRemovalPV;
        public PictureView croppedPV;
        public PictureView digestPV;

        public GradeRecognitionDebugView(Bitmap inputImage, string imageName) {
            InitializeComponent();

            this.Text = imageName;

            inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);
            removeBorderPV = PictureView.InsertIntoPanel(removeBorderPanel);
            noiseRemovalPV = PictureView.InsertIntoPanel(noiseRemovalPanel);
            croppedPV = PictureView.InsertIntoPanel(croppedPanel);
            digestPV = PictureView.InsertIntoPanel(digestPanel);

            this.Shown += new EventHandler(delegate {
                Thread worker = new Thread(new ThreadStart(delegate {
                    Util.Timed("Grade OCR debug run", () => {
                        RunOCR(inputImage);
                    });
                }));
                worker.IsBackground = true;
                worker.Start();
            });
        }

        public void RunOCR(Bitmap inputImage) {
            inputImagePV.Image = inputImage;
            Bitmap removeBorderImage = BorderRemoval.RemoveBorder(inputImage);
            removeBorderPV.Image = removeBorderImage;
            Bitmap removeNoiseImage = NoiseCleaner.RemoveNoise(removeBorderImage);
            noiseRemovalPV.Image = removeNoiseImage;
            Bitmap croppedImage = WhitespaceCropper.CropWhitespace(removeNoiseImage);
            croppedPV.Image = croppedImage;
            Bitmap digestImage = DigestExtractor.ExtractDigestImage(croppedImage);
            digestPV.Image = digestImage;
        }
    }
}
