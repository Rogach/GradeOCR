using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    public class MainForm : Form {

        private ApplicationContext context;
        private DataAccess dataAccess;
        private Settings settings;

        public MainForm(Settings settings, ApplicationContext context) {
            this.settings = settings;
            this.dataAccess = new DataAccess(settings.dbLocation);
            this.context = context;
            this.InitializeComponent();
        }

        private TabControl tabs;

        private RegisterGenerationTab registerGenerationTab;
        private RegisterImportTab registerImportTab;

        private void InitializeComponent() {
            this.SuspendLayout();

            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Location = new Point(0, 0);
            menuStrip.Size = new Size(1200, 20);

            ToolStripMenuItem menu_file = new ToolStripMenuItem("Файл");

            ToolStripMenuItem menu_file_select_base = new ToolStripMenuItem("Выбрать базу");
            menu_file_select_base.Click += new EventHandler(delegate {
                Settings.AskForDbLocation().ForEach(newDbLocation => {
                    context.MainForm = null;
                    this.Dispose();

                    settings.dbLocation = newDbLocation;
                    MainForm newMainForm = new MainForm(settings, context);
                    context.MainForm = newMainForm;
                    newMainForm.Show();
                });
            });
            menu_file.DropDownItems.Add(menu_file_select_base);

            ToolStripMenuItem menu_file_reload_base = new ToolStripMenuItem("Перезагрузить базу");
            menu_file_reload_base.Click += new EventHandler(delegate {
                context.MainForm = null;
                this.Dispose();

                MainForm newMainForm = new MainForm(settings, context);
                context.MainForm = newMainForm;
                newMainForm.Show();
            });
            menu_file.DropDownItems.Add(menu_file_reload_base);

            menu_file.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem menu_file_exit = new ToolStripMenuItem("Выход");
            menu_file_exit.Click += new EventHandler(delegate {
                if (registerImportTab.CheckForUnsavedChanges()) {
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

            registerGenerationTab = new RegisterGenerationTab(dataAccess);
            AddTab(tabs, registerGenerationTab);
            registerImportTab = new RegisterImportTab(dataAccess);
            AddTab(tabs, registerImportTab);
            AddTab(tabs, new TabPage("Просмотр оценок"));
            AddTab(tabs, new TabPage("Анализ оценок"));

            tabs.ResumeLayout(false);

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
            this.MinimumSize = new Size(600, 400);
            this.Controls.Add(tabs);
            this.Text = "Grader";
            this.ResumeLayout(false);

            this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e) {
                if (!registerImportTab.CheckForUnsavedChanges()) {
                    e.Cancel = true;
                }
            });
        }

        private void AddTab(TabControl tabs, TabPage tab) {
            tab.Location = new Point(4, 22);
            tab.UseVisualStyleBackColor = true;
            tabs.Controls.Add(tab);
        }
    }
}
