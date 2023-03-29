namespace RmSolution.Deployment
{
    partial class RmSolution
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
            this.tabControls = new System.Windows.Forms.TabControl();
            this.tab_1_apps = new System.Windows.Forms.TabPage();
            this.optUninstall = new System.Windows.Forms.RadioButton();
            this.optInstall = new System.Windows.Forms.RadioButton();
            this.grpApplications = new System.Windows.Forms.GroupBox();
            this.chkWIS = new System.Windows.Forms.CheckBox();
            this.chkAS = new System.Windows.Forms.CheckBox();
            this.tab_2_paths = new System.Windows.Forms.TabPage();
            this.cmdWSpath = new System.Windows.Forms.Button();
            this.cmdASpath = new System.Windows.Forms.Button();
            this.txtWSpath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtASpath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tab_3_sets = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDbAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tab_4_complete = new System.Windows.Forms.TabPage();
            this.lblMessage = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdNext = new System.Windows.Forms.Button();
            this.cmdPrev = new System.Windows.Forms.Button();
            this.pnlHideTabs = new System.Windows.Forms.Panel();
            this.txtDbPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDbUser = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDbPass = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControls.SuspendLayout();
            this.tab_1_apps.SuspendLayout();
            this.grpApplications.SuspendLayout();
            this.tab_2_paths.SuspendLayout();
            this.tab_3_sets.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tab_4_complete.SuspendLayout();
            this.pnlControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControls
            // 
            this.tabControls.Controls.Add(this.tab_1_apps);
            this.tabControls.Controls.Add(this.tab_2_paths);
            this.tabControls.Controls.Add(this.tab_3_sets);
            this.tabControls.Controls.Add(this.tab_4_complete);
            this.tabControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControls.Location = new System.Drawing.Point(0, 0);
            this.tabControls.Name = "tabControls";
            this.tabControls.SelectedIndex = 0;
            this.tabControls.Size = new System.Drawing.Size(534, 361);
            this.tabControls.TabIndex = 1;
            // 
            // tab_1_apps
            // 
            this.tab_1_apps.Controls.Add(this.optUninstall);
            this.tab_1_apps.Controls.Add(this.optInstall);
            this.tab_1_apps.Controls.Add(this.grpApplications);
            this.tab_1_apps.Location = new System.Drawing.Point(4, 22);
            this.tab_1_apps.Name = "tab_1_apps";
            this.tab_1_apps.Padding = new System.Windows.Forms.Padding(3);
            this.tab_1_apps.Size = new System.Drawing.Size(526, 335);
            this.tab_1_apps.TabIndex = 0;
            this.tab_1_apps.Text = "1";
            // 
            // optUninstall
            // 
            this.optUninstall.AutoSize = true;
            this.optUninstall.Location = new System.Drawing.Point(15, 109);
            this.optUninstall.Name = "optUninstall";
            this.optUninstall.Size = new System.Drawing.Size(68, 17);
            this.optUninstall.TabIndex = 3;
            this.optUninstall.Text = "Удалить";
            this.optUninstall.UseVisualStyleBackColor = true;
            // 
            // optInstall
            // 
            this.optInstall.AutoSize = true;
            this.optInstall.Checked = true;
            this.optInstall.Location = new System.Drawing.Point(15, 86);
            this.optInstall.Name = "optInstall";
            this.optInstall.Size = new System.Drawing.Size(85, 17);
            this.optInstall.TabIndex = 2;
            this.optInstall.TabStop = true;
            this.optInstall.Text = "Установить";
            this.optInstall.UseVisualStyleBackColor = true;
            // 
            // grpApplications
            // 
            this.grpApplications.Controls.Add(this.chkWIS);
            this.grpApplications.Controls.Add(this.chkAS);
            this.grpApplications.Location = new System.Drawing.Point(8, 6);
            this.grpApplications.Name = "grpApplications";
            this.grpApplications.Size = new System.Drawing.Size(281, 74);
            this.grpApplications.TabIndex = 1;
            this.grpApplications.TabStop = false;
            this.grpApplications.Text = "Выберите приложения";
            // 
            // chkWIS
            // 
            this.chkWIS.AutoSize = true;
            this.chkWIS.Checked = true;
            this.chkWIS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWIS.Location = new System.Drawing.Point(7, 44);
            this.chkWIS.Name = "chkWIS";
            this.chkWIS.Size = new System.Drawing.Size(254, 17);
            this.chkWIS.TabIndex = 1;
            this.chkWIS.Tag = "WIS";
            this.chkWIS.Text = "Веб-сервер РМ Гео, клиентское приложение";
            this.chkWIS.UseVisualStyleBackColor = true;
            // 
            // chkAS
            // 
            this.chkAS.AutoSize = true;
            this.chkAS.Checked = true;
            this.chkAS.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAS.Location = new System.Drawing.Point(7, 20);
            this.chkAS.Name = "chkAS";
            this.chkAS.Size = new System.Drawing.Size(191, 17);
            this.chkAS.TabIndex = 0;
            this.chkAS.Tag = "AS";
            this.chkAS.Text = "Сервер приложений РМ Солюшн";
            this.chkAS.UseVisualStyleBackColor = true;
            // 
            // tab_2_paths
            // 
            this.tab_2_paths.Controls.Add(this.cmdWSpath);
            this.tab_2_paths.Controls.Add(this.cmdASpath);
            this.tab_2_paths.Controls.Add(this.txtWSpath);
            this.tab_2_paths.Controls.Add(this.label2);
            this.tab_2_paths.Controls.Add(this.txtASpath);
            this.tab_2_paths.Controls.Add(this.label1);
            this.tab_2_paths.Location = new System.Drawing.Point(4, 22);
            this.tab_2_paths.Name = "tab_2_paths";
            this.tab_2_paths.Padding = new System.Windows.Forms.Padding(3);
            this.tab_2_paths.Size = new System.Drawing.Size(526, 335);
            this.tab_2_paths.TabIndex = 1;
            this.tab_2_paths.Text = "2";
            // 
            // cmdWSpath
            // 
            this.cmdWSpath.Location = new System.Drawing.Point(533, 68);
            this.cmdWSpath.Name = "cmdWSpath";
            this.cmdWSpath.Size = new System.Drawing.Size(75, 23);
            this.cmdWSpath.TabIndex = 5;
            this.cmdWSpath.Text = "Выбрать";
            this.cmdWSpath.UseVisualStyleBackColor = true;
            this.cmdWSpath.Click += new System.EventHandler(this.cmdWSpath_Click);
            // 
            // cmdASpath
            // 
            this.cmdASpath.Location = new System.Drawing.Point(533, 26);
            this.cmdASpath.Name = "cmdASpath";
            this.cmdASpath.Size = new System.Drawing.Size(75, 23);
            this.cmdASpath.TabIndex = 4;
            this.cmdASpath.Text = "Выбрать";
            this.cmdASpath.UseVisualStyleBackColor = true;
            this.cmdASpath.Click += new System.EventHandler(this.cmdASpath_Click);
            // 
            // txtWSpath
            // 
            this.txtWSpath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWSpath.Location = new System.Drawing.Point(11, 68);
            this.txtWSpath.Name = "txtWSpath";
            this.txtWSpath.Size = new System.Drawing.Size(507, 20);
            this.txtWSpath.TabIndex = 3;
            this.txtWSpath.Text = "C:\\Program Files\\RmSolution\\Cyclops";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(243, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Выберите путь для устанвки для Веб-сервера:";
            // 
            // txtASpath
            // 
            this.txtASpath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtASpath.Location = new System.Drawing.Point(11, 28);
            this.txtASpath.Name = "txtASpath";
            this.txtASpath.Size = new System.Drawing.Size(507, 20);
            this.txtASpath.TabIndex = 1;
            this.txtASpath.Text = "C:\\Program Files\\RmSolution\\Server";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Выберите путь для устанвки для сервера приложений:";
            // 
            // tab_3_sets
            // 
            this.tab_3_sets.Controls.Add(this.groupBox1);
            this.tab_3_sets.Location = new System.Drawing.Point(4, 22);
            this.tab_3_sets.Name = "tab_3_sets";
            this.tab_3_sets.Size = new System.Drawing.Size(526, 335);
            this.tab_3_sets.TabIndex = 3;
            this.tab_3_sets.Text = "3";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDbPass);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtDbUser);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtDbPort);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtDbAddress);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(8, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 366);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки базы данных (SQL-сервера):";
            // 
            // txtDbAddress
            // 
            this.txtDbAddress.Location = new System.Drawing.Point(119, 20);
            this.txtDbAddress.Name = "txtDbAddress";
            this.txtDbAddress.Size = new System.Drawing.Size(196, 20);
            this.txtDbAddress.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Адрес сервера БД:";
            // 
            // tab_4_complete
            // 
            this.tab_4_complete.Controls.Add(this.lblMessage);
            this.tab_4_complete.Controls.Add(this.progressBar);
            this.tab_4_complete.Location = new System.Drawing.Point(4, 22);
            this.tab_4_complete.Name = "tab_4_complete";
            this.tab_4_complete.Size = new System.Drawing.Size(526, 335);
            this.tab_4_complete.TabIndex = 2;
            this.tab_4_complete.Text = "4";
            this.tab_4_complete.UseVisualStyleBackColor = true;
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.Location = new System.Drawing.Point(9, 85);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(509, 23);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Прогрес";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(8, 55);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(510, 23);
            this.progressBar.TabIndex = 1;
            // 
            // pnlControls
            // 
            this.pnlControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlControls.Controls.Add(this.cmdClose);
            this.pnlControls.Controls.Add(this.cmdNext);
            this.pnlControls.Controls.Add(this.cmdPrev);
            this.pnlControls.Location = new System.Drawing.Point(123, 318);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(399, 31);
            this.pnlControls.TabIndex = 4;
            // 
            // cmdClose
            // 
            this.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdClose.Location = new System.Drawing.Point(267, 3);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(126, 23);
            this.cmdClose.TabIndex = 2;
            this.cmdClose.Text = "Закрыть";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdNext
            // 
            this.cmdNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNext.Location = new System.Drawing.Point(135, 3);
            this.cmdNext.Name = "cmdNext";
            this.cmdNext.Size = new System.Drawing.Size(126, 23);
            this.cmdNext.TabIndex = 1;
            this.cmdNext.Text = "Далее";
            this.cmdNext.UseVisualStyleBackColor = true;
            this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
            // 
            // cmdPrev
            // 
            this.cmdPrev.Enabled = false;
            this.cmdPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdPrev.Location = new System.Drawing.Point(3, 3);
            this.cmdPrev.Name = "cmdPrev";
            this.cmdPrev.Size = new System.Drawing.Size(126, 23);
            this.cmdPrev.TabIndex = 0;
            this.cmdPrev.Text = "Назад";
            this.cmdPrev.UseVisualStyleBackColor = true;
            this.cmdPrev.Click += new System.EventHandler(this.cmdPrev_Click);
            // 
            // pnlHideTabs
            // 
            this.pnlHideTabs.Location = new System.Drawing.Point(0, 0);
            this.pnlHideTabs.Name = "pnlHideTabs";
            this.pnlHideTabs.Size = new System.Drawing.Size(640, 20);
            this.pnlHideTabs.TabIndex = 2;
            // 
            // txtDbPort
            // 
            this.txtDbPort.Location = new System.Drawing.Point(378, 20);
            this.txtDbPort.Name = "txtDbPort";
            this.txtDbPort.Size = new System.Drawing.Size(132, 20);
            this.txtDbPort.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(324, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Порт:";
            // 
            // txtDbUser
            // 
            this.txtDbUser.Location = new System.Drawing.Point(119, 46);
            this.txtDbUser.Name = "txtDbUser";
            this.txtDbUser.Size = new System.Drawing.Size(196, 20);
            this.txtDbUser.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Пользователь:";
            // 
            // txtDbPass
            // 
            this.txtDbPass.Location = new System.Drawing.Point(378, 46);
            this.txtDbPass.Name = "txtDbPass";
            this.txtDbPass.PasswordChar = '*';
            this.txtDbPass.Size = new System.Drawing.Size(132, 20);
            this.txtDbPass.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(324, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Пароль:";
            // 
            // RmSolution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 361);
            this.Controls.Add(this.pnlHideTabs);
            this.Controls.Add(this.pnlControls);
            this.Controls.Add(this.tabControls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RmSolution";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Установка сервера приложений РМ Солюшн";
            this.tabControls.ResumeLayout(false);
            this.tab_1_apps.ResumeLayout(false);
            this.tab_1_apps.PerformLayout();
            this.grpApplications.ResumeLayout(false);
            this.grpApplications.PerformLayout();
            this.tab_2_paths.ResumeLayout(false);
            this.tab_2_paths.PerformLayout();
            this.tab_3_sets.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tab_4_complete.ResumeLayout(false);
            this.pnlControls.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControls;
        private System.Windows.Forms.TabPage tab_1_apps;
        private System.Windows.Forms.GroupBox grpApplications;
        private System.Windows.Forms.CheckBox chkWIS;
        private System.Windows.Forms.CheckBox chkAS;
        private System.Windows.Forms.TabPage tab_2_paths;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdNext;
        private System.Windows.Forms.Button cmdPrev;
        private System.Windows.Forms.Panel pnlHideTabs;
        private System.Windows.Forms.Button cmdWSpath;
        private System.Windows.Forms.Button cmdASpath;
        private System.Windows.Forms.TextBox txtWSpath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtASpath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tab_4_complete;
        private System.Windows.Forms.TabPage tab_3_sets;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton optUninstall;
        private System.Windows.Forms.RadioButton optInstall;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtDbAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDbPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDbPass;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDbUser;
        private System.Windows.Forms.Label label5;
    }
}

