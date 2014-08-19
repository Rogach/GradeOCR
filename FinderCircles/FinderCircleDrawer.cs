using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace ARCode {
    public static class FinderCircleDrawer {
        public static Bitmap GetFinderCircleImage(int radius) {
            Bitmap img = new Bitmap(radius * 2 + 1, radius * 2 + 1, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData bd = img.LockBits(ImageLockMode.WriteOnly);
                uint* ptr = (uint*) bd.Scan0.ToPointer();

                PointF center = new PointF(radius, radius);
                
                for (int y = 0; y < img.Height; y++) {
                    for (int x = 0; x < img.Width; x++) {
                        PointF p = new PointF(x, y);
                        float r = (float) PointOps.Distance(p, center) / radius;

                        int px = GetPixelAtRadius(r);
                        if (px == 1) {
                            *ptr = 0xff000000;
                        } else if (px == -1) {
                            *ptr = 0xffffffff;
                        }

                        ptr++;
                    }
                }

                img.UnlockBits(bd);
            }

            return img;
        }

        /* Returns 1 if pixel is black, 0 if transparent, -1 if pixel is white */
        public static int GetPixelAtRadius(float r) {
            if (r < 3f / 9) {
                return 1;
            } else if (r < 5f / 9) {
                return -1;
            } else if (r < 7f / 9) {
                return 1;
            } else if (r < 1) {
                return -1;
            } else {
                return 0;
            }
        }
    }
}
