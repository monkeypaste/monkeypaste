
using CefSharp;
using CefSharp.Enums;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpQuillEditorView.xaml
    /// </summary>
    public partial class MpQuillEditorView : MpContentUserControl<MpContentItemViewModel>, IDragHandler {
        public bool IsDomContentLoaded { get; private set; }

        public MpQuillEditorView() {
            InitializeComponent();
        }

        private void EditorBrowser_Loaded(object sender, RoutedEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if(civm == null || civm.IsPlaceholder) {
                return;
            }

            QuillWebView.DragHandler = this;

            MpMessenger.Register<MpMessageType>(
                BindingContext.Parent,
                ReceivedClipTileViewModelMessage,
                BindingContext.Parent);

            MpMessenger.Register<MpMessageType>(
                (Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior,
                ReceivedMainWindowResizeBehviorMessage);
        }

        private void QuillWebView_Unloaded(object sender, RoutedEventArgs e) {
            if (BindingContext.Parent != null) {
                MpMessenger.Unregister<MpMessageType>(
                    BindingContext.Parent,
                    ReceivedClipTileViewModelMessage,
                    BindingContext.Parent);
            }

            var mw = Application.Current.MainWindow as MpMainWindow;
            if (mw != null) {
                if (mw.MainWindowResizeBehvior != null) {
                    MpMessenger.Unregister<MpMessageType>(
                            mw.MainWindowResizeBehvior,
                            ReceivedMainWindowResizeBehviorMessage);
                }
            }
        }


        private void QuillWebView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

        }

        private void ReceivedClipTileViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.IsEditable:
                    QuillWebView.FitDocToWebView();
                    break;
                case MpMessageType.IsReadOnly:
                    QuillWebView.FitDocToWebView();
                    //MpHelpers.RunOnMainThread(async () => {
                    //    await SyncModelsAsync();
                    //});
                    break;
            }
        }

        private void ReceivedMainWindowResizeBehviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingContent:
                case MpMessageType.ResizeContentCompleted:
                    QuillWebView.FitDocToWebView();
                    break;
            }
        }
        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e) {
            if(e.IsLoading == false) {
                IsDomContentLoaded = true;
            }
        }

        private void QuillWebView_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape && BindingContext.IsEditingContent) {
                BindingContext.Parent.ToggleReadOnlyCommand.Execute(null);
            }
        }

        public bool OnDragEnter(IWebBrowser chromiumWebBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask) {
            return false;
        }

        public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions) {
            //throw new NotImplementedException();
        }
    }
}
