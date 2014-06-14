using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Grader.enums;
using System.Data.Linq;
using Grader.util;
using Grader.registers;
using LibUtil;
using LibUtil.templates;
using LibUtil.wrapper.excel;

namespace Grader.gui {
    class RegisterGenerationTab : TabPage {
        private DataAccess dataAccess;

        public RegisterGenerationTab(DataAccess dataAccess) {
            this.dataAccess = dataAccess;
            this.InitializeComponent();
        }

        private RegisterSpec[] registerSpecs = {
            new СводнаяВедомость(),
            new СводнаяНевыносимыеПредметыВедомость(),
            new СверочнаяВедомость(),
            new ВедомостьОГН(),
            new GenericRegister("ОВУ"),
            new ВедомостьСП(),
            new ВедомостьСТР(),
            new ВедомостьТП(),
            new ВедомостьФП(),
            new ВедомостьРХБЗ(),
            new GenericRegister("АВТ"),
            new GenericRegister("ОБВС"),
            new GenericRegister("ОЗГТ"),
            new GenericRegister("ЭО"),
            new GenericRegister("ОГП"),
            new GenericRegister("ИНЖ"),
            new GenericRegister("ВМП"),
            new GenericRegister("ТСП"),
            new GenericRegister("ТОП"),
            new GenericRegister("МП")
        };

        private PersonSelector personSelector;
        private RadioButton groupingNone;
        private RadioButton groupingByPlatoon;
        private RadioButton groupingByVus;
        private DateTimePicker registerDate;
        private CheckBox onlyKMN;
        private CheckBox strikeKMN;
        private ComboBox registerSubjectSelect;
        private ComboBox registerTypeSelect;
        private Button generateRegisterButton;

        private void InitializeComponent() {
            this.Text = "Печать ведомостей";

            personSelector = new PersonSelector(dataAccess);
            personSelector.Location = new Point(3, 3);
            this.Controls.Add(personSelector);

            DataContext dc = dataAccess.GetDataContext();

            FormLayout layout = new FormLayout(this, maxLabelWidth: 90, y: 6 + personSelector.PreferredSize.Height);

            groupingNone = new RadioButton { Text = "нет", Checked = true };
            groupingByPlatoon = new RadioButton { Text = "повзводно" };
            groupingByVus = new RadioButton { Text = "повусно" };
            layout.AddControlGroup("Сгруппировать", new List<Control> { groupingNone , groupingByPlatoon, groupingByVus });

            layout.AddSpacer(8);

            onlyKMN = layout.Add("Только КМН?", new CheckBox());

            strikeKMN = layout.Add("Пометить КМН", new CheckBox());
            GuiUtils.SetToolTip(layout, strikeKMN, "\"Представлен на экзамен в составе уч. гр. кандидатов на должности МК\"");

            layout.AddSpacer(10);

            registerDate = layout.Add("Дата", new DateTimePicker());
            registerDate.Value = DateTime.Now;

            registerSubjectSelect = layout.Add("ведомость:", new ComboBox());
            registerSubjectSelect.Items.AddRange(registerSpecs);
            registerSubjectSelect.SelectedIndex = 0;
            registerSubjectSelect.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            registerSubjectSelect.AutoCompleteSource = AutoCompleteSource.ListItems;

            registerTypeSelect = layout.Add("Тип ведомости", new ComboBox());
            registerTypeSelect.PopulateComboBox(typeof(RegisterType));
            registerTypeSelect.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            registerTypeSelect.AutoCompleteSource = AutoCompleteSource.ListItems;

            generateRegisterButton = layout.AddFullRow(new Button());
            generateRegisterButton.Text = "создать ведомость";
            generateRegisterButton.Click += new EventHandler(delegate {
                GenerateRegister();
            });

            layout.PerformLayout();
        }

        private class SoldierGrouping {
            public Func<ВоеннослужащийПоПодразделениям, int> keySelector { get; set; }
            public Func<int, string> registerName { get; set; }
            public Func<List<ВоеннослужащийПоПодразделениям>, Подразделение> subunit { get; set; }
        }

        private SoldierGrouping GetGrouping(DataContext dc) {
            if (groupingNone.Checked) {
                return new SoldierGrouping {
                    keySelector = v => 0,
                    registerName = _ => "ведомость",
                    subunit = _ => {
                        if (personSelector.IsFilter()) {
                            return (Подразделение) personSelector.personFilter.subunitSelector.SelectedItem;
                        } else {
                            return null;
                        }
                    }
                };
            } else if (groupingByPlatoon.Checked) {
                return new SoldierGrouping {
                    keySelector = v => v.КодПодразделения,
                    registerName = subunitId => Querying.GetSubunitName(dc, subunitId),
                    subunit = soldiers => dc.GetTable<Подразделение>().Where(s => s.Код == soldiers.First().КодПодразделения).ToListTimed().First()
                };
            } else if (groupingByVus.Checked) {
                return new SoldierGrouping {
                    keySelector = v => v.ВУС,
                    registerName = vus => vus.ToString(),
                    subunit = _ => {
                        if (personSelector.IsFilter()) {
                            return (Подразделение) personSelector.personFilter.subunitSelector.SelectedItem;
                        } else {
                            return null;
                        }
                    }
                };
            } else {
                throw new Exception("No valid grouping selected");
            }
        }

        private void GenerateRegister() {
            DataContext dc = dataAccess.GetDataContext();
            RegisterSpec spec = (RegisterSpec) registerSubjectSelect.SelectedItem;
            RegisterSettings settings = new RegisterSettings {
                registerType = registerTypeSelect.GetComboBoxEnumValue<RegisterType>(),
                onlyKMN = onlyKMN.Checked,
                strikeKMN = strikeKMN.Checked,
                registerDate = registerDate.Value
            };
            List<ВоеннослужащийПоПодразделениям> soldiers =
                personSelector.GetPersonList();
            if (onlyKMN.Checked) {
                soldiers = soldiers.Where(v => v.КМН == 1).ToList();
            }
            if (soldiers.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Нет соответствующих фильтру военнослужащих!");
            }
            SoldierGrouping grouping = GetGrouping(dc);

            var rwb = ExcelTemplates.LoadExcelTemplate(dataAccess.GetTemplateLocation(spec.templateName));
            ExcelWorksheet templateSheet = rwb.Worksheets.First();
            ProgressDialogs.ForEach(soldiers.GroupBy(grouping.keySelector), group => {
                templateSheet.Copy(After: rwb.Worksheets.Last());
                ExcelWorksheet rsh = rwb.Worksheets.Last();
                rsh.Name = grouping.registerName(group.Key);
                settings.soldiers = group.ToList();
                settings.subunit = grouping.subunit(settings.soldiers);
                if (personSelector.IsPredefinedList()) {
                    settings.subunitName = personSelector.predefinedPersonLists.GetRegisterName();
                }
                spec.Format(dc, rsh, settings);
            });

            templateSheet.Delete();
            rwb.Saved = true;
            rwb.Application.Visible = true;
            rwb.Activate();
        }

    }
}
