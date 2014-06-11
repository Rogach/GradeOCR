using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.model {
    public class Register {
        public long id { get; set; }
        public DateTime fillDate { get; set; }
        public DateTime? importDate { get; set; }
        public DateTime? editDate { get; set; }
        public string name { get; set; }
        public List<string> tags { get; set; }
        public bool virt { get; set; }
        public bool enabled { get; set; }
        public List<string> subjects { get; set; }
        public List<RegisterRecord> records { get; set; }
    }

    public class RegisterRecord {
        public Военнослужащий soldier { get; set; }
        public Dictionary<string, Mark> marks { get; set; }
    }

    public interface Mark {
    }

    public class Grade : Mark {
        public int value { get; set; }
    }
    public class Comment : Mark {
        public string comment { get; set; }
    }
}
