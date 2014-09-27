namespace TableOCR {
    partial class TableRecognitionDebugView {
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
            this.filteredLinesPanel = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.normalizedLinesPanel = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.tableRecognitionPanel = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.recognizedTablePanel = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.rotBwImagePanel = new System.Windows.Forms.Panel();
            this.rotEdgePointsPanel = new System.Windows.Forms.Panel();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // sourceImagePanel
            // 
            this.sourceImagePanel.Location = new System.Drawing.Point(12, 25);
            this.sourceImagePanel.Name = "sourceImagePanel";
            this.sourceImagePanel.Size = new System.Drawing.Size(305, 432);
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
            this.bwImagePanel.Location = new System.Drawing.Point(325, 25);
            this.bwImagePanel.Name = "bwImagePanel";
            this.bwImagePanel.Size = new System.Drawing.Size(305, 432);
            this.bwImagePanel.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(322, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Binarization";
            // 
            // edgePointsPanel
            // 
            this.edgePointsPanel.Location = new System.Drawing.Point(636, 25);
            this.edgePointsPanel.Name = "edgePointsPanel";
            this.edgePointsPanel.Size = new System.Drawing.Size(305, 432);
            this.edgePointsPanel.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(633, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Edge points";
            // 
            // houghPanel
            // 
            this.houghPanel.Location = new System.Drawing.Point(12, 476);
            this.houghPanel.Name = "houghPanel";
            this.houghPanel.Size = new System.Drawing.Size(305, 432);
            this.houghPanel.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 460);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Pseudo-Hough transform";
            // 
            // cyclicPatternsPanel
            // 
            this.cyclicPatternsPanel.Location = new System.Drawing.Point(325, 476);
            this.cyclicPatternsPanel.Name = "cyclicPatternsPanel";
            this.cyclicPatternsPanel.Size = new System.Drawing.Size(152, 432);
            this.cyclicPatternsPanel.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(322, 460);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "cyclic patterns detection";
            // 
            // filteredLinesPanel
            // 
            this.filteredLinesPanel.Location = new System.Drawing.Point(483, 476);
            this.filteredLinesPanel.Name = "filteredLinesPanel";
            this.filteredLinesPanel.Size = new System.Drawing.Size(305, 432);
            this.filteredLinesPanel.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(480, 460);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "filtered lines";
            // 
            // normalizedLinesPanel
            // 
            this.normalizedLinesPanel.Location = new System.Drawing.Point(794, 476);
            this.normalizedLinesPanel.Name = "normalizedLinesPanel";
            this.normalizedLinesPanel.Size = new System.Drawing.Size(305, 432);
            this.normalizedLinesPanel.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(791, 460);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "normalizedLines";
            // 
            // tableRecognitionPanel
            // 
            this.tableRecognitionPanel.Location = new System.Drawing.Point(1105, 476);
            this.tableRecognitionPanel.Name = "tableRecognitionPanel";
            this.tableRecognitionPanel.Size = new System.Drawing.Size(305, 432);
            this.tableRecognitionPanel.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1102, 460);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "table recognition";
            // 
            // recognizedTablePanel
            // 
            this.recognizedTablePanel.Location = new System.Drawing.Point(1416, 476);
            this.recognizedTablePanel.Name = "recognizedTablePanel";
            this.recognizedTablePanel.Size = new System.Drawing.Size(305, 432);
            this.recognizedTablePanel.TabIndex = 9;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(1413, 460);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "recognized table";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(944, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Rotated bw";
            // 
            // rotBwImagePanel
            // 
            this.rotBwImagePanel.Location = new System.Drawing.Point(947, 25);
            this.rotBwImagePanel.Name = "rotBwImagePanel";
            this.rotBwImagePanel.Size = new System.Drawing.Size(305, 432);
            this.rotBwImagePanel.TabIndex = 2;
            // 
            // rotEdgePointsPanel
            // 
            this.rotEdgePointsPanel.Location = new System.Drawing.Point(1258, 25);
            this.rotEdgePointsPanel.Name = "rotEdgePointsPanel";
            this.rotEdgePointsPanel.Size = new System.Drawing.Size(305, 432);
            this.rotEdgePointsPanel.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1255, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(103, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Rotated edge points";
            // 
            // TableRecognitionDebugView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1730, 919);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.rotEdgePointsPanel);
            this.Controls.Add(this.rotBwImagePanel);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.recognizedTablePanel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tableRecognitionPanel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.normalizedLinesPanel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.filteredLinesPanel);
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
            this.Name = "TableRecognitionDebugView";
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
        private System.Windows.Forms.Panel filteredLinesPanel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel normalizedLinesPanel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel tableRecognitionPanel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel recognizedTablePanel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel rotBwImagePanel;
        private System.Windows.Forms.Panel rotEdgePointsPanel;
        private System.Windows.Forms.Label label11;
    }
}