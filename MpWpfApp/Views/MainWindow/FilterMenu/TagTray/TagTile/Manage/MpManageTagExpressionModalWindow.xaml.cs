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
    /// Interaction logic for MpAssignHotkeyModalWindow.xaml
    /// </summary>
    public partial class MpManageTagExpressionModalWindow : Window {
        public MpManageTagExpressionModalWindow() {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void ShiftDownButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var btn = sender as Button;
            if(btn.DataContext == null) {
                return;
            }
            var pvm = (sender as FrameworkElement).DataContext as MpAnalyticItemPresetViewModel;
            btn.IsEnabled = pvm.Parent.ShiftPresetCommand.CanExecute(new object[] { 1, pvm });
            btn.Command = pvm.Parent.ShiftPresetCommand;
            btn.CommandParameter = new object[] { 1, pvm };
        }

        private void ShiftUpButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var btn = sender as Button;
            if (btn.DataContext == null) {
                return;
            }
            var pvm = (sender as FrameworkElement).DataContext as MpAnalyticItemPresetViewModel;
            btn.IsEnabled = pvm.Parent.ShiftPresetCommand.CanExecute(new object[] { -1, pvm }); 
            btn.Command = pvm.Parent.ShiftPresetCommand;
            btn.CommandParameter = new object[] { -1, pvm };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var aivm = DataContext as MpAnalyticItemViewModel;
            aivm.OnPropertyChanged(nameof(aivm.ItemIconBase64));
        }

        private void Window_Activated(object sender, EventArgs e) {
            //this is to update the shortcut key cell after an edit
            var aivm = DataContext as MpAnalyticItemViewModel;
            aivm.PresetViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.ShortcutViewModel)));
        }
    }
}
