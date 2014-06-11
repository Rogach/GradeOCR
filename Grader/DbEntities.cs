using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.OleDb;
using Grader.util;
using LibUtil;

namespace Grader {

    [Table]
    public class Военнослужащий {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Фамилия { get; set; }
        [Column]
        public string Имя { get; set; }
        [Column]
        public string Отчество { get; set; }
        [Column]
        public int КодЗвания { get; set; }
        [Column]
        public int КодПодразделения { get; set; }
        [Column]
        public int ВУС { get; set; }
        [Column]
        public string ТипВоеннослужащего { get; set; }
        [Column]
        public bool Убыл { get; set; }
        [Column]
        public int КМН { get; set; }
        [Column]
        public int sortWeight { get; set; }
        [Column]
        public bool НетДопускаНаЭкзамен { get; set; }

        private EntityRef<Звание> _Звание;
        [Association(Storage = "_Звание", ThisKey = "КодЗвания")]
        public Звание Звание {
            get { return _Звание.Entity; }
            set { _Звание.Entity = value; }
        }

        public string GetFullName() {
            return Звание.Название + " " + ФИО;
        }

        public string ФИО {
            get {
                return Фамилия + " " + Имя[0] + "." + Отчество[0] + ".";
            }
        }
    }

    [Table]
    public class ВусНаЦикле {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int КодЦикла { get; set; }
        [Column]
        public int ВУС { get; set; }
    }

    [Table]
    public class Должность {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Название { get; set; }
        [Column]
        public int КодПодразделения { get; set; }
        [Column]
        public int КодВоеннослужащего { get; set; }
    }

    [Table]
    public class Звание {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int order { get; set; }
        [Column]
        public string Название { get; set; }
    }

    [Table]
    public class Оценка {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int КодПроверяемого { get; set; }
        [Column]
        public int КодПредмета { get; set; }
        [Column]
        public bool ЭтоКомментарий { get; set; }
        [Column]
        public int Значение { get; set; }
        [Column]
        public string Текст { get; set; }
        [Column]
        public int КодПодразделения { get; set; }
        [Column]
        public int ВУС { get; set; }
        [Column]
        public string ТипВоеннослужащего { get; set; }
        [Column]
        public int КодЗвания { get; set; }
        [Column]
        public int КодВедомости { get; set; }
    }

    [Table]
    public class Подразделение {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Имя { get; set; }
        [Column]
        public int КодКомандира { get; set; }
        [Column]
        public string Тип { get; set; }
        [Column]
        public string ТипОбучения { get; set; }
        [Column]
        public Boolean Актив { get; set; }
        [Column]
        public string ИмяРодительный { get; set; }
        [Column]
        public string ИмяПредложный { get; set; }
        [Column]
        public string ИмяКраткое { get; set; }
        [Column]
        public bool ПодразделениеОхраны { get; set; }

        public Option<Военнослужащий> Командир(DataContext dc) {
            var query =
                from subunit in dc.GetTable<Подразделение>()
                join soldier in dc.GetTable<Военнослужащий>() on subunit.КодКомандира equals soldier.Код
                where subunit.Код == Код
                select soldier;
            return Options.HeadOption(query.ToList());
        }

        public override string ToString() {
            return Имя;
        }
    }

    [Table]
    public class ПодразделениеПодчинение {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int КодПодразделения { get; set; }
        [Column]
        public int КодСтаршегоПодразделения { get; set; }
    }

    [Table]
    public class Предмет {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Название { get; set; }
        [Column]
        public string ПолноеНазвание { get; set; }
        [Column]
        public string НазваниеДательный { get; set; }
        [Column]
        public bool ДЗД { get; set; }
    }

    [Table]
    public class Ведомость {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Название { get; set; }
        [Column]
        public DateTime ДатаЗаполнения { get; set; }
        [Column]
        public DateTime ДатаВнесения { get; set; }
        [Column]
        public DateTime ДатаИзменения { get; set; }
        [Column]
        public bool Виртуальная { get; set; }
        [Column]
        public bool Включена { get; set; }
    }

    [Table]
    public class ВедомостьЗапись {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int КодВоеннослужащего { get; set; }
        [Column]
        public int КодВедомости { get; set; }
        [Column]
        public int Порядок { get; set; }
    }

    [Table]
    public class ВедомостьПредмет {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public int КодПредмета { get; set; }
        [Column]
        public int КодВедомости { get; set; }
        [Column]
        public int Порядок { get; set; }
    }

    [Table]
    public class ВедомостьТег {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string Тег { get; set; }
        [Column]
        public int КодВедомости { get; set; }
    }


    // ACCESS-SIDE QUERIES

    [Table]
    public class ВоеннослужащийПоПодразделениям {
        [Column(IsPrimaryKey = true)]
        public int Код { get; set; }
        [Column]
        public string ФИО { get; set; }
        [Column]
        public string Фамилия { get; set; }
        [Column]
        public string Имя { get; set; }
        [Column]
        public string Отчество { get; set; }
        [Column]
        public int КодПодразделения { get; set; }
        [Column]
        public string ТипВоеннослужащего { get; set; }
        [Column]
        public int ВУС { get; set; }
        [Column]
        public string Звание { get; set; }
        [Column]
        public int КодЗвания { get; set; }
        [Column]
        public int КодСтаршегоПодразделения { get; set; }
        [Column]
        public int КМН { get; set; }
        [Column]
        public bool Убыл { get; set; }
        [Column]
        public int sortWeight { get; set; }
        [Column]
        public bool НетДопускаНаЭкзамен { get; set; }
    }

}
