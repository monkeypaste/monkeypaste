using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using MonkeyPaste;

namespace MpWpfApp {
    public partial class MpNotificationBalloonView : 
        MpUserControl<MpNotificationCollectionViewModel>, MpINotificationBalloonView {


        public MpNotificationBalloonView() {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            HideWindow();
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e) {
            HideWindow();
            MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(1);
        }

        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var mw = Application.Current.MainWindow as MpMainWindow;
            if(mw == null) {
                return;
            }
            var mwvm = mw.DataContext as MpMainWindowViewModel;
            if(mwvm == null) {
                return;
            }
            mwvm.ShowWindowCommand.Execute(null);
        }

        private void ContentControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var cc = sender as ContentControl;
            
        }


        public void ShowWindow() {
            var tw = this.GetVisualAncestor<Window>();
            tw.Show();
        }

        public void HideWindow() {
            if (BindingContext != null) {
                BindingContext.IsVisible = false;
            }
        }
    }
}
