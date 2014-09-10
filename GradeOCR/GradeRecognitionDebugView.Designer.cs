namespace GradeOCR {
    partial class GradeRecognitionDebugView {
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
            this.inputImagePanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.removeBorderPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.noiseRemovalPanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.croppedPanel = new System.Windows.Forms.Panel();
            this.digestPanel = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.recognizedGradeLabel = new System.Windows.Forms.Label();
            this.recognitionConfidenceLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // inputImagePanel
            // 
            this.inputImagePanel.Location = new System.Drawing.Point(12, 25);
            this.inputImagePanel.Name = "inputImagePanel";
            this.inputImagePanel.Size = new System.Drawing.Size(296, 100);
            this.inputImagePanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input image";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Removed border";
            // 
            // removeBorderPanel
            // 
            this.removeBorderPanel.Location = new System.Drawing.Point(12, 144);
            this.removeBorderPanel.Name = "removeBorderPanel";
            this.removeBorderPanel.Size = new System.Drawing.Size(296, 100);
            this.removeBorderPanel.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 247);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Noise removal";
            // 
            // noiseRemovalPanel
            // 
            this.noiseRemovalPanel.Location = new System.Drawing.Point(12, 263);
            this.noiseRemovalPanel.Name = "noiseRemovalPanel";
            this.noiseRemovalPanel.Size = new System.Drawing.Size(296, 100);
            this.noiseRemovalPanel.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 366);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Cropped";
            // 
            // croppedPanel
            // 
            this.croppedPanel.Location = new System.Drawing.Point(12, 382);
            this.croppedPanel.Name = "croppedPanel";
            this.croppedPanel.Size = new System.Drawing.Size(143, 100);
            this.croppedPanel.TabIndex = 3;
            // 
            // digestPanel
            // 
            this.digestPanel.Location = new System.Drawing.Point(165, 382);
            this.digestPanel.Name = "digestPanel";
            this.digestPanel.Size = new System.Drawing.Size(143, 100);
            this.digestPanel.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(162, 366);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Digest";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 485);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Best match";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(162, 485);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Difference";
            // 
            // recognizedGradeLabel
            // 
            this.recognizedGradeLabel.AutoSize = true;
            this.recognizedGradeLabel.Font = new System.Drawing.Font("Arial Black", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.recognizedGradeLabel.Location = new System.Drawing.Point(12, 507);
            this.recognizedGradeLabel.Name = "recognizedGradeLabel";
            this.recognizedGradeLabel.Size = new System.Drawing.Size(81, 90);
            this.recognizedGradeLabel.TabIndex = 8;
            this.recognizedGradeLabel.Text = "0";
            this.recognizedGradeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // recognitionConfidenceLabel
            // 
            this.recognitionConfidenceLabel.AutoSize = true;
            this.recognitionConfidenceLabel.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.recognitionConfidenceLabel.Location = new System.Drawing.Point(82, 554);
            this.recognitionConfidenceLabel.Name = "recognitionConfidenceLabel";
            this.recognitionConfidenceLabel.Size = new System.Drawing.Size(93, 29);
            this.recognitionConfidenceLabel.TabIndex = 9;
            this.recognitionConfidenceLabel.Text = "unsure";
            // 
            // GradeRecognitionDebugView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 602);
            this.Controls.Add(this.recognitionConfidenceLabel);
            this.Controls.Add(this.recognizedGradeLabel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.digestPanel);
            this.Controls.Add(this.croppedPanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.noiseRemovalPanel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.removeBorderPanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inputImagePanel);
            this.Name = "GradeRecognitionDebugView";
            this.Text = "GradeRecognitionDebugView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel inputImagePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel removeBorderPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel noiseRemovalPanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel croppedPanel;
        private System.Windows.Forms.Panel digestPanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label recognizedGradeLabel;
        private System.Windows.Forms.Label recognitionConfidenceLabel;
    }
}