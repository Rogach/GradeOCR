﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace ARCode {

    /*
     * Utility class to handle AR-code operations - creating code image for given value
     * and extracting value from image.
     */
    public static class ARCodeUtil {
        
        /* 
         * Create code image, that carries provided value. Unit size is
         * radius of code finder pattern - resulting image size is unit*2 x unit*12.
         */
        public static Bitmap BuildCode(uint value, int unitSize) {
            Bitmap res = new Bitmap(unitSize * 12, unitSize * 2, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(res);

            g.DrawImageUnscaled(FinderCircleDrawer.GetFinderCircleImage(unitSize), new Point(0, 0));
            g.DrawImageUnscaled(DataMatrixDrawer.DataMatrix(DataMarshaller.MarshallInt(value), unitSize * 8, unitSize * 2), new Point(unitSize * 2, 0));
            g.DrawImageUnscaled(FinderCircleDrawer.GetFinderCircleImage(unitSize), new Point(unitSize * 10, 0));

            g.Dispose();

            return res;
        }

        /*
        * Extract code from given image. Requires bounds for expected finder pattern radius.
        */
        public static Option<uint> ExtractCode(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius) {
            return ExtractCodeExt(sourceImage, minPatternRadius, maxPatternRadius).Map(t => t.Item1);
        }

        public static Option<Tuple<uint, DataMatrixExtraction>> ExtractCodeExt(Bitmap sourceImage, int minPatternRadius, int maxPatternRadius) {
            List<Point3> finderCircles = FinderCircleHoughTransform.LocateFinderCircles(sourceImage, minPatternRadius, maxPatternRadius, 2);

            var fpp = new FinderPatternPair();
            fpp.p1 = new Point(finderCircles[0].X, finderCircles[0].Y);
            fpp.size1 = finderCircles[0].Z;
            fpp.p2 = new Point(finderCircles[1].X, finderCircles[1].Y);
            fpp.size2 = finderCircles[1].Z;

            var dme = new DataMatrixExtraction(sourceImage, fpp);

            Option<uint> extractedCode = DataMarshaller.UnMarshallInt(dme.extractedData);
            if (extractedCode.NonEmpty()) {
                return new Some<Tuple<uint, DataMatrixExtraction>>(new Tuple<uint, DataMatrixExtraction>(extractedCode.Get(), dme));
            } else {
                return new None<Tuple<uint, DataMatrixExtraction>>();
            }
        }

    }
}
