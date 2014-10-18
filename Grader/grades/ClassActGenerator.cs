using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using LibUtil;

namespace Grader.grades {
    public static class ClassActGenerator {
        public static void GenerateClassAct(Entities et, Settings settings, Подразделение subunit, DateTime actDate, IQueryable<Оценка> gradeQuery) {
            var gradeSets =
                Grades.GradeSets(et, Grades.GetGradesForSubunit(et, gradeQuery, subunit.Код)).OrderBy(gs => gs.soldier.ФИО())
                .Where(gs => GradeCalcIndividual.ДопускНаКлассностьКурсанты(gs));
            
            if (gradeSets.Count() > 0) {
                ExcelWorkbook wb = ExcelTemplates.LoadExcelTemplate(settings.GetTemplateLocation("акт_на_классность.xlsx"));
                ExcelWorksheet sh = wb.Worksheets.First();
                FormatActSheet(sh, subunit, et, actDate, gradeSets);
                wb.Saved = true;
                ExcelTemplates.ActivateExcel(sh);
            } else {
                System.Windows.Forms.MessageBox.Show("Нет оценок!");
                return;
            }
        }

        public static void GenerateAllClassActs(Entities et, Settings settings, DateTime actDate, IQueryable<Оценка> gradeQuery) {
            var wb = ExcelTemplates.LoadExcelTemplate(settings.GetTemplateLocation("акт_на_классность.xlsx"));
            ExcelWorksheet templateSheet = wb.Worksheets.First();
            var platoonIds =
                Querying.GetSubunitsByType(et, "взвод")
                .Where(s => s.ТипОбучения == "срочники")
                .Select(s => s.Код)
                .ToList();
            ProgressDialogs.ForEach(platoonIds, subunitId => {
                var gradeSets =
                    Grades.GradeSets(et, Grades.GetGradesForSubunit(et, gradeQuery, subunitId)).OrderBy(gs => gs.soldier.ФИО())
                    .Where(gs => GradeCalcIndividual.ДопускНаКлассностьКурсанты(gs));
                Подразделение subunit = et.subunitIdToInstance[subunitId];
                var vusList = gradeSets.Select(gs => gs.soldier.ВУС).Distinct();
                if (vusList.Count() > 2) {
                    Console.WriteLine("Several possible vuses for {0}, won't generate act", subunit.ИмяКраткое);
                }
                if (gradeSets.Count() > 0 && vusList.Count() == 1) {
                    templateSheet.Copy(After: wb.Worksheets.Last());
                    ExcelWorksheet rsh = wb.Worksheets.Last();
                    rsh.Name = Querying.GetSubunitName(et, subunitId);
                    
                    FormatActSheet(rsh, subunit, et, actDate, gradeSets);
                }
            });
            templateSheet.Delete();
            wb.Saved = true;
            wb.Application.Visible = true;
            wb.Activate();
        }

        public static void FormatActSheet(
                ExcelWorksheet sh, 
                Подразделение subunit, 
                Entities et,
                DateTime actDate, 
                IEnumerable<GradeSet> gradeSets) {

            ExcelTemplates.ReplaceRange(sh, "Date", "$month$", ReadableTextUtil.GetMonthGenitive(actDate));
            ExcelTemplates.ReplaceRange(sh, "Date", "$year$", actDate.ToString("yyyy"));
            ExcelTemplates.AppendRange(sh, "ИмяПодразделения", subunit.ИмяКраткое);

            IEnumerable<Военнослужащий> comissionMembers =
                Querying.GetSubunitCommander(et, subunit.Код).ToList()
                .Concat(Querying.GetPostsForSubunit(et, subunit.Код, "Преподаватель"))
                .OrderBy(s => s.ФИО())
                .OrderBy(s => s.Звание.order)
                .Reverse();
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов1", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => et.rankIdToName[s.КодЗвания], s => s.ФИО() });
            ExcelTemplates.InsertPlainListMulti(sh, "СписокЧленов2", comissionMembers,
                new List<Func<Военнослужащий, string>> { s => et.rankIdToName[s.КодЗвания], s => "", s => "", s => s.ФИО() });
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
