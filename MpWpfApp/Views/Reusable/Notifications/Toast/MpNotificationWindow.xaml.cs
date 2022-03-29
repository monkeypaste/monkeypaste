using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public partial class MpNotificationWindow : Window, MpINotificationBalloonView {
        private static MpNotificationWindow _instance;
        public static MpNotificationWindow Instance => _instance;
        
        public MpNotificationWindow() {
            InitializeComponent();

            if(_instance == null) {
                _instance = this;
            }
        }
        
        
        private void SetWindowToBottomRightOfScreen() {
            Left = SystemParameters.WorkArea.Width - ActualWidth - 10;
            Top = SystemParameters.WorkArea.Height - ActualHeight - 10;
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
            if(MpBootstrapperViewModelBase.IsLoaded) {
                Hide();
            }
            
        }


        private void PropertiesButton_Click(object sender, RoutedEventArgs e) {
            HideWindow();
            MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(1);
        }

        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var mw = Application.Current.MainWindow as MpMainWindow;
            if (mw == null) {
                return;
            }
            var mwvm = mw.DataContext as MpMainWindowViewModel;
            if (mwvm == null) {
                return;
            }
            mwvm.ShowWindowCommand.Execute(null);
        }

        public void ShowWindow() {
            MpHelpers.RunOnMainThread(() => {
                this.Opacity = 0;
                this.Show();

                var fisb = this.Resources["FadeIn"] as Storyboard;
                Storyboard.SetTarget(fisb, this);

                fisb.Begin();
                if (DataContext != null) {
                    var nwvm = DataContext as MpNotificationCollectionViewModel;
                    nwvm.IsVisible = true;
                }
            });
            //tw.Show();
        }

        public void HideWindow() {
            MpHelpers.RunOnMainThread(() => {
                var fisb = this.Resources["FadeOut"] as Storyboard;
                Storyboard.SetTarget(fisb, this);
                fisb.Begin();

                if (DataContext != null && DataContext is MpNotificationCollectionViewModel nwvm) {
                    nwvm.NotificationQueue.Clear();
                    nwvm.OnPropertyChanged(nameof(nwvm.CurrentNotificationViewModel));
                    nwvm.IsVisible = false;
                    //var tw = this.GetVisualAncestor<Window>();
                    //tw.Opacity = 1;
                    //tw.Show();
                }
            });
        }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }

        private void ContentControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            SetWindowToBottomRightOfScreen();
        }
    }
}
