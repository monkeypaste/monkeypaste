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
            if(BindingContext == null) {
                return;
            }
            foreach (var civm in BindingContext.Parent.Parent.ItemViewModels) {
                foreach (var thlvm in civm.TemplateCollection.Templates) {
                    thlvm.OnTemplateSelected -= Thlvm_OnTemplateSelected;
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
            SelectedTemplateComboBox.IsDropDownOpen = true;
            e.Handled = true;
        }

        private void ClipTilePasteTemplateToolbar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(BindingContext == null || ((bool)e.NewValue) == false) {
                return;
            }
            foreach(var civm in BindingContext.Parent.Parent.ItemViewModels) {
                foreach (var thlvm in civm.TemplateCollection.Templates) {
                    thlvm.OnTemplateSelected += Thlvm_OnTemplateSelected;
                }
            }

            SelectedTemplateTextBox.Focus();
        }

        private void SelectedTemplateTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                if(BindingContext.IsAllTemplatesFilled) {
                    BindingContext.PasteTemplateCommand.Execute(null);
                } else {
                    BindingContext.SelectNextTemplateCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
