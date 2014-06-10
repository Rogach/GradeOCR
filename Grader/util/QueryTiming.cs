using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil.sqlparse;
using System.Data.Linq;

namespace Grader.util {
    public static class QueryTiming {
        private static double totalQueryTime = 0;
        private static int totalQueryCount = 0;

        public static List<A> ToListTimed<A>(this IQueryable<A> query) {
            return ToListTimed(query, null);
        }

        public static List<A> ToListTimed<A>(this IQueryable<A> query, string note) {
            DateTime stt = DateTime.Now;
            try {
                return query.ToList();
            } finally {
                DateTime end = DateTime.Now;
                double elapsed = (end - stt).TotalMilliseconds;
                totalQueryTime += elapsed;
                totalQueryCount++;
                //if (note != null) {
                //    Logger.Log("[TIMING] query ({0}) took {1:F0} ms", note, elapsed);
                //} else {
                //    Logger.Log("[TIMING] query took {0:F0} ms", elapsed);
                //}
            }
        }

        public static void ResetCounter() {
            totalQueryTime = 0;
        }

        public static double TotalQueryTime {
            get {
                return totalQueryTime;
            }
        }

        public static int TotalQueryCount {
            get {
                return totalQueryCount;
            }
        }
    }
}
