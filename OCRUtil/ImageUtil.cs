using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OCRUtil {
    public static class ImageUtil {
        public static Bitmap LoadImage(string fileName) {
            FileStream fs = File.OpenRead(fileName);
            Bitmap img = (Bitmap) Image.FromStream(fs);
            fs.Close();
            return ImageUtil.ToStdFormat(img);
        }

        public static Bitmap ToStdFormat(Bitmap b) {
            Bitmap res = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
            res.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            Graphics g = Graphics.FromImage(res);
            g.DrawImageUnscaled(b, 0, 0);
            g.Dispose();
            return res;
        }

        public static Bitmap ToBlackAndWhite(Bitmap src) {
            Bitmap res = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData srcBD = src.LockBits(ImageLockMode.ReadOnly);
                BitmapData resBD = res.LockBits(ImageLockMode.WriteOnly);

                byte* srcPtr = (byte*) srcBD.Scan0.ToPointer();
                byte* resPtr = (byte*) resBD.Scan0.ToPointer();

                for (int q = 0; q < srcBD.Width * srcBD.Height; q++) {
                    int gray = (*srcPtr * 30 + *(srcPtr + 1) * 59 + *(srcPtr + 2) * 11) / 100;
                    if (gray < 220) {
                        *resPtr = 0;
                        *(resPtr + 1) = 0;
                        *(resPtr + 2) = 0;
                    } else {
                        *resPtr = 255;
                        *(resPtr + 1) = 255;
                        *(resPtr + 2) = 255;
                    }
                    *(resPtr + 3) = 255;
                    srcPtr += 4;
                    resPtr += 4;
                }

                src.UnlockBits(srcBD);
                res.UnlockBits(resBD);
            }

            return res;
        }

        public static Bitmap ToGrayscale(Bitmap src) {
            Bitmap res = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);

            unsafe {
                BitmapData srcBD = src.LockBits(ImageLockMode.ReadOnly);
                BitmapData resBD = res.LockBits(ImageLockMode.WriteOnly);

                byte* srcPtr = (byte*) srcBD.Scan0.ToPointer();
                byte* resPtr = (byte*) resBD.Scan0.ToPointer();

                for (int q = 0; q < srcBD.Width * srcBD.Height; q++) {
                    byte gray = (byte) ((*srcPtr * 30 + *(srcPtr + 1) * 59 + *(srcPtr + 2) * 11) / 100);

                    *resPtr = gray;
                    *(resPtr + 1) = gray;
                    *(resPtr + 2) = gray;
                    *(resPtr + 3) = 255;

                    srcPtr += 4;
                    resPtr += 4;
                }

                src.UnlockBits(srcBD);
                res.UnlockBits(resBD);
            }

            return res;
        }

        public static Bitmap Rotate(Bitmap src) {
            Bitmap rotated = new Bitmap(src.Height, src.Width, PixelFormat.Format32bppArgb);
            rotated.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            
            Graphics g = Graphics.FromImage(rotated);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            
            g.TranslateTransform(0, rotated.Height);
            g.RotateTransform(-90);

            g.DrawImageUnscaled(src, 0, 0);
            g.Dispose();

            return rotated;
        }
        
    }
}
