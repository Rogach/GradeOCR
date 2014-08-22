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

        public GradeRecognitionDebugView(Bitmap inputImage, string imageName) {
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
            Bitmap removeNoiseImage = NoiseCleaner.RemoveNoiseWithBorder(removeBorderImage);
            noiseRemovalPV.Image = removeNoiseImage;
            Bitmap croppedImage = WhitespaceCropper.CropWhitespace(removeNoiseImage);
            croppedPV.Image = croppedImage;
            if (EmptyImageDetector.IsImageEmpty(croppedImage)) {
                recognizedGradeLabel.Text = "0";
                recognitionConfidenceLabel.Text = "empty";
            } else {
                Bitmap digestImage = DigestExtractor.ExtractDigestImage(croppedImage);
                digestPV.Image = digestImage;

                GradeDigest digest = GradeDigest.FromImage(digestImage);
                RecognitionResult recognitionResult = Util.Timed("digest matching", () => GradeDigestSet.staticInstance.FindBestMatch(digest));
                Console.WriteLine("best-match digest index: {0}", recognitionResult.MatchIndex);
                Bitmap bestMatchImage = recognitionResult.Digest.DigestImage();
                bestMatchPV.Image = bestMatchImage;
                recognizedGradeLabel.Text = recognitionResult.Digest.grade.ToString();
                if (MatchConfidence.Sure(recognitionResult.ConfidenceScore)) {
                    recognitionConfidenceLabel.Text = String.Format("sure, {0} pt", recognitionResult.ConfidenceScore);
                } else {
                    recognitionConfidenceLabel.Text = String.Format("unsure, {0} pt", recognitionResult.ConfidenceScore);
                }

                Bitmap differenceImage = DigestDifference.GenerateDifferenceImage(digest, recognitionResult.Digest);
                differencePV.Image = differenceImage;
            }
        }
    }
}
