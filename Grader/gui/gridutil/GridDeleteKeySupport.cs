using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Grader.gui.gridutil {
    public static class GridDeleteKeySupport {
        public static void AddDeleteKeySupport(DataGridView dataGridView, int fromColumn = 0) {
            dataGridView.KeyUp += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.Delete) {
                    foreach (DataGridViewCell sc in dataGridView.SelectedCells) {
                        if (sc.ColumnIndex >= fromColumn) {
                            sc.Value = "";
                        }
                    }
                }
            });
        }
    }
}
