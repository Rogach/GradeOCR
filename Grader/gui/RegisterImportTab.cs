using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Grader.model;

namespace Grader.gui {
    class RegisterImportTab : TabPage {
        private DataAccess dataAccess;

        private static string TAB_NAME = "Внесение ведомостей";

        public RegisterImportTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
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
                return changesPending;
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
            newRegisterButton.Size = new Size(150, 25);
            newRegisterButton.Click += new EventHandler(delegate {
                if (CheckForUnsavedChanges()) {
                    DeselectRegisterInList();
                    changesPending = true;
                    registerEditor.SetRegister(registerEditor.GetEmptyRegister());
                    SetRegisterPanelEnabled(true);
                }
            });
            this.Controls.Add(newRegisterButton);

            registerList = new ListView();
            registerList.MultiSelect = false;
            registerList.FullRowSelect = true;
            registerList.Location = new Point(3, 30);
            registerList.Size = new Size(150, 770);
            registerList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            registerList.DoubleClick += new EventHandler(delegate {
                if (registerList.SelectedIndices.Count > 0) {
                    ListViewItem item = registerList.Items[registerList.SelectedIndices[0]];
                    RegisterDesc rd = (RegisterDesc) item.Tag;
                    if (CheckForUnsavedChanges()) {
                        SelectRegisterInList(rd.id);
                        Register r = RegisterLoad.LoadRegister(rd.id, dataAccess.GetDataContext());
                        registerEditor.SetRegister(r);
                        SetRegisterPanelEnabled(true);
                    }
                }
            });
            this.Controls.Add(registerList);
            UpdateRegisterList();

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(160, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            registerEditor = new RegisterEditor(dataAccess);
            registerEditor.Location = new Point(170, 0);
            registerEditor.Size = new Size(1020, 770);
            registerEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            registerEditor.RegisterEdited += new EventHandler(delegate {
                changesPending = true;
            });
            this.Controls.Add(registerEditor);
            
            saveRegister = new Button();
            saveRegister.Text = "Сохранить";
            saveRegister.Location = new Point(170, 775);
            saveRegister.Size = new Size(100, 25);
            saveRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            saveRegister.Click += new EventHandler(delegate {
                Register register = registerEditor.GetRegister();
                RegisterLoad.SaveRegister(registerEditor.GetRegister(), dataAccess.GetDataContext());
                changesPending = false;
                UpdateRegisterList();
                SelectRegisterInList(register.id);
            });
            this.Controls.Add(saveRegister);

            cancelRegister = new Button();
            cancelRegister.Text = "Отменить";
            cancelRegister.Location = new Point(280, 775);
            cancelRegister.Size = new Size(100, 25);
            cancelRegister.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cancelRegister.Click += new EventHandler(delegate {
                registerEditor.SetRegister(registerEditor.GetEmptyRegister());
                SetRegisterPanelEnabled(false);
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
                    RegisterLoad.SaveRegister(registerEditor.GetRegister(), dataAccess.GetDataContext());
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
            dataAccess.GetDataContext().GetTable<Ведомость>()
                .OrderByDescending(r => r.ДатаВнесения)
                .Select(r => new RegisterDesc(r.Название, r.Код, r.ДатаВнесения))
                .ToList().ForEach(rd => {
                    ListViewItem item = new ListViewItem(rd.ToString());
                    item.Tag = rd;
                    registerList.Items.Add(item);
                });
        }

        public void DeselectRegisterInList() {
            registerList.SelectedIndices.Clear();
            foreach (ListViewItem item in registerList.Items) {
                item.BackColor = Color.White;
            }
        }

        public void SelectRegisterInList(int registerId) {
            foreach (ListViewItem item in registerList.Items) {
                if (((RegisterDesc) item.Tag).id == registerId) {
                    item.BackColor = Color.LightGray;
                } else {
                    item.BackColor = Color.White;
                }
            }
        }

        public void SetRegisterPanelEnabled(bool enabled) {
            registerEditor.Enabled = enabled;
            saveRegister.Enabled = enabled;
            cancelRegister.Enabled = enabled;
        }

        class RegisterDesc {
            public string name;
            public DateTime fillDate;
            public int id;
            public RegisterDesc(string name, int id, DateTime fillDate) {
                this.name = name;
                this.id = id;
                this.fillDate = fillDate;
            }
            public override string ToString() {
                return String.Format("{0} ({1}, {2})", name, fillDate.ToString("dd.MM.yyyy"));
            }
        }
    }
}
