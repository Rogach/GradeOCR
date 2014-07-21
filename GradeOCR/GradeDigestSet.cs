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
            byte b1 = SafeReadByte(s);
            byte b2 = SafeReadByte(s);
            byte b3 = SafeReadByte(s);
            byte b4 = SafeReadByte(s);
            return (UInt32) (b4 << 24 + b3 << 16 + b2 << 8 + b1);
        }
    }
}
