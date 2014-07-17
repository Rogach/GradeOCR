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
            this.freqPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sourcePanel
            // 
            this.sourcePanel.Location = new System.Drawing.Point(12, 25);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.Size = new System.Drawing.Size(405, 572);
            this.sourcePanel.TabIndex = 0;
            // 
            // outputPanel
            // 
            this.outputPanel.Location = new System.Drawing.Point(1245, 25);
            this.outputPanel.Name = "outputPanel";
            this.outputPanel.Size = new System.Drawing.Size(405, 572);
            this.outputPanel.TabIndex = 1;
            // 
            // bwPanel
            // 
            this.bwPanel.Location = new System.Drawing.Point(423, 25);
            this.bwPanel.Name = "bwPanel";
            this.bwPanel.Size = new System.Drawing.Size(405, 572);
            this.bwPanel.TabIndex = 2;
            // 
            // freqPanel
            // 
            this.freqPanel.Location = new System.Drawing.Point(834, 25);
            this.freqPanel.Name = "freqPanel";
            this.freqPanel.Size = new System.Drawing.Size(405, 572);
            this.freqPanel.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(624, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Black&&White";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1008, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "black count graph";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1441, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "result";
            // 
            // OcrResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1659, 608);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.freqPanel);
            this.Controls.Add(this.bwPanel);
            this.Controls.Add(this.outputPanel);
            this.Controls.Add(this.sourcePanel);
            this.Name = "OcrResultForm";
            this.Text = "OcrResultForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel sourcePanel;
        private System.Windows.Forms.Panel outputPanel;
        private System.Windows.Forms.Panel bwPanel;
        private System.Windows.Forms.Panel freqPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}