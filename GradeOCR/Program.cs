﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using OCRUtil;
using System.IO;

namespace GradeOCR {
    public class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new GradeRecognitionDebugView(
            //    ImageUtil.LoadImage("E:/Pronko/prj/Grader/ocr-data/grade-3/g13571.png"), ""));

            Application.Run(new MassGradeView(new Size(50, 50), b => {
                return NormalizeImage(b);
            }));
        }

        public static Bitmap NormalizeImage(Bitmap img) {
            return
                DigestExtractor.ExtractDigestImage(
                    WhitespaceCropper.CropWhitespace(
                        NoiseCleaner.RemoveNoise(
                            BorderRemoval.RemoveBorder(img))));
        }

    }
}
