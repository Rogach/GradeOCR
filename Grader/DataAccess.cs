using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.Linq;
using System.IO;
using System.Reflection;

namespace Grader {
    public class DataAccess {
        private readonly string dbLocation;
        private DataContext dataContextInstance;

        public DataAccess(string dbLocation) {
            this.dbLocation = dbLocation;
        }

        private DataContext CreateDataContext() {
            var conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dbLocation + ";");
            var dc = new DataContext(conn);
            //dc.Log = Console.Out;
            return dc;
        }

        public DataContext GetDataContext() {
            if (dataContextInstance == null) {
                dataContextInstance = CreateDataContext();
            }
            return dataContextInstance;
        }

        /* Resets data context object cache after insert */
        public static void AfterInsert(DataContext dc) {
            MethodInfo clearCache = dc.GetType().GetMethod("ClearCache", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            clearCache.Invoke(dc, new object[] {});
        }

        public string GetTemplateLocation(string template) {
            return Directory.GetParent(dbLocation) + "/templates/" + template;
        }
    }
}
