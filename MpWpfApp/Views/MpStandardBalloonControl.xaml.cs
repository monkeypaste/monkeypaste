using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MpWpfApp {
    public partial class MpStandardBalloonControl : UserControl {
        private bool isClosing = false;

        #region BalloonText dependency property
        //public static readonly DependencyProperty BalloonTextProperty =
        //    DependencyProperty.Register(
        //        "BalloonText",
        //        typeof(string),
        //        typeof(MpBalloonControl),
        //        new FrameworkPropertyMetadata("Content"));
        //public string BalloonText {
        //    get { return (string)GetValue(BalloonTextProperty); }
        //    set { SetValue(BalloonTextProperty, value); }
        //}

        //public static readonly DependencyProperty BalloonTitleProperty =
        //    DependencyProperty.Register(
        //        "BalloonTitle",
        //        typeof(string),
        //        typeof(MpBalloonControl),
        //        new FrameworkPropertyMetadata("Title"));
        //public string BalloonTitle {
        //    get { return (string)GetValue(BalloonTitleProperty); }
        //    set { SetValue(BalloonTitleProperty, value); }
        //}

        #endregion

        public MpStandardBalloonControl() {
            InitializeComponent();
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        public MpStandardBalloonControl(string title, string content, string bitmapSourcePath) {
            InitializeComponent();

            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
            DataContext = new MpStandardBalloonViewModel(title, content, bitmapSourcePath);
        }
        /// <summary>
        /// By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent"/>
        /// and setting the "Handled" property to true, we suppress the popup
        /// from being closed in order to display the custom fade-out animation.
        /// </summary>
        private void OnBalloonClosing(object sender, RoutedEventArgs e) {
            e.Handled = true; //suppresses the popup from being closed immediately
            isClosing = true;
        }


        /// <summary>
        /// Resolves the <see cref="TaskbarIcon"/> that displayed
        /// the balloon and requests a close action.
        /// </summary>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e) {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        /// <summary>
        /// If the users hovers over the balloon, we don't close it.
        /// </summary>
        private void grid_MouseEnter(object sender, MouseEventArgs e) {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (isClosing) {
                return;
            }

            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.ResetBalloonCloseTimer();
        }

        /// <summary>
        /// Closes the popup once the fade-out animation completed.
        /// The animation was triggered in XAML through the attached
        /// BalloonClosing event.
        /// </summary>
        private void OnFadeOutCompleted(object sender, EventArgs e) {
            Popup pp = (Popup)Parent;
            pp.IsOpen = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            //the tray icon assigned this attached property to simplify access
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

        private void PropertiesButton_Click(object sender, RoutedEventArgs e) {
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
            MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(1);
        }

        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var mwvm = (Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel;
            mwvm.ShowWindowCommand.Execute(null);
        }
    }
}
