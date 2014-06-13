using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUtil;

namespace Grader.gui {
    public partial class InputDialog : Form {
        public InputDialog() {
            InitializeComponent();
        }

        public static Option<string> ShowInputDialog(string question, string title, string value = "") {
            InputDialog id = new InputDialog();
            id.questionLabel.Text = question;
            id.Text = title;
            id.textBox.Text = value;
            
            if (id.ShowDialog() == DialogResult.OK) {
                return new Some<string>(id.textBox.Text);
            } else {
                return new None<string>();
            }
        }

        private void buttonOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void buttonCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
    }
}
