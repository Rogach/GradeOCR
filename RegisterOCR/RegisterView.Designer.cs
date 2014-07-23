namespace RegisterOCR {
    partial class RegisterView {
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
            this.registerPanel = new System.Windows.Forms.Panel();
            this.debugButton = new System.Windows.Forms.Button();
            this.selectRegisterButton = new System.Windows.Forms.Button();
            this.nextRegisterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // registerPanel
            // 
            this.registerPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registerPanel.Location = new System.Drawing.Point(12, 12);
            this.registerPanel.Name = "registerPanel";
            this.registerPanel.Size = new System.Drawing.Size(788, 842);
            this.registerPanel.TabIndex = 0;
            // 
            // debugButton
            // 
            this.debugButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.debugButton.Enabled = false;
            this.debugButton.Location = new System.Drawing.Point(12, 860);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(75, 23);
            this.debugButton.TabIndex = 1;
            this.debugButton.Text = "Debug OCR";
            this.debugButton.UseVisualStyleBackColor = true;
            // 
            // selectRegisterButton
            // 
            this.selectRegisterButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.selectRegisterButton.Enabled = false;
            this.selectRegisterButton.Location = new System.Drawing.Point(562, 860);
            this.selectRegisterButton.Name = "selectRegisterButton";
            this.selectRegisterButton.Size = new System.Drawing.Size(116, 23);
            this.selectRegisterButton.TabIndex = 2;
            this.selectRegisterButton.Text = "Select register";
            this.selectRegisterButton.UseVisualStyleBackColor = true;
            // 
            // nextRegisterButton
            // 
            this.nextRegisterButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nextRegisterButton.Enabled = false;
            this.nextRegisterButton.Location = new System.Drawing.Point(684, 860);
            this.nextRegisterButton.Name = "nextRegisterButton";
            this.nextRegisterButton.Size = new System.Drawing.Size(116, 23);
            this.nextRegisterButton.TabIndex = 3;
            this.nextRegisterButton.Text = "Select next register";
            this.nextRegisterButton.UseVisualStyleBackColor = true;
            // 
            // RegisterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 895);
            this.Controls.Add(this.nextRegisterButton);
            this.Controls.Add(this.selectRegisterButton);
            this.Controls.Add(this.debugButton);
            this.Controls.Add(this.registerPanel);
            this.Name = "RegisterView";
            this.Text = "RegisterView";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel registerPanel;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Button selectRegisterButton;
        private System.Windows.Forms.Button nextRegisterButton;
    }
}