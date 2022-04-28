using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonkeyPaste.Plugin;

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
            Rect extRect = new Rect(
                0,0,
                MpMeasurements.Instance.ScreenWidth,MpMainWindowViewModel.Instance.MainWindowTop);
            return new List<Rect> { extRect };
        }

        public override int GetDropTargetRectIdx() {
            //Point mp = Mouse.GetPosition(Application.Current.MainWindow);
            Point mp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            //MpConsole.WriteLine("Mouse Relative to Main Window: " + mp.ToString());
            if (GetDropTargetRects()[0].Contains(mp)) {
                //Application.Current.MainWindow.Top = 20000;
                IntPtr lastHandle = MpProcessHelper.MpProcessManager.LastHandle;
                WinApi.SetForegroundWindow(lastHandle);
                WinApi.SetActiveWindow(lastHandle);
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

            //var ido = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, true);
            var mpdo = await MpWpfDataObjectHelper.Instance.GetCopyItemDataObjectAsync(MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem, true, MpProcessHelper.MpProcessManager.LastHandle);
            var ido = MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformFormat(mpdo);

            DragDrop.DoDragDrop(AssociatedObject, ido, DragDropEffects.Copy);
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            return;
        }
                

        

        
    }

}
