using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GradeOCR {

    public class GradeDigestSet {
        private List<GradeDigest> digestList;

        public GradeDigestSet(List<GradeDigest> digests) {
            this.digestList = digests;
        }

        public RecognitionResult FindBestMatch(GradeDigest digest) {
            double maxConfidence = 0;
            GradeDigest bestDigest = digestList[0];
            foreach (var gd in digestList) {
                double match = MatchDigests(digest, gd);
                if (maxConfidence < match) {
                    maxConfidence = match;
                    bestDigest = gd;
                }
            }
            return new RecognitionResult(bestDigest, maxConfidence);
        }

        public double MatchDigests(GradeDigest gd1, GradeDigest gd2) {
            int union = 0;
            int intersection = 0;
            for (int q = 0; q < gd1.data.Length; q++) {
                intersection += ByteUtils.CountBits(gd1.data[q] & gd2.data[q]);
                union += ByteUtils.CountBits(gd1.data[q] | gd2.data[q]);
            }
            if (union == 0) {
                return 0;
            } else {
                return (double) intersection / (double) union;
            }
        }

        public void Save(string fileName) {
            Stream outStream = File.Open(fileName, FileMode.Create);
            ByteUtils.WriteUInt(outStream, (uint) digestList.Count);
            foreach (var digest in digestList) {
                outStream.WriteByte(digest.grade);
                for (int q = 0; q < digest.data.Length; q++) {
                    ByteUtils.WriteULong(outStream, digest.data[q]);
                }
            }
        }

        public static GradeDigestSet Read(string fileName) {
            List<GradeDigest> digests = new List<GradeDigest>();

            Stream inStream = File.OpenRead(fileName);
            uint digestCount = ByteUtils.ReadUInt(inStream);
            for (int q = 0; q < digestCount; q++) {
                GradeDigest gd = new GradeDigest();
                gd.grade = ByteUtils.SafeReadByte(inStream);
                for (int w = 0; w < gd.data.Length; w++) {
                    gd.data[w] = ByteUtils.ReadULong(inStream);
                }
                digests.Add(gd);
            }

            return new GradeDigestSet(digests);
        }

        public static GradeDigestSet ReadDefault() {
            return GradeDigestSet.Read("E:/Pronko/prj/Grader/ocr-data/grade-digests.db");
        }
        
    }
}
