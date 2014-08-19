using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ARCode {
    public static class ARCodeExtractor {

        /*
         * Extract code from given image. Requires bounds for expected finder pattern radius.
         */
        public static uint ExtractCode(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius) {
            List<Point3> finderCircles = CircleHoughTransform.LocateFinderCircles(sourceImage, minPatternRadius, maxPatternRadius, 2);
            
            var fpp = new FinderPatternPair();
            fpp.p1 = new Point(finderCircles[0].X, finderCircles[0].Y);
            fpp.size1 = finderCircles[0].Z;
            fpp.p2 = new Point(finderCircles[1].X, finderCircles[1].Y);
            fpp.size2 = finderCircles[1].Z;
            
            var dme = new DataMatrixExtraction(sourceImage, fpp);
            
            return DataMarshaller.UnMarshallInt(dme.extractedData);
        }
    }
}
