using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace GradeOCR {
    public static class EmptyImageDetector {
        public static bool IsImageEmpty(Bitmap image) {
            int imageArea = image.Size.Width * image.Size.Height;
            if (imageArea < 200) {
                return true;
            } else {
                int whiteCount = 0;
                unsafe {
                    BitmapData bd = image.LockBits(ImageLockMode.ReadOnly);
                    uint* ptr = (uint*) bd.Scan0.ToPointer();

                    int imageWidth = image.Width;
                    int imageHeight = image.Height;

                    for (int y = 0; y < imageHeight; y++) {
                        for (int x = 0; x < imageWidth; x++) {
                            if (*(ptr++) == 0xffffffff) whiteCount++;
                        }
                    }

                    image.UnlockBits(bd);
                }

                int blackCount = imageArea - whiteCount;

                return (whiteCount * 100 / imageArea > 97 || blackCount < 40);
            }
        }
    }
}
