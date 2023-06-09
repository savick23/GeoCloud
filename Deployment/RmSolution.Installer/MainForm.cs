﻿//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmSolution Form –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Deployment
{
    #region Using
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;
    #endregion Using

    public partial class RmSolution : Form
    {
        const string DATASOURCE = @"(?<=""datasource"": "".*Data Source=).*?(?=;.*"")";
        const string USERID = @"(?<=""datasource"": "".*User ID=).*?(?=;.*"")";
        const string PASSWORD = @"(?<=""datasource"": "".*Password=).*?(?=;.*"")";

        public RmSolution()
        {
            InitializeComponent();
            using (var ms = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.Deployment.setup.cab"))
            using (var reader = new CabReader(ms))
            {
                foreach (var app in XDocument.Parse(reader.ReadAllText("[Content_Types].xml")).Root.Element("applications").Elements())
                    if (app.Attribute("id").Value == "AS")
                    {
                        var cfgfile = "AS\\" + app.Attribute("config")?.Value;
                        if (cfgfile != null)
                        {
                            var cfg= reader.ReadAllText(cfgfile);
                            var ds = Regex.Match(cfg, DATASOURCE).Value.Split(new char[] { ',' });
                            txtDbAddress.Text = ds[0];
                            txtDbPort.Text = ds.Length == 1 ? "1433" : ds[1];
                            txtDbUser.Text = Regex.Match(cfg, USERID).Value;
                            txtDbPass.Text = Regex.Match(cfg, PASSWORD).Value;
                        }
                    }
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        void Message(string text)
        {
            if (InvokeRequired)
                Invoke(new Action(() => Message(text)));
            else
                lblMessage.Text = text;
        }

        void cmdNext_Click(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex == 2)
                Install(Message).ConfigureAwait(false);

            if (tabControls.SelectedIndex < tabControls.TabCount - 1)
                tabControls.SelectedIndex++;
        }

        void cmdPrev_Click(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex > 0)
                tabControls.SelectedIndex--;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            cmdPrev.Enabled = tabControls.SelectedIndex > 0;
            cmdNext.Enabled = tabControls.SelectedIndex < tabControls.TabCount - 1;
            base.OnPaint(e);
        }

        void cmdASpath_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                SelectedPath = txtASpath.Text
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtASpath.Text = dlg.SelectedPath;
        }

        void cmdWSpath_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                SelectedPath = txtWSpath.Text
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtWSpath.Text = dlg.SelectedPath;
        }

        async Task Install(Action<string> logger) => await Task.Run(() =>
        {
            using (var ms = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.Deployment.setup.cab"))
            using (var reader = new CabReader(ms))
            {
                foreach (var app in XDocument.Parse(reader.ReadAllText("[Content_Types].xml")).Root.Element("applications").Elements())
                {
                    var appid = app.Attribute("id").Value;
                    if (appid == chkAS.Tag.ToString() && chkAS.Checked || appid == chkWIS.Tag.ToString() && chkWIS.Checked)
                    {
                        int start = appid.Length + 1;
                        var cfgfile = app.Attribute("config") == null ? null : appid + "\\" + app.Attribute("config").Value;
                        var path = app.Attribute("path").Value;
                        foreach (var file in reader.GetFiles(appid))
                        {
                            var filename = Path.Combine(path, file.Substring(start));
                            logger.Invoke(filename);
                            var content = reader.ReadAllBytes(file);
                            if (file == cfgfile) content = WriteConfig(content);
                            WriteFile(filename, content);
                        }
                        if (!path.EndsWith("\\")) path += "\\";
                        var srvc = app.Element("service");
                        if (srvc != null)
                        {
                            var srvname = srvc.Attribute("id").Value;
                            var cmd = string.Concat("create ",
                                srvname, " start=",
                                srvc.Attribute("start").Value, " error=",
                                srvc.Attribute("error").Value, " binpath=\"",
                                srvc.Attribute("binpath").Value.Replace("%TARGET%", path), "\" obj=",
                                srvc.Attribute("obj").Value, " displayname=\"",
                                srvc.Attribute("displayname").Value, "\"");

                            Shell.Run("sc", cmd);

                            if (srvc.Attribute("description") != null)
                            {
                                cmd = string.Concat("description ", srvname, " \"", srvc.Attribute("description").Value, "\"");
                                Shell.Run("sc", cmd);
                            }
                            if (srvc.Attribute("depend") != null)
                            {
                                cmd = string.Concat("config ", srvname, " depend= ", srvc.Attribute("depend").Value);
                                Shell.Run("sc", cmd);
                            }
                        }
                    }
                }
                logger.Invoke("Успешно!");
            }
        });

        byte[] WriteConfig(byte[] content)
        {
            string datasource = null, username = null, password = null;
            var t = Invoke(new Action(() =>
            {
                datasource = txtDbAddress.Text;
                username = txtDbUser.Text;
                password = txtDbPass.Text;
            }));
            var cfg = Encoding.UTF8.GetString(content);
            cfg = Regex.Replace(Regex.Replace(Regex.Replace(cfg, PASSWORD, password), USERID, username), DATASOURCE, datasource);
            return Encoding.UTF8.GetBytes(cfg);
        }

        void WriteFile(string filename, byte[] content)
        {
            string path = string.Empty;
            string comma = string.Empty;
            foreach (var part in Path.GetDirectoryName(filename).Split(new char[] { '\\' }))
            {
                path += comma + part;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                comma = "\\";
            }
            File.WriteAllBytes(filename, content);
        }
    }
}