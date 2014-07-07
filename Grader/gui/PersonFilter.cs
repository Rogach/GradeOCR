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

        private Entities et;

        public PersonFilter(Entities et) {
            this.et = et;
            this.InitializeComponent();
        }

        public ComboBox subunitSelector;
        public CheckBox selectRelatedSubunits;
        public ComboBox studyType;
        public ComboBox vusSelector;
        public CheckBox selectCadets;
        public CheckBox selectPermanent;
        public CheckBox selectContract;

        private void InitializeComponent() {

            FormLayout layout = new FormLayout(this, x: 0, y: 0);

            subunitSelector = layout.Add("Подразделение", new ComboBox());
            subunitSelector.Items.AddRange(et.Подразделение.ToArray());
            subunitSelector.SelectedIndex = 0;
            subunitSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            subunitSelector.AutoCompleteSource = AutoCompleteSource.ListItems;

            subunitSelector.SelectedValueChanged += new EventHandler(delegate {
                Подразделение subunit = (Подразделение) subunitSelector.SelectedItem;
                if (subunit.ТипОбучения == "срочники") {
                    selectCadets.Checked = true;
                    selectPermanent.Checked = false;
                    selectContract.Checked = false;
                } else if (subunit.ТипОбучения == "" || subunit.ТипОбучения == null) {
                    selectCadets.Checked = false;
                    selectPermanent.Checked = false;
                    selectContract.Checked = true;
                }
            });

            selectRelatedSubunits = layout.AddFullRow(new CheckBox(), leftPadding: 60);
            selectRelatedSubunits.Text = "Подчиненные подразделения?";
            selectRelatedSubunits.Checked = true;

            layout.AddSpacer(10);

            studyType = layout.Add("Тип обучения", new ComboBox());
            studyType.PopulateComboBox(typeof(StudyType));
            studyType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            studyType.AutoCompleteSource = AutoCompleteSource.ListItems;

            vusSelector = layout.Add("ВУС", new ComboBox());
            string[] possibleVuses =
                et.Военнослужащий.Select(v => v.ВУС).Distinct()
                .Where(v => v != 0).ToList().Select(v => v.ToString()).ToArray();
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

        public IQueryable<Оценка> GetGradeQuery() {
            Подразделение selectedSubunit = (Подразделение) subunitSelector.SelectedItem;
            StudyType st = studyType.GetComboBoxEnumValue<StudyType>();
            Option<int> vus;
            if (vusSelector.SelectedItem == null) {
                vus = new None<int>();
            } else {
                vus = new Some<int>(Int32.Parse((string) vusSelector.SelectedItem));
            }
            IQueryable<Оценка> gradeQuery =
                from grade in et.Оценка

                join subunitRel in et.ПодразделениеПодчинение on grade.КодПодразделения equals subunitRel.КодПодразделения
                where subunitRel.КодСтаршегоПодразделения == selectedSubunit.Код
                where (selectRelatedSubunits.Checked || grade.КодПодразделения == selectedSubunit.Код)

                join subunit in et.Подразделение on grade.КодПодразделения equals subunit.Код
                where (st == StudyType.все || subunit.ТипОбучения == st.ToString())

                where
                    (grade.ТипВоеннослужащего == "курсант" && selectCadets.Checked) ||
                    (grade.ТипВоеннослужащего == "постоянный срочник" && selectPermanent.Checked) ||
                    (grade.ТипВоеннослужащего == "контрактник" && selectContract.Checked)
                where vus.IsEmpty() || grade.ВУС == vus.GetOrElse(-1)
                select grade;

            return gradeQuery;
        }

        public IQueryable<Военнослужащий> GetPersonQuery() {
            StudyType st = studyType.GetComboBoxEnumValue<StudyType>();
            Подразделение selectedSubunit = (Подразделение) subunitSelector.SelectedItem;
            Option<int> vus;
            if (vusSelector.SelectedItem == null) {
                vus = new None<int>();
            } else {
                vus = new Some<int>(Int32.Parse((string) vusSelector.SelectedItem));
            }

            var query =
                from soldier in et.Военнослужащий
                join subunitRel in et.ПодразделениеПодчинение on soldier.КодПодразделения equals subunitRel.КодПодразделения
                join subunit in et.Подразделение on soldier.КодПодразделения equals subunit.Код
                where subunitRel.КодСтаршегоПодразделения == selectedSubunit.Код

                where selectRelatedSubunits.Checked || soldier.КодПодразделения == selectedSubunit.Код

                where (st == StudyType.все || subunit.ТипОбучения == st.ToString())

                where
                    (soldier.ТипВоеннослужащего == "курсант" && selectCadets.Checked) ||
                    (soldier.ТипВоеннослужащего == "постоянный срочник" && selectPermanent.Checked) ||
                    (soldier.ТипВоеннослужащего == "контрактник" && selectContract.Checked)
                where vus.IsEmpty() || soldier.ВУС == vus.GetOrElse(-1)
                select soldier;
            return Querying.GetSubunitSoldiers(et, selectedSubunit.Код, query);
        }

    }
}
