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
            this.tabAppSelect = new System.Windows.Forms.TabPage();
            this.grpApplications = new System.Windows.Forms.GroupBox();
            this.rmWebClient = new System.Windows.Forms.CheckBox();
            this.chkAppServer = new System.Windows.Forms.CheckBox();
            this.tabPaths = new System.Windows.Forms.TabPage();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdNext = new System.Windows.Forms.Button();
            this.cmdPrev = new System.Windows.Forms.Button();
            this.pnlHideTabs = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtASpath = new System.Windows.Forms.TextBox();
            this.txtWSpath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdASpath = new System.Windows.Forms.Button();
            this.cmdWSpath = new System.Windows.Forms.Button();
            this.tabComplete = new System.Windows.Forms.TabPage();
            this.tabRunning = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControls.SuspendLayout();
            this.tabAppSelect.SuspendLayout();
            this.grpApplications.SuspendLayout();
            this.tabPaths.SuspendLayout();
            this.pnlControls.SuspendLayout();
            this.tabRunning.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControls
            // 
            this.tabControls.Controls.Add(this.tabAppSelect);
            this.tabControls.Controls.Add(this.tabPaths);
            this.tabControls.Controls.Add(this.tabComplete);
            this.tabControls.Controls.Add(this.tabRunning);
            this.tabControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControls.Location = new System.Drawing.Point(0, 0);
            this.tabControls.Name = "tabControls";
            this.tabControls.SelectedIndex = 0;
            this.tabControls.Size = new System.Drawing.Size(624, 441);
            this.tabControls.TabIndex = 1;
            // 
            // tabAppSelect
            // 
            this.tabAppSelect.Controls.Add(this.grpApplications);
            this.tabAppSelect.Location = new System.Drawing.Point(4, 22);
            this.tabAppSelect.Name = "tabAppSelect";
            this.tabAppSelect.Padding = new System.Windows.Forms.Padding(3);
            this.tabAppSelect.Size = new System.Drawing.Size(616, 415);
            this.tabAppSelect.TabIndex = 0;
            this.tabAppSelect.Text = "1";
            // 
            // grpApplications
            // 
            this.grpApplications.Controls.Add(this.rmWebClient);
            this.grpApplications.Controls.Add(this.chkAppServer);
            this.grpApplications.Location = new System.Drawing.Point(8, 6);
            this.grpApplications.Name = "grpApplications";
            this.grpApplications.Size = new System.Drawing.Size(281, 74);
            this.grpApplications.TabIndex = 1;
            this.grpApplications.TabStop = false;
            this.grpApplications.Text = "Выберите приложения";
            // 
            // rmWebClient
            // 
            this.rmWebClient.AutoSize = true;
            this.rmWebClient.Checked = true;
            this.rmWebClient.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rmWebClient.Location = new System.Drawing.Point(7, 44);
            this.rmWebClient.Name = "rmWebClient";
            this.rmWebClient.Size = new System.Drawing.Size(254, 17);
            this.rmWebClient.TabIndex = 1;
            this.rmWebClient.Text = "Веб-сервер РМ Гео, клиентское приложение";
            this.rmWebClient.UseVisualStyleBackColor = true;
            // 
            // chkAppServer
            // 
            this.chkAppServer.AutoSize = true;
            this.chkAppServer.Checked = true;
            this.chkAppServer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAppServer.Location = new System.Drawing.Point(7, 20);
            this.chkAppServer.Name = "chkAppServer";
            this.chkAppServer.Size = new System.Drawing.Size(191, 17);
            this.chkAppServer.TabIndex = 0;
            this.chkAppServer.Text = "Сервер приложений РМ Солюшн";
            this.chkAppServer.UseVisualStyleBackColor = true;
            // 
            // tabPaths
            // 
            this.tabPaths.Controls.Add(this.cmdWSpath);
            this.tabPaths.Controls.Add(this.cmdASpath);
            this.tabPaths.Controls.Add(this.txtWSpath);
            this.tabPaths.Controls.Add(this.label2);
            this.tabPaths.Controls.Add(this.txtASpath);
            this.tabPaths.Controls.Add(this.label1);
            this.tabPaths.Location = new System.Drawing.Point(4, 22);
            this.tabPaths.Name = "tabPaths";
            this.tabPaths.Padding = new System.Windows.Forms.Padding(3);
            this.tabPaths.Size = new System.Drawing.Size(616, 415);
            this.tabPaths.TabIndex = 1;
            this.tabPaths.Text = "2";
            // 
            // pnlControls
            // 
            this.pnlControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlControls.Controls.Add(this.cmdClose);
            this.pnlControls.Controls.Add(this.cmdNext);
            this.pnlControls.Controls.Add(this.cmdPrev);
            this.pnlControls.Location = new System.Drawing.Point(213, 398);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Выберите путь для устанвки для сервера приложений:";
            // 
            // txtASpath
            // 
            this.txtASpath.Location = new System.Drawing.Point(11, 29);
            this.txtASpath.Name = "txtASpath";
            this.txtASpath.Size = new System.Drawing.Size(516, 20);
            this.txtASpath.TabIndex = 1;
            this.txtASpath.Text = "C:\\Program Files\\RmSolution\\Server";
            // 
            // txtWSpath
            // 
            this.txtWSpath.Location = new System.Drawing.Point(11, 68);
            this.txtWSpath.Name = "txtWSpath";
            this.txtWSpath.Size = new System.Drawing.Size(516, 20);
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
            // tabComplete
            // 
            this.tabComplete.Location = new System.Drawing.Point(4, 22);
            this.tabComplete.Name = "tabComplete";
            this.tabComplete.Size = new System.Drawing.Size(616, 415);
            this.tabComplete.TabIndex = 2;
            this.tabComplete.Text = "3";
            this.tabComplete.UseVisualStyleBackColor = true;
            // 
            // tabRunning
            // 
            this.tabRunning.Controls.Add(this.groupBox1);
            this.tabRunning.Location = new System.Drawing.Point(4, 22);
            this.tabRunning.Name = "tabRunning";
            this.tabRunning.Size = new System.Drawing.Size(616, 415);
            this.tabRunning.TabIndex = 3;
            this.tabRunning.Text = "4";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(8, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 366);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Выполнение установки";
            // 
            // RmSolution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
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
            this.tabAppSelect.ResumeLayout(false);
            this.grpApplications.ResumeLayout(false);
            this.grpApplications.PerformLayout();
            this.tabPaths.ResumeLayout(false);
            this.tabPaths.PerformLayout();
            this.pnlControls.ResumeLayout(false);
            this.tabRunning.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControls;
        private System.Windows.Forms.TabPage tabAppSelect;
        private System.Windows.Forms.GroupBox grpApplications;
        private System.Windows.Forms.CheckBox rmWebClient;
        private System.Windows.Forms.CheckBox chkAppServer;
        private System.Windows.Forms.TabPage tabPaths;
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
        private System.Windows.Forms.TabPage tabComplete;
        private System.Windows.Forms.TabPage tabRunning;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

