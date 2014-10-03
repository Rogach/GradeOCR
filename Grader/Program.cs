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

        public static DateTime initTime;
        public static DateTime lastReportTime;

        public static void ReportEvent(string msg) {
            DateTime reportTime = DateTime.Now;
            TimeSpan fullDiff = reportTime - initTime;
            TimeSpan incrDiff = reportTime - lastReportTime;
            Console.WriteLine("[{0}.{1} ({2}.{3})] {4}",
                ((int) fullDiff.TotalSeconds).ToString().PadLeft(3, ' '),
                ((int) fullDiff.Milliseconds).ToString().PadLeft(3, '0'),
                ((int) incrDiff.TotalSeconds).ToString().PadLeft(2, ' '),
                ((int) incrDiff.Milliseconds).ToString().PadLeft(3, '0'),
                msg);
            lastReportTime = reportTime;
        }

        [STAThread]
        static void Main() {
            initTime = DateTime.Now;
            lastReportTime = DateTime.Now;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Option<Settings> settingsOpt = Settings.Load();
            ReportEvent("Loaded settings");
            settingsOpt.ForEach(settings => {
                ApplicationContext ctx = new ApplicationContext();
                MainForm mainForm = new MainForm(settings, ctx);
                ReportEvent("Created main form");
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
