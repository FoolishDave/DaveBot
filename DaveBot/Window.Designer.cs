namespace DaveBot
{
    partial class Window
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
            this.loginBox = new System.Windows.Forms.GroupBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.tokenBox = new System.Windows.Forms.TextBox();
            this.tokenLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.userBox = new System.Windows.Forms.GroupBox();
            this.colorButton = new System.Windows.Forms.Button();
            this.banButton = new System.Windows.Forms.Button();
            this.kickButton = new System.Windows.Forms.Button();
            this.deafenButton = new System.Windows.Forms.Button();
            this.muteButton = new System.Windows.Forms.Button();
            this.memberList = new System.Windows.Forms.ListView();
            this.serverBox = new System.Windows.Forms.GroupBox();
            this.addServerButton = new System.Windows.Forms.Button();
            this.serverIdBox = new System.Windows.Forms.TextBox();
            this.serverIdList = new System.Windows.Forms.ListBox();
            this.musicBox = new System.Windows.Forms.GroupBox();
            this.enqueueButton = new System.Windows.Forms.Button();
            this.urlBox = new System.Windows.Forms.TextBox();
            this.volumeText = new System.Windows.Forms.Label();
            this.volumeBar = new System.Windows.Forms.TrackBar();
            this.nextButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.queueBox = new System.Windows.Forms.ListBox();
            this.loginBox.SuspendLayout();
            this.userBox.SuspendLayout();
            this.serverBox.SuspendLayout();
            this.musicBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.volumeBar)).BeginInit();
            this.SuspendLayout();
            // 
            // loginBox
            // 
            this.loginBox.Controls.Add(this.loginButton);
            this.loginBox.Controls.Add(this.tokenBox);
            this.loginBox.Controls.Add(this.tokenLabel);
            this.loginBox.Location = new System.Drawing.Point(12, 12);
            this.loginBox.Name = "loginBox";
            this.loginBox.Size = new System.Drawing.Size(225, 87);
            this.loginBox.TabIndex = 0;
            this.loginBox.TabStop = false;
            this.loginBox.Text = "Login";
            this.loginBox.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(140, 48);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(75, 23);
            this.loginButton.TabIndex = 6;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // tokenBox
            // 
            this.tokenBox.AcceptsTab = true;
            this.tokenBox.Location = new System.Drawing.Point(53, 22);
            this.tokenBox.Name = "tokenBox";
            this.tokenBox.PasswordChar = '*';
            this.tokenBox.Size = new System.Drawing.Size(162, 20);
            this.tokenBox.TabIndex = 5;
            this.tokenBox.Text = "MTcyNDIxNzg2MTgzNDAxNDcy.CfnSDw._rArOv052d_eHRuf2Xw-ARs3pWg";
            this.tokenBox.Enter += new System.EventHandler(this.tokenBox_Enter);
            // 
            // tokenLabel
            // 
            this.tokenLabel.AutoSize = true;
            this.tokenLabel.Location = new System.Drawing.Point(6, 25);
            this.tokenLabel.Name = "tokenLabel";
            this.tokenLabel.Size = new System.Drawing.Size(41, 13);
            this.tokenLabel.TabIndex = 2;
            this.tokenLabel.Text = "Token:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.ForeColor = System.Drawing.Color.Red;
            this.statusLabel.Location = new System.Drawing.Point(9, 348);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(73, 13);
            this.statusLabel.TabIndex = 7;
            this.statusLabel.Text = "Status: Offline";
            // 
            // userBox
            // 
            this.userBox.Controls.Add(this.colorButton);
            this.userBox.Controls.Add(this.banButton);
            this.userBox.Controls.Add(this.kickButton);
            this.userBox.Controls.Add(this.deafenButton);
            this.userBox.Controls.Add(this.muteButton);
            this.userBox.Controls.Add(this.memberList);
            this.userBox.Location = new System.Drawing.Point(574, 4);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(239, 357);
            this.userBox.TabIndex = 1;
            this.userBox.TabStop = false;
            this.userBox.Text = "User Control";
            this.userBox.Enter += new System.EventHandler(this.groupBox1_Enter_1);
            // 
            // colorButton
            // 
            this.colorButton.Enabled = false;
            this.colorButton.Location = new System.Drawing.Point(6, 146);
            this.colorButton.Name = "colorButton";
            this.colorButton.Size = new System.Drawing.Size(109, 24);
            this.colorButton.TabIndex = 5;
            this.colorButton.Text = "User Color";
            this.colorButton.UseVisualStyleBackColor = true;
            // 
            // banButton
            // 
            this.banButton.Enabled = false;
            this.banButton.Location = new System.Drawing.Point(6, 116);
            this.banButton.Name = "banButton";
            this.banButton.Size = new System.Drawing.Size(109, 24);
            this.banButton.TabIndex = 4;
            this.banButton.Text = "Ban";
            this.banButton.UseVisualStyleBackColor = true;
            // 
            // kickButton
            // 
            this.kickButton.Enabled = false;
            this.kickButton.Location = new System.Drawing.Point(6, 86);
            this.kickButton.Name = "kickButton";
            this.kickButton.Size = new System.Drawing.Size(109, 24);
            this.kickButton.TabIndex = 3;
            this.kickButton.Text = "Kick";
            this.kickButton.UseVisualStyleBackColor = true;
            // 
            // deafenButton
            // 
            this.deafenButton.Enabled = false;
            this.deafenButton.Location = new System.Drawing.Point(6, 56);
            this.deafenButton.Name = "deafenButton";
            this.deafenButton.Size = new System.Drawing.Size(109, 24);
            this.deafenButton.TabIndex = 2;
            this.deafenButton.Text = "Server Deafen";
            this.deafenButton.UseVisualStyleBackColor = true;
            // 
            // muteButton
            // 
            this.muteButton.Enabled = false;
            this.muteButton.Location = new System.Drawing.Point(6, 26);
            this.muteButton.Name = "muteButton";
            this.muteButton.Size = new System.Drawing.Size(109, 24);
            this.muteButton.TabIndex = 1;
            this.muteButton.Text = "Server Mute";
            this.muteButton.UseVisualStyleBackColor = true;
            // 
            // memberList
            // 
            this.memberList.Location = new System.Drawing.Point(121, 19);
            this.memberList.Name = "memberList";
            this.memberList.Size = new System.Drawing.Size(112, 329);
            this.memberList.TabIndex = 0;
            this.memberList.UseCompatibleStateImageBehavior = false;
            this.memberList.View = System.Windows.Forms.View.List;
            // 
            // serverBox
            // 
            this.serverBox.Controls.Add(this.addServerButton);
            this.serverBox.Controls.Add(this.serverIdBox);
            this.serverBox.Controls.Add(this.serverIdList);
            this.serverBox.Location = new System.Drawing.Point(12, 117);
            this.serverBox.Name = "serverBox";
            this.serverBox.Size = new System.Drawing.Size(224, 175);
            this.serverBox.TabIndex = 8;
            this.serverBox.TabStop = false;
            this.serverBox.Text = "Server Connection";
            // 
            // addServerButton
            // 
            this.addServerButton.Enabled = false;
            this.addServerButton.Location = new System.Drawing.Point(9, 148);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(205, 21);
            this.addServerButton.TabIndex = 10;
            this.addServerButton.Text = "Add Server";
            this.addServerButton.UseVisualStyleBackColor = true;
            this.addServerButton.Click += new System.EventHandler(this.addServerButton_Click);
            // 
            // serverIdBox
            // 
            this.serverIdBox.Enabled = false;
            this.serverIdBox.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.serverIdBox.Location = new System.Drawing.Point(9, 122);
            this.serverIdBox.Name = "serverIdBox";
            this.serverIdBox.Size = new System.Drawing.Size(204, 20);
            this.serverIdBox.TabIndex = 9;
            this.serverIdBox.Text = "Server ID";
            this.serverIdBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.serverIdBox.Enter += new System.EventHandler(this.serverIdBox_GotFocus);
            this.serverIdBox.Leave += new System.EventHandler(this.serverIdBox_LostFocus);
            // 
            // serverIdList
            // 
            this.serverIdList.FormattingEnabled = true;
            this.serverIdList.Location = new System.Drawing.Point(9, 20);
            this.serverIdList.Name = "serverIdList";
            this.serverIdList.Size = new System.Drawing.Size(205, 95);
            this.serverIdList.TabIndex = 8;
            this.serverIdList.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            // 
            // musicBox
            // 
            this.musicBox.Controls.Add(this.enqueueButton);
            this.musicBox.Controls.Add(this.urlBox);
            this.musicBox.Controls.Add(this.volumeText);
            this.musicBox.Controls.Add(this.volumeBar);
            this.musicBox.Controls.Add(this.nextButton);
            this.musicBox.Controls.Add(this.playButton);
            this.musicBox.Controls.Add(this.backButton);
            this.musicBox.Controls.Add(this.queueBox);
            this.musicBox.Location = new System.Drawing.Point(243, 4);
            this.musicBox.Name = "musicBox";
            this.musicBox.Size = new System.Drawing.Size(325, 357);
            this.musicBox.TabIndex = 9;
            this.musicBox.TabStop = false;
            this.musicBox.Text = "Music Control";
            // 
            // enqueueButton
            // 
            this.enqueueButton.Enabled = false;
            this.enqueueButton.Location = new System.Drawing.Point(6, 86);
            this.enqueueButton.Name = "enqueueButton";
            this.enqueueButton.Size = new System.Drawing.Size(153, 20);
            this.enqueueButton.TabIndex = 12;
            this.enqueueButton.Text = "Add To Queue";
            this.enqueueButton.UseVisualStyleBackColor = true;
            // 
            // urlBox
            // 
            this.urlBox.Enabled = false;
            this.urlBox.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.urlBox.Location = new System.Drawing.Point(6, 60);
            this.urlBox.Name = "urlBox";
            this.urlBox.Size = new System.Drawing.Size(153, 20);
            this.urlBox.TabIndex = 11;
            this.urlBox.Text = "Youtube URL";
            this.urlBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.urlBox.Enter += new System.EventHandler(this.urlBox_Enter);
            this.urlBox.Leave += new System.EventHandler(this.urlBox_Leave);
            // 
            // volumeText
            // 
            this.volumeText.AutoSize = true;
            this.volumeText.Location = new System.Drawing.Point(46, 335);
            this.volumeText.Name = "volumeText";
            this.volumeText.Size = new System.Drawing.Size(74, 13);
            this.volumeText.TabIndex = 5;
            this.volumeText.Text = "Volume: 100%";
            this.volumeText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // volumeBar
            // 
            this.volumeBar.Location = new System.Drawing.Point(6, 303);
            this.volumeBar.Maximum = 100;
            this.volumeBar.Name = "volumeBar";
            this.volumeBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.volumeBar.Size = new System.Drawing.Size(150, 45);
            this.volumeBar.TabIndex = 4;
            this.volumeBar.TickFrequency = 5;
            this.volumeBar.Value = 100;
            this.volumeBar.Scroll += new System.EventHandler(this.volumeBar_Scroll);
            // 
            // nextButton
            // 
            this.nextButton.Enabled = false;
            this.nextButton.Location = new System.Drawing.Point(129, 19);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(30, 27);
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = ">>";
            this.nextButton.UseVisualStyleBackColor = true;
            // 
            // playButton
            // 
            this.playButton.Enabled = false;
            this.playButton.Location = new System.Drawing.Point(42, 19);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(81, 27);
            this.playButton.TabIndex = 2;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            // 
            // backButton
            // 
            this.backButton.Enabled = false;
            this.backButton.Location = new System.Drawing.Point(6, 19);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(30, 27);
            this.backButton.TabIndex = 1;
            this.backButton.Text = "<<";
            this.backButton.UseVisualStyleBackColor = true;
            // 
            // queueBox
            // 
            this.queueBox.Enabled = false;
            this.queueBox.FormattingEnabled = true;
            this.queueBox.Location = new System.Drawing.Point(165, 19);
            this.queueBox.Name = "queueBox";
            this.queueBox.Size = new System.Drawing.Size(154, 329);
            this.queueBox.TabIndex = 0;
            // 
            // Window
            // 
            this.AcceptButton = this.loginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 370);
            this.Controls.Add(this.musicBox);
            this.Controls.Add(this.serverBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.userBox);
            this.Controls.Add(this.loginBox);
            this.Name = "Window";
            this.Text = " DaveBot Control";
            this.loginBox.ResumeLayout(false);
            this.loginBox.PerformLayout();
            this.userBox.ResumeLayout(false);
            this.serverBox.ResumeLayout(false);
            this.serverBox.PerformLayout();
            this.musicBox.ResumeLayout(false);
            this.musicBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.volumeBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox loginBox;
        private System.Windows.Forms.TextBox tokenBox;
        private System.Windows.Forms.Label tokenLabel;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.GroupBox userBox;
        private System.Windows.Forms.Button colorButton;
        private System.Windows.Forms.Button banButton;
        private System.Windows.Forms.Button kickButton;
        private System.Windows.Forms.Button deafenButton;
        private System.Windows.Forms.Button muteButton;
        public System.Windows.Forms.ListView memberList;
        private System.Windows.Forms.GroupBox serverBox;
        private System.Windows.Forms.Button addServerButton;
        private System.Windows.Forms.TextBox serverIdBox;
        public System.Windows.Forms.ListBox serverIdList;
        private System.Windows.Forms.GroupBox musicBox;
        private System.Windows.Forms.ListBox queueBox;
        private System.Windows.Forms.TrackBar volumeBar;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button enqueueButton;
        private System.Windows.Forms.TextBox urlBox;
        private System.Windows.Forms.Label volumeText;
    }
}