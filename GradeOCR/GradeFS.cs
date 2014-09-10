using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibUtil;
using OCRUtil;

namespace GradeOCR {
    public static class GradeFS {
        public static List<Tuple<string, byte>> GetGradeFileLocs(string fsPath) {
            List<Tuple<string, byte>> gradeGroups = new List<Tuple<string, byte>> {
                new Tuple<string, byte>(fsPath + "/grade-2/", 2),
                new Tuple<string, byte>(fsPath + "/grade-3/", 3),
                new Tuple<string, byte>(fsPath + "/grade-4/", 4),
                new Tuple<string, byte>(fsPath + "/grade-5/", 5)
            };

            List<Tuple<string, byte>> inputImages = new List<Tuple<string, byte>>();
            foreach (var gg in gradeGroups) {
                string[] imageFiles = Directory.GetFiles(gg.Item1);
                foreach (var imageFile in imageFiles) {
                    inputImages.Add(new Tuple<string, byte>(imageFile, gg.Item2));
                }
            }

            return inputImages;
        }

        public static List<string> GetGradeFileNames(string fsPath) {
            List<string> images = new List<string>();
            images.AddRange(Directory.GetFiles(fsPath + "/grade-2"));
            images.AddRange(Directory.GetFiles(fsPath + "/grade-3"));
            images.AddRange(Directory.GetFiles(fsPath + "/grade-4"));
            images.AddRange(Directory.GetFiles(fsPath + "/grade-5"));
            return images;
        }

        public static List<GradeDigest> LoadDigests(string fsPath) {
            List<GradeDigest> gradeDigests = new List<GradeDigest>();

            List<Tuple<string, byte>> inputImages = GetGradeFileLocs(fsPath);
            
            List<string> emptyGradeDigestFiles = new List<string>();

            Util.Timed("loading grade digests", () => {
                int c = 0;
                foreach (var input in inputImages) {
                    if (c % 100 == 0) Console.WriteLine("Processed {0}/{1} images...", c, inputImages.Count);

                    Option<GradeDigest> gdOpt = GradeOCR.Program.GetGradeDigest(ImageUtil.LoadImage(input.Item1));
                    gdOpt.ForEach(gd => {
                        gd.grade = input.Item2;
                        gd.fileName = new Some<string>(input.Item1);
                        gradeDigests.Add(gd);
                    });

                    if (gdOpt.IsEmpty()) {
                        emptyGradeDigestFiles.Add(input.Item1);
                    }

                    c++;
                }
            });

            Console.WriteLine("Empty digests: " + emptyGradeDigestFiles.Count);
            foreach (var emptyGD in emptyGradeDigestFiles) {
                Console.WriteLine("empty digest at '{0}'", emptyGD);
            }

            return gradeDigests;
        }

        public static void PackDatabase(string fsPath) {
            List<GradeDigest> gradeDigests = GradeFS.LoadDigests(fsPath);
            GradeDigestSet digestSet = new GradeDigestSet(gradeDigests);
            digestSet.Save(fsPath + "/grade-digests.db");
        }
    }
}
