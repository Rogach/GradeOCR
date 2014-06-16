using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grader.model;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using LibUtil;
using System.Threading;
using Grader.util;
using System.Text.RegularExpressions;
using System.Data.Linq;
using Grader.gui.gridutil;

namespace Grader.gui {
    public class RegisterEditor : Panel {
        DataAccess dataAccess;
        Dictionary<string, int> subjectNameToId;
        Dictionary<int, string> subjectIdToName;
        Dictionary<int, string> rankIdToName;
        public EventManager RegisterEdited = new EventManager();

        public RegisterEditor(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
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
            rankIdToName =
                dataAccess.GetDataContext().GetTable<Звание>()
                .Select(r => new { id = r.Код, rank = r.Название })
                .ToListTimed()
                .ToDictionary(r => r.id, r => r.rank);
        }

        private Register currentRegister;

        private TextBox registerName;
        private DateTimePicker registerFillDate;
        private Label registerImportDate;
        private Label registerEditDate;
        private TextBox registerTags;
        private CheckBox registerVirtual;
        private CheckBox registerEnabled;

        private DataSet registerDataSet;
        private DataTable registerDataTable;
        private DataGridView registerDataGridView;

        private void InitializeComponent() {
            this.Size = new Size(600, 600);
            FormLayout layout = new FormLayout(this);

            EventHandler changeHandler = new EventHandler(delegate {
                RegisterEdited.Invoke();
            });
            
            registerName = layout.Add("Имя ведомости", new TextBox());
            registerName.TextChanged += changeHandler;
            
            
            registerFillDate = layout.Add("Дата заполнения", new DateTimePicker());
            registerFillDate.Value = DateTime.Now;
            registerFillDate.ValueChanged += changeHandler;

            registerImportDate = layout.Add("Дата внесения", new Label());

            registerEditDate = layout.Add("Дата изменения", new Label());

            registerTags = layout.Add("Тэги", new TextBox());
            registerTags.TextChanged += changeHandler;

            layout.AddSpacer(10);

            layout.PerformLayout();

            FormLayout secondaryOptions = new FormLayout(this, x: layout.GetX() + 30);

            registerVirtual = secondaryOptions.Add("виртуальная?", new CheckBox());
            GuiUtils.SetToolTip(secondaryOptions, registerVirtual, "Физически ведомости не существует - оценки были внесены без документа");
            registerVirtual.CheckedChanged += changeHandler;

            registerEnabled = secondaryOptions.Add("включена?", new CheckBox());
            registerEnabled.Checked = true;
            GuiUtils.SetToolTip(secondaryOptions, registerEnabled, "Ведомость учитывается при анализе?");
            registerEnabled.CheckedChanged += changeHandler;

            secondaryOptions.PerformLayout();

            registerDataGridView = new DataGridView();
            registerDataGridView.Location = new Point(0, layout.GetY());
            registerDataGridView.Size = new Size(600, 600 - layout.GetY());
            registerDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            
            registerDataGridView.AllowUserToAddRows = true;
            registerDataGridView.AllowUserToDeleteRows = true;
            registerDataGridView.AllowUserToResizeColumns = true;
            registerDataGridView.AllowUserToOrderColumns = false;
            registerDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            FluidGradeEntering.EnableFluidGradeEntering(registerDataGridView, registerDataTable, 4);
            GridPasteSupport.AddPasteSupport(registerDataGridView, registerDataTable);
            GridDeleteKeySupport.AddDeleteKeySupport(registerDataGridView);

            registerDataGridView.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(delegate(object sender, DataGridViewCellMouseEventArgs e) {
                if (e.Button == MouseButtons.Right && e.ColumnIndex > 2) {
                    int x = 0;
                    x += registerDataGridView.RowHeadersWidth;
                    for (int ci = 0; ci < e.ColumnIndex; ci++) {
                        x += registerDataGridView.Columns[ci].Width;
                    }
                    x += e.X;

                    MenuItem deleteColumnAction = new MenuItem("Удалить предмет");
                    deleteColumnAction.Click += new EventHandler(delegate {
                        registerDataGridView.Columns.Remove(registerDataGridView.Columns[e.ColumnIndex]);
                    });

                    MenuItem addSubjectSubmenu = new MenuItem("Добавить предмет");
                    foreach (string subj in dataAccess.GetDataContext().GetTable<Предмет>().Select(s => s.Название).ToList().OrderBy(s => s)) {
                        string subject = subj;
                        MenuItem subjectItem = new MenuItem(subj);
                        subjectItem.Click += new EventHandler(delegate {
                            DataGridViewColumn newCol = new DataGridViewColumn();
                            newCol.Name = subject;
                            newCol.CellTemplate = new DataGridViewTextBoxCell();
                            // insert after current column
                            registerDataGridView.Columns.Insert(e.ColumnIndex + 1, newCol);
                        });
                        addSubjectSubmenu.MenuItems.Add(subjectItem);
                    }

                    MenuItem addSubjects = new MenuItem("Добавить предметы");
                    addSubjects.Click += new EventHandler(delegate {
                        InputDialog.ShowInputDialog("Список предметов:", "Выбор предметов", "").ForEach(subjects => {
                            List<string> subjectList =
                                subjects.Split(new char[] { ' ', ',', ';' })
                                .Where(s => s.Trim().Length > 0)
                                .Select(s => s.ToUpper())
                                .ToList();
                            int c = 1;
                            foreach (var subj in subjectList) {
                                DataGridViewColumn newCol = new DataGridViewColumn();
                                newCol.Name = subj;
                                newCol.CellTemplate = new DataGridViewTextBoxCell();
                                // insert after current column
                                registerDataGridView.Columns.Insert(e.ColumnIndex + (c++), newCol);
                            }
                        });
                    });

                    ContextMenu ctx;
                    if (e.ColumnIndex > 3) {
                        ctx = new ContextMenu(new MenuItem[] { deleteColumnAction, addSubjectSubmenu, addSubjects });
                    } else  {
                        ctx = new ContextMenu(new MenuItem[] { addSubjectSubmenu, addSubjects });
                    }
                    ctx.Show(registerDataGridView, new Point(x, e.Y));
                }
            });

            List<string> autocompleteRanks = 
                dataAccess.GetDataContext().GetTable<Звание>().Select(r => r.Название).ToListTimed();
            List<string> autocompleteNames =
                dataAccess.GetDataContext().GetTable<Военнослужащий>()
                .Where(v => v.Убыл == 0)
                .Select(v => v.ФИО).ToListTimed();

            registerDataGridView.EditingControlShowing += 
                new DataGridViewEditingControlShowingEventHandler(delegate(object sender, DataGridViewEditingControlShowingEventArgs args) {
                    TextBox ed = (TextBox) registerDataGridView.EditingControl;
                    if (registerDataGridView.CurrentCell.ColumnIndex == 2) {
                        ed.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        ed.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        ed.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        ed.AutoCompleteCustomSource.AddRange(autocompleteRanks.ToArray());
                    } else if (registerDataGridView.CurrentCell.ColumnIndex == 3) {
                        ed.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        ed.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        ed.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        ed.AutoCompleteCustomSource.AddRange(autocompleteNames.ToArray());
                    } else {
                        ed.AutoCompleteCustomSource = null;
                    }
            });

            registerDataGridView.CellValueChanged += new DataGridViewCellEventHandler(delegate(object sender, DataGridViewCellEventArgs e) {
                if (e.ColumnIndex == 3) {
                    Regex nameRegex = new Regex(@"(\w+) (\w).(\w).");
                    Match match = nameRegex.Match(registerDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                    if (match.Success) {
                        string surname = match.Groups[1].Value;
                        string name = match.Groups[2].Value;
                        string patronymic = match.Groups[3].Value;
                        DataContext dc = dataAccess.GetDataContext();
                        var query =
                            from v in dc.GetTable<Военнослужащий>()
                            where v.Убыл == 0
                            where v.Фамилия == surname
                            where v.Имя.StartsWith(name)
                            where v.Отчество.StartsWith(patronymic)
                            join r in dc.GetTable<Звание>() on v.КодЗвания equals r.Код
                            select new { id = v.Код, rank = r.Название };
                        var soldierList = query.ToListTimed();
                        if (soldierList.Count == 1) {
                            registerDataGridView.Rows[e.RowIndex].Cells[1].Value = soldierList[0].id.ToString();
                            registerDataGridView.Rows[e.RowIndex].Cells[2].Value = soldierList[0].rank;
                        }
                    }
                }
                RegisterEdited.Invoke();
            });

            registerDataGridView.ColumnAdded += new DataGridViewColumnEventHandler(delegate(object e, DataGridViewColumnEventArgs args) {
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

            this.Controls.Add(registerDataGridView);
        }

        public Register GetEmptyRegister() {
            return new model.Register {
                id = -1,
                name = "",
                fillDate = DateTime.Now,
                importDate = null,
                editDate = null,
                tags = new List<string>(),
                virt = false,
                enabled = true,
                subjectIds = new List<int>(),
                records = new List<RegisterRecord>()
            };
        }

        public void SetRegister(Register register) {
            currentRegister = register;
            registerName.Text = register.name;
            registerFillDate.Value = register.fillDate;
            registerImportDate.Text = register.importDate.ToOption().Map(d => d.ToLongDateString()).GetOrElse("");
            registerEditDate.Text = register.editDate.ToOption().Map(d => d.ToLongDateString()).GetOrElse("");
            registerTags.Text = register.tags.MkString(" ");
            registerVirtual.Checked = register.virt;
            registerEnabled.Checked = register.enabled;
            registerDataSet = new DataSet("register");
            registerDataTable = new DataTable("register");
            registerDataSet.Tables.Add(registerDataTable);

            registerDataTable.Columns.Add(new DataColumn("N"));
            registerDataTable.Columns.Add(new DataColumn("ID"));
            registerDataTable.Columns.Add(new DataColumn("Звание"));
            registerDataTable.Columns.Add(new DataColumn("Фамилия И.О."));

            foreach (int subjectId in register.subjectIds) {
                registerDataTable.Columns.Add(new DataColumn(subjectIdToName[subjectId]));
            }

            List<int> soldierIds = register.records.Select(rec => rec.soldierId).ToList();
            Dictionary<int, string> soldierIdToName =
                dataAccess.GetDataContext().GetTable<Военнослужащий>()
                .Where(s => soldierIds.Contains(s.Код))
                .ToListTimed()
                .Select(s => new { id = s.Код, name = s.ФИО })
                .ToDictionary(s => s.id, s => s.name);

            int c = 1;
            foreach (RegisterRecord record in register.records) {
                List<string> cells = new List<string>();
                cells.Add((c++).ToString());
                cells.Add(record.soldierId.ToString());
                if (record.marks.Count > 0) {
                    cells.Add(rankIdToName[record.marks[0].КодЗвания]);
                    cells.Add(soldierIdToName[record.marks[0].КодПроверяемого]);
                } else if (record.soldier != null) {
                    cells.Add(record.soldier.Звание);
                    cells.Add(record.soldier.ФИО);
                } else {
                    cells.Add("");
                    cells.Add("");
                }

                foreach (int subjectId in register.subjectIds) {
                    Option<Оценка> markOpt = record.marks.FindOption(g => g.КодПредмета == subjectId);
                    if (markOpt.IsEmpty()) {
                        cells.Add("");
                    }
                    markOpt.ForEach(grade => {
                        if (grade.ЭтоКомментарий) {
                            cells.Add(grade.Текст);
                        } else {
                            cells.Add(grade.Значение.ToString());
                        }
                    });
                }
                registerDataTable.Rows.Add(cells.ToArray());
            }

            registerDataGridView.Columns.Clear();

            registerDataGridView.DataSource = registerDataSet;
            registerDataGridView.DataMember = "register";
            registerDataGridView.Refresh();
        }

        public Register GetRegister() {
            List<int> subjectIds = 
                registerDataGridView.Columns
                .ToNormalList<DataGridViewColumn>().Skip(4)
                .Select(col => subjectNameToId[col.Name]).ToList();
            Dictionary<int, Подразделение> subunitCache = new Dictionary<int,Подразделение>();
            return new Register {
                id = currentRegister.id,
                name = registerName.Text,
                fillDate = registerFillDate.Value,
                importDate = currentRegister.importDate.ToOption().GetOrElse(DateTime.Now),
                editDate = DateTime.Now,
                tags = SplitTags(registerTags.Text),
                virt = registerVirtual.Checked,
                enabled = registerEnabled.Checked,
                subjectIds = subjectIds,
                records = 
                    registerDataGridView.Rows.ToNormalList<DataGridViewRow>()
                    .Where(row => {
                        if (row.Cells[1].Value != null) {
                            return row.Cells[1].Value.ToString().Trim().Length > 0;
                        } else {
                            return false;
                        }
                    })
                    .Select(row => {
                        int soldierId = Int32.Parse(row.Cells[1].Value.ToString());

                        RegisterRecord record = currentRegister.records.Find(r => r.soldierId == soldierId);

                        ВоеннослужащийПоПодразделениям maybeSoldier = record.soldier;

                        List<Оценка> previousMarks = record.marks;

                        int subunitId;
                        if (previousMarks.Count > 0) {
                            subunitId = previousMarks[0].КодПодразделения;
                        } else if (maybeSoldier != null) {
                            subunitId = maybeSoldier.КодПодразделения;
                        } else {
                            maybeSoldier = 
                                dataAccess.GetDataContext().GetTable<ВоеннослужащийПоПодразделениям>()
                                .Where(s => s.Код == soldierId).Where(s => s.КодПодразделения == s.КодСтаршегоПодразделения)
                                .ToList().First();
                            subunitId = maybeSoldier.Код;
                        }

                        List<Оценка> marks = new List<Оценка>();

                        for (int col = 4; col < registerDataGridView.Columns.Count; col++) {
                            if (row.Cells[col].Value != null && row.Cells[col].Value.ToString().Trim() != "") {
                                Оценка g;
                                if (previousMarks.Count > 0) {
                                    g = new Оценка {
                                        Код = -1,
                                        КодПроверяемого = soldierId,
                                        КодПредмета = subjectIds[col - 4],
                                        КодПодразделения = previousMarks[0].КодПодразделения,
                                        ВУС = previousMarks[0].ВУС,
                                        ТипВоеннослужащего = previousMarks[0].ТипВоеннослужащего,
                                        КодЗвания = previousMarks[0].КодЗвания,
                                        КодВедомости = -1
                                    };
                                } else {
                                    Подразделение subunit =
                                        subunitCache.GetOrElseInsertAndGet(subunitId, () => {
                                            return
                                                dataAccess.GetDataContext().GetTable<Подразделение>()
                                                .Where(s => s.Код == subunitId).ToList().First();
                                        });

                                    // maybeSoldier is surely not null now, since we initialized it above
                                    g = new Оценка {
                                        Код = -1,
                                        КодПроверяемого = soldierId,
                                        КодПредмета = subjectIds[col - 4],
                                        КодПодразделения = maybeSoldier.КодПодразделения,
                                        ВУС = maybeSoldier.ВУС,
                                        ТипВоеннослужащего = subunit.ТипОбучения == null ? "" : subunit.ТипОбучения,
                                        КодЗвания = maybeSoldier.КодЗвания,
                                        КодВедомости = -1
                                    };
                                }
                                Util.ParseInt(row.Cells[col].Value.ToString()).Map(v => {
                                    g.ЭтоКомментарий = false;
                                    g.Значение = v;
                                    g.Текст = "";
                                    return true;
                                }).GetOrElse(() => {
                                    g.ЭтоКомментарий = true;
                                    g.Текст = row.Cells[col].Value.ToString();
                                    g.Значение = 0;
                                    return true;
                                });
                                marks.Add(g);
                            }
                        }
                        return new RegisterRecord {
                            soldierId = soldierId,
                            marks = marks
                        };
                    })
                    .Where(rec => rec.marks.Count > 0) // don't save empty rows
                    .ToList()
            };
        }

        public static List<String> SplitTags(string tags) {
            return tags.Split(new char[] { ' ' }).Where(t => t.Trim().Length > 0).ToList();
        }
    }
}
