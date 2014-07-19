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

namespace RegisterProcessor {
    public partial class RegisterViewForm : Form {
        public PictureView registerPV;

        private string currentFileName;
        private Bitmap currentImage;

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
        }

        private void ProcessNextImage() {
            currentFileName = NextImageName();
            this.Text = currentFileName;
            currentImage = LoadImage(currentFileName);
            registerPV.Image = currentImage;
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
            return ImageUtil.ToStdFormat(img);
        }

        private void MoveImageToDone(string fileName) {
            File.Move(fileName, "E:/Pronko/prj/Grader/ocr-data/register-old/" + Path.DirectorySeparatorChar + Path.GetFileName(fileName));
        }

    }
}
