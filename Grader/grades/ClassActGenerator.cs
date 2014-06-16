using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using LibUtil;

namespace Grader.grades {
    public static class ClassActGenerator {
        public static void GenerateClassAct(DataAccess dataAccess, Подразделение subunit, DateTime actDate, IQueryable<Оценка> gradeQuery) {
            ExcelWorkbook wb = ExcelTemplates.LoadExcelTemplate(dataAccess.GetTemplateLocation("акт_на_классность.xlsx"));
            ExcelWorksheet sh = wb.Worksheets.First();
            FormatActSheet(sh, subunit, dataAccess.GetDataContext(), actDate, gradeQuery);
            wb.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }

        public static void GenerateAllClassActs(DataAccess dataAccess, DateTime actDate, IQueryable<Оценка> gradeQuery) {
            var wb = ExcelTemplates.LoadExcelTemplate(dataAccess.GetTemplateLocation("акт_на_классность.xlsx"));
            ExcelWorksheet templateSheet = wb.Worksheets.First();
            DataContext dc = dataAccess.GetDataContext();
            var platoonIds =
                Querying.GetSubunitsByType(dc, "взвод")
                .Where(s => s.ТипОбучения == "срочники")
                .Select(s => s.Код);
            ProgressDialogs.ForEach(platoonIds, subunitId => {
                templateSheet.Copy(After: wb.Worksheets.Last());
                ExcelWorksheet rsh = wb.Worksheets.Last();
                rsh.Name = Querying.GetSubunitName(dc, subunitId);
                Подразделение subunit =
                    dc.GetTable<Подразделение>()
                    .Where(s => s.Код == subunitId)
                    .ToListTimed().First();
                FormatActSheet(rsh, subunit, dc, actDate, gradeQuery);
            });
            templateSheet.Delete();
            wb.Saved = true;
            wb.Application.Visible = true;
            wb.Activate();
        }

        public static void FormatActSheet(
                ExcelWorksheet sh, 
                Подразделение subunit, 
                DataContext dc, 
                DateTime actDate, 
                IQueryable<Оценка> gradeQuery) {

            ExcelTemplates.ReplaceRange(sh, "Date", "$month$", ReadableTextUtil.GetMonthGenitive(actDate));
            ExcelTemplates.ReplaceRange(sh, "Date", "$year$", actDate.ToString("yyyy"));
            ExcelTemplates.AppendRange(sh, "ИмяПодразделения", subunit.ИмяКраткое);

            IEnumerable<Военнослужащий> comissionMembers =
                Querying.GetSubunitCommander(dc, subunit.Код).ToList()
                .Concat(Querying.GetPostsForSubunit(dc, subunit.Код, "Преподаватель"))
                .OrderBy(s => s.ФИО)
                .OrderBy(s => s.Звание.order)
                .Reverse();
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов1", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => s.Звание.Название, s => s.ФИО });
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов2", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => s.Звание.Название, s => "", s => "", s => s.ФИО });

            var gradeSets =
                Grades.GradeSets(dc, Grades.GetGradesForSubunit(dc, gradeQuery, subunit.Код)).OrderBy(gs => gs.soldier.ФИО)
                .Where(gs => GradeCalcIndividual.ДопускНаКлассностьКурсанты(gs));
            var vusList = gradeSets.Select(gs => gs.soldier.ВУС).Distinct();
            if (vusList.Count() > 1) {
                throw new Exception("Несколько возможных ВУСов: " + vusList.MkString());
            }
            ExcelTemplates.ReplaceRange(sh, "Заголовок", "$vus$", vusList.First());

            int i = 1;
            ExcelTemplates.WithTemplateRow(sh.GetRange("SoldierList"), gradeSets, displayProgress: true, format: (gradeSet, c) => {
                c.Value = i++;
                c.GetOffset(0, 1).Value = gradeSet.rank.Название;
                c.GetOffset(0, 3).Value = gradeSet.soldier.Фамилия;
                c.GetOffset(0, 4).Value = gradeSet.soldier.Имя;
                c.GetOffset(0, 5).Value = gradeSet.soldier.Отчество;
                c.GetOffset(0, 6).Value = gradeSet.soldier.ВУС;
                return true;
            });
        }
    }
}
