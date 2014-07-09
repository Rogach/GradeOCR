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
        public Dictionary<int, string> subunitIdToName;
        public Dictionary<string, int> subunitNameToId;
        public Dictionary<int, Подразделение> subunitIdToInstance;

        public void initCache() {
            subunitRelCache = ПодразделениеПодчинение.ToList();
            rankCache = Звание.ToList();
            subunitCache = Подразделение.ToList();
            subjectNameToId = Предмет.Select(s => new { id = s.Код, name = s.Название }).ToList().ToDictionary(s => s.name, s => s.id);
            subjectIdToName = Предмет.Select(s => new { id = s.Код, name = s.Название }).ToList().ToDictionary(s => s.id, s => s.name);
            rankIdToName = Звание.Select(r => new { id = r.Код, rank = r.Название }).ToList().ToDictionary(r => r.id, r => r.rank);
            rankNameToId = Звание.Select(r => new { id = r.Код, rank = r.Название }).ToList().ToDictionary(r => r.rank, r => r.id);
            subunitIdToName = Подразделение.Select(s => new { id = s.Код, name = s.Имя }).ToList().ToDictionary(s => s.id, s => s.name);
            subunitNameToId = Подразделение.Select(s => new { id = s.Код, name = s.Имя }).ToList().ToDictionary(s => s.name, s => s.id);
            subunitIdToInstance = Подразделение.ToList().ToDictionary(s => s.Код, s => s);
        }

    }

    public partial class Подразделение {
        public override string ToString() {
            return this.Имя;
        }
    }
}
