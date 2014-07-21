using System;
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

            Application.Run(new MassGradeView(new Size(100, 50), b => {
                return RecognizeGrade(b);
            }));
        }

        public static Bitmap RecognizeGrade(Bitmap img) {
            return
                NoiseCleaner.RemoveNoise(
                    BorderRemoval.RemoveBorder(img));
        }

    }
}
