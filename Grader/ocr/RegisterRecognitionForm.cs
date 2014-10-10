﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OCRUtil;
using LibUtil;
using Grader.gui;
using Grader.model;
using TableOCR;
using ARCode;
using GradeOCR;

namespace Grader.ocr {
    public partial class RegisterRecognitionForm : Form {
        public class Options {
            public Bitmap sourceImage;
            public Bitmap debugImage;
            public Option<ВедомостьДляРаспознавания> registerInfoOpt = new None<ВедомостьДляРаспознавания>();
            public Option<Register> registerOpt = new None<Register>();
            public Option<Table> recognizedTable = new None<Table>();
            public Option<DataMatrixExtraction> dmeOpt = new None<DataMatrixExtraction>();
            public int minFinderCircleRadius;
            public int maxFinderCircleRadius;
            public Action<Register> onSave;
        }

        private PictureView ocrImagePV;
        private RegisterEditor registerEditor;
        private Options formOpts;

        public RegisterRecognitionForm(Entities et, Options formOpts) {
            this.formOpts = formOpts;
            InitializeComponent();
            
            ocrImagePV = PictureView.InsertIntoPanel(ocrImagePanel);

            registerEditor = new RegisterEditor(et);
            registerEditor.InsertIntoPanel(registerEditorPanel);
            if (formOpts.registerOpt.NonEmpty()) {
                registerEditor.SetRegister(formOpts.registerOpt.Get());
            } else {
                registerEditor.SetRegister(registerEditor.GetEmptyRegister());
            }

            this.Shown += new EventHandler(delegate { 
                ocrImagePV.Image = formOpts.debugImage;
            });

            formOpts.recognizedTable.ForEach(table => {
                Bitmap bwImage = ImageUtil.ToBlackAndWhite(formOpts.sourceImage);

                this.ocrImagePV.AddDoubleClickListener((pt, e) => {
                    Option<Point> cellOpt = table.GetCellAtPoint(pt.X, pt.Y);
                    cellOpt.ForEach(cell => {
                        table.GetCellImage(bwImage, cell.X, cell.Y).ForEach(cellImage => {
                            new GradeRecognitionDebugView(cellImage, "<>").ShowDialog();
                        });
                    });
                });
            });

            this.debugARCodeOCRbutton.Click += new EventHandler(delegate {
                new FinderCircleDebugView(
                    formOpts.sourceImage, 
                    formOpts.minFinderCircleRadius, 
                    formOpts.maxFinderCircleRadius, 
                    inputValue: 0,
                    noiseFilter: new EmptyFilter()).ShowDialog();
            });

            this.debugTableOCRbutton.Click += new EventHandler(delegate {
                new TableRecognitionDebugView(formOpts.sourceImage).ShowDialog();
            });

            this.cancelButton.Click += new EventHandler(delegate {
                this.Hide();
                this.Dispose();
            });

            this.FormClosing += new FormClosingEventHandler(delegate {
                this.Dispose();
            });

            this.saveRegisterButton.Click += new EventHandler(delegate {
                Register register = registerEditor.GetRegister();
                RegisterMarshaller.SaveRegister(register, et);
                formOpts.registerInfoOpt.ForEach(registerInfo => {
                    registerInfo.ДатаВнесения = DateTime.Now;
                    et.SaveChanges();
                });
                this.Hide();
                formOpts.onSave(register);
                this.Dispose();
            });

            this.Disposed += new EventHandler(delegate {
                formOpts.sourceImage.Dispose();
                formOpts.sourceImage.Dispose();
                System.GC.Collect();
            });
        }

    }
}
