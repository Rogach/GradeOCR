using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using OCRUtil;
using System.IO;
using LibUtil;
using NeuralNetwork;
using System.Reflection;

namespace GradeOCR {
    public class Program {

        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new MassGradeView(new Size(50, 50), b => {
            //    return NormalizeImage(b);
            //}));

            string gradeFile = @"E:/Pronko/prj/Grader/ocr-data/test-data/grade-4/g00381.png";
            Application.Run(
                new GradeRecognitionDebugView(
                    ImageUtil.LoadImage(gradeFile),
                    gradeFile));
        }

        public static Bitmap NormalizeImage(Bitmap img) {
            return
                DigestExtractor.ExtractDigestImage(
                    WhitespaceCropper.CropWhitespace(
                        NoiseCleaner.RemoveNoiseWithBorder(
                            BorderRemoval.RemoveBorder(img))));
        }

        public static Option<GradeDigest> GetGradeDigest(Bitmap img) {
            Bitmap croppedImage =
                WhitespaceCropper.CropWhitespace(
                    NoiseCleaner.RemoveNoiseWithBorder(
                        BorderRemoval.RemoveBorder(img)));
            if (EmptyImageDetector.IsImageEmpty(croppedImage)) {
                return new None<GradeDigest>();
            } else {
                return new Some<GradeDigest>(
                    GradeDigest.FromImage(
                        DigestExtractor.ExtractDigestImage(croppedImage)));
            }
        }

        public static Option<RecognitionResult> RecognizeGrade(Bitmap img) {
            return GetGradeDigest(img).Map(gd => RecognizeGrade(gd));
        }

        public static readonly MultiLayerNetwork gradeRecognitionNetwork =
            new MultiLayerNetwork(Assembly.GetExecutingAssembly().GetManifestResourceStream("GradeOCR.grade-recognition.nn"));

        public static readonly double recognitionConfidenceThreshold = 0.0001;

        public static RecognitionResult RecognizeGrade(GradeDigest digest) {
            List<int> gradeCodes = new List<int> { 2, 3, 4, 5 };
            double[] output = new double[4];
            gradeRecognitionNetwork.NetOUT(NNUtils.ToNetworkInput(GradeDigest.UnpackBits(digest.data)), out output);
            return new RecognitionResult(
                grade: gradeCodes[NNUtils.Answer(output)], 
                confident: NNUtils.AnswerConfidence(output) < recognitionConfidenceThreshold
            );
        }

    }
}
