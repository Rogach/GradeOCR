using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;
using LibUtil.templates;
using LibUtil.wrapper.excel;

namespace Grader.grades {
    class ReleaseOrderTableGenerator {
        public static void GenerateReleaseOrderTable(Entities et, IQueryable<Оценка> gradeQuery) {
            var gradeSets = Grades.GradeSets(et, gradeQuery);
            
            ExcelWorksheet sh = ExcelTemplates.CreateEmptyExcelTable();
            ExcelRange rng = sh.GetRange("A1");

            rng.GetOffset(0, 1).Value = "ОТЛИЧНО";
            rng = rng.GetOffset(1, 0);
            rng = OutputSoldiers(rng, gradeSets.Where(gs => GradeCalcIndividual.ОценкаКурсантыОБЩ(gs).GetOrElse(0) == 5).ToList(), genitive: true);

            rng.GetOffset(0, 1).Value = "ХОРОШО";
            rng = rng.GetOffset(1, 0);
            rng = OutputSoldiers(rng, gradeSets.Where(gs => GradeCalcIndividual.ОценкаКурсантыОБЩ(gs).GetOrElse(0) == 4).ToList(), genitive: true);

            rng.GetOffset(0, 1).Value = "УДОВЛЕТВОРИТЕЛЬНО";
            rng = rng.GetOffset(1, 0);
            rng = OutputSoldiers(rng, gradeSets.Where(gs => GradeCalcIndividual.ОценкаКурсантыОБЩ(gs).GetOrElse(0) == 3).ToList(), genitive: true);

            rng.GetOffset(0, 1).Value = "НЕУДОВЛЕТВОРИТЕЛЬНО";
            rng = rng.GetOffset(1, 0);
            rng = OutputSoldiers(rng, gradeSets.Where(gs => GradeCalcIndividual.ОценкаКурсантыОБЩ(gs).GetOrElse(0) == 2).ToList(), genitive: true);

            sh.Workbook.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }

        public static void GenerateClassnostOrderTable(Entities et, IQueryable<Оценка> gradeQuery) {
            var gradeSets = Grades.GradeSets(et, gradeQuery).Where(gs => GradeCalcIndividual.КлассностьКурсанты(gs));
            
            ExcelWorksheet sh = ExcelTemplates.CreateEmptyExcelTable();
            ExcelRange rng = sh.GetRange("A1");

            gradeSets.GroupBy(gs => gs.soldier.ВУС).ToList().OrderBy(gs => gs.Key).ToList().ForEach(gss => {
                rng.GetOffset(0, 1).Value = "ВУС-" + gss.Key;
                rng = rng.GetOffset(1, 0);
                rng = OutputSoldiers(rng, gss.ToList(), genitive: false);
            });

            sh.Workbook.Saved = true;
            ExcelTemplates.ActivateExcel(sh);
        }

        public static ExcelRange OutputSoldiers(ExcelRange rng, List<GradeSet> gradeSets, bool genitive) {
            gradeSets.GroupBy(gs => gs.subunit.Имя).OrderBy(kv => kv.Key).ToList().ForEach(kv => {
                rng.GetOffset(0, 1).Value = kv.Key;
                rng = rng.GetOffset(1, 0);

                kv.ToList()
                    .OrderBy(gs => gs.soldier.Отчество)
                    .OrderBy(gs => gs.soldier.Имя)
                    .OrderBy(gs => gs.soldier.Фамилия).ToList()
                    .ForEach(s => {
                        if (genitive) {
                            rng.Value = "рядового";
                            rng.GetOffset(0, 1).Value = ReadableTextUtil.ФамилияРодительный(s.soldier.Фамилия);
                            rng.GetOffset(0, 2).Value = ReadableTextUtil.ИмяРодительный(s.soldier.Имя);
                            rng.GetOffset(0, 3).Value = ReadableTextUtil.ОтчествоРодительный(s.soldier.Отчество);
                        } else {
                            rng.Value = "рядовому";
                            rng.GetOffset(0, 1).Value = ReadableTextUtil.ФамилияДательный(s.soldier.Фамилия);
                            rng.GetOffset(0, 2).Value = ReadableTextUtil.ИмяДательный(s.soldier.Имя);
                            rng.GetOffset(0, 3).Value = ReadableTextUtil.ОтчествоДательный(s.soldier.Отчество);
                        }
                        rng = rng.GetOffset(1, 0);
                });
            });
            return rng;
        }
    }
}
