using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using Grader.grades;
using ExcelWorksheet = Microsoft.Office.Interop.Excel.Worksheet;
using Grader.util;
using LibUtil;

namespace Grader.grades {

    public static class Grades {

        public static IQueryable<Оценка> GetGradesForSubunit(DataContext dc, IQueryable<Оценка> gradeQuery, int subunitId) {
            return
                from g in gradeQuery
                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where g.КодПодразделения == subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select g;
        }

        public static IQueryable<Оценка> GetGradesForSubunitExact(DataContext dc, IQueryable<Оценка> gradeQuery, int subunitId) {
            return
                from g in gradeQuery
                where g.КодПодразделения == subunitId
                select g;
        }

        public static IQueryable<Оценка> GetGradesOnCycle(DataContext dc, IQueryable<Оценка> gradeQuery, int cycleId) {
            return
                from g in gradeQuery
                from vusRel in dc.GetTable<ВусНаЦикле>()
                where g.ВУС == vusRel.ВУС
                where vusRel.КодЦикла == cycleId
                select g;
        }

        public static List<int> GetSubjectGrades(this IEnumerable<GradeSet> gradeSets, string subjectName) {
            return gradeSets.Select(g => GradeCalcIndividual.GetGrade(g, subjectName)).ToList().Flatten();
        }

        public static List<int> GetSubjectGrades(this IQueryable<Оценка> grades, DataContext dc, string subjectName) {
            return GradeSets(dc, grades).GetSubjectGrades(subjectName);
        }

        public static List<GradeSet> GradeSets(DataContext dc, IQueryable<Оценка> gradeQuery) {
            var query =
                from g in gradeQuery

                from subj in dc.GetTable<Предмет>()
                where g.КодПредмета == subj.Код

                from soldier in dc.GetTable<Военнослужащий>()
                where g.КодПроверяемого == soldier.Код

                from subunit in dc.GetTable<Подразделение>()
                where g.КодПодразделения == subunit.Код

                from rank in dc.GetTable<Звание>()
                where g.КодЗвания == rank.Код

                from register in dc.GetTable<Ведомость>()
                where g.КодВедомости == register.Код

                orderby register.ДатаЗаполнения

                select new { 
                    grade = g.Значение,
                    isComment = g.ЭтоКомментарий,
                    soldier, 
                    rank, 
                    subunit, 
                    date = register.ДатаЗаполнения, 
                    subj = subj.Название 
                };
            var gradeSets = new Dictionary<int, GradeSet>();
            foreach (var g in query.ToListTimed("GradeSets")) {
                if (!g.isComment) {
                    var gradeSet = gradeSets.GetOrElseInsertAndGet(g.soldier.Код, () => new GradeSet(g.soldier, g.rank, g.subunit, g.date));
                    gradeSet.AddGrade(g.subj, g.grade);
                }
            }
            return gradeSets.Values.ToList();
        }

    }


}
