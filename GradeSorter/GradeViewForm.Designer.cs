namespace GradeSorter {
    partial class GradeViewForm {
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
            this.gradePanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // gradePanel
            // 
            this.gradePanel.Location = new System.Drawing.Point(12, 12);
            this.gradePanel.Name = "gradePanel";
            this.gradePanel.Size = new System.Drawing.Size(260, 138);
            this.gradePanel.TabIndex = 0;
            // 
            // GradeViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 162);
            this.Controls.Add(this.gradePanel);
            this.Name = "GradeViewForm";
            this.Text = "GradeViewForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel gradePanel;
    }
}