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
        public static Option<Военнослужащий> GetCommander(Entities et, int subunitId) {
            var query =
                from subunit in et.Подразделение
                join soldier in et.Военнослужащий on subunit.КодКомандира equals soldier.Код
                where subunit.Код == subunitId
                select soldier;
            return query.ToList().HeadOption();
        }

        public static Option<Военнослужащий> GetCompanyCommander(Entities et, int subunitId) {
            var query =
                from subunit in et.Подразделение
                join subunitRel in et.ПодразделениеПодчинение on subunit.Код equals subunitRel.КодСтаршегоПодразделения
                join soldier in et.Военнослужащий on subunit.КодКомандира equals soldier.Код
                
                where subunit.Тип == "рота"
                where subunitRel.КодПодразделения == subunitId
                select soldier;
            return query.ToList().HeadOption();
        }

        public static Option<Подразделение> GetCompany(Entities et, int subunitId) {
            var query =
                from subunit in et.Подразделение
                join subunitRel in et.ПодразделениеПодчинение on subunit.Код equals subunitRel.КодСтаршегоПодразделения

                where subunit.Тип == "рота"
                where subunitRel.КодПодразделения == subunitId
                select subunit;
            return query.ToList().HeadOption();
        }

        public static Option<Военнослужащий> GetSubunitCommander(Entities et, int subunitId) {
            var query =
                from subunit in et.Подразделение
                join soldier in et.Военнослужащий on subunit.КодКомандира equals soldier.Код
                where subunit.Код == subunitId
                select soldier;
            return query.ToList().HeadOption();
        }

        public static string GetSubunitName(Entities et, int subunitId) {
            return (from subunit in et.Подразделение where subunit.Код == subunitId select subunit.Имя).First();
        }

        public static IQueryable<Подразделение> GetSlaveSubunitsByType(Entities et, string subunitType, int subunitId) {
            return
                from subunit in et.Подразделение
                join subunitRel in et.ПодразделениеПодчинение on subunit.Код equals subunitRel.КодПодразделения

                where subunit.Тип == subunitType
                where subunitRel.КодСтаршегоПодразделения == subunitId
                where subunit.Актив
                select subunit;
        }

        public static IQueryable<Подразделение> GetSubunitsByType(Entities et, string subunitType) {
            return
                from subunit in et.Подразделение
                where subunit.Тип == subunitType
                where subunit.Актив
                select subunit;
        }

        public static IQueryable<Военнослужащий> GetPostsForSubunit(Entities et, int subunitId, string postName) {
            return
                from pos in et.Должность
                join soldier in et.Военнослужащий on pos.КодВоеннослужащего equals soldier.Код
                join rank in et.Звание on soldier.КодЗвания equals rank.Код
                
                where pos.КодПодразделения == subunitId
                where pos.Название == postName
                orderby rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;
        }

        public static Option<Военнослужащий> GetPostForSubunit(Entities et, int subunitId, string postName) {
            return GetPostsForSubunit(et, subunitId, postName).ToList().HeadOption();
        }

        public static IQueryable<Военнослужащий> GetSubunitSoldiers(Entities et, int subunitId, IQueryable<Военнослужащий> soldierQuery) {
            return
                from soldier in soldierQuery
                join subunitRel in et.ПодразделениеПодчинение on soldier.КодПодразделения equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == subunitId

                join rank in et.Звание on soldier.КодЗвания equals rank.Код

                where !soldier.Убыл
                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;
        }

        public static IQueryable<Военнослужащий> GetSubunitSoldiersExact(Entities et, int subunitId, IQueryable<Военнослужащий> soldierQuery) {
            return
                from soldier in soldierQuery
                where soldier.КодПодразделения == subunitId

                join rank in et.Звание on soldier.КодЗвания equals rank.Код

                where !soldier.Убыл

                orderby soldier.sortWeight descending, rank.order descending, soldier.Фамилия, soldier.Имя, soldier.Отчество
                select soldier;
        }

    }
}
