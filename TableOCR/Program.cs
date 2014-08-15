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
                Bitmap sourceImage = (Bitmap) Image.FromFile(fd.FileName);
                Util.Timed("to std format", () => {
                    sourceImage = ImageUtil.ToStdFormat(sourceImage);
                });

                Application.Run(new TableRecognitionDebugView(sourceImage));
            }
        }

        public static Option<Table> RecognizeTable(Bitmap sourceImage) {
            Bitmap sourceImageVert = ImageUtil.RotateCounterClockwise(sourceImage);
            return new Some<Table>(new TableRecognitionDebugObj(sourceImage).recognizedTable);
        }
    }
}
