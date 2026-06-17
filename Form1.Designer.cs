namespace MinecraftOnPC
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            button1 = new Button();
            progressBar1 = new ProgressBar();
            richTextBox1 = new RichTextBox();
            comboBox_Rezolve = new ComboBox();
            label_Rezolve = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(14, 14);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(175, 31);
            button1.TabIndex = 0;
            button1.Text = "Çalıştır ve Minecraft'ı aç";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(196, 14);
            progressBar1.Margin = new Padding(4, 3, 4, 3);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(267, 31);
            progressBar1.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.BackColor = Color.Black;
            richTextBox1.Font = new Font("Consolas", 9F);
            richTextBox1.ForeColor = Color.Lime;
            richTextBox1.Location = new Point(14, 65);
            richTextBox1.Margin = new Padding(4, 3, 4, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox1.Size = new Size(454, 311);
            richTextBox1.TabIndex = 2;
            richTextBox1.Text = "";
            // 
            // comboBox_Rezolve
            // 
            comboBox_Rezolve.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Rezolve.FormattingEnabled = true;
            comboBox_Rezolve.Location = new Point(786, 10);
            comboBox_Rezolve.Margin = new Padding(4, 3, 4, 3);
            comboBox_Rezolve.Name = "comboBox_Rezolve";
            comboBox_Rezolve.Size = new Size(40, 23);
            comboBox_Rezolve.TabIndex = 4;
            // 
            // label_Rezolve
            // 
            label_Rezolve.AutoSize = true;
            label_Rezolve.Location = new Point(697, 14);
            label_Rezolve.Margin = new Padding(4, 0, 4, 0);
            label_Rezolve.Name = "label_Rezolve";
            label_Rezolve.Size = new Size(71, 15);
            label_Rezolve.TabIndex = 3;
            label_Rezolve.Text = "Çözünürlük:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(483, 390);
            Controls.Add(comboBox_Rezolve);
            Controls.Add(label_Rezolve);
            Controls.Add(richTextBox1);
            Controls.Add(progressBar1);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "Form1";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Minecraft PTP";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBox_Rezolve;
        private System.Windows.Forms.Label label_Rezolve;
    }
}