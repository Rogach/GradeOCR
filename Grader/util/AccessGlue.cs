using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using System.Data.OleDb;
using System.Data.Linq;
using LibUtil;

namespace Grader.util {
    public static class AccessGlue {

        public static Option<Form> GetForm(this Application accessApp, string name) {
            foreach (Form f in accessApp.Forms) {
                if (f.Name == name) return new Some<Form>(f);
            }
            return new None<Form>();
        }

        public static Option<Control> GetControl(this Form form, string name) {
            foreach (Control c in form.Controls) {
                if (c.Name == name) return new Some<Control>(c);
            }
            return new None<Control>();
        }

        public static string GetDirectory(this Application accessApp) {
            return System.IO.Path.GetDirectoryName(accessApp.CurrentDb().Name);
        }

        public static DataContext GetDataContext(this Application accessApp) {
            var conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + accessApp.CurrentDb().Name + ";");
            var dc = new DataContext(conn);
            //dc.Log = Logger.logStream;
            return dc;
        }

        public static string Template(this Application accessApp, string templateName) {
            return accessApp.GetDirectory() + "/templates/" + templateName;
        }

        public static bool BooleanValue(this Control control) {
            return control.OldValue == -1;
        }

        public static string StringValue(this Control control) {
            return control.OldValue;
        }

        public static int IntegerValue(this Control control) {
            if (control.OldValue is string) {
                return Int32.Parse(control.OldValue);
            } else {
                return control.OldValue;
            }
        }

        public static Option<int> OptionalComboBoxIntegerValue(this Control control) {
            if (control is ComboBox && ((ComboBox) control).ListIndex == -1) {
                return new None<int>();
            } else {
                if (control.OldValue == null) {
                    return new None<int>();
                } else if (control.OldValue is string) {
                    return new Some<int>(Int32.Parse(control.OldValue));
                } else {
                    return new Some<int>(control.OldValue);
                }
            }
        }

        public static DateTime DateTimeValue(this Control control) {
            return control.OldValue;
        }

    }
}
