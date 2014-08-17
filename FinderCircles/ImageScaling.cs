using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace FinderCircles {
    public static class ImageScaling {
        public static Bitmap ScaleDown(Bitmap src, int factor) {
            Bitmap res = new Bitmap(src.Width / factor, src.Height / factor, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData srcBD = src.LockBits(ImageLockMode.ReadOnly);
                BitmapData resBD = res.LockBits(ImageLockMode.ReadWrite);
                byte* srcPtr = (byte*) srcBD.Scan0.ToPointer();
                byte* resPtr = (byte*) resBD.Scan0.ToPointer();

                int resWidth = res.Width;
                int resHeight = res.Height;

                for (int y = 0; y < resHeight; y++) {
                    for (int x = 0; x < resWidth; x++) {
                        int sum = 0;
                        for (int dy = 0; dy < factor; dy++) {
                            for (int dx = 0; dx < factor; dx++) {
                                sum += *(srcPtr + 4 * ((y * factor + dy) * src.Width + x * factor + dx));
                            }
                        }
                        byte v = (byte) (sum / (factor * factor));
                        byte* outPtr = resPtr + 4 * (y * resWidth + x);
                        *outPtr = v;
                        *(outPtr + 1) = v;
                        *(outPtr + 2) = v;
                        *(outPtr + 3) = 255;
                    }
                }

                res.UnlockBits(resBD);
                src.UnlockBits(srcBD);
            }

            return res;
        }
    }
}
