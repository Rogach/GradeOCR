using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.EntityClient;
using Grader.model;

namespace Grader.gui {
    public class MainForm : Form {

        private ApplicationContext context;
        private Entities et;
        private Settings settings;

        public MainForm(Settings settings, ApplicationContext context) {
            this.settings = settings;
            this.et = Entities.CreateEntities(settings);
            Grader.Program.ReportEvent("Loaded entities");
            this.context = context;
            this.InitializeComponent();
        }

        private TabControl tabs;

        private RegisterGenerationTab registerGenerationTab;
        private RegisterImportTab registerImportTab;
        private GradeViewTab gradeViewTab;
        private GradeAnalysisTab gradeAnalysisTab;
        private TableEditorTab tableEditorTab;

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

            ToolStripMenuItem menu_import = new ToolStripMenuItem("Импорт");

            ToolStripMenuItem menu_import_cadets = new ToolStripMenuItem("Импортировать курсантов");
            menu_import_cadets.Click += new EventHandler(delegate {
                Import.ImportCadets(et);
            });
            menu_import.DropDownItems.Add(menu_import_cadets);

            ToolStripMenuItem menu_import_compare_cadets = new ToolStripMenuItem("Сравнить курсантов");
            menu_import_compare_cadets.Click += new EventHandler(delegate {
                Difference.CalculateCadetDifference(et);
            });
            menu_import.DropDownItems.Add(menu_import_compare_cadets);

            ToolStripMenuItem menu_import_permanents = new ToolStripMenuItem("Импортировать пост. срочников");
            menu_import_permanents.Click += new EventHandler(delegate {
                Import.ImportPermanents(et);
            });
            menu_import.DropDownItems.Add(menu_import_permanents);

            menuStrip.Items.Add(menu_import);

            ToolStripMenuItem version_item = new ToolStripMenuItem(Program.GetVersion());
            version_item.Enabled = false;
            menuStrip.Items.Add(version_item);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            tabs = new TabControl();
            tabs.SuspendLayout();
            tabs.Location = new Point(0, 24);
            tabs.SelectedIndex = 0;
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            tabs.Size = new System.Drawing.Size(1200, 776);

            registerGenerationTab = new RegisterGenerationTab(et, settings);
            AddTab(registerGenerationTab);
            registerImportTab = new RegisterImportTab(et);
            AddTab(registerImportTab);
            gradeViewTab = new GradeViewTab(et, settings);
            AddTab(gradeViewTab);
            gradeAnalysisTab = new GradeAnalysisTab(et, settings);
            AddTab(gradeAnalysisTab);
            tableEditorTab = new TableEditorTab(et);
            AddTab(tableEditorTab);

            gradeViewTab.ChangesSaved.AddEventListener(() => {
                registerImportTab.UpdateRegisterList();
            });

            registerImportTab.RegisterSaved.AddEventListener(() => {
                registerImportTab.RefreshAutocomplete();
                gradeViewTab.RefreshAutocomplete();
                gradeAnalysisTab.RefreshAutocomplete();
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

        private void AddTab(TabPage tab) {
            tab.Location = new Point(4, 22);
            tab.UseVisualStyleBackColor = true;
            tabs.Controls.Add(tab);
        }

        public bool CheckForUnsavedChanges() {
            return registerImportTab.CheckForUnsavedChanges() && gradeViewTab.CheckForUnsavedChanges();
        }
    }
}
