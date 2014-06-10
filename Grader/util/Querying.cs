using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using LibUtil;
using System.Windows.Forms;
using Grader.enums;

namespace Grader.util {
    public static class Querying {
        public static Option<Военнослужащий> GetCompanyCommander(DataContext dc, int subunitId) {
            var query =
                from subunit in dc.GetTable<Подразделение>()
                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where subunit.Код == subunitRel.КодСтаршегоПодразделения
                from soldier in dc.GetTable<Военнослужащий>()
                where subunit.КодКомандира == soldier.Код
                
                where subunit.Тип == "рота"
                where subunitRel.КодПодразделения == subunitId
                select soldier;
            return Options.HeadOption(query.ToListTimed("GetCompanyCommander"));
        }

        public static Option<Подразделение> GetCompany(DataContext dc, int subunitId) {
            var query =
                from subunit in dc.GetTable<Подразделение>()
                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where subunit.Код == subunitRel.КодСтаршегоПодразделения

                where subunit.Тип == "рота"
                where subunitRel.КодПодразделения == subunitId
                select subunit;
            return query.ToListTimed("GetCompany").HeadOption();
        }

        public static Option<Военнослужащий> GetSubunitCommander(DataContext dc, int subunitId) {
            var query =
                from subunit in dc.GetTable<Подразделение>()
                join soldier in dc.GetTable<Военнослужащий>() on subunit.КодКомандира equals soldier.Код
                where subunit.Код == subunitId
                select soldier;
            return Options.HeadOption(query.ToListTimed("GetSubunitCommander"));
        }

        public static string GetSubunitName(DataContext dc, int subunitId) {
            return (from subunit in dc.GetTable<Подразделение>() where subunit.Код == subunitId select subunit.Имя).ToListTimed("GetSubunitName").First();
        }

        public static List<Подразделение> GetSlaveSubunitsByType(DataContext dc, string subunitType, int subunitId) {
            return (
                from subunit in dc.GetTable<Подразделение>()
                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where subunit.Код == subunitRel.КодПодразделения

                where subunit.Тип == subunitType
                where subunitRel.КодСтаршегоПодразделения == subunitId
                select subunit).ToListTimed("GetSlaveSubunitsByType").Where(s => s.Актив).ToList();
        }

        public static List<Подразделение> GetSubunitsByType(DataContext dc, string subunitType) {
            return (
                from subunit in dc.GetTable<Подразделение>() 
                where subunit.Тип == subunitType
                select subunit).ToListTimed("GetSubunitsByType").Where(s => s.Актив).ToList();
        }

        public static List<Военнослужащий> GetPostsForSubunit(DataContext dc, int subunitId, string postName) {
            var query =
                from pos in dc.GetTable<Должность>()
                from soldier in dc.GetTable<Военнослужащий>()
                where pos.КодВоеннослужащего == soldier.Код
                from rank in dc.GetTable<Звание>()
                where soldier.КодЗвания == rank.Код
                
                where pos.КодПодразделения == subunitId
                where pos.Название == postName
                orderby rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;
            return query.ToListTimed("GetPostsForSubunit");
        }

        public static Option<Военнослужащий> GetPostForSubunit(DataContext dc, int subunitId, string postName) {
            return GetPostsForSubunit(dc, subunitId, postName).HeadOption();
        }

        public static Func<IQueryable<ВоеннослужащийПоПодразделениям>, IQueryable<ВоеннослужащийПоПодразделениям>> GetSoldierQueryFilterByType(
            DataContext dc, bool selectCadets, bool selectPermanent, bool selectContract, StudyType studyType
            ) {
            return q => {
                return
                    from soldier in q
                    from subunit in dc.GetTable<Подразделение>()
                    where soldier.КодПодразделения == subunit.Код

                    where (studyType.ToString() == "все" || subunit.ТипОбучения == studyType.ToString())

                    where
                        (soldier.ТипВоеннослужащего == "курсант" && selectCadets) ||
                        (soldier.ТипВоеннослужащего == "постоянный срочник" && selectPermanent) ||
                        (soldier.ТипВоеннослужащего == "контрактник" && selectContract)
                    select soldier;
            };
        }

        public static List<ВоеннослужащийПоПодразделениям> GetSubunitSoldiers(DataContext dc, int subunitId, Func<IQueryable<ВоеннослужащийПоПодразделениям>, IQueryable<ВоеннослужащийПоПодразделениям>> queryFilter) {
            var query =
                from soldier in dc.GetTable<ВоеннослужащийПоПодразделениям>()
                from subunit in dc.GetTable<Подразделение>()
                where soldier.КодПодразделения == subunit.Код

                from rank in dc.GetTable<Звание>()
                where soldier.КодЗвания == rank.Код

                where soldier.КодСтаршегоПодразделения == subunitId
                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;

            return queryFilter(query).ToListTimed("GetSubunitSoldiers").Where(s => !s.Убыл).ToList();
        }

        public static List<ВоеннослужащийПоПодразделениям> GetSubunitSoldiersExact(DataContext dc, int subunitId, Func<IQueryable<ВоеннослужащийПоПодразделениям>, IQueryable<ВоеннослужащийПоПодразделениям>> queryFilter) {
            var query =
                from soldier in dc.GetTable<ВоеннослужащийПоПодразделениям>()
                where soldier.КодПодразделения == subunitId
                where soldier.КодСтаршегоПодразделения == subunitId
                from subunit in dc.GetTable<Подразделение>()
                where subunit.Код == subunitId
                from rank in dc.GetTable<Звание>()
                where soldier.КодЗвания == rank.Код
                
                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;

            return queryFilter(query).ToListTimed("GetSubunitSoldiersExact").Where(s => !s.Убыл).ToList();
        }

    }
}
