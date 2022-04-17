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
    public partial class MpEditTemplateToolbarView : MpUserControl<MpTemplateCollectionViewModel> {
        private RichTextBox _activeRtb;

        public MpEditTemplateToolbarView() {
            InitializeComponent();
        }

        public void SetActiveRtb(RichTextBox trtb) {
            if (_activeRtb == trtb) {
                return;
            }
            _activeRtb = trtb;
            _activeRtb.PreviewMouseLeftButtonDown += ActiveRtb_PreviewMouseLeftButtonDown;
        }

        public void CancelEdit() {
            var thlvm = (DataContext as MpTemplateCollectionViewModel).SelectedTemplate;

            thlvm.CancelCommand.Execute(null);
            if (thlvm.WasNewOnEdit) {
                //var selectionStart = SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start;
                //        SelectedTemplateHyperlinkViewModel.Dispose(false);
                //        _originalSelection.Text = _originalText;
                //        var sr = MpHelpers.FindStringRangeFromPosition(selectionStart, _originalText, true);
                //        SubSelectedRtbViewModel.Rtb.Selection.Select(sr.Start, sr.End); 
                var rtbv = _activeRtb.GetVisualAncestor<MpContentView>();
                new TextRange(rtbv.LastEditedHyperlink.ElementStart, rtbv.LastEditedHyperlink.ElementEnd).Text = string.Empty;

                _activeRtb.Selection.Select(rtbv.NewStartRange.Start, rtbv.NewStartRange.End);
                _activeRtb.Selection.Text = string.Empty;
                _activeRtb.Selection.Text = rtbv.NewOriginalText;
                thlvm.WasNewOnEdit = false;
            }
            //Visibility = Visibility.Collapsed;
        }

        private void ActiveRtb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(Visibility == Visibility.Visible) {
                var thlvm = (DataContext as MpTemplateCollectionViewModel).SelectedTemplate;
                if (thlvm != null) {
                    thlvm.OkCommand.Execute(null);
                }
            }
        }

        private void TemplateNameEditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var thlvm = (DataContext as MpTemplateCollectionViewModel).SelectedTemplate;
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
            CancelEdit();
        }
    }
}
