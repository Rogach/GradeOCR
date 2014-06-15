using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;

namespace Grader.gui {
    public class PredefinedPersonLists : ListBox {
        private DataAccess dataAccess;

        public PredefinedPersonLists(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            this.Items.Clear();
            this.Items.AddRange(predefinedLists.ToArray());
            this.SelectedIndex = 0;
            this.SelectionMode = SelectionMode.One;
        }

        private PersonList GetSelectedList() {
            return (PersonList) this.SelectedItem;
        }

        public IQueryable<Оценка> GetGradeQuery() {
            DataContext dc = dataAccess.GetDataContext();
            return
                from grade in dc.GetTable<Оценка>()
                from soldier in GetPersonQuery()
                where grade.КодПроверяемого == soldier.Код
                select grade;
        }

        public IQueryable<ВоеннослужащийПоПодразделениям> GetPersonQuery() {
            DataContext dc = dataAccess.GetDataContext();
            List<int> personIds = GetSelectedList().soldierIds;
            return
                from s in dc.GetTable<ВоеннослужащийПоПодразделениям>()
                where personIds.Contains(s.Код)
                where s.КодСтаршегоПодразделения == s.КодПодразделения
                where s.Убыл == 0
                select s;
        }

        public List<ВоеннослужащийПоПодразделениям> GetPersonList() {
            List<int> personIds = GetSelectedList().soldierIds;
            return GetPersonQuery().ToList().OrderBy(p => personIds.IndexOf(p.Код)).ToList();
        }

        public string GetRegisterName() {
            return GetSelectedList().registerName;
        }

        private class PersonList {
            public string name { get; set; }
            public string registerName { get; set; }
            public List<int> soldierIds { get; set; }

            public override string ToString() {
                return name;
            }
        }

        private List<PersonList> predefinedLists = new List<PersonList> {
            new PersonList {
                name = "офицеры 1 группы", registerName = "офицеров 1 группы",
                soldierIds = new List<int> { 
                    6320, // Швец
                    6336, // Федосеев
                    6353, // Попов
                    6357, // Шахмин
                    6342, // Чистикин
                    6321, // Федоров
                    6337, // Демченко
                    6350, // Кочетков
                    6338, // Дружинин
                    6328, // Минко
                    6323, // Момотов
                    6355, // Самаковский
                    6349, // Третьяков
                    6354, // Шевченко
                    6348, // Баскаков
                    6329, // Перепелицин
                    6358, // Рыбаков
                    6339, // Снесарев
                    6325, // Спиридонов
                    6316, // Новолоака
                    6356, // Барашков
                    6326, // Вострикова
                    6525, // Дзись
                    6361, // Макуров
                    6540, // Митюнин
                    6458, // Прохоров
                    6576, // Шендрик
                    6570, // Виницкий
                    6513, // Могильный
                    6409, // Титаренко
                    6552, // Бакалдин
                    6561  // Федоров
                }
            }
        };
    }
}
