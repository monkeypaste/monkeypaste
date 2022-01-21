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
        public MpSystemTrayView() {
            InitializeComponent();
        }

        private void SystemTrayTaskbarIcon_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.AppStatus));
            BindingContext.OnPropertyChanged(nameof(BindingContext.AccountStatus));
            //BindingContext.OnPropertyChanged(nameof(BindingContext.TotalItemCount));
            BindingContext.OnPropertyChanged(nameof(BindingContext.DbSizeInMbs));
        }

        private void SystemTrayTaskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e) {
            MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
        }

        private void SystemTrayTaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
            //SystemTrayTaskbarIcon.IconSource = new ImageSource(new Uri(Application.Current.Resources["AppIcon"] as string));
        }
    }
}
