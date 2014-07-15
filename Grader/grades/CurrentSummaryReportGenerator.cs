using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using LibUtil;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.grades {
    public class CurrentSummaryReportGenerator {
        public static void GenerateCurrentSummaryReport(RichTextBox resultBox, Entities et, IQueryable<Оценка> gradeQuery) {

            var gradeSets = Grades.GradeSets(et, gradeQuery);
            var platoonCount = gradeSets.Select(gs => gs.subunit.Код).Distinct().Count();

            resultBox.Clear();
            resultBox.Text += "Справка хода ВЭ в 90 МРУЦ\n\n";
            resultBox.Text += "По состоянию на " + DateTime.Now.ToString("HH:mm dd.MM.yyyy года") + "\n\n";
            resultBox.Text += String.Format("Обработаны результаты по {0} взводам, сдававшим ВЭ.\n\n", platoonCount);
            
            foreach (var subj in et.Предмет.ToList()) {
                List<int> grades = gradeSets.GetSubjectGrades(subj.Название);
                Func<int, int> countGrades = x => grades.Where(g => g == x).Count();
                Func<int, double> percentGrades = x => (double) countGrades(x) / grades.Count * 100;
                if (grades.Count > 0) {
                    resultBox.Text += String.Format(
                            "Средний балл по {0} - {1}, из них на:\n" +
                            "Отлично - {2} ({3:F1}%);\n" +
                            "Хорошо - {4} ({5:F1}%);\n" +
                            "Удовлетворительно - {6} ({7:F1}%);\n" +
                            "Неудовлетворительно - {8} ({9:F1}%).\n\n",
                            subj.Название, String.Format("{0:F2}", CollectionUtil.Mean(grades)).Replace(",","."),
                            countGrades(5), percentGrades(5),
                            countGrades(4), percentGrades(4),
                            countGrades(3), percentGrades(3),
                            countGrades(2), percentGrades(2)
                    );
                }
            }

            resultBox.SelectAll();
            resultBox.SelectionFont = new Font("Times New Roman", 14f, FontStyle.Regular);
            Clipboard.SetText(resultBox.Rtf, TextDataFormat.Rtf);
        }
    }
}
