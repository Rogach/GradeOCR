using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using System.Data.Linq;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using LibUtil;

namespace Grader.grades {
    public static class PerDaySummaryGenerator {
        public static void GenerateSummaryPerDay(Application accessApp) {
            DataContext dc = accessApp.GetDataContext();
            Form f = accessApp.GetForm("ПоОценкам").Get();
            bool excludeKMN = f.GetControl("ExcludeKMNFromPerDaySummary").Get().BooleanValue();

            var wb = ExcelTemplates.LoadExcelTemplate(accessApp.Template("день_за_днем.xlsx"));
            ExcelWorksheet sh = wb.Worksheets.First();

            IQueryable<Оценка> genGradeQuery =
                from g in Grades.GetGradeQuery(accessApp, dc)
                from soldier in dc.GetTable<Военнослужащий>()
                where g.КодПроверяемого == soldier.Код
                where (!excludeKMN) || (soldier.КМН == 0)
                select g;

            var subjects = (from subj in dc.GetTable<Предмет>() select subj).ToListTimed().Where(subj => subj.ДЗД);
            ExcelTemplates.WithTemplateRow(sh.GetRange("A2:L8"), subjects, displayProgress: true, format: (subj, subjectRange) => {
                return OutputSubject(subj, subjectRange, genGradeQuery, dc);
            });
            sh.ResetPrintArea();
            ExcelTemplates.ActivateExcel(sh);
        }

        static bool OutputSubject(Предмет subj, ExcelRange subjectRange, IQueryable<Оценка> genGradeQuery, DataContext dc) {
            var dateQuery =
                from g in genGradeQuery
                where g.КодПредмета == subj.Код
                orderby g.ДатаОценки
                select g.ДатаОценки;
            var dateList = dateQuery.Distinct().ToListTimed();
            if (dateList.Count == 0) {
                return false;
            } else {
                IQueryable<Оценка> subjectGradeQuery =
                    from g in genGradeQuery
                    where g.КодПредмета == subj.Код
                    select g;

                var totalSubjectGrades = new List<int>();
                var totalSubjectSubunitGrades = new List<int>();
                subjectRange.GetResize(1, 1).Value = subj.ПолноеНазвание;
                subjectRange.GetOffset(1, 0).GetResize(1, 1).Value = ReadableTextUtil.GetMonthGenitive(dateList.First()) + " " + dateList.First().Year;
                var sumCell = subjectRange.GetOffset(5, 0).GetResize(1, 1);
                ExcelTemplates.WithTemplateRow(subjectRange.GetOffset(3, 0).GetResize(2, 12), dateList, displayProgress: true, format: (date, dateCells) => {
                    var cell = dateCells.GetResize(1, 1);
                    var totalDateGrades = new List<int>();
                    var subunitSummaryGrades = new List<int>();
                    var gradeQuery =
                        from g in subjectGradeQuery
                        where g.ДатаОценки == date
                        select g;
                    foreach (var subunitGrades in gradeQuery.ToListTimed().GroupBy(g => g.КодПодразделения)) {
                        var grades = subunitGrades.Select(g => g.Значение).ToList();
                        totalDateGrades.AddRange(grades);
                        GradeCalcGroup.ФормулаКурсантыПоПредмету(grades).ForEach(subjectGrade => {
                            subunitSummaryGrades.Add(subjectGrade);
                        });
                    }
                    totalSubjectGrades.AddRange(totalDateGrades);
                    totalSubjectSubunitGrades.AddRange(subunitSummaryGrades);

                    cell.Value = date.Day;
                    cell.GetOffset(0, 1).Value = subunitSummaryGrades.Count;
                    foreach (int a in new int[] { 5, 4, 3, 2 }) {
                        cell.GetOffset(0, 7 - a).Value = subunitSummaryGrades.Count(g => g == a);
                        cell.GetOffset(0, 7 - a).GetOffset(1, 0).Value = subunitSummaryGrades.PercentWhere(g => g == a);
                    }
                    cell.GetOffset(0, 6).Value = totalDateGrades.Count();
                    foreach (int a in new int[] { 5, 4, 3, 2 }) {
                        cell.GetOffset(0, 12 - a).Value = totalDateGrades.Count(g => g == a);
                        cell.GetOffset(0, 12 - a).GetOffset(1, 0).Value = totalDateGrades.PercentWhere(g => g == a);
                    }
                    GradeCalcGroup.КурсантыПоПредметуЗаЧасть(dc, Grades.GradeSets(dc, gradeQuery), subj.Название).ForEach(summGrade => {
                        cell.GetOffset(0, 11).Value = summGrade;
                    });
                    cell.GetOffset(0, 11).GetOffset(1, 0).Value = totalDateGrades.Mean();
                    return true;
                });
                sumCell.GetOffset(0, 1).Value = totalSubjectSubunitGrades.Count;
                foreach (int a in new int[] { 5, 4, 3, 2 }) {
                    sumCell.GetOffset(0, 7 - a).Value = totalSubjectSubunitGrades.Count(g => g == a);
                    sumCell.GetOffset(0, 7 - a).GetOffset(1, 0).Value = totalSubjectSubunitGrades.PercentWhere(g => g == a);
                }
                sumCell.GetOffset(0, 6).Value = totalSubjectGrades.Count();
                foreach (int a in new int[] { 5, 4, 3, 2 }) {
                    sumCell.GetOffset(0, 12 - a).Value = totalSubjectGrades.Count(g => g == a);
                    sumCell.GetOffset(0, 12 - a).GetOffset(1, 0).Value = totalSubjectGrades.PercentWhere(g => g == a);
                }
                GradeCalcGroup.КурсантыПоПредметуЗаЧасть(dc, Grades.GradeSets(dc, subjectGradeQuery), subj.Название).ForEach(summaryGrade => {
                    sumCell.GetOffset(0, 11).Value = summaryGrade;
                });
                sumCell.GetOffset(0, 11).GetOffset(1, 0).Value = totalSubjectGrades.Mean();
                return true;
            }
        }
    }
}
