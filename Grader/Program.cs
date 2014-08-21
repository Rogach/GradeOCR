using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grader.gui;
using System.IO;
using System.Xml;
using LibUtil;
using System.Data.Linq;
using Grader.ocr;
using OCRUtil;

namespace Grader {
    public static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Settings.Load().ForEach(settings => {
                Entities et = Entities.CreateEntities(settings);
                RegisterRecognition.RecognizeRegisterImage(et, ImageUtil.LoadImage("E:/arcode-scan.jpg"));
            });

            //Settings.Load().ForEach(settings => {
            //    ApplicationContext ctx = new ApplicationContext();
            //    MainForm mainForm = new MainForm(settings, ctx);
            //    ctx.MainForm = mainForm;
            //    Application.Run(ctx);
            //    settings.Save();
            //});
        }

    }
}
