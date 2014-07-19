using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibUtil;

namespace GradeOCR {
    public partial class TableRecognitionDebugView : Form {
        public PictureView sourcePV;
        public PictureView bwPV;
        public PictureView freqPV;
        public PictureView outputPV;

        public PictureView sourcePV_vert;
        public PictureView bwPV_vert;
        public PictureView freqPV_vert;
        public PictureView outputPV_vert;

        public PictureView resultPV;

        public TableRecognitionDebugView(Bitmap sourceImage) {
            InitializeComponent();
            sourcePV = PictureView.InsertIntoPanel(this.sourcePanel);
            bwPV = PictureView.InsertIntoPanel(this.bwPanel);
            freqPV = PictureView.InsertIntoPanel(this.freqPanel);
            outputPV = PictureView.InsertIntoPanel(this.outputPanel);

            sourcePV_vert = PictureView.InsertIntoPanel(this.sourcePanelVert);
            bwPV_vert = PictureView.InsertIntoPanel(this.bwPanelVert);
            freqPV_vert = PictureView.InsertIntoPanel(this.freqPanelVert);
            outputPV_vert = PictureView.InsertIntoPanel(this.outputPanelVert);

            resultPV = PictureView.InsertIntoPanel(this.tableResultPanel);

            this.Shown += new EventHandler(delegate {
                Thread worker = new Thread(new ThreadStart(delegate {
                    Util.Timed("Table OCR debug run", () => {
                        RunOCR(sourceImage);
                    });
                }));
                worker.IsBackground = true;
                worker.Start();
            });
        }

        public void RunOCR(Bitmap sourceImage) {
            this.sourcePV.Image = sourceImage;

            Bitmap sourceImageVert = Util.Timed("rotate", () => ImageUtil.Rotate(sourceImage));
            this.sourcePV_vert.Image = sourceImageVert;

            Bitmap bwImage = Util.Timed("to black and white (horiz)", () => ImageUtil.ToBlackAndWhite(sourceImage));
            this.bwPV.Image = bwImage;

            Bitmap bwImageVert = Util.Timed("to black and white (vert)", () => ImageUtil.ToBlackAndWhite(sourceImageVert));
            this.bwPV_vert.Image = bwImageVert;

            BWImage bw = Util.Timed("bw (horiz)", () => new BWImage(sourceImage));
            BWImage bwVert = Util.Timed("bw (vert)", () => new BWImage(sourceImageVert));

            Bitmap freqImage = Util.Timed("freq", () => WhiteRowDetection.DisplayWhiteRows(bwImage, bw));
            this.freqPV.Image = freqImage;

            Bitmap freqImageVert = Util.Timed("freqVert", () => WhiteRowDetection.DisplayWhiteRows(bwImageVert, bwVert));
            this.freqPV_vert.Image = freqImageVert;

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

            this.outputPV.Image = drw;

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
            this.outputPV_vert.Image = drwVert;

            Table t = new Table(lines, linesVert);
            Bitmap tablePic = new Bitmap(bwImage);
            Graphics tableG = Graphics.FromImage(tablePic);
            t.DrawTable(tableG, p);
            tableG.Dispose();
            this.resultPV.Image = tablePic;
            this.resultPV.AddDoubleClickListener((pt, e) => {
                t.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                    var gradeRecognition = new GradeRecognitionDebugView(t.GetCellImage(bwImage, cell.X, cell.Y));
                    gradeRecognition.ShowDialog();
                });
            });
        }
    }
}
