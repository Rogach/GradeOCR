using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Grader.gui {
    public static class GuiUtils {
        public static void PopulateComboBox(this ComboBox comboBox, Type enumType) {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(Enum.GetNames(enumType).Select(n => n.Replace('_', ' ')).ToArray());
            comboBox.SelectedIndex = 0;
        }

        public static T GetComboBoxEnumValue<T>(this ComboBox comboBox) {
            return (T) Enum.Parse(typeof(T), ((string) comboBox.SelectedItem).Replace(' ', '_'));
        }
    }
}
