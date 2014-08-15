using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableOCR {
    public struct RecognitionParams {
        public float maxAngleFactor;
        public int houghThreshold;
        public int houghWindowWidth;
        public int houghWindowHeight;
        public int width;
        public int height;
        public int minLineLength;
        public bool detectCyclicPatterns;
        public int cyclicPatternsMinWidth;
        public int cyclicPatternsMaxWidth;
    }
}
