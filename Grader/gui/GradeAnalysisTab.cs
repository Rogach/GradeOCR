using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;
using Grader.grades;

namespace Grader.gui {
    public class GradeAnalysisTab : TabPage {
        private DataAccess dataAccess;

        public GradeAnalysisTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private PersonFilter personFilter;
        private TextBox tags;
        private ComboBox minRankSelector;
        private ComboBox maxRankSelector;
        private ComboBox subjectSelector;

        private Button gradeListButton;

        private Button gradeSummaryButton;
        private CheckBox withSummaryGrade;

        private Button averageGradesForAllSubjectsButton;
        private Button classnostActButton;
        private Button allClassnostActsButton;
        private Button classnostListButton;
        private Button currentSummaryButton;

        private Button dzdButton;
        private CheckBox withoutKMN;

        private Button gradesByBatallionButton;
        private Button gradesByCompanyButton;
        private Button gradesByPlatoonButton;
        private Button gradesByCycleButton;
        private CheckBox displayAllSubunits;
        private CheckBox displayAllSubjects;

        private Button analysisButton;
        private ComboBox analysisType;

        private ErrorProvider errorProvider;

        private void InitializeComponent() {
            DataContext dc = dataAccess.GetDataContext();
            this.Text = "Анализ оценок";
            this.Size = new Size(1200, 800);

            errorProvider = new ErrorProvider();

            this.SuspendLayout();

            LayoutFilter();
            LayoutAnalysis();

            this.ResumeLayout(false);
        }

        private void LayoutFilter() {
            FormLayout layout = new FormLayout(this, maxLabelWidth: 87);

            dateFrom = layout.Add("от", new DateTimePicker());
            dateFrom.Format = DateTimePickerFormat.Long;
            dateFrom.ShowCheckBox = true;
            dateFrom.Checked = false;

            dateTo = layout.Add("до", new DateTimePicker());
            dateTo.Format = DateTimePickerFormat.Long;
            dateTo.ShowCheckBox = true;
            dateTo.Checked = false;

            layout.PerformLayout();

            personFilter = new PersonFilter(dataAccess);
            personFilter.Location = new Point(3, layout.GetY() + 5);
            this.Controls.Add(personFilter);

            FormLayout layout2 = new FormLayout(this, maxLabelWidth: 87, y: layout.GetY() + 5 + personFilter.Height + 10);

            tags = layout2.Add("Тэги", new TextBox());

            List<string> ranks = 
                dataAccess.GetDataContext().GetTable<Звание>()
                .OrderByDescending(r => r.order)
                .Select(r => r.Название)
                .ToList();

            maxRankSelector = layout2.Add("Звание <=", new ComboBox());
            maxRankSelector.Items.AddRange(ranks.ToArray());
            maxRankSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            maxRankSelector.AutoCompleteSource = AutoCompleteSource.ListItems;
            maxRankSelector.SelectedItem = "полковник";

            minRankSelector = layout2.Add("Звание >=", new ComboBox());
            minRankSelector.Items.AddRange(ranks.ToArray());
            minRankSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            minRankSelector.AutoCompleteSource = AutoCompleteSource.ListItems;
            minRankSelector.SelectedItem = "рядовой";

            layout2.AddSpacer(15);

            List<string> subjectNames =
                dataAccess.GetDataContext().GetTable<Предмет>()
                .Select(s => s.Название)
                .OrderBy(s => s)
                .ToList();

            subjectSelector = layout2.Add("Предмет", new ComboBox());
            subjectSelector.Items.AddRange(subjectNames.ToArray());
            subjectSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            subjectSelector.AutoCompleteSource = AutoCompleteSource.ListItems;

            layout2.PerformLayout();

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(270, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);
        }

        IQueryable<Оценка> GetGradeQuery() {
            DataContext dc = dataAccess.GetDataContext();
            List<string> selectedTags = RegisterEditor.SplitTags(tags.Text);
            return
                from grade in personFilter.GetGradeQuery()
                from rank in dc.GetTable<Звание>()
                where grade.КодЗвания == rank.Код

                from subj in dc.GetTable<Предмет>()
                where grade.КодПредмета == subj.Код
                where subjectSelector.SelectedItem == null ||  subj.Название == ((string) subjectSelector.SelectedItem)

                from r in dc.GetTable<Ведомость>()
                where grade.КодВедомости == r.Код

                where r.Включена == 1
                where !dateFrom.Checked || r.ДатаЗаполнения >= dateFrom.Value.Date
                where !dateTo.Checked || r.ДатаЗаполнения <= dateTo.Value.Date

                from minRank in dc.GetTable<Звание>()
                where minRank.Название == ((string) minRankSelector.SelectedItem)
                where rank.order >= minRank.order

                from maxRank in dc.GetTable<Звание>()
                where maxRank.Название == ((string) maxRankSelector.SelectedItem)
                where rank.order <= maxRank.order

                where selectedTags.Count == 0 ||
                    (from t in dc.GetTable<ВедомостьТег>()
                     where t.КодВедомости == r.Код
                     where selectedTags.Contains(t.Тег)
                     select t).SingleOrDefault() != default(ВедомостьТег)

                select grade;
        }

        private void LayoutAnalysis() {
            FormLayout layout = new FormLayout(this, x: 280, maxLabelWidth: 50);

            gradeListButton = layout.AddFullRow(new Button());
            gradeListButton.Text = "Список оценок";
            gradeListButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                if (subjectSelector.SelectedItem == null) {
                    errorProvider.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
                    errorProvider.SetError(subjectSelector, "Выберите предмет!");
                } else {
                    GradeListGenerator.GenerateGradeList(dataAccess, GetGradeQuery(), (string) subjectSelector.SelectedItem);
                }
            });

            gradeSummaryButton = layout.AddFullRow(new Button());
            gradeSummaryButton.Text = "Сводка";
            gradeSummaryButton.Click += new EventHandler(delegate {
                
            });

            layout.PerformLayout();
        }
    }
}
