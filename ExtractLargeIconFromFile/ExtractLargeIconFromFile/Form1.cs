using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtractLargeIconFromFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            foreach (var size in Enum.GetValues(typeof(ShellEx.IconSizeEnum)))
            {
                iconSizesComboBox.Items.Add(size);
            }
            iconSizesComboBox.SelectedItem = ShellEx.IconSizeEnum.ExtraLargeIcon;
        }

        private void chooseFileButton_Click(object sender, EventArgs e)
        {
            var size = (ShellEx.IconSizeEnum)iconSizesComboBox.SelectedItem;
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fname = ofd.FileName;
                labelFilePath.Text = fname;
                pictureBox1.Image = ShellEx.GetBitmapFromFilePath(fname, size);
            }
        }

        private void chooseFolderButton_Click(object sender, EventArgs e)
        {
            var size = (ShellEx.IconSizeEnum)iconSizesComboBox.SelectedItem;
            var ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fname = ofd.SelectedPath;
                labelFilePath.Text = fname;
                pictureBox1.Image = ShellEx.GetBitmapFromFolderPath(fname, size);
            }
        }
    }
}
