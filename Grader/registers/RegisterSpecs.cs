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

    public class GradeLocation {
        public string subjectName { get; set; }
        public Point gradesLocation { get; set; }
        public GradeLocation(string subjectName, Point gradesLocation) {
            this.subjectName = subjectName;
            this.gradesLocation = gradesLocation;
        }
    }

    public abstract class RegisterSpec {
        public string specName;
        public string templateName;
        public List<GradeLocation> gradeLocations;
        public int tableLastColumn;
        public abstract void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings);

        public static RegisterSpec FromSpecName(string specName) {
            switch (specName) {
                case "сводная": return new СводнаяВедомость();
                case "сводная (невыносимые)": return new СводнаяНевыносимыеПредметыВедомость();
                case "сверка": return new СверочнаяВедомость();
                case "ОГН": return new ВедомостьОГН();
                case "ОГН (урс)": return new ВедомостьОГН_урс();
                case "ОВУ": return new GenericRegister("ОВУ");
                case "СП": return new ВедомостьСП();
                case "СПТП": return new ВедомостьСПТП();
                case "СТР": return new ВедомостьСТР();
                case "ТП": return new ВедомостьТП();
                case "ФП": return new ВедомостьФП();
                case "РХБЗ": return new ВедомостьФП();
                case "АВТ": return new GenericRegister("АВТ");
                case "ОБВС": return new GenericRegister("ОБВС");
                case "ОЗГТ": return new GenericRegister("ОЗГТ");
                case "ЭО": return new GenericRegister("ЭО");
                case "ОГП": return new GenericRegister("ОГП");
                case "ИНЖ": return new GenericRegister("ИНЖ");
                case "ВМП": return new GenericRegister("ВМП");
                case "ТСП": return new GenericRegister("ТСП");
                case "ТОП": return new GenericRegister("ТОП");
                case "МП": return new GenericRegister("МП");
                case "ТАК": return new GenericRegister("ТАК");
                default: throw new ArgumentException("Unknown register spec: " + specName);
            }
        }

        public static RegisterSpec[] registerSpecs = {
            new СводнаяВедомость(),
            new СводнаяНевыносимыеПредметыВедомость(),
            new СверочнаяВедомость(),
            new ВедомостьОГН(),
            new ВедомостьОГН_урс(),
            new GenericRegister("ОВУ"),
            new ВедомостьСП(),
            new ВедомостьСПТП(),
            new ВедомостьСТР(),
            new ВедомостьТП(),
            new ВедомостьФП(),
            new ВедомостьРХБЗ(),
            new GenericRegister("АВТ"),
            new GenericRegister("ОБВС"),
            new GenericRegister("ОЗГТ"),
            new GenericRegister("ЭО"),
            new GenericRegister("ОГП"),
            new GenericRegister("ИНЖ"),
            new GenericRegister("ВМП"),
            new GenericRegister("ТСП"),
            new GenericRegister("ТОП"),
            new GenericRegister("МП"),
            new GenericRegister("ТАК")
        };
    }

    public abstract class GeneralRegister : RegisterSpec {
        public int gradeColumnCount;
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
                Querying.GetPostsForSubunit(et, settings.subunit.Код, "ЗКВ").ToList().ForEach(zkv => {
                    soldiers.Add(zkv);
                });
                Querying.GetPostsForSubunit(et, settings.subunit.Код, "КО").ToList().ForEach(ko => {
                    soldiers.Add(ko);
                });
                soldiers.AddRange(settings.soldiers);
                settings.soldiers = soldiers;
            }

            InsertSoldiers(et, sh, settings);

            if (settings.registerType == RegisterType.экзамен || settings.registerType == RegisterType.зачет) {
                sh.GetRange("EmptyRows").GetResize(5, 1).EntireRow.Delete();
            }

            if (settings.forOCR)
                RegisterFormat.InsertRegisterCode(et, sh, settings, this);
        }

        public virtual void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(
                et,
                sh,
                settings,
                useShortNames: true,
                strikeLen: gradeColumnCount
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
            specName = subject;
            templateName = "ведомость_курсанты_стд.xlsx";
            gradeColumnCount = 10;
            tableLastColumn = 12;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation(subject, new Point(12, 1))
            };
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
                case "ИНЖ": return "инженерной подготовке";
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
            specName = "ОГН";
            templateName = "ведомость_курсанты_ОГН.xlsx";
            gradeColumnCount = 13;
            tableLastColumn = 16;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("ОГН", new Point(12, 1))
            };
        }
        public override string ToString() { return "ОГН"; }
    }

    public class ВедомостьОГН_урс : GeneralRegister {
        public ВедомостьОГН_урс() {
            specName = "ОГН (урс)";
            templateName = "ведомость_урс_ОГН.xlsx";
            gradeColumnCount = 12;
            tableLastColumn = 14;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("ОГН", new Point(14, 1))
            };
        }
        public override string ToString() { return "ОГН (урс)"; }
    }

    public class ВедомостьПолныеИмена : GeneralRegister {
        override public void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(
                et,
                sh,
                settings,
                useShortNames: false,
                strikeLen: gradeColumnCount
            );
        }
    }
    public class СводнаяВедомость : ВедомостьПолныеИмена {
        public СводнаяВедомость() {
            specName = "сводная";
            templateName = "ведомость_курсанты_сводная.xlsx";
            gradeColumnCount = 9;
            tableLastColumn = 11;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("СП", new Point(3, 0)),
                new GradeLocation("ТП", new Point(4, 0)),
                new GradeLocation("СТР", new Point(5, 0)),
                new GradeLocation("ФП", new Point(6, 0)),
                new GradeLocation("ОВУ", new Point(7, 0)),
                new GradeLocation("ОГН", new Point(8, 0))
            };
        }
        public override string ToString() { return "сводная"; }
    }
    public class ВедомостьСПТП : ВедомостьПолныеИмена {
        public ВедомостьСПТП() {
            specName = "сводная";
            templateName = "ведомость_курсанты_СПТП.xlsx";
            gradeColumnCount = 5;
            tableLastColumn = 8;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("СП", new Point(3, 0)),
                new GradeLocation("ТП", new Point(4, 0))
            };
        }
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            // this must be done before calling base function, because base function
            // can add commanders to soldiers and fuck up vus set
            List<int> vuses = settings.soldiers.Select(s => s.ВУС).Distinct().ToList();
            if (vuses.Count == 1 && vuses.First() != 0) {
                ExcelTemplates.AppendRange(sh, "ВУС", vuses.First());
            } else {
                ExcelTemplates.DeleteRow(sh, "ВУС");
            }
            var teachers = Querying.GetPostsForSubunit(et, settings.subunit.Код, "преподаватель").ToList();
            if (settings.subunit.Тип == "взвод" && teachers.Count > 0) {
                ExcelTemplates.AppendRange(sh, "Преподаватели",
                    teachers.Select(c => c.GetFullName(et)).MkString());
            } else {
                ExcelTemplates.DeleteRow(sh, "Преподаватели");
            }
        }
        public override string ToString() { return "СПТП"; }
    }
    public class СводнаяНевыносимыеПредметыВедомость : ВедомостьПолныеИмена {
        public СводнаяНевыносимыеПредметыВедомость() {
            specName = "сводная (невыносимые)";
            templateName = "ведомость_курсанты_сводная_невыносимые_предметы.xlsx";
            gradeColumnCount = 6;
            tableLastColumn = 8;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("ОГП", new Point(3, 0)),
                new GradeLocation("РХБЗ", new Point(4, 0)),
                new GradeLocation("ВМП", new Point(5, 0))
            };
        }
        public override string ToString() { return "сводная (невыносимые)"; }
    }
    public class ВедомостьОВПиВУС : ВедомостьОВП {
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            // this must be done before calling base function, because base function
            // can add commanders to soldiers and fuck up vus set
            List<int> vuses = settings.soldiers.Select(s => s.ВУС).Distinct().ToList();
            if (vuses.Count == 1 && vuses.First() != 0) {
                ExcelTemplates.AppendRange(sh, "ВУС", vuses.First());
            } else {
                ExcelTemplates.DeleteRow(sh, "ВУС");
            }
            base.Format(et, sh, settings);
        }
    }

    public class ВедомостьСП : ВедомостьОВПиВУС {
        public ВедомостьСП() {
            specName = "СП";
            templateName = "ведомость_курсанты_СП.xlsx";
            gradeColumnCount = 10;
            tableLastColumn = 12;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("СП", new Point(12, 1))
            };
        }
        public override string ToString() { return "СП"; }
    }
    public class ВедомостьСТР : ВедомостьОВП {
        public ВедомостьСТР() {
            specName = "СТР";
            templateName = "ведомость_курсанты_СТР.xlsx";
            gradeColumnCount = 14;
            tableLastColumn = 16;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("СТР", new Point(16, 1))
            };
        }
        public override string ToString() { return "СТР"; }
    }
    public class ВедомостьТП : ВедомостьОВПиВУС {
        public ВедомостьТП() {
            specName = "ТП";
            templateName = "ведомость_курсанты_ТП.xlsx";
            gradeColumnCount = 10;
            tableLastColumn = 12;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("ТП", new Point(12, 1))
            };
        }
        public override string ToString() { return "ТП"; }
    }
    public class ВедомостьФП : ВедомостьОВП {
        public ВедомостьФП() {
            specName = "ФП";
            templateName = "ведомость_курсанты_ФП.xlsx";
            gradeColumnCount = 17;
            tableLastColumn = 19;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("ФП", new Point(19, 1))
            };
        }
        public override string ToString() { return "ФП"; }
    }
    public class ВедомостьРХБЗ : ВедомостьОВП {
        public ВедомостьРХБЗ() {
            specName = "РХБЗ";
            templateName = "ведомость_курсанты_РХБЗ.xlsx";
            gradeColumnCount = 10;
            tableLastColumn = 12;
            gradeLocations = new List<GradeLocation> {
                new GradeLocation("РХБЗ", new Point(12, 1))
            };
        }
        public override string ToString() { return "РХБЗ"; }
    }
    public class СверочнаяВедомость : ВедомостьПолныеИмена {
        public СверочнаяВедомость() {
            specName = "сверка";
            templateName = "ведомость_курсанты_сверка.xlsx";
            gradeColumnCount = 2;
            tableLastColumn = 4;
        }
        public override void Format(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            base.Format(et, sh, settings);
            ExcelTemplates.AppendRange(sh, "ЗКВ", Querying.GetPostForSubunit(et, settings.subunit.Код, "ЗКВ").Map(c => c.GetFullName(et)).GetOrElse(""));
            ExcelTemplates.AppendRange(sh, "КО", Querying.GetPostForSubunit(et, settings.subunit.Код, "КО").Map(c => c.GetFullName(et)).GetOrElse(""));
            ExcelTemplates.AppendRange(sh, "Преподаватели", Querying.GetPostsForSubunit(et, settings.subunit.Код, "преподаватель").ToList().Select(c => c.GetFullName(et)).MkString());
        }
        public override void InsertSoldiers(Entities et, ExcelWorksheet sh, RegisterSettings settings) {
            RegisterFormat.InsertSoldierList(et, sh, settings,
                useShortNames: false, strikeLen: gradeColumnCount,
                additionalFormatting: (rng, soldier) => {
                    if (soldier.ВУС != 0) {
                        rng.GetOffset(0, 3).Value = "'" + soldier.ВУС.ToString().PadLeft(3, '0');
                    } else {
                        rng.GetOffset(0, 3).Value = "";
                    }
                });
        }
        public override string ToString() { return "сверка"; }
    }

}
