using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using LibUtil;

namespace Grader.gui {
    public class FormLayout {
        private Control control;
        private List<LayoutUnit> rows = new List<LayoutUnit>();
        private int controlWidth;
        private int maxLabelWidth;
        private int y;
        private int x;
        private Dictionary<Control, Label> labelForControl = new Dictionary<Control, Label>();

        public FormLayout(Control control, int x = 3, int y = 3, int controlWidth = 150, int maxLabelWidth = 0) {
            this.control = control;
            this.controlWidth = controlWidth;
            this.x = x;
            this.y = y;
            this.maxLabelWidth = maxLabelWidth;
        }

        public T Add<T>(string labelText, T control, bool thin = false) where T : Control {
            Label label = new Label();
            label.Text = labelText;
            labelForControl.Add(control, label);
            maxLabelWidth = Math.Max(maxLabelWidth, label.PreferredWidth);
            if (thin) {
                rows.Add(new Row { control = control, label = label, height = control.PreferredSize.Height + 3 });
            } else {
                rows.Add(new Row { control = control, label = label, height = 25 });
            }
            return control;
        }

        public T AddFullRow<T>(T control, int leftPadding = 0) where T : Control {
            rows.Add(new FullRow { control = control, leftPadding = leftPadding });
            return control;
        }

        public void AddControlGroup(string labelText, List<Control> controls) {
            Label label = new Label();
            label.Text = labelText;
            maxLabelWidth = Math.Max(maxLabelWidth, label.PreferredWidth);
            rows.Add(new ControlGroup { label = label, controls = controls });
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
            this.x += maxLabelWidth + 5 + controlWidth;
        }

        public Option<Label> LabelForControl(Control control) {
            return labelForControl.GetOption(control);
        }

        public int GetY() {
            return y;
        }

        public int GetX() {
            return x;
        }

        public int GetControlX() {
            return x + maxLabelWidth + 5;
        }

        interface LayoutUnit {
            void Layout(FormLayout layout);
        }

        private class Row : LayoutUnit {
            public Control control { get; set; }
            public Label label { get; set; }
            public int height { get; set; }
            public void Layout(FormLayout layout) {
                if (control is Label) {
                    control.Location = new Point(layout.GetControlX(), layout.y + 2);
                    control.Size = new Size(layout.controlWidth, control.PreferredSize.Height);
                } else {
                    control.Location = new Point(layout.GetControlX(), layout.y);
                    control.Size = new Size(layout.controlWidth, control.PreferredSize.Height);
                }
                layout.control.Controls.Add(control);
                label.Location = new Point(layout.x, layout.y + 2);
                label.Size = new Size(layout.maxLabelWidth, label.PreferredHeight);
                label.Click += new EventHandler(delegate {
                    control.Focus();
                });
                layout.control.Controls.Add(label);
                layout.y += height;
            }
        }

        private class Spacer : LayoutUnit {
            public int height { get; set; }
            public void Layout(FormLayout layout) {
                layout.y += height;
            }
        }

        private class FullRow : LayoutUnit {
            public Control control { get; set; }
            public int leftPadding { get; set; }
            public void Layout(FormLayout layout) {
                control.Location = new Point(layout.x + leftPadding, layout.y);
                control.Size = new Size(layout.maxLabelWidth + 5 + layout.controlWidth - leftPadding, control.PreferredSize.Height);
                layout.control.Controls.Add(control);
                layout.y += control.PreferredSize.Height + 3;
            }
        }

        private class ControlGroup : LayoutUnit {
            public Label label { get; set; }
            public List<Control> controls { get; set; }
            public void Layout(FormLayout layout) {
                int dy = 0;
                foreach (var control in controls) {
                    control.Location = new Point(layout.GetControlX(), layout.y + dy);
                    control.Size = new Size(layout.controlWidth, control.PreferredSize.Height);
                    layout.control.Controls.Add(control);
                    dy += control.PreferredSize.Height + 1;
                }
                label.Location = new Point(layout.x, layout.y + (dy - label.PreferredHeight) / 2);
                label.Size = new Size(layout.maxLabelWidth, label.PreferredHeight);
                layout.control.Controls.Add(label);
                layout.y += dy;
            }
        }

    }
}
