using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.Linq;

namespace Grader.gui {
    public class GradeAnalysisTab : TabPage {
        private DataAccess dataAccess;

        public GradeAnalysisTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private PersonFilter personFilter;
        private TextBox tags;
        private ComboBox minRank;
        private ComboBox maxRank;

        private void InitializeComponent() {
            DataContext dc = dataAccess.GetDataContext();
            this.Text = "Анализ оценок";
            this.Size = new Size(1200, 800);

            this.SuspendLayout();

            FormLayout layout = new FormLayout(this, maxLabelWidth: 87);

            dateFrom = layout.Add("от", new DateTimePicker());
            dateFrom.Format = DateTimePickerFormat.Long;
            dateFrom.ShowCheckBox = true;
            dateFrom.Checked = false;

            dateTo = layout.Add("до", new DateTimePicker());
            dateTo.Format = DateTimePickerFormat.Long;
            dateTo.ShowCheckBox = true;
            dateTo.Checked = false;

            layout.PerformLayout();

            personFilter = new PersonFilter(dataAccess);
            personFilter.Location = new Point(3, layout.GetY() + 5);
            this.Controls.Add(personFilter);

            FormLayout layout2 = new FormLayout(this, maxLabelWidth: 87, y: layout.GetY() + 5 + personFilter.Height + 10);

            tags = layout2.Add("Тэги", new TextBox());

            List<string> ranks = dc.GetTable<Звание>().OrderByDescending(r => r.order).Select(r => r.Название).ToList();

            maxRank = layout2.Add("Звание <=", new ComboBox());
            maxRank.Items.AddRange(ranks.ToArray());
            maxRank.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            maxRank.AutoCompleteSource = AutoCompleteSource.ListItems;
            maxRank.SelectedItem = "полковник";

            minRank = layout2.Add("Звание >=", new ComboBox());
            minRank.Items.AddRange(ranks.ToArray());
            minRank.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            minRank.AutoCompleteSource = AutoCompleteSource.ListItems;
            minRank.SelectedItem = "рядовой";

            layout2.AddSpacer(15);

            Separator sep = new Separator(Separator.Direction.Vertical);
            sep.Location = new Point(250, 0);
            sep.Size = new Size(4, 800);
            sep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(sep);

            layout2.PerformLayout();

            this.ResumeLayout(false);
        }
    }
}
