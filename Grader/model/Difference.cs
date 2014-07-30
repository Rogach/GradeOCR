using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil.wrapper.excel;
using LibUtil.templates;
using LibUtil;

namespace Grader.model {
    public static class Difference {
        public static void CalculateCadetDifference(Entities et) {
            Import.WithExcelSheet("Выберите файл с данными курсантов", sh => {
                var inputMap = new Dictionary<Tuple<string, string, string>, int>();

                var field = Import.GetField(sh);
                var r = sh.GetRange("A2");
                while (r.Value != null) {
                    inputMap.Add(
                        new Tuple<string, string, string>(field(r, "фамилия"), field(r, "имя"), field(r, "отчество")), 
                        et.subunitShortNameToId[field(r, "подразделение")]);
                    r = r.GetOffset(1, 0);
                }
                
                var outputSheet = ExcelTemplates.CreateEmptyExcelTable();
                var output = outputSheet.GetRange("A1");

                var dataMap = new Dictionary<Tuple<string, string, string>, int>();

                var cadetQuery =
                    from cadet in et.Военнослужащий
                    join subunit in et.Подразделение on cadet.КодПодразделения equals subunit.Код
                    where !cadet.Убыл
                    where cadet.ТипВоеннослужащего == "курсант"
                    where subunit.ТипОбучения == "срочники"
                    select cadet;
                var cadetList = cadetQuery.ToList();

                ProgressDialogs.ForEach(cadetList, cadet => {
                    var fio = new Tuple<string, string, string>(cadet.Фамилия, cadet.Имя, cadet.Отчество);
                    try {
                        dataMap.Add(fio, cadet.КодПодразделения);
                    } catch (ArgumentException) {
                        Console.WriteLine("cadet " + fio + " has duplicates in database");
                    }

                    Option<int> optSubunitId = inputMap.GetOption(fio);
                    if (optSubunitId.NonEmpty()) {
                        if (optSubunitId.Get() != cadet.КодПодразделения) {
                            // cadet was moved between subunits
                            output.Value = cadet.Фамилия;
                            output.GetOffset(0, 1).Value = cadet.Имя;
                            output.GetOffset(0, 2).Value = cadet.Отчество;
                            output.GetOffset(0, 3).Value = et.subunitIdToShortName[optSubunitId.Get()];
                            output.GetOffset(0, 3).BackgroundColor = ExcelEnums.Color.Blue;
                            output = output.GetOffset(1, 0);
                        }
                    } else {
                        // cadet was deleted
                        output.Value = cadet.Фамилия;
                        output.GetOffset(0, 1).Value = cadet.Имя;
                        output.GetOffset(0, 2).Value = cadet.Отчество;
                        output.GetOffset(0, 3).Value = et.subunitIdToShortName[dataMap[fio]];
                        output.GetResize(1, 4).BackgroundColor = ExcelEnums.Color.Red;
                        output = output.GetOffset(1, 0);
                    }

                });

                ProgressDialogs.ForEach(inputMap.Keys, fio => {
                    if (dataMap.GetOption(fio).IsEmpty()) {
                        // new cadet was added
                        output.Value = fio.Item1;
                        output.GetOffset(0, 1).Value = fio.Item2;
                        output.GetOffset(0, 2).Value = fio.Item3;
                        output.GetOffset(0, 3).Value = et.subunitIdToShortName[inputMap[fio]];
                        output.GetResize(1, 4).BackgroundColor = ExcelEnums.Color.Green;
                        output = output.GetOffset(1, 0);
                    }
                });

                ExcelTemplates.ActivateExcel(outputSheet);
            });
        }
    }
}
