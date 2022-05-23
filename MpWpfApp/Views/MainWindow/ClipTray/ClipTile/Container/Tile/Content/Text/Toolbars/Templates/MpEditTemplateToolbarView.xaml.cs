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
    public partial class MpEditTemplateToolbarView : MpUserControl<MpTemplateViewModel> {
        private RichTextBox _activeRtb {
            get {
                var ctv = this.GetVisualAncestor<MpClipTileView>();
                if(ctv == null) {
                    return null;
                }
                return ctv.GetVisualDescendent<RichTextBox>();
            }
        }

        public MpEditTemplateToolbarView() {
            InitializeComponent();
        }

        public void SetActiveRtb(RichTextBox trtb) {
            //if (_activeRtb == trtb) {
            //    return;
            //}
            //_activeRtb = trtb;
            //_activeRtb.PreviewMouseLeftButtonDown += ActiveRtb_PreviewMouseLeftButtonDown;
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
    }
}
