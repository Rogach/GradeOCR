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
            
            var form = new OcrResultForm();

            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate {
                RunOCR(form);
            }));

            Application.Run(form);
        }

        static void RunOCR(OcrResultForm form) {
            Image img = Image.FromFile("E:/ocr/scan1.jpg");

            Image sourceImage = new Bitmap((Image) img.Clone());
            form.sourcePV.Image = sourceImage;
            form.sourcePV.ZoomToFit();

            Image bwImage = ImageUtil.ToBlackAndWhite((Bitmap) img.Clone());
            form.bwPV.Image = bwImage;
            form.bwPV.ZoomToFit();

            List<Tuple<Point, Point>> lines = LineRecognition.RunRecognition((Bitmap) bwImage.Clone());

            Bitmap drw = null;
            Util.Timed("Segment drawing", () => {
                drw = new Bitmap(bwImage);

                Graphics g = Graphics.FromImage(drw);

                Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 1);
                foreach (var ln in lines) {
                    int dist = (int) Math.Sqrt(Math.Pow(ln.Item1.X - ln.Item2.X, 2) + Math.Pow(ln.Item1.Y - ln.Item2.Y, 2));
                    if (dist > 1000) {
                        g.DrawLine(p, ln.Item1, ln.Item2);
                    }
                }
                g.Dispose();
            });

            form.outputPV.Image = drw;
            form.outputPV.ZoomToFit();
        }

    }
}
