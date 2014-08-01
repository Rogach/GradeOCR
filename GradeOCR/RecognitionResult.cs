using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GradeOCR {
    public class RecognitionResult {
        public GradeDigest Digest { get; set; }
        public int Confidence { get; set; }

        public RecognitionResult(GradeDigest gd, int confidence) {
            this.Digest = gd;
            this.Confidence = confidence;
        }
    }
}
