using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using System.Threading;
using TableOCR;
using LibUtil;
using GradeOCR;
using System.IO;

namespace RegisterOCR {
    public partial class RegisterView : Form {
        private PictureView registerPV;

        private string imageFileName;
        private Bitmap originalImage;
        private Option<Table> currentTable;

        private Point? cell1 = null;
        private Point? cell2 = null;

        public RegisterView() {
            InitializeComponent();

            registerPV = PictureView.InsertIntoPanel(registerPanel);

            this.Shown += new EventHandler(delegate {
                Option<string> nextRegisterName = GetRegisterFileName();
                nextRegisterName.ForEach(fileName => {
                    LoadRegister(fileName);
                });
                if (nextRegisterName.IsEmpty()) {
                    Environment.Exit(0);
                }
            });

            registerPV.AddDoubleClickListener((pt, e) => {
                if (e.Button == MouseButtons.Left) {
                    currentTable.ForEach(table => {
                        table.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                            if (cell1.HasValue && !cell2.HasValue) {
                                cell2 = cell;
                                CopyData();
                            } else {
                                cell1 = cell;
                                cell2 = null;
                                MarkCell();
                            }
                        });
                    });
                }
            });

            debugButton.Click += new EventHandler(delegate {
                new TableRecognitionDebugView(ImageUtil.LoadImage(imageFileName)).ShowDialog();
            });

            selectRegisterButton.Click += new EventHandler(delegate {
                Option<string> nextRegisterName = GetRegisterFileName();
                nextRegisterName.ForEach(fileName => {
                    LoadRegister(fileName);
                });
            });

            nextRegisterButton.Click += new EventHandler(delegate {
                string imgDir = Path.GetDirectoryName(imageFileName);
                string[] files = Directory.GetFiles(imgDir);
                int imgN = 0;
                for (int q = 0; q < files.Length; q++) {
                    if (files[q] == imageFileName) {
                        imgN = q + 1;
                    }
                }
                if (imgN < files.Length) {
                    LoadRegister(files[imgN]);
                } else {
                    MessageBox.Show("Ведомостей больше нет!");
                }
            });
        }

        private Option<string> GetRegisterFileName() {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Выберите изображение ведомости";
            if (fd.ShowDialog() == DialogResult.OK) {
                return new Some<string>(fd.FileName);
            } else {
                return new None<string>();
            }
        }

        private void LoadRegister(string fileName) {
            registerPV.Image = PictureView.LoadPlaceholder();
            debugButton.Enabled = false;
            nextRegisterButton.Enabled = false;
            selectRegisterButton.Enabled = false;
            cell1 = null;
            cell2 = null;
            imageFileName = fileName;
            Thread worker = new Thread(new ThreadStart(delegate {
                originalImage = ImageUtil.ToBlackAndWhite(ImageUtil.LoadImage(fileName));

                currentTable = TableOCR.Program.RecognizeTable(originalImage);
                if (currentTable.IsEmpty()) {
                    registerPV.Invoke(new EventHandler(delegate {
                        MessageBox.Show("Не удалось распознать таблицу");
                    }));
                }
                    
                Bitmap processedImage = new Bitmap(originalImage);
                Graphics g = Graphics.FromImage(processedImage);
                currentTable.ForEach(table => {
                    table.DrawTable(g, new Pen(Color.Red, 2));
                });
                g.Dispose();

                registerPV.Image = processedImage;
                this.Invoke(new EventHandler(delegate {
                    this.Text = fileName;
                }));
                debugButton.Invoke(new EventHandler(delegate {
                    debugButton.Enabled = true;
                }));
                selectRegisterButton.Invoke(new EventHandler(delegate {
                    selectRegisterButton.Enabled = true;
                }));
                nextRegisterButton.Invoke(new EventHandler(delegate {
                    nextRegisterButton.Enabled = true;
                }));
            }));
            worker.IsBackground = true;
            worker.Start();
        }

        private void MarkCell() {
            Bitmap currentImage = new Bitmap(originalImage);
            Graphics g = Graphics.FromImage(currentImage);
            currentTable.ForEach(table => {
                table.DrawTable(g, new Pen(Color.Red, 2));
                Brush fillBrush = new SolidBrush(Color.FromArgb(50, Color.Blue));
                if (cell1.HasValue) {
                    g.FillPath(fillBrush, table.GetCellContour(cell1.Value.X, cell1.Value.Y));
                }
            });
            g.Dispose();
            registerPV.SetImageKeepZoom(currentImage);
        }

        private void CopyData() {
            if (cell1.HasValue && cell2.HasValue && currentTable.NonEmpty()) {
                Point c1 = cell1.Value;
                Point c2 = cell2.Value;

                int minY = Math.Min(c1.Y, c2.Y);
                int maxY = Math.Max(c1.Y, c2.Y);
                int minX = Math.Min(c1.X, c2.X);
                int maxX = Math.Max(c1.X, c2.X);

                string str = "";

                Bitmap currentImage = new Bitmap(originalImage);
                Graphics g = Graphics.FromImage(currentImage);
                currentTable.ForEach(table => {
                    table.DrawTable(g, new Pen(Color.Red, 2));
                    Brush recognitionBrush = new SolidBrush(Color.FromArgb(50, Color.Green));
                    Brush unsureBrush = new SolidBrush(Color.FromArgb(50, Color.Yellow));
                    Brush noneBrush = new SolidBrush(Color.FromArgb(50, Color.Red));

                    int cellCount = (maxY - minY + 1) * (maxX - minX + 1);
                    ProgressDialogs.WithProgress(cellCount, ph => {
                        for (int y = minY; y <= maxY; y++) {
                            for (int x = minX; x <= maxX; x++) {
                                Option<GradeDigest> digestOpt = GradeOCR.Program.GetGradeDigest(table.GetCellImage(originalImage, x, y));
                                digestOpt.ForEach(gd => {
                                    RecognitionResult res = GradeDigestSet.staticInstance.FindBestMatch(gd);
                                    str += res.Digest.grade;
                                    if (MatchConfidence.Sure(res.ConfidenceScore)) {
                                        g.FillPath(recognitionBrush, table.GetCellContour(x, y));
                                    } else {
                                        g.FillPath(unsureBrush, table.GetCellContour(x, y));
                                    }
                                });
                                if (digestOpt.IsEmpty()) {
                                    g.FillPath(unsureBrush, table.GetCellContour(x, y));
                                }
                                str += "\t";
                                ph.Increment();
                            }
                            str += "\n";
                        }
                    });
                });
                g.Dispose();
                registerPV.SetImageKeepZoom(currentImage);

                str = str.Substring(0, str.Length - 1); // trim last newline

                Clipboard.SetText(str);
            }

        }
    }
}
