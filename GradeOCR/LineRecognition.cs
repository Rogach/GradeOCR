﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace GradeOCR {
    public class LineRecognition {
        public static readonly float maxAngleFactor = 0.03f;
        public static readonly int minSegmentLength = 200;
        public static readonly int maxSkipLength = 200;

        public static List<Tuple<Point, Point>> RunRecognition(BWImage bw) {
            List<Tuple<Point, Point>> lines = new List<Tuple<Point, Point>>();
            Util.Timed("sweepline segment detection", () => {
                bool[] blackRows = DetectPossibleBlackRows(bw);

                int maxDy = (int) (bw.Width * maxAngleFactor);

                // precompute inclination table
                short[,] inclination = new short[maxDy * 2 + 1, bw.Width];
                for (int dy = -maxDy; dy < 0; dy++) {
                    for (int x = 0; x < bw.Width; x++) {
                        inclination[maxDy + dy, x] = (short) Math.Round((float) x / (float) bw.Width * (float) dy + 0.5);
                    }
                }
                for (int x = 0; x < bw.Width; x++) {
                    inclination[maxDy, x] = 0;
                }
                for (int dy = 1; dy <= maxDy; dy++) {
                    for (int x = 0; x < bw.Width; x++) {
                        inclination[maxDy + dy, x] = (short) Math.Round((float) x / (float) bw.Width * (float) dy - 0.5);
                    }
                }

                for (int Y = 0; Y < bw.Height; Y++) {
                    if (Y % 100 == 0) {
                        Console.WriteLine("sweepline y = " + Y);
                    }
                    if (blackRows[Y]) {
                        int minY = Math.Max(0, Y - maxDy);
                        int maxY = Math.Min(bw.Height - 1, Y + maxDy);

                        for (int y2 = minY; y2 <= maxY; y2++) {
                            Point? rStt = null;
                            Point? rEnd = null;
                            Point? stt = null;
                            Point? end = null;

                            int dy = y2 - Y;

                            for (int x = 0; x < bw.Width; x++) {
                                short y = (short) (Y + inclination[maxDy + dy, x]);
                                bool b = bw.data[y * bw.Width + x];
                                if (b) {
                                    if (!stt.HasValue) {
                                        stt = new Point(x, y);
                                    }
                                    end = new Point(x, y);
                                } else {
                                    if (end.HasValue && end.Value.X + 1 == x) {
                                        if (end.Value.X - stt.Value.X >= minSegmentLength) {
                                            if (rStt.HasValue) {
                                                if (rEnd.Value.X + maxSkipLength > stt.Value.X) {
                                                    rEnd = end;
                                                } else {
                                                    lines.Add(new Tuple<Point, Point>(rStt.Value, rEnd.Value));
                                                    rStt = stt;
                                                    rEnd = end;
                                                }
                                            } else {
                                                rStt = stt;
                                                rEnd = end;
                                            }
                                        }
                                        stt = null;
                                        end = null;
                                    }
                                }
                            }
                            if (rStt.HasValue && rEnd.HasValue) {
                                lines.Add(new Tuple<Point, Point>(rStt.Value, rEnd.Value));
                            }
                        }
                    }
                }
                Console.WriteLine("lines found: " + lines.Count);
            });

            return lines;
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
            bool[] blackRows = new bool[img.Height];
            for (int y = 0; y < img.Height; y++) {
                if (blackCount[y] > img.Width) blackRows[y] = true;
            }
            return blackRows;
        }


        public static Bitmap DisplayWhiteRows(Bitmap src, BWImage bw) {
            ImageUtil.AssertImageFormat(src);
            Bitmap res = new Bitmap(src.Width + 200, src.Height, PixelFormat.Format8bppIndexed);
            res.Palette = src.Palette;

            int[] blackCount = Util.Timed("TallyBlackPixels", () => { return TallyBlackPixels(bw); });
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
                    for (int x = 0; x < 200; x++) {
                        if ((src.Width) * 200 / maxCount == x) {
                            *(resPtr + y * (src.Width + 200) + src.Width + x) = 150;
                        }
                    }
                }

                res.UnlockBits(resBD);
                src.UnlockBits(srcBD);
            }

            return res;
        }
    }
}
