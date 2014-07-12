namespace Grader.gui {
    partial class DbConnectionDialog {
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.server_text = new System.Windows.Forms.TextBox();
            this.port_text = new System.Windows.Forms.TextBox();
            this.user_text = new System.Windows.Forms.TextBox();
            this.password_text = new System.Windows.Forms.TextBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Сервер";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Порт";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Пользователь";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Пароль";
            // 
            // server_text
            // 
            this.server_text.Location = new System.Drawing.Point(109, 12);
            this.server_text.Name = "server_text";
            this.server_text.Size = new System.Drawing.Size(100, 20);
            this.server_text.TabIndex = 1;
            // 
            // port_text
            // 
            this.port_text.Location = new System.Drawing.Point(109, 38);
            this.port_text.Name = "port_text";
            this.port_text.Size = new System.Drawing.Size(100, 20);
            this.port_text.TabIndex = 2;
            // 
            // user_text
            // 
            this.user_text.Location = new System.Drawing.Point(109, 64);
            this.user_text.Name = "user_text";
            this.user_text.Size = new System.Drawing.Size(100, 20);
            this.user_text.TabIndex = 3;
            // 
            // password_text
            // 
            this.password_text.Location = new System.Drawing.Point(109, 90);
            this.password_text.Name = "password_text";
            this.password_text.Size = new System.Drawing.Size(100, 20);
            this.password_text.TabIndex = 4;
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(109, 116);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(100, 23);
            this.ok_button.TabIndex = 5;
            this.ok_button.Text = "ОК";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(12, 116);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(91, 23);
            this.cancel_button.TabIndex = 6;
            this.cancel_button.Text = "Отмена";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // DbConnectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(224, 151);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.password_text);
            this.Controls.Add(this.user_text);
            this.Controls.Add(this.port_text);
            this.Controls.Add(this.server_text);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "DbConnectionDialog";
            this.Text = "Подключение";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.TextBox server_text;
        private System.Windows.Forms.TextBox port_text;
        private System.Windows.Forms.TextBox user_text;
        private System.Windows.Forms.TextBox password_text;
    }
}