using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUtil;
using TableOCR;
using System.IO;

namespace GradeSorter {
    public partial class GradeViewForm : Form {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";

        private PictureView gradePV;

        private string currentFileName;
        private Bitmap currentImage;

        public GradeViewForm() {
            InitializeComponent();

            gradePV = PictureView.InsertIntoPanel(gradePanel);

            this.Shown += new EventHandler(delegate {
                ProcessNextGrade();
            });

            gradePV.KeyUp += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                Dictionary<Keys, int> gradeKeys = new Dictionary<Keys, int> {
                    { Keys.D2, 2 }, { Keys.NumPad2, 2 },
                    { Keys.D3, 3 }, { Keys.NumPad3, 3 },
                    { Keys.D4, 4 }, { Keys.NumPad4, 4 },
                    { Keys.D5, 5 }, { Keys.NumPad5, 5 }
                };
                gradeKeys.GetOption(e.KeyCode).ForEach(grade => {
                    MoveImageToDone(currentFileName, grade);
                    ProcessNextGrade();
                });

            });

        }

        private void ProcessNextGrade() {
            currentFileName = NextImageName();
            this.Text = currentFileName;

            currentImage = LoadImage(currentFileName);

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

        private Bitmap LoadImage(string fileName) {
            FileStream fs = File.OpenRead(fileName);
            Bitmap img = (Bitmap) Image.FromStream(fs);
            fs.Close();
            return img;
        }

        private void MoveImageToDone(string fileName, int grade) {
            string destDir = OcrData + "/grade-" + grade;
            string dest = destDir + "/" + Path.GetFileName(fileName);
            if (File.Exists(dest)) {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = destDir;
                sfd.Title = "Выберите место для сохранения изображения оценки";
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
