using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.model {
    public class RegisterIndex {
        List<TermReg> index = new List<TermReg>();

        public RegisterIndex() { }

        class TermReg {
            public int regId { get; set; }
            public string term { get; set; }
        }

        public void InitIndex(Entities et) {
            Program.ReportEvent("Starting register indexing...");
            var nameQuery =
                from register in et.Ведомость
                select new TermReg { term = register.Название, regId = register.Код };
            AddFromQuery(nameQuery);
            var surnameQuery =
                from register in et.Ведомость
                join record in et.ВедомостьЗапись on register.Код equals record.КодВедомости
                join soldier in et.Военнослужащий on record.КодВоеннослужащего equals soldier.Код
                select new TermReg { term = soldier.Фамилия, regId = register.Код };
            AddFromQuery(surnameQuery);
            var tagQuery =
                from register in et.Ведомость
                join tag in et.ВедомостьТег on register.Код equals tag.КодВедомости
                select new TermReg { term = tag.Тег, regId = register.Код };
            AddFromQuery(tagQuery);
            Program.ReportEvent(String.Format("Loaded register index ({0} elements)", index.Count));
        }

        void AddFromQuery(IQueryable<TermReg> query) {
            foreach (var ti in query) {
                ti.term = ti.term.ToLower();
                index.Add(ti);
            }
        }

        public void AddToIndex(Entities et, Register register) {
            index.Add(new TermReg { regId = register.id, term = register.name });
            var surnameQuery = 
                from record in et.ВедомостьЗапись
                where record.КодВедомости == register.id
                join soldier in et.Военнослужащий on record.КодВоеннослужащего equals soldier.Код
                select new TermReg { term = soldier.Фамилия, regId = register.id };
            AddFromQuery(surnameQuery);
            foreach (var tag in register.tags) {
                index.Add(new TermReg { regId = register.id, term = tag });
            }
        }

        public void RemoveFromIndex(int registerId) {
            index.RemoveAll(ti => ti.regId == registerId);
        }

        public void UpdateIndex(Entities et, Register register) {
            RemoveFromIndex(register.id);
            AddToIndex(et, register);
        }

        public HashSet<int> Search(string searchString) {
            string str = searchString.ToLower();
            HashSet<int> registerIds = new HashSet<int>();
            foreach (var ti in index) {
                if (ti.term.Contains(str)) {
                    registerIds.Add(ti.regId);
                }
            }
            return registerIds;
        }
    }
}
