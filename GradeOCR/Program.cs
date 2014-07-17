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

            Thread worker = new Thread(new ThreadStart(delegate {
                RunOCR(form);
            }));
            worker.IsBackground = true;
            worker.Start();

            Application.Run(form);
        }

        static void RunOCR(OcrResultForm form) {
            Image img = Image.FromFile("E:/Pronko/prj/Grader/ocr-data/scan1.jpg");

            Image sourceImage = new Bitmap((Image) img.Clone());
            form.sourcePV.Invoke(new EventHandler(delegate {
                form.sourcePV.Image = sourceImage;
                form.sourcePV.ZoomToFit();
            }));

            Thread.Sleep(200);

            Image bwImage = ImageUtil.ToBlackAndWhite((Bitmap) img.Clone());
            form.bwPV.Invoke(new EventHandler(delegate {
                form.bwPV.Image = bwImage;
                form.bwPV.ZoomToFit();
            }));

            Thread.Sleep(200);

            BWImage bw = new BWImage((Bitmap) img.Clone());

            Thread.Sleep(200);

            Image freqImage = null;
            Util.Timed("freq", () => {
                freqImage = LineRecognition.DisplayWhiteRows((Bitmap) bwImage.Clone(), bw);
            });
            form.freqPV.Invoke(new EventHandler(delegate {
                form.freqPV.Image = freqImage;
                form.freqPV.ZoomToFit();
            }));

            Thread.Sleep(200);

            List<Tuple<Point, Point>> lines = LineRecognition.RunRecognition(bw);

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

            form.outputPV.Invoke(new EventHandler(delegate {
                form.outputPV.Image = drw;
                form.outputPV.ZoomToFit();
            }));
        }

    }
}
