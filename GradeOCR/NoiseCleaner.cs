using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class NoiseCleaner {
        public static readonly double cutoffRatio = 0.33;

        public static Bitmap RemoveNoise(Bitmap b) {
            List<List<Point>> islands = new List<List<Point>>();

            unsafe {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte* ptr = (byte*) bd.Scan0.ToPointer();
                byte* origPtr = ptr;

                bool[] visited = new bool[b.Width * b.Height];

                for (int y = 0; y < b.Height; y++) {
                    for (int x = 0; x < b.Width; x++) {
                        if (!visited[y * b.Width + x] && *ptr == 0) {
                            List<Point> island = new List<Point>();
                            Queue<Point> queue = new Queue<Point>();
                            queue.Enqueue(new Point(x, y));

                            while (queue.Count > 0) {
                                Point pt = queue.Dequeue();
                                if (!visited[pt.Y * b.Width + pt.X] && *(origPtr + 4 * (pt.Y * b.Width + pt.X)) == 0) {
                                    visited[pt.Y * b.Width + pt.X] = true;
                                    island.Add(pt);
                                    if (pt.X > 0) queue.Enqueue(new Point(pt.X - 1, pt.Y));
                                    if (pt.X < b.Width - 1) queue.Enqueue(new Point(pt.X + 1, pt.Y));
                                    if (pt.Y > 0) queue.Enqueue(new Point(pt.X, pt.Y - 1));
                                    if (pt.Y < b.Height - 1) queue.Enqueue(new Point(pt.X, pt.Y + 1));
                                }
                            }

                            islands.Add(island);
                        }
                        ptr += 4;
                    }
                }

                b.UnlockBits(bd);
            }

            int maxIslandSize = islands.Select(i => i.Count).Max();

            List<List<Point>> bigIslands = islands.Where(i => i.Count > maxIslandSize * cutoffRatio).ToList();

            Bitmap result = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(result);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, result.Width, result.Height));
            g.Dispose();

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

            return result;
        }
    }
}
