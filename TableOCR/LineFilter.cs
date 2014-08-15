using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OCRUtil;
using LibUtil;
using System.Drawing;
using System.Drawing.Imaging;

namespace TableOCR {

    public static class LineFilter {
        
        /*
         * Forms solid lines from set of raw lines.
         * Discards lines that have cyclic patterns in them.
         */
        public static List<Line> FilterLines(List<Point> edgePoints, List<RawLine> rawLines, RecognitionOptions options) {
            List<Line> lines = new List<Line>();

            foreach (var rawLine in rawLines) {
                
                // convert rawLine to list of black/white pixels
                bool[] linePoints = new bool[options.imageWidth];
                foreach (var pt in edgePoints) {
                    if (Math.Abs(rawLine.yInt - (pt.Y - pt.X * rawLine.k)) < 2) {
                        linePoints[pt.X] = true;
                    }
                }

                var segments = SegmentDetector.GetSegments(linePoints, rawLine);

                if (segments.Count > 0) {
                    Line solidLine = SegmentDetector.GetSolidLine(segments);

                    if (!options.detectCyclicPatterns ||
                        !CyclicPatternDetector.HasCyclicPatterns(linePoints, solidLine.p1.X, solidLine.p2.X, options)) {
                        lines.Add(solidLine);
                    }
                }
            }

            return lines;
        }

    }
}
