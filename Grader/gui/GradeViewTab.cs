using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;
using LibUtil;
using System.Data;
using Grader.util;
using Grader.gui.gridutil;

namespace Grader.gui {
    public class GradeViewTab : TabPage {
        private DataAccess dataAccess;
        Dictionary<string, int> subjectNameToId;
        Dictionary<int, string> subjectIdToName;

        private static string TAB_NAME = "Просмотр оценок";

        public GradeViewTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            subjectNameToId =
                dataAccess.GetDataContext().GetTable<Предмет>()
                .Select(s => new { id = s.Код, name = s.Название })
                .ToListTimed()
                .ToDictionary(s => s.name, s => s.id);
            subjectIdToName =
                dataAccess.GetDataContext().GetTable<Предмет>()
                .Select(s => new { id = s.Код, name = s.Название })
                .ToListTimed()
                .ToDictionary(s => s.id, s => s.name);
            this.InitializeComponent();
        }

        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private PersonSelector personSelector;
        private TextBox tags;
        private Button showGrades;
        private Button saveChanges;
        private Button cancelChanges;
        private DataGridView gradeView;
        private DataSet gradeViewDataSet;
        private DataTable gradeViewDataTable;

        private bool _changesPending = false;
        private bool changesPending {
            get {
                return _changesPending;
            }
            set {
                if (value) {
                    this.Text = TAB_NAME + " *";
                } else {
                    this.Text = TAB_NAME;
                }
                _changesPending = value;
            }
        }

        private Dictionary<Tuple<int, int>, GradeDesc> originalGrades;

        private void InitializeComponent() {
            this.Text = TAB_NAME;
            this.Size = new Size(1200, 800);

            this.SuspendLayout();

            FormLayout layout = new FormLayout(this, maxLabelWidth: 90);

            dateFrom = layout.Add("от", new DateTimePicker());
            dateFrom.Format = DateTimePickerFormat.Long;
            dateFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            dateTo = layout.Add("до", new DateTimePicker());
            dateTo.Format = DateTimePickerFormat.Long;
            dateTo.Value = DateTime.Now.AddDays(1);

            layout.PerformLayout();

            personSelector = new PersonSelector(dataAccess);
            personSelector.Location = new Point(3, layout.GetY() + 5);
            this.Controls.Add(personSelector);

            Label tagsLabel = new Label { Text = "Тэги" };
            tagsLabel.Location = new Point(3, layout.GetY() + 5 + personSelector.Height + 10 + 2);
            tagsLabel.Size = new Size(90, 20);
            this.Controls.Add(tagsLabel);

            tags = new TextBox();
            tags.Location = new Point(95, layout.GetY() + 5 + personSelector.Height + 10);
            tags.Size = new Size(152, 20);
            this.Controls.Add(tags);

            showGrades = new Button { Text = "Показать оценки" };
            showGrades.Location = new Point(3, layout.GetY() + 5 + personSelector.Height + 10 + 30);
            showGrades.Size = new Size(245, 25);
            showGrades.Click += new EventHandler(showGrades_Click);
            this.Controls.Add(showGrades);

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(260, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            saveChanges = new Button { Text = "Сохранить изменения" };
            saveChanges.Location = new Point(270, 770);
            saveChanges.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            saveChanges.Size = new Size(150, 25);
            saveChanges.Enabled = false;
            saveChanges.Click += new EventHandler(delegate {
                throw new NotImplementedException();
            });
            this.Controls.Add(saveChanges);

            cancelChanges = new Button { Text = "Отменить" };
            cancelChanges.Location = new Point(430, 770);
            cancelChanges.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cancelChanges.Size = new Size(150, 25);
            cancelChanges.Enabled = false;
            cancelChanges.Click += new EventHandler(delegate {
                throw new NotImplementedException();
            });
            this.Controls.Add(cancelChanges);

            gradeView = new DataGridView();
            gradeView.Location = new Point(270, 3);
            gradeView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            gradeView.Size = new Size(925, 760);
            gradeView.AllowUserToAddRows = false;
            gradeView.AllowUserToDeleteRows = false;
            gradeView.AllowUserToResizeColumns = true;
            gradeView.AllowUserToOrderColumns = false;
            gradeView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.Controls.Add(gradeView);

            gradeView.CellBeginEdit += new DataGridViewCellCancelEventHandler(delegate(object sender, DataGridViewCellCancelEventArgs e) {
                if (e.ColumnIndex < 4) {
                    e.Cancel = true;
                }
            });

            gradeView.CellValueChanged += new DataGridViewCellEventHandler(delegate(object sender, DataGridViewCellEventArgs e) {
                // indicate that cell value has been changed
                changesPending = true;
                gradeView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 153);
            });

            gradeView.ColumnAdded += new DataGridViewColumnEventHandler(delegate(object e, DataGridViewColumnEventArgs args) {
                args.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (args.Column.Index == 0) {
                    args.Column.Width = 20;
                } else if (args.Column.Index == 1) {
                    args.Column.Width = 45;
                } else if (args.Column.Index == 2) {
                    args.Column.Width = 100;
                } else if (args.Column.Index == 3) {
                    args.Column.Width = 150;
                } else {
                    args.Column.Width = 40;
                    args.Column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            });

            FluidGradeEntering.EnableFluidGradeEntering(gradeView, gradeViewDataTable, 4);
            GridPasteSupport.AddPasteSupport(gradeView, gradeViewDataTable);

            this.ResumeLayout(false);
        }

        void gradeView_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            throw new NotImplementedException();
        }

        private void showGrades_Click(object sender, EventArgs e) {
            DataContext dc = dataAccess.GetDataContext();
            List<string> selectedTags = RegisterEditor.SplitTags(tags.Text);
            IQueryable<GradeDesc> gradeQuery =
                from g in dc.GetTable<Оценка>()

                from v in personSelector.GetPersonQuery()
                where g.КодПроверяемого == v.Код

                from rank in dc.GetTable<Звание>()
                where v.КодЗвания == rank.Код

                from r in dc.GetTable<Ведомость>()
                where g.КодВедомости == r.Код

                where r.Включена == 1
                where r.ДатаЗаполнения >= dateFrom.Value.Date && r.ДатаЗаполнения <= dateTo.Value.Date

                where selectedTags.Count == 0 ||
                    (from t in dc.GetTable<ВедомостьТег>()
                     where t.КодВедомости == r.Код
                     where selectedTags.Contains(t.Тег)
                     select t).SingleOrDefault() != default(ВедомостьТег)
                
                orderby rank.order, v.Фамилия, v.Имя, v.Отчество, r.ДатаЗаполнения
                select new GradeDesc { grade = g, soldier = v, virt = r.Виртуальная };

            originalGrades = new Dictionary<Tuple<int, int>, GradeDesc>();

            List<GradeDesc> grades = gradeQuery.ToList();

            foreach (GradeDesc gd in grades) {
                originalGrades.AddOrReplace(new Tuple<int, int>(gd.soldier.Код, gd.grade.КодПредмета), gd);
            }

            gradeViewDataSet = new DataSet("gradeView");
            gradeViewDataTable = new DataTable("gradeView");
            gradeViewDataSet.Tables.Add(gradeViewDataTable);

            gradeViewDataTable.Columns.Add(new DataColumn("N"));
            gradeViewDataTable.Columns.Add(new DataColumn("ID"));
            gradeViewDataTable.Columns.Add(new DataColumn("Звание"));
            gradeViewDataTable.Columns.Add(new DataColumn("Фамилия И.О."));

            List<int> subjectIds = 
                grades.Select(gd => gd.grade.КодПредмета)
                .Distinct()
                .OrderBy(s => subjectIdToName[s])
                .ToList();
            foreach (int subjectId in subjectIds) {
                gradeViewDataTable.Columns.Add(new DataColumn(subjectIdToName[subjectId]));
            }

            List<ВоеннослужащийПоПодразделениям> soldiers = grades.Select(gd => gd.soldier).Distinct().ToList();
            int c = 1;
            foreach (var soldier in soldiers) {
                List<string> cells = new List<string>();
                cells.Add((c++).ToString());
                cells.Add(soldier.Код.ToString());
                cells.Add(soldier.Звание);
                cells.Add(soldier.ФИО);

                foreach (int subjectId in subjectIds) {
                    Option<GradeDesc> gradeOpt = originalGrades.GetOption(new Tuple<int, int>(soldier.Код, subjectId));
                    if (gradeOpt.IsEmpty()) {
                        cells.Add("");
                    }
                    gradeOpt.ForEach(gd => {
                        if (gd.grade.ЭтоКомментарий) {
                            cells.Add(gd.grade.Текст);
                        } else {
                            cells.Add(gd.grade.Значение.ToString());
                        }
                    });
                }
                gradeViewDataTable.Rows.Add(cells.ToArray());
            }

            gradeView.Columns.Clear();

            gradeView.DataSource = gradeViewDataSet;
            gradeView.DataMember = "gradeView";
            gradeView.Refresh();

            // color grades coming from virtual registers
            foreach (var soldier in soldiers) {
                foreach (int subjectId in subjectIds) {
                    if (originalGrades.GetOption(new Tuple<int, int>(soldier.Код, subjectId)).Map(gd => gd.virt == 1).GetOrElse(false)) {
                        gradeView.Rows[soldiers.IndexOf(soldier)].Cells[subjectIds.IndexOf(subjectId) + 4].Style.BackColor = Color.FromArgb(171, 191, 255);
                    }
                }
            }
        }

        private class GradeDesc {
            public Оценка grade { get; set; }
            public int virt { get; set; }
            public ВоеннослужащийПоПодразделениям soldier { get; set; }
        }

        public bool CheckForUnsavedChanges() {
            if (changesPending) {
                DialogResult result = MessageBox.Show("Сохранить изменения в оценках?", "Несохраненные изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) {
                    return false;
                } else if (result == DialogResult.OK) {
                    // save changes
                    throw new NotImplementedException();
                    changesPending = false;
                    return true;
                } else if (result == DialogResult.No) {
                    return true;
                } else {
                    throw new Exception("Unexpected dialog result: " + result);
                }
            } else {
                return true;
            }
        }
    }
}
