﻿using System;
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

            this.registerPV.AddDoubleClickListener(pt => {
                currentTable.GetCellAtPoint(pt.X, pt.Y).ForEach(cell => {
                    Bitmap cellImage = currentTable.GetCellImage(origImage, cell.Item1, cell.Item2);

                    cellImage.Save(GetNextUnsortGradeImageName());

                    // color processed cell with green
                    GraphicsPath cellPath = currentTable.GetCellContour(cell.Item1, cell.Item2);
                    Graphics g = Graphics.FromImage(currentImage);
                    g.FillPath(new SolidBrush(Color.FromArgb(100, Color.Green)), cellPath);
                    g.Dispose();
                    registerPV.SetImageKeepZoom(currentImage);
                });
            });
        }

        private void ProcessNextImage() {
            currentFileName = NextImageName();
            this.Text = currentFileName;

            currentImage = LoadImage(currentFileName);
            origImage = new Bitmap(currentImage);

            currentTable = GradeOCR.Program.RecognizeTable(currentImage);
            Graphics g = Graphics.FromImage(currentImage);
            Pen p = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
            currentTable.DrawTable(g, p);
            g.Dispose();

            registerPV.Image = currentImage;
        }

        private string GetNextUnsortGradeImageName() {
            string[] images = Directory.GetFiles(OcrData + "/grade-unsort");
            if (images.Length == 0) {
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
            string[] images = Directory.GetFiles("E:/Pronko/prj/Grader/ocr-data/register-new/");
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
            return ImageUtil.ToBlackAndWhite(ImageUtil.ToStdFormat(img));
        }

        private void MoveImageToDone(string fileName) {
            File.Move(fileName, "E:/Pronko/prj/Grader/ocr-data/register-old/" + Path.DirectorySeparatorChar + Path.GetFileName(fileName));
        }

    }
}
