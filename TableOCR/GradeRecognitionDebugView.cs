using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using LibUtil;

namespace TableOCR {
    public partial class GradeRecognitionDebugView : Form {
        public PictureView inputImagePV;

        public GradeRecognitionDebugView(Bitmap inputImage) {
            InitializeComponent();

            inputImagePV = PictureView.InsertIntoPanel(inputImagePanel);

            this.Shown += new EventHandler(delegate {
                Thread worker = new Thread(new ThreadStart(delegate {
                    Util.Timed("Grade OCR debug run", () => {
                        RunOCR(inputImage);
                    });
                }));
                worker.IsBackground = true;
                worker.Start();
            });
        }

        public void RunOCR(Bitmap inputImage) {
            inputImagePV.Image = inputImage;
        }
    }
}
