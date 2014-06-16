using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.Office.Interop.Access;
using LibUtil.templates;
using Grader.util;
using LibUtil;

namespace Grader.grades {
    public static class GradeSummaryGenerator {
        public static void GenerateSummary(
                DataContext dc, 
                Подразделение subunit, 
                IQueryable<Оценка> gradeQuery, 
                string subjectName, 
                bool produceSummaryGrade,
                bool cadetsSelected,
                bool selectRelatedSubunits) {

            List<int> grades = Grades.GetSubjectGrades(gradeQuery, dc, subjectName);
            if (grades.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Нет оценок!");
                return;
            }

            var doc = WordTemplates.CreateEmptyWordDoc();
            var sel = doc.Application.Selection;
            sel.ParagraphFormat.LeftIndent = doc.Application.CentimetersToPoints(4);
            sel.ParagraphFormat.SpaceBefore = 0;
            sel.ParagraphFormat.SpaceBeforeAuto = 0;

            foreach (int g in new int[] { 5, 4, 3, 2 }) {
                int gCount = grades.Where(c => c == g).Count();
                sel.TypeText(String.Format("«{0}»\t\t- {1}\t({2:F1}%)", ReadableTextUtil.HumanReadableGrade(g), gCount, (float) gCount / grades.Count * 100));
                sel.TypeParagraph();
            }
            sel.TypeText(String.Format("Средний балл\t- {0:F2}", grades.Mean()));
            sel.TypeParagraph();
            if (produceSummaryGrade) {
                GradeCalcGroup.ОбщаяОценка(dc, gradeQuery, subunit, subjectName, cadetsSelected, selectRelatedSubunits).ForEach(summaryGrade => {
                    sel.TypeText(String.Format("Общая оценка «{0}»", ReadableTextUtil.HumanReadableGradeLong(summaryGrade)));
                    sel.TypeParagraph();
                });
            }
            doc.Saved = true;
            WordTemplates.ActivateWord(doc);
        }
    }
}
