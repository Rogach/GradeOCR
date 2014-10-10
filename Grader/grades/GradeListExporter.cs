using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using LibUtil.wrapper.excel;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using LibUtil;

namespace Grader.grades {
    public static class GradeListExporter {
        class ExportUnit {
            public string subunitName { get; set; }
            public bool exactSubunit { get; set; }
            public string sheetName { get; set; }
            public string rangeName { get; set; }
        }

        static List<ExportUnit> contractExports = new List<ExportUnit> {
            new ExportUnit { subunitName = "упр", exactSubunit = true, sheetName = "Упр", rangeName = "insert_упр" },
                        
            new ExportUnit { subunitName = "1ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_1ц" },
            new ExportUnit { subunitName = "2ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_2ц" },
            new ExportUnit { subunitName = "3ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_3ц" },
            new ExportUnit { subunitName = "4ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_4ц" },
            new ExportUnit { subunitName = "5ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_5ц" },
            new ExportUnit { subunitName = "6ц", exactSubunit = true, sheetName = "Циклы", rangeName = "insert_6ц" },

            new ExportUnit { subunitName = "1 бат", exactSubunit = true, sheetName = "1б", rangeName = "insert_1бат" },
            new ExportUnit { subunitName = "11", exactSubunit = false, sheetName = "1б", rangeName = "insert_11" },
            new ExportUnit { subunitName = "12", exactSubunit = false, sheetName = "1б", rangeName = "insert_12" },
            new ExportUnit { subunitName = "13", exactSubunit = false, sheetName = "1б", rangeName = "insert_13" },

            new ExportUnit { subunitName = "2 бат", exactSubunit = true, sheetName = "2б", rangeName = "insert_2бат" },
            new ExportUnit { subunitName = "21", exactSubunit = false, sheetName = "2б", rangeName = "insert_21" },
            new ExportUnit { subunitName = "22", exactSubunit = false, sheetName = "2б", rangeName = "insert_22" },
            new ExportUnit { subunitName = "23", exactSubunit = false, sheetName = "2б", rangeName = "insert_23" },

            new ExportUnit { subunitName = "3 бат", exactSubunit = true, sheetName = "3б", rangeName = "insert_3бат" },
            new ExportUnit { subunitName = "31", exactSubunit = false, sheetName = "3б", rangeName = "insert_31" },
            new ExportUnit { subunitName = "32", exactSubunit = false, sheetName = "3б", rangeName = "insert_32" },
            new ExportUnit { subunitName = "33", exactSubunit = false, sheetName = "3б", rangeName = "insert_33" },

            new ExportUnit { subunitName = "УРС", exactSubunit = true, sheetName = "УРС", rangeName = "insert_УРС" },
                        
            new ExportUnit { subunitName = "БОУП", exactSubunit = true, sheetName = "БОУП", rangeName = "insert_БОУП" },
            new ExportUnit { subunitName = "РС", exactSubunit = true, sheetName = "БОУП", rangeName = "insert_РС" },
            new ExportUnit { subunitName = "РО", exactSubunit = true, sheetName = "БОУП", rangeName = "insert_РО" }
        };

        public static void ContractGradeListExport(
                Entities et,
                Settings settings,
                IQueryable<Оценка> gradeQuery,
                IQueryable<Военнослужащий> soldierQuery) {
            DoContractGradeListExport(
                et,
                settings,
                gradeQuery,
                soldierQuery,
                "список_оценок_контрактники.xlsx",
                new List<string> { "ТСП", "СП", "ТП", "ФП", "РХБЗ", "МП", "ОГН", "СТР", "ОВУ", "ОГП", "АВТ", "ВМП", "ИНЖ", "ПОЖ", "МОБ", "ОБВС", "ОЗГТ", "ТАК", "ТОП" },
                contractExports
            );
        }

        static void DoContractGradeListExport(
                Entities et,
                Settings settings,
                IQueryable<Оценка> gradeQuery, 
                IQueryable<Военнослужащий> soldierQuery,
                string templateName, 
                List<string> subjects, List<ExportUnit> exports) {

            var wb = ExcelTemplates.LoadExcelTemplate(settings.GetTemplateLocation(templateName));

            ExcelWorksheet mainSheet = wb.Worksheets.Find(s => s.Name == "Главная");
            mainSheet.GetRange("CURRENT_YEAR").Value = DateTime.Now.Year;
            mainSheet.GetRange("CURRENT_MONTH").Value = ReadableTextUtil.GetMonthName(DateTime.Now);
            ProgressDialogs.ForEach(exports, e => {
                ExcelWorksheet sh = wb.Worksheets.ToList().Find(s => s.Name == e.sheetName);
                var subunitId = (from s in et.Подразделение where s.ИмяКраткое == e.subunitName select s.Код).First();

                var soldiers = (e.exactSubunit ?
                    Querying.GetSubunitSoldiersExact(et, subunitId, soldierQuery) :
                    Querying.GetSubunitSoldiers(et, subunitId, soldierQuery)).ToList();
                IQueryable<Оценка> allGradesQuery =
                    (e.exactSubunit ?
                    Grades.GetGradesForSubunitExact(et, gradeQuery, subunitId) :
                    Grades.GetGradesForSubunit(et, gradeQuery, subunitId));
                allGradesQuery = 
                    from g in allGradesQuery
                    join r in et.Ведомость on g.КодВедомости equals r.Код
                    orderby r.ДатаЗаполнения
                    select g;
                
                List<Оценка> allGrades = allGradesQuery.ToList().ApplyOverriding();
                var grades = allGrades.GroupBy(g => g.КодПроверяемого).ToDictionary(t => t.Key, t => t.ToList());

                List<int> gradeSoldierIds = grades.Keys.Where(id => soldiers.Find(v => v.Код == id) == default(Военнослужащий)).ToList();
                List<Военнослужащий> gradeSoldiers = et.Военнослужащий.Where(v => gradeSoldierIds.Contains(v.Код)).ToList();
                soldiers.AddRange(gradeSoldiers);
                soldiers = (
                    from s in soldiers
                    orderby s.sortWeight descending, et.rankIdToOrder[s.КодЗвания] descending, s.Фамилия, s.Имя, s.Отчество
                    select s
                ).ToList();

                Dictionary<int, Ведомость> registerCache =
                    (from g in allGradesQuery
                     join r in et.Ведомость on g.КодВедомости equals r.Код
                     select r).Distinct().ToList().ToDictionary(r => r.Код);

                int c = 1;
                List<Военнослужащий> realSoldiers = soldiers.ToList().Where(s => s.КодЗвания != et.rankNameToId["ГП"]).ToList();
                ExcelRange exportRange = sh.GetRange(e.rangeName);
                ExcelRange subjectRange = exportRange.GetOffset(0, 2).GetOffset(1, 0);
                List<int> soldierSummaryGrades = new List<int>();
                
                ExcelTemplates.WithTemplateRow(exportRange, realSoldiers, displayProgress: true,
                    format: (s, rng) => {
                        var r = rng;
                        r.GetOffset(0, -1).Value = c++;
                        r.Value = et.rankIdToName[s.КодЗвания];
                        r.GetOffset(0, 1).Value = s.ФИО();

                        grades.GetOption(s.Код).ForEach(gl => {
                            r = r.GetOffset(0, 2);
                            GradeSet gs = new GradeSet(null, null, et.subunitIdToInstance[gl.First().КодПодразделения], DateTime.Now);
                            foreach (string subj in subjects) {
                                gl.Where(g => g.КодПредмета == et.subjectNameToId[subj]).ToList().LastOption().ForEach(g => {
                                    if (g.ЭтоКомментарий) {
                                        r.Value = g.Текст;
                                    } else {
                                        r.Value = g.Значение;
                                        gs.AddGrade(subj, g.Значение);
                                    }
                                    if (registerCache[g.КодВедомости].Виртуальная) {
                                        r.BackgroundColor = ExcelEnums.Color.PaleVioletRed;
                                    }
                                });
                                r = r.GetOffset(0, 1);
                            }
                            GradeCalcIndividual.ОценкаКонтрактникиОБЩ(gs).ForEach(summGrade => {
                                soldierSummaryGrades.Add(summGrade);
                                r.Value = summGrade;
                            });
                        });
                        return true;
                });

                Dictionary<string, int> subjectSummaryGrades = new Dictionary<string, int>();
                foreach (string subj in subjects) {
                    subjectRange.NumberFormat = "0.00";
                    List<int> subjectGrades = allGrades
                        .Where(g => g.КодПредмета == et.subjectNameToId[subj])
                        .Where(g => !g.ЭтоКомментарий)
                        .Select(g => (int) g.Значение)
                        .ToList();
                    if (subjectGrades.Count > 0) {
                        subjectRange.Value = subjectGrades.Average();
                        GradeCalcGroup.ФормулаПостоянныйСоставПоПредмету(subjectGrades).ForEach(summaryGrade => {
                            subjectSummaryGrades.Add(subj, summaryGrade);
                            subjectRange.GetOffset(1, 0).Value = summaryGrade;
                        });
                    }
                    subjectRange = subjectRange.GetOffset(0, 1);
                }

                if (soldierSummaryGrades.Count > 0) {
                    subjectRange.NumberFormat = "0.00";
                    subjectRange.Value = soldierSummaryGrades.Average();
                }
                GradeCalcGroup.БоеваяПодготовкаЗаПодразделение(subjectSummaryGrades, et.subunitIdToInstance[subunitId]).ForEach(subunitSummaryGrade => {
                    subjectRange.GetOffset(1, 0).Value = subunitSummaryGrade;
                }); 
            });
            ExcelTemplates.ActivateExcel(wb);
        }
    }
}

