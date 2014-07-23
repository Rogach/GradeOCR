using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Grader.gui.gridutil {
    public static class GridPasteSupport {
        public static void AddPasteSupport(DataGridView dataGridView) {
            dataGridView.KeyDown += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.V && e.Control) {
                    int minX = Int32.MaxValue;
                    int minY = Int32.MaxValue;
                    foreach (DataGridViewCell sc in dataGridView.SelectedCells) {
                        minX = Math.Min(minX, sc.ColumnIndex);
                        minY = Math.Min(minY, sc.RowIndex);
                    }

                    List<List<string>> copiedData =
                        Clipboard.GetText(TextDataFormat.UnicodeText)
                        .Split(new char[] { '\n' })
                        .Select(line => line.Split(new char[] { '\t' }).ToList())
                        .ToList();

                    int clipX = copiedData.Select(line => line.Count).Max();
                    int clipY = copiedData.Count;

                    for (int row = 0; row < clipY; row++) {
                        if (row >= dataGridView.Rows.Count - 1) {
                            DataTable dataTable = ((DataSet) dataGridView.DataSource).Tables[0];
                            dataTable.Rows.Add(new object[] { });
                        }
                        for (int col = 0; col < clipX; col++) {
                            if (minX + col < dataGridView.Columns.Count) {
                                dataGridView.Rows[minY + row].Cells[minX + col].Value = copiedData[row][col];
                            }
                        }
                    }

                }
            });
        }
    }
}
