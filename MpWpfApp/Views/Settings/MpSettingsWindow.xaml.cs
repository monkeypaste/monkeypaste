using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSettingsWindow.xaml
    /// </summary>
    public partial class MpSettingsWindow : MpWindow<MpSettingsWindowViewModel> {
        public static async Task ShowDialog(int tabToShow, object args = null) {
            await MpHelpers.RunOnMainThreadAsync(async () => {
                var swvm = new MpSettingsWindowViewModel();
                await swvm.InitializeAsync(tabToShow, args);
                var sw = new MpSettingsWindow();
                sw.DataContext = swvm;

                MpMainWindowViewModel.Instance.IsShowingDialog = true;
                var result = sw.ShowDialog();
                MpMainWindowViewModel.Instance.IsShowingDialog = false;

                if (result.HasValue) {
                    if (result.Value) {
                        // clicked save
                        swvm.SaveSettingsCommand.Execute(null);
                        return;
                    }
                    // clicked cancel
                    swvm.CancelSettingsCommand.Execute(null);
                    return;
                }
                //clicked restore defaults (may no implement
                swvm.ResetSettingsCommand.Execute(null);
                return;
            });
        }

        public MpSettingsWindow() : base() {
            InitializeComponent();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) {
            var swvm = DataContext as MpSettingsWindowViewModel;

            var result = MessageBox.Show("Are you sure you want to reset all settings to default?", "Reset All", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes) {
                swvm.ResetSettingsCommand.Execute(null);
                Close();
                return;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            var swvm = DataContext as MpSettingsWindowViewModel;

            swvm.SaveSettingsCommand.Execute(null);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            var swvm = DataContext as MpSettingsWindowViewModel;

            swvm.CancelSettingsCommand.Execute(null);
            Close();
        }

        private void MpWindow_StateChanged(object sender, EventArgs e) {
            //if(this.WindowState == WindowState.Minimized) {
            //    MpMainWindowViewModel.Instance.IsShowingDialog = false;
            //}
        }
    }
}
