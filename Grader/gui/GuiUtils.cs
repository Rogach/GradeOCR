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

        public static void SetToolTip(Control control, string tooltip) {
            ToolTip tip = new ToolTip();
            tip.ShowAlways = true;
            tip.InitialDelay = 500;
            tip.ReshowDelay = 500;
            tip.SetToolTip(control, tooltip);
        }

        public static void SetToolTip(FormLayout layout, Control control, string tooltip) {
            SetToolTip(control, tooltip);
            layout.LabelForControl(control).ForEach(label => {
                SetToolTip(label, tooltip);
            });
        }
    }
}
