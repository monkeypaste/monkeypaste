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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAssignHotkeyModalWindow.xaml
    /// </summary>
    public partial class MpAssignHotkeyModalWindow : Window {
        public MpAssignHotkeyModalWindow() {
            InitializeComponent();
        }

        public MpAssignHotkeyModalWindow(MpAssignShortcutModalWindowViewModel vm) {

        }


        private void Window_Closed(object sender, EventArgs e) {
            KeyGestureBehavior.StopListening();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var hwnd = new WindowInteropHelper(this).Handle;
            WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE) & ~WinApi.WS_SYSMENU);

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            (DataContext as MpAssignShortcutModalWindowViewModel).OkCommand.Execute(null);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
