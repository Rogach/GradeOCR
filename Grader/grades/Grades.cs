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

        public static IQueryable<Оценка> GetGradeQuery(Application accessApp, DataContext dc) {
            Form f = accessApp.GetForm("ПоОценкам").Get();
            DateTime dateFrom = f.GetControl("SelectDateFrom").Get().DateTimeValue();
            DateTime dateTo = f.GetControl("SelectDateTo").Get().DateTimeValue();
            string studyType = f.GetControl("SelectStudyType").Get().StringValue();
            bool selectCadets = f.GetControl("SelectCadets").Get().BooleanValue();
            bool selectPermanent = f.GetControl("SelectPermanent").Get().BooleanValue();
            bool selectContract = f.GetControl("SelectContract").Get().BooleanValue();
            bool selectContractAndPermanent = f.GetControl("SelectContractAndPermanent").Get().BooleanValue();

            bool selectRelatedSubunits = f.GetControl("SelectRelatedSubunits").Get().BooleanValue();

            string minRankName = f.GetControl("SelectMinRank").Get().StringValue();
            string maxRankName = f.GetControl("SelectMaxRank").Get().StringValue();

            int parentSubunitId = f.GetControl("SubunitSelect").Get().OptionalComboBoxIntegerValue().GetOrElse(1);

            int vusOrNull = f.GetControl("VusSelect").Get().OptionalComboBoxIntegerValue().GetOrElse(0);

            return (
                from grade in dc.GetTable<Оценка>()
                from subunit in dc.GetTable<Подразделение>()
                where grade.КодПодразделения == subunit.Код
                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where grade.КодПодразделения == subunitRel.КодПодразделения

                from rank in dc.GetTable<Звание>()
                where grade.КодЗвания == rank.Код

                where subunitRel.КодСтаршегоПодразделения == parentSubunitId
                where (selectRelatedSubunits || grade.КодПодразделения == parentSubunitId)

                from minRank in dc.GetTable<Звание>()
                where minRank.Название == minRankName
                where rank.order >= minRank.order

                from maxRank in dc.GetTable<Звание>()
                where maxRank.Название == maxRankName
                where rank.order <= maxRank.order

                //where grade.ДатаОценки >= dateFrom.Date && grade.ДатаОценки <= dateTo.Date
                where (studyType == "все" || subunit.ТипОбучения == studyType)
                where
                    (selectCadets && grade.ТипВоеннослужащего == "курсант") ||
                    (selectPermanent && grade.ТипВоеннослужащего == "постоянный срочник") ||
                    (selectContract && grade.ТипВоеннослужащего == "контрактник") ||
                    (selectContractAndPermanent && (grade.ТипВоеннослужащего == "контрактник" || grade.ТипВоеннослужащего == "постоянный срочник"))
                where vusOrNull == 0 || grade.ВУС == vusOrNull
                select grade);
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

                select new { grade = g.Значение, soldier, rank, subunit, date = register.ДатаЗаполнения, subj = subj.Название };
            var gradeSets = new Dictionary<int, GradeSet>();
            foreach (var g in query.ToListTimed("GradeSets")) {
                var gradeSet = gradeSets.GetOrElseInsertAndGet(g.soldier.Код, () => new GradeSet(g.soldier, g.rank, g.subunit, g.date));
                gradeSet.AddGrade(g.subj, g.grade);
            }
            return gradeSets.Values.ToList();
        }

    }


}
