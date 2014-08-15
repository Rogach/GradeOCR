using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableOCR {
    /* Class to carry various tweakable recognition options */
    public struct RecognitionOptions {
        /* max factor of line Y difference to X difference */
        public float maxAngleFactor;

        /* threshold for peak selection in Pseudo-Hough transform */
        public double houghThreshold;
        /* width of window for peak selection in Pseudo-Hough transform */
        public int houghWindowWidth;
        /* height of window for peak selection in Pseudo-Hough transform */
        public int houghWindowHeight;

        /* width of image being processed */
        public int imageWidth;
        /* height of image being processed */
        public int imageHeight;

        /* controls application of cyclic pattern detection */
        public bool detectCyclicPatterns;
        /* min width of cyclic pattern */
        public int cyclicPatternsMinWidth;
        /* max width of cyclic pattern */
        public int cyclicPatternsMaxWidth;

        public static RecognitionOptions CommonOptions() {
            var options = new RecognitionOptions();
            options.maxAngleFactor = 0.03f;
            options.houghWindowWidth = 20;
            options.houghWindowHeight = 10;
            return options;
        }

        public static RecognitionOptions HorizontalOptions() {
            var options = CommonOptions();
            options.houghThreshold = 0.2;
            options.detectCyclicPatterns = false;
            return options;
        }

        public static RecognitionOptions VerticalOptions() {
            var options = CommonOptions();
            options.houghThreshold = 0.4;
            options.detectCyclicPatterns = true;
            options.cyclicPatternsMinWidth = 10;
            options.cyclicPatternsMaxWidth = 100;
            return options;
        }
    }
}
