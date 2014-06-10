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
        public static void GenerateSummary(Application accessApp) {
            
            Form f = accessApp.GetForm("ПоОценкам").Get();
            int subunitId = f.GetControl("SubunitSelect").Get().IntegerValue();
            string subjectName = f.GetControl("SubjectSelect").Get().StringValue();
            bool produceSummaryGrade = f.GetControl("ProduceSummaryGrade").Get().BooleanValue();
            DataContext dc = accessApp.GetDataContext();
            Подразделение subunit = dc.GetTable<Подразделение>().Where(s => s.Код == subunitId).ToListTimed().First();
            IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);

            List<int> grades = Grades.GetSubjectGrades(Grades.GetGradesForSubunit(dc, gradeQuery, subunitId), dc, subjectName);
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
                GradeCalcGroup.ОбщаяОценка(accessApp, dc, gradeQuery, subunit, subjectName).ForEach(summaryGrade => {
                    sel.TypeText(String.Format("Общая оценка «{0}»", ReadableTextUtil.HumanReadableGradeLong(summaryGrade)));
                    sel.TypeParagraph();
                });
            }
            doc.Saved = true;
            WordTemplates.ActivateWord(doc);
        }
    }
}
