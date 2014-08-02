using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class NoiseCleaner {
        public static readonly double cutoffRatio = 0.2;
        public static readonly double secondaryCutoffRatio = 0.33;
        public static readonly int noiseCrop = 2;

        public static Bitmap RemoveNoise(Bitmap b) {
            List<List<Point>> islands = new List<List<Point>>();

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();

                bool[] visited = new bool[b.Width * b.Height];

                for (int y = noiseCrop; y < b.Height - noiseCrop; y++) {
                    for (int x = noiseCrop; x < b.Width - noiseCrop; x++) {
                        if (!visited[y * b.Width + x] && *(ptr + 4 * (y * b.Width + x)) == 0) {
                            List<Point> island = new List<Point>();
                            Queue<Point> queue = new Queue<Point>();
                            queue.Enqueue(new Point(x, y));

                            while (queue.Count > 0) {
                                Point pt = queue.Dequeue();
                                if (!visited[pt.Y * b.Width + pt.X] && *(ptr + 4 * (pt.Y * b.Width + pt.X)) == 0) {
                                    visited[pt.Y * b.Width + pt.X] = true;
                                    island.Add(pt);
                                    if (pt.X > 0) 
                                        queue.Enqueue(new Point(pt.X - 1, pt.Y));
                                    if (pt.X > 0 && pt.Y > 0) 
                                        queue.Enqueue(new Point(pt.X - 1, pt.Y - 1));
                                    if (pt.X < b.Width - 1) 
                                        queue.Enqueue(new Point(pt.X + 1, pt.Y));
                                    if (pt.X < b.Width - 1 && pt.Y > 0) 
                                        queue.Enqueue(new Point(pt.X + 1, pt.Y - 1));
                                    if (pt.Y > 0) 
                                        queue.Enqueue(new Point(pt.X, pt.Y - 1));
                                    if (pt.X > 0 && pt.Y < b.Height - 1)
                                        queue.Enqueue(new Point(pt.X - 1, pt.Y + 1));
                                    if (pt.Y < b.Height - 1) 
                                        queue.Enqueue(new Point(pt.X, pt.Y + 1));
                                    if (pt.X < b.Width - 1 && pt.Y < b.Height - 1)
                                        queue.Enqueue(new Point(pt.X + 1, pt.Y + 1));
                                }
                            }

                            islands.Add(island);
                        }
                    }
                }

                b.UnlockBits(bd);
            }

            
            Bitmap result = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, result.Width, result.Height));
            g.Dispose();

            if (islands.Count > 0) {
                int maxIslandSize = islands.Select(i => i.Count).Max();

                List<List<Point>> bigIslands = 
                    islands
                    .Where(i => i.Count > maxIslandSize * cutoffRatio)
                    .Where(i => {
                        // remove circular islands less than secondary cutoff
                        if (i.Count > maxIslandSize * secondaryCutoffRatio) { 
                            return true;
                        } else {
                            int dx = i.Select(p => p.X).Max() - i.Select(p => p.X).Min();
                            int dy = i.Select(p => p.Y).Max() - i.Select(p => p.Y).Min();
                            int circleDiameter = (int) Math.Ceiling(Math.Sqrt(i.Count) / Math.PI * 2);
                            return (dy > 3 * circleDiameter) || (dx > 3 * circleDiameter);
                        }
                    })
                    .ToList();

                unsafe {
                    BitmapData rbd = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    byte* rptr = (byte*) rbd.Scan0.ToPointer();

                    foreach (var island in bigIslands) {
                        foreach (Point pt in island) {
                            byte* iptr = rptr + 4 * (pt.Y * result.Width + pt.X);
                            *iptr = 0;
                            *(iptr + 1) = 0;
                            *(iptr + 2) = 0;
                        }
                    }

                    result.UnlockBits(rbd);
                }
            }

            return result;
        }
    }
}
