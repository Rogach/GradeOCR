using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Grader.model;
using LibUtil;
using System.Data.Linq;
using Grader.util;

namespace Grader.gui {
    class RegisterImportTab : TabPage {
        private Entities et;

        private static string TAB_NAME = "Внесение ведомостей";

        public RegisterImportTab(Entities et) {
            this.et = et;
            this.InitializeComponent();
        }

        private ListView registerList;
        private RegisterEditor registerEditor;
        private Button newRegisterButton;
        private Button saveRegister;
        private Button cancelRegister;

        private bool _changesPending = false;
        private bool changesPending {
            get {
                return _changesPending;
            }
            set {
                if (value) {
                    this.Text = TAB_NAME + " *";
                } else {
                    this.Text = TAB_NAME;
                }
                _changesPending = value;
            }
        }

        private void InitializeComponent() {
            this.Text = TAB_NAME;
            this.Size = new Size(1200, 800);

            newRegisterButton = new Button();
            newRegisterButton.Text = "Новая ведомость";
            newRegisterButton.Location = new Point(3, 3);
            newRegisterButton.Size = new Size(200, 25);
            newRegisterButton.Click += new EventHandler(delegate {
                if (CheckForUnsavedChanges()) {
                    AskForRegisterInit().ForEach(register => {
                        DeselectRegisterInList();
                        changesPending = true;
                        registerEditor.SetRegister(register);
                        SetRegisterPanelEnabled(true);
                    });
                }
            });
            this.Controls.Add(newRegisterButton);

            registerList = new ListView();
            registerList.View = View.List;
            registerList.MultiSelect = false;
            registerList.FullRowSelect = true;
            registerList.Location = new Point(3, 30);
            registerList.Size = new Size(200, 770);
            registerList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            registerList.DoubleClick += new EventHandler(delegate {
                if (registerList.SelectedIndices.Count > 0) {
                    ListViewItem item = registerList.Items[registerList.SelectedIndices[0]];
                    RegisterDesc rd = (RegisterDesc) item.Tag;
                    if (CheckForUnsavedChanges()) {
                        SetRegisterPanelEnabled(false);
                        SelectRegisterInList(rd.id);
                        Register r = RegisterMarshaller.LoadRegister(rd.id, et);
                        registerEditor.SetRegister(r);
                        changesPending = false;
                        SetRegisterPanelEnabled(true);
                    }
                }
            });
            registerList.MouseClick += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Clicks == 1 && e.Button == MouseButtons.Right) {
                    ListViewItem item = registerList.Items[registerList.SelectedIndices[0]];
                    RegisterDesc rd = (RegisterDesc) item.Tag;

                    MenuItem deleteRegister = new MenuItem("Удалить ведомость");
                    deleteRegister.Click += new EventHandler(delegate {
                        DialogResult confirmationResult =
                            MessageBox.Show(
                                String.Format("Действительно удалить ведомость '{0}'?", rd.name), 
                                "Удаление ведомости", 
                                MessageBoxButtons.OKCancel, 
                                MessageBoxIcon.Warning, 
                                MessageBoxDefaultButton.Button1
                                );
                        if (confirmationResult == DialogResult.OK) {
                            RegisterMarshaller.DeleteRegister(rd.id, et);
                            UpdateRegisterList();
                        }
                    });

                    ContextMenu contextMenu = new ContextMenu(new MenuItem[] { deleteRegister });
                    contextMenu.Show(registerList, e.Location);
                }
            });
            this.Controls.Add(registerList);
            UpdateRegisterList();

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(210, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            registerEditor = new RegisterEditor(et);
            registerEditor.Location = new Point(220, 0);
            registerEditor.Size = new Size(970, 770);
            registerEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            registerEditor.RegisterEdited.AddEventListener(() => {
                changesPending = true;
            });
            this.Controls.Add(registerEditor);
            
            saveRegister = new Button();
            saveRegister.Text = "Сохранить";
            saveRegister.Location = new Point(220, 775);
            saveRegister.Size = new Size(100, 25);
            saveRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            saveRegister.Click += new EventHandler(delegate {
                SetRegisterPanelEnabled(false);
                Register register = Util.Timed<Register>("get register", () => registerEditor.GetRegister());
                Util.Timed("save register", () => RegisterMarshaller.SaveRegister(registerEditor.GetRegister(), et));
                changesPending = false;
                UpdateRegisterList();
                SelectRegisterInList(register.id);
                SetRegisterPanelEnabled(true);
            });
            this.Controls.Add(saveRegister);

            cancelRegister = new Button();
            cancelRegister.Text = "Отменить";
            cancelRegister.Location = new Point(330, 775);
            cancelRegister.Size = new Size(100, 25);
            cancelRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cancelRegister.Click += new EventHandler(delegate {
                SetRegisterPanelEnabled(false);
                registerEditor.SetRegister(registerEditor.GetEmptyRegister());
                changesPending = false;
                DeselectRegisterInList();
            });
            this.Controls.Add(cancelRegister);

            SetRegisterPanelEnabled(false);
        }

        public bool CheckForUnsavedChanges() {
            if (changesPending) {
                DialogResult result = MessageBox.Show("Сохранить изменения в текущей ведомости?", "Несохраненные изменения", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) {
                    return false;
                } else if (result == DialogResult.OK) {
                    RegisterMarshaller.SaveRegister(registerEditor.GetRegister(), et);
                    changesPending = false;
                    return true;
                } else if (result == DialogResult.No) {
                    return true;
                } else {
                    throw new Exception("Unexpected dialog result: " + result);
                }
            } else {
                return true;
            }
        }

        public void UpdateRegisterList() {
            registerList.Items.Clear();
            et.Ведомость
                .OrderByDescending(r => r.ДатаВнесения)
                .Select(r => new RegisterDesc { 
                    id = r.Код, 
                    name = r.Название,
                    fillDate = r.ДатаВнесения, 
                    virt = r.Виртуальная, 
                    enabled = r.Включена
                })
                .ToList().ForEach(rd => {
                    ListViewItem item = new ListViewItem(rd.ToString());
                    item.Tag = rd;
                    registerList.Items.Add(item);
                });
            UpdateRegisterListColors();
        }

        public void DeselectRegisterInList() {
            registerList.SelectedIndices.Clear();
            UpdateRegisterListColors();
        }

        public void UpdateRegisterListColors() {
            foreach (ListViewItem item in registerList.Items) {
                RegisterDesc rd = (RegisterDesc) item.Tag;
                if (rd.virt) {
                    if (rd.enabled) {
                        item.BackColor = Color.FromArgb(171, 191, 255);
                    } else {
                        item.BackColor = Color.FromArgb(250, 250, 150);
                    }
                } else {
                    if (!rd.enabled) {
                        item.BackColor = Color.FromArgb(255, 200, 200);
                    } else {
                        item.BackColor = Color.White;
                    }
                }
            }
        }

        public void SelectRegisterInList(int registerId) {
            UpdateRegisterListColors();
            foreach (ListViewItem item in registerList.Items) {
                if (((RegisterDesc) item.Tag).id == registerId) {
                    item.BackColor = Color.LightGray;
                }
            }
        }

        public void SetRegisterPanelEnabled(bool enabled) {
            registerEditor.Enabled = enabled;
            saveRegister.Enabled = enabled;
            cancelRegister.Enabled = enabled;
        }

        class RegisterDesc {
            public int id { get; set; }
            public string name { get; set; }
            public DateTime fillDate { get; set; }
            public bool virt { get; set; }
            public bool enabled { get; set; }
            public override string ToString() {
                return String.Format("{0} ({1})", name, fillDate.ToString("dd.MM.yyyy"));
            }
        }

        public Option<Register> AskForRegisterInit() {
            Form f = new Form();
            f.Text = "Новая ведомость";
            f.SuspendLayout();
            
            PersonSelector personSelector = new PersonSelector(et);
            personSelector.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            f.Location = new Point(0, 0);
            f.Size = new Size(personSelector.PreferredSize.Width + 15, personSelector.PreferredSize.Height + 105);
            f.Controls.Add(personSelector);

            CheckBox forExam = new CheckBox { Text = "Для экзмена?" };
            forExam.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            forExam.Size = new Size(100, 20);
            forExam.Location = new Point(5, personSelector.Height + 5);
            f.Controls.Add(forExam);
            
            Button buttonOk = new Button { Text = "ОК" };
            buttonOk.Click += new EventHandler(delegate {
                f.DialogResult = DialogResult.OK;
                f.Hide();
            });
            buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonOk.Size = new Size(100, 25);
            buttonOk.Location = new Point(35, personSelector.Height + 35);
            f.Controls.Add(buttonOk);

            Button buttonCancel = new Button { Text = "Отменить" };
            buttonCancel.Click += new EventHandler(delegate {
                f.DialogResult = DialogResult.Cancel;
                f.Hide();
            });
            buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonCancel.Size = new Size(100, 25);
            buttonCancel.Location = new Point(145, personSelector.Height + 35);
            f.Controls.Add(buttonCancel);

            f.ResumeLayout(false);

            DialogResult result = f.ShowDialog();
            if (result == DialogResult.OK) {
                Register reg = registerEditor.GetEmptyRegister();

                List<Военнослужащий> personList = new List<Военнослужащий>();
                if (forExam.Checked && personSelector.IsFilter()) {
                    Подразделение subunit = (Подразделение) personSelector.personFilter.subunitSelector.SelectedItem;
                    Querying.GetSubunitCommander(et, subunit.Код).ForEach(commander => {
                        personList.Add(commander);
                    });
                    Querying.GetPostForSubunit(et, subunit.Код, "ЗКВ").ForEach(zkv => {
                        personList.Add(zkv);
                    });
                    Querying.GetPostForSubunit(et, subunit.Код, "КО").ForEach(ko => {
                        personList.Add(ko);
                    });
                }
                personList.AddRange(personSelector.GetPersonList());

                reg.records = personList.Select(p => new RegisterRecord { marks = new List<Оценка>(), soldierId = p.Код, soldier = p }).ToList();
                return new Some<Register>(reg);
            } else {
                return new None<Register>();
            }
        }
    }
}
