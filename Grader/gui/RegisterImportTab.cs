using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    class RegisterImportTab : TabPage {
        private DataAccess dataAccess;

        public RegisterImportTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private void InitializeComponent() {
            this.Text = "Внесение ведомостей";
            this.Size = new Size(1200, 800);

            Button newRegisterButton = new Button();
            newRegisterButton.Text = "Новая ведомость";
            newRegisterButton.Location = new Point(3, 3);
            newRegisterButton.Size = new Size(150, 25);
            this.Controls.Add(newRegisterButton);

            ListBox registerList = new ListBox();
            registerList.Items.Add(new RegisterDesc("asdf", 1));
            registerList.Items.Add(new RegisterDesc("aoeu", 2));
            registerList.Location = new Point(3, 30);
            registerList.Size = new Size(150, 770);
            registerList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            registerList.DoubleClick += new EventHandler(delegate {
                Console.WriteLine(((RegisterDesc) registerList.Items[registerList.SelectedIndex]).id);
            });
            this.Controls.Add(registerList);

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(160, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            RegisterEditor registerEditor = new RegisterEditor(dataAccess);
            registerEditor.Location = new Point(170, 0);
            registerEditor.Size = new Size(1030, 800);
            registerEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.Controls.Add(registerEditor);

            model.RegisterRecord rec1 = new model.RegisterRecord {
                soldier = new Военнослужащий {
                    Фамилия = "Александров",
                    Имя = "Александр",
                    Отчество = "Александрович",
                    Код = 1,
                    Звание = new Звание {
                        Название = "рядовой"
                    }
                },
                marks = new Dictionary<string, model.Mark> {
                            {"OGN", new model.Grade { value = 4 }},
                            {"STR", new model.Comment { comment = "ab" }}
                        }
            };
            model.RegisterRecord rec2 = new model.RegisterRecord {
                soldier = new Военнослужащий {
                    Фамилия = "Белов",
                    Имя = "Билык",
                    Отчество = "Биарутдинович",
                    Код = 2,
                    Звание = new Звание {
                        Название = "рядовой"
                    }
                },
                marks = new Dictionary<string, model.Mark> {
                            {"OGN", new model.Grade { value = 5 }}
                        }
            };

            model.Register reg = new model.Register {
                name = "alpha",
                fillDate = DateTime.Now,
                importDate = DateTime.Now,
                editDate = DateTime.Now,
                tags = new List<string> { "a", "b" },
                subjects = new List<string> { "OGN", "STR" },
                records = new List<model.RegisterRecord> {
                    rec1, rec2
                }
            };

            registerEditor.SetRegister(reg);

            //registerEditor.Visible = false;
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
