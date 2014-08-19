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

    }
}
