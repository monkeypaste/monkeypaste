using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public partial class MpToastWindow : MpWindow<MpNotificationBalloonViewModel> {
        private static MpToastWindow _instance;
        public static MpToastWindow Instance => _instance;
        
        public MpToastWindow() {
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

        private void uiMainNotifyWindow_Loaded(object sender, RoutedEventArgs e) {
            SetWindowToBottomRightOfScreen();
        }
    }
}
