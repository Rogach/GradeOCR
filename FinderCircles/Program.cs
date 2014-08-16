using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FinderCircles {
    class Program {
        [STAThread]
        static void Main(string[] args) {
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
    }
}
