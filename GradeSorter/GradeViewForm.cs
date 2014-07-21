using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUtil;
using System.IO;
using OCRUtil;

namespace GradeSorter {
    public partial class GradeViewForm : Form {
        public string OcrData;

        private PictureView gradePV;

        private string currentFileName;
        private Bitmap currentImage;

        public GradeViewForm() {
            if (Directory.Exists("E:/Pronko/prj/Grader/ocr-data")) {
                OcrData = "E:/Pronko/prj/Grader/ocr-data";
            } else {
                var fbd = new FolderBrowserDialog();
                fbd.Description = "Выберите директорию с оценками";
                if (fbd.ShowDialog() == DialogResult.OK) {
                    OcrData = fbd.SelectedPath;
                } else {
                    throw new Exception("no base directory was chosen");
                }
            }

            InitializeComponent();

            gradePV = PictureView.InsertIntoPanel(gradePanel);

            this.Shown += new EventHandler(delegate {
                ProcessNextGrade();
            });

            gradePV.KeyUp += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.Control && e.KeyCode == Keys.Z) {
                    if (actionHistory.Count > 0) {
                        Tuple<string, string> lastAction = actionHistory.Last();
                        actionHistory.Remove(lastAction);
                        File.Move(lastAction.Item2, lastAction.Item1);
                        ProcessGrade(lastAction.Item1);
                    }
                } else {
                    Dictionary<Keys, int> gradeKeys = new Dictionary<Keys, int> {
                        { Keys.D2, 2 }, { Keys.NumPad2, 2 },
                        { Keys.D3, 3 }, { Keys.NumPad3, 3 },
                        { Keys.D4, 4 }, { Keys.NumPad4, 4 },
                        { Keys.D5, 5 }, { Keys.NumPad5, 5 },
                        { Keys.D0, 0 }, { Keys.NumPad0, 0 }
                    };
                    gradeKeys.GetOption(e.KeyCode).ForEach(grade => {
                        MoveImageToDone(currentFileName, grade);
                        ProcessNextGrade();
                    });
                }
            });

        }

        private void ProcessNextGrade() {
            ProcessGrade(NextImageName());
        }

        private void ProcessGrade(string fileName) {
            currentFileName = fileName;
            this.Text = currentFileName;

            currentImage = ImageUtil.LoadImage(currentFileName);

            gradePV.Image = currentImage;
        }

        private string NextImageName() {
            string[] images = Directory.GetFiles(OcrData + "/grade-unsort/");
            if (images.Length > 0) {
                return images[0];
            } else {
                MessageBox.Show("Все оценки обработаны");
                Environment.Exit(0);
                return null;
            }
        }

        private List<Tuple<string, string>> actionHistory = new List<Tuple<string, string>>();

        private void MoveImageToDone(string fileName, int grade) {
            string destDir = OcrData + "/grade-" + grade;
            string dest = destDir + "/" + Path.GetFileName(fileName);
            if (File.Exists(dest)) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = destDir;
                sfd.Title = "Выберите место для сохранения изображения оценки";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    dest = sfd.FileName;
                } else {
                    throw new Exception("File name already exists!");
                }
            }
            File.Move(fileName, dest);
            actionHistory.Add(new Tuple<string, string>(fileName, dest));
        }
    }
}
