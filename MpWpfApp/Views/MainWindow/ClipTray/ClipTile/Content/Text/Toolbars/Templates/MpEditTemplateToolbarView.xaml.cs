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
        }

        public void SetActiveRtb(RichTextBox trtb) {
            _activeRtb = trtb;
            _activeRtb.PreviewMouseLeftButtonDown += ActiveRtb_PreviewMouseLeftButtonDown;
            var ettvm = DataContext as MpEditTemplateToolbarViewModel;

            if (ettvm.HostClipTileViewModel.IsEditingTemplate && ettvm.HostClipTileViewModel.IsEditingTile) {
                TemplateNameEditorTextBox.Focus();
                TemplateNameEditorTextBox.SelectAll();
            } else if (ettvm.WasEdited) {
                ettvm.ResetState();                
            }
        }

        public void SetActiveTemplate(MpTemplateHyperlinkViewModel thvm) {
            var ettvm = DataContext as MpEditTemplateToolbarViewModel;
            //ettvm.CopyItemTemplate = cit;
            ettvm.SetTemplate(thvm, true);
        }

        private void ActiveRtb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ettvm = DataContext as MpEditTemplateToolbarViewModel;
            if(ettvm.HostClipTileViewModel.IsEditingTemplate) {
                ettvm.OkCommand.Execute(null);
                _activeRtb.Focus();
            }
        }

        private void TemplateNameEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var ettvm = DataContext as MpEditTemplateToolbarViewModel;
            ettvm.WasEdited = true;
            ettvm.SetTemplateName(TemplateNameEditorTextBox.Text);
        }

        private void TemplateNameEditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var ettvm = DataContext as MpEditTemplateToolbarViewModel;
            if (e.Key == Key.Escape) {
                ettvm.CancelCommand.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.Enter) {
                ettvm.OkCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
