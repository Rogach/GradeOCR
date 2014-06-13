using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;
using LibUtil;
using Grader.util;
using Grader.enums;

namespace Grader.gui {
    public class PersonFilter : Panel {

        private DataAccess dataAccess;

        public PersonFilter(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        public ComboBox subunitSelector;
        public ComboBox studyType;
        public ComboBox vusSelector;
        public CheckBox selectCadets;
        public CheckBox selectPermanent;
        public CheckBox selectContract;

        private void InitializeComponent() {
            DataContext dc = dataAccess.GetDataContext();

            FormLayout layout = new FormLayout(this, x: 0, y: 0);

            subunitSelector = layout.Add("Подразделение", new ComboBox());
            subunitSelector.Items.AddRange(dc.GetTable<Подразделение>().ToListTimed().ToArray());
            subunitSelector.SelectedIndex = 0;
            subunitSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            subunitSelector.AutoCompleteSource = AutoCompleteSource.ListItems;

            studyType = layout.Add("Тип обучения", new ComboBox());
            studyType.PopulateComboBox(typeof(StudyType));
            studyType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            studyType.AutoCompleteSource = AutoCompleteSource.ListItems;

            vusSelector = layout.Add("ВУС", new ComboBox());
            string[] possibleVuses =
                dc.GetTable<Военнослужащий>().Select(v => v.ВУС).Distinct().ToListTimed()
                .Where(v => v != 0).Select(v => v.ToString()).ToArray();
            vusSelector.Items.AddRange(possibleVuses);
            vusSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            vusSelector.AutoCompleteSource = AutoCompleteSource.ListItems;

            layout.AddSpacer(13);

            selectCadets = layout.Add("", new CheckBox(), thin: true);
            selectCadets.Text = "Курсанты?";
            selectCadets.Checked = true;

            selectPermanent = layout.Add("", new CheckBox(), thin: true);
            selectPermanent.Text = "Постоянные срочники?";

            selectContract = layout.Add("", new CheckBox(), thin: false);
            selectContract.Text = "Контрактники?";

            layout.PerformLayout();

            this.Size = new Size(layout.GetX(), layout.GetY());
        }

        public IQueryable<ВоеннослужащийПоПодразделениям> GetPersonQuery() {
            DataContext dc = dataAccess.GetDataContext();
            Option<int> vus;
            if (vusSelector.SelectedItem == null) {
                vus = new None<int>();
            } else {
                vus = new Some<int>(Int32.Parse((string) vusSelector.SelectedItem));
            }
            Подразделение subunit = (Подразделение) subunitSelector.SelectedItem;
            return Querying.GetSubunitSoldiersQuery(dc, subunit.Код, GetPersonQueryFilter(dc))
                   .Where(s => vus.IsEmpty() || vus.GetOrElse(-1) == s.ВУС);
        }

        private Func<IQueryable<ВоеннослужащийПоПодразделениям>, IQueryable<ВоеннослужащийПоПодразделениям>> GetPersonQueryFilter(DataContext dc) {
            return q => {
                StudyType st = studyType.GetComboBoxEnumValue<StudyType>();
                return
                    from soldier in q
                    from subunit in dc.GetTable<Подразделение>()
                    where soldier.КодПодразделения == subunit.Код

                    where (st == StudyType.все || subunit.ТипОбучения == st.ToString())

                    where
                        (soldier.ТипВоеннослужащего == "курсант" && selectCadets.Checked) ||
                        (soldier.ТипВоеннослужащего == "постоянный срочник" && selectPermanent.Checked) ||
                        (soldier.ТипВоеннослужащего == "контрактник" && selectContract.Checked)
                    select soldier;
            };
        }

    }
}
