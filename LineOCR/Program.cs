using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;

namespace LineOCR {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new LineRecognitionDebugForm(
                ImageUtil.LoadImage(@"e:\Pronko\prj\Grader\ocr-data\register-bad\reg002.jpg")));
        }
    }
}
