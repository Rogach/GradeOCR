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
using System.Deployment;
using System.Reflection;

namespace Grader {
    public static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Settings.Load().ForEach(settings => {
                ApplicationContext ctx = new ApplicationContext();
                MainForm mainForm = new MainForm(settings, ctx);
                ctx.MainForm = mainForm;
                Application.Run(ctx);
                settings.Save();
            });
        }

        public static string GetVersion() {
            try {
                Version vers = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                return String.Format("{0}.{1}.{2}.{3}", vers.Major, vers.Minor, vers.Build, vers.Revision);
            } catch (Exception) {
                return "<dev>";
            }
        }

    }
}
