using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using LibUtil;
using OCRUtil;

namespace ARCode {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            //Util.Timed("stress test", () => {
            //    StressTest();
            //});

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int patternRadius = 100;

            Bitmap sourceImage = new Bitmap(1000, 1000, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(sourceImage);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
            g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), new Point(100, 200));
            g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), new Point(700, 200));
            g.Dispose();

            Application.Run(new FinderCircleDebugView(sourceImage, 90, 110));
        }

        public static NoiseFilter GetTestNoiseFilter() {
            return new FilterSeq(
                        new RandomBlots(0.2),
                        new RandomNoise(0.2),
                        new RandomStripes(0.2, 20)
                    );
        }

        static void StressTest() {
            int patternRadius = 100;
            int minPatternRadius = 90;
            int maxPatternRadius = 110;
            int imgSize = 1000;

            int N = 100;
            int success = 0;
            int failure = 0;

            Random r = new Random();
            double time = 0;
            for (int q = 0; q < N; q++) {
                Point3 patternLocation = new Point3(
                    r.Next(imgSize - patternRadius * 2) + patternRadius, 
                    r.Next(imgSize - patternRadius * 2) + patternRadius, 
                    patternRadius);
                Point patternDrawLocation = new Point(patternLocation.X - patternRadius, patternLocation.Y - patternRadius);

                Bitmap sourceImage = new Bitmap(imgSize, imgSize, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(sourceImage);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
                g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), patternDrawLocation);
                g.Dispose();

                Bitmap noisedImage = GetTestNoiseFilter().Apply(sourceImage);
                DateTime stt = DateTime.Now;
                Point3 recognizedLocation = CircleHoughTransform.LocateFinderCircles(noisedImage, minPatternRadius, maxPatternRadius, 1).First();
                DateTime end = DateTime.Now;
                time += (end - stt).TotalMilliseconds;

                bool succ =
                    Math.Abs(recognizedLocation.X - patternLocation.X) < 2 &&
                    Math.Abs(recognizedLocation.Y - patternLocation.Y) < 2;
                if (succ) {
                    success++;
                    Console.WriteLine("({0}/{1}) Success.", q + 1, N);
                } else {
                    failure++;
                    Console.WriteLine("({0}/{1}) Failure: expected {2}, got {3}", q + 1, N, patternLocation, recognizedLocation);
                }
            }
            Console.WriteLine("successes/failures: {0}/{1}", success, failure);
            Console.WriteLine("average recognition time: {0:F3} ms", time / N);
        }
    }
}
