using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableOCR {
    /* line, described by equation 'y = yInt + k * x' */
    public struct RawLine {
        /* Y-intersect: value of y at x = 0 */
        public int yInt;
        public double k;
    }
}
