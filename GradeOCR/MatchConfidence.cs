using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GradeOCR {
    public static class MatchConfidence {
        public static readonly int cutoffConfidenceScore = 250;

        public static int GetConfidenceScore(GradeDigest gd1, GradeDigest gd2) {
            int?[] distance = new int?[GradeDigest.dataSize];
            Queue<Tuple<Point, int>> floodQueue = new Queue<Tuple<Point, int>>();
            bool[] bits2 = GradeDigest.UnpackBits(gd2.data);
            for (int y = 0; y < GradeDigest.digestSize; y++) {
                for (int x = 0; x < GradeDigest.digestSize; x++) {
                    if (bits2[y * GradeDigest.digestSize + x])
                        floodQueue.Enqueue(new Tuple<Point, int>(new Point(x, y), 0));
                }
            }

            // run flooding to determine all pixels' distance

            while (floodQueue.Count > 0) {
                var t = floodQueue.Dequeue();
                Point p = t.Item1;
                int q = p.Y * GradeDigest.digestSize + p.X;
                int dist = t.Item2;
                if (!distance[q].HasValue) {
                    distance[q] = dist;
                    if (p.X > 0)
                        floodQueue.Enqueue(new Tuple<Point, int>(new Point(p.X - 1, p.Y), dist + 1));
                    if (p.X < GradeDigest.digestSize - 1)
                        floodQueue.Enqueue(new Tuple<Point, int>(new Point(p.X + 1, p.Y), dist + 1));
                    if (p.Y > 0)
                        floodQueue.Enqueue(new Tuple<Point, int>(new Point(p.X, p.Y - 1), dist + 1));
                    if (p.Y < GradeDigest.digestSize - 1)
                        floodQueue.Enqueue(new Tuple<Point, int>(new Point(p.X, p.Y + 1), dist + 1));
                }
            }

            // run over input digest to get distances
            int distanceSum = 0;
            bool[] bits1 = GradeDigest.UnpackBits(gd1.data);
            for (int q = 0; q < bits1.Length; q++) {
                if (bits1[q]) distanceSum += distance[q].Value * distance[q].Value;
            }

            return distanceSum;
        }

        public static bool Sure(int confidenceScore) {
            return confidenceScore < cutoffConfidenceScore;
        }

    }
}
