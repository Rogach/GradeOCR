namespace ARCode {
    partial class FinderCircleDebugView {
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
            this.houghImagePanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.noiseImagePanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.houghPeaksImagePanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.peakResultImagePanel = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.dataMatrixLocationPanel = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.rotatedDataMatrixPanel = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.inputDataLabel = new System.Windows.Forms.Label();
            this.outputDataLabel = new System.Windows.Forms.Label();
            this.recognizedDataMatrixPanel = new System.Windows.Forms.Panel();
            this.label20 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // inputImagePanel
            // 
            this.inputImagePanel.Location = new System.Drawing.Point(12, 25);
            this.inputImagePanel.Name = "inputImagePanel";
            this.inputImagePanel.Size = new System.Drawing.Size(300, 300);
            this.inputImagePanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "input image";
            // 
            // houghImagePanel
            // 
            this.houghImagePanel.Location = new System.Drawing.Point(318, 25);
            this.houghImagePanel.Name = "houghImagePanel";
            this.houghImagePanel.Size = new System.Drawing.Size(300, 300);
            this.houghImagePanel.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(315, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Hough transform";
            // 
            // noiseImagePanel
            // 
            this.noiseImagePanel.Location = new System.Drawing.Point(12, 344);
            this.noiseImagePanel.Name = "noiseImagePanel";
            this.noiseImagePanel.Size = new System.Drawing.Size(300, 300);
            this.noiseImagePanel.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 328);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "noise applied";
            // 
            // houghPeaksImagePanel
            // 
            this.houghPeaksImagePanel.Location = new System.Drawing.Point(318, 344);
            this.houghPeaksImagePanel.Name = "houghPeaksImagePanel";
            this.houghPeaksImagePanel.Size = new System.Drawing.Size(300, 300);
            this.houghPeaksImagePanel.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(315, 328);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Hough transform with peaks";
            // 
            // peakResultImagePanel
            // 
            this.peakResultImagePanel.Location = new System.Drawing.Point(624, 25);
            this.peakResultImagePanel.Name = "peakResultImagePanel";
            this.peakResultImagePanel.Size = new System.Drawing.Size(300, 300);
            this.peakResultImagePanel.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(621, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Peak results (with tuning)";
            // 
            // dataMatrixLocationPanel
            // 
            this.dataMatrixLocationPanel.Location = new System.Drawing.Point(624, 344);
            this.dataMatrixLocationPanel.Name = "dataMatrixLocationPanel";
            this.dataMatrixLocationPanel.Size = new System.Drawing.Size(300, 300);
            this.dataMatrixLocationPanel.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(621, 328);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Data matrix location";
            // 
            // rotatedDataMatrixPanel
            // 
            this.rotatedDataMatrixPanel.Location = new System.Drawing.Point(930, 25);
            this.rotatedDataMatrixPanel.Name = "rotatedDataMatrixPanel";
            this.rotatedDataMatrixPanel.Size = new System.Drawing.Size(300, 140);
            this.rotatedDataMatrixPanel.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(927, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(99, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Rotated data matrix";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(1114, 616);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Input";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(1106, 630);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Output";
            // 
            // inputDataLabel
            // 
            this.inputDataLabel.AutoSize = true;
            this.inputDataLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.inputDataLabel.Location = new System.Drawing.Point(1151, 616);
            this.inputDataLabel.Name = "inputDataLabel";
            this.inputDataLabel.Size = new System.Drawing.Size(77, 14);
            this.inputDataLabel.TabIndex = 11;
            this.inputDataLabel.Text = "0000000000";
            // 
            // outputDataLabel
            // 
            this.outputDataLabel.AutoSize = true;
            this.outputDataLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.outputDataLabel.Location = new System.Drawing.Point(1151, 630);
            this.outputDataLabel.Name = "outputDataLabel";
            this.outputDataLabel.Size = new System.Drawing.Size(77, 14);
            this.outputDataLabel.TabIndex = 12;
            this.outputDataLabel.Text = "0000000000";
            // 
            // recognizedDataMatrixPanel
            // 
            this.recognizedDataMatrixPanel.Location = new System.Drawing.Point(930, 185);
            this.recognizedDataMatrixPanel.Name = "recognizedDataMatrixPanel";
            this.recognizedDataMatrixPanel.Size = new System.Drawing.Size(300, 140);
            this.recognizedDataMatrixPanel.TabIndex = 6;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(930, 169);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(118, 13);
            this.label20.TabIndex = 13;
            this.label20.Text = "Recognized data matrix";
            // 
            // FinderCircleDebugView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1240, 653);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.recognizedDataMatrixPanel);
            this.Controls.Add(this.outputDataLabel);
            this.Controls.Add(this.inputDataLabel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.rotatedDataMatrixPanel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.dataMatrixLocationPanel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.peakResultImagePanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.houghPeaksImagePanel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.noiseImagePanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.houghImagePanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inputImagePanel);
            this.Name = "FinderCircleDebugView";
            this.Text = "FinderCircleDebugView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel inputImagePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel houghImagePanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel noiseImagePanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel houghPeaksImagePanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel peakResultImagePanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel dataMatrixLocationPanel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel rotatedDataMatrixPanel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label inputDataLabel;
        private System.Windows.Forms.Label outputDataLabel;
        private System.Windows.Forms.Panel recognizedDataMatrixPanel;
        private System.Windows.Forms.Label label20;
    }
}