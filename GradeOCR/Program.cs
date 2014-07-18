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

            Image sourceImageVert = Util.Timed("rotate", () => { return ImageUtil.Rotate((Bitmap) img.Clone()); });
            form.sourcePV_vert.Invoke(new EventHandler(delegate {
                form.sourcePV_vert.Image = sourceImageVert;
                form.sourcePV_vert.ZoomToFit();
            }));


            Thread.Sleep(200);

            Image bwImage = ImageUtil.ToBlackAndWhite((Bitmap) img.Clone());
            form.bwPV.Invoke(new EventHandler(delegate {
                form.bwPV.Image = bwImage;
                form.bwPV.ZoomToFit();
            }));

            Thread.Sleep(200);

            Image bwImageVert = ImageUtil.ToBlackAndWhite((Bitmap) sourceImageVert.Clone());
            form.bwPV_vert.Invoke(new EventHandler(delegate {
                form.bwPV_vert.Image = bwImageVert;
                form.bwPV_vert.ZoomToFit();
            }));

            Thread.Sleep(200);

            BWImage bw = new BWImage((Bitmap) img.Clone());
            BWImage bwVert = new BWImage((Bitmap) sourceImageVert.Clone());

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

            Image freqImageVert = null;
            Util.Timed("freqVert", () => {
                freqImageVert = LineRecognition.DisplayWhiteRows((Bitmap) bwImageVert.Clone(), bwVert);
            });
            form.freqPV_vert.Invoke(new EventHandler(delegate {
                form.freqPV_vert.Image = freqImageVert;
                form.freqPV_vert.ZoomToFit();
            }));

            Thread.Sleep(200);

            List<Line> lines = Util.Timed("sweepline segment detection", () => { 
                return LineRecognition.RunRecognition(bw, (int) (bw.Width * LineRecognition.minHorizontalLineRatio)); 
            });

            Bitmap drw = new Bitmap(bwImage);

            Graphics g = Graphics.FromImage(drw);

            Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
            foreach (var ln in lines) {
                g.DrawLine(p, ln.p1, ln.p2);
            }
            g.Dispose();

            form.outputPV.Invoke(new EventHandler(delegate {
                form.outputPV.Image = drw;
                form.outputPV.ZoomToFit();
            }));

            Thread.Sleep(200);

            List<Line> linesVert = Util.Timed("sweepline segment detection (vert)", () => { 
                return LineRecognition.RunRecognition(bwVert, (int) (bw.Height * LineRecognition.minVerticalLineRatio)); 
            });
            Bitmap drwVert = new Bitmap(bwImage);

            Graphics gVert = Graphics.FromImage(drwVert);
            foreach (var ln in linesVert) {
                gVert.DrawLine(p, drwVert.Width - 1 - ln.p1.Y, ln.p1.X, drwVert.Width - 1 - ln.p2.Y, ln.p2.X);
            }
            gVert.Dispose();
            form.outputPV_vert.Invoke(new EventHandler(delegate {
                form.outputPV_vert.Image = drwVert;
                form.outputPV_vert.ZoomToFit();
            }));
        }

    }
}
