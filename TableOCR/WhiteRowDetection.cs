using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace TableOCR {
    public static class WhiteRowDetection {
        public static int[] TallyBlackPixels(BWImage img) {
            int spillOverFactor = (int) (img.Width * LineRecognition.maxAngleFactor / 4);
            int[] blackCount = new int[img.Height];

            for (int y = 0; y < img.Height; y++) {
                int c = 0;

                for (int x = 0; x < img.Width; x++) {
                    if (img.data[y * img.Width + x]) c++;
                }

                for (int fy = Math.Max(0, y - spillOverFactor); fy <= Math.Min(img.Height - 1, y + spillOverFactor); fy++) {
                    blackCount[fy] += c;
                }
            }

            return blackCount;
        }

        public static bool[] DetectPossibleBlackRows(BWImage img) {
            int[] blackCount = TallyBlackPixels(img);
            int max = blackCount.Max();
            bool[] blackRows = new bool[img.Height];
            for (int y = 0; y < img.Height; y++) {
                if (blackCount[y] > max / 5) blackRows[y] = true;
            }
            return blackRows;
        }


        public static Bitmap DisplayWhiteRows(Bitmap src, BWImage bw) {
            Bitmap res = new Bitmap(src.Width + 200, src.Height, PixelFormat.Format32bppArgb);
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            int[] blackCount = TallyBlackPixels(bw);
            bool[] blackRows = DetectPossibleBlackRows(bw);

            Graphics g = Graphics.FromImage(res);
            g.DrawImageUnscaled(src, 0, 0);
            g.Dispose();

            int maxCount = (int) (blackCount.Max() * 1.1);

            unsafe {
                BitmapData resBD = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                uint* resPtr = (uint*) resBD.Scan0.ToPointer();

                for (int y = 0; y < src.Height; y++) {
                    int f = blackCount[y] * 200 / maxCount;
                    if (!blackRows[y]) {
                        for (int x = 0; x < f; x++) {
                            *(resPtr + y * (src.Width + 200) + src.Width + x) = 4288059030; // 150 gray
                        }
                    } else {
                        for (int x = 0; x < f; x++) {
                            *(resPtr + y * (src.Width + 200) + src.Width + x) = 4278190080; // black
                        }
                    }
                    for (int x = f; x < 200; x++) {
                        *(resPtr + y * (src.Width + 200) + src.Width + x) = 4294967295; // white
                    }
                }

                res.UnlockBits(resBD);
            }

            return res;
        }
    }
}
