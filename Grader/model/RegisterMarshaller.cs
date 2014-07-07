using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil;

namespace Grader.model {
    public class RegisterMarshaller {
        public static Register LoadRegister(int id, Entities et) {
            Ведомость r = et.Ведомость.Where(reg => reg.Код == id).First();
            List<string> tags = et.ВедомостьТег.Where(t => t.КодВедомости == id).Select(t => t.Тег).ToList();
            List<int> subjectIds = et.ВедомостьПредмет.Where(s => s.КодВедомости == id).OrderBy(s => s.Порядок).Select(s => s.КодПредмета).ToList();
            List<RegisterRecord> records = (
                from rec in et.ВедомостьЗапись
                where rec.КодВедомости == id
                join v in et.Военнослужащий on rec.КодВоеннослужащего equals v.Код
                orderby rec.Порядок
                select new RegisterRecord { soldierId = v.Код }).ToList();
            foreach (var record in records) {
                record.marks = new List<Оценка>();
            }
            foreach (Оценка g in et.Оценка.Where(g => g.КодВедомости == id).ToList()) {
                records.FindOption(rec => rec.soldierId == g.КодПроверяемого).ForEach(rec => {
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
                virt = (r.Виртуальная),
                enabled = (r.Включена),
                subjectIds = subjectIds,
                records = records
            };
        }

        public static void SaveRegister(Register register, Entities et) {
            if (register.id != -1) {
                DeleteRegister(register.id, et);
            }
            int rid = register.id;
            if (register.id == -1) {
                // generate next id for register
                List<int> registerIds = et.Ведомость.Select(v => v.Код).ToList();
                if (registerIds.Count > 0) {
                    rid = registerIds.Max() + 1;
                } else {
                    rid = 1;
                }
            }

            List<Action> commands = new List<Action>();
            
            // save the register

            et.Ведомость.AddObject(new Ведомость {
                Код = rid,
                Название = register.name,
                ДатаЗаполнения = register.fillDate,
                ДатаВнесения = register.importDate.Value,
                ДатаИзменения = register.editDate.Value,
                Виртуальная = register.virt,
                Включена = register.enabled
            });

            
            foreach (string tag in register.tags) {
               et.ВедомостьТег.AddObject(new ВедомостьТег {
                    Тег = tag,
                    КодВедомости = rid
                });
            }

            int subjectOrder = 1;
            foreach (int subjectId in register.subjectIds) {
                et.ВедомостьПредмет.AddObject(new ВедомостьПредмет {
                    КодПредмета = subjectId,
                    КодВедомости = rid,
                    Порядок = subjectOrder++
                });
            }

            int recordOrder = 1;
            foreach (RegisterRecord record in register.records) {
                et.ВедомостьЗапись.AddObject(new ВедомостьЗапись {
                    КодВоеннослужащего = record.soldierId,
                    КодВедомости = rid,
                    Порядок = recordOrder++
                });
            }

            foreach (RegisterRecord record in register.records) {
                foreach (Оценка grade in record.marks) {
                    grade.КодВедомости = rid;
                    et.Оценка.AddObject(grade);
                }
            }

            et.SaveChanges();
        }

        public static void DeleteRegister(int rid, Entities et) {
            var registerToDelete = et.Ведомость.Where(r => r.Код == rid).First();
            et.Ведомость.DeleteObject(registerToDelete);
            et.SaveChanges();
        }
    }
}
