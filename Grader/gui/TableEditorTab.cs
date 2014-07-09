using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace Grader.gui {
    public class TableEditorTab : TabPage {
        private Entities et;
        private List<TableDefinition> tables;
        private bool tableConstructed = false;
        private TableDefinition currentTable;
        private List<object> objects;

        public TableEditorTab(Entities et) {
            this.et = et;
            tables = new List<TableDefinition> {
                new TableDefinition(
                    name: "Военнослужащий", 
                    allowToAddRows: true,
                    allowToRemoveRows: false,
                    getObjects: () => et.Военнослужащий.ToList().Select(o => (object) o).ToList(), 
                    newObject: () => {
                        var v = et.Военнослужащий.CreateObject();
                        v.Фамилия = "";
                        v.Имя = "";
                        v.Отчество = "";
                        v.КодЗвания = et.rankNameToId["рядовой"];
                        v.КодПодразделения = 1;
                        v.ВУС = 0;
                        v.ТипВоеннослужащего = "курсант";
                        et.Военнослужащий.AddObject(v);
                        return v;
                    },
                    deleteObject: obj => {
                        et.Военнослужащий.DeleteObject((Военнослужащий) obj);
                    },
                    Columns: new List<ColumnDefinition> {
                        new ColumnDefinition("Код", 50, typeof(int), false, obj => ((Военнослужащий) obj).Код, null, null),
                        new ColumnDefinition("Фамилия", 100, typeof(string), true, obj => ((Военнослужащий) obj).Фамилия, 
                            (obj, value) => ((Военнослужащий) obj).Фамилия = (string) value, null),
                        new ColumnDefinition("Имя", 100, typeof(string), true, obj => ((Военнослужащий) obj).Имя, 
                            (obj, value) => ((Военнослужащий) obj).Имя = (string) value, null),
                        new ColumnDefinition("Отчество", 100, typeof(string), true, obj => ((Военнослужащий) obj).Отчество, 
                            (obj, value) => ((Военнослужащий) obj).Отчество = (string) value, null),
                        new ColumnDefinition("Звание", 100, typeof(string), true, obj => et.rankIdToName[((Военнослужащий) obj).КодЗвания],
                            (obj, value) => ((Военнослужащий) obj).КодЗвания = et.rankNameToId[(string) value], 
                            et.rankCache.Select(r => r.Название).ToList()),
                        new ColumnDefinition("Подразделение", 100, typeof(string), true, obj => et.subunitIdToName[((Военнослужащий) obj).КодПодразделения],
                            (obj, value) => ((Военнослужащий) obj).КодПодразделения = et.subunitNameToId[(string) value], 
                            et.subunitCache.Select(s => s.Имя).ToList()),
                        new ColumnDefinition("ВУС", 50, typeof(int), true, obj => ((Военнослужащий) obj).ВУС,
                            (obj, value) => ((Военнослужащий) obj).ВУС = (int) value, null),
                        new ColumnDefinition("ТипВоеннослужащего", 150, typeof(string), true, obj => ((Военнослужащий) obj).ТипВоеннослужащего,
                            (obj, value) => ((Военнослужащий) obj).ТипВоеннослужащего = (string) value,
                            et.Военнослужащий.Select(v => v.ТипВоеннослужащего).Distinct().ToList()),
                        new ColumnDefinition("Убыл", 50, typeof(bool), true, obj => ((Военнослужащий) obj).Убыл,
                            (obj, value) => ((Военнослужащий) obj).Убыл = (bool) value, null),
                        new ColumnDefinition("КМН", 50, typeof(bool), true, obj => ((Военнослужащий) obj).КМН,
                            (obj, value) => ((Военнослужащий) obj).КМН = (bool) value, null),
                        new ColumnDefinition("sortWeight", 100, typeof(int), true, obj => ((Военнослужащий) obj).sortWeight,
                            (obj, value) => ((Военнослужащий) obj).sortWeight = (int) value, null),
                        new ColumnDefinition("Нет допуска на экзамен", 150, typeof(bool), true, obj => ((Военнослужащий) obj).НетДопускаНаЭкзамен,
                            (obj, value) => ((Военнослужащий) obj).НетДопускаНаЭкзамен = (bool) value, null)
                    }
                ),
                new TableDefinition(
                    name: "Должность",
                    allowToAddRows: true,
                    allowToRemoveRows: true,
                    getObjects: () => et.Должность.ToList().Select(d => (object) d).ToList(),
                    newObject: () => {
                        var d = et.Должность.CreateObject();
                        d.КодПодразделения = 1;
                        d.КодВоеннослужащего = et.soldierIdToName.Keys.First();
                        et.Должность.AddObject(d);
                        return d;
                    },
                    deleteObject: (obj) => {
                        et.Должность.DeleteObject((Должность) obj);
                    },
                    Columns: new List<ColumnDefinition> {
                        new ColumnDefinition("Код", 50, typeof(int), false, (obj) => ((Должность) obj).Код, null, null),
                        new ColumnDefinition("Название", 100, typeof(string), true, (obj) => ((Должность) obj).Название, 
                            (obj, value) => ((Должность) obj).Название = (string) value, et.Должность.Select(d => d.Название).Distinct().ToList()),
                        new ColumnDefinition("Подразделение", 150, typeof(string), true, obj => et.subunitIdToName[((Должность) obj).КодПодразделения],
                            (obj, value) => ((Должность) obj).КодПодразделения = et.subunitNameToId[(string) value], 
                            et.subunitCache.Select(s => s.Имя).ToList()),
                        new ColumnDefinition("Военнослужащий", 250, typeof(string), true, obj => et.soldierIdToName[((Должность) obj).КодВоеннослужащего],
                            (obj, value) => ((Должность) obj).КодВоеннослужащего = et.soldierNameToId[(string) value],
                            et.soldierNameCache)
                    }
                ),
                new TableDefinition(
                    name: "Подразделение",
                    allowToAddRows: false,
                    allowToRemoveRows: false,
                    getObjects: () => et.Подразделение.ToList().Select(s => (object) s).ToList(),
                    newObject: null,
                    deleteObject: null,
                    Columns: new List<ColumnDefinition> {
                        new ColumnDefinition("Код", 50, typeof(int), false, (obj) => ((Подразделение) obj).Код, null, null),
                        new ColumnDefinition("Имя", 150, typeof(string), true, obj => ((Подразделение) obj).Имя,
                            (obj, value) => ((Должность) obj).КодПодразделения = et.subunitNameToId[(string) value], null),
                        new ColumnDefinition("Тип", 100, typeof(string), true, obj => ((Подразделение) obj).Тип,
                            (obj, value) => ((Подразделение) obj).Тип = (string) value,
                            et.Подразделение.Select(s => s.Тип).Distinct().ToList()),
                        new ColumnDefinition("ТипОбучения", 100, typeof(string), true, obj => ((Подразделение) obj).ТипОбучения,
                            (obj, value) => ((Подразделение) obj).ТипОбучения = (string) value,
                            et.Подразделение.Select(s => s.ТипОбучения).Distinct().ToList()),
                        new ColumnDefinition("Актив", 50, typeof(bool), true, obj => ((Подразделение) obj).Актив,
                            (obj, value) => ((Подразделение) obj).Актив = (bool) value, null),
                        new ColumnDefinition("ИмяРодительный", 150, typeof(string), true, obj => ((Подразделение) obj).ИмяРодительный,
                            (obj, value) => ((Подразделение) obj).ИмяРодительный = (string) value, null),
                        new ColumnDefinition("ИмяПредложный", 150, typeof(string), true, obj => ((Подразделение) obj).ИмяПредложный,
                            (obj, value) => ((Подразделение) obj).ИмяПредложный = (string) value, null),
                        new ColumnDefinition("ИмяКраткое", 150, typeof(string), true, obj => ((Подразделение) obj).ИмяКраткое,
                            (obj, value) => ((Подразделение) obj).ИмяКраткое = (string) value, null),
                        new ColumnDefinition("ПодразделениеОхраны", 50, typeof(bool), true, obj => ((Подразделение) obj).ПодразделениеОхраны,
                            (obj, value) => ((Подразделение) obj).ПодразделениеОхраны = (bool) value, null)
                    }
                ),
                new TableDefinition(
                    name: "ВусНаЦикле",
                    allowToAddRows: true,
                    allowToRemoveRows: true,
                    getObjects: () => et.ВусНаЦикле.ToList().Select(v => (object) v).ToList(),
                    newObject: () => {
                        var v = et.ВусНаЦикле.CreateObject();
                        v.КодЦикла = et.subunitNameToId["1 цикл"];
                        et.ВусНаЦикле.AddObject(v);
                        return v;
                    },
                    deleteObject: (obj) => {
                        et.ВусНаЦикле.DeleteObject((ВусНаЦикле) obj);
                    },
                    Columns: new List<ColumnDefinition> {
                        new ColumnDefinition("Код", 50, typeof(int), false, obj => ((ВусНаЦикле) obj).Код, null, null),
                        new ColumnDefinition("Цикл", 150, typeof(string), true, obj => et.subunitIdToName[((ВусНаЦикле) obj).КодЦикла],
                            (obj, value) => ((ВусНаЦикле) obj).КодЦикла = et.subunitNameToId[(string) value],
                            et.subunitCache.Select(s => s.Имя).ToList()),
                        new ColumnDefinition("ВУС", 50, typeof(int), true, obj => ((ВусНаЦикле) obj).ВУС,
                            (obj, value) => ((ВусНаЦикле) obj).ВУС = (int) value, null)
                    }
                )
            };
            this.InitializeComponent();
        }

        class TableDefinition {
            public string name;
            public bool allowToAddRows;
            public bool allowToRemoveRows;
            public Func<List<object>> getObjects;
            public Func<object> newObject;
            public Action<object> deleteObject;
            public List<ColumnDefinition> Columns;
            
            public TableDefinition(
                string name, 
                bool allowToAddRows, 
                bool allowToRemoveRows, 
                Func<List<object>> getObjects,
                Func<object> newObject,
                Action<object> deleteObject,
                List<ColumnDefinition> Columns) {
                    this.name = name;
                    this.allowToAddRows = allowToAddRows;
                    this.allowToRemoveRows = allowToRemoveRows;
                    this.Columns = Columns;
                    this.getObjects = getObjects;
                    this.newObject = newObject;
                    this.deleteObject = deleteObject;
            }

            public override string ToString() {
                return name;
            }
        }

        class ColumnDefinition {
            string name;
            int columnWidth;
            Type type;
            bool editable;
            Func<object, object> getValue;
            Action<object, object> setValue;
            List<string> completions;

            public ColumnDefinition(
                string name,
                int columnWidth,
                Type type,
                bool editable,
                Func<object, object> getValue,
                Action<object, object> setValue,
                List<string> completions) {
                    this.name = name;
                    this.columnWidth = columnWidth;
                    this.type = type;
                    this.editable = editable;
                    this.getValue = getValue;
                    this.setValue = setValue;
                    this.completions = completions;
            }

            public string Name() {
                return name;
            }
            public int ColumnWidth() {
                return columnWidth;
            }
            public Type GetColType() {
                return type;
            }
            public bool isEditable() {
                return editable;
            }
            public object GetValue(object obj) {
                return getValue(obj);
            }
            public void SetValue(object obj, object value) {
                setValue(obj, value);
            }
            public List<string> GetCompletions() {
                return completions;
            }
        }

        private ComboBox tableSelector;
        private TextBox filterString;
        private DataGridView tableView;

        private void InitializeComponent() {
            this.Text = "Редактор данных";
            this.Size = new Size(1200, 800);
            this.SuspendLayout();

            tableSelector = new ComboBox();
            tableSelector.Items.AddRange(tables.ToArray());
            tableSelector.SelectedIndexChanged += new EventHandler(delegate {
                filterString.Text = "";
                SetEditedTable((TableDefinition) tableSelector.SelectedItem, "");
            });

            tableSelector.Size = new Size(150, 25);
            tableSelector.Location = new Point(3, 3);
            this.Controls.Add(tableSelector);

            Label filterLabel = new Label { Text = "Фильтр" };
            filterLabel.Size = new Size(50, 15);
            filterLabel.Location = new Point(183, 6);
            this.Controls.Add(filterLabel);

            filterString = new TextBox();
            filterString.Size = new Size(150, 25);
            filterString.Location = new Point(233, 3);
            filterString.KeyUp += new KeyEventHandler(delegate {
                if (currentTable != null) {
                    SetEditedTable(currentTable, filterString.Text);
                }
            });
            this.Controls.Add(filterString);

            tableView = new DataGridView();
            tableView.Size = new Size(1190, 765);
            tableView.Location = new Point(3, 30);
            tableView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            tableView.AllowUserToOrderColumns = false;
            tableView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            SetUpTableEventHandlers();
            this.Controls.Add(tableView);

            this.ResumeLayout(false);
        }

        private void SetUpTableEventHandlers() {
            tableView.CellBeginEdit += new DataGridViewCellCancelEventHandler(delegate(object sender, DataGridViewCellCancelEventArgs e) {
                if (!currentTable.Columns[e.ColumnIndex].isEditable()) {
                    e.Cancel = true;
                }
            });
            tableView.CellValueChanged += new DataGridViewCellEventHandler(delegate(object sender, DataGridViewCellEventArgs e) {
                object newValue = tableView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                object editedObj;
                if (e.RowIndex >= objects.Count) {
                    // new object
                    editedObj = currentTable.newObject();
                    objects.Add(editedObj);
                } else {
                    // editing already present object
                    editedObj = objects[e.RowIndex];
                }
                object oldValue = currentTable.Columns[e.ColumnIndex].GetValue(editedObj);
                try {
                    currentTable.Columns[e.ColumnIndex].SetValue(editedObj, newValue);
                    et.SaveChanges();
                } catch (Exception ex) {
                    tableView.EndEdit();
                    tableView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = oldValue;
                    throw ex;
                }
            });
            tableView.RowsRemoved += new DataGridViewRowsRemovedEventHandler(delegate(object sender, DataGridViewRowsRemovedEventArgs e) {
                if (!tableConstructed) {
                    for (int i = 0; i < e.RowCount; i++) {
                        currentTable.deleteObject(objects[e.RowIndex]);
                        objects.RemoveAt(e.RowIndex);
                        et.SaveChanges();
                    }
                }
            });
            tableView.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(
                delegate(object sender, DataGridViewEditingControlShowingEventArgs args) {
                    List<string> completions = currentTable.Columns[tableView.CurrentCell.ColumnIndex].GetCompletions();
                    if (tableView.EditingControl is TextBox) {
                        TextBox ed = (TextBox) tableView.EditingControl;
                        if (completions != null) {
                            ed.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                            ed.AutoCompleteSource = AutoCompleteSource.CustomSource;
                            ed.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                            ed.AutoCompleteCustomSource.AddRange(completions.ToArray());
                        } else {
                            ed.AutoCompleteCustomSource = null;
                        }
                    }
                });
            tableView.ColumnAdded += new DataGridViewColumnEventHandler(delegate(object e, DataGridViewColumnEventArgs args) {
                args.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                args.Column.Width = currentTable.Columns[args.Column.Index].ColumnWidth();
            });
        }

        private void SetEditedTable(TableDefinition tdef, string filter) {
            tableConstructed = true;
            currentTable = tdef;
            tableView.AllowUserToAddRows = tdef.allowToAddRows;
            tableView.AllowUserToDeleteRows = tdef.allowToRemoveRows;

            DataSet dataSet = new DataSet("dt");
            DataTable dataTable = new DataTable("dt");
            dataSet.Tables.Add(dataTable);

            foreach (var col in tdef.Columns) {
                dataTable.Columns.Add(col.Name(), col.GetColType());
            }
            objects = new List<object>();
            foreach (var obj in tdef.getObjects()) {
                List<object> cells = new List<object>();
                bool matches = false;
                foreach (var col in tdef.Columns) {
                    object value = col.GetValue(obj);
                    matches = matches || value.ToString().Contains(filter);
                    cells.Add(col.GetValue(obj));
                }
                if (matches) {
                    dataTable.Rows.Add(cells.ToArray());
                    objects.Add(obj);
                }
            }

            tableView.Columns.Clear();
            tableView.DataSource = dataSet;
            tableView.DataMember = "dt";
            tableConstructed = false;
        }
    }
}
