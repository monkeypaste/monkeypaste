using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public partial class MpNotificationWindow : Window {
        private static MpNotificationWindow _instance;
        public static MpNotificationWindow Instance => _instance;
        
        public MpNotificationWindow() {
            InitializeComponent();

            SetWindowToBottomRightOfScreen();

            if(_instance == null) {
                _instance = this;
            }
        }
        
        
        private void SetWindowToBottomRightOfScreen() {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height - 10;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e) {
            SetWindowToBottomRightOfScreen();
        }

        private void NotificationWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            SetWindowToBottomRightOfScreen();
        }

        private void Storyboard_Completed(object sender, EventArgs e) {
            Hide();
        }
    }
}
