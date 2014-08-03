using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace GradeImageMove {
    class Program {
        public static readonly string source = "E:/Pronko/prj/Grader/ocr-data/test-data";
        public static readonly string destination = "E:/Pronko/prj/Grader/ocr-data";

        static void Main(string[] args) {
            int idx = GetNextGradeIndex();
            foreach (string gdir in new List<string> { "grade-unsort", "grade-0", "grade-2", "grade-3", "grade-4", "grade-5" }) {
                foreach (string srcFile in Directory.GetFiles(source + "/" + gdir)) {
                    string destFile = destination + "/" + gdir + "/g" + (idx++).ToString().PadLeft(5, '0') + ".png";
                    File.Move(srcFile, destFile);
                }
            }
        }

        private static int GetNextGradeIndex() {
            List<string> images = new List<string>();
            images.AddRange(Directory.GetFiles(destination + "/grade-unsort"));
            images.AddRange(Directory.GetFiles(destination + "/grade-2"));
            images.AddRange(Directory.GetFiles(destination + "/grade-3"));
            images.AddRange(Directory.GetFiles(destination + "/grade-4"));
            images.AddRange(Directory.GetFiles(destination + "/grade-5"));
            images.AddRange(Directory.GetFiles(destination + "/grade-0"));
            if (images.Count == 0) {
                return 1;
            } else {
                Regex rgx = new Regex(@"g(\d{5}).png");
                int nextN = images.Select(img => {
                    var m = rgx.Match(Path.GetFileName(img));
                    if (m.Success) {
                        return int.Parse(m.Groups[1].Value);
                    } else {
                        return 0;
                    }
                }).Max();
                return nextN + 1;
            }
        }
    }
}
