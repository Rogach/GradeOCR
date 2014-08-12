namespace LineOCR {
    partial class LineRecognitionDebugForm {
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
            this.sourceImagePanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.bwImagePanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.edgePointsPanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.houghPanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.cyclicPatternsPanel = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sourceImagePanel
            // 
            this.sourceImagePanel.Location = new System.Drawing.Point(12, 25);
            this.sourceImagePanel.Name = "sourceImagePanel";
            this.sourceImagePanel.Size = new System.Drawing.Size(305, 468);
            this.sourceImagePanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source image";
            // 
            // bwImagePanel
            // 
            this.bwImagePanel.Location = new System.Drawing.Point(323, 25);
            this.bwImagePanel.Name = "bwImagePanel";
            this.bwImagePanel.Size = new System.Drawing.Size(305, 468);
            this.bwImagePanel.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(320, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Binarization";
            // 
            // edgePointsPanel
            // 
            this.edgePointsPanel.Location = new System.Drawing.Point(634, 25);
            this.edgePointsPanel.Name = "edgePointsPanel";
            this.edgePointsPanel.Size = new System.Drawing.Size(305, 468);
            this.edgePointsPanel.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(631, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Edge points";
            // 
            // houghPanel
            // 
            this.houghPanel.Location = new System.Drawing.Point(945, 25);
            this.houghPanel.Name = "houghPanel";
            this.houghPanel.Size = new System.Drawing.Size(305, 468);
            this.houghPanel.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(942, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Pseudo-Hough transform";
            // 
            // cyclicPatternsPanel
            // 
            this.cyclicPatternsPanel.Location = new System.Drawing.Point(1256, 25);
            this.cyclicPatternsPanel.Name = "cyclicPatternsPanel";
            this.cyclicPatternsPanel.Size = new System.Drawing.Size(139, 468);
            this.cyclicPatternsPanel.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1253, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "cyclic patterns detection";
            // 
            // LineRecognitionDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1407, 505);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cyclicPatternsPanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.houghPanel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.edgePointsPanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bwImagePanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sourceImagePanel);
            this.Name = "LineRecognitionDebugForm";
            this.Text = "LineRecognitionDebugForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel sourceImagePanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel bwImagePanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel edgePointsPanel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel houghPanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel cyclicPatternsPanel;
        private System.Windows.Forms.Label label5;
    }
}