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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpPasteTemplateToolbarView.xaml
    /// </summary>
    public partial class MpPasteTemplateToolbarView : MpUserControl<MpTemplateCollectionViewModel> {
        RichTextBox _activeRtb;

        public MpPasteTemplateToolbarView() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }
        public void SetActiveRtb(RichTextBox trtb) {
            if (_activeRtb == trtb) {
                return;
            }
            _activeRtb = trtb;
            var rtbvm = _activeRtb.DataContext as MpContentItemViewModel;

            rtbvm.TemplateCollection.UpdateCommandsCanExecute();
            foreach (var thlvm in rtbvm.TemplateCollection.Templates) {
                thlvm.OnTemplateSelected += Thlvm_OnTemplateSelected;
            }
        }

        private void Thlvm_OnTemplateSelected(object sender, EventArgs e) {
            if(BindingContext.Parent.IsPastingTemplate) {
                //BindingContext.ClearSelection();
                //(sender as MpTemplateViewModel).IsSelected = true;
            }
        }

        private void ClipTilePasteTemplateToolbar_Unloaded(object sender, RoutedEventArgs e) {
            if(_activeRtb != null) {
                var rtbvm = _activeRtb.DataContext as MpContentItemViewModel;
                if(rtbvm != null) {
                    foreach (var thlvm in rtbvm.TemplateCollection.Templates) {
                        thlvm.OnTemplateSelected -= Thlvm_OnTemplateSelected;
                    }
                }
            }
        }

        private void PreviousTemplateButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.SelectPreviousTemplateCommand.Execute(null);
            e.Handled = true;
        }

        private void NextTemplateButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.SelectNextTemplateCommand.Execute(null);
            e.Handled = true;
        }

        private void PasteTemplateButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.PasteTemplateCommand.Execute(null);
            e.Handled = true;
        }

        private void ClearAllTemplatesButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.ClearAllTemplatesCommand.Execute(null);
            e.Handled = true;
        }

        private void SelectedTemplateComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            SelectedTemplateComboBox.RaiseEvent(e);
            e.Handled = true;
        }
    }
}
