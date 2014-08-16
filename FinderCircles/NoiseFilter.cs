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
        Bitmap Apply(Bitmap src);
    }

    public class FilterSeq : NoiseFilter {
        private NoiseFilter[] filters;
        public FilterSeq(params NoiseFilter[] filters) {
            this.filters = filters;
        }

        public Bitmap Apply(Bitmap src) {
            Bitmap res = src;
            foreach (var filter in filters) {
                res = filter.Apply(res);
            }
            return res;
        }
    }

    public class RandomNoise : NoiseFilter {
        private double intensity;
        public RandomNoise(double intensity) {
            this.intensity = intensity;
        }
        public Bitmap Apply(Bitmap src) {
            Random r = new Random();
            Bitmap res = new Bitmap(src);
            unsafe {
                BitmapData bd = res.LockBits(ImageLockMode.WriteOnly);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

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

    public class RandomBlots : NoiseFilter {
        private double intensity;
        public RandomBlots(double intensity) {
            this.intensity = intensity;
        }
        public Bitmap Apply(Bitmap src) {
            Random r = new Random();
            Bitmap res = new Bitmap(src);
            int blotCount = (int) (src.Width * src.Height / 10 * intensity);
            Graphics g = Graphics.FromImage(res);
            for (int q = 0; q < blotCount; q++) {
                int blotSize = r.Next(1, 10);
                int blotColor = r.Next(255);
                int x = r.Next(src.Width);
                int y = r.Next(src.Height);
                Color c = Color.FromArgb(blotColor, blotColor, blotColor);
                g.FillEllipse(new SolidBrush(c), new Rectangle(x, y, blotSize, blotSize));
            }
            g.Dispose();

            return res;
        }
    }

    public class RandomLines : NoiseFilter {
        private double intensity;
        public RandomLines(double intensity) {
            this.intensity = intensity;
        }
        public Bitmap Apply(Bitmap src) {
            Random r = new Random();
            Bitmap res = new Bitmap(src);

            int lineCount = (int) (src.Width * src.Height / 100 * intensity);
            Graphics g = Graphics.FromImage(res);
            for (int q = 0; q < lineCount; q++) {
                float angle = (float) ((r.NextDouble() * Math.PI - Math.PI / 2) * 0.9);
                float k = (float) Math.Tan(angle);
                
                float dy = k * src.Width / 2;

                int x = r.Next(src.Width);
                int y = r.Next(src.Height);

                PointF p1 = new PointF(x - src.Width, y - dy);
                PointF p2 = new PointF(x + src.Width, y + dy);

                g.DrawLine(new Pen(Color.FromArgb(125, Color.Black), 2), p1, p2);
            }
            g.Dispose();

            return res;
        }
    }

    public class RandomStripes : NoiseFilter {
        private double intensity;
        private int stripeWidth;
        public RandomStripes(double intensity, int stripeWidth) {
            this.intensity = intensity;
            this.stripeWidth = stripeWidth;
        }
        public Bitmap Apply(Bitmap src) {
            Random r = new Random();
            Bitmap res = new Bitmap(src);

            int lineCount = (int) (src.Width * intensity / stripeWidth * 2);

            Graphics g = Graphics.FromImage(res);

            for (int q = 0; q < lineCount; q++) {
                int x = r.Next(src.Width);

                g.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.Black)),
                    new Rectangle(x, 0, stripeWidth, src.Height));
            }

            g.Dispose();

            return res;
        }
    }

}
