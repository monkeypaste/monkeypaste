using System;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSystemTrayVIew.xaml
    /// </summary>
    public partial class MpSystemTrayView : MpUserControl<MpSystemTrayViewModel> {
        private MpNotifyIcon _notifyIcon; // global class scope for the icon as it needs to exist foer the lifetime of the window

        public MpSystemTrayView() {
            // Create a manager (ExtendedNotifyIcon) for handling interaction with the notification icon and wire up events. 
            _notifyIcon = new MpNotifyIcon();
            _notifyIcon.MouseClick += ExtendedNotifyIcon_MouseClick1;
            _notifyIcon.TargetNotifyIcon.Text = "Monkey Paste";

            System.IO.Stream iconStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/MpWpfApp;component/Resources/Icons/monkey (2).ico")).Stream;
            _notifyIcon.TargetNotifyIcon.Icon = new System.Drawing.Icon(iconStream, System.Windows.Forms.SystemInformation.SmallIconSize);            

            InitializeComponent();

            InitContextMenu();
        }

        private void InitContextMenu() {
            _notifyIcon.TargetNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();


            _notifyIcon.TargetNotifyIcon.ContextMenuStrip.Items.Add(
                "Settings",
                MpBase64Images.SettingsIcon.ToBitmapSource().ToBitmap(), 
                SettingsMenuItem_Click);

            _notifyIcon.TargetNotifyIcon.ContextMenuStrip.Items.Add(
                "Help",
                MpBase64Images.QuestionMark.ToBitmapSource().ToBitmap(),
                HelpMenuItem_Click);

            _notifyIcon.TargetNotifyIcon.ContextMenuStrip.Items.Add("-");

            _notifyIcon.TargetNotifyIcon.ContextMenuStrip.Items.Add(
                "Exit",
                MpBase64Images.SignOutIcon.ToBitmapSource().Tint(System.Windows.Media.Colors.MidnightBlue,true).ToBitmap(),
                ExitMenuItem_Click);

            //foreach(var mi in _notifyIcon.TargetNotifyIcon.ContextMenuStrip.Items) {
            //    if(mi is System.Windows.Forms.ToolStripMenuItem tsmi) {
            //        tsmi.Size = new System.Drawing.Size(200, 70);
            //    }                
            //}
        }
        private void ExtendedNotifyIcon_MouseClick1(object sender, bool isRightMouseButtonClick) {
            if (isRightMouseButtonClick) {

                //var showSettingsMenuItem = new System.Windows.Forms.MenuItem(
                //    "Settings", new EventHandler(SettingsMenuItem_Click));

                //var showHelpMenuItem = new System.Windows.Forms.MenuItem(
                //    "Help", new EventHandler(HelpMenuItem_Click));

                //var exitMenuItem = new System.Windows.Forms.MenuItem(
                //    "Exit", new EventHandler(ExitMenuItem_Click));

                //var cm = new System.Windows.Forms.ContextMenu();
                //cm.MenuItems.Add(showSettingsMenuItem);
                //cm.MenuItems.Add(showHelpMenuItem);
                //cm.MenuItems.Add(new System.Windows.Forms.MenuItem("-"));
                //cm.MenuItems.Add(exitMenuItem);

                //_notifyIcon.TargetNotifyIcon.ContextMenu = cm;
                //System.Drawing.Point cm_loc = new System.Drawing.Point();
                //_notifyIcon.TargetNotifyIcon.ContextMenu.Show(_notifyIcon.TargetNotifyIcon, cm_loc);

            } else {
                //left mouse click

                MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            }
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e) {
            MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(null);
        }

        private void HelpMenuItem_Click(object sender, EventArgs e) {
            MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand.Execute(4);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e) {
            Application.Current.Shutdown();
        }

        private void SystemTrayTaskbarIcon_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.AppStatus));
            BindingContext.OnPropertyChanged(nameof(BindingContext.AccountStatus));
            //BindingContext.OnPropertyChanged(nameof(BindingContext.TotalItemCount));
            BindingContext.OnPropertyChanged(nameof(BindingContext.DbSizeInMbs));
        }
    }
}
