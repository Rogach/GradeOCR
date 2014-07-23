using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace OCRUtil {
    public static class MonkeyPatches {
        public static BitmapData LockBits(this Bitmap b, ImageLockMode lockMode) {
            return b.LockBits(new Rectangle(0, 0, b.Width, b.Height), lockMode, PixelFormat.Format32bppArgb);
        }
    }
}
