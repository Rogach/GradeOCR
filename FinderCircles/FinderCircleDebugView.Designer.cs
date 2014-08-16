namespace FinderCircles {
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
            // FinderCircleDebugView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 655);
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
    }
}