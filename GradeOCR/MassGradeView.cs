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
using System.IO;

namespace GradeOCR {
    public partial class MassGradeView : Form {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";

        public MassGradeView(Size picSize, Func<Bitmap, Bitmap> converter) {
            InitializeComponent();

            int W = 1900;
            int H = 1000;
            int frameW = 15;
            int frameH = 37;

            int cx = (W - frameW) / (picSize.Width + 1);
            int cy = (H - frameH) / (picSize.Height + 1);

            List<PictureView> pvs = new List<PictureView>();
            for (int y = 0; y < cy; y++) {
                for (int x = 0; x < cx; x++) {
                    var pv = new PictureView();
                    pvs.Add(pv);
                    pv.Size = picSize;
                    pv.Location = new Point(x * (picSize.Width + 1), y * (picSize.Height + 1));
                    pv.ZoomToFit();
                    pv.AllowZoom = false;
                    this.Controls.Add(pv);
                }
            }

            this.Size = new Size(cx * (picSize.Width + 1) + frameW, cy * (picSize.Height + 1) + frameH);
            Console.WriteLine("placed {0} pictures in mass view", cx * cy);

            this.Shown += new EventHandler(delegate {
                Thread worker = new Thread(new ThreadStart(delegate {
                    List<string> images = new List<string>();
                    images.AddRange(Directory.GetFiles(OcrData + "/grade-unsort"));
                    images.AddRange(Directory.GetFiles(OcrData + "/grade-2"));
                    images.AddRange(Directory.GetFiles(OcrData + "/grade-3"));
                    images.AddRange(Directory.GetFiles(OcrData + "/grade-4"));
                    images.AddRange(Directory.GetFiles(OcrData + "/grade-5"));

                    // shuffle images
                    Random r = new Random();
                    images = images.OrderBy(s => r.NextDouble()).ToList();

                    for (int q = 0; q < pvs.Count; q++) {
                        string imageFile = images[q];
                        Bitmap img = ImageUtil.LoadImage(imageFile);
                        pvs[q].Image = converter(img);
                        pvs[q].DoubleClick += new EventHandler(delegate {
                            new GradeRecognitionDebugView(img, imageFile).ShowDialog();
                        });
                    }
                }));
                worker.IsBackground = true;
                worker.Start();
            });
        }

        
    }
}
