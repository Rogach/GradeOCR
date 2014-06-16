using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grader.util;
using LibUtil;

namespace Grader.grades {
    public static class GradeCalcIndividual {
        public static Option<int> GetGrade(GradeSet gradeSet, string subjectName) {
            if (subjectName == "ОВП (курсанты)") {
                return ОценкаКурсантыОВП(gradeSet);
            } else if (subjectName == "ОБЩ (курсанты)") {
                return ОценкаКурсантыОБЩ(gradeSet);
            } else if (subjectName == "ОБЩ (контракт)") {
                return ОценкаКонтрактникиОБЩ(gradeSet);
            } else if (subjectName == "ОБЩ (урс)") {
                return ОценкаУрсОБЩ(gradeSet);
            } else if (subjectName == "СП/ТП") {
                return ОценкаСПТП(gradeSet);
            } else if (subjectName == "командирск.подгот.") {
                return КомандирскаяПодготовка(gradeSet);
            } else { 
                return gradeSet.grades.GetOption(subjectName);
            }
        }

        public static Option<int> ОценкаСПТП(GradeSet gradeSet) {
            var sp = gradeSet.grades.GetOption("СП");
            var tp = gradeSet.grades.GetOption("ТП");
            if (sp.NonEmpty() && tp.NonEmpty()) {
                return new Some<int>(new List<int> { sp.Get(), tp.Get() }.Min());
            } else if (sp.NonEmpty()) {
                return sp;
            } else if (tp.NonEmpty()) {
                return tp;
            } else {
                return new None<int>();
            }
        }

        public static Option<int> ОценкаКурсантыОВП(GradeSet gradeSet) {
            var ovpGrades = gradeSet.grades.Where(kv => kv.Key != "СП" && kv.Key != "СЭС" && kv.Key != "ТП").Select(kv => kv.Value);
            if (ovpGrades.Count() == 0) {
                return new None<int>();
            } else {
                Func<int, float> gradeCount = x => (float) ovpGrades.Where(g => g == x).Count() / ovpGrades.Count();
                if (gradeCount(2) != 0) {
                    return Options.Create(2);
                } else if (gradeCount(3) == 0 && gradeCount(5) >= gradeCount(4)) {
                    return Options.Create(5);
                } else if (gradeCount(4) + gradeCount(5) >= 0.7) {
                    return Options.Create(4);
                } else {
                    return Options.Create(3);
                }
            }
        }

        public static Option<int> ОценкаКурсантыОБЩ(GradeSet gradeSet) {
            if (gradeSet.grades.Count() == 0) {
                return new None<int>();
            } else {
                var sp = gradeSet.grades.GetOption("СП");
                var tp = gradeSet.grades.GetOption("ТП");
                Func<int, float> count = x => (float) gradeSet.grades.Where(g => g.Value == x).Count() / gradeSet.grades.Count;
                if (count(2) != 0) {
                    return Options.Create(2);
                } else if (count(3) == 0 && count(5) >= count(4) && sp.Map(g => g == 5).GetOrElse(true) && tp.Map(g => g == 5).GetOrElse(true)) {
                    return Options.Create(5);
                } else if (count(4) + count(5) >= 0.7 && sp.Map(g => g >= 4).GetOrElse(true) && tp.Map(g => g >= 4).GetOrElse(true)) {
                    return Options.Create(4);
                } else {
                    return Options.Create(3);
                }
            }
        }

        public static Option<int> ОценкаКонтрактникиВАЖНЫЕ(GradeSet gradeSet) {
            List<string> importantSubjects =
                gradeSet.subunit.ПодразделениеОхраны ?
                new List<string> { "ТСП", "СП", "ТП ", "ФП", "РХБЗ", "МП", "ОГН" } :
                new List<string> { "ТСП", "СП", "ТП", "ФП", "РХБЗ", "МП" };
            var importantGrades = importantSubjects.ConvertAll(s => gradeSet.grades.GetOption(s)).Flatten();
            var mobGrade = gradeSet.grades.GetOption("МОБ");
            if (mobGrade.Filter(mob => mob == 2).IsEmpty()) {
                if (importantGrades.Count() == 0) {
                    return new None<int>();
                } else {
                    return new Some<int>(importantGrades.Min());
                }
            } else {
                return new Some<int>(2);
            }
        }

        public static Option<int> ОценкаКонтрактникиОБЩ(GradeSet gradeSet) {
            var grades = gradeSet.grades.Where(g => g.Key != "МОБ").ToList();
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> count = x => (float) grades.Where(g => g.Value == x).Count() / grades.Count;
                int summGrade;
                if (count(3) == 0 && count(2) == 0 && count(5) >= count(4)) {
                    summGrade = 5;
                } else if (count(4) + count(5) >= 0.5 && count(2) == 0) {
                    summGrade = 4;
                } else if (count(2) < 0.3) {
                    summGrade = 3;
                } else {
                    summGrade = 2;
                }
                return Options.Create(ОценкаКонтрактникиВАЖНЫЕ(gradeSet).Map(i => Math.Min(summGrade, i)).GetOrElse(summGrade));
            }
        }

        public static Option<int> ОценкаУрсВАЖНЫЕ(GradeSet gradeSet) {
            var importantGrades = new List<string> { "CП", "ТП", "ФП", "РХБЗ" }.ConvertAll(s => gradeSet.grades.GetOption(s)).Flatten();
            if (importantGrades.Count() == 0) {
                return new None<int>();
            } else {
                return new Some<int>(importantGrades.Min());
            }
        }

        public static Option<int> ОценкаУрсОБЩ(GradeSet gradeSet) {
            var grades = new List<string> { "CП", "ТП", "ФП", "РХБЗ", "МП", "ОГН", "СТР", "ОВУ" }.ConvertAll(s => gradeSet.grades.GetOption(s)).Flatten();
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> count = x => (float) gradeSet.grades.Where(g => g.Value == x).Count() / gradeSet.grades.Count;
                int summGrade;
                if (count(3) == 0 && count(2) == 0 && count(5) >= count(4)) {
                    summGrade = 5;
                } else if (count(4) + count(5) >= 0.5 && count(2) == 0) {
                    summGrade = 4;
                } else if (count(2) == 0) {
                    summGrade = 3;
                } else {
                    summGrade = 2;
                }
                return Options.Create(ОценкаУрсВАЖНЫЕ(gradeSet).Map(i => Math.Min(summGrade, i)).GetOrElse(summGrade));
            }
        }

        public static Option<int> КомандирскаяПодготовка(GradeSet gradeSet) {
            List<int> grades = gradeSet.grades.Values.ToList();
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> count = x => (float) grades.Where(g => g == x).Count() / grades.Count;
                if (count(3) == 0 && count(2) == 0 && count(5) >= count(4)) {
                    return new Some<int>(5);
                } else if (count(4) + count(5) >= 0.5 && count(2) == 0) {
                    return new Some<int>(4);
                } else if (count(2) < 0.3) {
                    return new Some<int>(3);
                } else {
                    return new Some<int>(2);
                }
            }
        }

        public static bool ДопускНаКлассностьКурсанты(GradeSet gradeSet) {
            return
                new List<string> { "ФП", "СТР", "ОГН", "ОВУ" }.Select(subj => gradeSet.grades.GetOption(subj)).All(g => g.NonEmpty()) &&
                new List<string> { "ФП", "СТР", "ОГН", "ОВУ" }.Where(subj => gradeSet.grades[subj] == 3).Count() <= 1 &&
                new List<string> { "ФП", "СТР", "ОГН", "ОВУ" }.Where(subj => gradeSet.grades[subj] == 2).Count() == 0;
        }

        public static bool КлассностьКурсанты(GradeSet gradeSet) {
            var sp = gradeSet.grades.GetOption("СП").Filter(g => g >= 4).NonEmpty();
            var tp = gradeSet.grades.GetOption("ТП").Filter(g => g >= 4).NonEmpty();
            return ДопускНаКлассностьКурсанты(gradeSet) && sp && tp;
        }

        public static Option<int> ОценкаОБЩ(Dictionary<string, int> grades, string ТипОбучения, string ТипВоеннослужащего) {
            GradeSet gs = new GradeSet() { grades = grades };
            if (ТипВоеннослужащего == "контрактник" || ТипВоеннослужащего == "постоянный срочник") {
                return ОценкаКонтрактникиОБЩ(gs);
            } else if (ТипОбучения == "3мес" || ТипОбучения == "6мес") {
                return ОценкаУрсОБЩ(gs);
            } else if (ТипВоеннослужащего == "курсант" || ТипОбучения == "срочники") {
                return ОценкаКурсантыОБЩ(gs);
            } else {
                throw new Exception(String.Format("Неожиданный ТипОбучения+ТипВоеннослужащего: '{0}'+'{1}'", ТипОбучения, ТипВоеннослужащего));
            }
        }
    }
}
