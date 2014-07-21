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
                if (maxConfidence < match && match < 0.9999) {
                    maxConfidence = match;
                    bestDigest = gd;
                }
            }
            return new RecognitionResult(bestDigest, maxConfidence);
        }

        public double MatchDigests(GradeDigest gd1, GradeDigest gd2) {
            int union = 0;
            int intersection = 0;
            for (int q = 0; q < GradeDigest.digestSize * GradeDigest.digestSize; q++) {
                if (gd1.data[q] && gd2.data[q]) {
                    intersection++;
                    union++;
                } else if (gd1.data[q] || gd2.data[q]) {
                    union++;
                }
            }
            if (union == 0) {
                return 0;
            } else {
                return (double) intersection / (double) union;
            }
        }

        public void Save(string fileName) {
            Stream outStream = File.Open(fileName, FileMode.Create);
            WriteUInt(outStream, (uint) digestList.Count);
            foreach (var digest in digestList) {
                outStream.WriteByte(digest.grade);
                for (int q = 0; q < digest.data.Length; q++) {
                    outStream.WriteByte((byte) (digest.data[q] ? 1 : 0));
                }
            }
        }

        public static GradeDigestSet Read(string fileName) {
            List<GradeDigest> digests = new List<GradeDigest>();

            Stream inStream = File.OpenRead(fileName);
            uint digestCount = ReadUInt(inStream);
            for (int q = 0; q < digestCount; q++) {
                GradeDigest gd = new GradeDigest();
                gd.grade = SafeReadByte(inStream);
                for (int w = 0; w < gd.data.Length; w++) {
                    byte b = SafeReadByte(inStream);
                    gd.data[w] = b == 1;
                }
                digests.Add(gd);
            }

            return new GradeDigestSet(digests);
        }

        public static GradeDigestSet ReadDefault() {
            return GradeDigestSet.Read("E:/Pronko/prj/Grader/ocr-data/grade-digests.db");
        }

        public static void WriteUInt(Stream s, uint i) {
            s.WriteByte((byte) i);
            s.WriteByte((byte) (i >> 8));
            s.WriteByte((byte) (i >> 16));
            s.WriteByte((byte) (i >> 24));
        }

        public static byte SafeReadByte(Stream s) {
            int b = s.ReadByte();
            if (b == -1) {
                throw new Exception("EOF");
            } else {
                return (byte) b;
            }
        }

        public static uint ReadUInt(Stream s) {
            int b1 = SafeReadByte(s);
            int b2 = SafeReadByte(s);
            int b3 = SafeReadByte(s);
            int b4 = SafeReadByte(s);
            return (uint) ((b4 << 24) + (b3 << 16) + (b2 << 8) + b1);
        }
    }
}
