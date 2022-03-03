using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSystemTrayVIew.xaml
    /// </summary>
    public partial class MpSystemTrayView : MpUserControl<MpSystemTrayViewModel> {
        private MpNotifyIcon extendedNotifyIcon; // global class scope for the icon as it needs to exist foer the lifetime of the window

        public MpSystemTrayView() {
            // Create a manager (ExtendedNotifyIcon) for handling interaction with the notification icon and wire up events. 
            extendedNotifyIcon = new MpNotifyIcon();
            extendedNotifyIcon.MouseClick += ExtendedNotifyIcon_MouseClick1;
            extendedNotifyIcon.targetNotifyIcon.Text = "Monkey Paste";


            System.IO.Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/MpWpfApp;component/Resources/Icons/monkey (2).ico")).Stream;
            extendedNotifyIcon.targetNotifyIcon.Icon = new System.Drawing.Icon(iconStream);

            InitializeComponent();
        }

        private void ExtendedNotifyIcon_MouseClick1(object sender, bool e) {
            if(!e) {
                //left mouse click

                MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            }
        }

        private void SystemTrayTaskbarIcon_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.AppStatus));
            BindingContext.OnPropertyChanged(nameof(BindingContext.AccountStatus));
            //BindingContext.OnPropertyChanged(nameof(BindingContext.TotalItemCount));
            BindingContext.OnPropertyChanged(nameof(BindingContext.DbSizeInMbs));
        }
    }
}
