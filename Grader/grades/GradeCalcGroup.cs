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

        public static Option<int> ОбщаяОценка(
                Entities et, 
                IQueryable<Оценка> gradeQuery, 
                Подразделение subunit, 
                string subjectName, 
                bool cadetsSelected,
                bool selectRelatedSubunits) {

            if (subjectName == "ОБЩ (курсанты)" || subjectName == "ОВП (курсанты)" || cadetsSelected) {
                return КурсантыПоПредмету(et, gradeQuery, subunit, subjectName);
            } else if (subjectName == "командирск.подгот.") {
                return КомандирскаяПодготовка(et, gradeQuery, subunit);
            } else if (subjectName == "ОБЩ (контракт)" && !selectRelatedSubunits) {
                IEnumerable<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
                return БоеваяПодготовкаЗаПодразделение(et, gradeSets, subunit.Код);
            } else if (subjectName == "ОБЩ (контракт)") {
                return БоеваяПодготвкаОбщая(et, gradeQuery, subunit);
            } else {
                return БоеваяПодготовкаПоПредмету(et, gradeQuery, subunit, subjectName);
            }
        }

        public static Option<int> ОбщаяОценка( 
                Entities et,
                IQueryable<Оценка> gradeQuery, 
                string subjectName, 
                bool cadetsSelected) {

            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
            if (subjectName == "ОБЩ (курсанты)" || subjectName == "ОВП (курсанты)" || cadetsSelected) {
                return КурсантыПоПредметуЗаЧасть(et, gradeSets, subjectName);
            } else if (subjectName == "ОБЩ (контракт)") {
                return БоеваяПодготовкаЗаЧасть(et, gradeSets);
            } else {
                return БоеваяПодготовкаПоПредметуЗаЧасть(et, gradeSets, subjectName);
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

        public static Option<int> ФормулаПостоянныйСоставПоФПСУправлением(Option<int> hqGrade, List<int> grades) {
            if (grades.Count == 0) {
                return new None<int>();
            } else {
                List<int> allGrades = new List<int>();
                allGrades.AddRange(grades);
                if (hqGrade.NonEmpty()) allGrades.Add(hqGrade.Get());
                Func<int, float> countGrades = c => (float) allGrades.Where(g => g == c).Count() / allGrades.Count;
                if (hqGrade.Map(g => g == 5).GetOrElse(true) && countGrades(5) >= 0.5 && countGrades(3) == 0 && countGrades(2) == 0) {
                    return Options.Create(5);
                } else if (hqGrade.Map(g => g >= 4).GetOrElse(true) && countGrades(5) + countGrades(4) >= 0.5 && countGrades(2) == 0) {
                    return Options.Create(4);
                } else if (hqGrade.Map(g => g >= 3).GetOrElse(true) && countGrades(2) < 0.5) {
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

        public static Option<int> КурсантыПоПредмету(Entities et, IQueryable<Оценка> gradeQuery, Подразделение subunit, string subjectName) {
            List<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
            if (subunit.Тип == "взвод") {
                return КурсантыПоПредметуЗаВзвод(gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "рота") {
                return КурсантыПоПредметуЗаРоту(et, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "батальон") {
                return КурсантыПоПредметуЗаБатальон(et, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Имя == "часть") {
                return КурсантыПоПредметуЗаЧасть(et, gradeSets, subjectName);
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

        public static Option<int> КурсантыПоПредметуЗаРоту(Entities et, IEnumerable<GradeSet> gradeQuery, int companyId, string subjectName) {
            return
                Querying.GetSlaveSubunitsByType(et, "взвод", companyId).ToList()
                .ConvertAll(subunit => КурсантыПоПредметуЗаВзвод(gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        public static Option<int> КурсантыПоПредметуЗаБатальон(Entities et, IEnumerable<GradeSet> gradeQuery, int batallionId, string subjectName) {
            return
                Querying.GetSlaveSubunitsByType(et, "рота", batallionId).ToList()
                .ConvertAll(subunit => КурсантыПоПредметуЗаРоту(et, gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        public static Option<int> КурсантыПоПредметуЗаЦикл(Entities et, IQueryable<Оценка> gradeQuery, int cycleId, string subjectName) {
            return (
                from g in gradeQuery
                join cycleRel in et.ВусНаЦикле on g.ВУС equals cycleRel.ВУС
                where cycleRel.КодЦикла == cycleId
                select g)
                .GetSubjectGrades(et, subjectName)
                .ФормулаКурсантыПоПредмету();
        }

        public static Option<int> КурсантыПоПредметуЗаЧасть(Entities et, IEnumerable<GradeSet> gradeQuery, string subjectName) {
            // в руководящих документах оценка по предмету за часть
            // не описана, поэтому здесь она рассчитывается подобно
            // тому, как рассчитываются оценки за батальон и роту
            return
                Querying.GetSubunitsByType(et, "батальон").ToList()
                .ConvertAll(subunit => КурсантыПоПредметуЗаБатальон(et, gradeQuery, subunit.Код, subjectName)).Flatten()
                .ФормулаНольПять();
        }

        
        // БОЕВАЯ ПОДГОТОВКА ПО ПРЕДМЕТУ

        public static Option<int> БоеваяПодготовкаПоПредмету(Entities et, IQueryable<Оценка> gradeQuery, Подразделение subunit, string subjectName) {
            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
            if (subunit.Тип == "взвод" || subunit.Тип == "рота" || subunit.Тип == "цикл" || subunit.Имя == "управление") {
                return БоеваяПодготовкаПоПредметуЗаПодразделение(et, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Тип == "батальон") {
                return БоеваяПодготовкаПоПредметуЗаБатальон(et, gradeSets, subunit.Код, subjectName);
            } else if (subunit.Имя == "часть") {
                return БоеваяПодготовкаПоПредметуЗаЧасть(et, gradeSets, subjectName);
            } else {
                throw new Exception("Unexpected subunit type");
            }
        }

        /* Боевая подготовка по отдельному предмету за простое подразделение -
         * за взвод, роту, цикл, управление батальона или учебного центра
         */
        public static Option<int> БоеваяПодготовкаПоПредметуЗаПодразделение(Entities et, IEnumerable<GradeSet> gradeQuery, int subunitId, string subjectName) {
            return (
                from g in gradeQuery
                join subunitRel in et.subunitRelCache on g.subunit.Код equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select g)
                .GetSubjectGrades(subjectName)
                .ФормулаПостоянныйСоставПоПредмету();
        }

        public static Option<int> БоеваяПодготовкаПоПредметуЗаБатальон(Entities et, IEnumerable<GradeSet> gradeQuery, int batallionId, string subjectName) {
            if (subjectName == "ФП") {
                return gradeQuery.GetSubjectGrades("ФП").ФормулаПостоянныйСоставПоПредмету();
            } else {
                Option<int> hqGrade =
                    gradeQuery.Where(g => g.subunit.Код == batallionId)
                    .GetSubjectGrades(subjectName)
                    .ФормулаПостоянныйСоставПоПредмету();
                List<int> subunitGrades =
                    Querying.GetSlaveSubunitsByType(et, "рота", batallionId).ToList()
                    .ConvertAll(subunit => БоеваяПодготовкаПоПредметуЗаПодразделение(et, gradeQuery, subunit.Код, subjectName)).Flatten();
                return ФормулаПостоянныйСоставПоПредметуСУправлением(hqGrade, subunitGrades);
            }
        }

        public static Option<int> БоеваяПодготовкаПоПредметуЗаЧасть(Entities et, IEnumerable<GradeSet> gradeQuery, string subjectName) {
            int hqId = (from subunit in et.Подразделение where subunit.Имя == "управление" select subunit.Код).First();
            Option<int> hqGrade =
                gradeQuery.Where(g => g.subunit.Код == hqId)
                .GetSubjectGrades(subjectName)
                .ФормулаПостоянныйСоставПоПредмету();

            List<int> batallionGrades =
                Querying.GetSubunitsByType(et, "батальон").ToList()
                .ConvertAll(batallion => БоеваяПодготовкаПоПредметуЗаБатальон(et, gradeQuery, batallion.Код, subjectName)).Flatten();
            Option<int> ursGrade =
                БоеваяПодготовкаПоПредметуЗаПодразделение(et, gradeQuery,
                    (from s in et.Подразделение where s.Имя == "УРС" select s.Код).First(),
                    subjectName);
            List<int> subunitGrades = batallionGrades.Concat(ursGrade.ToList()).ToList();
            if (subjectName == "ФП") {
                return ФормулаПостоянныйСоставПоФПСУправлением(hqGrade, subunitGrades);
            } else {
                return ФормулаПостоянныйСоставПоПредметуСУправлением(hqGrade, subunitGrades);
            }
        }


        // БОЕВАЯ ПОДГОТОВКА - ОБЩАЯ

        public static Option<int> БоеваяПодготвкаОбщая(Entities et, IQueryable<Оценка> gradeQuery, Подразделение subunit) {
            IEnumerable<GradeSet> gradeSets = Grades.GradeSets(et, gradeQuery);
            if (subunit.Тип == "взвод" || subunit.Тип == "рота" || subunit.Тип == "цикл" || subunit.Имя == "управление") {
                return БоеваяПодготовкаЗаПодразделение(et, gradeSets, subunit.Код);
            } else if (subunit.Тип == "батальон") {
                return БоеваяПодготовкаЗаБатальон(et, gradeSets, subunit.Код);
            } else if (subunit.Имя == "часть") {
                return БоеваяПодготовкаЗаЧасть(et, gradeSets);
            } else {
                throw new Exception("Unexpected subunit type");
            }
        }

        /* Общая оценка за боевую подготовку в простом подразделении -
         * за взвод, роту, цикл, управление батальона или учебного центра
         */
        public static Option<int> БоеваяПодготовкаЗаПодразделение(Entities et, IEnumerable<GradeSet> gradeQuery, int subunitId) {
            List<string> subjects = gradeQuery.SelectMany(g => g.grades.Keys).Where(subj => subj != "МОБ").Distinct().ToList();
            Подразделение subunit = (from s in et.Подразделение where s.Код == subunitId select s).First();
            if (subjects.Count == 0) {
                return new None<int>();
            } else {
                Dictionary<string, int> grades =
                    subjects.Select(subj => new { subj = subj, grade = БоеваяПодготовкаПоПредметуЗаПодразделение(et, gradeQuery, subunitId, subj) })
                    .Where(g => g.grade.NonEmpty()).ToDictionary(g => g.subj, g => g.grade.Get());
                return БоеваяПодготовкаЗаПодразделение(grades, subunit);
            }
        }

        public static Option<int> БоеваяПодготовкаЗаПодразделение(Dictionary<string, int> grades, Подразделение subunit) {
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

        public static Option<int> БоеваяПодготовкаЗаБатальон(Entities et, IEnumerable<GradeSet> gradeQuery, int batallionId) {
            Option<int> hqGrade = БоеваяПодготовкаЗаПодразделение(et, gradeQuery, batallionId);
            List<int> rotaGrades =
                Querying.GetSlaveSubunitsByType(et, "рота", batallionId).ToList()
                .ConvertAll(subunit => БоеваяПодготовкаЗаПодразделение(et, gradeQuery, subunit.Код)).Flatten();
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

        public static Option<int> БоеваяПодготовкаЗаЧасть(Entities et, IEnumerable<GradeSet> gradeQuery) {
            int hqId = (from s in et.Подразделение where s.Имя == "управление" select s.Код).First();
            Option<int> hqGradeOpt = БоеваяПодготовкаЗаПодразделение(et, gradeQuery, hqId);
            List<int> batallionGrades =
                Querying.GetSubunitsByType(et, "батальон").ToList()
                .ConvertAll(batallion => БоеваяПодготовкаЗаБатальон(et, gradeQuery, batallion.Код)).Flatten();
            Option<int> ursGrade =
                БоеваяПодготовкаЗаПодразделение(et, gradeQuery,
                    (from s in et.Подразделение where s.Имя == "УРС" select s.Код).First());
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

        public static Option<int> КомандирскаяПодготовка(Entities et, IQueryable<Оценка> gradeQuery, Подразделение subunit) {
            Option<int> mpGrade = БоеваяПодготовкаПоПредмету(et, gradeQuery, subunit, "МЕТ");
            List<int> commandGradeList = Grades.GetSubjectGrades(Grades.GradeSets(et, Grades.GetGradesForSubunit(et, gradeQuery, subunit.Код)), "командирск.подгот.");
            Option<int> commandGrade = ФормулаПостоянныйСоставПоПредмету(commandGradeList);
            return mpGrade.FlatMap(mp => commandGrade.Map(cm => Math.Min(mp, cm))).OrElse(commandGrade);
        }
    }
}

