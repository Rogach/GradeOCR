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
    public partial class DbConnectionDialog : Form {
        public DbConnectionDialog() {
            InitializeComponent();
        }

        public static Option<string> ShowDbConnectionDialog() {
            var dcd = new DbConnectionDialog();
            dcd.port_text.Text = "3306";

            if (dcd.ShowDialog() == DialogResult.OK) {
                return new Some<string>(dcd.ConnectionString);
            } else {
                return new None<string>();
            }
        }

        private void ok_button_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void cancel_button_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        public string server {
            get {
                return server_text.Text;
            }
        }

        public string port {
            get {
                return port_text.Text;
            }
        }

        public string user {
            get {
                return user_text.Text;
            }
        }

        public string password {
            get {
                return password_text.Text;
            }
        }

        public string ConnectionString {
            get {
                return String.Format("server={0};port={1};uid={2};password={3};database=grader;charset=utf8", server, port, user, password);
            }
        }
    }
}
