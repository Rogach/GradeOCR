using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.Office.Interop.Access;
using Grader.util;
using LibUtil.templates;
using LibUtil.wrapper.excel;
using LibUtil;

namespace Grader.grades {
    public static class AverageGradeTableGenerator {
        public static void GenerateTableWithResultsBySubunitType(Application accessApp, string subunitType, bool byVus) {
            DataContext dc = accessApp.GetDataContext();
            var f = accessApp.GetForm("ПоОценкам").Get();
            bool forAllSubunits = f.GetControl("ChoiceResultsForAllSubunits").Get().BooleanValue();
            bool forAllSubjects = f.GetControl("ChoiceResultsForAllSubjects").Get().BooleanValue();
            if (byVus && subunitType != "цикл") {
                throw new Exception("expecting cycles for byVus calculation");
            }

            var sh = ExcelTemplates.CreateEmptyExcelTable();

            IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);

            if (forAllSubjects) {
                var subjects = dc.GetTable<Предмет>().ToListTimed();
                var subunits = Querying.GetSubunitsByType(dc, subunitType);
                ProgressDialogs.WithProgress(subjects.Count * subunits.Count, pd => {
                    var subunitNameCell = sh.GetRange("B1");
                    foreach (var subunit in subunits) {
                        subunitNameCell.Value = subunit.ИмяКраткое;
                        subunitNameCell = subunitNameCell.GetOffset(0, 1);
                    }
                    var subjectNameCell = sh.GetRange("A2");
                    foreach (var subj in subjects) {
                        var c = subjectNameCell.GetOffset(0, 1);
                        int subunitCount = 0;
                        foreach (var subunit in subunits) {
                            List<int> grades = Grades.GetSubjectGrades(
                                byVus ? 
                                    Grades.GetGradesOnCycle(dc, gradeQuery, subunit.Код) :
                                    Grades.GetGradesForSubunit(dc, gradeQuery, subunit.Код), 
                                dc, subj.Название);
                            if (grades.Count > 0) {
                                c.Value = grades.Mean();
                                subunitCount++;
                            }
                            c = c.GetOffset(0, 1);
                            pd.Increment();
                        }
                        if (subunitCount > 0) {
                            subjectNameCell.Value = subj.Название;
                            subjectNameCell = subjectNameCell.GetOffset(1, 0);
                        }
                    }
                });
            } else {
                string subjectName = f.GetControl("SubjectSelect").Get().StringValue();
                var c = sh.GetRange("A1");
                c.GetOffset(1, 0).Value = "средняя";
                c.GetOffset(2, 0).Value = "общая";
                c.GetOffset(3, 0).Value = "место";
                c = c.GetOffset(0, 1);
                var subunits = Querying.GetSubunitsByType(dc, subunitType);
                var subunitGrades = ProgressDialogs.Map(subunits, subunit => 
                    new {
                        subunitId = subunit.Код,
                        grades = Grades.GetSubjectGrades(
                            byVus ?
                                Grades.GetGradesOnCycle(dc, gradeQuery, subunit.Код) :
                                Grades.GetGradesForSubunit(dc, gradeQuery, subunit.Код),
                            dc, subjectName)
                    }
                );
                var subunitGrade = subunitGrades.ToDictionary(s => s.subunitId);
                var subunitIds = subunitGrades
                    .Where(s => s.grades.Count() > 0)
                    .OrderBy(s => s.grades.Mean())
                    .Select(s => s.subunitId)
                    .Reverse()
                    .ToList();
                ProgressDialogs.ForEach(subunits, subunit => {
                    List<int> grades = subunitGrade[subunit.Код].grades;
                    if (grades.Count == 0) {
                        if (forAllSubunits) {
                            c.Value = subunit.ИмяКраткое;
                            c = c.GetOffset(0, 1);
                        }
                    } else {
                        c.Value = subunit.ИмяКраткое;
                        c.GetOffset(1, 0).Value = grades.Mean();
                        if (byVus) {
                            GradeCalcGroup.КурсантыПоПредметуЗаЦикл(dc, gradeQuery, subunit.Код, subjectName).ForEach(g => {
                                c.GetOffset(2, 0).Value = g;
                            });
                        } else {
                            GradeCalcGroup.ОбщаяОценка(accessApp, dc, gradeQuery, subunit, subjectName).ForEach(g => {
                                c.GetOffset(2, 0).Value = g;
                            });
                        }
                        c.GetOffset(3, 0).Value = subunitIds.IndexOf(subunit.Код) + 1;
                        if (subunitIds.First() == subunit.Код) {
                            c.GetOffset(3, 0).BackgroundColor = ExcelEnums.Color.Green;
                        } else if (subunitIds.Last() == subunit.Код) {
                            c.GetOffset(3, 0).BackgroundColor = ExcelEnums.Color.Red;
                        }
                        c = c.GetOffset(0, 1);
                    }
                });
            }

            sh.Workbook.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }
    }
}
