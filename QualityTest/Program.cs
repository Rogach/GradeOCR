using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GradeOCR;
using LibUtil;
using System.IO;
using OCRUtil;

namespace QualityTest {
    class Program {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";
        public static readonly string TestOcrData = "E:/Pronko/prj/Grader/ocr-data/test-data";

        static void Main(string[] args) {
            var digestSet = GradeDigestSet.ReadDefault();

            List<Tuple<string, GradeDigest>> testDigests = LoadTestDigests();
            var gradePairs = new List<Tuple<GradeDigest, RecognitionResult>>();
            Util.Timed("compare with database", () => {
                int c = 0;
                foreach (var gd in testDigests) {
                    if (c % 100 == 0) Console.WriteLine("Processed {0}/{1} digests...", c, testDigests.Count);

                    gradePairs.Add(new Tuple<GradeDigest, RecognitionResult>(gd.Item2, digestSet.FindBestMatch(gd.Item2)));

                    c++;
                }
            });

            // output results

            Func<byte, int> rights = g => gradePairs.Count(gp =>
                gp.Item1.grade == gp.Item2.Digest.grade && 
                gp.Item1.grade == g);
            Func<byte, int> wrongs = g => gradePairs.Count(gp =>
                gp.Item1.grade != gp.Item2.Digest.grade && 
                gp.Item1.grade == g);
            

            Func<int, double> percentTotal = c => (double) c / gradePairs.Count * 100;
            Func<int, int, double> percent = (c, g) => {
                int tot = gradePairs.Count(gp => gp.Item1.grade == g);
                if (tot > 0) {
                    return (double) c / tot * 100;
                } else {
                    return 0;
                }
            };
    
            Console.WriteLine("RESULTS:\n");

            int rightTotal = gradePairs.Count(gp => gp.Item1.grade == gp.Item2.Digest.grade);
            int wrongTotal = gradePairs.Count(gp => gp.Item1.grade != gp.Item2.Digest.grade);

            Console.WriteLine("Total (r/w): {0}/{1}, {2:F1}/{3:F1} %", 
                rightTotal, wrongTotal, 
                percentTotal(rightTotal), percentTotal(wrongTotal));

            for (byte g = 2; g <= 5; g++) {
                Console.WriteLine("{0}: {1}/{2}, {3:F1}/{4:F1} %", g,
                    rights(g), wrongs(g), percent(rights(g), g), percent(wrongs(g), g));
            }

            Console.WriteLine();

            foreach (double confidenceThreshold in new List<double> { 0.7, 0.6, 0.5 }) {
                Console.WriteLine("confidence threshold = " + confidenceThreshold);

                Func<byte, int> sureRights = g => gradePairs.Count(gp =>
                    gp.Item1.grade == gp.Item2.Digest.grade &&
                    gp.Item1.grade == g &&
                    gp.Item2.Confidence > confidenceThreshold);
                Func<byte, int> sureWrongs = g => gradePairs.Count(gp =>
                    gp.Item1.grade != gp.Item2.Digest.grade &&
                    gp.Item1.grade == g &&
                    gp.Item2.Confidence > confidenceThreshold);
                Func<byte, int> unsure = g => gradePairs.Count(gp =>
                    gp.Item1.grade == g &&
                    gp.Item2.Confidence <= confidenceThreshold);

                int sureRightTotal = gradePairs.Count(gp =>
                    gp.Item1.grade == gp.Item2.Digest.grade &&
                    gp.Item2.Confidence > confidenceThreshold);
                int sureWrongTotal = gradePairs.Count(gp =>
                    gp.Item1.grade != gp.Item2.Digest.grade &&
                    gp.Item2.Confidence > confidenceThreshold);
                int unsureTotal = gradePairs.Count(gp => gp.Item2.Confidence <= confidenceThreshold);

                Console.WriteLine("Total (r/w/u): {0}/{1}/{2}, {3:F1}/{4:F1}/{5:F1} %",
                    sureRightTotal, sureWrongTotal, unsureTotal,
                    percentTotal(sureRightTotal), percentTotal(sureWrongTotal), percentTotal(unsureTotal));

                for (byte g = 2; g <= 5; g++) {
                    Console.WriteLine("{0}: {1}/{2}/{3}, {4:F1}/{5:F1}/{6:F1} %", g,
                        sureRights(g), sureWrongs(g), unsure(g),
                        percent(sureRights(g), g), percent(sureWrongs(g), g), percent(unsure(g), g));
                }

                Console.WriteLine();
            }

            Console.WriteLine("Recognition failures:");
            gradePairs.Where(gp => gp.Item1.grade != gp.Item2.Digest.grade).Where(gp => gp.Item1.grade != 2).ToList().ForEach(gp => {
                Console.WriteLine(testDigests.Find(gd => gd.Item2 == gp.Item1).Item1);
            });
        }

        public static List<Tuple<string, GradeDigest>> LoadTestDigests() {
            List<Tuple<string, GradeDigest>> gradeDigests = new List<Tuple<string, GradeDigest>>();

            List<Tuple<string, byte>> gradeGroups = new List<Tuple<string, byte>> {
                new Tuple<string, byte>(TestOcrData + "/grade-2/", 2),
                new Tuple<string, byte>(TestOcrData + "/grade-3/", 3),
                new Tuple<string, byte>(TestOcrData + "/grade-4/", 4),
                new Tuple<string, byte>(TestOcrData + "/grade-5/", 5)
            };

            List<Tuple<string, byte>> inputImages = new List<Tuple<string, byte>>();
            foreach (var gg in gradeGroups) {
                string[] imageFiles = Directory.GetFiles(gg.Item1);
                foreach (var imageFile in imageFiles) {
                    inputImages.Add(new Tuple<string, byte>(imageFile, gg.Item2));
                }
            }

            Util.Timed("test digest loading", () => {

                int c = 0;
                foreach (var input in inputImages) {
                    if (c % 100 == 0) Console.WriteLine("Processed {0}/{1} images...", c, inputImages.Count);

                    GradeDigest gd =
                        GradeDigest.FromImage(
                            GradeOCR.Program.NormalizeImage(
                                ImageUtil.LoadImage(input.Item1)));
                    gd.grade = input.Item2;
                    gradeDigests.Add(new Tuple<string, GradeDigest>(input.Item1, gd));

                    c++;
                }

            });

            return gradeDigests;
        }
    }
}
