﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.Linq;
using System.IO;

namespace Grader {
    public class DataAccess {
        private readonly string dbLocation;
        
        public DataAccess(string dbLocation) {
            this.dbLocation = dbLocation;
        }

        public DataContext GetDataContext() {
            var conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dbLocation + ";");
            var dc = new DataContext(conn);
            return dc;
        }

        public string GetTemplateLocation(string template) {
            return Directory.GetParent(dbLocation) + "/templates/" + template;
        }
    }
}
