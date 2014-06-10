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

        private ComboBox subunitSelector;
        private CheckBox forAllPlatoons;
        private ComboBox vusSelector;
        private ComboBox studyType;
        private DateTimePicker registerDate;
        private CheckBox selectCadets;
        private CheckBox selectPermanent;
        private CheckBox selectContract;
        private CheckBox onlyKMN;
        private CheckBox strikeKMN;
        private ComboBox registerSubjectSelect;
        private ComboBox registerTypeSelect;
        private Button generateRegisterButton;

        private void InitializeComponent() {
            this.Text = "Печать ведомостей";

            DataContext dc = dataAccess.GetDataContext();

            FormLayout layout = new FormLayout(this);

            subunitSelector = layout.Add("Подразделение", new ComboBox());
            subunitSelector.Items.AddRange(dc.GetTable<Подразделение>().ToListTimed().ToArray());
            subunitSelector.SelectedIndex = 0;

            forAllPlatoons = layout.Add("", new CheckBox());
            forAllPlatoons.Text = "Все взвода?";
            forAllPlatoons.Checked = true;
            forAllPlatoons.CheckedChanged += new EventHandler(delegate {
                subunitSelector.Enabled = !forAllPlatoons.Checked;
            });
            subunitSelector.Enabled = !forAllPlatoons.Checked;

            studyType = layout.Add("Тип обучения", new ComboBox());
            studyType.PopulateComboBox(typeof(StudyType));

            vusSelector = layout.Add("ВУС", new ComboBox());
            string[] possibleVuses =
                dc.GetTable<Военнослужащий>().Select(v => v.ВУС).Distinct().ToListTimed()
                .Where(v => v != 0).Select(v => v.ToString()).ToArray();
            vusSelector.Items.AddRange(possibleVuses);
            
            registerDate = layout.Add("Дата", new DateTimePicker());
            registerDate.Value = DateTime.Now;

            layout.AddSpacer(13);

            selectCadets = layout.Add("", new CheckBox(), thin: true);
            selectCadets.Text = "Курсанты?";
            selectCadets.Checked = true;

            selectPermanent = layout.Add("", new CheckBox(), thin: true);
            selectPermanent.Text = "Постоянные срочники?";

            selectContract = layout.Add("", new CheckBox(), thin: false);
            selectContract.Text = "Контрактники?";

            layout.AddSpacer(5);

            onlyKMN = layout.Add("Только КМН?", new CheckBox());

            strikeKMN = layout.Add("Пометить КМН", new CheckBox());

            layout.AddSpacer(10);

            registerSubjectSelect = layout.Add("ведомость:", new ComboBox());
            registerSubjectSelect.Items.AddRange(registerSpecs);
            registerSubjectSelect.SelectedIndex = 0;

            registerTypeSelect = layout.Add("Тип ведомости", new ComboBox());
            registerTypeSelect.PopulateComboBox(typeof(RegisterType));

            generateRegisterButton = layout.AddFullRow(new Button());
            generateRegisterButton.Text = "создать ведомость";
            generateRegisterButton.Click += new EventHandler(delegate {
                GenerateRegister();
            });

            layout.PerformLayout();
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
            Option<int> vus;
            if (vusSelector.SelectedItem == null) {
                vus = new None<int>();
            } else {
                vus = new Some<int>(Int32.Parse((string) vusSelector.SelectedItem));
            }
            

            if (forAllPlatoons.Checked) {
                MakeRegistersForPlatoons(dc, spec.templateName, (sh, subunitId, soldiers) => {
                    Подразделение subunit = (from s in dc.GetTable<Подразделение>() where s.Код == subunitId select s).ToListTimed().First();
                    settings.subunit = subunit;
                    settings.soldiers = soldiers.Where(s => vus.Map(v => v == s.ВУС).GetOrElse(true)).ToList();
                    spec.Format(dc, sh, settings);
                });
            } else {
                settings.subunit = (Подразделение) subunitSelector.SelectedItem;
                var soldiers =
                    Querying.GetSubunitSoldiers(dc, settings.subunit.Код, 
                        Querying.GetSoldierQueryFilterByType(dc, 
                            selectCadets: selectCadets.Checked, 
                            selectPermanent: selectPermanent.Checked,
                            selectContract: selectContract.Checked,
                            studyType: studyType.GetComboBoxEnumValue<StudyType>()))
                    .Where(s => vus.Map(v => v == s.ВУС).GetOrElse(true)).ToList();
                if (onlyKMN.Checked) {
                    soldiers = soldiers.Where(s => s.КМН == 1).ToList();
                }
                if (soldiers.Count != 0) {
                    MakeRegister(spec.templateName, sh => {
                        settings.soldiers = soldiers;
                        spec.Format(dc, sh, settings);
                    });
                } else {
                    System.Windows.Forms.MessageBox.Show("Взвод пуст!");
                }
            }
        }

        private void MakeRegister(string templateName, Action<ExcelWorksheet> format) {
            var rwb = ExcelTemplates.LoadExcelTemplate(Program.GetTemplateLocation(templateName));
            ExcelWorksheet rsh = rwb.Worksheets.First();
            format(rsh);
            rwb.Saved = true;
            rwb.Application.Visible = true;
            rsh.Activate();
        }

        private void MakeRegistersForPlatoons(DataContext dc, string templateName,
                Action<ExcelWorksheet, int, List<ВоеннослужащийПоПодразделениям>> format) {
            var rwb = ExcelTemplates.LoadExcelTemplate(Program.GetTemplateLocation(templateName));
            ExcelWorksheet templateSheet = rwb.Worksheets.First();
            var platoonIds =
                Querying.GetSubunitsByType(dc, "взвод").Select(s => s.Код);
            ProgressDialogs.ForEach(platoonIds, subunitId => {
                var soldiers = Querying.GetSubunitSoldiers(dc, subunitId, 
                    Querying.GetSoldierQueryFilterByType(dc, 
                        selectCadets: selectCadets.Checked,
                        selectPermanent: selectPermanent.Checked,
                        selectContract: selectContract.Checked,
                        studyType: studyType.GetComboBoxEnumValue<StudyType>()));
                if (onlyKMN.Checked) {
                    soldiers = soldiers.Where(s => s.КМН == 1).ToList();
                }

                if (soldiers.Count != 0) {
                    templateSheet.Copy(After: rwb.Worksheets.Last());
                    ExcelWorksheet rsh = rwb.Worksheets.Last();
                    rsh.Name = Querying.GetSubunitName(dc, subunitId);
                    format(rsh, subunitId, soldiers);
                }
            });
            templateSheet.Delete();
            rwb.Saved = true;
            rwb.Application.Visible = true;
            rwb.Activate();
        }

    }
}
