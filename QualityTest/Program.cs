using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GradeOCR;
using LibUtil;
using System.IO;
using OCRUtil;

namespace QualityTest {
    public class Program {
        public static readonly string OcrData = "E:/Pronko/prj/Grader/ocr-data";
        public static readonly string TestOcrData = "E:/Pronko/prj/Grader/ocr-data/test-data";

        static void Main(string[] args) {
            List<GradeDigest> testDigests = GradeFS.LoadDigests(TestOcrData);
            var gradePairs = new List<Tuple<GradeDigest, RecognitionResult>>();
            Util.Timed("compare with database", () => {
                int c = 0;
                foreach (var gd in testDigests) {
                    if (c % 100 == 0) Console.WriteLine("Processed {0}/{1} digests...", c, testDigests.Count);

                    gradePairs.Add(new Tuple<GradeDigest, RecognitionResult>(gd, GradeDigestSet.staticInstance.FindBestMatch(gd)));

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

            Func<byte, int> sureRights = g => gradePairs.Count(gp =>
                    gp.Item1.grade == gp.Item2.Digest.grade &&
                    gp.Item1.grade == g &&
                    MatchConfidence.Sure(gp.Item2.ConfidenceScore));
            Func<byte, int> sureWrongs = g => gradePairs.Count(gp =>
                gp.Item1.grade != gp.Item2.Digest.grade &&
                gp.Item1.grade == g &&
                MatchConfidence.Sure(gp.Item2.ConfidenceScore));
            Func<byte, int> unsure = g => gradePairs.Count(gp =>
                gp.Item1.grade == g &&
                !MatchConfidence.Sure(gp.Item2.ConfidenceScore));

            int sureRightTotal = gradePairs.Count(gp =>
                gp.Item1.grade == gp.Item2.Digest.grade &&
                MatchConfidence.Sure(gp.Item2.ConfidenceScore));
            int sureWrongTotal = gradePairs.Count(gp =>
                gp.Item1.grade != gp.Item2.Digest.grade &&
                MatchConfidence.Sure(gp.Item2.ConfidenceScore));
            int unsureTotal = gradePairs.Count(gp => !MatchConfidence.Sure(gp.Item2.ConfidenceScore));

            Console.WriteLine("Total (r/w/u): {0}/{1}/{2}, {3:F1}/{4:F1}/{5:F1} %",
                sureRightTotal, sureWrongTotal, unsureTotal,
                percentTotal(sureRightTotal), percentTotal(sureWrongTotal), percentTotal(unsureTotal));

            for (byte g = 2; g <= 5; g++) {
                Console.WriteLine("{0}: {1}/{2}/{3}, {4:F1}/{5:F1}/{6:F1} %", g,
                    sureRights(g), sureWrongs(g), unsure(g),
                    percent(sureRights(g), g), percent(sureWrongs(g), g), percent(unsure(g), g));
            }

            Console.WriteLine();

            int minFailureConfidenceScore =
                gradePairs.Where(gp => gp.Item1.grade != gp.Item2.Digest.grade).Select(t => t.Item2.ConfidenceScore).Min();
            Console.WriteLine("min confidence score in recognition failures: {0}", minFailureConfidenceScore);
            Console.WriteLine("successes with score above min failure score: {0}",
                gradePairs.Where(gp => gp.Item1.grade == gp.Item2.Digest.grade).Where(t => t.Item2.ConfidenceScore > minFailureConfidenceScore).Count());

            Console.WriteLine("Recognition failures:");
            gradePairs.Where(gp => gp.Item1.grade != gp.Item2.Digest.grade).ToList().ForEach(gp => {
                Console.WriteLine("file: " + testDigests.Find(gd => gd == gp.Item1).fileName + ", confidence = " + gp.Item2.ConfidenceScore);
            });
        }
    }
}
