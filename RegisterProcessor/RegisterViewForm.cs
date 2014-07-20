using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GradeOCR;
using System.IO;
using LibUtil;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace RegisterProcessor {
    public partial class RegisterViewForm : Form {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";

        public PictureView registerPV;

        private string currentFileName;
        private Bitmap origImage;
        private Bitmap bwImage;
        private Bitmap currentImage;
        private Table currentTable;

        public RegisterViewForm() {
            InitializeComponent();

            registerPV = PictureView.InsertIntoPanel(registerPanel);

            this.Shown += new EventHandler(delegate {
                ProcessNextImage();
            });

            this.nextRegisterButton.Click += new EventHandler(delegate {
                MoveImageToDone(currentFileName);
                ProcessNextImage();
            });

            Point? selectionStart = null;
            this.registerPV.AddDoubleClickListener((pt, e) => {
                if (e.Button == MouseButtons.Left) {
                    currentTable.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                        ProcessTableCell(cell.X, cell.Y);

                        // color processed cell with green
                        GraphicsPath cellPath = currentTable.GetCellContour(cell.X, cell.Y);
                        Graphics g = Graphics.FromImage(currentImage);
                        g.FillPath(new SolidBrush(Color.FromArgb(100, Color.Green)), cellPath);
                        g.Dispose();
                        registerPV.SetImageKeepZoom(currentImage);
                    });
                } else if (e.Button == MouseButtons.Right) {
                    currentTable.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                        if (selectionStart.HasValue) {
                            Graphics g = Graphics.FromImage(currentImage);

                            for (int y = selectionStart.Value.Y; y <= cell.Y; y++) {
                                for (int x = selectionStart.Value.X; x <= cell.X; x++) {
                                    ProcessTableCell(x, y);
                                    GraphicsPath cellPath = currentTable.GetCellContour(x, y);
                                    g.FillPath(new SolidBrush(Color.FromArgb(100, Color.Green)), cellPath);
                                }
                            }

                            // color processed cells with green
                            g.Dispose();
                            registerPV.SetImageKeepZoom(currentImage);

                            selectionStart = null;
                        } else {
                            selectionStart = new Point(cell.X, cell.Y);
                        }
                    });
                }
            });

            this.debugOcrButton.Click += new EventHandler(delegate {
                (new TableRecognitionDebugView(origImage)).ShowDialog();
            });
        }

        private void ProcessNextImage() {
            currentFileName = NextImageName();
            this.Text = currentFileName;

            origImage = LoadImage(currentFileName);
            bwImage = ImageUtil.ToBlackAndWhite(origImage);
            currentImage = new Bitmap(bwImage);

            currentTable = GradeOCR.Program.RecognizeTable(currentImage);
            Graphics g = Graphics.FromImage(currentImage);
            Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
            currentTable.DrawTable(g, p);
            g.Dispose();

            registerPV.Image = currentImage;
        }

        private void ProcessTableCell(int x, int y) {
            Bitmap cellImage = currentTable.GetCellImage(bwImage, x, y);
            cellImage.Save(GetNextUnsortGradeImageName());
        }

        private string GetNextUnsortGradeImageName() {
            List<string> images = new List<string>();
            images.AddRange(Directory.GetFiles(OcrData + "/grade-unsort"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-2"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-3"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-4"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-5"));
            if (images.Count == 0) {
                return OcrData + "/grade-unsort/g00001.png";
            } else {
                Regex rgx = new Regex(@"g(\d{5}).png");
                int nextN = images.Select(img => {
                    var m = rgx.Match(Path.GetFileName(img));
                    if (m.Success) {
                        return int.Parse(m.Groups[1].Value);
                    } else {
                        return 0;
                    }
                }).Max();
                return OcrData + "/grade-unsort/g" + (nextN + 1).ToString().PadLeft(5, '0') + ".png";
            }
        }

        private string NextImageName() {
            string[] images = Directory.GetFiles(OcrData + "/register-new/");
            if (images.Length > 0) {
                return images[0];
            } else {
                MessageBox.Show("Все ведомости обработаны");
                Environment.Exit(0);
                return null;
            }
        }

        private Bitmap LoadImage(string fileName) {
            FileStream fs = File.OpenRead(fileName);
            Bitmap img = (Bitmap) Image.FromStream(fs);
            fs.Close();
            return ImageUtil.ToStdFormat(img);
        }

        private void MoveImageToDone(string fileName) {
            string dest = OcrData + "/register-old/" + Path.GetFileName(fileName);
            if (File.Exists(dest)) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = OcrData + "/register-old/";
                sfd.Title = "Выберите место для сохранения обработанной ведомости";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    dest = sfd.FileName;
                    File.Move(fileName, dest);
                } else {
                    throw new Exception("File name already exists!");
                }
            } else {
                File.Move(fileName, dest);
            }
        }

    }
}
