namespace RegisterProcessor {
    partial class RegisterViewForm {
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
            this.nextRegisterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // registerPanel
            // 
            this.registerPanel.Location = new System.Drawing.Point(12, 12);
            this.registerPanel.Name = "registerPanel";
            this.registerPanel.Size = new System.Drawing.Size(871, 891);
            this.registerPanel.TabIndex = 0;
            // 
            // nextRegisterButton
            // 
            this.nextRegisterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.nextRegisterButton.Location = new System.Drawing.Point(375, 909);
            this.nextRegisterButton.Name = "nextRegisterButton";
            this.nextRegisterButton.Size = new System.Drawing.Size(145, 31);
            this.nextRegisterButton.TabIndex = 1;
            this.nextRegisterButton.Text = "Next register";
            this.nextRegisterButton.UseVisualStyleBackColor = true;
            // 
            // RegisterViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 952);
            this.Controls.Add(this.nextRegisterButton);
            this.Controls.Add(this.registerPanel);
            this.Name = "RegisterViewForm";
            this.Text = "RegisterViewForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel registerPanel;
        private System.Windows.Forms.Button nextRegisterButton;
    }
}