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

namespace Grader.registers {

    public class RegisterSettings {
        public Подразделение subunit { get; set; }
        public string subunitName { get; set; }
        public DateTime registerDate { get; set; }
        public RegisterType registerType { get; set; }
        public bool onlyKMN { get; set; }
        public bool strikeKMN { get; set; }
        public List<Военнослужащий> soldiers { get; set; }
    }

    public abstract class RegisterSpec {
        public string templateName;
        public abstract void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings);
    }

    public abstract class GeneralRegister : RegisterSpec {
        public int columnCount;
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            if (settings.onlyKMN) {
                ExcelTemplates.SetRange(sh, "ИмяПодразделения", "кандидатов на замещение вакантных должностей");
            } else {
                if (settings.subunit != null) {
                    ExcelTemplates.SetRange(sh, "ИмяПодразделения", settings.subunit.ИмяРодительный);
                } else {
                    ExcelTemplates.SetRange(sh, "ИмяПодразделения", settings.subunitName);
                }
            }

            ExcelTemplates.ReplaceRange(sh, "RegisterDate", "$month$", ReadableTextUtil.GetMonthGenitive(settings.registerDate));
            ExcelTemplates.ReplaceRange(sh, "RegisterDate", "$year$", settings.registerDate.ToString("yyyy"));

            if (settings.subunit != null && settings.subunit.Тип == "взвод") {
                ExcelTemplates.AppendRange(sh, "КВ", Querying.GetSubunitCommander(et, settings.subunit.Код).Map(s => s.GetFullName(et)).GetOrElse(""));
            } else {
                ExcelTemplates.DeleteRow(sh, "КВ");
            }

            if (settings.subunit == null) {
                ExcelTemplates.DeleteRow(sh, "Подпись");
            } else {
                Option<Военнослужащий> commander;
                Подразделение signatureSubunit;

                if (settings.subunit.Тип == "взвод") {
                    commander = Querying.GetCompanyCommander(et, settings.subunit.Код);
                    signatureSubunit = Querying.GetCompany(et, settings.subunit.Код).Get();
                } else {
                    commander = Querying.GetCommander(et, settings.subunit.Код);
                    signatureSubunit = settings.subunit;
                }
                ExcelTemplates.SetRange(
                        sh, "Подпись",
                        String.Format(
                            "Командир {0}: {1}                    {2}",
                            signatureSubunit.ИмяРодительный,
                            commander.Map(c => et.rankIdToName[c.КодЗвания]).GetOrElse("???"),
                            commander.Map(c => c.ФИО()).GetOrElse("???")));
            }

            if (settings.registerType == RegisterType.экзамен && settings.subunit.Тип == "взвод") {
                List<Военнослужащий> soldiers = new List<Военнослужащий>();
                Querying.GetSubunitCommander(et, settings.subunit.Код).ForEach(commander => {
                    soldiers.Add(commander);
                });
                Querying.GetPostForSubunit(et, settings.subunit.Код, "ЗКВ").ForEach(zkv => {
                    soldiers.Add(zkv);
                });
                Querying.GetPostForSubunit(et, settings.subunit.Код, "КО").ForEach(ko => {
                    soldiers.Add(ko);
                });
                soldiers.AddRange(settings.soldiers);
                settings.soldiers = soldiers;
            }

            InsertSoldiers(et, sh, settings);

            if (settings.registerType == RegisterType.экзамен || settings.registerType == RegisterType.зачет) {
                sh.GetRange("EmptyRows").GetResize(5, 1).EntireRow.Delete();
            }
        }

        public virtual void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(
                et,
                sh, 
                settings.soldiers, 
                isExam: settings.registerType == RegisterType.экзамен, 
                useShortNames: true, 
                strikeKMN: settings.strikeKMN, 
                strikeLen: columnCount
            );
        }
    }

    public abstract class ВедомостьОВП : GeneralRegister {
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            ExcelTemplates.ReplaceRange(sh, "Заголовок", "$registerType$", GetRegisterHeader(settings.registerType));
            var teachers = Querying.GetPostsForSubunit(et, settings.subunit.Код, "преподаватель").ToList();
            if (settings.subunit.Тип == "взвод" && teachers.Count > 0) {
                ExcelTemplates.AppendRange(sh, "Преподаватели", 
                    teachers.Select(c => c.GetFullName(et)).MkString());
            } else {
                ExcelTemplates.DeleteRow(sh, "Преподаватели");
            }
        }

        string GetRegisterHeader(RegisterType registerType) {
            switch (registerType) {
                case RegisterType.экзамен: return "сдачи выпускных экзаменов";
                case RegisterType.зачет: return "сдачи зачета";
                case RegisterType.контрольное_занятие: return "контрольного занятия";
                default: throw new ArgumentException("Unexpected register type: " + registerType);
            }
        }

        public override string ToString() { return "ОВП"; }
    }

    public class GenericRegister : ВедомостьОВП {
        string subject;
        public GenericRegister(string subject) {
            templateName = "ведомость_курсанты_стд.xlsx";
            columnCount = 10;
            this.subject = subject;
        }
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            ExcelTemplates.ReplaceRange(sh, "Заголовок", "$subjectName$", GetHeaderSubject(subject));
        }

        string GetHeaderSubject(string subject) {
            switch (subject) {
                case "ОВУ": return "общевоинским уставам ВС РФ";
                case "АВТ": return "автомобильной подготовке";
                case "ОБВС": return "основам безопасности военной службы";
                case "ОЗГТ": return "основам защиты государственной тайны";
                case "ЭО": return "экологическому обучению";
                case "ОГП": return "общественно-государственной подготовке";
                case "ИНЖ": return "и   нженерной подготовке";
                case "ВМП": return "военно-медицинской подготовке";
                case "ТСП": return "тактико-специальной подготовке";
                case "ТОП": return "топографической подготовке";
                case "МП": return "методической подгтовке";
                case "ТАК": return "тактической подготовке";
                default: throw new ArgumentException("Unexpected subject: " + subject);
            }
        }

        public override string ToString() { return subject; }
    }

    public class ВедомостьОГН : GeneralRegister {
        public ВедомостьОГН() {
            templateName = "ведомость_курсанты_ОГН.xlsx";
            columnCount = 13;
        }
        public override string ToString() { return "ОГН"; }
    }
    public class ВедомостьПолныеИмена : GeneralRegister {
        override public void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(
                et,
                sh, 
                settings.soldiers, 
                isExam: settings.registerType == RegisterType.экзамен, 
                useShortNames: false, 
                strikeKMN: settings.strikeKMN, 
                strikeLen: columnCount
            );
        }
    }
    public class СводнаяВедомость : ВедомостьПолныеИмена {
        public СводнаяВедомость() {
            templateName = "ведомость_курсанты_сводная.xlsx";
            columnCount = 9;
        }
        public override string ToString() { return "сводная"; }
    }
    public class СводнаяНевыносимыеПредметыВедомость : ВедомостьПолныеИмена {
        public СводнаяНевыносимыеПредметыВедомость() {
            templateName = "ведомость_курсанты_сводная_невыносимые_предметы.xlsx";
            columnCount = 6;
        }
        public override string ToString() { return "сводная (невыносимые)"; }
    }
    public class ВедомостьОВПиВУС : ВедомостьОВП {
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            List<int> vuses = settings.soldiers.Select(s => s.ВУС).Distinct().ToList();
            if (vuses.Count == 1) {
                ExcelTemplates.AppendRange(sh, "ВУС", vuses.First());
            } else {
                ExcelTemplates.DeleteRow(sh, "ВУС");
            }
        }
    }

    public class ВедомостьСП : ВедомостьОВПиВУС {
        public ВедомостьСП() {
            templateName = "ведомость_курсанты_СП.xlsx";
            columnCount = 10;
        }
        public override string ToString() { return "СП"; }
    }
    public class ВедомостьСТР : ВедомостьОВП {
        public ВедомостьСТР() {
            templateName = "ведомость_курсанты_СТР.xlsx";
            columnCount = 14;
        }
        public override string ToString() { return "СТР"; }
    }
    public class ВедомостьТП : ВедомостьОВПиВУС {
        public ВедомостьТП() {
            templateName = "ведомость_курсанты_ТП.xlsx";
            columnCount = 10;
        }
        public override string ToString() { return "ТП"; }
    }
    public class ВедомостьФП : ВедомостьОВП {
        public ВедомостьФП() {
            templateName = "ведомость_курсанты_ФП.xlsx";
            columnCount = 17;
        }
        public override string ToString() { return "ФП"; }
    }
    public class ВедомостьРХБЗ : ВедомостьОВП {
        public ВедомостьРХБЗ() {
            templateName = "ведомость_курсанты_РХБЗ.xlsx";
            columnCount = 10;
        }
        public override string ToString() { return "РХБЗ"; }
    }
    public class СверочнаяВедомость : ВедомостьПолныеИмена {
        public СверочнаяВедомость() {
            templateName = "ведомость_курсанты_сверка.xlsx";
            columnCount = 2;
        }
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            ExcelTemplates.AppendRange(sh, "ЗКВ", Querying.GetPostForSubunit(et, settings.subunit.Код, "ЗКВ").Map(c => c.GetFullName(et)).GetOrElse(""));
            ExcelTemplates.AppendRange(sh, "КО", Querying.GetPostForSubunit(et, settings.subunit.Код, "КО").Map(c => c.GetFullName(et)).GetOrElse(""));
            ExcelTemplates.AppendRange(sh, "Преподаватели", Querying.GetPostsForSubunit(et, settings.subunit.Код, "преподаватель").Select(c => c.GetFullName(et)).MkString());
        }
        public override void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(et, sh, settings.soldiers,
                isExam: settings.registerType == RegisterType.экзамен, useShortNames: false, strikeKMN: settings.strikeKMN, strikeLen: columnCount,
                additionalFormatting: (rng, soldier) => {
                    if (soldier.ВУС != 0) {

                        rng.GetOffset(0, 3).Value = "'" + soldier.ВУС.ToString().PadLeft(3, '0');
                    }
                });
        }
        public override string ToString() { return "сверка"; }
    }

    public static class RegisterFormat {
        public static void InsertSoldierList(Entities et, ExcelWorksheet sh, List<Военнослужащий> soldiers, 
                bool isExam = false, bool useShortNames = false, bool strikeKMN = false, int strikeLen = 1, 
                Action<ExcelRange, Военнослужащий> additionalFormatting = null) {
            sh.GetRange("SoldierList").EntireRow.Copy();
            for (int i = 0; i < soldiers.Count - 1; i++) {
                sh.GetRange("SoldierList").GetOffset(1, 0).EntireRow.Insert(ExcelEnums.Direction.Down);
            }
            ExcelRange c = sh.GetRange("SoldierList");
            int r = 1;
            ProgressDialogs.ForEach(soldiers, soldier => {
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
                if ((strikeKMN && soldier.КМН) || (isExam && soldier.НетДопускаНаЭкзамен)) {
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
    }
}
