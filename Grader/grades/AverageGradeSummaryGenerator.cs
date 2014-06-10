using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using LibUtil;

namespace Grader.grades {
    public static class AverageGradeSummaryGenerator {
        public static void GenerateAverageGradeSummary(AccessApplication accessApp) {
            DataContext dc = accessApp.GetDataContext();
            var f = accessApp.GetForm("ПоОценкам");
            IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);

            var doc = WordTemplates.CreateEmptyWordDoc();
            var sel = doc.Application.Selection;
            Dictionary<string, double> averageGrades = new Dictionary<string, double>();

            ProgressDialogs.ForEach(dc.GetTable<Предмет>().ToListTimed(), subj => {
                List<int> grades = Grades.GetSubjectGrades(gradeQuery, dc, subj.Название);
                if (grades.Count > 0) {
                    double avgGrade = averageGrades.GetOrElseInsertAndGet(subj.Название, () => grades.Average());
                    sel.TypeText(String.Format("- по {0} средний балл составил {1:F2}.\n", subj.НазваниеДательный, avgGrade));
                }
            });

            if (averageGrades.Count > 0) {
                sel.TypeText("\n\n");
                Func<double, string> subjectWithGrade = grade => averageGrades.Where(kv => kv.Value == grade).Select(kv => kv.Key).MkString(", ");
                double maxGrade = averageGrades.Values.Max();
                sel.TypeText("Лучше подготовка по предметам: " + subjectWithGrade(maxGrade) + "\n");
                double minGrade = averageGrades.Values.Min();
                sel.TypeText("Хуже подготовка по предметам: " + subjectWithGrade(minGrade) + "\n");
            }

            doc.Saved = true;
            WordTemplates.ActivateWord(doc);
        }
    }
}
