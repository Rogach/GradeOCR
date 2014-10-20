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

        public static void GenerateTableWithResultsBySubunitType(
                Entities et,
                IQueryable<Оценка> gradeQuery,
                string subunitType,
                string subjectName,
                bool byVus, 
                bool forAllSubunits,
                bool forAllSubjects,
                bool cadetsSelected,
                bool transposeTable) {
            
            Func<ExcelRange, int, int, ExcelRange> GetOffset2 = (rng, row, col) => {
                if (transposeTable) {
                    return rng.GetOffset(col, row);
                } else {
                    return rng.GetOffset(row, col);
                }
            };

            if (byVus && subunitType != "цикл") {
                throw new Exception("expecting cycles for byVus calculation");
            }

            var sh = ExcelTemplates.CreateEmptyExcelTable();

            if (forAllSubjects) {
                List<Оценка> gradeList = gradeQuery.ToList();
                var subjects = et.Предмет.ToList();
                var subunits = Querying.GetSubunitsByType(et, subunitType).ToList();
                ProgressDialogs.WithProgress(subjects.Count * subunits.Count, pd => {
                    var subunitNameCell = GetOffset2(sh.GetRange("A1"), 1, 0);
                    foreach (var subunit in subunits) {
                        subunitNameCell.Value = subunit.ИмяКраткое;
                        subunitNameCell = GetOffset2(subunitNameCell, 1, 0);;
                    }
                    var subjectNameCell = GetOffset2(sh.GetRange("A1"), 0, 1);
                    foreach (var subj in subjects) {
                        var c = GetOffset2(subjectNameCell, 1, 0);
                        int subunitCount = 0;
                        foreach (var subunit in subunits) {
                            List<int> grades = 
                                (byVus ? 
                                    Grades.GetGradesOnCycle(et, gradeList, subunit.Код) :
                                    Grades.GetGradesForSubunit(et, gradeList, subunit.Код))
                                .Where(g => g.КодПредмета == subj.Код && !g.ЭтоКомментарий)
                                .Select(g => (int) g.Значение).ToList();
                            if (grades.Count > 0) {
                                c.Value = grades.Mean();
                                c.NumberFormat = "0.00";
                                subunitCount++;
                            }
                            c = GetOffset2(c, 1, 0);
                            pd.Increment();
                        }
                        if (subunitCount > 0) {
                            subjectNameCell.Value = subj.Название;
                            subjectNameCell = GetOffset2(subjectNameCell, 0, 1);
                        }
                    }
                });
            } else {
                var c = sh.GetRange("A1");
                GetOffset2(c, 0, 1).Value = "средняя";
                GetOffset2(c, 0, 2).Value = "общая";
                GetOffset2(c, 0, 3).Value = "место";
                c = GetOffset2(c, 1, 0);
                var subunits = Querying.GetSubunitsByType(et, subunitType).ToList();
                var subunitGrades = ProgressDialogs.Map(subunits, subunit => 
                    new {
                        subunitId = subunit.Код,
                        grades = Grades.GetSubjectGrades(
                            byVus ?
                                Grades.GetGradesOnCycle(et, gradeQuery, subunit.Код) :
                                Grades.GetGradesForSubunit(et, gradeQuery, subunit.Код),
                            et, subjectName)
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
                            c = GetOffset2(c, 1, 0);
                        }
                    } else {
                        c.Value = subunit.ИмяКраткое;
                        GetOffset2(c, 0, 1).NumberFormat = "0.00";
                        GetOffset2(c, 0, 1).Value = grades.Mean();
                        if (byVus && cadetsSelected) {
                            GradeCalcGroup.КурсантыПоПредметуЗаЦикл(et, gradeQuery, subunit.Код, subjectName).ForEach(g => {
                                GetOffset2(c, 0, 2).Value = g;
                            });
                        } else {
                            GradeCalcGroup.ОбщаяОценка(
                                et,
                                Grades.GetGradesForSubunit(et, gradeQuery, subunit.Код),
                                subunit,
                                subjectName,
                                cadetsSelected,
                                selectRelatedSubunits: true).ForEach(g => {
                                    GetOffset2(c, 0, 2).Value = g;
                            });
                        }
                        GetOffset2(c, 0, 3).Value = subunitIds.IndexOf(subunit.Код) + 1;
                        if (subunitIds.First() == subunit.Код) {
                            GetOffset2(c, 0, 3).BackgroundColor = ExcelEnums.Color.Green;
                        } else if (subunitIds.Last() == subunit.Код) {
                            GetOffset2(c, 0, 3).BackgroundColor = ExcelEnums.Color.Red;
                        }
                        c = GetOffset2(c, 1, 0);
                    }
                });
            }

            sh.Workbook.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }
    }
}
