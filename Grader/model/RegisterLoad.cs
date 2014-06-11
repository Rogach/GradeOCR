using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil;

namespace Grader.model {
    public class RegisterLoad {
        public static Register LoadRegister(int id, DataContext dc) {
            Ведомость r = dc.GetTable<Ведомость>().Where(reg => reg.Код == id).ToList().First();
            List<string> tags = dc.GetTable<ВедомостьТег>().Where(t => t.КодВедомости == id).Select(t => t.Тег).ToList();
            List<int> subjectIds = dc.GetTable<ВедомостьПредмет>().Where(s => s.КодВедомости == id).OrderBy(s => s.Порядок).Select(s => s.КодПредмета).ToList();
            List<RegisterRecord> records = (
                from rec in dc.GetTable<ВедомостьЗапись>()
                where rec.КодВедомости == id
                join v in dc.GetTable<Военнослужащий>() on rec.КодВоеннослужащего equals v.Код
                select new RegisterRecord { marks = new List<Оценка>(), soldier = v }).ToList();
            foreach (Оценка g in dc.GetTable<Оценка>().Where(g => g.КодВедомости == id).ToList()) {
                records.FindOption(rec => rec.soldier.Код == g.КодПроверяемого).ForEach(rec => {
                    rec.marks.Add(g);
                });
            }
            return new Register {
                id = r.Код,
                name = r.Название,
                fillDate = r.ДатаЗаполнения,
                importDate = r.ДатаВнесения,
                editDate = r.ДатаИзменения,
                tags = tags,
                virt = r.Виртуальная,
                enabled = r.Включена,
                subjectIds = subjectIds,
                records = records
            };
        }

        public static void SaveRegister(Register register, DataContext dc) {
            if (register.id != -1) {
                // delete all records about old register
                dc.ExecuteCommand("delete from ВедомостьТег where КодВедомости = @p0", register.id);
                dc.ExecuteCommand("delete from ВедомостьПредмет where КодВедомости = @p0", register.id);
                dc.ExecuteCommand("delete from ВедомостьЗапись where КодВедомости = @p0", register.id);
                dc.ExecuteCommand("delete from Оценка where КодВедомости = @p0", register.id);
                dc.ExecuteCommand("delete from Ведомость where Код = @p0", register.id);
            }
            int rid = register.id;
            if (register.id == -1) {
                // generate next id for register
                List<int> registerIds = dc.GetTable<Ведомость>().Select(v => v.Код).ToList();
                if (registerIds.Count > 0) {
                    rid = registerIds.Max() + 1;
                } else {
                    rid = 1;
                }
            }
            // save the register
            dc.ExecuteCommand(@"insert into Ведомость(Код, Название, ДатаЗаполнения, ДатаВнесения, ДатаИзменения, Виртуальная, Включена) 
                                values (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                              rid, register.name,
                              SqlTime(register.fillDate), SqlTime(register.importDate.Value), SqlTime(register.editDate.Value),
                              register.virt, register.enabled);
            foreach (string tag in register.tags) {
                dc.ExecuteCommand(@"insert into ВедомостьТег(Тег, КодВедомости) values(@p0, @p1)", tag, rid);
            }
            int subjectOrder = 1;
            foreach (int subjectId in register.subjectIds) {
                dc.ExecuteCommand(@"insert into ВедомостьПредмет(КодПредмета, КодВедомости, Порядок) values (@p0, @p1, @p2)",
                                    subjectId, rid, subjectOrder++);
            }
            int recordOrder = 1;
            foreach (RegisterRecord record in register.records) {
                dc.ExecuteCommand(@"insert into ВедомостьЗапись(КодВоеннослужащего, КодВедомости, Порядок) values (@p0, @p1, @p2)",
                                    record.soldier.Код, rid, recordOrder++);
            }
            foreach (RegisterRecord record in register.records) {
                foreach (Оценка grade in record.marks) {
                    dc.ExecuteCommand(@"insert into Оценка(
                                            КодПроверяемого, КодПредмета, ЭтоКомментарий, Значение, 
                                            Текст, КодПодразделения, ВУС, ТипВоеннослужащего, 
                                            КодЗвания, КодВедомости)
                                        values (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)",
                                        grade.КодПроверяемого, grade.КодПредмета, grade.ЭтоКомментарий,
                                        grade.Значение, grade.Текст, grade.КодПодразделения,
                                        grade.ВУС, grade.ТипВоеннослужащего, grade.КодЗвания, grade.КодВедомости);
                }
            }
        }

        private static string SqlTime(DateTime t) {
            return t.ToString("dd.MM.yyyy HH:mm:ss");
        }
    }
}
