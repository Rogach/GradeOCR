using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using System.IO;
using TableOCR;
using System.Drawing;
using LibUtil;

namespace TableRecognitionTest {
    class Program {
        public static readonly string TableInputDir = "E:/Pronko/prj/Grader/ocr-data/register-bad";
        public static readonly string TableOutputDir = "E:/Pronko/prj/Grader/ocr-data/register-bad-tables";

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
            Bitmap res = new Bitmap(src);
            var tableOpt = TableOCR.Program.RecognizeTable(src);
            var g = Graphics.FromImage(res);
            tableOpt.ForEach(table => {
                table.DrawTable(g, new Pen(Brushes.Red, 2));
            });
            g.Dispose();
            return res;
        }
    }
}
