using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grader.model;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using LibUtil;

namespace Grader.gui {
    public class RegisterEditor : Panel {
        DataAccess dataAccess;
        Register currentRegister = null;

        public RegisterEditor(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private TextBox registerName;
        private DateTimePicker registerFillDate;
        private Label registerImportDate;
        private Label registerEditDate;
        private TextBox tags;

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

            tags = layout.Add("Тэги", new TextBox());

            layout.AddSpacer(10);

            layout.PerformLayout();

            registerDataGridView = new DataGridView();
            registerDataGridView.Location = new Point(0, layout.GetY());
            registerDataGridView.Size = new Size(600, 600 - layout.GetY());
            registerDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            
            registerDataGridView.AllowUserToAddRows = true;
            registerDataGridView.AllowUserToDeleteRows = true;
            registerDataGridView.AllowUserToResizeColumns = true;
            registerDataGridView.AllowUserToOrderColumns = false;
            registerDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            registerDataGridView.KeyDown += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.V && e.Control) {
                    DataGridViewCell c = registerDataGridView.SelectedCells[0];
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

                    Console.WriteLine("minX = {0}, minY = {0}", minX, minY);
                    Console.WriteLine("clipX = {0}, clipY = {0}", clipX, clipY);

                    foreach (var line in copiedData) {
                        Console.WriteLine("paste line: " + line.MkString(","));
                    }

                    Console.WriteLine(registerDataGridView.Rows.Count);

                    for (int row = 0; row < clipY; row++) {
                        if (row >= registerDataGridView.Rows.Count - 1) {
                            Console.WriteLine("Adding");
                            Console.WriteLine("Before add " + registerDataGridView.Rows.Count);
                            registerDataTable.Rows.Add(new object[] {});
                            Console.WriteLine("After add " + registerDataGridView.Rows.Count);
                        }
                        for (int col = 0; col < clipX; col++) {
                            if (minX + col < registerDataGridView.Columns.Count) {
                                Console.WriteLine("row = {0}, col = {1}", row, col);
                                registerDataGridView.Rows[minY + row].Cells[minX + col].Value = copiedData[row][col];
                            }
                        }
                    }

                }
            });

            //registerDataGridView.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(delegate(object sender, DataGridViewCellMouseEventArgs e) {
            //    if (e.Button == MouseButtons.Right) {
            //        int x = 0;
            //        x += registerDataGridView.RowHeadersWidth;
            //        for (int ci = 0; ci < e.ColumnIndex; ci++) {
            //            x += registerDataGridView.Columns[ci].Width;
            //        }
            //        x += e.X;

            //        MenuItem deleteColumnAction = new MenuItem("delete column");
            //        deleteColumnAction.Click += new EventHandler(delegate {
            //            registerDataGridView.Columns.Remove(registerDataGridView.Columns[e.ColumnIndex]);
            //        });

            //        MenuItem addColumnAction = new MenuItem("add column");
            //        addColumnAction.Click += new EventHandler(delegate {
            //            DataGridViewColumn newCol = new DataGridViewColumn();
            //            newCol.Name = "new col";

            //            newCol.CellTemplate = new DataGridViewTextBoxCell();
            //            newCol.CellTemplate.Style.BackColor = Color.AliceBlue;

            //            // insert after current column
            //            registerDataGridView.Columns.Insert(e.ColumnIndex + 1, newCol);
            //        });

            //        ContextMenu ctx;
            //        if (e.ColumnIndex > 0) {
            //            ctx = new ContextMenu(new MenuItem[] { deleteColumnAction, addColumnAction });
            //        } else {
            //            ctx = new ContextMenu(new MenuItem[] { addColumnAction });
            //        }
            //        ctx.Show(registerDataGridView, new Point(x, e.Y));
            //    }
            //});

            this.Controls.Add(registerDataGridView);
        }

        public void SetRegister(Register register) {
            registerName.Text = register.name;
            registerFillDate.Value = register.fillDate;
            registerImportDate.Text = register.importDate.ToOption().Map(d => d.ToLongDateString()).GetOrElse("");
            registerEditDate.Text = register.editDate.ToOption().Map(d => d.ToLongDateString()).GetOrElse("");
            tags.Text = register.tags.MkString(" ");
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
                    args.Column.Width = 30;
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
            throw new NotImplementedException();
        }
    }
}
