using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using LibUtil;
using OCRUtil;
using ZXing.Common.ReedSolomon;

namespace ARCode {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            OpenFileDialog fd = new OpenFileDialog();
            fd.Title = "Выберите изображение ведомости";
            if (fd.ShowDialog() == DialogResult.OK) {
                Bitmap sourceImage = ImageUtil.LoadImage(fd.FileName);
                Application.Run(new FinderCircleDebugView(sourceImage, 50, 70, inputValue: 0, noiseFilter: new EmptyFilter()));
            }
        }

        public static NoiseFilter GetTestNoiseFilter() {
            return new FilterSeq(
                        new RandomBlots(0.2),
                        new RandomNoise(0.2),
                        new RandomStripes(0.05, 20)
                    );
        }

        static void RandomTest() {
            Random r = new Random();
            uint value = (uint) r.Next();
            
            int patternRadius = 60;

            Bitmap sourceImage = new Bitmap(1000, 1000, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(sourceImage);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));

            g.TranslateTransform(sourceImage.Width / 2, sourceImage.Height / 2);
            g.RotateTransform(-15);
            g.TranslateTransform(-sourceImage.Width / 2, -sourceImage.Height / 2);

            Bitmap codeImage = ARCodeUtil.BuildCode(value, patternRadius);

            g.DrawImage(codeImage, new Point(150, 400));

            g.Dispose();

            Application.Run(new FinderCircleDebugView(sourceImage, 50, 70, value, noiseFilter: GetTestNoiseFilter()));
        }

        static void StressTest() {
            int patternRadius = 60;
            int minPatternRadius = 50;
            int maxPatternRadius = 70;
            int imgSize = 3000;

            int N = 100;
            int success = 0;
            int failure = 0;

            Random r = new Random();
            double time = 0;
            for (int q = 0; q < N; q++) {
                uint codeValue = (uint) r.Next();
                Bitmap codeImage = ARCodeUtil.BuildCode(codeValue, patternRadius);

                int diag = (int) PointOps.Distance(0, 0, codeImage.Width, codeImage.Height);
                Point codeLocation = new Point(
                    r.Next(imgSize - diag) + diag / 2, 
                    r.Next(imgSize - diag) + diag / 2);
                
                Bitmap sourceImage = new Bitmap(imgSize, imgSize, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(sourceImage);
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height));
                
                g.TranslateTransform(codeLocation.X, codeLocation.Y);
                g.RotateTransform((float) ((r.NextDouble() - 0.5) * 2));
                g.TranslateTransform(-codeLocation.X, -codeLocation.Y);

                Point codeDrawLocation = new Point(
                    codeLocation.X - codeImage.Width / 2,
                    codeLocation.Y - codeImage.Height / 2);

                g.DrawImage(codeImage, codeDrawLocation);
                g.Dispose();

                Bitmap noisedImage = GetTestNoiseFilter().Apply(sourceImage);

                DateTime stt = DateTime.Now;
                Option<uint> extractedValue = ARCodeUtil.ExtractCode(noisedImage, minPatternRadius, maxPatternRadius);
                DateTime end = DateTime.Now;
                time += (end - stt).TotalMilliseconds;

                if (extractedValue.NonEmpty() && extractedValue.Get() == codeValue) {
                    success++;
                    Console.WriteLine("({0}/{1}) Success.", q + 1, N);
                } else {
                    failure++;
                    Console.WriteLine("({0}/{1}) Failure.", q + 1, N);
                }
            }
            Console.WriteLine("successes/failures: {0}/{1}", success, failure);
            Console.WriteLine("average recognition time: {0:F3} ms", time / N);
        }
    }
}
