using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using LibUtil;

namespace GradeOCR {
    public class LineRecognition {
        public static readonly float maxAngleFactor = 0.05f;
        public static readonly int minSegmentLength = 200;
        public static readonly int maxSkipLength = 200;

        public static List<Tuple<Point, Point>> RunRecognition(Bitmap img) {
            List<Tuple<Point, Point>> lines = new List<Tuple<Point, Point>>();
            BWImage bw = new BWImage(img);
            Util.Timed("sweepline segment detection", () => {
                int maxDy = (int) (bw.Width * maxAngleFactor);

                for (int y = 0; y < bw.Height; y++) {
                    if (y % 100 == 0) {
                        Console.WriteLine("sweepline y = " + y);
                    }
                    int minY = Math.Max(0, y - maxDy);
                    int maxY = Math.Min(bw.Height - 1, y + maxDy);
                    for (int y2 = minY; y2 <= maxY; y2++) {
                        Point? rStt = null;
                        Point? rEnd = null;
                        Point? stt = null;
                        Point? end = null;

                        Action<int, int, bool> onPixel = (px, py, b) => {
                            if (b) {
                                if (!stt.HasValue) {
                                    stt = new Point(px, py);
                                }
                                end = new Point(px, py);
                            } else {
                                if (end.HasValue && end.Value.X + 1 == px) {
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
                        };

                        int dy = y2 - y;
                        if (dy == 0) {
                            for (int x = 0; x < bw.Width; x++) {
                                bool b = bw.Pixel(x, y);
                                onPixel(x, y, b);
                            }
                        } else if (dy > 0) {
                            for (int x = 0; x < bw.Width; x++) {
                                int cy = (int) Math.Round((float) x / (float) bw.Width * (float) dy - 0.5);
                                bool b = bw.Pixel(x, y + cy);
                                onPixel(x, y + cy, b);
                            }
                        } else {
                            for (int x = 0; x < bw.Width; x++) {
                                int cy = (int) Math.Round((float) x / (float) bw.Width * (float) dy + 0.5);
                                bool b = bw.Pixel(x, y + cy);
                                onPixel(x, y + cy, b);
                            }
                        }
                        if (rStt.HasValue && rEnd.HasValue) {
                            lines.Add(new Tuple<Point, Point>(rStt.Value, rEnd.Value));
                        }
                    }
                }
                Console.WriteLine("lines found: " + lines.Count);
            });

            return lines;
        }
    }
}
