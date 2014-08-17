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

            //Util.Timed("stress test", () => {
            //    StressTest();
            //});

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int patternRadius = 25;

            Bitmap sourceImage = new Bitmap(200, 200, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(sourceImage);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
            g.DrawImage(CircleDrawer.GetFinderCircleImage(patternRadius), new Point(50, 50));
            g.Dispose();

            Application.Run(new FinderCircleDebugView(sourceImage));
        }

        static void StressTest() {
            int patternSize = 25;
            int N = 30;
            int success = 0;
            int failure = 0;

            Random r = new Random();
            for (int q = 0; q < N; q++) {
                Point patternLocation = new Point(r.Next(150) + patternSize, r.Next(150) + patternSize);
                Point patternDrawLocation = new Point(patternLocation.X - patternSize, patternLocation.Y - patternSize);

                Bitmap sourceImage = new Bitmap(200, 200, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(sourceImage);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
                g.DrawImage(CircleDrawer.GetFinderCircleImage(patternSize), patternDrawLocation);
                g.Dispose();

                NoiseFilter filter = new FilterSeq(
                    new RandomBlots(0.15),
                    new RandomNoise(0.15),
                    new RandomLines(0.15),
                    new RandomStripes(0.2, 20)
                );

                Bitmap noisedImage = filter.Apply(sourceImage);
                int[,] hough = CircleHoughTransform.HoughTransform(noisedImage, patternSize);
                List<Point> peaks = CircleHoughTransform.LocatePeaks(hough, patternSize);
                Point recognizedLocation = new Point(peaks.First().X + patternSize, peaks.First().Y + patternSize);
                bool succ = recognizedLocation == patternLocation;
                if (succ) {
                    success++;
                    Console.WriteLine("({0}/{1}) Success.", q + 1, N);
                } else {
                    failure++;
                    Console.WriteLine("({0}/{1}) Failure: expected {2}, got {3}", q + 1, N, patternLocation, recognizedLocation);
                }
                
                
            }
        }
    }
}
