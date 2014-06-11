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

namespace Grader.gui {
    public class RegisterEditor : Panel {
        DataAccess dataAccess;

        public RegisterEditor(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
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
            
            registerName = layout.Add("Имя ведомости", new TextBox());
            
            
            registerFillDate = layout.Add("Дата заполнения", new DateTimePicker());
            registerFillDate.Value = DateTime.Now;

            registerImportDate = layout.Add("Дата внесения", new Label());

            registerEditDate = layout.Add("Дата изменения", new Label());

            registerTags = layout.Add("Тэги", new TextBox());

            layout.AddSpacer(10);

            layout.PerformLayout();

            FormLayout secondaryOptions = new FormLayout(this, x: layout.GetX() + 30);

            registerVirtual = secondaryOptions.Add("виртуальная?", new CheckBox());
            GuiUtils.SetToolTip(secondaryOptions, registerVirtual, "Физически ведомости не существует - оценки были внесены без документа");

            registerEnabled = secondaryOptions.Add("включена?", new CheckBox());
            registerEnabled.Checked = true;
            GuiUtils.SetToolTip(secondaryOptions, registerEnabled, "Ведомость учитывается при анализе?");

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
                if (args.KeyCode == Keys.D2 || args.KeyCode == Keys.NumPad2) {
                    int minX = Int32.MaxValue;
                    int minY = Int32.MaxValue;
                    foreach (DataGridViewCell sc in registerDataGridView.SelectedCells) {
                        minX = Math.Min(minX, sc.ColumnIndex);
                        minY = Math.Min(minY, sc.RowIndex);
                    }
                    registerDataGridView.Rows[minY].Cells[minX].Value = "2";
                    if (minY + 1 < registerDataGridView.Rows.Count) {
                        registerDataGridView.ClearSelection();
                        registerDataGridView.Rows[minY + 1].Cells[minX].Selected = true;
                    }
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

                    MenuItem addColumnAction = new MenuItem("Добавить предмет");
                    addColumnAction.Click += new EventHandler(delegate {
                        DataGridViewColumn newCol = new DataGridViewColumn();
                        
                        newCol.Name = "FP";

                        newCol.CellTemplate = new DataGridViewTextBoxCell();

                        // insert after current column
                        registerDataGridView.Columns.Insert(e.ColumnIndex + 1, newCol);
                    });

                    ContextMenu ctx;
                    if (e.ColumnIndex > 2) {
                        ctx = new ContextMenu(new MenuItem[] { deleteColumnAction, addColumnAction });
                    } else  {
                        ctx = new ContextMenu(new MenuItem[] { addColumnAction });
                    }
                    ctx.Show(registerDataGridView, new Point(x, e.Y));
                }
            });

            List<string> autocompleteRanks = 
                dataAccess.GetDataContext().GetTable<Звание>().Select(r => r.Название).ToListTimed();
            List<string> autocompleteNames =
                dataAccess.GetDataContext().GetTable<Военнослужащий>()
                .Where(v => v.Убыл == false)
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
                            where v.Убыл == false
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
            });

            this.Controls.Add(registerDataGridView);
        }

        public Register GetEmptyRegister() {
            return new model.Register {
                id = -1,
                name = "",
                fillDate = DateTime.Now,
                importDate = DateTime.Now,
                editDate = DateTime.Now,
                tags = new List<string>(),
                virt = false,
                enabled = true,
                subjects = new List<string>(),
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

            foreach (string subject in register.subjects) {
                registerDataTable.Columns.Add(new DataColumn(subject));
            }

            foreach (RegisterRecord record in register.records) {
                List<string> cells = new List<string>();
                cells.Add(record.soldier.Код.ToString());
                cells.Add(record.soldier.Звание.Название);
                cells.Add(record.soldier.ФИО);
                foreach (string subject in register.subjects) {
                    Option<Mark> markOpt = record.marks.GetOption(subject);
                    if (markOpt.IsEmpty()) {
                        cells.Add("");
                    }
                    record.marks.GetOption(subject).ForEach(mark => {
                        if (mark is Grade) {
                            cells.Add(((Grade) mark).value.ToString());
                        } else if (mark is Comment) {
                            cells.Add(((Comment) mark).comment.ToString());
                        }
                    });
                }
                registerDataTable.Rows.Add(cells.ToArray());
            }

            registerDataGridView.DataSource = registerDataSet;
            registerDataGridView.DataMember = "register";
            registerDataGridView.Refresh();
            registerDataGridView.ColumnAdded += new DataGridViewColumnEventHandler(delegate (object e, DataGridViewColumnEventArgs args) {
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
        }

        public Register GetRegister() {
            List<string> subjects = registerDataGridView.Columns.ToNormalList<DataGridViewColumn>().Skip(3).Select(col => col.Name).ToList();
            return new Register {
                id = currentRegister.id,
                name = registerName.Text,
                fillDate = registerFillDate.Value,
                importDate = currentRegister.importDate.ToOption().GetOrElse(DateTime.Now),
                editDate = DateTime.Now,
                tags = registerTags.Text.Split(new char[] { ' ' }).Where(t => t.Trim().Length > 0).ToList(),
                virt = registerVirtual.Checked,
                enabled = registerEnabled.Checked,
                subjects = subjects,
                records = 
                    registerDataGridView.Rows.ToNormalList<DataGridViewRow>()
                    .Where(row => row.Cells[0].Value.ToString().Trim().Length > 0)
                    .Select(row => {
                        Dictionary<string, Mark> marks = new Dictionary<string, Mark>();
                        for (int col = 3; col < registerDataGridView.Columns.Count; col++) {
                            Util.ParseInt(row.Cells[col].Value.ToString()).Map(g => {
                                marks.Add(subjects[col - 3], new Grade { value = g });
                                return true;
                            }).GetOrElse(() => {
                                marks.Add(subjects[col - 3], new Comment { comment = row.Cells[col].Value.ToString() });
                                return true;
                            });
                        }
                        return new RegisterRecord {
                            soldier = 
                                dataAccess.GetDataContext().GetTable<Военнослужащий>()
                                .Where(v => v.Код == Int32.Parse(row.Cells[0].Value.ToString()))
                                .ToListTimed().First(),
                            marks = marks
                        };
                    }).ToList()
            };
        }
    }
}
