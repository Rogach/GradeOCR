using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grader.util {
    public static class Common {
        public static string ФИО(this Военнослужащий v) {
            return v.Фамилия + " " + (v.Имя.Length > 0 ? v.Имя.Substring(0, 1) : " ") + "." + (v.Отчество.Length > 0 ? v.Отчество.Substring(0, 1) : " ") + ".";
        }

        public static string GetFullName(this Военнослужащий v, Entities et) {
            Звание rank = et.rankCache.Find(r => r.Код == v.КодЗвания);
            return rank.Название + " " + v.ФИО();
        }
    }
}
