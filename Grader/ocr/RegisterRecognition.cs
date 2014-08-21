using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using LibUtil;
using System.Threading;
using ARCode;
using System.Windows.Forms;
using TableOCR;
using Grader.model;
using Grader.gui;
using GradeOCR;

namespace Grader.ocr {
    public static class RegisterRecognition {
        public static readonly int minFinderCircleRadius = 50;
        public static readonly int maxFinderCircleRadius = 70;
        public static GradeDigestSet digestSet = GradeDigestSet.ReadDefault();

        public static void RecognizeRegisterImage(Entities et, Bitmap sourceImage) {
            var formOpts = new RegisterRecognitionForm.Options {
                sourceImage = sourceImage,
                minFinderCircleRadius = minFinderCircleRadius,
                maxFinderCircleRadius = maxFinderCircleRadius
            };
            bool cancel = false;

            ProgressDialogs.WithProgress(3, ph => {
                Option<Tuple<ВедомостьДляРаспознавания, DataMatrixExtraction>> registerInfoOpt = 
                    ARCodeUtil.ExtractCodeExt(sourceImage, minFinderCircleRadius, maxFinderCircleRadius)
                    .FlatMap(t => {
                        int registerCode = (int) t.Item1;
                        // extract information from database
                        Option<ВедомостьДляРаспознавания> registerInfoRow =
                            et.ВедомостьДляРаспознавания.Where(v => v.Код == registerCode).ToList().HeadOption();
                        return registerInfoRow.Map(ri => new Tuple<ВедомостьДляРаспознавания, DataMatrixExtraction>(ri, t.Item2));
                    });
                ph.Increment();

                if (registerInfoOpt.NonEmpty() && registerInfoOpt.Get().Item1.ДатаВнесения.HasValue) {
                    MessageBox.Show("Данная ведомость уже была внесена!", "Ошибка в распознавании", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                } else if (registerInfoOpt.NonEmpty()) {
                    ВедомостьДляРаспознавания registerInfo = registerInfoOpt.Get().Item1;
                    DataMatrixExtraction dme = registerInfoOpt.Get().Item2;
                    formOpts.registerInfoOpt = new Some<ВедомостьДляРаспознавания>(registerInfo);
                    formOpts.dmeOpt = new Some<DataMatrixExtraction>(dme);

                    List<int> subjectIds = registerInfo.СписокВоеннослужащих.Split(',').Select(sid => int.Parse(sid)).ToList();
                    List<RegisterRecord> records = subjectIds.Select(sid =>
                        new RegisterRecord { 
                            soldierId = sid, 
                            soldier = et.Военнослужащий.Where(v => v.Код == sid).ToList().First(),
                            marks = new List<Оценка>()
                        }).ToList();

                    Register reg = new Register {
                            id = -1,
                            fillDate = DateTime.Now,
                            importDate = null,
                            editDate = null,
                            name = registerInfo.ИмяВедомости,
                            virt = false,
                            enabled = true,
                            tags = RegisterEditor.SplitTags(registerInfo.Теги),
                            subjectIds = new List<int>(),
                            records = records
                        };
                    formOpts.registerOpt = new Some<Register>(reg);

                    Option<Table> tableOpt = TableOCR.Program.RecognizeTable(sourceImage);

                    ph.Increment();
                    if (tableOpt.NonEmpty()) {
                        formOpts.recognizedTable = tableOpt;
                    } else {
                        // failed to recognize the table, no grades will be filled into the register
                        DialogResult dres = MessageBox.Show(
                            "Не удалось распознать таблицу, оценки не будут распознаны",
                            "Ошибка в распознавании",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Error);
                    
                        if (dres != DialogResult.OK) {
                            cancel = true;
                        }
                    }
                } else {
                    // no code was extracted from image, create empty register
                    DialogResult dres = MessageBox.Show(
                        "Не удалось распознать код ведомости",
                        "Ошибка в распознавании", 
                        MessageBoxButtons.OKCancel, 
                        MessageBoxIcon.Error);

                    if (dres != DialogResult.OK) {
                        cancel = true;
                    }
                }
            });

            if (!cancel)
                new RegisterRecognitionForm(et, digestSet, formOpts).ShowDialog();
        }
    }
}
