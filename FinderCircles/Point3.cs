using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCode {

    public class Point3 {
        public int X;
        public int Y;
        public int Z;

        public Point3(int X, int Y, int Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public override string ToString() {
            return String.Format("[X={0},Y={1},Z={2}]", X, Y, Z);
        }
    }

}
