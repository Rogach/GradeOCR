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
                    DeselectRegisterInList();
                    changesPending = true;
                    registerEditor.SetRegister(registerEditor.GetEmptyRegister());
                    SetRegisterPanelEnabled(true);
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
                        Register r = RegisterLoad.LoadRegister(rd.id, dataAccess.GetDataContext());
                        registerEditor.SetRegister(r);
                        changesPending = false;
                        SetRegisterPanelEnabled(true);
                    }
                }
            });
            this.Controls.Add(registerList);
            UpdateRegisterList();

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(210, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            registerEditor = new RegisterEditor(dataAccess);
            registerEditor.Location = new Point(220, 0);
            registerEditor.Size = new Size(970, 770);
            registerEditor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            registerEditor.RegisterEdited += new EventHandler(delegate {
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
                Register register = registerEditor.GetRegister();
                RegisterLoad.SaveRegister(registerEditor.GetRegister(), dataAccess.GetDataContext());
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
    }
}
