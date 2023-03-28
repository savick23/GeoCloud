//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmSolution Form –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Deployment
{
    #region Using
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Xml.Linq;
    #endregion Using

    public partial class RmSolution : Form
    {
        public RmSolution()
        {
            InitializeComponent();
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        void cmdNext_Click(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex == 2)
                Setup();

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

        void Setup()
        {
            using (var ms = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.Deployment.setup.cab"))
            using (var reader = new CabReader(ms))
            {
                foreach (var app in XDocument.Parse(reader.ReadAllText("[Content_Types].xml")).Root.Element("applications").Elements())
                {
                    var appid = app.Attribute("id").Value;
                    int start = appid.Length + 1;
                    var path = app.Attribute("path").Value;
                    foreach(var file in reader.GetFiles(appid))
                    {
                        var filename = Path.Combine(path, file.Substring(start));
                        WriteFile(filename, reader.ReadAllBytes(file));
                    }
                }
            }
        }

        void WriteFile(string filename, byte[] content)
        {
            string path = string.Empty;
            string comma = string.Empty;
            foreach(var part in Path.GetDirectoryName(filename).Split(new char[] { '\\' }))
            {
                path += comma + part;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                comma = "\\";
            }
            File.WriteAllBytes(filename, content);
        }
    }
}
