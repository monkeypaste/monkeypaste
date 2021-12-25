using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public enum MpExternalDropFileType {
        None = 0,
        Txt,
        Csv,
        Rtf,
        Bmp,
        Png,
        File
    }

    public class MpExternalDropBehavior : MpDropBehaviorBase<FrameworkElement> {
        #region Singleton Definition
        private static readonly Lazy<MpExternalDropBehavior> _Lazy = new Lazy<MpExternalDropBehavior>(() => new MpExternalDropBehavior());
        public static MpExternalDropBehavior Instance { get { return _Lazy.Value; } }
        #endregion

        private IDataObject _ido;

        public override MpDropType DropType => MpDropType.External;

        public override FrameworkElement AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override bool IsEnabled { get; set; } = true;

        public override UIElement RelativeToElement {
            get {
                return (Application.Current.MainWindow as MpMainWindow).MainWindowGrid;
            }
        }

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExpandComplete:
                    IsEnabled = false;
                    UpdateAdorner();
                    break;
                case MpMessageType.UnexpandComplete:
                    IsEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            Rect extRect = new Rect(
                0,0,
                MpMeasurements.Instance.ScreenWidth,MpMainWindowViewModel.Instance.MainWindowTop + 20);
            return new List<Rect> { extRect };
        }

        public override int GetDropTargetRectIdx() {
            Point mp = Mouse.GetPosition(Application.Current.MainWindow);
            MpConsole.WriteLine("Mouse Relative to Main Window: " + mp.ToString());
            if(GetDropTargetRects()[0].Contains(mp)) {
                //Application.Current.MainWindow.Top = 20000;
                IntPtr lastHandle = MpClipboardManager.Instance.LastWindowWatcher.LastHandle;
                WinApi.SetForegroundWindow(lastHandle);
                WinApi.SetActiveWindow(lastHandle);
                return 0;
            }
            return -1;
        }

        public override async Task StartDrop() {
            var ido = await MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, true);
            DragDrop.DoDragDrop(AssociatedObject, ido, DragDropEffects.Copy);
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await Task.Delay(1);
        }

        public override void AutoScrollByMouse() {
            return;
        }

        public bool IsProcessNeedFileDrop(string processPath) {
            if (string.IsNullOrEmpty(processPath) || !File.Exists(processPath)) {
                return false;
            }

            try {
                string processName = Path.GetFileNameWithoutExtension(processPath).ToLower();
                if (processName == null) {
                    return false;
                }
                switch (processName) {
                    case "explorer":
                    case "mspaint":
                    case "notepad":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("IsProcessNeedFileDrop GetFileName exception: " + ex);
                return false;
            }
        }

        public bool IsProcessLikeNotepad(string processPath) {
            if (string.IsNullOrEmpty(processPath) || !File.Exists(processPath)) {
                return false;
            }

            try {
                string processName = Path.GetFileNameWithoutExtension(processPath).ToLower();
                if (processName == null) {
                    return false;
                }
                switch (processName) {
                    case "notepad":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("IsProcessLikeNotepad GetFileName exception: " + ex);
                return false;
            }
        }
    }

}
