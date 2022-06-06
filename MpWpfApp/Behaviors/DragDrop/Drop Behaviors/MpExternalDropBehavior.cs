using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Diagnostics;
using MpProcessHelper;
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
            //return base.IsDragDataValid(isCopy, dragData);            
            return dragData != null;
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
                MpProcessAutomation.SetActiveProcess(dropHandle);
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
            if(MpClipboardHandlerCollectionViewModel.Instance.SelectedItem != null) {
                MpClipboardHandlerCollectionViewModel.Instance.SelectedItem.IsDraggingToExternal = true;
            } else {
                Debugger.Break();
            }

            var ctvm = MpDragDropManager.DragData as MpClipTileViewModel;
            if(ctvm == null) {
                Debugger.Break();
                return;
            }

            MpPortableDataObject mpdo = await ctvm.ConvertToPortableDataObject(                
                true, 
                MpProcessManager.LastHandle,
                false);
            DataObject wpfdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformClipboardDataObject(mpdo) as DataObject;

            if (ctvm.HasTemplates) {
                MpDragDropManager.IsPreExternalTemplateDrop = true;
                DragDrop.AddPreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
                DragDrop.AddPreviewGiveFeedbackHandler(AssociatedObject, OnGiveFeedback);
            }

            DragDrop.DoDragDrop(AssociatedObject, wpfdo, DragDropEffects.Copy);
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e) {
            MpConsole.WriteLine("Feedback: " + e.Effects);

            if(!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                // TODO Handle external drag icon here
                e.Handled = true;
            }
        }
        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            MpConsole.WriteLine("Action: " + e.Action);

            if (!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                var dropAppHandle = MpProcessHelper.MpProcessManager.LastHandle;

                e.Handled = true;
                e.Action = DragAction.Cancel;

                MpProcessAutomation.ActivateThisApp();

                MpClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(dropAppHandle);

                MpHelpers.RunOnMainThread(async () => {
                    while(MpClipTrayViewModel.Instance.SelectedItem.IsPasting) {
                        await Task.Delay(100);
                    }
                    Reset();
                });
            }
        }

        public override async Task Drop(bool isCopy, object dragData) {            
            await Task.Delay(1);
            Reset();
        }

        public override void AutoScrollByMouse() {
            return;
        }

        public override void Reset() {
            base.Reset();

            MpDragDropManager.IsPreExternalTemplateDrop = false;

            if (MpClipboardHandlerCollectionViewModel.Instance.SelectedItem != null) {
                MpClipboardHandlerCollectionViewModel.Instance.SelectedItem.IsDraggingToExternal = false;
            } else {
                Debugger.Break();
            }

            DragDrop.RemovePreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
            DragDrop.RemovePreviewGiveFeedbackHandler(AssociatedObject, OnGiveFeedback);
        }
    }

}
