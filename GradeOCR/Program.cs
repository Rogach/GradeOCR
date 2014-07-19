using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using LibUtil;
using System.Threading;

namespace GradeOCR {
    class Program {
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Bitmap sourceImage = (Bitmap) Image.FromFile("E:/Pronko/prj/Grader/ocr-data/scan1.jpg");
            Util.Timed("to std format", () => {
                sourceImage = ImageUtil.ToStdFormat(sourceImage);
            });

            Application.Run(new OcrResultForm(sourceImage));
        }
    }
}
