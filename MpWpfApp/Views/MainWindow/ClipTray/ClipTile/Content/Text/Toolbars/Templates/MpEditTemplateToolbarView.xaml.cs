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
    public partial class MpEditTemplateToolbarView : UserControl {
        private RichTextBox _activeRtb;
        private Hyperlink _selectedTemplateHyperlink = null;
        private string _originalText = string.Empty;
        private string _originalTemplateName = string.Empty;
        private TextRange _originalSelection = null;
        private Brush _originalTemplateColor = Brushes.Pink;

        public MpEditTemplateToolbarView() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void SetActiveRtb(RichTextBox trtb) {
            _activeRtb = trtb;
            _activeRtb.PreviewMouseLeftButtonDown += ActiveRtb_PreviewMouseLeftButtonDown;
            //var rtbvm = _activeRtb.DataContext as MpContentItemViewModel;
            //foreach(var thlvm in rtbvm.TemplateCollection.Templates) {
            //    thlvm.OnTemplateSelected += Thlvm_OnTemplateSelected;
            //}
        }

        public void CancelCreate() {
            //var rtb = this.ElementStart.Parent.FindParentOfType<RichTextBox>();

            //rtb.Selection.Select(ElementStart, ElementEnd);
            //rtb.Selection.Text = string.Empty;


            //var thlvm = DataContext as MpTemplateHyperlinkViewModel;

            //if (thlvm != null) {
            //    var thlcvm = thlvm.HostTemplateCollectionViewModel;
            //    if (thlcvm != null) {
            //        thlcvm.RemoveItem(thlvm.CopyItemTemplate, false);
            //    }
            //}

            //rtb.Selection.Select(NewStartPointer, NewStartPointer);
            //rtb.Selection.Text = NewOriginalText;
        }

        public void ShowToolbar() {
            var ctdv = this.GetVisualAncestor<MpClipTileView>().GetVisualDescendent<MpClipTileDetailView>();
            ctdv.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
        }

        public void HideToolbar() {
            Visibility = Visibility.Collapsed;
            var ctdv = this.GetVisualAncestor<MpClipTileView>().GetVisualDescendent<MpClipTileDetailView>();
            ctdv.Visibility = Visibility.Visible;            
        }

        private void Thlvm_OnTemplateSelected(object sender, EventArgs e) {
            DataContext = sender as MpTemplateViewModel;
            ShowToolbar();
        }

        private void ActiveRtb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            if(thlvm != null) {
                thlvm.OkCommand.Execute(null);
                HideToolbar();
                _activeRtb.Focus();
            }
        }


        private void TemplateNameEditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            if (e.Key == Key.Escape) {
                thlvm.CancelCommand.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.Enter) {
                thlvm.OkCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            if(thlvm.IsNew) {

            }
        }
    }
}
