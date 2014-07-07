﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.gui {
    public class PersonSelector : TabControl {
        private Entities et;

        public PersonSelector(Entities et) {
            this.et = et;
            this.InitializeComponent();
        }

        public PersonFilter personFilter;
        public PredefinedPersonLists predefinedPersonLists;
        private TabPage personFilterPage;
        private TabPage predefinedListsPage;

        private void InitializeComponent() {
            this.SuspendLayout();

            this.SelectedIndex = 0;

            personFilter = new PersonFilter(et);

            personFilterPage = new TabPage("Фильтр");
            personFilterPage.Location = new Point(4, 22);
            personFilterPage.UseVisualStyleBackColor = true;
            personFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            personFilterPage.Controls.Add(personFilter);
            this.Controls.Add(personFilterPage);

            this.Size = new Size(8 + personFilter.PreferredSize.Width, 25 + personFilter.PreferredSize.Height);

            predefinedListsPage = new TabPage("Списки");
            predefinedListsPage.Location = new Point(4, 22);
            predefinedListsPage.UseVisualStyleBackColor = true;
            predefinedPersonLists = new PredefinedPersonLists(et);
            predefinedPersonLists.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            predefinedPersonLists.Size = new Size(personFilter.PreferredSize.Width - 47, personFilter.PreferredSize.Height - 37);
            predefinedListsPage.Controls.Add(predefinedPersonLists);
            this.Controls.Add(predefinedListsPage);

            this.ResumeLayout(false);
        }

        public List<Военнослужащий> GetPersonList() {
            if (this.SelectedTab == personFilterPage) {
                return personFilter.GetPersonQuery().ToList();
            } else if (this.SelectedTab == predefinedListsPage) {
                return predefinedPersonLists.GetPersonList();
            } else {
                throw new Exception("no selector tab is selected");
            }
        }

        public IQueryable<Военнослужащий> GetPersonQuery() {
            if (this.SelectedTab == personFilterPage) {
                return personFilter.GetPersonQuery();
            } else if (this.SelectedTab == predefinedListsPage) {
                return predefinedPersonLists.GetPersonQuery();
            } else {
                throw new Exception("no selector tab is selected");
            }
        }

        public IQueryable<Оценка> GetGradeQuery() {
            if (this.SelectedTab == personFilterPage) {
                return personFilter.GetGradeQuery();
            } else if (this.SelectedTab == predefinedListsPage) {
                return predefinedPersonLists.GetGradeQuery();
            } else {
                throw new Exception("no selector tab is selected");
            }
        }

        public bool IsFilter() {
            return this.SelectedTab == personFilterPage;
        }

        public bool IsPredefinedList() {
            return this.SelectedTab == predefinedListsPage;
        }
    }
}
