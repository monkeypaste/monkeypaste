using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonkeyPaste.Plugin;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System.Threading;

namespace MonkeyPaste {

    public enum MpExternalDropFileType {
        None = 0,
        Txt,
        Csv,
        Rtf,
        Bmp,
        Png,
        File
    }
}

namespace MpWpfApp {

    public class MpExternalDropBehavior : MpDropBehaviorBase<FrameworkElement> {
        #region Private Variables

        private DragDropEffects _dragDropEffects = DragDropEffects.None;

        #endregion

        #region Singleton Definition
        private static readonly Lazy<MpExternalDropBehavior> _Lazy = new Lazy<MpExternalDropBehavior>(() => new MpExternalDropBehavior());
        public static MpExternalDropBehavior Instance { get { return _Lazy.Value; } }
        #endregion

        public override MpDropType DropType => MpDropType.External;
        public override FrameworkElement AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        
        public override bool IsDropEnabled { get; set; } = true;

        public override UIElement RelativeToElement {
            get {
                return (Application.Current.MainWindow as MpMainWindow).MainWindowGrid;
            }
        }

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        protected override void OnLoad() {
            IsDebugEnabled = false;
            base.OnLoad();

        }
        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    RefreshDropRects();
                    break;
            }
        }

        public override bool IsDragDataValid(bool isCopy, object dragData) {
            if(MpDragDropManager.IsDraggingFromExternal) {
                return false;
            }
            return base.IsDragDataValid(isCopy, dragData);            
        }

        public override List<Rect> GetDropTargetRects() {
            int pad = 1;
            // NOTE subtracting pad from bottom of rect (main window top) because converting mp to winforms
            // isn't exactly accurate or the title bar highlight border isn't accounted for w/ main window top or
            // timing may make point within this app
            Rect extRect = new Rect(
                0,0,
                MpMeasurements.Instance.ScreenWidth,MpMainWindowViewModel.Instance.MainWindowTop - pad);
            return new List<Rect> { extRect };
        }

        public override int GetDropTargetRectIdx() {
            //Point mp = Mouse.GetPosition(Application.Current.MainWindow);
            Point mp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            //MpConsole.WriteLine("Mouse Relative to Main Window: " + mp.ToString());
            if (GetDropTargetRects()[0].Contains(mp)) {
                //Application.Current.MainWindow.Top = 20000;
                var winforms_mp = MpScreenInformation.ConvertWpfScreenPointToWinForms(mp);
                IntPtr dropHandle = MpProcessHelper.MpProcessManager.GetParentHandleAtPoint(winforms_mp);
                if(dropHandle == MpProcessHelper.MpProcessManager.ThisAppHandle) {
                    //Debugger.Break();
                    return -1;
                }

                WinApi.SetForegroundWindow(dropHandle);
                WinApi.SetActiveWindow(dropHandle);
                return 0;
            }
            return -1;
        }
        public override MpShape[] GetDropTargetAdornerShape() {
            var drl = GetDropTargetRects();
            if (DropIdx < 0 || DropIdx >= drl.Count) {
                return null;
            }

            return new MpRect(new MpPoint(), new MpSize(drl[0].Width, drl[0].Height)).ToArray<MpShape>();
        }

        public override async Task StartDrop() {
            var ci = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem;
            //var ido = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, true);
            var mpdo = await MpWpfDataObjectHelper.Instance.GetCopyItemDataObjectAsync(
                ci, 
                true, 
                MpProcessHelper.MpProcessManager.LastHandle);
            var ido = MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformClipboardDataObject(mpdo) as DataObject;

            //var startAction = (Action<VirtualFileDataObject>)((vfdo) => {
            //    MessageBox.Show("ICKY BOOM BOOM DAAYE");
            //});

            //var endAction = (Action<VirtualFileDataObject>)((vfdo) => {
            //    MessageBox.Show("Done");
            //});

            ////var vido = new VirtualFileDataObject(startAction, endAction);
            //var vido = new VirtualFileDataObject(
            //    // BeginInvoke ensures UI operations happen on the right thread
            //    (vfdo) => Dispatcher.BeginInvoke((Action)(() => MessageBox.Show("ICKY BOOM BOOM DAAYE"))),
            //    (vfdo) => Dispatcher.BeginInvoke((Action)(() => MessageBox.Show("Done"))));

            ////vido.SetData( 
            ////    (short)DataFormats.GetDataFormat(DataFormats.FileDrop).Id,
            ////    new List<Byte>(Encoding.Default.GetBytes(ci.ItemData)));

            //vido.SetData(new VirtualFileDataObject.FileDescriptor[] {
            //    new VirtualFileDataObject.FileDescriptor
            //    {
            //        Name = Path.GetFileName(ci.ItemData),
            //        //StreamContents = stream => new FileStream(ci.ItemData, FileMode.Open)
            //        StreamContents = (stream) => {
            //                var data = MpFileIo.ReadBytesFromFile(ci.ItemData);
            //                stream.Write(data,0,data.Length);
            //            }

            //        //}
            //        //stream =>
            //        //    {
            //        //        using(var webClient = new FileStream())
            //        //        {
            //        //            var data = webClient.DownloadData("http://blogs.msdn.com/delay/rss.xml");
            //        //            stream.Write(data, 0, data.Length);
            //        //        }
            //        //    }
            //    },
            // });
            DragDrop.AddPreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
            DragDrop.AddPreviewGiveFeedbackHandler(AssociatedObject, OnGiveFeedback);
            DragDrop.DoDragDrop(AssociatedObject, ido, DragDropEffects.Copy);
            //VirtualFileDataObject.DoDragDrop(AssociatedObject, vido, DragDropEffects.Copy);
        }
        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e) {
            MpConsole.WriteLine("Feedback: " + e.Effects);

            if(!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                e.Handled = true;

                //var handle = MpProcessHelper.MpProcessManager.ThisAppHandle;
                //WinApi.SetForegroundWindow(handle);
                //WinApi.SetActiveWindow(handle);
                //MessageBox.Show("ICKY BOOM BOOM DAAYE");
            }
        }
        private async void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            MpConsole.WriteLine("Action: " + e.Action);

            if (!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                //while(true) {
                //    Thread.Sleep(100);
                //}
                e.Handled = true;
                e.Action = DragAction.Cancel;
                //var handle = MpProcessHelper.MpProcessManager.ThisAppHandle;
                //WinApi.SetForegroundWindow(handle);
                //WinApi.SetActiveWindow(handle);
                //MessageBox.Show("ICKY BOOM BOOM DAAYE");
            }
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            return;
        }
    }

}
