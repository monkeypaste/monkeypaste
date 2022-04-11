using CefSharp;
using CefSharp.Enums;
using CefSharp.Wpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpJsComAdapter {
        public string Test1() {
            return "SUCCESS";
        }

        public string Test2(string val) {
            return "SUCCESS " + val;
        }

        

        public async Task<string> GetClipboardContent() {
            var result = await MpNativeWrapper.Services.ClipboardContentDataProvider.GetClipboardContentData();
            return result;
        }
    }

    public class MpJsDropHandler : IDragHandler {

        public bool OnDragEnter(IWebBrowser chromiumWebBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask) {
            //MpHelpers.RunOnMainThread(async () => {
            //    MpConsole.WriteLine("Chromium Drag Enter: ");
            //    MpConsole.WriteLine(dragData.FragmentText);

            //    if (chromiumWebBrowser.CanExecuteJavascriptInMainFrame) {
            //        string data = dragData.FragmentText.SerializeToJsonByteString();

            //        await chromiumWebBrowser.EvaluateScriptAsync($"onCefDragEnter('{data}')");
            //        await chromiumWebBrowser.EvaluateScriptAsync($"onDrop()");
            //    }

            //    //if (chromiumWebBrowser is FrameworkElement fe) {
            //    //    fe.MouseMove += Fe_MouseMove;
            //    //}
            //});

            //var dataObject = new DataObject();

            //dataObject.SetText(dragData.FragmentText, TextDataFormat.Text);
            //dataObject.SetText(dragData.FragmentText, TextDataFormat.UnicodeText);
            //dataObject.SetText(dragData.FragmentHtml, TextDataFormat.Html);

            //// TODO: The following code block *should* handle images, but GetFileContents is
            //// not yet implemented.
            ////if (dragData.IsFile)
            ////{
            ////    var bmi = new BitmapImage();
            ////    bmi.BeginInit();
            ////    bmi.StreamSource = dragData.GetFileContents();
            ////    bmi.EndInit();
            ////    dataObject.SetImage(bmi);
            ////}


            MpConsole.WriteLine("Chromium drag enter called");
            return false;
        }

        private void Fe_MouseMove(object sender, MouseEventArgs e) {
            var fe = sender as FrameworkElement;
            bool result = fe.CaptureMouse();
            MpConsole.WriteLine("Mouse was " + (result ? "CAPTURED" : "NOT CAPTURED"));
            if (Mouse.LeftButton == MouseButtonState.Released) {
                MpHelpers.RunOnMainThread(async () => {
                    var chromiumWebBrowser = sender as ChromiumWebBrowser;
                    fe.ReleaseMouseCapture();
                    if (chromiumWebBrowser.CanExecuteJavascriptInMainFrame) {

                        await chromiumWebBrowser.EvaluateScriptAsync($"onDrop()");

                        MpConsole.WriteLine("js onDrop called from cef");
                        chromiumWebBrowser.MouseMove -= Fe_MouseMove;
                    }
                });
            }
        }

        public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions) {
            return;
        }
    }
}
