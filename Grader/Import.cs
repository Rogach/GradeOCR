using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUtil.wrapper.excel;

namespace Grader {
    public static class Import {
        public static void ImportCadets(Entities et) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите файл с данными курсантов";
            ofd.Multiselect = false;
            ofd.Filter = "Файлы Excel|*.xls;*.xlsx;*.xlsb;*.xlsm";
            if (ofd.ShowDialog() == DialogResult.OK) {
                var excelApp = new ExcelApplication();
                var sh = excelApp.OpenWorkbook(ofd.FileName).Worksheets.First();

                // load headers
                Dictionary<string, int> headerOffset = new Dictionary<string, int>();
                var h = sh.GetRange("A1");
                while (h.Value != null) {
                    headerOffset.Add(h.Value.ToString().ToLower(), h.Column - 1);
                    h = h.GetOffset(0, 1);
                }
                
                var r = sh.GetRange("A2");
                Func<ExcelRange, string, string> field = (rng, colName) => rng.GetOffset(0, headerOffset[colName]).Value.ToString();
                while (r.Value != null) {
                    et.Военнослужащий.AddObject(new Военнослужащий {
                        Фамилия = field(r, "фамилия"),
                        Имя = field(r, "имя"),
                        Отчество = field(r, "отчество"),
                        КодЗвания = et.rankNameToId[field(r, "звание")],
                        КодПодразделения = et.subunitShortNameToId[field(r, "подразделение")],
                        ТипВоеннослужащего = "курсант"
                    });
                    r = r.GetOffset(1, 0);
                }
                et.SaveChanges();
            }
        }
    }
}
