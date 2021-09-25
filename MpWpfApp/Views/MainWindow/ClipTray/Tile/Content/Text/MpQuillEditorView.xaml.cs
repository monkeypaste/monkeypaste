using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for MpQuillEditorView.xaml
    /// </summary>
    public partial class MpQuillEditorView : UserControl {
        public MpQuillEditorView() {
            InitializeComponent();
        }

        private void EditorBrowser_Loaded(object sender, RoutedEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            //var html = MonkeyPaste.MpHelpers.Instance.LoadTextResource("MonkeyPaste.Resources.Html.Editor.Editor2.html");
            string html = MpHelpers.Instance.ReadTextFromFile(@"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\Editor2.html");
            
            string contentTag = @"<div id='editor'>";
            string itemHtml = civm.CopyItem.ItemData;
            html = html.Replace(contentTag, contentTag + itemHtml);

            string envTag = @"var envName = '';";
            string envVal = @"var envName = 'wpf';";
            html = html.Replace(envTag, envVal);

            
            var ds = itemHtml.ToFlowDocument().GetDocumentSize();
            var civ = this.GetVisualAncestor<MpContentListView>();
            EditorBrowser.Width = civ.ActualWidth;
            EditorBrowser.Height = civ.ActualHeight;

            MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                await EditorBrowser.EnsureCoreWebView2Async(null);
                EditorBrowser.NavigateToString(html);
                EditorBrowser.UpdateLayout();
            });
        }
    }
}
