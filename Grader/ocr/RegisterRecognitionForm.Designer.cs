namespace Grader.ocr {
    partial class RegisterRecognitionForm {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ocrImagePanel = new System.Windows.Forms.Panel();
            this.debugTableOCRbutton = new System.Windows.Forms.Button();
            this.debugARCodeOCRbutton = new System.Windows.Forms.Button();
            this.registerEditorPanel = new System.Windows.Forms.Panel();
            this.saveRegisterButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ocrImagePanel);
            this.splitContainer1.Panel1.Controls.Add(this.debugTableOCRbutton);
            this.splitContainer1.Panel1.Controls.Add(this.debugARCodeOCRbutton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.registerEditorPanel);
            this.splitContainer1.Panel2.Controls.Add(this.saveRegisterButton);
            this.splitContainer1.Panel2.Controls.Add(this.cancelButton);
            this.splitContainer1.Size = new System.Drawing.Size(1400, 962);
            this.splitContainer1.SplitterDistance = 700;
            this.splitContainer1.TabIndex = 0;
            // 
            // ocrImagePanel
            // 
            this.ocrImagePanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ocrImagePanel.Location = new System.Drawing.Point(3, 3);
            this.ocrImagePanel.Name = "ocrImagePanel";
            this.ocrImagePanel.Size = new System.Drawing.Size(694, 918);
            this.ocrImagePanel.TabIndex = 2;
            // 
            // debugTableOCRbutton
            // 
            this.debugTableOCRbutton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.debugTableOCRbutton.Location = new System.Drawing.Point(141, 927);
            this.debugTableOCRbutton.Name = "debugTableOCRbutton";
            this.debugTableOCRbutton.Size = new System.Drawing.Size(101, 23);
            this.debugTableOCRbutton.TabIndex = 1;
            this.debugTableOCRbutton.Text = "Debug table OCR";
            this.debugTableOCRbutton.UseVisualStyleBackColor = true;
            // 
            // debugARCodeOCRbutton
            // 
            this.debugARCodeOCRbutton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.debugARCodeOCRbutton.Location = new System.Drawing.Point(12, 927);
            this.debugARCodeOCRbutton.Name = "debugARCodeOCRbutton";
            this.debugARCodeOCRbutton.Size = new System.Drawing.Size(123, 23);
            this.debugARCodeOCRbutton.TabIndex = 0;
            this.debugARCodeOCRbutton.Text = "Debug AR-code OCR";
            this.debugARCodeOCRbutton.UseVisualStyleBackColor = true;
            // 
            // registerEditorPanel
            // 
            this.registerEditorPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registerEditorPanel.Location = new System.Drawing.Point(3, 3);
            this.registerEditorPanel.Name = "registerEditorPanel";
            this.registerEditorPanel.Size = new System.Drawing.Size(690, 918);
            this.registerEditorPanel.TabIndex = 2;
            // 
            // saveRegisterButton
            // 
            this.saveRegisterButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveRegisterButton.Location = new System.Drawing.Point(477, 927);
            this.saveRegisterButton.Name = "saveRegisterButton";
            this.saveRegisterButton.Size = new System.Drawing.Size(126, 23);
            this.saveRegisterButton.TabIndex = 1;
            this.saveRegisterButton.Text = "Сохранить ведомость";
            this.saveRegisterButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(609, 927);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // RegisterRecognitionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 962);
            this.Controls.Add(this.splitContainer1);
            this.Name = "RegisterRecognitionForm";
            this.Text = "RegisterRecognition";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel ocrImagePanel;
        private System.Windows.Forms.Button saveRegisterButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel registerEditorPanel;
        public System.Windows.Forms.Button debugTableOCRbutton;
        public System.Windows.Forms.Button debugARCodeOCRbutton;
    }
}