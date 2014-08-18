using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ARCode {
    public class FinderPatternPair {
        public Point p1 { get; set; }
        public int size1 { get; set; }
        public Point p2 { get; set; }
        public int size2 { get; set; }
    }

    public static class FinderPatternRecognition {
        public static FinderPatternPair LocateFinderPatternPair(Bitmap sourceImage, int patternRadius) {
            int minPatternRadius = (int) Math.Floor((double) patternRadius * 0.9);
            int maxPatternRadius = (int) Math.Ceiling((double) patternRadius * 1.1);
            List<Point3> finderPatterns = CircleHoughTransform.LocateFinderCircles(
                sourceImage, minPatternRadius, maxPatternRadius, 2);
            FinderPatternPair fp = new FinderPatternPair();
            fp.p1 = new Point(finderPatterns[0].X, finderPatterns[0].Y);
            fp.size1 = finderPatterns[0].Z;
            fp.p2 = new Point(finderPatterns[1].X, finderPatterns[1].Y);
            fp.size2 = finderPatterns[1].Z;
            return fp;
        }
    }
}
