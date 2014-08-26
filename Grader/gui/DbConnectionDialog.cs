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
            KeyEventHandler inputFieldsKeyHandler = new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.Enter) {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                } else if (e.KeyCode == Keys.Escape) {
                    this.DialogResult = DialogResult.Cancel;
                    this.Hide();
                }
            });

            this.Shown += new EventHandler(delegate {
                this.server_text.KeyDown += inputFieldsKeyHandler;
                this.port_text.KeyDown += inputFieldsKeyHandler;
                this.user_text.KeyDown += inputFieldsKeyHandler;
                this.password_text.KeyDown += inputFieldsKeyHandler;
            });
        }

        private void ok_button_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void cancel_button_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            if (FocusOnPassword) {
                this.password_text.Focus();
            }
        }

        public bool FocusOnPassword { get; set; }

        public string Server {
            get {
                return server_text.Text;
            }
            set {
                server_text.Text = value;
            }
        }

        public string Port {
            get {
                return port_text.Text;
            }
            set {
                port_text.Text = value;
            }
        }

        public string User {
            get {
                return user_text.Text;
            }
            set {
                user_text.Text = value;
            }
        }

        public string Password {
            get {
                return password_text.Text;
            }
            set {
                password_text.Text = value;
            }
        }

        public string ConnectionString {
            get {
                return String.Format("server={0};port={1};uid={2};password={3};database=grader;charset=utf8", Server, Port, User, Password);
            }
        }
    }
}
