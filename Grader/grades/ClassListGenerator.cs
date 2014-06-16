using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using LibUtil.wrapper.excel;
using LibUtil;

namespace Grader.grades {
    public static class ClassListGenerator {
        public static void GenerateClassList(DataContext dc, IQueryable<Оценка> gradeQuery) {
            var gradeSets = Grades.GradeSets(dc, gradeQuery).Where(gs => GradeCalcIndividual.КлассностьКурсанты(gs));

            ExcelWorksheet sh = ExcelTemplates.CreateEmptyExcelTable();
            ExcelRange r = sh.GetRange("A1");

            r.Value = "звание";
            r.GetOffset(0, 1).Value = "Фамилия";
            r.GetOffset(0, 2).Value = "Имя";
            r.GetOffset(0, 3).Value = "Отчество";
            r = r.GetOffset(1, 0);

            ProgressDialogs.ForEach(gradeSets, gs => {
                r.Value = gs.rank.Название;
                r.GetOffset(0, 1).Value = gs.soldier.Фамилия;
                r.GetOffset(0, 2).Value = gs.soldier.Имя;
                r.GetOffset(0, 3).Value = gs.soldier.Отчество;
                r = r.GetOffset(1, 0);
            });

            sh.GetRange("A1").EntireColumn.AutoFit();
            sh.GetRange("B1").EntireColumn.AutoFit();
            sh.GetRange("C1").EntireColumn.AutoFit();
            sh.GetRange("D1").EntireColumn.AutoFit();

            sh.Workbook.Saved = true;

            ExcelTemplates.ActivateExcel(sh);
        }
    }
}