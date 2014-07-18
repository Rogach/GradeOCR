using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GradeOCR {
    public partial class OcrResultForm : Form {
        public PictureView sourcePV;
        public PictureView bwPV;
        public PictureView freqPV;
        public PictureView outputPV;

        public PictureView sourcePV_vert;
        public PictureView bwPV_vert;
        public PictureView freqPV_vert;
        public PictureView outputPV_vert;

        public PictureView resultPV;

        public OcrResultForm() {
            InitializeComponent();
            sourcePV = PictureView.InsertIntoPanel(this.sourcePanel);
            bwPV = PictureView.InsertIntoPanel(this.bwPanel);
            freqPV = PictureView.InsertIntoPanel(this.freqPanel);
            outputPV = PictureView.InsertIntoPanel(this.outputPanel);

            sourcePV_vert = PictureView.InsertIntoPanel(this.sourcePanelVert);
            bwPV_vert = PictureView.InsertIntoPanel(this.bwPanelVert);
            freqPV_vert = PictureView.InsertIntoPanel(this.freqPanelVert);
            outputPV_vert = PictureView.InsertIntoPanel(this.outputPanelVert);

            resultPV = PictureView.InsertIntoPanel(this.tableResultPanel);
        }
    }
}
