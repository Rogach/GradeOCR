using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GradeOCR;
using System.Drawing;
using System.IO;
using OCRUtil;
using LibUtil;

namespace DigestDatabase {
    class Program {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";

        static void Main(string[] args) {
            List<GradeDigest> gradeDigests = new List<GradeDigest>();

            List<Tuple<string, byte>> gradeGroups = new List<Tuple<string, byte>> {
                new Tuple<string, byte>(OcrData + "/grade-2/", 2),
                new Tuple<string, byte>(OcrData + "/grade-3/", 3),
                new Tuple<string, byte>(OcrData + "/grade-4/", 4),
                new Tuple<string, byte>(OcrData + "/grade-5/", 5)
            };

            List<Tuple<string, byte>> inputImages = new List<Tuple<string, byte>>();
            foreach (var gg in gradeGroups) {
                string[] imageFiles = Directory.GetFiles(gg.Item1);
                foreach (var imageFile in imageFiles) {
                    inputImages.Add(new Tuple<string, byte>(imageFile, gg.Item2));
                }
            }

            Util.Timed("digest loading", () => {

                int c = 0;
                foreach (var input in inputImages) {
                    if (c % 100 == 0) Console.WriteLine("Processed {0}/{1} images...", c, inputImages.Count);

                    GradeDigest gd =
                        GradeDigest.FromImage(
                            GradeOCR.Program.NormalizeImage(
                                ImageUtil.LoadImage(input.Item1)));
                    gd.grade = input.Item2;
                    gradeDigests.Add(gd);

                    c++;
                }

            });

            GradeDigestSet digestSet = new GradeDigestSet(gradeDigests);
            digestSet.Save(OcrData + "/grade-digests.db");
        }
    }
}
