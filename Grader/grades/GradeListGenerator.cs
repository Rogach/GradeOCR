using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using System.Data.Linq;
using LibUtil;

namespace Grader.grades {
    public static class GradeListGenerator {

        public static void GenerateGradeList(DataAccess dataAccess, IQueryable<Оценка> gradeQuery, string subjectName) {
            DataContext dc = dataAccess.GetDataContext();
            List<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);

            ExcelWorksheet sh = ExcelTemplates.CreateEmptyExcelTable();
            sh.GetRange("A1").Value = "дата";
            sh.GetRange("B1").Value = "подразделение";
            sh.GetRange("C1").Value = "звание";
            sh.GetRange("D1").Value = "ФИО";
            sh.GetRange("E1").Value = "Фамилия";
            sh.GetRange("F1").Value = "Имя";
            sh.GetRange("G1").Value = "Отчество";
            sh.GetRange("H1").Value = "оценка";
            var c = sh.GetRange("A2");
            ProgressDialogs.ForEach(gradeSets, s => {
                var g = GradeCalcIndividual.GetGrade(s, subjectName);
                g.ForEach(v => {
                    c.Value = s.gradeDate.ToString("MM.yyyy");
                    c.GetOffset(0, 1).Value = s.subunit.Имя;
                    c.GetOffset(0, 2).Value = s.rank.Название;
                    c.GetOffset(0, 3).Value = s.soldier.ФИО;
                    c.GetOffset(0, 4).Value = s.soldier.Фамилия;
                    c.GetOffset(0, 5).Value = s.soldier.Имя;
                    c.GetOffset(0, 6).Value = s.soldier.Отчество;
                    c.GetOffset(0, 7).Value = v;
                    c = c.GetOffset(1, 0);
                });
            });
            foreach (var col in new List<string> { "A1", "B1", "C1", "D1", "E1", "F1", "G1", "H1" }) {
                sh.GetRange(col).EntireColumn.AutoFit();
            }
            sh.Workbook.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }


    }
}
