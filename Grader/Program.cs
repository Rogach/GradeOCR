using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grader.gui;

namespace Grader {
    public static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(new DataAccess(GetDbLocation())));
        }

        public static string GetDbLocation() {
            return "E:/Pronko/prj/Grader/Grades.accdb";
        }

        public static string GetTemplateLocation(string template) {
            return "E:/Pronko/prj/Grader/templates/" + template;
        }

    }
}
