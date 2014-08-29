﻿using System;
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
        private Entities et;
        private Settings settings;

        public RegisterGenerationTab(Entities et, Settings settings) {
            this.et = et;
            this.settings = settings;
            this.InitializeComponent();
        }

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
        private CheckBox forOCR;
        private TextBox registerNamePrefix;
        private TextBox registerTags;

        private void InitializeComponent() {
            this.Text = "Печать ведомостей";

            personSelector = new PersonSelector(et);
            personSelector.Location = new Point(3, 3);
            this.Controls.Add(personSelector);

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
            registerSubjectSelect.Items.AddRange(RegisterSpec.registerSpecs);
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
                try {
                    GenerateRegister();
                } catch (WorkAbortedException) {
                    Console.WriteLine("task aborted");
                }
            });

            layout.PerformLayout();

            FormLayout ocrLayout = new FormLayout(this, maxLabelWidth: 90, x: 270, y: 6);

            Action updateOcrFields = () => {
                registerNamePrefix.Enabled = forOCR.Checked;
                registerTags.Enabled = forOCR.Checked;
            };
            forOCR = ocrLayout.Add("Для распознавания?", new CheckBox());
            forOCR.CheckedChanged += new EventHandler(delegate {
                updateOcrFields();
            });

            registerNamePrefix = ocrLayout.Add("Префикс имени ведомости", new TextBox());

            registerTags = ocrLayout.Add("Тэг ведомости", new TextBox());

            updateOcrFields();

            ocrLayout.PerformLayout();
        }

        private class SoldierGrouping {
            public Func<Военнослужащий, int> keySelector { get; set; }
            public Func<int, string> registerName { get; set; }
            public Func<List<Военнослужащий>, Подразделение> subunit { get; set; }
        }

        private SoldierGrouping GetGrouping(Entities et) {
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
                    registerName = subunitId => Querying.GetSubunitName(et, subunitId),
                    subunit = soldiers => {
                        int subunitId = soldiers.First().КодПодразделения;
                        return et.Подразделение.Where(s => s.Код == subunitId).First();
                    }
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

        private ExcelApplication excelAppInstance = null;
        private ExcelApplication GetExcel() {
            if (excelAppInstance == null) {
                excelAppInstance = new ExcelApplication();
            }
            return excelAppInstance;
        }

        private void GenerateRegister() {
            RegisterSpec spec = (RegisterSpec) registerSubjectSelect.SelectedItem;
            RegisterSettings settings = new RegisterSettings {
                registerType = registerTypeSelect.GetComboBoxEnumValue<RegisterType>(),
                onlyKMN = onlyKMN.Checked,
                strikeKMN = strikeKMN.Checked,
                registerDate = registerDate.Value,
                forOCR = forOCR.Checked,
                registerNamePrefix = registerNamePrefix.Text,
                registerTags = registerTags.Text
            };
            List<Военнослужащий> soldiers =
                personSelector.GetPersonList();
            if (onlyKMN.Checked) {
                soldiers = soldiers.Where(v => v.КМН).ToList();
            }
            if (soldiers.Count == 0) {
                System.Windows.Forms.MessageBox.Show("Нет соответствующих фильтру военнослужащих!");
            }
            SoldierGrouping grouping = GetGrouping(et);

            var rwb = ExcelTemplates.LoadExcelTemplate(GetExcel(), this.settings.GetTemplateLocation(spec.templateName));
            ExcelWorksheet templateSheet = rwb.Worksheets.First();
            ProgressDialogs.ForEach(soldiers.GroupBy(grouping.keySelector).OrderBy(group => group.Key), group => {
                templateSheet.Copy(After: rwb.Worksheets.Last());
                ExcelWorksheet rsh = rwb.Worksheets.Last();
                rsh.Name = grouping.registerName(group.Key);
                settings.soldiers = group.ToList();
                settings.subunit = grouping.subunit(settings.soldiers);
                if (personSelector.IsPredefinedList()) {
                    settings.subunitName = personSelector.predefinedPersonLists.GetRegisterName();
                }
                spec.Format(et, rsh, settings);
            });

            if (soldiers.Count > 0) {
                templateSheet.Delete();
                rwb.Saved = true;
                rwb.Application.Visible = true;
                rwb.Activate();
            }
        }

    }
}
