using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OCRUtil;
using LibUtil;

namespace FinderCircles {
    public interface NoiseFilter {
        Bitmap Apply(Bitmap src, double intensity);
    }

    public class FilterSeq : NoiseFilter {
        private NoiseFilter[] filters;
        public FilterSeq(params NoiseFilter[] filters) {
            this.filters = filters;
        }

        public Bitmap Apply(Bitmap src, double intensity) {
            Bitmap res = src;
            foreach (var filter in filters) {
                res = filter.Apply(res, intensity);
            }
            return res;
        }
    }

    public class RandomNoise : NoiseFilter {
        public Bitmap Apply(Bitmap src, double intensity) {
            Bitmap res = new Bitmap(src);
            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                Random r = new Random();
                for (int q = 0; q < src.Width * src.Height; q++) {
                    if (r.NextDouble() < intensity) {
                        byte b = (byte) r.Next(255);
                        *ptr = b;
                        *(ptr + 1) = b;
                        *(ptr + 2) = b;
                        *(ptr + 3) = 255;
                    }
                    ptr += 4;
                }

                res.UnlockBits(bd);
            }
            return res;
        }
    }

    public class Rotation : NoiseFilter {
        public Bitmap Apply(Bitmap src, double intensity) {
            Random r = new Random();
            float angle = (float) (r.NextDouble() * intensity);
            return ImageUtil.Rotate(src, angle);
        }
    }
}
