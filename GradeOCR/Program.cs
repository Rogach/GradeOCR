using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GradeOCR {
    class Program {
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Form form = new Form();
            form.Size = new Size(600, 600);

            PictureView pv = new PictureView();
            pv.Location = new Point(0, 0);
            pv.Size = new Size(584, 562);
            pv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            form.Controls.Add(pv);

            Image img = Image.FromFile("E:/ocr/scan1.jpg");
            pv.Image = img;
            pv.ZoomToFit();

            Application.Run(form);
        }

    }
}
