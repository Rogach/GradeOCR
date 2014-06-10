using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    class FormLayout {
        private Control control;
        private List<LayoutUnit> rows = new List<LayoutUnit>();
        private int controlWidth;
        private int maxLabelWidth = 0;
        private int offset = 3;

        public FormLayout(Control control, int controlWidth = 150) {
            this.control = control;
            this.controlWidth = controlWidth;
        }

        public T Add<T>(string labelText, T control, bool thin = false) where T : Control {
            Label label = new Label();
            label.Text = labelText;
            maxLabelWidth = Math.Max(maxLabelWidth, label.PreferredWidth);
            AddRow(label, control, thin);
            return control;
        }

        public T AddWithLongLabel<T>(string labelText, T control, bool thin = false) where T : Control {
            Label label = new Label();
            label.Text = labelText;
            
            AddRow(label, control, thin);
            return control;
        }

        public T AddFullRow<T>(T control) where T : Control {
            rows.Add(new FullRow { control = control });
            return control;
        }

        private void AddRow(Label label, Control control, bool thin) {
            if (thin) {
                rows.Add(new Row { control = control, label = label, height = control.PreferredSize.Height + 3 });
            } else {
                rows.Add(new Row { control = control, label = label, height = 25 });
            }
        }

        public void AddSpacer(int height) {
            rows.Add(new Spacer { height = height });
        }

        public void PerformLayout() {
            control.SuspendLayout();
            foreach (var r in rows) {
                r.Layout(this);
            }
            control.ResumeLayout(false);
            control.PerformLayout();
        }

        interface LayoutUnit {
            void Layout(FormLayout layout);
        }

        private class Row : LayoutUnit {
            public Control control { get; set; }
            public Label label { get; set; }
            public int height { get; set; }
            public void Layout(FormLayout layout) {
                control.Location = new Point(3 + layout.maxLabelWidth + 5, layout.offset);
                control.Size = new Size(layout.controlWidth, control.PreferredSize.Height);
                layout.control.Controls.Add(control);
                label.Location = new Point(3, layout.offset + 2);
                label.Size = new Size(layout.maxLabelWidth, label.PreferredHeight);
                label.Click += new EventHandler(delegate {
                    control.Focus();
                });
                layout.control.Controls.Add(label);
                layout.offset += height;
            }
        }

        private class Spacer : LayoutUnit {
            public int height { get; set; }
            public void Layout(FormLayout layout) {
                layout.offset += height;
            }
        }

        private class FullRow : LayoutUnit {
            public Control control { get; set; }
            public void Layout(FormLayout layout) {
                control.Location = new Point(3, layout.offset);
                control.Size = new Size(layout.maxLabelWidth + 5 + layout.controlWidth, control.PreferredSize.Height);
                layout.control.Controls.Add(control);
                layout.offset += control.PreferredSize.Height + 3;
            }
        }

    }
}
