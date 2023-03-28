using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RmSolution.Deployment
{
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

        private void cmdNext_Click(object sender, EventArgs e)
        {
            if (tabControls.SelectedIndex < tabControls.TabCount - 1)
                tabControls.SelectedIndex++;
        }

        private void cmdPrev_Click(object sender, EventArgs e)
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

        private void cmdASpath_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                SelectedPath = txtASpath.Text
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtASpath.Text = dlg.SelectedPath;
        }

        private void cmdWSpath_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                SelectedPath = txtWSpath.Text
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtWSpath.Text = dlg.SelectedPath;
        }
    }
}
