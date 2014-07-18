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
                Util.Timed("OCR run", () => {
                    RunOCR(form);
                });
            }));
            worker.IsBackground = true;
            worker.Start();

            Application.Run(form);
        }

        static void RunOCR(OcrResultForm form) {
            Image img = Image.FromFile("E:/Pronko/prj/Grader/ocr-data/scan1.jpg");

            Image sourceImage = new Bitmap((Image) img.Clone());
            form.sourcePV.Image = sourceImage;

            Image sourceImageVert = Util.Timed("rotate", () => { return ImageUtil.Rotate((Bitmap) img.Clone()); });
            form.sourcePV_vert.Image = sourceImageVert;

            Image bwImage = ImageUtil.ToBlackAndWhite((Bitmap) img.Clone());
            form.bwPV.Image = bwImage;

            Image bwImageVert = ImageUtil.ToBlackAndWhite((Bitmap) sourceImageVert.Clone());
            form.bwPV_vert.Image = bwImageVert;

            BWImage bw = new BWImage((Bitmap) img.Clone());
            BWImage bwVert = new BWImage((Bitmap) sourceImageVert.Clone());

            Image freqImage = null;
            Util.Timed("freq", () => {
                freqImage = LineRecognition.DisplayWhiteRows((Bitmap) bwImage.Clone(), bw);
            });
            form.freqPV.Image = freqImage;

            Image freqImageVert = null;
            Util.Timed("freqVert", () => {
                freqImageVert = LineRecognition.DisplayWhiteRows((Bitmap) bwImageVert.Clone(), bwVert);
            });
            form.freqPV_vert.Image = freqImageVert;

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

            form.outputPV.Image = drw;

            List<Line> linesVert = Util.Timed("sweepline segment detection (vert)", () => { 
                return LineRecognition.RunRecognition(bwVert, (int) (bw.Height * LineRecognition.minVerticalLineRatio)); 
            }).ConvertAll(ln => {
                return new Line(new Point(bwImage.Width - 1 - ln.p1.Y, ln.p1.X), new Point(bwImage.Width - 1 - ln.p2.Y, ln.p2.X));
            });
            Bitmap drwVert = new Bitmap(bwImage);

            Graphics gVert = Graphics.FromImage(drwVert);
            foreach (var ln in linesVert) {
                gVert.DrawLine(p, ln.p1.X, ln.p1.Y, ln.p2.X, ln.p2.Y);
            }
            gVert.Dispose();
            form.outputPV_vert.Image = drwVert;

            Table t = new Table(lines, linesVert);
            Bitmap tablePic = new Bitmap(bwImage);
            Graphics tableG = Graphics.FromImage(tablePic);
            t.DrawTable(tableG, p);
            tableG.Dispose();
            form.resultPV.Image = tablePic;
            form.resultPV.AddDoubleClickListener(pt => {
                t.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                    Console.WriteLine(cell);
                });
            });
        }

    }
}
