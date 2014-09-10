using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GradeOCR {
    public class RecognitionResult {
        public int Grade { get; set; }
        public bool Confident { get; set; }

        public RecognitionResult(int grade, bool confident) {
            this.Grade = grade;
            this.Confident = confident;
        }
    }
}
