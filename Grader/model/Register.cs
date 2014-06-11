using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.model {
    public class Register {
        public int id { get; set; }
        public DateTime fillDate { get; set; }
        public DateTime? importDate { get; set; }
        public DateTime? editDate { get; set; }
        public string name { get; set; }
        public bool virt { get; set; }
        public bool enabled { get; set; }
        public List<string> tags { get; set; }
        public List<int> subjectIds { get; set; }
        public List<RegisterRecord> records { get; set; }
    }

    public class RegisterRecord {
        public Военнослужащий soldier { get; set; }
        public List<Оценка> marks { get; set; }
    }
}
