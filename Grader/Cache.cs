using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil;

namespace Grader {
    public class Cache {
        private static Dictionary<string, object> internalCache = new Dictionary<string,object>();

        public static void DropCache() {
            internalCache.Clear();
        }

        public static T Cached<T>(string key, Func<T> fun) {
            return (T) internalCache.GetOrElseInsertAndGet(key, () => fun());
        }

        public static List<ПодразделениеПодчинение> ПодразделениеПодчинение(DataContext dc) {
            return Cached("ПодразделениеПодчиненеие", () => dc.GetTable<ПодразделениеПодчинение>().ToListTimed());
        }
    }
}
