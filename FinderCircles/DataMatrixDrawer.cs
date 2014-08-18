using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace ARCode {
    public static class DataMatrixDrawer {
        public static readonly int rowCount = 8;
        public static readonly int columnCount = 32;

        public static Bitmap DataMatrix(bool[] data, int width, int height) {
            if (data.Length != rowCount * columnCount)
                throw new ArgumentException(String.Format(
                    "data array should have length of {0}, got {1} instead.", 
                    rowCount * columnCount, data.Length));

            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        int cx = (int) Math.Floor((float) x * columnCount / width);
                        int cy = (int) Math.Floor((float) y * rowCount / height);
                        *(ptr++) = data[cy * columnCount + cx] ? 0xff000000 : 0xffffffff;
                    }
                }

                res.UnlockBits(bd);
            }

            return res;
        }
    }
}
