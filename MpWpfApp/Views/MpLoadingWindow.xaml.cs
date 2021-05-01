using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MpLoadingWindow.xaml
    /// </summary>
    public partial class MpLoadingWindow : Window {
        private static Window _window = null;

        public static bool IsOpen = false;

        public MpLoadingWindow() {
            InitializeComponent();
        }
        public static void ShowLoadingWindow(string label = "") {
            (Application.Current.MainWindow.DataContext as MpMainWindowViewModel).IsShowingDialog = true;
            IsOpen = true;
            (Application.Current.MainWindow.DataContext as MpMainWindowViewModel).ProcessingVisibility = Visibility.Visible;
        }

        public static void HideLoadingWindow() {
            (Application.Current.MainWindow.DataContext as MpMainWindowViewModel).IsShowingDialog = false;
            IsOpen = false;
            (Application.Current.MainWindow.DataContext as MpMainWindowViewModel).ProcessingVisibility = Visibility.Hidden;
        }
    }
}
