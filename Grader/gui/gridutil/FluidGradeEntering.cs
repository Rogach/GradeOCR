using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Grader.gui.gridutil {
    public static class FluidGradeEntering {
        public static void EnableFluidGradeEntering(DataGridView dataGridView, DataTable dataTable, int columnFrom) {
            bool afterGradeType = false;
            dataGridView.PreviewKeyDown += new PreviewKeyDownEventHandler(delegate(object sender, PreviewKeyDownEventArgs args) {
                Dictionary<Keys, string> quickKeys = new Dictionary<Keys, string> {
                    { Keys.D2, "2" }, { Keys.NumPad2, "2" },
                    { Keys.D3, "3" }, { Keys.NumPad3, "3" },
                    { Keys.D4, "4" }, { Keys.NumPad4, "4" },
                    { Keys.D5, "5" }, { Keys.NumPad5, "5" }
                };
                if (quickKeys.ContainsKey(args.KeyCode)) {
                    int minX = Int32.MaxValue;
                    int minY = Int32.MaxValue;
                    foreach (DataGridViewCell sc in dataGridView.SelectedCells) {
                        minX = Math.Min(minX, sc.ColumnIndex);
                        minY = Math.Min(minY, sc.RowIndex);
                    }
                    if (minX >= columnFrom) {
                        if (minY + 1 < dataGridView.Rows.Count) {
                            dataGridView.Rows[minY].Cells[minX].Value = quickKeys[args.KeyCode];
                            dataGridView.ClearSelection();
                            dataGridView.Rows[minY + 1].Cells[minX].Selected = true;
                            dataGridView.CurrentCell = dataGridView.Rows[minY + 1].Cells[minX];
                        } else {
                            DataRow row = dataTable.Rows.Add(new object[] { });
                            row.SetField(dataTable.Columns[minX], quickKeys[args.KeyCode]);
                            dataGridView.Rows.RemoveAt(dataGridView.Rows.Count - 2);
                            dataGridView.ClearSelection();
                            dataGridView.CurrentCell = dataGridView.Rows[dataGridView.Rows.Count - 1].Cells[minX];
                            dataGridView.CurrentCell.Selected = true;
                        }
                        afterGradeType = true;
                    }
                }
            });

            dataGridView.CellBeginEdit += new DataGridViewCellCancelEventHandler(delegate(object sender, DataGridViewCellCancelEventArgs e) {
                if (afterGradeType) {
                    e.Cancel = true;
                    afterGradeType = false;
                }
            });
        }
    }
}
