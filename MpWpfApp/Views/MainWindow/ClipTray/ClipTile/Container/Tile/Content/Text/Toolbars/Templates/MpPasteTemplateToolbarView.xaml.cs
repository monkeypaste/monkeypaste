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
        }


        private void ClipTilePasteTemplateToolbar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(BindingContext == null || ((bool)e.NewValue) == false) {
                return;
            }
            if(BindingContext.Items.Count == 0) {
                return;
            }
            if(BindingContext.SelectedItem == null) {
                BindingContext.SelectedItem = BindingContext.Items[0];                
            }

            var rtb = this.GetVisualAncestor<MpClipTileView>()
                .GetVisualDescendent<MpContentView>()
                .GetVisualDescendent<RichTextBox>();
            //rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);

            BindingContext.SelectedItem.IsPasteTextBoxFocused = true;
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
