using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    class RegisterImportTab : TabPage {
        private DataAccess dataAccess;

        private static string TAB_NAME = "Внесение ведомостей";

        public RegisterImportTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            this.Text = TAB_NAME;
            this.Size = new Size(1200, 800);

            Button newRegisterButton = new Button();
            newRegisterButton.Text = "Новая ведомость";
            newRegisterButton.Location = new Point(3, 3);
            newRegisterButton.Size = new Size(150, 25);
            this.Controls.Add(newRegisterButton);

            ListBox registerList = new ListBox();
            registerList.Location = new Point(3, 30);
            registerList.Size = new Size(150, 770);
            registerList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            registerList.DoubleClick += new EventHandler(delegate {

            });
            this.Controls.Add(registerList);

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(160, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            RegisterEditor registerEditor = new RegisterEditor(dataAccess);
            registerEditor.Location = new Point(170, 0);
            registerEditor.Size = new Size(1020, 770);
            registerEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.Controls.Add(registerEditor);
            
            Button saveRegister = new Button();
            saveRegister.Text = "Сохранить";
            saveRegister.Location = new Point(170, 775);
            saveRegister.Size = new Size(100, 25);
            saveRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(saveRegister);

            Button cancelRegister = new Button();
            cancelRegister.Text = "Отменить";
            cancelRegister.Location = new Point(280, 775);
            cancelRegister.Size = new Size(100, 25);
            cancelRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(cancelRegister);

            registerEditor.Enabled = false;
            saveRegister.Enabled = false;
            cancelRegister.Enabled = false;
        }

        class RegisterDesc {
            public string name;
            public int id;
            public RegisterDesc(string name, int id) {
                this.name = name;
                this.id = id;
            }
            public override string ToString() {
                return name;
            }
        }
    }
}
