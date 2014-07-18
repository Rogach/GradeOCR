using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace GradeOCR {
    public class LineRecognition {
        public static readonly float maxAngleFactor = 0.03f;
        public static readonly float minHorizontalLineRatio = 0.5f;
        public static readonly float minVerticalLineRatio = 0.1f;

        public static List<Line> RunRecognition(BWImage bw, int minLineLength) {
            List<Line> lines = new List<Line>();

            bool[] blackRows = DetectPossibleBlackRows(bw);

            int maxDy = (int) (bw.Width * maxAngleFactor);

            short[,] inclination = PrecomputeInclination(maxDy, bw.Width);

            int optDy = DetectOptimalDy(bw, inclination, maxDy, blackRows, minLineLength);

            for (int Y = 1; Y < bw.Height - 1; Y++) {
                if (Y + optDy >= 0 && Y + optDy < bw.Height) {

                    LineDetector mavg = new LineDetector(bw.Width, minLineLength);
                    for (int x = 0; x < bw.Width; x++) {
                        short y = (short) (Y + inclination[maxDy + optDy, x]);
                        mavg.Advance(bw.data[y * bw.Width + x] || bw.data[(y + 1) * bw.Width + x] || bw.data[(y - 1) * bw.Width + x]);
                    }
                    mavg.Finish();
                    lines.AddRange(mavg.GetLines(getY: x => Y + inclination[maxDy + optDy, x]));
                }
            }
            lines = RemoveAdjacentLines(lines);

            return lines;
        }

        public static short[,] PrecomputeInclination(int maxDy, int width) {
            short[,] inclination = new short[maxDy * 2 + 1, width];
            for (int dy = -maxDy; dy < 0; dy++) {
                for (int x = 0; x < width; x++) {
                    inclination[maxDy + dy, x] = (short) Math.Round((float) x / (float) width * (float) dy + 0.5);
                }
            }
            for (int x = 0; x < width; x++) {
                inclination[maxDy, x] = 0;
            }
            for (int dy = 1; dy <= maxDy; dy++) {
                for (int x = 0; x < width; x++) {
                    inclination[maxDy + dy, x] = (short) Math.Round((float) x / (float) width * (float) dy - 0.5);
                }
            }
            return inclination;
        }

        public static int DetectOptimalDy(BWImage bw, short[,] inclination, int maxDy, bool[] blackRows, int minLineLength) {
            for (int Y = 0; Y < bw.Height; Y++) {
                if (blackRows[Y]) {
                    int minY = Math.Max(0, Y - maxDy);
                    int maxY = Math.Min(bw.Height - 1, Y + maxDy);

                    for (int y2 = minY; y2 <= maxY; y2++) {
                        LineDetector mavg = new LineDetector(bw.Width, minLineLength);
                        int dy = y2 - Y;

                        for (int x = 0; x < bw.Width; x++) {
                            short y = (short) (Y + inclination[maxDy + dy, x]);
                            bool b = bw.data[y * bw.Width + x];
                            mavg.Advance(b);
                        }
                        mavg.Finish();

                        List<Line> detectedLines = mavg.GetLines(getY: x => Y + inclination[maxDy + dy, x]);

                        if (detectedLines.Count > 0) {
                            List<Line> optLines = new List<Line>();
                            for (int oY = Math.Max(0, Y - 40); oY < bw.Height && oY < Y + 40; oY++) {
                                int oMinY = Math.Max(0, oY - maxDy);
                                int oMaxY = Math.Min(bw.Height - 1, oY + maxDy);
                                for (int oy2 = oMinY; oy2 <= oMaxY; oy2++) {
                                    LineDetector omavg = new LineDetector(bw.Width, minLineLength);
                                    int ody = oy2 - oY;
                                    for (int x = 0; x < bw.Width; x++) {
                                        short oy = (short) (oY + inclination[maxDy + ody, x]);
                                        bool b = bw.data[oy * bw.Width + x];
                                        omavg.Advance(b);
                                    }
                                    omavg.Finish();
                                    optLines.AddRange(omavg.GetLines(getY: x => Y + inclination[maxDy + ody, x]));
                                }
                            }

                            double maxLength = optLines.MaxBy(ln => ln.Length()).Length();
                            optLines = optLines.Where(ln => ln.Length() > maxLength * 0.9).ToList();

                            int optimalOffset = (int) Math.Round(bw.Width * optLines.Select(ln => ln.Tangent()).Average());
                            return optimalOffset;
                        }
                    }
                }
            }
            throw new Exception("no optimal dy detected");
        }

        public static int[] TallyBlackPixels(BWImage img) {
            int spillOverFactor = (int) (img.Width * maxAngleFactor / 4);
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
            ImageUtil.AssertImageFormat(src);
            Bitmap res = new Bitmap(src.Width + 200, src.Height, PixelFormat.Format8bppIndexed);
            res.Palette = src.Palette;

            int[] blackCount = TallyBlackPixels(bw);
            bool[] blackRows = DetectPossibleBlackRows(bw);

            unsafe {
                BitmapData srcBD = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
                BitmapData resBD = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

                byte* srcPtr = (byte*) srcBD.Scan0.ToPointer();
                byte* resPtr = (byte*) resBD.Scan0.ToPointer();

                for (int y = 0; y < src.Height; y++) {
                    int c = 0;
                    for (int x = 0; x < src.Width; x++) {
                        if (*(srcPtr) == 0) c++;
                        *(resPtr++) = *(srcPtr++);
                    }
                    resPtr += 200;
                }

                int maxCount = (int) (blackCount.Max() * 1.1);
                resPtr = (byte*) resBD.Scan0.ToPointer();
                for (int y = 0; y < src.Height; y++) {
                    int f = blackCount[y] * 200 / maxCount;
                    if (!blackRows[y]) {
                        for (int x = 0; x < f; x++) {
                            *(resPtr + y * (src.Width + 200) + src.Width + x) = 150;
                        }
                    }
                    for (int x = f; x < 200; x++) {
                        *(resPtr + y * (src.Width + 200) + src.Width + x) = 255;
                    }
                }

                res.UnlockBits(resBD);
                src.UnlockBits(srcBD);
            }

            return res;
        }

        public static List<Line> RemoveAdjacentLines(List<Line> lines) {
            List<Line> filteredLines = new List<Line>();

            lines = lines.OrderBy(ln => ln.Y_atZero()).ToList();

            bool[] used = new bool[lines.Count];
            for (int q = 0; q < lines.Count; q++) {
                if (!used[q]) {
                    List<Line> adjacentLines = new List<Line>();
                    adjacentLines.Add(lines[q]);
                    used[q] = true;

                    int y = lines[q].Y_atZero();
                    int w = q + 1;
                    while (w < lines.Count && lines[w].Y_atZero() - 10 < y) {
                        used[w] = true;
                        adjacentLines.Add(lines[w]);
                        w++;
                    }

                    filteredLines.Add(adjacentLines.MaxBy(ln => ln.Length()));
                }
            }

            return filteredLines;
        }
    }
}
