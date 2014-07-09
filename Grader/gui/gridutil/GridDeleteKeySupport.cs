using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Grader.gui.gridutil {
    public static class GridDeleteKeySupport {
        public static void AddDeleteKeySupport(DataGridView dataGridView, Func<int, bool> isEditingAllowed) {
            bool rowWasRemoved = false;
            dataGridView.RowsRemoved += new DataGridViewRowsRemovedEventHandler(delegate {
                rowWasRemoved = true;
            });
            dataGridView.KeyUp += new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.Delete && !rowWasRemoved) {
                    foreach (DataGridViewCell sc in dataGridView.SelectedCells) {
                        if (isEditingAllowed(sc.ColumnIndex)) {
                            sc.Value = "";
                        }
                    }
                }
                rowWasRemoved = false;
            });
        }
    }
}
