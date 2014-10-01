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
using Grader.model;
using Grader.grades;

namespace Grader.gui {
    public class GradeViewTab : TabPage {
        private Entities et;
        private Settings settings;
        public EventManager ChangesSaved = new EventManager();

        private static string TAB_NAME = "Просмотр оценок";

        public GradeViewTab(Entities et, Settings settings) {
            this.et = et;
            this.settings = settings;
            this.InitializeComponent();
            this.RefreshAutocomplete();
        }

        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private PersonSelector personSelector;
        private TextBox tags;
        private TextBox subjectList;
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
        private List<int> soldierIds;
        private List<int> subjectIds;

        private void InitializeComponent() {
            this.Text = TAB_NAME;
            this.Size = new Size(1200, 800);

            this.SuspendLayout();

            FormLayout layout = new FormLayout(this, maxLabelWidth: 90);

            dateFrom = layout.Add("от", new DateTimePicker());
            dateFrom.Format = DateTimePickerFormat.Long;
            dateFrom.ShowCheckBox = true;
            dateFrom.Checked = false;

            dateTo = layout.Add("до", new DateTimePicker());
            dateTo.Format = DateTimePickerFormat.Long;
            dateTo.ShowCheckBox = true;
            dateTo.Checked = false;

            layout.PerformLayout();

            personSelector = new PersonSelector(et);
            personSelector.Location = new Point(3, layout.GetY() + 5);
            this.Controls.Add(personSelector);

            Label tagsLabel = new Label { Text = "Тэги" };
            tagsLabel.Location = new Point(3, layout.GetY() + 5 + personSelector.Height + 10 + 2);
            tagsLabel.Size = new Size(90, 20);
            this.Controls.Add(tagsLabel);

            tags = new TextBox();
            tags.Location = new Point(95, layout.GetY() + 5 + personSelector.Height + 10);
            tags.Size = new Size(152, 20);
            tags.Text = settings.gradeViewTags.GetValue();
            this.Controls.Add(tags);

            Label subjectListLabel = new Label { Text = "Предметы" };
            subjectListLabel.Location = new Point(3, layout.GetY() + 5 + personSelector.Height + 5 + tags.Height + 10 + 2);
            subjectListLabel.Size = new Size(90, 20);
            this.Controls.Add(subjectListLabel);

            subjectList = new TextBox();
            subjectList.Location = new Point(95, layout.GetY() + 5 + personSelector.Height + 5 + tags.Height + 10);
            subjectList.Size = new Size(152, 20);
            subjectList.Text = settings.gradeViewSubjects.GetValue();
            this.Controls.Add(subjectList);
            
            showGrades = new Button { Text = "Показать оценки" };
            showGrades.Location = new Point(3, layout.GetY() + 5 + personSelector.Height + 10 + 60);
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
                SaveChanges();
            });
            this.Controls.Add(saveChanges);

            cancelChanges = new Button { Text = "Отменить" };
            cancelChanges.Location = new Point(430, 770);
            cancelChanges.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cancelChanges.Size = new Size(150, 25);
            cancelChanges.Enabled = false;
            cancelChanges.Click += new EventHandler(delegate {
                changesPending = false;
                gradeView.DataSource = null;
                cancelChanges.Enabled = false;
                saveChanges.Enabled = false;
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
            gradeView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.Controls.Add(gradeView);

            gradeView.CellBeginEdit += new DataGridViewCellCancelEventHandler(delegate(object sender, DataGridViewCellCancelEventArgs e) {
                if (e.ColumnIndex < 4 || e.ColumnIndex == gradeView.ColumnCount - 1) {
                    e.Cancel = true;
                }
            });

            gradeView.CellValueChanged += new DataGridViewCellEventHandler(delegate(object sender, DataGridViewCellEventArgs e) {
                if (e.ColumnIndex >= 4 && e.ColumnIndex != gradeView.ColumnCount - 1) {
                    // indicate that cell value has been changed
                    changesPending = true;
                    gradeView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 153);
                    UpdateSummaryGradeInRow(e.RowIndex);
                }
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

                if (args.Column.Name == "ОБЩ") {
                    args.Column.DefaultCellStyle.BackColor = Color.LightYellow;
                }
            });

            FluidGradeEntering.EnableFluidGradeEntering(gradeView, col => col >= 4 && col != gradeView.ColumnCount - 1);
            GridPasteSupport.AddPasteSupport(gradeView);
            GridDeleteKeySupport.AddDeleteKeySupport(gradeView, isEditingAllowed: col => col >= 4 && col != gradeView.ColumnCount - 1);

            this.ResumeLayout(false);
        }

        private void showGrades_Click(object sender, EventArgs e) {
            List<string> selectedTags = RegisterEditor.SplitTags(tags.Text);

            DateTime dtFrom = dateFrom.Value.Date;
            DateTime dtTo = dateTo.Value.Date.AddDays(1);

            List<SoldierDesc> soldiers = personSelector.GetPersonList().ConvertAll(v => 
                new SoldierDesc { 
                    rank = et.rankIdToName[v.КодЗвания], 
                    soldierId = v.Код, 
                    ФИО = 
                        v.Фамилия + " " + 
                        (v.Имя.Length > 0 ? v.Имя.Substring(0,1) : " ") + "." + 
                        (v.Отчество.Length > 0 ? v.Отчество.Substring(0, 1) : " ") + "."
                });
            soldierIds = soldiers.Select(v => v.soldierId).ToList();
            Dictionary<int, SoldierDesc> idToSoldierDesc = soldiers.ToDictionary(sd => sd.soldierId, sd => sd);

            IQueryable<int> registerIdQuery =
                (from t in et.ВедомостьТег
                 join r in et.Ведомость on t.КодВедомости equals r.Код
                 where r.Включена
                 where !dateFrom.Checked || r.ДатаЗаполнения >= dtFrom
                 where !dateTo.Checked || r.ДатаЗаполнения <= dtTo
                 where selectedTags.Contains(t.Тег)
                 select r.Код).Distinct();
            List<int> registerIds = registerIdQuery.ToList();

            IQueryable<GradeDesc> gradeQuery =
                from g in et.Оценка
                join r in et.Ведомость on g.КодВедомости equals r.Код
                where registerIds.Contains(r.Код)
                where soldierIds.Contains(g.КодПроверяемого)
                orderby r.ДатаЗаполнения
                select new GradeDesc {
                    grade = g,
                    registerId = r.Код,
                    virt = r.Виртуальная
                };

            originalGrades = new Dictionary<Tuple<int, int>, GradeDesc>();

            List<GradeDesc> grades = gradeQuery.ToList();

            List<int> selectedSubjectsIds = subjectList.Text.Split(new char[] { ' ', ',', ';' })
                .Where(s => s.Trim().Length > 0)
                .Select(s => et.subjectNameToId[s.ToUpper()])
                .ToList();

            foreach (var gd in grades) {
                gd.soldier = idToSoldierDesc[gd.grade.КодПроверяемого];
            }

            foreach (GradeDesc gd in grades) {
                if (gd.grade.ЭтоКомментарий && gd.grade.Текст == "_") {
                    // found marker for grade deletion
                    originalGrades.Remove(new Tuple<int, int>(gd.grade.КодПроверяемого, gd.grade.КодПредмета));
                } else {
                    originalGrades.AddOrReplace(new Tuple<int, int>(gd.grade.КодПроверяемого, gd.grade.КодПредмета), gd);
                }
            }

            gradeViewDataSet = new DataSet("gradeView");
            gradeViewDataTable = new DataTable("gradeView");
            gradeViewDataSet.Tables.Add(gradeViewDataTable);

            gradeViewDataTable.Columns.Add(new DataColumn("N"));
            gradeViewDataTable.Columns.Add(new DataColumn("ID"));
            gradeViewDataTable.Columns.Add(new DataColumn("Звание"));
            gradeViewDataTable.Columns.Add(new DataColumn("Фамилия И.О."));

            subjectIds = 
                selectedSubjectsIds.Count != 0 ? 
                    selectedSubjectsIds :
                    grades.Select(gd => gd.grade.КодПредмета)
                    .Distinct()
                    .OrderBy(s => et.subjectIdToName[s])
                    .ToList();
            foreach (int subjectId in subjectIds) {
                gradeViewDataTable.Columns.Add(new DataColumn(et.subjectIdToName[subjectId]));
            }

            gradeViewDataTable.Columns.Add(new DataColumn("ОБЩ"));

            int c = 1;
            foreach (var soldierId in soldierIds) {
                List<string> cells = new List<string>();
                cells.Add((c++).ToString());
                cells.Add(soldierId.ToString());
                cells.Add(idToSoldierDesc[soldierId].rank);
                cells.Add(idToSoldierDesc[soldierId].ФИО);

                foreach (int subjectId in subjectIds) {
                    Option<GradeDesc> gradeOpt = originalGrades.GetOption(new Tuple<int, int>(soldierId, subjectId));
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
            foreach (var soldierId in soldierIds) {
                foreach (int subjectId in subjectIds) {
                    if (originalGrades.GetOption(new Tuple<int, int>(soldierId, subjectId)).Map(gd => gd.virt).GetOrElse(false)) {
                        gradeView.Rows[soldierIds.IndexOf(soldierId)].Cells[subjectIds.IndexOf(subjectId) + 4].Style.BackColor = Color.FromArgb(171, 191, 255);
                    }
                }
            }

            for (int r = 0; r < gradeView.RowCount; r++) {
                UpdateSummaryGradeInRow(r);
            }
            changesPending = false;

            saveChanges.Enabled = true;
            cancelChanges.Enabled = true;
            settings.gradeViewTags.SetValue(tags.Text);
            settings.gradeViewSubjects.SetValue(subjectList.Text);
            settings.Save();
        }

        private void UpdateSummaryGradeInRow(int rowIndex) {
            Dictionary<string, int> grades = new Dictionary<string, int>();
            foreach (int subjectId in subjectIds) {
                string v = gradeView.Rows[rowIndex].Cells[4 + subjectIds.IndexOf(subjectId)].Value.ToString().Trim();
                Util.ParseInt(v).ForEach(g => {
                    grades.Add(et.subjectIdToName[subjectId], g);
                });
            }

            if (grades.Count == 0) {
                gradeView.Rows[rowIndex].Cells[gradeView.ColumnCount - 1].Value = "";
            } else {
                IEnumerable<GradeDesc> soldierGrades = originalGrades.Where(kv => kv.Key.Item1 == soldierIds[rowIndex]).Select(kv => kv.Value);
                if (soldierGrades.Count() > 0) {
                    GradeDesc someGradeDesc = soldierGrades.First();
                    GradeSet gs = new GradeSet() { grades = grades, subunit = et.subunitIdToInstance[someGradeDesc.grade.КодПодразделения] };

                    Option<int> summaryGrade =
                        GradeCalcIndividual.ОценкаОБЩ(
                            gs,
                            et.subunitIdToInstance[someGradeDesc.grade.КодПодразделения].ТипОбучения,
                            someGradeDesc.grade.ТипВоеннослужащего
                        );
                    summaryGrade.ForEach(sg => {
                        gradeView.Rows[rowIndex].Cells[gradeView.ColumnCount - 1].Value = sg.ToString();
                    });
                    if (summaryGrade.IsEmpty()) {
                        gradeView.Rows[rowIndex].Cells[gradeView.ColumnCount - 1].Value = "";
                    }
                } else {
                    int soldierId = soldierIds[rowIndex];
                    Военнослужащий soldier = et.Военнослужащий.Where(s => s.Код == soldierId).First();

                    GradeSet gs = new GradeSet() { grades = grades, subunit = et.subunitIdToInstance[soldier.КодПодразделения] };

                    Option<int> summaryGrade =
                        GradeCalcIndividual.ОценкаОБЩ(
                            gs,
                            et.subunitIdToInstance[soldier.КодПодразделения].ТипОбучения,
                            soldier.ТипВоеннослужащего
                        );
                    summaryGrade.ForEach(sg => {
                        gradeView.Rows[rowIndex].Cells[gradeView.ColumnCount - 1].Value = sg.ToString();
                    });
                    if (summaryGrade.IsEmpty()) {
                        gradeView.Rows[rowIndex].Cells[gradeView.ColumnCount - 1].Value = "";
                    }
                }
            }
        }

        private class SoldierDesc {
            public int soldierId { get; set; }
            public string rank { get; set; }
            public string ФИО { get; set; }
        }

        private class GradeDesc {
            public Оценка grade { get; set; }
            public bool virt { get; set; }
            public SoldierDesc soldier { get; set; }
            public int registerId { get; set; }
        }

        public bool CheckForUnsavedChanges() {
            if (changesPending) {
                DialogResult result = MessageBox.Show("Сохранить изменения в оценках?", "Несохраненные изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) {
                    return false;
                } else if (result == DialogResult.OK) {
                    SaveChanges();
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

        public void SaveChanges() {
            Dictionary<int, List<Оценка>> editedGrades = 
                new Dictionary<int, List<Оценка>>();

            foreach (var soldierId in soldierIds) {
                foreach (int subjectId in subjectIds) {
                    string v = gradeView.Rows[soldierIds.IndexOf(soldierId)].Cells[subjectIds.IndexOf(subjectId) + 4].Value.ToString().Trim();
                    Option<GradeDesc> gdOpt = originalGrades.GetOption(new Tuple<int, int>(soldierId, subjectId));
                    if (v == "" && gdOpt.IsEmpty()) {
                        // no change
                    } else if (gdOpt.IsEmpty()) {
                        // new grade was added
                        List<Оценка> grades = editedGrades.GetOrElseInsertAndGet(soldierId, () => new List<Оценка>());
                        IEnumerable<GradeDesc> originalSoldierGrades = originalGrades.Where(kv => kv.Key.Item1 == soldierId).Select(kv => kv.Value);
                        if (originalSoldierGrades.Count() > 0) {
                            GradeDesc someGradeDesc = originalSoldierGrades.First();
                            Оценка g = new Оценка {
                                Код = -1,
                                КодВедомости = -1,
                                КодПроверяемого = soldierId,
                                ВУС = someGradeDesc.grade.ВУС,
                                КодЗвания = someGradeDesc.grade.КодЗвания,
                                КодПодразделения = someGradeDesc.grade.КодПодразделения,
                                КодПредмета = subjectId,
                                ТипВоеннослужащего = someGradeDesc.grade.ТипВоеннослужащего
                            };
                            Util.ParseInt(v).Map(vv => {
                                g.ЭтоКомментарий = false;
                                g.Значение = (sbyte) vv;
                                g.Текст = "";
                                return true;
                            }).GetOrElse(() => {
                                g.ЭтоКомментарий = true;
                                g.Текст = v;
                                g.Значение = 0;
                                return true;
                            });
                            grades.Add(g);
                        } else {
                            Военнослужащий soldier = et.Военнослужащий.Where(s => s.Код == soldierId).First();
                            Оценка g = new Оценка {
                                Код = -1,
                                КодВедомости = -1,
                                КодПроверяемого = soldierId,
                                ВУС = soldier.ВУС,
                                КодЗвания = soldier.КодЗвания,
                                КодПодразделения = soldier.КодПодразделения,
                                КодПредмета = subjectId,
                                ТипВоеннослужащего = soldier.ТипВоеннослужащего
                            };
                            Util.ParseInt(v).Map(vv => {
                                g.ЭтоКомментарий = false;
                                g.Значение = (sbyte) vv;
                                g.Текст = "";
                                return true;
                            }).GetOrElse(() => {
                                g.ЭтоКомментарий = true;
                                g.Текст = v;
                                g.Значение = 0;
                                return true;
                            });
                            grades.Add(g);
                        }
                    } else {
                        GradeDesc gd = gdOpt.Get();
                        if (v == "") {
                            // grade was deleted
                            List<Оценка> grades = editedGrades.GetOrElseInsertAndGet(soldierId, () => new List<Оценка>());
                            Оценка g = new Оценка {
                                Код = -1,
                                КодВедомости = -1,
                                КодПроверяемого = soldierId,
                                ВУС = gd.grade.ВУС,
                                КодЗвания = gd.grade.КодЗвания,
                                КодПодразделения = gd.grade.КодПодразделения,
                                КодПредмета = subjectId,
                                ТипВоеннослужащего = gd.grade.ТипВоеннослужащего,
                                ЭтоКомментарий = true,
                                Значение = 0,
                                Текст = "_"
                            };
                            grades.Add(g);
                        } else if ((gd.grade.ЭтоКомментарий && gd.grade.Текст == v) || gd.grade.Значение.ToString() == v) {
                            // no change happened
                        } else {
                            // grade was changed
                            List<Оценка> grades = editedGrades.GetOrElseInsertAndGet(soldierId, () => new List<Оценка>());
                            Оценка g = new Оценка {
                                Код = -1,
                                КодВедомости = -1,
                                КодПроверяемого = soldierId,
                                ВУС = gd.grade.ВУС,
                                КодЗвания = gd.grade.КодЗвания,
                                КодПодразделения = gd.grade.КодПодразделения,
                                КодПредмета = subjectId,
                                ТипВоеннослужащего = gd.grade.ТипВоеннослужащего
                            };
                            Util.ParseInt(v).Map(vv => {
                                g.ЭтоКомментарий = false;
                                g.Значение = (sbyte) vv;
                                g.Текст = "";
                                return true;
                            }).GetOrElse(() => {
                                g.ЭтоКомментарий = true;
                                g.Текст = v;
                                g.Значение = 0;
                                return true;
                            });
                            grades.Add(g);
                        }
                    }
                }
            }

            if (editedGrades.Count > 0) {

                Register newRegister = new Register {
                    id = -1,
                    fillDate = DateTime.Now,
                    importDate = DateTime.Now,
                    editDate = DateTime.Now,
                    enabled = true,
                    virt = true,
                    name = "",
                    tags = RegisterEditor.SplitTags(tags.Text),
                    subjectIds = 
                        editedGrades.Values
                        .SelectMany(grades => grades.Select(g => g.КодПредмета))
                        .Distinct()
                        .OrderBy(sid => subjectIds.IndexOf(sid))
                        .ToList(),
                    records =
                        editedGrades.ToList()
                        .Select(kv => new RegisterRecord { soldierId = kv.Key, marks = kv.Value })
                        .ToList()
                };

                Form f = new Form();
                f.Text = "Сохранение изменений";
                f.Size = new Size(800, 900);

                RegisterEditor changesEditor = new RegisterEditor(et);
                changesEditor.Location = new Point(5, 5);
                changesEditor.Size = new Size(770, 820);
                changesEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                changesEditor.SetRegister(newRegister);
                f.Controls.Add(changesEditor);

                Button okButton = new Button { Text = "Сохранить" };
                okButton.Location = new Point(5, 830);
                okButton.Size = new Size(100, 25);
                okButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                okButton.Click += new EventHandler(delegate {
                    RegisterMarshaller.SaveRegister(changesEditor.GetRegister(), et);
                    ChangesSaved.Invoke();
                    f.DialogResult = DialogResult.OK;
                    f.Hide();
                });
                f.Controls.Add(okButton);

                Button cancelButton = new Button { Text = "Отменить" };
                cancelButton.Location = new Point(115, 830);
                cancelButton.Size = new Size(100, 25);
                cancelButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                cancelButton.Click += new EventHandler(delegate {
                    f.DialogResult = DialogResult.Cancel;
                    f.Hide();
                });
                f.Controls.Add(cancelButton);

                if (f.ShowDialog() == DialogResult.OK) {
                    changesPending = false;
                    showGrades_Click(null, null);
                }
            }
        }

        public void RefreshAutocomplete() {
            tags.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tags.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tags.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            tags.AutoCompleteCustomSource.AddRange(et.ВедомостьТег.Select(t => t.Тег).Distinct().ToArray());

        }
    }
}
