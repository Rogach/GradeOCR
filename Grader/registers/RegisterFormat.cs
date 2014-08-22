using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using LibUtil;
using Grader.enums;
using ARCode;
using System.Drawing;
using System.IO;

namespace Grader.registers {

    public static class RegisterFormat {
        public static void InsertSoldierList(Entities et, ExcelWorksheet sh, RegisterSettings settings, 
                bool useShortNames = false, int strikeLen = 1, Action<ExcelRange, Военнослужащий> additionalFormatting = null) {

            sh.GetRange("SoldierList").EntireRow.Copy();
            for (int i = 0; i < settings.soldiers.Count - 1; i++) {
                sh.GetRange("SoldierList").GetOffset(1, 0).EntireRow.Insert(ExcelEnums.Direction.Down);
            }
            ExcelRange c = sh.GetRange("SoldierList");
            int r = 1;
            ProgressDialogs.ForEach(settings.soldiers, soldier => {
                c.Value = r++;
                c.GetOffset(0, 1).Value = et.rankIdToName[soldier.КодЗвания];
                if (useShortNames) {
                    c.GetOffset(0, 2).Value = soldier.ФИО();
                } else {
                    c.GetOffset(0, 2).Value = soldier.Фамилия + " " + soldier.Имя + " " + soldier.Отчество;
                }
                if (additionalFormatting != null) {
                    additionalFormatting(c, soldier);
                }
                if ((settings.strikeKMN && soldier.КМН) || (settings.isExam && soldier.НетДопускаНаЭкзамен)) {
                    ExcelRange strike = c.GetOffset(0, 3).GetResize(1, strikeLen);
                    if (strikeLen > 1) {
                        strike.Merge();
                    }
                    if (soldier.КМН) {
                        strike.Value = "Представлен на экзамен в составе уч. гр. кандидатов на должности МК";
                    } else if (soldier.НетДопускаНаЭкзамен) {
                        strike.Value = "Не допущен решением УМС. Протокол № 5 от 11 апреля 2014 года";
                    }
                    strike.HorizontalAlignment = ExcelEnums.ExcelConstants.Center;
                    strike.Font.Size = 8;
                }

                c = c.GetOffset(1, 0);
            });
        }

        public static void InsertRegisterCode(Entities et, ExcelWorksheet sh, RegisterSettings settings, string specName) {
            string registerTypeString;
            switch (settings.registerType) {
                case RegisterType.зачет: registerTypeString = "ЗАЧ"; break;
                case RegisterType.контрольное_занятие: registerTypeString = "КЗ"; break;
                case RegisterType.экзамен: registerTypeString = "ЭКЗ"; break;
                default: throw new ArgumentException("Unexpected register type");
            }
            string registerName = String.Format(
                "{0}{1} {2} {3}", 
                settings.registerNamePrefix.Length == 0 ? "" : settings.registerNamePrefix + " ",
                registerTypeString,
                settings.subunit == null ? settings.subunitName : settings.subunit.ИмяКраткое,
                specName);

            string soldierList = settings.soldiers.Select(v => v.Код).MkString();
            string skipSoldierList =
                settings.soldiers
                .Where(soldier => (settings.strikeKMN && soldier.КМН) || (settings.isExam && soldier.НетДопускаНаЭкзамен))
                .Select(v => v.Код).MkString();

            var regEntry = new ВедомостьДляРаспознавания {
                ДатаПечати = DateTime.Now,
                ДатаВнесения = null,
                СписокВоеннослужащих = soldierList,
                СписокНенужныхВоеннослужащих = skipSoldierList,
                ТипВедомости = specName,
                ИмяВедомости = registerName,
                Теги = settings.registerTags
            };

            et.ВедомостьДляРаспознавания.AddObject(regEntry);
            et.SaveChanges();

            uint codeValue = (uint) regEntry.Код;

            Bitmap codeImage = ARCodeUtil.BuildCode(codeValue, 60);
            string tempFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
            codeImage.Save(tempFileName);

            var codeRange = sh.GetRange("OCRCode");

            float imageWidth = 168;

            // 1.1f is a fix for known Office bug, where image heights are scaled when printing
            float imageHeight = 28 * 1.1f;

            sh.AddPicture(
                tempFileName,
                (float) (codeRange.Left + codeRange.Width - imageWidth),
                (float) (codeRange.Top),
                imageWidth,
                imageHeight);

            File.Delete(tempFileName);
        }
    }
}
