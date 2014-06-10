﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    public class MainForm : Form {

        private DataAccess dataAccess;

        public MainForm(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private TabControl tabs;

        private void InitializeComponent() {
            this.SuspendLayout();

            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Location = new Point(0, 0);
            menuStrip.Size = new Size(1200, 20);

            ToolStripMenuItem menu_file = new ToolStripMenuItem("Файл");

            ToolStripMenuItem menu_file_select_base = new ToolStripMenuItem("Выбрать базу...");
            menu_file_select_base.Click += new EventHandler(delegate {
                throw new NotImplementedException();
            });
            menu_file.DropDownItems.Add(menu_file_select_base);

            menuStrip.Items.Add(menu_file);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;


            tabs = new TabControl();
            tabs.SuspendLayout();
            tabs.Location = new Point(0, 24);
            tabs.SelectedIndex = 0;
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            tabs.Size = new System.Drawing.Size(1200, 776);

            AddTab(tabs, new RegisterGenerationTab(dataAccess));
            AddTab(tabs, new TabPage("Внесение ведомостей"));
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
        }

        private void AddTab(TabControl tabs, TabPage tab) {
            tab.Location = new Point(4, 22);
            tab.UseVisualStyleBackColor = true;
            tabs.Controls.Add(tab);
        }
    }
}
