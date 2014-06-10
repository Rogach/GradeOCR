using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using Grader.util;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using AccessForm = Microsoft.Office.Interop.Access.Form;
using LibUtil;

namespace Grader.grades {
    public static class GradeCalcGroup {

        public static Option<int> ОбщаяОценка(AccessApplication accessApp, DataContext dc, IQueryable<Оценка> gradeQuery, Подразделение subunit, string subjectName) {
            AccessForm f = accessApp.GetForm("ПоОценкам").Get();
            bool selectCadets = f.GetControl("SelectCadets").Get().BooleanValue();
            bool selectRelatedSubunits = f.GetControl("SelectRelatedSubunits").Get().BooleanValue();

            if (subjectName == "ОБЩ (курсанты)" || subjectName == "ОВП (курсанты)" || selectCadets) {
                return КурсантыПоПредмету(dc, gradeQuery, subunit, subjectName);
            } else if (subjectName == "командирск.подгот.") {
                return КомандирскаяПодготовка(dc, gradeQuery, subunit);
            } else if (subjectName == "ОБЩ (контракт)" && !selectRelatedSubunits) {
                IEnumerable<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);
                return БоеваяПодготовкаЗаПодразделение(dc, gradeSets, subunit.Код);
            } else if (subjectName == "ОБЩ (контракт)") {
                return БоеваяПодготвкаОбщая(dc, gradeQuery, subunit);
            } else {
                return БоеваяПодготовкаПоПредмету(dc, gradeQuery, subunit, subjectName);
            }
        }

        public static Option<int> ОбщаяОценка(AccessApplication accessApp, DataContext dc, IQueryable<Оценка> gradeQuery, string subjectName) {
            AccessForm f = accessApp.GetForm("ПоОценкам").Get();
            bool selectCadets = f.GetControl("SelectCadets").Get().BooleanValue();
            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);
            if (subjectName == "ОБЩ (курсанты)" || subjectName == "ОВП (курсанты)" || selectCadets) {
                return КурсантыПоПредметуЗаЧасть(dc, gradeSets, subjectName);
            } else if (subjectName == "ОБЩ (контракт)") {
                return БоеваяПодготовкаЗаЧасть(dc, gradeSets);
            } else {
                return БоеваяПодготовкаПоПредметуЗаЧасть(dc, gradeSets, subjectName);
            }
        }

        // ОБЩИЕ ФОРМУЛЫ

        public static Option<int> ФормулаКурсантыПоПредмету(this List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> countGrades = c => (float) grades.Where(g => g == c).Count() / grades.Count;
                if (countGrades(5) >= 0.5 && countGrades(2) < 0.05) {
                    return Options.Create(5);
                } else if (countGrades(4) + countGrades(5) >= 0.5 && countGrades(2) < 0.1) {
                    return Options.Create(4);
                } else if (countGrades(2) < 0.15) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        public static Option<int> ФормулаНольПять(this List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> countGrades = c => (float) grades.Where(g => g == c).Count() / grades.Count;
                if (countGrades(5) >= 0.5 && countGrades(2) == 0 && countGrades(3) == 0) {
                    return Options.Create(5);
                } else if (countGrades(5) + countGrades(4) >= 0.5 && countGrades(2) == 0) {
                    return Options.Create(4);
                } else if (countGrades(2) == 0) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        public static Option<int> ФормулаПостоянныйСоставПоПредмету(this List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> countGrades = c => (float) grades.Where(g => g == c).Count() / grades.Count;
                if (countGrades(5) >= 0.5 && countGrades(2) <= 0.1) {
                    return Options.Create(5);
                } else if (countGrades(5) + countGrades(4) >= 0.5 && countGrades(2) <= 0.2) {
                    return Options.Create(4);
                } else if (countGrades(2) <= 0.3) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        public static Option<int> ФормулаПостоянныйСоставПоПредметуСУправлением(Option<int> hqGrade, List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> countGrades = c => (float) grades.Where(g => g == c).Count() / grades.Count;
                if (hqGrade.Map(g => g == 5).GetOrElse(true) && countGrades(5) >= 0.5 && countGrades(3) == 0 && countGrades(2) == 0) {
                    return Options.Create(5);
                } else if (hqGrade.Map(g => g >= 4).GetOrElse(true) && countGrades(5) + countGrades(4) >= 0.5 && countGrades(2) == 0) {
                    return Options.Create(4);
                } else if (hqGrade.Map(g => g >= 3).GetOrElse(true) && countGrades(2) <= 0.334) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        public static Option<int> ФормулаПостоянныйСоставПоПредметам(List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                Func<int, float> countGrades = c => (float) grades.Where(g => g == c).Count() / grades.Count;
                if (countGrades(5) >= 0.5 && countGrades(3) == 0 && countGrades(2) == 0) {
                    return Options.Create(5);
                } else if (countGrades(5) + countGrades(4) >= 0.5 && countGrades(2) == 0) {
                    return Options.Create(4);
                } else if (countGrades(2) <= 0.3) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }


        // ПОДГОТОВКА КУРСАНТОВ ПО ПРЕДМЕТУ

        public static Option<int> КурсантыПоПредмету(DataContext dc, IQueryable<Оценка> gradeQuery, Подразделение subunit, string subjectName) {
            List<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);
            if (subunit.Тип == "взвод") {
                return КурсантыПоПредметуЗаВзвод(gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "рота") {
                return КурсантыПоПредметуЗаРоту(dc, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "батальон") {
                return КурсантыПоПредметуЗаБатальон(dc, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Имя == "часть") {
                return КурсантыПоПредметуЗаЧасть(dc, gradeSets, subjectName);
            } else {
                throw new Exception("Unexpected subunit type");
            }
        }

        public static Option<int> КурсантыПоПредметуЗаВзвод(IEnumerable<GradeSet> gradeQuery, int platoonId, string subjectName) {
            IEnumerable<GradeSet> filteredGrades = gradeQuery.Where(g => g.subunit.Код == platoonId);
            if (filteredGrades.Count() == 0) {
                return new None<int>();
            } else {
                List<int> grades = filteredGrades.GetSubjectGrades(subjectName);
                Подразделение platoon = filteredGrades.First().subunit;
                if (platoon.Имя.Contains("УРС")) {
                    return ФормулаПостоянныйСоставПоПредмету(grades);
                } else {
                    return ФормулаКурсантыПоПредмету(grades);
                }
            }
        }

        public static Option<int> КурсантыПоПредметуЗаРоту(DataContext dc, IEnumerable<GradeSet> gradeQuery, int companyId, string subjectName) {
            return
                Querying.GetSlaveSubunitsByType(dc, "взвод", companyId)
                .ConvertAll(subunit => КурсантыПоПредметуЗаВзвод(gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        public static Option<int> КурсантыПоПредметуЗаБатальон(DataContext dc, IEnumerable<GradeSet> gradeQuery, int batallionId, string subjectName) {
            return
                Querying.GetSlaveSubunitsByType(dc, "рота", batallionId)
                .ConvertAll(subunit => КурсантыПоПредметуЗаРоту(dc, gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        public static Option<int> КурсантыПоПредметуЗаЦикл(DataContext dc, IQueryable<Оценка> gradeQuery, int cycleId, string subjectName) {
            return (
                from g in gradeQuery
                from cycleRel in dc.GetTable<ВусНаЦикле>()
                where g.ВУС == cycleRel.ВУС
                where cycleRel.КодЦикла == cycleId
                select g)
                .GetSubjectGrades(dc, subjectName)
                .ФормулаКурсантыПоПредмету();
        }

        public static Option<int> КурсантыПоПредметуЗаЧасть(DataContext dc, IEnumerable<GradeSet> gradeQuery, string subjectName) {
            // в руководящих документах оценка по предмету за часть
            // не описана, поэтому здесь она рассчитывается подобно
            // тому, как рассчитываются оценки за батальон и роту
            return
                Querying.GetSubunitsByType(dc, "батальон")
                .ConvertAll(subunit => КурсантыПоПредметуЗаБатальон(dc, gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        
        // БОЕВАЯ ПОДГОТОВКА ПО ПРЕДМЕТУ

        public static Option<int> БоеваяПодготовкаПоПредмету(DataContext dc, IQueryable<Оценка> gradeQuery, Подразделение subunit, string subjectName) {
            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);
            if (subunit.Тип == "взвод" || subunit.Тип == "рота" || subunit.Тип == "цикл" || subunit.Имя == "управление") {
                return БоеваяПодготовкаПоПредметуЗаПодразделение(dc, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "батальон") {
                return БоеваяПодготовкаПоПредметуЗаБатальон(dc, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Имя == "часть") {
                return БоеваяПодготовкаПоПредметуЗаЧасть(dc, gradeSets, subjectName);
            } else {
                throw new Exception("Unexpected subunit type");
            }
        }

        /* Боевая подготовка по отдельному предмету за простое подразделение -
         * за взвод, роту, цикл, управление батальона или учебного центра
         */
        public static Option<int> БоеваяПодготовкаПоПредметуЗаПодразделение(DataContext dc, IEnumerable<GradeSet> gradeQuery, int subunitId, string subjectName) {
            return (
                from g in gradeQuery
                join subunitRel in Cache.ПодразделениеПодчинение(dc) on g.subunit.Код equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select g)
                .GetSubjectGrades(subjectName)
                .ФормулаПостоянныйСоставПоПредмету();
        }

        public static Option<int> БоеваяПодготовкаПоПредметуЗаБатальон(DataContext dc, IEnumerable<GradeSet> gradeQuery, int batallionId, string subjectName) {
            Option<int> hqGrade = 
                gradeQuery.Where(g => g.subunit.Код == batallionId)
                .GetSubjectGrades(subjectName)
                .ФормулаПостоянныйСоставПоПредмету();
            List<int> subunitGrades =
                Querying.GetSlaveSubunitsByType(dc, "рота", batallionId)
                .ConvertAll(subunit => БоеваяПодготовкаПоПредметуЗаПодразделение(dc, gradeQuery, subunit.Код, subjectName)).Flatten();
            return ФормулаПостоянныйСоставПоПредметуСУправлением(hqGrade, subunitGrades);
        }

        public static Option<int> БоеваяПодготовкаПоПредметуЗаЧасть(DataContext dc, IEnumerable<GradeSet> gradeQuery, string subjectName) {
            int hqId = (from subunit in dc.GetTable<Подразделение>() where subunit.Имя == "управление" select subunit.Код).ToListTimed().First();
            Option<int> hqGrade = 
                gradeQuery.Where(g => g.subunit.Код == hqId)
                .GetSubjectGrades(subjectName)
                .ФормулаПостоянныйСоставПоПредмету();

            List<int> batallionGrades =
                Querying.GetSubunitsByType(dc, "батальон")
                .ConvertAll(batallion => БоеваяПодготовкаПоПредметуЗаБатальон(dc, gradeQuery, batallion.Код, subjectName)).Flatten();
            Option<int> ursGrade =
                БоеваяПодготовкаПоПредметуЗаПодразделение(dc, gradeQuery,
                    (from s in dc.GetTable<Подразделение>() where s.Имя == "УРС" select s.Код).ToListTimed().First(),
                    subjectName);
            List<int> subunitGrades = batallionGrades.Concat(ursGrade.ToList()).ToList();
            return ФормулаПостоянныйСоставПоПредметуСУправлением(hqGrade, subunitGrades);
        }


        // БОЕВАЯ ПОДГОТОВКА - ОБЩАЯ

        public static Option<int> БоеваяПодготвкаОбщая(DataContext dc, IQueryable<Оценка> gradeQuery, Подразделение subunit) {
            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(dc, gradeQuery);
            if (subunit.Тип == "взвод" || subunit.Тип == "рота" || subunit.Тип == "цикл" || subunit.Имя == "управление") {
                return БоеваяПодготовкаЗаПодразделение(dc, gradeSets, subunit.Код);
            } else if (subunit.Тип == "батальон") {
                return БоеваяПодготовкаЗаБатальон(dc, gradeSets, subunit.Код);
            } else if (subunit.Имя == "часть") {
                return БоеваяПодготовкаЗаЧасть(dc, gradeSets);
            } else {
                throw new Exception("Unexpected subunit type");
            }
        }

        /* Общая оценка за боевую подготовку в простом подразделении -
         * за взвод, роту, цикл, управление батальона или учебного центра
         */
        public static Option<int> БоеваяПодготовкаЗаПодразделение(DataContext dc, IEnumerable<GradeSet> gradeQuery, int subunitId) {
            List<string> subjects = gradeQuery.SelectMany(g => g.grades.Keys).Where(subj => subj != "МОБ").Distinct().ToList();
            Подразделение subunit = (from s in dc.GetTable<Подразделение>() where s.Код == subunitId select s).ToListTimed().First();
            if (subjects.Count == 0) {
                return new None<int>();
            } else {
                Dictionary<string, int> grades =
                    subjects.Select(subj => new { subj = subj, grade = БоеваяПодготовкаПоПредметуЗаПодразделение(dc, gradeQuery, subunitId, subj) })
                    .Where(g => g.grade.NonEmpty()).ToDictionary(g => g.subj, g => g.grade.Get());
                List<string> importantSubjects;
                if (subunit.ПодразделениеОхраны) {
                    importantSubjects = new List<string> { "ТСП", "СП", "ТП", "ОГН", "РХБЗ" };
                } else {
                    importantSubjects = new List<string> { "ТСП", "СП", "ТП", "РХБЗ" };
                }
                Option<int> maxGrade = importantSubjects.ConvertAll(subj => grades.GetOption(subj)).Flatten().MinOption();
                Option<int> summGrade = ФормулаПостоянныйСоставПоПредметам(grades.Values.ToList());
                return maxGrade.ToList().Concat(summGrade.ToList()).MinOption();
            }
        }

        public static Option<int> БоеваяПодготовкаЗаБатальон(DataContext dc, IEnumerable<GradeSet> gradeQuery, int batallionId) {
            Option<int> hqGrade = БоеваяПодготовкаЗаПодразделение(dc, gradeQuery, batallionId);
            List<int> rotaGrades =
                Querying.GetSlaveSubunitsByType(dc, "рота", batallionId)
                .ConvertAll(subunit => БоеваяПодготовкаЗаПодразделение(dc, gradeQuery, subunit.Код)).Flatten();
            Func<int, float> gradeCount = c => (float) rotaGrades.Where(g => g == c).Count() / rotaGrades.Count;
            if (rotaGrades.Count == 0) {
                return new None<int>();
            } else {
                if (hqGrade.Map(g => g == 5).GetOrElse(true) && gradeCount(5) >= 0.5 && gradeCount(2) == 0 && gradeCount(3) == 0) {
                    return Options.Create(5);
                } else if (hqGrade.Map(g => g >= 4).GetOrElse(true) && gradeCount(5) + gradeCount(4) >= 0.5 && gradeCount(2) == 0) {
                    return Options.Create(4);
                } else if (hqGrade.Map(g => g >= 3).GetOrElse(true) && rotaGrades.Where(g => g == 2).Count() <= 1) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        public static Option<int> БоеваяПодготовкаЗаЧасть(DataContext dc, IEnumerable<GradeSet> gradeQuery) {
            int hqId = (from s in dc.GetTable<Подразделение>() where s.Имя == "управление" select s.Код).ToListTimed().First();
            Option<int> hqGradeOpt = БоеваяПодготовкаЗаПодразделение(dc, gradeQuery, hqId);
            List<int> batallionGrades =
                Querying.GetSubunitsByType(dc, "батальон")
                .ConvertAll(batallion => БоеваяПодготовкаЗаБатальон(dc, gradeQuery, batallion.Код)).Flatten();
            Option<int> ursGrade =
                БоеваяПодготовкаЗаПодразделение(dc, gradeQuery,
                    (from s in dc.GetTable<Подразделение>() where s.Имя == "УРС" select s.Код).ToListTimed().First());
            List<int> subunitGrades = batallionGrades.Concat(ursGrade.ToList()).ToList();
            if (hqGradeOpt.IsEmpty() || subunitGrades.Count == 0) {
                return new None<int>();
            } else {
                int hqGrade = hqGradeOpt.Get();
                Func<int, float> gradeCount = c => (float) subunitGrades.Where(g => g == c).Count() / subunitGrades.Count();
                if (hqGrade == 5 && gradeCount(5) >= 0.5 && gradeCount(3) == 0 && gradeCount(2) == 0) {
                    return Options.Create(5);
                } else if (hqGrade >= 4 && gradeCount(5) + gradeCount(4) >= 0.5 && gradeCount(2) == 0) {
                    return Options.Create(4);
                } else if (hqGrade >= 3 && gradeCount(2) == 0) {
                    return Options.Create(3);
                } else {
                    return Options.Create(2);
                }
            }
        }

        // КОМАНДИРСКАЯ ПОДГОТОВКА

        public static Option<int> КомандирскаяПодготовка(DataContext dc, IQueryable<Оценка> gradeQuery, Подразделение subunit) {
            Option<int> mpGrade = БоеваяПодготовкаПоПредмету(dc, gradeQuery, subunit, "МЕТ");
            List<int> commandGradeList = Grades.GetSubjectGrades(Grades.GradeSets(dc, Grades.GetGradesForSubunit(dc, gradeQuery, subunit.Код)), "командирск.подгот.");
            Option<int> commandGrade = ФормулаПостоянныйСоставПоПредмету(commandGradeList);
            return mpGrade.FlatMap(mp => commandGrade.Map(cm => Math.Min(mp, cm))).OrElse(commandGrade);
        }
    }
}

