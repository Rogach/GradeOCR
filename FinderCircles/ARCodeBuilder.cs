using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace ARCode {
    public static class ARCodeBuilder {
        public static Bitmap BuildCode(bool[] data, int unitSize) {
            Bitmap res = new Bitmap(unitSize * 12, unitSize * 2, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(res);

            g.DrawImageUnscaled(CircleDrawer.GetFinderCircleImage(unitSize), new Point(0, 0));
            g.DrawImageUnscaled(DataMatrixDrawer.DataMatrix(data, unitSize * 8, unitSize * 2), new Point(unitSize * 2, 0));
            g.DrawImageUnscaled(CircleDrawer.GetFinderCircleImage(unitSize), new Point(unitSize * 10, 0));

            g.Dispose();

            return res;
        }
    }
}
