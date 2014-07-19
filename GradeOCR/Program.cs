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

            Bitmap sourceImage = (Bitmap) Image.FromFile("E:/Pronko/prj/Grader/ocr-data/register-new/scan1.jpg");
            Util.Timed("to std format", () => {
                sourceImage = ImageUtil.ToStdFormat(sourceImage);
            });

            Application.Run(new TableRecognitionDebugView(sourceImage));
        }

        public static Table RecognizeTable(Bitmap sourceImage) {
            Bitmap sourceImageVert = ImageUtil.Rotate(sourceImage);
            BWImage bw = new BWImage(sourceImage);
            BWImage bwVert = new BWImage(sourceImageVert);
            List<Line> hLines = 
                LineRecognition.RunRecognition(bw, (int) (bw.Width * LineRecognition.minHorizontalLineRatio));
            List<Line> vLines = 
                LineRecognition.RunRecognition(bwVert, (int) (bw.Height * LineRecognition.minVerticalLineRatio))
                .ConvertAll(ln => {
                    return new Line(
                        new Point(sourceImage.Width - 1 - ln.p1.Y, ln.p1.X), 
                        new Point(sourceImage.Width - 1 - ln.p2.Y, ln.p2.X));
                });
            return new Table(hLines, vLines);
        }
    }
}
