﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;
using Grader.grades;
using Grader.enums;

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

        private Button exportToOldGrader;
        private Button exportToGradeList;

        private ErrorProvider errorProvider;

        private void InitializeComponent() {
            DataContext dc = dataAccess.GetDataContext();
            this.Text = "Анализ оценок";
            this.Size = new Size(1200, 800);

            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;

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
            dateFrom.ValueChanged += new EventHandler(delegate {
                errorProvider.SetError(dateFrom, null);
            });

            dateTo = layout.Add("до", new DateTimePicker());
            dateTo.Format = DateTimePickerFormat.Long;
            dateTo.ShowCheckBox = true;
            dateTo.Checked = false;
            dateTo.ValueChanged += new EventHandler(delegate {
                errorProvider.SetError(dateTo, null);
            });

            layout.PerformLayout();

            personFilter = new PersonFilter(dataAccess);
            personFilter.Size = new Size(personFilter.Size.Width + 15, personFilter.Size.Height);
            personFilter.Location = new Point(3, layout.GetY() + 5);
            this.Controls.Add(personFilter);
            personFilter.subunitSelector.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.subunitSelector, null);
            });
            personFilter.selectRelatedSubunits.CheckedChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.selectRelatedSubunits, null);
            });
            personFilter.vusSelector.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.vusSelector, null);
            });
            personFilter.studyType.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.studyType, null);
            });
            personFilter.selectCadets.CheckedChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.selectCadets, null);
            });
            personFilter.selectPermanent.CheckedChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.selectPermanent, null);
            });
            personFilter.selectContract.CheckedChanged += new EventHandler(delegate {
                errorProvider.SetError(personFilter.selectContract, null);
            });

            FormLayout layout2 = new FormLayout(this, maxLabelWidth: 87, y: layout.GetY() + 5 + personFilter.Height + 10);

            tags = layout2.Add("Тэги", new TextBox());
            tags.TextChanged += new EventHandler(delegate {
                errorProvider.SetError(tags, null);
            });

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
            maxRankSelector.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(maxRankSelector, null);
            });

            minRankSelector = layout2.Add("Звание >=", new ComboBox());
            minRankSelector.Items.AddRange(ranks.ToArray());
            minRankSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            minRankSelector.AutoCompleteSource = AutoCompleteSource.ListItems;
            minRankSelector.SelectedItem = "рядовой";
            minRankSelector.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(minRankSelector, null);
            });

            layout2.AddSpacer(15);

            List<string> subjectNames =
                dataAccess.GetDataContext().GetTable<Предмет>()
                .Select(s => s.Название)
                .ToList();
            subjectNames.AddRange(ComplexSubjects.complexSubjectNames);
            subjectNames = subjectNames.OrderBy(s => s).ToList();

            subjectSelector = layout2.Add("Предмет", new ComboBox());
            subjectSelector.Items.AddRange(subjectNames.ToArray());
            subjectSelector.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            subjectSelector.AutoCompleteSource = AutoCompleteSource.ListItems;
            subjectSelector.SelectedValueChanged += new EventHandler(delegate {
                errorProvider.SetError(subjectSelector, null);
            });

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
                where subjectSelector.SelectedItem == null
                   || ComplexSubjects.IsComplexSubject((string) subjectSelector.SelectedItem)
                   ||  subj.Название == ((string) subjectSelector.SelectedItem)

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
            FormLayout layout = new FormLayout(this, x: 300, maxLabelWidth: 50);

            gradeListButton = layout.AddFullRow(new Button());
            gradeListButton.Text = "Список оценок";
            gradeListButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                if (subjectSelector.SelectedItem == null) {
                    errorProvider.SetError(subjectSelector, "Выберите предмет!");
                    return;
                }

                GradeListGenerator.GenerateGradeList(dataAccess, GetGradeQuery(), (string) subjectSelector.SelectedItem);
            });

            gradeSummaryButton = layout.AddFullRow(new Button());
            gradeSummaryButton.Text = "Сводка";
            gradeSummaryButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                bool errors = false;
                if (subjectSelector.SelectedItem == null) {
                    errorProvider.SetError(subjectSelector, "Выберите предмет!");
                    errors = true;
                }
                if (personFilter.subunitSelector.SelectedItem == null) {
                    errorProvider.SetError(personFilter.subunitSelector, "Выберите подразделение!");
                    errors = true;
                }
                if (errors) {
                    return;
                }

                GradeSummaryGenerator.GenerateSummary(
                    dataAccess.GetDataContext(),
                    (Подразделение) personFilter.subunitSelector.SelectedItem,
                    GetGradeQuery(),
                    (string) subjectSelector.SelectedItem,
                    withSummaryGrade.Checked,
                    personFilter.selectCadets.Checked,
                    personFilter.selectRelatedSubunits.Checked
                );
            });

            withSummaryGrade = layout.AddFullRow(new CheckBox());
            withSummaryGrade.Text = "С общей оценкой?";
            withSummaryGrade.Checked = true;

            layout.AddSpacer(15);

            averageGradesForAllSubjectsButton = layout.AddFullRow(new Button());
            averageGradesForAllSubjectsButton.Text = "Средние оценки по всем предметам";
            averageGradesForAllSubjectsButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                AverageGradeSummaryGenerator.GenerateAverageGradeSummary(dataAccess.GetDataContext(), GetGradeQuery());
            });

            layout.AddSpacer(15);

            classnostActButton = layout.AddFullRow(new Button());
            classnostActButton.Text = "Акт на классность";
            classnostActButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                bool errors = false;
                if (personFilter.subunitSelector.SelectedItem == null) {
                    errorProvider.SetError(personFilter.subunitSelector, "Выберите подразделение!");
                    errors = true;
                }
                if (!dateTo.Checked) {
                    errorProvider.SetError(dateTo, "Выберите дату для акта");
                    errors = true;
                }
                if (errors) {
                    return;
                }

                ClassActGenerator.GenerateClassAct(
                    dataAccess,
                    (Подразделение) personFilter.subunitSelector.SelectedItem,
                    dateTo.Value,
                    GetGradeQuery()
                );
            });

            allClassnostActsButton = layout.AddFullRow(new Button());
            allClassnostActsButton.Text = "Все акты на классность";
            allClassnostActsButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                if (!dateTo.Checked) {
                    errorProvider.SetError(dateTo, "Выберите дату для акта");
                    return;
                }

                ClassActGenerator.GenerateAllClassActs(
                    dataAccess,
                    dateTo.Value,
                    GetGradeQuery()
                );
            });

            classnostListButton = layout.AddFullRow(new Button());
            classnostListButton.Text = "Список с классностью";
            classnostListButton.Click += new EventHandler(delegate {
                errorProvider.Clear();

                ClassListGenerator.GenerateClassList(dataAccess.GetDataContext(), GetGradeQuery());
            });

            layout.AddSpacer(15);

            currentSummaryButton = layout.AddFullRow(new Button());
            currentSummaryButton.Text = "Справка хода экзаменов";
            currentSummaryButton.Click += new EventHandler(delegate {
                errorProvider.Clear();

                CurrentSummaryReportGenerator.GenerateCurrentSummaryReport(dataAccess.GetDataContext(), GetGradeQuery());
            });

            layout.AddSpacer(15);

            dzdButton = layout.AddFullRow(new Button());
            dzdButton.Text = "День за днем";
            dzdButton.Click += new EventHandler(delegate {
                errorProvider.Clear();

                PerDaySummaryGenerator.GenerateSummaryPerDay(dataAccess, GetGradeQuery(), withoutKMN.Checked);
            });

            withoutKMN = layout.AddFullRow(new CheckBox());
            withoutKMN.Text = "Без КМН?";

            layout.AddSpacer(15);

            gradesByBatallionButton = layout.AddFullRow(new Button());
            gradesByBatallionButton.Text = "Оценки по батальонам";
            gradesByBatallionButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                CallBySubunitTable("батальон", false);
            });

            gradesByCompanyButton = layout.AddFullRow(new Button());
            gradesByCompanyButton.Text = "Оценки по ротам";
            gradesByCompanyButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                CallBySubunitTable("рота", false);
            });

            gradesByPlatoonButton = layout.AddFullRow(new Button());
            gradesByPlatoonButton.Text = "Оценки по взводам";
            gradesByPlatoonButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                CallBySubunitTable("взвод", false);
            });

            gradesByCycleButton = layout.AddFullRow(new Button());
            gradesByCycleButton.Text = "Оценки по циклам";
            gradesByCycleButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                CallBySubunitTable("цикл", true);
            });

            displayAllSubunits = layout.AddFullRow(new CheckBox());
            displayAllSubunits.Text = "Все подразделения?";

            displayAllSubjects = layout.AddFullRow(new CheckBox());
            displayAllSubjects.Text = "Все предметы?";

            layout.AddSpacer(15);

            analysisButton = layout.AddFullRow(new Button());
            analysisButton.Text = "Сводка для подведения";
            analysisButton.Click += new EventHandler(delegate {
                errorProvider.Clear();
                if (subjectSelector.SelectedItem == null) {
                    errorProvider.SetError(subjectSelector, "Выберите предмет!");
                    return;
                }

                GradeAnalysisGenerator.GenerateGradeAnalysis(
                    dataAccess.GetDataContext(),
                    GetGradeQuery(),
                    (string) subjectSelector.SelectedItem,
                    (string) analysisType.SelectedItem,
                    personFilter.selectCadets.Checked,
                    personFilter.selectPermanent.Checked,
                    personFilter.selectContract.Checked
                );
            });

            analysisType = layout.Add("Тип сводки", new ComboBox());
            analysisType.Items.AddRange(new object[] {
                "по батальонам/ротам",
                "по циклам",
                "по батальонам/циклам",
                "по курсантам на циклах"
            });
            analysisType.SelectedIndex = 0;
            analysisType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            analysisType.AutoCompleteSource = AutoCompleteSource.ListItems;

            layout.AddSpacer(15);

            exportToOldGrader = layout.AddFullRow(new Button());
            exportToOldGrader.Text = "экспорт в старую учетку";
            exportToOldGrader.Click += new EventHandler(delegate {
                errorProvider.Clear();

                OldGraderExporter.ExportToOldGrader(
                    dataAccess,
                    GetGradeQuery(),
                    personFilter.GetPersonQuery(),
                    personFilter.selectCadets.Checked,
                    personFilter.selectContract.Checked
                );
            });

            exportToGradeList = layout.AddFullRow(new Button());
            exportToGradeList.Text = "экспорт в список оценок";
            exportToGradeList.Click += new EventHandler(delegate {
                errorProvider.Clear();

                GradeListExporter.ContractGradeListExport(
                    dataAccess,
                    GetGradeQuery(),
                    personFilter.GetPersonQuery()
                );
            });
            
            layout.PerformLayout();
        }

        private void CallBySubunitTable(string subunitType, bool byVus) {
            AverageGradeTableGenerator.GenerateTableWithResultsBySubunitType(
                dataAccess.GetDataContext(),
                GetGradeQuery(),
                subunitType,
                (string) subjectSelector.SelectedItem,
                byVus,
                displayAllSubunits.Checked,
                displayAllSubjects.Checked,
                personFilter.selectCadets.Checked
            );
        }
    }
}