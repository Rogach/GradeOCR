using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil.wrapper.excel;
using LibUtil.templates;
using LibUtil;

namespace Grader.model {
    public static class Difference {
        private class SecondaryData {
            public int subunitId { get; set; }
            public int vus { get; set; }
        }

        public static void CalculateCadetDifference(Entities et) {
            Import.WithExcelSheet("Выберите файл с данными курсантов", sh => {
                var inputMap = new Dictionary<Tuple<string, string, string>, SecondaryData>();

                var field = Import.GetField(sh);
                var r = sh.GetRange("A2");
                while (r.Value != null) {
                    inputMap.Add(
                        new Tuple<string, string, string>(field(r, "фамилия"), field(r, "имя"), field(r, "отчество")),
                        new SecondaryData {
                            subunitId = et.subunitShortNameToId[field(r, "подразделение")],
                            vus = int.Parse(field(r, "вус"))
                        }
                    );
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

                foreach (var cadet in cadetList) {
                    var fio = new Tuple<string, string, string>(cadet.Фамилия, cadet.Имя, cadet.Отчество);
                    try {
                        dataMap.Add(fio, cadet.КодПодразделения);
                    } catch (ArgumentException) {
                        Console.WriteLine("cadet " + fio + " has duplicates in database");
                    }

                    Option<SecondaryData> optSecondaryData = inputMap.GetOption(fio);
                    if (optSecondaryData.NonEmpty()) {
                        bool subunitChanged = optSecondaryData.Get().subunitId != cadet.КодПодразделения;
                        bool vusChanged = optSecondaryData.Get().vus != cadet.ВУС;
                        if (subunitChanged || vusChanged) {
                            output.Value = cadet.Фамилия;
                            output.GetOffset(0, 1).Value = cadet.Имя;
                            output.GetOffset(0, 2).Value = cadet.Отчество;

                            output.GetOffset(0, 3).Value = et.subunitIdToShortName[optSecondaryData.Get().subunitId];
                            if (subunitChanged) output.GetOffset(0, 3).BackgroundColor = ExcelEnums.Color.Azure;

                            output.GetOffset(0, 4).Value = optSecondaryData.Get().vus;
                            if (vusChanged) output.GetOffset(0, 4).BackgroundColor = ExcelEnums.Color.Azure;

                            output = output.GetOffset(1, 0);
                        }
                    } else {
                        // cadet was deleted
                        output.Value = cadet.Фамилия;
                        output.GetOffset(0, 1).Value = cadet.Имя;
                        output.GetOffset(0, 2).Value = cadet.Отчество;
                        output.GetOffset(0, 3).Value = et.subunitIdToShortName[dataMap[fio]];
                        output.GetResize(1, 4).BackgroundColor = ExcelEnums.Color.PaleVioletRed;
                        output = output.GetOffset(1, 0);
                    }

                };

                foreach (var fio in inputMap.Keys) {
                    if (dataMap.GetOption(fio).IsEmpty()) {
                        // new cadet was added
                        output.Value = fio.Item1;
                        output.GetOffset(0, 1).Value = fio.Item2;
                        output.GetOffset(0, 2).Value = fio.Item3;
                        output.GetOffset(0, 3).Value = et.subunitIdToShortName[inputMap[fio].subunitId];
                        output.GetOffset(0, 4).Value = inputMap[fio].vus;
                        output.GetResize(1, 5).BackgroundColor = ExcelEnums.Color.PaleGreen;
                        output = output.GetOffset(1, 0);
                    }
                };

                outputSheet.GetRange("A1").EntireColumn.AutoFit();
                outputSheet.GetRange("A2").EntireColumn.AutoFit();
                outputSheet.GetRange("A3").EntireColumn.AutoFit();
                outputSheet.GetRange("A4").EntireColumn.AutoFit();
                outputSheet.GetRange("A5").EntireColumn.AutoFit();

                ExcelTemplates.ActivateExcel(outputSheet);
            });
        }
    }
}
