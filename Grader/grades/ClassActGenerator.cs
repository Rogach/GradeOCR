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
        public static void GenerateClassAct(AccessApplication accessApp) {
            var f = accessApp.GetForm("ПоОценкам");
            int subunitId = accessApp.GetForm("ПоОценкам").Get().GetControl("SubunitSelect").Get().IntegerValue();
            ExcelWorkbook wb = ExcelTemplates.LoadExcelTemplate(accessApp.Template("акт_на_классность.xlsx"));
            ExcelWorksheet sh = wb.Worksheets.First();
            FormatActSheet(sh, subunitId, accessApp);
            wb.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }

        public static void GenerateAllClassActs(AccessApplication accessApp) {
            var wb = ExcelTemplates.LoadExcelTemplate(accessApp.Template("акт_на_классность.xlsx"));
            ExcelWorksheet templateSheet = wb.Worksheets.First();
            DataContext dc = accessApp.GetDataContext();
            var platoonIds =
                Querying.GetSubunitsByType(dc, "взвод")
                .Where(s => s.ТипОбучения == "срочники")
                .Select(s => s.Код);
            ProgressDialogs.ForEach(platoonIds, subunitId => {
                templateSheet.Copy(After: wb.Worksheets.Last());
                ExcelWorksheet rsh = wb.Worksheets.Last();
                rsh.Name = Querying.GetSubunitName(dc, subunitId);
                FormatActSheet(rsh, subunitId, accessApp);
            });
            templateSheet.Delete();
            wb.Saved = true;
            wb.Application.Visible = true;
            wb.Activate();
        }

        public static void FormatActSheet(ExcelWorksheet sh, int subunitId, AccessApplication accessApp) {
            DataContext dc = accessApp.GetDataContext();
            var f = accessApp.GetForm("ПоОценкам").Get();
            DateTime actDate = f.GetControl("SelectDateTo").Get().DateTimeValue();
            IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);
            Подразделение subunit = dc.GetTable<Подразделение>().Where(s => s.Код == subunitId).ToListTimed().First();

            ExcelTemplates.ReplaceRange(sh, "Date", "$month$", ReadableTextUtil.GetMonthGenitive(actDate));
            ExcelTemplates.ReplaceRange(sh, "Date", "$year$", actDate.ToString("yyyy"));
            ExcelTemplates.AppendRange(sh, "ИмяПодразделения", subunit.ИмяКраткое);

            IEnumerable<Военнослужащий> comissionMembers =
                Querying.GetSubunitCommander(dc, subunitId).ToList()
                .Concat(Querying.GetPostsForSubunit(dc, subunitId, "Преподаватель"))
                .OrderBy(s => s.ФИО)
                .OrderBy(s => s.Звание.order)
                .Reverse();
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов1", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => s.Звание.Название, s => s.ФИО });
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов2", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => s.Звание.Название, s => "", s => "", s => s.ФИО });

            var gradeSets =
                Grades.GradeSets(dc, Grades.GetGradesForSubunit(dc, gradeQuery, subunitId)).OrderBy(gs => gs.soldier.ФИО)
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
