using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public partial class MpNotificationWindow : Window, MpINotificationBalloonView {

        private static ObservableCollection<MpNotificationWindow> _windows;

        

        public MpNotificationWindow() {
            InitializeComponent();

            if(_windows == null) {
                _windows = new ObservableCollection<MpNotificationWindow>();
                _windows.CollectionChanged += _windows_CollectionChanged;
            }
        }

        private static void _windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindows();
        }

        private static void UpdateWindows() {
            foreach(var window in _windows.ToList()) {
                window.SetWindowToBottomRightOfScreen();
            }
        }
        private void SetWindowToBottomRightOfScreen() {
            if(!_windows.Contains(this)) {
                return;
            }
            Left = SystemParameters.WorkArea.Width - ActualWidth - 10;

            double pad = 10;

            double offsetY = _windows.Where(x => _windows.IndexOf(x) < _windows.IndexOf(this)).Sum(x => x.ActualHeight + pad);
            offsetY += ActualHeight + pad;
            Top = SystemParameters.WorkArea.Height - offsetY;
        }

        
        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            HideWindow(DataContext as MpNotificationViewModelBase);
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e) {
            UpdateWindows();
        }

        private void NotificationWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateWindows();
        }

        private void FadeIn_Completed(object sender, EventArgs e) {
           
        }

        private void FadeOut_Completed(object sender, EventArgs e) {
            //var nw = sender as MpNotificationWindow;
            _windows.Remove(this);
            this.Close();
        }


        private void PropertiesButton_Click(object sender, RoutedEventArgs e) {
            HideWindow(DataContext as MpNotificationViewModelBase);
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

        public void ShowWindow(object dc) {
            MpHelpers.RunOnMainThread(() => {
                var nw = new MpNotificationWindow();
                nw.DataContext = dc;
                nw.Opacity = 0;

                if (dc is MpNotificationViewModelBase nvmb && !nvmb.IsVisible) {
                    nvmb.IsVisible = true;
                }

                var fisb = nw.Resources["FadeIn"] as Storyboard;
                Storyboard.SetTarget(fisb, nw);

                //nw.Show();

                fisb.Begin();


                _windows.Add(nw);
            });
        }

        public void HideWindow(object nvmb) {
            MpHelpers.RunOnMainThread(() => {
                var nw = _windows.FirstOrDefault(x => x.DataContext == nvmb);
                var fisb = nw.Resources["FadeOut"] as Storyboard;
                if(nw !=null) {
                    Storyboard.SetTarget(fisb, nw);
                    fisb.Begin();
                }
                
            });
        }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }

        private void ContentControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            SetWindowToBottomRightOfScreen();
        }

        public void ShowWindow() {
            ShowWindow(DataContext as MpNotificationViewModelBase);
        }

        public void HideWindow() {
            HideWindow(DataContext as MpNotificationViewModelBase);
        }

    }
}
