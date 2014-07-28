using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TableOCR;
using System.IO;
using LibUtil;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using OCRUtil;

namespace RegisterProcessor {
    public partial class RegisterViewForm : Form {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";

        public PictureView registerPV;

        private string currentFileName;
        private Bitmap origImage;
        private Bitmap bwImage;
        private Bitmap currentImage;
        private Option<Table> currentTable;

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
                currentTable.ForEach(table => {
                    if (e.Button == MouseButtons.Left) {
                        table.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                            ProcessTableCell(cell.X, cell.Y);

                            // color processed cell with green
                            GraphicsPath cellPath = table.GetCellContour(cell.X, cell.Y);
                            Graphics g = Graphics.FromImage(currentImage);
                            g.FillPath(new SolidBrush(Color.FromArgb(100, Color.Green)), cellPath);
                            g.Dispose();
                            registerPV.SetImageKeepZoom(currentImage);
                        });
                    } else if (e.Button == MouseButtons.Right) {
                        table.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                            if (selectionStart.HasValue) {
                                Graphics g = Graphics.FromImage(currentImage);

                                int minX = Math.Min(selectionStart.Value.X, cell.X);
                                int maxX = Math.Max(selectionStart.Value.X, cell.X);
                                int minY = Math.Min(selectionStart.Value.Y, cell.Y);
                                int maxY = Math.Max(selectionStart.Value.Y, cell.Y);

                                ProgressDialogs.WithProgress((maxX - minX + 1) * (maxY - minY + 1), pd => {
                                    for (int y = minY; y <= maxY; y++) {
                                        for (int x = minX; x <= maxX; x++) {
                                            ProcessTableCell(x, y);
                                            GraphicsPath cellPath = table.GetCellContour(x, y);
                                            g.FillPath(new SolidBrush(Color.FromArgb(100, Color.Green)), cellPath);
                                            pd.Increment();
                                        }
                                    }
                                });

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
            });

            this.debugOcrButton.Click += new EventHandler(delegate {
                (new TableRecognitionDebugView(origImage)).ShowDialog();
            });
        }

        private void ProcessNextImage() {
            currentFileName = NextImageName();
            this.Text = currentFileName;

            origImage = ImageUtil.LoadImage(currentFileName);
            bwImage = ImageUtil.ToBlackAndWhite(origImage);
            currentImage = new Bitmap(bwImage);

            currentTable = TableOCR.Program.RecognizeTable(currentImage);
            Graphics g = Graphics.FromImage(currentImage);
            Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
            currentTable.ForEach(table => table.DrawTable(g, p));
            g.Dispose();

            registerPV.Image = currentImage;
        }

        private void ProcessTableCell(int x, int y) {
            currentTable.ForEach(table => {
                Bitmap cellImage = table.GetCellImage(bwImage, x, y);
                cellImage.Save(GetNextUnsortGradeImageName());
            });
        }

        private string GetNextUnsortGradeImageName() {
            List<string> images = new List<string>();
            images.AddRange(Directory.GetFiles(OcrData + "/grade-unsort"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-2"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-3"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-4"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-5"));
            images.AddRange(Directory.GetFiles(OcrData + "/grade-0"));
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
