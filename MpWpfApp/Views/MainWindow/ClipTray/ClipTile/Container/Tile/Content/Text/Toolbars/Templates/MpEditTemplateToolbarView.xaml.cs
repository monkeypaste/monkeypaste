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
using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpEditTemplateToolbarView.xaml
    /// </summary>
    public partial class MpEditTemplateToolbarView : MpUserControl<MpTextTemplateViewModelBase> {

        public MpEditTemplateToolbarView() {
            InitializeComponent();
        }

        private void TemplateNameEditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            //var thlvm = (DataContext as MpTemplateCollectionViewModel).SelectedItem;
            if (e.Key == Key.Escape) {
                BindingContext.CancelEditTemplateCommand.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.Enter) {
                BindingContext.FinishEditTemplateCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void TemplateColorButton_Click(object sender, RoutedEventArgs e) {
            MpContextMenuView.Instance.DataContext = new MpMenuItemViewModel() {
                SubItems = new List<MpMenuItemViewModel>() {
                    MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(BindingContext)
                }
            };
            MpContextMenuView.Instance.PlacementTarget = sender as Button;
            MpContextMenuView.Instance.IsOpen = true;
            
        }

        private void ClipTileEditTemplateToolbar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue) {
                TemplateNameEditorTextBox.Focus();
                Keyboard.Focus(TemplateNameEditorTextBox);
            }
        }

        private void ContactFieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            var cfcmb = sender as ComboBox;
            BindingContext.TemplateData = ((MpContactFieldType)cfcmb.SelectedIndex).ToString();
        }

        private void TemplateTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { 
            //if (BindingContext == null) {
            //    return;
            //}
            //if(e.RemovedItems != null && e.RemovedItems.Count > 0) {
            //    // only clear template data when user changes selection not when loading
            //    BindingContext.TemplateData = null;
            //}

            //BindingContext.TemplateData = null;
            var templateSelector = EditTemplateContainerGrid.Resources["EditTemplateDetailViewSelector"] as MpEditTemplateDetailViewSelector;
            TemplateDetailContentControl.ContentTemplate = templateSelector.SelectTemplate(BindingContext, TemplateDetailContentControl);

            //BindingContext.OnPropertyChanged(nameof(BindingContext.SelfBindingRef));
        }

        private void ContactFieldComboBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue) {
                if (BindingContext == null) {
                    return;
                }
                var cfcmb = sender as ComboBox;
                if (string.IsNullOrEmpty(BindingContext.TemplateData)) {
                    cfcmb.SelectedIndex = 0;
                } else if (Enum.TryParse(BindingContext.TemplateData, out MpContactFieldType fieldType)) {
                    cfcmb.SelectedIndex = (int)fieldType;
                } else {
                    BindingContext.TemplateData = null;
                    cfcmb.SelectedIndex = 0;
                }
            }
        }
    }
}
