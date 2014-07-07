using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.util {
    public static class Common {
        public static string ФИО(this Военнослужащий v) {
            char name = v.Имя.Length > 0 ? v.Имя[0] : ' ';
            char patronymic = v.Отчество.Length > 0 ? v.Отчество[0] : ' ';
            return v.Фамилия + " " + name + "." + patronymic + ".";
        }

        public static string GetFullName(this Военнослужащий v, Entities et) {
            Звание rank = et.rankCache.Find(r => r.Код == v.КодЗвания);
            return rank + " " + v.ФИО();
        }
    }
}
