using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        public PictureView bestMatchPV;
        public PictureView differencePV;

        private GradeDigestSet digestSet;

        public GradeRecognitionDebugView(Bitmap inputImage, string imageName, GradeDigestSet digestSet) {
            this.digestSet = digestSet;
            InitializeComponent();

            this.Text = imageName;

            inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);
            removeBorderPV = PictureView.InsertIntoPanel(removeBorderPanel);
            noiseRemovalPV = PictureView.InsertIntoPanel(noiseRemovalPanel);
            croppedPV = PictureView.InsertIntoPanel(croppedPanel);
            digestPV = PictureView.InsertIntoPanel(digestPanel);
            bestMatchPV = PictureView.InsertIntoPanel(bestMatchPanel);
            differencePV = PictureView.InsertIntoPanel(differencePanel);

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

            GradeDigest digest = GradeDigest.FromImage(digestImage);
            RecognitionResult recognitionResult = Util.Timed("digest matching", () => digestSet.FindBestMatch(digest));
            Bitmap bestMatchImage = recognitionResult.Digest.DigestImage();
            bestMatchPV.Image = bestMatchImage;
            recognizedGradeLabel.Text = recognitionResult.Digest.grade.ToString();
            recognitionConfidenceLabel.Text = String.Format("{0}%", (int) Math.Floor(recognitionResult.Confidence * 100));

            Bitmap differenceImage = DigestDifference.GenerateDifferenceImage(digest, recognitionResult.Digest);
            differencePV.Image = differenceImage;
        }
    }
}
