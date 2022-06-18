using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpMessageBox.xaml
    /// </summary>
    public partial class MpMessageBox : Window {
        public static string ShowCustomMessageBox(string title = "sup", string caption = "Default caption", string iconPath = null, string[] buttons = null) {
            buttons = buttons == null || buttons.Length == 0 ? new string[]{ "Ok","Cancel"}:buttons;
            var mbvm = new MpMessageBoxViewModel() {
                Caption = caption,
                Title = title,
                IconPath = iconPath == null ? @"/Images/info.png" : iconPath,
                ButtonLabels = new ObservableCollection<string>(buttons)
            };
            var mb = new MpMessageBox();
            mb.DataContext = mbvm;
            mb.Topmost = true;
            mb.WindowState = WindowState.Maximized;
            MpMainWindowViewModel.Instance.IsShowingDialog = true;

            mb.ShowDialog();
            MpMainWindowViewModel.Instance.IsShowingDialog = false;

            return mbvm.Result;
        }

        public MpMessageBox() {
            InitializeComponent();
        }

        private void btn_Click(object sender, RoutedEventArgs e) {
            (DataContext as MpMessageBoxViewModel).Result = (sender as Button).Content as string;
            this.Close();
        }

        private void grdMessageBox_Loaded(object sender, RoutedEventArgs e) {
            var btnl = new List<Button>() { btn1, btn2, btn3 };
            var mbvm = DataContext as MpMessageBoxViewModel;

            for (int i = 0; i < btnl.Count; i++) {
                if(mbvm.ButtonLabels.Count - 1 <= i) {
                    btnl[i].Visibility = Visibility.Collapsed;
                } else {
                    btnl[i].Visibility = Visibility.Visible;
                }
            }
        }
    }
}
