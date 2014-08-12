using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.IO;
using TableOCR;
using System.Drawing;
using LibUtil;
using LineOCR;

namespace TableRecognitionTest {
    class Program {
        public static readonly string TableInputDir = "E:/Pronko/prj/Grader/ocr-data/register-test-input";
        public static readonly string TableOutputDir = "E:/Pronko/prj/Grader/ocr-data/register-test-output";

        static void Main(string[] args) {
            Util.Timed("table processing", () => {
                var images = Directory.GetFiles(TableInputDir);
                int c = 0;
                foreach (var img in images) {
                    DrawTable(ImageUtil.LoadImage(img)).Save(TableOutputDir + "/" + Path.GetFileName(img));

                    c++;
                    Console.WriteLine("Processed {0}/{1} tables...", c, images.Length);
                }
            });
        }

        static Bitmap DrawTable(Bitmap src) {
            return LineOCR.Program.CreateHoughImage(src);
        }
    }
}
