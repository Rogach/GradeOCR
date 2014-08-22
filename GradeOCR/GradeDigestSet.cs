using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace GradeOCR {

    public class GradeDigestSet {
        public static readonly GradeDigestSet staticInstance = ReadDefault();

        private List<GradeDigest> digestList;

        public List<GradeDigest> GetDigestList() {
            return digestList;
        }

        public GradeDigestSet(List<GradeDigest> digests) {
            this.digestList = digests;
        }

        public RecognitionResult FindBestMatch(GradeDigest digest) {
            double maxMatch = 0;
            int bestIndex = 0;
            GradeDigest bestDigest = digestList[0];
            for (int q = 0; q < digestList.Count; q++) {
                var gd = digestList[q];
                double match = MatchDigests(digest, gd);
                if (maxMatch < match && match < 0.9999) {
                    maxMatch = match;
                    bestDigest = gd;
                    bestIndex = q;
                }
            }
            return new RecognitionResult(
                bestDigest,
                MatchConfidence.GetConfidenceScoreSymmetric(digest, bestDigest),
                bestIndex);
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
            using (Stream inStream = File.OpenRead(fileName)) {
                return Read(inStream);
            }
        }

        public static GradeDigestSet Read(Stream inStream) {
            List<GradeDigest> digests = new List<GradeDigest>();

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

        private static GradeDigestSet ReadDefault() {
            using (Stream inStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GradeOCR.grade-digests.db")) {
                return Read(inStream);
            }
        }
        
    }
}
