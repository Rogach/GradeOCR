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

        public static List<ВоеннослужащийПоПодразделениям> GetSubunitSoldiers(
                DataContext dc, int subunitId, 
                IQueryable<ВоеннослужащийПоПодразделениям> soldierQuery) {

            return GetSubunitSoldiersQuery(dc, subunitId, soldierQuery).ToListTimed("GetSubunitSoldiers").ToList();
        }

        public static IQueryable<ВоеннослужащийПоПодразделениям> GetSubunitSoldiersQuery(
                DataContext dc, int subunitId, 
                IQueryable<ВоеннослужащийПоПодразделениям> soldierQuery) {

            var query =
                from soldier in soldierQuery

                from subunitRel in dc.GetTable<ПодразделениеПодчинение>()
                where soldier.КодПодразделения == subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId

                from rank in dc.GetTable<Звание>()
                where soldier.КодЗвания == rank.Код

                where soldier.Убыл == 0
                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;
            return query;
        }

        public static List<ВоеннослужащийПоПодразделениям> GetSubunitSoldiersExact(
                DataContext dc, int subunitId,
                IQueryable<ВоеннослужащийПоПодразделениям> soldierQuery) {

            var query =
                from soldier in soldierQuery
                where soldier.КодПодразделения == subunitId

                from rank in dc.GetTable<Звание>()
                where soldier.КодЗвания == rank.Код

                where soldier.Убыл == 0
                
                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;

            return query.ToListTimed("GetSubunitSoldiersExact").ToList();
        }

    }
}
