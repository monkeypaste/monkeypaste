using MonkeyPaste;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp { 
    /// <summary>
    /// Interaction logic for MpFindReplaceToolbarView.xaml
    /// </summary>
    public partial class MpFindReplaceToolbarView : MpUserControl<MpClipTileViewModel> {
        public MpFindReplaceToolbarView() {
            InitializeComponent();
        }

        private void ClearFindTextButton_Click(object sender, RoutedEventArgs e) {
            BindingContext.FindText = string.Empty;
        }

        private void ClearReplaceTextButton_Click(object sender, RoutedEventArgs e) {
            BindingContext.ReplaceText = string.Empty;
        }

        private void ComboBox_PreviewKeyUp(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                e.Handled = true;
                UserClick_UpdateRecents(null, null);
                //BindingContext.PerformInitialFindAndOrReplaceCommand.Execute(null);
            }
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if (sender == FindComboBox) {
                BindingContext.IsFindTextBoxFocused = true;
            } else if (sender == ReplaceComboBox) {
                BindingContext.IsReplaceTextBoxFocused = true;
            }

            if (BindingContext.IsFindTextBoxFocused || BindingContext.IsReplaceTextBoxFocused) {
                //Keyboard.Focus(sender as IInputElement);
                MpMainWindowViewModel.Instance.IsAnyTextBoxFocused = true;
            }
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if (sender == FindComboBox) {
                BindingContext.IsFindTextBoxFocused = false;
            } else if (sender == ReplaceComboBox) {
                BindingContext.IsReplaceTextBoxFocused = false;
            }

            if (BindingContext.IsFindTextBoxFocused && BindingContext.IsReplaceTextBoxFocused) {
                MpMainWindowViewModel.Instance.IsAnyTextBoxFocused = false;
            }
        }

        private void FindComboBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue == false || BindingContext == null) {
                return;
            }
            var cmb = sender as ComboBox;
            cmb.Focus();

            var ctv = this.GetVisualAncestor<MpClipTileView>();
            var cv = ctv.GetVisualDescendent<MpContentView>();
            if(!cv.Rtb.Selection.IsEmpty) {
                BindingContext.FindText = cv.Rtb.Selection.Text;
            }
        }

        private void UserClick_UpdateRecents(object sender, RoutedEventArgs e) {
            BindingContext.UpdateFindAndReplaceRecentsCommand.Execute(null);
        }
    }
}
