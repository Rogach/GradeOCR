using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using System.Drawing;
using LibUtil;

namespace LineOCR {
    public struct RecognitionParams {
        public float maxAngleFactor;
        public int houghThreshold;
        public int houghWindowWidth;
        public int houghWindowHeight;
        public int width;
        public int height;
        public int minLineLength;
        public bool detectCyclicPatterns;
        public int cyclicPatternsMinWidth;
        public int cyclicPatternsMaxWidth;
    }

    public static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new LineRecognitionDebugForm(
                    ImageUtil.LoadImage(@"e:\Pronko\prj\Grader\ocr-data\register-test-input\scan0030.jpg")));
        }
    }
}
