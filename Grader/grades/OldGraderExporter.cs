using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using Grader.util;
using LibUtil.templates;
using LibUtil.wrapper.excel;
using LibUtil;

namespace Grader.grades {
    public static class OldGraderExporter {

        class ExportUnit {
            public string subunitName { get; set; }
            public bool exactSubunit { get; set; }
            public string sheetName { get; set; }
            public string rangeName { get; set; }
        }

        static List<ExportUnit> cadetExports = new List<ExportUnit> {
            new ExportUnit { subunitName = "111", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_111" },
            new ExportUnit { subunitName = "112", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_112" },
            new ExportUnit { subunitName = "113", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_113" },
            new ExportUnit { subunitName = "114", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_114" },
            new ExportUnit { subunitName = "121", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_121" },
            new ExportUnit { subunitName = "122", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_122" },
            new ExportUnit { subunitName = "123", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_123" },
            new ExportUnit { subunitName = "124", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_124" },
            new ExportUnit { subunitName = "131", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_131" },
            new ExportUnit { subunitName = "132", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_132" },
            new ExportUnit { subunitName = "133", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_133" },
            new ExportUnit { subunitName = "134", exactSubunit = true, sheetName = "Учет 1б", rangeName = "insert_134" },
            new ExportUnit { subunitName = "211", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_211" },
            new ExportUnit { subunitName = "212", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_212" },
            new ExportUnit { subunitName = "213", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_213" },
            new ExportUnit { subunitName = "214", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_214" },
            new ExportUnit { subunitName = "221", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_221" },
            new ExportUnit { subunitName = "222", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_222" },
            new ExportUnit { subunitName = "223", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_223" },
            new ExportUnit { subunitName = "224", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_224" },
            new ExportUnit { subunitName = "231", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_231" },
            new ExportUnit { subunitName = "232", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_232" },
            new ExportUnit { subunitName = "233", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_233" },
            new ExportUnit { subunitName = "234", exactSubunit = true, sheetName = "Учет 2б", rangeName = "insert_234" },
            new ExportUnit { subunitName = "311", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_311" },
            new ExportUnit { subunitName = "312", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_312" },
            new ExportUnit { subunitName = "313", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_313" },
            new ExportUnit { subunitName = "314", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_314" },
            new ExportUnit { subunitName = "321", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_321" },
            new ExportUnit { subunitName = "322", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_322" },
            new ExportUnit { subunitName = "323", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_323" },
            new ExportUnit { subunitName = "324", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_324" },
            new ExportUnit { subunitName = "331", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_331" },
            new ExportUnit { subunitName = "332", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_332" },
            new ExportUnit { subunitName = "333", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_333" },
            new ExportUnit { subunitName = "334", exactSubunit = true, sheetName = "Учет 3б", rangeName = "insert_334" }
        };

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

            new ExportUnit { subunitName = "РС", exactSubunit = true, sheetName = "БОУП", rangeName = "insert_РС" },
            new ExportUnit { subunitName = "РО", exactSubunit = true, sheetName = "БОУП", rangeName = "insert_РО" }
        };

        public static void ExportToOldGrader(
                Entities et,
                Settings settings,
                IQueryable<Оценка> gradeQuery, 
                IQueryable<Военнослужащий> soldierQuery, 
                bool selectCadets, 
                bool selectContract) {

            if (selectCadets) {
                DoExport(
                    et,
                    settings,
                    gradeQuery,
                    soldierQuery,
                    "старая_учетка_курсанты.xlsm",
                    new List<string> { "СП", "СЭС", "ТП", "СТР", "ФП", "ОВУ", "ОГН", "РХБЗ", "ВМП", "ОГП", "ТАК" },
                    cadetExports
                );
            } else if (selectContract) {
                DoExport(
                    et,
                    settings,
                    gradeQuery,
                    soldierQuery,
                    "старая_учетка_контрактники.xlsx",
                    new List<string> { "ТСП", "СП", "ТП", "ФП", "РХБЗ", "МП", "ОГН", "СТР", "ОВУ", "ОГП" },
                    contractExports
                );
            } else {
                throw new Exception("Unable to export this soldier type - no such old grader found");
            }
        }

        static void DoExport(
                Entities et,
                Settings settings,
                IQueryable<Оценка> gradeQuery, 
                IQueryable<Военнослужащий> soldierQuery,
                string templateName, 
                List<string> subjects, 
                List<ExportUnit> exports) {
                
            var wb = ExcelTemplates.LoadExcelTemplate(settings.GetTemplateLocation(templateName));
            ProgressDialogs.ForEach(exports, e => {
                try {
                    ExcelWorksheet sh = wb.Worksheets.ToList().Find(s => s.Name == e.sheetName);
                    ExcelRange c = sh.GetRange(e.rangeName);
                    var subunitId = (from s in et.Подразделение where s.ИмяКраткое == e.subunitName select s.Код).First();
                    var soldiers = e.exactSubunit ?
                        Querying.GetSubunitSoldiersExact(et, subunitId, soldierQuery) :
                        Querying.GetSubunitSoldiers(et, subunitId, soldierQuery);
                    var grades = Grades.GradeSets(et,
                        e.exactSubunit ?
                            Grades.GetGradesForSubunitExact(et, gradeQuery, subunitId) :
                            Grades.GetGradesForSubunit(et, gradeQuery, subunitId))
                        .ToDictionary(g => g.soldier.Код);

                    int gpId = et.rankNameToId["ГП"];
                    List<Военнослужащий> realSoldiers = soldiers.Where(s => s.КодЗвания != gpId).ToList();
                    ProgressDialogs.ForEach(realSoldiers, s => {
                        var r = c;
                        r.Value = et.rankCache.Find(rk => rk.Код == s.КодЗвания).Название;
                        r.GetOffset(0, 1).Value = s.ФИО();
                        grades.GetOption(s.Код).ForEach(g => {
                            r = r.GetOffset(0, 2);
                            foreach (string subj in subjects) {
                                g.grades.GetOption(subj).ForEach(v => {
                                    r.Value = v;
                                });
                                r = r.GetOffset(0, 1);
                            }
                        });
                        c = c.GetOffset(1, 0);
                    });
                } catch (Exception ex) {
                    Logger.Log(ex.ToString());
                }
            });
            ExcelTemplates.ActivateExcel(wb);
        }
        
    }
}
