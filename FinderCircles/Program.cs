using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using LibUtil;

namespace FinderCircles {
    class Program {
        [STAThread]
        static void Main(string[] args) {

            Util.Timed("stress test", () => {
                StressTest();
            });

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            //int patternRadius = 25;

            //Bitmap sourceImage = new Bitmap(200, 200, PixelFormat.Format32bppArgb);
            //Graphics g = Graphics.FromImage(sourceImage);
            //g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
            //g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), new Point(50, 50));
            //g.Dispose();

            //Application.Run(new FinderCircleDebugView(sourceImage));
        }

        static void StressTest() {
            int patternRadius = 25;
            int minPatternRadius = 20;
            int maxPatternRadius = 30;

            int N = 100;
            int success = 0;
            int failure = 0;

            Random r = new Random();
            double time = 0;
            for (int q = 0; q < N; q++) {
                Point3 patternLocation = new Point3(r.Next(150) + patternRadius, r.Next(150) + patternRadius, patternRadius);
                Point patternDrawLocation = new Point(patternLocation.X - patternRadius, patternLocation.Y - patternRadius);

                Bitmap sourceImage = new Bitmap(200, 200, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(sourceImage);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
                g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), patternDrawLocation);
                g.Dispose();

                NoiseFilter filter = new FilterSeq(
                    new RandomBlots(0.10),
                    new RandomNoise(0.10),
                    new RandomStripes(0.10, 20)
                );

                Bitmap noisedImage = filter.Apply(sourceImage);

                DateTime stt = DateTime.Now;
                int[,,] hough = CircleHoughTransform.HoughTransform(noisedImage, minPatternRadius, maxPatternRadius);
                DateTime end = DateTime.Now;
                time += (end - stt).TotalMilliseconds;

                List<Point3> peaks = CircleHoughTransform.LocatePeaks(hough);
                Point3 recognizedLocation = peaks.First();
                recognizedLocation.Z += minPatternRadius;
                bool succ =
                    Math.Abs(recognizedLocation.X - patternLocation.X) < 3 &&
                    Math.Abs(recognizedLocation.Y - patternLocation.Y) < 3;

                if (succ) {
                    success++;
                    Console.WriteLine("({0}/{1}) Success.", q + 1, N);
                } else {
                    failure++;
                    Console.WriteLine("({0}/{1}) Failure: expected {2}, got {3}", q + 1, N, patternLocation, recognizedLocation);
                }
                
            }
            Console.WriteLine("successes/failures: {0}/{1}", success, failure);
            Console.WriteLine("hough transform time: {0:F3} ms", time);
        }
    }
}
