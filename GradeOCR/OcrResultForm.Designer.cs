namespace GradeOCR {
    partial class OcrResultForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.sourcePanel = new System.Windows.Forms.Panel();
            this.outputPanel = new System.Windows.Forms.Panel();
            this.bwPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // sourcePanel
            // 
            this.sourcePanel.Location = new System.Drawing.Point(12, 12);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.Size = new System.Drawing.Size(405, 572);
            this.sourcePanel.TabIndex = 0;
            // 
            // outputPanel
            // 
            this.outputPanel.Location = new System.Drawing.Point(843, 12);
            this.outputPanel.Name = "outputPanel";
            this.outputPanel.Size = new System.Drawing.Size(405, 572);
            this.outputPanel.TabIndex = 1;
            // 
            // bwPanel
            // 
            this.bwPanel.Location = new System.Drawing.Point(423, 12);
            this.bwPanel.Name = "bwPanel";
            this.bwPanel.Size = new System.Drawing.Size(405, 572);
            this.bwPanel.TabIndex = 2;
            // 
            // OcrResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1255, 593);
            this.Controls.Add(this.bwPanel);
            this.Controls.Add(this.outputPanel);
            this.Controls.Add(this.sourcePanel);
            this.Name = "OcrResultForm";
            this.Text = "OcrResultForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel sourcePanel;
        private System.Windows.Forms.Panel outputPanel;
        private System.Windows.Forms.Panel bwPanel;
    }
}