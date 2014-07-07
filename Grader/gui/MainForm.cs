using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.EntityClient;

namespace Grader.gui {
    public class MainForm : Form {

        private ApplicationContext context;
        private Entities et;
        private Settings settings;

        public MainForm(Settings settings, ApplicationContext context) {
            this.settings = settings;
            
            EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder();
            ecsb.Provider = "MySql.Data.MySqlClient";
            ecsb.ProviderConnectionString = settings.dbConnectionString.GetValue();
            ecsb.Metadata = @"res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl";
            this.et = new Entities(ecsb.ConnectionString);
            this.et.Connection.Open();
            this.et.initCache();
            this.context = context;
            this.InitializeComponent();
        }

        private TabControl tabs;

        private RegisterGenerationTab registerGenerationTab;
        private RegisterImportTab registerImportTab;
        private GradeViewTab gradeViewTab;
        private GradeAnalysisTab gradeAnalysisTab;

        private void InitializeComponent() {
            this.SuspendLayout();

            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Location = new Point(0, 0);
            menuStrip.Size = new Size(1200, 20);

            ToolStripMenuItem menu_file = new ToolStripMenuItem("Файл");

            ToolStripMenuItem menu_file_select_base = new ToolStripMenuItem("Выбрать базу");
            menu_file_select_base.Click += new EventHandler(delegate {
                if (CheckForUnsavedChanges()) {
                    if (settings.dbConnectionString.init()) {
                        context.MainForm = null;
                        this.Dispose();

                        MainForm newMainForm = new MainForm(settings, context);
                        context.MainForm = newMainForm;
                        newMainForm.Show();
                        settings.Save();
                    }
                }
            });
            menu_file.DropDownItems.Add(menu_file_select_base);

            ToolStripMenuItem menu_file_select_templates = new ToolStripMenuItem("Выбрать шаблоны");
            menu_file_select_templates.Click += new EventHandler(delegate {
                if (settings.templatesLocation.init()) {
                    settings.Save();
                }
            });
            menu_file.DropDownItems.Add(menu_file_select_templates);

            ToolStripMenuItem menu_file_reload_base = new ToolStripMenuItem("Перезагрузить базу");
            menu_file_reload_base.Click += new EventHandler(delegate {
                if (CheckForUnsavedChanges()) {
                    context.MainForm = null;
                    this.Dispose();

                    MainForm newMainForm = new MainForm(settings, context);
                    context.MainForm = newMainForm;
                    newMainForm.Show();
                }
            });
            menu_file.DropDownItems.Add(menu_file_reload_base);

            menu_file.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem menu_file_exit = new ToolStripMenuItem("Выход");
            menu_file_exit.Click += new EventHandler(delegate {
                if (CheckForUnsavedChanges()) {
                    Environment.Exit(0);
                }
            });
            menu_file.DropDownItems.Add(menu_file_exit);

            menuStrip.Items.Add(menu_file);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            tabs = new TabControl();
            tabs.SuspendLayout();
            tabs.Location = new Point(0, 24);
            tabs.SelectedIndex = 0;
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            tabs.Size = new System.Drawing.Size(1200, 776);

            registerGenerationTab = new RegisterGenerationTab(et, settings);
            AddTab(tabs, registerGenerationTab);
            registerImportTab = new RegisterImportTab(et);
            AddTab(tabs, registerImportTab);
            gradeViewTab = new GradeViewTab(et, settings);
            AddTab(tabs, gradeViewTab);
            gradeAnalysisTab = new GradeAnalysisTab(et, settings);
            AddTab(tabs, gradeAnalysisTab);

            gradeViewTab.ChangesSaved.AddEventListener(() => {
                registerImportTab.UpdateRegisterList();
            });

            tabs.ResumeLayout(false);

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
            this.MinimumSize = new Size(600, 400);
            this.Controls.Add(tabs);
            this.Text = "Grader";
            this.ResumeLayout(false);

            this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e) {
                if (!CheckForUnsavedChanges()) {
                    e.Cancel = true;
                }
            });
        }

        private void AddTab(TabControl tabs, TabPage tab) {
            tab.Location = new Point(4, 22);
            tab.UseVisualStyleBackColor = true;
            tabs.Controls.Add(tab);
        }

        public bool CheckForUnsavedChanges() {
            return registerImportTab.CheckForUnsavedChanges() && gradeViewTab.CheckForUnsavedChanges();
        }
    }
}
