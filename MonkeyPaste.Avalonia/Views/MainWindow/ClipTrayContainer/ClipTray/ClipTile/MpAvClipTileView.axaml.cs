using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebViewControl;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvContentView ContentView { get; private set; }
        private WebView wv;
        public MpAvClipTileView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileView_AttachedToVisualTree;
            this.PointerPressed += MpAvClipTileView_PointerPressed;
            this.DataContextChanged += MpAvClipTileView_DataContextChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            var dbgButton = this.FindControl<Button>("DebugButton");
            dbgButton.Click += DbgButton_Click;

            wv = this.FindControl<WebView>("WebView");

            //string[] resources = new string[] {
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\DateFormatter\jquery.dateFormat.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\DynamicQuillTools\DynamicQuillTools.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\font-awesome\fontawesome-free-6.1.1-web\css\all.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\font-awesome\fontawesome-free-6.1.1-web\all.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\HighlightJs\github.min.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\HighlightJs\highlight.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\HighlightJs\xml.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\jquery\jquery.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\jquery-context-menus\dist\context-menu.min.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\jquery-context-menus\dist\context-menu.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\jquery-context-menus\src\context-menu.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\jquery-context-menus\src\context-menu.scss",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\katex\katex.min.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\katex\katex.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill\quill.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill\quill.min.js.map",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill\quill.snow.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill-Better-Table\quill-better-table.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill-Better-Table\quill-better-table.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill-Html-Edit-Button\quill.htmlEditButton.min.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\lib\Quill-Html-Edit-Button\quill.htmlEditButton.min.js.map",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\clipboard\clipboard.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\content\content.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\content\content-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\dragdrop\dragdrop.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\editor\editor.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\editor\editor-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\editor\quill-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\font\font.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\table\table.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\toolbar\create\createTemplateToolbarItem.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\toolbar\edit\editTemplateToolbar.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\toolbar\edit\edit-template-toolbar-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\toolbar\paste\pasteTemplateToolbar.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\toolbar\paste\paste-template-toolbar-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\template.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\template\template-style.css",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\util\dataTransfer.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\util\debug.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\util\jsComAdapter.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\util\resize.js",
            //        @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\Resources\Html\Editor\src\components\util\util.js",
            //        @"file:///C:/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html"
            //};
            //foreach (var resource in resources) {
            //    var r = new ResourceUrl(resource);
            //    wv.LoadResource(r);
            //}

            wv.Address = @"file:///C:/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html";
            //wv.JavascriptContextCreated += Wv_JavascriptContextCreated;
            wv.Navigated += Wv_Navigated;
            //Dispatcher.UIThread.Post(async () => {
            //    while (!wv.IsJavascriptEngineInitialized) {
            //        await Task.Delay(100);
            //    }

            //    await Task.Delay(3000);
            //    try {
            //        bool isLoaded = await wv.EvaluateScriptFunctionInFrame<bool>("checkIsLoaded", string.Empty);
            //        while (!isLoaded) {
            //            isLoaded = await wv.EvaluateScriptFunctionInFrame<bool>("checkIsLoaded", string.Empty); //wv.EvaluateScriptFunction<bool>("checkIsLoaded");
            //        }
            //        wv.ExecuteScriptFunctionInFrame($"setText", $"{BindingContext.CopyItemData}", string.Empty);
            //    }
            //    catch (Exception ex) {
                    
            //        return;
            //    }
            //});


            //var ctvBorder = this.FindControl<Border>("ClipTileContainerBorder");
            //ctvBorder.AddHandler(Border.PointerMovedEvent, CtvBorder_PointerMoved, RoutingStrategies.Tunnel);
            //ctvBorder.AddHandler(Border.PointerPressedEvent, CtvBorder_PointerPressed, RoutingStrategies.Tunnel);
            //ctvBorder.AttachedToLogicalTree += CtvBorder_AttachedToLogicalTree;

            //ContentView = new MpAvContentView();
            //ctvBorder.Child = ContentView.Default.ContentControl;
        }

        private void Wv_Navigated(string url, string frameName) {

            //     while (true) {
            //         //bool isLoaded = await IsLoaded(string.Empty);
            //         if (true) {
            //             string initRequest = @"reqMsg = {
            //envName: 'wpf',
            //isReadOnlyEnabled: true,
            //usedTextTemplates: {},
            //isPasteRequest: false,
            //itemEncodedHtmlData: " + BindingContext.CopyItemData + "}";

            //             wv.ExecuteScriptFunctionInFrame("init", frameName, initRequest);

            //             break;
            //         }
            //         await Task.Delay(100);
            //     }
            string initRequest = @"{
			    envName: 'wpf',
			    isReadOnlyEnabled: true,
			    usedTextTemplates: {},
			    isPasteRequest: false,
			    itemEncodedHtmlData: '" + BindingContext.CopyItemData + "'}";

            wv.ExecuteScriptFunction("init", $"'{BindingContext.CopyItemData}'");
        }

        private async void Wv_JavascriptContextCreated(string frameName) {
            if(frameName != null) {
                Debugger.Break();
            }
            while(true) {
                bool isLoaded = true;// await IsLoaded(frameName);
                if (isLoaded) {
                    Wv_Navigated(null, string.Empty);
                    break;
                }
                await Task.Delay(100);
            }
            //wv.ExecuteScriptFunctionInFrame($"setText", $"blah blah", frameName);
        }

        private async Task<bool> IsLoaded(string frameName) {
            try {
                bool isLoaded = await wv.EvaluateScriptFunctionInFrame<bool>("checkIsLoaded", string.Empty);
                return isLoaded;                
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return false;
            }
        }

        private void CtvBorder_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            var ctvBorder = sender as Border;

            

        }

        private void CtvBorder_PointerMoved(object sender, PointerEventArgs e) {
            if(BindingContext == null) {
                return;
            }
            e.Handled = BindingContext.IsContentReadOnly || BindingContext.IsSubSelectionEnabled;
        }

        private void CtvBorder_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if(BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                sivm.Selector.SelectedItem = BindingContext;
            }
            e.Handled = BindingContext.IsContentReadOnly || BindingContext.IsSubSelectionEnabled;
        }

        private void DbgButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var webView = this.FindControl<WebView>("WebView");
            webView.ShowDeveloperTools();
            //if(ContentView.Default.ContentControl is WebView wv) {
            //    wv.ShowDeveloperTools();
            //}
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayLayoutChanged:
                    break;
            }
        }
        private void MpAvClipTileView_DataContextChanged(object sender, System.EventArgs e) {
            //if(BindingContext == null) {
            //    return;
            //}
            //ContentView.Default.ContentData = BindingContext.CopyItemData;

            BindingContext.PropertyChanged += BindingContext_PropertyChanged;

            string initRequest = @"reqMsg = {
			    envName: 'wpf',
			    isReadOnlyEnabled: true,
			    usedTextTemplates: {},
			    isPasteRequest: false,
			    itemEncodedHtmlData: "+BindingContext.CopyItemData+"}";


        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                //case nameof(BindingContext.CopyItemData):
                //    ContentView.Default.ContentData = BindingContext.CopyItemData;
                //    break;
            }
        }

        private void MpAvClipTileView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                //if(BindingContext is MpISelectableViewModel svm) {
                //    svm.IsSelected = true;
                //}
                if (BindingContext is MpISelectorItemViewModel<MpAvClipTileViewModel> sivm) {
                    sivm.Selector.SelectedItem = BindingContext;
                }
            }
        }

        private void MpAvClipTileView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
