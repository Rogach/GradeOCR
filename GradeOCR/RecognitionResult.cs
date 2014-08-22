using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GradeOCR {
    public class RecognitionResult {
        public GradeDigest Digest { get; set; }
        public int ConfidenceScore { get; set; }
        public int MatchIndex { get; set; }

        public RecognitionResult(GradeDigest gd, int confidenceScore, int matchIndex) {
            this.Digest = gd;
            this.ConfidenceScore = confidenceScore;
            this.MatchIndex = matchIndex;
        }
    }
}
