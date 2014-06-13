﻿using System;
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

namespace Grader.gui {
    public class RegisterEditor : Panel {
        DataAccess dataAccess;
        Dictionary<string, int> subjectNameToId;
        Dictionary<int, string> subjectIdToName;

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

        private bool afterGradeType = false;

        private void InitializeComponent() {
            this.Size = new Size(600, 600);
            FormLayout layout = new FormLayout(this);

            EventHandler changeHandler = new EventHandler(delegate {
                RegisterEdited(this, null);
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

            registerDataGridView.PreviewKeyDown +=new PreviewKeyDownEventHandler(delegate (object sender, PreviewKeyDownEventArgs args) {
                Dictionary<Keys, string> quickKeys = new Dictionary<Keys, string> {
                    { Keys.D2, "2" }, { Keys.NumPad2, "2" },
                    { Keys.D3, "3" }, { Keys.NumPad3, "3" },
                    { Keys.D4, "4" }, { Keys.NumPad4, "4" },
                    { Keys.D5, "5" }, { Keys.NumPad5, "5" }
                };
                if (quickKeys.ContainsKey(args.KeyCode)) {
                    int minX = Int32.MaxValue;
                    int minY = Int32.MaxValue;
                    foreach (DataGridViewCell sc in registerDataGridView.SelectedCells) {
                        minX = Math.Min(minX, sc.ColumnIndex);
                        minY = Math.Min(minY, sc.RowIndex);
                    }
                    if (minX > 2) {
                        if (minY + 1 < registerDataGridView.Rows.Count) {
                            registerDataGridView.Rows[minY].Cells[minX].Value = quickKeys[args.KeyCode];
                            registerDataGridView.ClearSelection();
                            registerDataGridView.Rows[minY + 1].Cells[minX].Selected = true;
                            registerDataGridView.CurrentCell = registerDataGridView.Rows[minY + 1].Cells[minX];
                        } else {
                            DataRow row = registerDataTable.Rows.Add(new object[] { });
                            row.SetField(registerDataTable.Columns[minX], quickKeys[args.KeyCode]);
                            registerDataGridView.Rows.RemoveAt(registerDataGridView.Rows.Count - 2);
                            registerDataGridView.ClearSelection();
                            registerDataGridView.CurrentCell = registerDataGridView.Rows[registerDataGridView.Rows.Count - 1].Cells[minX];
                            registerDataGridView.CurrentCell.Selected = true;
                        }
                        afterGradeType = true;
                    }
                }
            });
            registerDataGridView.CellBeginEdit += new DataGridViewCellCancelEventHandler(delegate (object sender, DataGridViewCellCancelEventArgs e) {
                if (afterGradeType) {
                    e.Cancel = true;
                    afterGradeType = false;
                }
            });
            
            registerDataGridView.KeyDown += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.V && e.Control) {
                    int minX = Int32.MaxValue;
                    int minY = Int32.MaxValue;
                    foreach (DataGridViewCell sc in registerDataGridView.SelectedCells) {
                        minX = Math.Min(minX, sc.ColumnIndex);
                        minY = Math.Min(minY, sc.RowIndex);
                    }

                    List<List<string>> copiedData = 
                        Clipboard.GetText(TextDataFormat.UnicodeText)
                        .Split(new char[] { '\n' })
                        .Where(line => line.Trim().Length > 0)
                        .Select(line => line.Split(new char[] { '\t' }).ToList()).ToList();

                    int clipX = copiedData.Select(line => line.Count).Max();
                    int clipY = copiedData.Count;

                    for (int row = 0; row < clipY; row++) {
                        if (row >= registerDataGridView.Rows.Count - 1) {
                            registerDataTable.Rows.Add(new object[] {});
                        }
                        for (int col = 0; col < clipX; col++) {
                            if (minX + col < registerDataGridView.Columns.Count) {
                                registerDataGridView.Rows[minY + row].Cells[minX + col].Value = copiedData[row][col];
                            }
                        }
                    }

                }
            });

            registerDataGridView.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(delegate(object sender, DataGridViewCellMouseEventArgs e) {
                if (e.Button == MouseButtons.Right && e.ColumnIndex > 1) {
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
                    if (e.ColumnIndex > 2) {
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
                    if (registerDataGridView.CurrentCell.ColumnIndex == 1) {
                        ed.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        ed.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        ed.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        ed.AutoCompleteCustomSource.AddRange(autocompleteRanks.ToArray());
                    } else if (registerDataGridView.CurrentCell.ColumnIndex == 2) {
                        ed.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        ed.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        ed.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        ed.AutoCompleteCustomSource.AddRange(autocompleteNames.ToArray());
                    } else {
                        ed.AutoCompleteCustomSource = null;
                    }
            });

            registerDataGridView.CellValueChanged += new DataGridViewCellEventHandler(delegate(object sender, DataGridViewCellEventArgs e) {
                if (e.ColumnIndex == 2) {
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
                            registerDataGridView.Rows[e.RowIndex].Cells[0].Value = soldierList[0].id.ToString();
                            registerDataGridView.Rows[e.RowIndex].Cells[1].Value = soldierList[0].rank;
                        }
                    }
                }
                RegisterEdited(this, null);
            });

            registerDataGridView.ColumnAdded += new DataGridViewColumnEventHandler(delegate(object e, DataGridViewColumnEventArgs args) {
                args.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (args.Column.Index == 0) {
                    args.Column.Width = 45;
                } else if (args.Column.Index == 1) {
                    args.Column.Width = 100;
                } else if (args.Column.Index == 2) {
                    args.Column.Width = 150;
                } else {
                    args.Column.Width = 40;
                    args.Column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            });

            this.Controls.Add(registerDataGridView);
        }

        public event EventHandler RegisterEdited;

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

            registerDataTable.Columns.Add(new DataColumn("ID"));
            registerDataTable.Columns.Add(new DataColumn("Звание"));
            registerDataTable.Columns.Add(new DataColumn("Фамилия И.О."));

            foreach (int subjectId in register.subjectIds) {
                registerDataTable.Columns.Add(new DataColumn(subjectIdToName[subjectId]));
            }

            foreach (RegisterRecord record in register.records) {
                List<string> cells = new List<string>();
                cells.Add(record.soldier.Код.ToString());
                cells.Add(record.soldier.Звание);
                cells.Add(record.soldier.ФИО);

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

            registerDataGridView.DataSource = registerDataSet;
            registerDataGridView.DataMember = "register";
            registerDataGridView.Refresh();
        }

        public Register GetRegister() {
            List<int> subjectIds = 
                registerDataGridView.Columns
                .ToNormalList<DataGridViewColumn>().Skip(3)
                .Select(col => subjectNameToId[col.Name]).ToList();
            return new Register {
                id = currentRegister.id,
                name = registerName.Text,
                fillDate = registerFillDate.Value,
                importDate = currentRegister.importDate.ToOption().GetOrElse(DateTime.Now),
                editDate = DateTime.Now,
                tags = registerTags.Text.Split(new char[] { ' ' }).Where(t => t.Trim().Length > 0).ToList(),
                virt = registerVirtual.Checked,
                enabled = registerEnabled.Checked,
                subjectIds = subjectIds,
                records = 
                    registerDataGridView.Rows.ToNormalList<DataGridViewRow>()
                    .Where(row => {
                        if (row.Cells[0].Value != null) {
                            return row.Cells[0].Value.ToString().Trim().Length > 0;
                        } else {
                            return false;
                        }
                    })
                    .Select(row => {
                        ВоеннослужащийПоПодразделениям soldier = dataAccess.GetDataContext().GetTable<ВоеннослужащийПоПодразделениям>()
                                .Where(v => v.Код == Int32.Parse(row.Cells[0].Value.ToString()))
                                .ToListTimed().First();
                        List<Оценка> marks = new List<Оценка>();
                        for (int col = 3; col < registerDataGridView.Columns.Count; col++) {
                            Оценка g = new Оценка {
                                Код = -1,
                                КодПроверяемого = soldier.Код,
                                КодПредмета = subjectIds[col - 3],
                                КодПодразделения = soldier.КодПодразделения,
                                ВУС = soldier.ВУС,
                                ТипВоеннослужащего = soldier.ТипВоеннослужащего,
                                КодЗвания = soldier.КодЗвания,
                                КодВедомости = -1
                            };
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
                        return new RegisterRecord {
                            soldier = soldier,
                            marks = marks
                        };
                    }).ToList()
            };
        }
    }
}
