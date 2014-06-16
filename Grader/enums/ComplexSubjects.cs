using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.enums {
    public static class ComplexSubjects {
        public static List<string> complexSubjectNames = new List<string> {
            "ОВП (курсанты)",
            "ОБЩ (курсанты)",
            "ОБЩ (контракт)",
            "ОБЩ (урс)",
            "СП/ТП",
            "командирск.подгот."
        };

        public static bool IsComplexSubject(string subjectName) {
            return complexSubjectNames.Contains(subjectName);
        }
    }
}
