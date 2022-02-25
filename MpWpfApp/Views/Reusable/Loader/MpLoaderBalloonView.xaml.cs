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
    public partial class MpLoaderBalloonView : MpUserControl<MpLoaderBalloonViewModel> {

        private static TaskbarIcon _tbi = null;

        private static MpLoaderBalloonView _instance;
        public static MpLoaderBalloonView Instance => _instance ?? (_instance = new MpLoaderBalloonView());

        private bool isClosing = false;


        public static void Init() {
            Application.Current.Dispatcher.Invoke(() => {
                _instance = new MpLoaderBalloonView(MpLoaderBalloonViewModel.Instance);

                _tbi = new TaskbarIcon();
                _tbi.ShowCustomBalloon(
                    _instance,
                    System.Windows.Controls.Primitives.PopupAnimation.Slide,
                    null);
            });
        }

        private MpLoaderBalloonView() {
            throw new Exception("no");
        }

        public MpLoaderBalloonView(MpLoaderBalloonViewModel lbvm) {            
            InitializeComponent();
            DataContext = lbvm;
            TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
        }

        

        public void CloseBalloon() {            
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            if(taskbarIcon == null) {
                return;
            }

            Storyboard sb = this.FindResource("FadeOut") as Storyboard;
            Storyboard.SetTarget(sb, this);
            sb.Begin();

            //taskbarIcon.CloseBalloon();
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

    }
}
