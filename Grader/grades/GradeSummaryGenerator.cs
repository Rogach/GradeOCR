using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.Office.Interop.Access;
using LibUtil.templates;
using Grader.util;
using LibUtil;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.grades {
    public static class GradeSummaryGenerator {
        public static void GenerateSummary(
                RichTextBox resultBox,
                Entities et, 
                Подразделение subunit, 
                IQueryable<Оценка> gradeQuery, 
                string subjectName, 
                bool produceSummaryGrade,
                bool cadetsSelected,
                bool selectRelatedSubunits) {

            List<int> grades = Grades.GetSubjectGrades(gradeQuery, et, subjectName);
            if (grades.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Нет оценок!");
                return;
            }

            resultBox.Clear();
            
            foreach (int g in new int[] { 5, 4, 3, 2 }) {
                int gCount = grades.Where(c => c == g).Count();
                resultBox.Text += String.Format("«{0}»{1}- {2}\t({3:F1}%)\n", 
                                    ReadableTextUtil.HumanReadableGrade(g), 
                                    g == 5 ? "\t" : "\t\t", 
                                    gCount, 
                                    (float) gCount / grades.Count * 100);
            }
            resultBox.Text += String.Format("Средний балл\t- {0:F2}\n", grades.Mean());
            
            if (produceSummaryGrade) {
                GradeCalcGroup.ОбщаяОценка(et, gradeQuery, subunit, subjectName, cadetsSelected, selectRelatedSubunits).ForEach(summaryGrade => {
                    resultBox.Text += String.Format("Общая оценка «{0}»\n", ReadableTextUtil.HumanReadableGradeLong(summaryGrade));
                });
            }

            resultBox.SelectAll();
            resultBox.SelectionFont = new Font("Times New Roman", 14f, FontStyle.Regular);
            resultBox.SelectionIndent = (int) (4 * (96 / 2.51)); // cm
            Clipboard.SetText(resultBox.Rtf, TextDataFormat.Rtf);
        }
    }
}
