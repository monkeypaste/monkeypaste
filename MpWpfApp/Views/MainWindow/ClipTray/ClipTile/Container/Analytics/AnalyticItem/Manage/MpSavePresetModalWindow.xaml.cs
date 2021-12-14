using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAssignHotkeyModalWindow.xaml
    /// </summary>
    public partial class MpSavePresetModalWindow : Window {
        public MpSavePresetModalWindow() {
            InitializeComponent();
        }

        public MpSavePresetModalWindow(string defaultText) {
            InitializeComponent();
            ResponseTextBox.Text = defaultText;
        }

        public string ResponseText {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void ResponseTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            OkButton.IsEnabled = !string.IsNullOrWhiteSpace(ResponseTextBox.Text);
        }

        private void ResponseTextBox_Loaded(object sender, RoutedEventArgs e) {
            ResponseTextBox.Focus();
            ResponseTextBox.SelectAll();
        }
    }
}
