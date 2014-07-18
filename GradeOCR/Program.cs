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
            Bitmap sourceImage = (Bitmap) Image.FromFile("E:/Pronko/prj/Grader/ocr-data/scan1.jpg");
            Util.Timed("to std format", () => {
                sourceImage = ImageUtil.ToStdFormat(sourceImage);
            });

            form.sourcePV.Image = sourceImage;

            Bitmap sourceImageVert = Util.Timed("rotate", () => ImageUtil.Rotate(sourceImage));
            form.sourcePV_vert.Image = sourceImageVert;

            Bitmap bwImage = Util.Timed("to black and white (horiz)", () => ImageUtil.ToBlackAndWhite(sourceImage));
            form.bwPV.Image = bwImage;

            Bitmap bwImageVert = Util.Timed("to black and white (vert)", () => ImageUtil.ToBlackAndWhite(sourceImageVert));
            form.bwPV_vert.Image = bwImageVert;

            BWImage bw = Util.Timed("bw (horiz)", () => new BWImage(sourceImage));
            BWImage bwVert = Util.Timed("bw (vert)", () => new BWImage(sourceImageVert));

            Bitmap freqImage = Util.Timed("freq", () => LineRecognition.DisplayWhiteRows(bwImage, bw));
            form.freqPV.Image = freqImage;

            Bitmap freqImageVert = Util.Timed("freqVert", () => LineRecognition.DisplayWhiteRows(bwImageVert, bwVert));
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
