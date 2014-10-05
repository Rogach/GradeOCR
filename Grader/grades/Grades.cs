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

        public static IQueryable<Оценка> GetGradesForSubunit(Entities et, IQueryable<Оценка> gradeQuery, int subunitId) {
            return
                from g in gradeQuery
                join subunitRel in et.ПодразделениеПодчинение on g.КодПодразделения equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select g;
        }

        public static IEnumerable<Оценка> GetGradesForSubunit(Entities et, IEnumerable<Оценка> gradeList, int subunitId) {
            return
                from g in gradeList
                join subunitRel in et.subunitRelCache on g.КодПодразделения equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select g;
        }

        public static IQueryable<Оценка> GetGradesForSubunitExact(Entities et, IQueryable<Оценка> gradeQuery, int subunitId) {
            return
                from g in gradeQuery
                where g.КодПодразделения == subunitId
                select g;
        }

        public static IQueryable<Оценка> GetGradesOnCycle(Entities et, IQueryable<Оценка> gradeQuery, int cycleId) {
            return
                from g in gradeQuery
                join vusRel in et.ВусНаЦикле on g.ВУС equals vusRel.ВУС
                where vusRel.КодЦикла == cycleId
                select g;
        }

        public static IEnumerable<Оценка> GetGradesOnCycle(Entities et, IEnumerable<Оценка> gradeList, int cycleId) {
            return
                from g in gradeList
                join vusRel in et.cycleVusCache on g.ВУС equals vusRel.ВУС
                where vusRel.КодЦикла == cycleId
                select g;
        }

        public static List<int> GetSubjectGrades(this IEnumerable<GradeSet> gradeSets, string subjectName) {
            return gradeSets.Select(g => GradeCalcIndividual.GetGrade(g, subjectName)).ToList().Flatten();
        }

        public static List<int> GetSubjectGrades(this IQueryable<Оценка> grades, Entities et, string subjectName) {
            return GradeSets(et, grades).GetSubjectGrades(subjectName);
        }

        public static List<GradeSet> GradeSets(Entities et, IQueryable<Оценка> gradeQuery) {
            var query =
                from g in gradeQuery

                join subj in et.Предмет on g.КодПредмета equals subj.Код
                join soldier in et.Военнослужащий on g.КодПроверяемого equals soldier.Код
                join subunit in et.Подразделение on g.КодПодразделения equals subunit.Код
                join rank in et.Звание on g.КодЗвания equals rank.Код
                join register in et.Ведомость on g.КодВедомости equals register.Код

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
            foreach (var g in query) {
                if (!g.isComment) {
                    var gradeSet = gradeSets.GetOrElseInsertAndGet(g.soldier.Код, () => new GradeSet(g.soldier, g.rank, g.subunit, g.date));
                    gradeSet.AddGrade(g.subj, g.grade);
                }
            }
            return gradeSets.Values.ToList();
        }

        public static List<ExtendedGradeSet> ExtendedGradeSets(Entities et, IQueryable<Оценка> gradeQuery) {
            var query =
                from g in gradeQuery

                join register in et.Ведомость on g.КодВедомости equals register.Код

                orderby register.ДатаЗаполнения

                select g;
            var gradeSets = new Dictionary<int, ExtendedGradeSet>();
            foreach (var g in query) {
                var gradeSet = gradeSets.GetOrElseInsertAndGet(g.КодПроверяемого, () => new ExtendedGradeSet());
                gradeSet.AddGrade(g);
            }
            return gradeSets.Values.ToList();
        }
    }


}
