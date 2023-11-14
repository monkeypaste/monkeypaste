using Avalonia.Controls;
using WebViewControl;

namespace OutSysTest {
    public partial class MainWindow : Window {
        public MainWindow() {
            WebView.Settings.OsrEnabled = true;
            WebView.Settings.LogFile = "ceflog.txt";
            InitializeComponent();
        }
    }
}