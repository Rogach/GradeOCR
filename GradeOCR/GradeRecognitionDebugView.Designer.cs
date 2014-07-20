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
            // GradeRecognitionDebugView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 369);
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
    }
}