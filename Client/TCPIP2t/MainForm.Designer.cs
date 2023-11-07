namespace TCPIP2t
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chattingBox1 = new System.Windows.Forms.TextBox();
            this.chattingBox2 = new System.Windows.Forms.TextBox();
            this.serverChange = new System.Windows.Forms.Button();
            this.logo = new System.Windows.Forms.Label();
            this.fileUpload = new System.Windows.Forms.Button();
            this.megaChat = new System.Windows.Forms.Button();
            this.sendChat = new System.Windows.Forms.Button();
            this.nickNameBox = new System.Windows.Forms.TextBox();
            this.download = new System.Windows.Forms.Button();
            this.lb1 = new System.Windows.Forms.ListBox();
            this.filesend = new System.Windows.Forms.Button();
            this.path = new System.Windows.Forms.Label();
            this.servername = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chattingBox1
            // 
            this.chattingBox1.Location = new System.Drawing.Point(1, 427);
            this.chattingBox1.Name = "chattingBox1";
            this.chattingBox1.Size = new System.Drawing.Size(594, 23);
            this.chattingBox1.TabIndex = 1;
            // 
            // chattingBox2
            // 
            this.chattingBox2.Location = new System.Drawing.Point(1, 59);
            this.chattingBox2.MaximumSize = new System.Drawing.Size(659, 374);
            this.chattingBox2.MinimumSize = new System.Drawing.Size(659, 80);
            this.chattingBox2.Multiline = true;
            this.chattingBox2.Name = "chattingBox2";
            this.chattingBox2.ReadOnly = true;
            this.chattingBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chattingBox2.Size = new System.Drawing.Size(659, 338);
            this.chattingBox2.TabIndex = 2;
            // 
            // serverChange
            // 
            this.serverChange.Location = new System.Drawing.Point(748, 28);
            this.serverChange.Name = "serverChange";
            this.serverChange.Size = new System.Drawing.Size(53, 23);
            this.serverChange.TabIndex = 10;
            this.serverChange.Text = "종료";
            this.serverChange.UseVisualStyleBackColor = true;
            this.serverChange.Click += new System.EventHandler(this.button1_Click);
            // 
            // logo
            // 
            this.logo.AutoSize = true;
            this.logo.Font = new System.Drawing.Font("Franklin Gothic Medium", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.logo.ForeColor = System.Drawing.SystemColors.Highlight;
            this.logo.Location = new System.Drawing.Point(12, 8);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(157, 43);
            this.logo.TabIndex = 12;
            this.logo.Text = "MIZOON";
            // 
            // fileUpload
            // 
            this.fileUpload.BackColor = System.Drawing.Color.Transparent;
            this.fileUpload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.fileUpload.Image = global::TCPIP2t.Properties.Resources.file1;
            this.fileUpload.Location = new System.Drawing.Point(33, 395);
            this.fileUpload.Margin = new System.Windows.Forms.Padding(0);
            this.fileUpload.Name = "fileUpload";
            this.fileUpload.Size = new System.Drawing.Size(32, 32);
            this.fileUpload.TabIndex = 14;
            this.fileUpload.UseVisualStyleBackColor = false;
            this.fileUpload.Click += new System.EventHandler(this.fileUpload_Click);
            // 
            // megaChat
            // 
            this.megaChat.BackColor = System.Drawing.Color.Transparent;
            this.megaChat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.megaChat.Image = global::TCPIP2t.Properties.Resources.megaphone;
            this.megaChat.Location = new System.Drawing.Point(1, 395);
            this.megaChat.Margin = new System.Windows.Forms.Padding(0);
            this.megaChat.Name = "megaChat";
            this.megaChat.Size = new System.Drawing.Size(32, 32);
            this.megaChat.TabIndex = 15;
            this.megaChat.UseVisualStyleBackColor = false;
            // 
            // sendChat
            // 
            this.sendChat.Location = new System.Drawing.Point(601, 426);
            this.sendChat.Name = "sendChat";
            this.sendChat.Size = new System.Drawing.Size(53, 23);
            this.sendChat.TabIndex = 16;
            this.sendChat.Text = "보내기";
            this.sendChat.UseVisualStyleBackColor = true;
            this.sendChat.Click += new System.EventHandler(this.sendChat_Click);
            // 
            // nickNameBox
            // 
            this.nickNameBox.Location = new System.Drawing.Point(503, 28);
            this.nickNameBox.Name = "nickNameBox";
            this.nickNameBox.ReadOnly = true;
            this.nickNameBox.Size = new System.Drawing.Size(149, 23);
            this.nickNameBox.TabIndex = 17;
            // 
            // download
            // 
            this.download.Location = new System.Drawing.Point(693, 374);
            this.download.Name = "download";
            this.download.Size = new System.Drawing.Size(75, 23);
            this.download.TabIndex = 18;
            this.download.Text = "다운로드";
            this.download.UseVisualStyleBackColor = true;
            this.download.Click += new System.EventHandler(this.download_Click);
            // 
            // lb1
            // 
            this.lb1.FormattingEnabled = true;
            this.lb1.ItemHeight = 15;
            this.lb1.Location = new System.Drawing.Point(660, 59);
            this.lb1.Name = "lb1";
            this.lb1.Size = new System.Drawing.Size(141, 304);
            this.lb1.TabIndex = 19;
            // 
            // filesend
            // 
            this.filesend.Location = new System.Drawing.Point(601, 400);
            this.filesend.Name = "filesend";
            this.filesend.Size = new System.Drawing.Size(53, 23);
            this.filesend.TabIndex = 20;
            this.filesend.Text = "전송";
            this.filesend.UseVisualStyleBackColor = true;
            this.filesend.Click += new System.EventHandler(this.filesend_Click);
            // 
            // path
            // 
            this.path.AutoSize = true;
            this.path.Location = new System.Drawing.Point(68, 403);
            this.path.Name = "path";
            this.path.Size = new System.Drawing.Size(0, 15);
            this.path.TabIndex = 21;
            // 
            // servername
            // 
            this.servername.AutoSize = true;
            this.servername.Location = new System.Drawing.Point(660, 34);
            this.servername.Name = "servername";
            this.servername.Size = new System.Drawing.Size(0, 15);
            this.servername.TabIndex = 22;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.servername);
            this.Controls.Add(this.path);
            this.Controls.Add(this.filesend);
            this.Controls.Add(this.lb1);
            this.Controls.Add(this.download);
            this.Controls.Add(this.nickNameBox);
            this.Controls.Add(this.sendChat);
            this.Controls.Add(this.megaChat);
            this.Controls.Add(this.chattingBox2);
            this.Controls.Add(this.fileUpload);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.serverChange);
            this.Controls.Add(this.chattingBox1);
            this.Name = "MainForm";
            this.Text = "채팅프로그램";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TextBox chattingBox1;
        private TextBox chattingBox2;
        private Button serverChange;
        private Label logo;
        private Button fileUpload;
        private Button megaChat;
        private Button sendChat;
        private TextBox nickNameBox;
        private Button download;
        private ListBox lb1;
        private Button filesend;
        private Label path;
        private Label servername;
    }
}