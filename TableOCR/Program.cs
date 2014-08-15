using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using LibUtil;
using System.Threading;
using OCRUtil;

namespace TableOCR {
    public class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Выберите изображение ведомости";
            if (fd.ShowDialog() == DialogResult.OK) {
                Bitmap sourceImage = ImageUtil.LoadImage(fd.FileName);

                Application.Run(new TableRecognitionDebugView(sourceImage));
            }
        }

        /*
         * Convenience function to recognize table in an image.
         */
        public static Option<Table> RecognizeTable(Bitmap sourceImage) {
            Bitmap bw = ImageUtil.ToBlackAndWhite(sourceImage);
            Bitmap rotBw = ImageUtil.RotateCounterClockwise(bw);

            var horizOptions = RecognitionOptions.HorizontalOptions();
            horizOptions.imageWidth = bw.Width;
            horizOptions.imageHeight = bw.Height;

            var vertOptions = RecognitionOptions.VerticalOptions();
            vertOptions.imageWidth = rotBw.Width;
            vertOptions.imageHeight = rotBw.Height;

            List<Line> horizLines = RecognizeLines(bw, horizOptions);
            List<Line> vertLines = RecognizeLines(rotBw, vertOptions);

            if (horizLines.Count == 0 || vertLines.Count == 0) {
                return new None<Table>();
            } else {
                var lnorm = new LineNormalization(horizLines, vertLines, sourceImage);
                return TableBuilder.NewBuilder(lnorm).table;
            }
        }

        private static List<Line> RecognizeLines(Bitmap bw, RecognitionOptions options) {
            List<Point> edgePoints = EdgePointExtraction.ExtractEdgePoints(bw);
            List<RawLine> rawLines = PseudoHoughTransform.RecognizeLines(edgePoints, options);
            return LineFilter.FilterLines(edgePoints, rawLines, options);
        }
    }
}
