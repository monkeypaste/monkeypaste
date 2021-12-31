using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpMessageBox.xaml
    /// </summary>
    public partial class MpTextBoxMessageBox : Window {
        public static string ShowCustomMessageBox(string text) {
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
            var mb = new MpTextBoxMessageBox();
            mb.MsgBoxTextBox.Text = text;
            mb.Topmost = true;
            mb.WindowState = WindowState.Maximized;
            var result = mb.ShowDialog();
            MpMainWindowViewModel.Instance.IsShowingDialog = false;
            return mb.MsgBoxTextBox.Text;
        }

        public MpTextBoxMessageBox() {
            InitializeComponent();
        }

        private void btn_Click(object sender, RoutedEventArgs e) {
            if(string.IsNullOrWhiteSpace(MsgBoxTextBox.Text)) {
                MsgBoxTextBox.BorderBrush = Brushes.Red;
                return;
            }
            this.Close();
        }

        private void btn2_Click(object sender, RoutedEventArgs e) {
            MsgBoxTextBox.Text = null;
            Close();
        }


        private void MsgBoxTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            MsgBoxTextBox.BorderBrush = string.IsNullOrWhiteSpace(MsgBoxTextBox.Text) ? Brushes.Red : Brushes.Black;
        }
    }
}
