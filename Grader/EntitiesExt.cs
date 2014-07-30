using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader {
    public partial class Entities {
        public List<ПодразделениеПодчинение> subunitRelCache;
        public List<Звание> rankCache;
        public List<Подразделение> subunitCache;

        public Dictionary<string, int> subjectNameToId;
        public Dictionary<int, string> subjectIdToName;

        public Dictionary<int, string> rankIdToName;
        public Dictionary<string, int> rankNameToId;
        public Dictionary<int, int> rankIdToOrder;
        public Dictionary<string, int> rankNameToOrder;

        public Dictionary<int, string> subunitIdToName;
        public Dictionary<string, int> subunitNameToId;
        public Dictionary<string, int> subunitShortNameToId;
        public Dictionary<int, Подразделение> subunitIdToInstance;
        public Dictionary<int, string> subunitIdToShortName;

        public List<string> soldierNameCache;
        public Dictionary<int, string> soldierIdToName;
        public Dictionary<string, int> soldierNameToId;

        public void initCache() {
            subunitRelCache = ПодразделениеПодчинение.ToList();
            rankCache = Звание.ToList();
            subunitCache = Подразделение.ToList();
            subjectNameToId = Предмет.Select(s => new { id = s.Код, name = s.Название }).ToList().ToDictionary(s => s.name, s => s.id);
            subjectIdToName = Предмет.Select(s => new { id = s.Код, name = s.Название }).ToList().ToDictionary(s => s.id, s => s.name);
            rankIdToName = Звание.Select(r => new { id = r.Код, rank = r.Название }).ToList().ToDictionary(r => r.id, r => r.rank);
            rankNameToId = Звание.Select(r => new { id = r.Код, rank = r.Название }).ToList().ToDictionary(r => r.rank, r => r.id);
            rankIdToOrder = Звание.Select(r => new { id = r.Код, order = r.order }).ToList().ToDictionary(r => r.id, r => r.order);
            rankNameToOrder = Звание.Select(r => new { name = r.Название, order = r.order }).ToList().ToDictionary(r => r.name, r => r.order);

            subunitIdToName = Подразделение.Select(s => new { id = s.Код, name = s.Имя }).ToList().ToDictionary(s => s.id, s => s.name);
            subunitNameToId = Подразделение.Select(s => new { id = s.Код, name = s.Имя }).ToList().ToDictionary(s => s.name, s => s.id);
            subunitIdToInstance = Подразделение.ToList().ToDictionary(s => s.Код, s => s);
            subunitShortNameToId = Подразделение.Select(s => new { id = s.Код, name = s.ИмяКраткое }).ToList().ToDictionary(s => s.name, s => s.id);
            subunitIdToShortName = Подразделение.Select(s => new { id = s.Код, name = s.ИмяКраткое }).ToList().ToDictionary(s => s.id, s => s.name);

            soldierIdToName =
                (from v in Военнослужащий
                 join r in Звание on v.КодЗвания equals r.Код
                 select new {
                     id = v.Код,
                     name =
                         r.Название + " " +
                         v.Фамилия + " " +
                         (v.Имя.Length > 0 ? v.Имя.Substring(0, 1) : " ") + "." +
                         (v.Отчество.Length > 0 ? v.Отчество.Substring(0, 1) : " ") + "."
                 }).ToList().ToDictionary(v => v.id, v => v.name + " id" + v.id);
            soldierNameToId = soldierIdToName.ToList().ToDictionary(kv => kv.Value, kv => kv.Key);
            soldierNameCache = soldierNameToId.Keys.ToList();
        }

    }

    public partial class Подразделение {
        public override string ToString() {
            return this.Имя;
        }
    }
}
