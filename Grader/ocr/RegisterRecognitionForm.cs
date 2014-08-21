using System;
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
            public Option<ВедомостьДляРаспознавания> registerInfoOpt = new None<ВедомостьДляРаспознавания>();
            public Option<Register> registerOpt = new None<Register>();
            public Option<Table> recognizedTable = new None<Table>();
            public Option<DataMatrixExtraction> dmeOpt = new None<DataMatrixExtraction>();
            public int minFinderCircleRadius;
            public int maxFinderCircleRadius;
        }

        private PictureView ocrImagePV;
        private RegisterEditor registerEditor;

        public RegisterRecognitionForm(Entities et, GradeDigestSet digestSet, Options formOpts) {
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
                    Bitmap debugImage = new Bitmap(formOpts.sourceImage);
                    formOpts.dmeOpt.ForEach(dme => {
                        dme.DrawPositioningDebug(debugImage);
                    });
                    formOpts.recognizedTable.ForEach(table => {
                        Graphics g = Graphics.FromImage(debugImage);
                        table.DrawTable(g, new Pen(Brushes.Red, 4));
                        g.Dispose();

                        Bitmap bwImage = ImageUtil.ToBlackAndWhite(formOpts.sourceImage);

                        this.ocrImagePV.AddDoubleClickListener((pt, e) => {
                            Option<Point> cellOpt = table.GetCellAtPoint(pt.X, pt.Y);
                            cellOpt.ForEach(cell => {
                                Bitmap cellImage = table.GetCellImage(bwImage, cell.X, cell.Y);
                                new GradeRecognitionDebugView(cellImage, "<>", digestSet).ShowDialog();
                            });
                        });
                    });
                    ocrImagePV.Image = debugImage;
                });

                this.debugARCodeOCRbutton.Click += new EventHandler(delegate {
                    new FinderCircleDebugView(
                        formOpts.sourceImage, 
                        formOpts.minFinderCircleRadius, 
                        formOpts.maxFinderCircleRadius, 
                        0).ShowDialog();
                });

                this.debugTableOCRbutton.Click += new EventHandler(delegate {
                    new TableRecognitionDebugView(formOpts.sourceImage).ShowDialog();
                });
        }

    }
}
