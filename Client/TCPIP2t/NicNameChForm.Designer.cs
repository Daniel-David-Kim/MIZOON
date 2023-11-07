namespace TCPIP2t
{
    partial class NicNameChForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.enter = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.logo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // enter
            // 
            this.enter.Location = new System.Drawing.Point(115, 160);
            this.enter.Name = "enter";
            this.enter.Size = new System.Drawing.Size(216, 23);
            this.enter.TabIndex = 1;
            this.enter.Text = "입장";
            this.enter.UseVisualStyleBackColor = true;
            this.enter.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(115, 131);
            this.textBox1.MaxLength = 8;
            this.textBox1.Name = "textBox1";
            this.textBox1.PlaceholderText = "닉네임은 8글자 이하로만 가능합니다.";
            this.textBox1.Size = new System.Drawing.Size(216, 23);
            this.textBox1.TabIndex = 2;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "서버1",
            "서버2"});
            this.comboBox1.Location = new System.Drawing.Point(115, 102);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(216, 23);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.Text = "서버 선택";
            // 
            // logo
            // 
            this.logo.AutoSize = true;
            this.logo.Font = new System.Drawing.Font("Franklin Gothic Medium", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.logo.ForeColor = System.Drawing.SystemColors.Highlight;
            this.logo.Location = new System.Drawing.Point(145, 47);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(157, 43);
            this.logo.TabIndex = 13;
            this.logo.Text = "MIZOON";
            // 
            // NicNameChForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 255);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.enter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NicNameChForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "닉네임 설정";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Button enter;
        private TextBox textBox1;
        private ComboBox comboBox1;
        private Label logo;
    }
}