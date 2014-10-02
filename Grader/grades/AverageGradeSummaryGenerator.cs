using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using LibUtil;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.grades {
    public static class AverageGradeSummaryGenerator {
        public static void GenerateAverageGradeSummary(RichTextBox resultBox, Entities et, IQueryable<Оценка> gradeQuery) {
            Dictionary<string, double> averageGrades = new Dictionary<string, double>();

            resultBox.Clear();

            List<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
            ProgressDialogs.ForEach(et.Предмет.ToList(), subj => {
                List<int> grades = gradeSets.GetSubjectGrades(subj.Название);
                if (grades.Count > 0) {
                    double avgGrade = averageGrades.GetOrElseInsertAndGet(subj.Название, () => grades.Average());
                    resultBox.Text += String.Format("- по {0} средний балл составил {1:F2}.\n", subj.НазваниеДательный, avgGrade);
                }
            });

            if (averageGrades.Count > 0) {
                resultBox.Text += "\n\n";
                Func<double, string> subjectWithGrade = grade => averageGrades.Where(kv => kv.Value == grade).Select(kv => kv.Key).MkString(", ");
                double maxGrade = averageGrades.Values.Max();

                resultBox.Text += "Лучше подготовка по предметам: " + subjectWithGrade(maxGrade) + "\n";
                double minGrade = averageGrades.Values.Min();
                resultBox.Text += "Хуже подготовка по предметам: " + subjectWithGrade(minGrade) + "\n";
            }

            resultBox.SelectAll();
            resultBox.SelectionFont = new Font("Times New Roman", 14f, FontStyle.Regular);
            Clipboard.SetText(resultBox.Rtf, TextDataFormat.Rtf);
        }
    }
}
