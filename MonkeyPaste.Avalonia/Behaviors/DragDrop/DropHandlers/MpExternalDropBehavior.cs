using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {

    public class MpExternalDropBehavior : MpAvDropBehaviorBase<Control> {
        #region Private Variables
        
        private bool _wasReset = false;

        #endregion

        #region Singleton Definition
        private static MpExternalDropBehavior _instance;
        public static MpExternalDropBehavior Instance => _instance ?? (_instance = new MpExternalDropBehavior());
        #endregion

        #region Properties

        #region MpIDropBehavior

        public override MpDropType DropType => MpDropType.External;
        public override Control AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        
        public override bool IsDropEnabled { get; set; } = true;

        public override Control RelativeToElement {
            get {
                return MpAvMainWindow.Instance;
            }
        }

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        #endregion

        public bool IsPreExternalTemplateDrop { get; set; }

        #endregion 

        protected override void OnLoad() {
            IsDebugEnabled = false;
            base.OnLoad();

        }
        protected override void ReceivedGlobalMessage(MpMessageType msg) {
            base.ReceivedGlobalMessage(msg);
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    RefreshDropRects();
                    break;
            }
        }

        public override bool IsDragDataValid(bool isCopy, object dragData) {
            if(MpAvDragDropManager.IsDraggingFromExternal) {
                return false;
            }
            //return base.IsDragDataValid(isCopy, dragData);            
            return dragData != null;
        }

        public override List<MpRect> GetDropTargetRects() {
            int pad = 1;
            // NOTE subtracting pad from bottom of rect (main window top) because converting mp to winforms
            // isn't exactly accurate or the title bar highlight border isn't accounted for w/ main window top or
            // timing may make point within this app
            MpRect extRect = new MpRect(
                0,0,
                MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width,
                MpAvMainWindowViewModel.Instance.MainWindowTop - pad);
            return new List<MpRect> { extRect };
        }

        public override int GetDropTargetRectIdx() {
            //Point mp = Mouse.GetPosition(Application.Current.MainWindow);
            MpPoint mp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            //MpConsole.WriteLine("Mouse Relative to Main Window: " + mp.ToString());
            if (GetDropTargetRects()[0].Contains(mp)) {
                //Application.Current.MainWindow.Top = 20000;
                //var winforms_mp = MpScreenInformation.ConvertWpfScreenPointToWinForms(mp);
                IntPtr dropHandle = MpPlatformWrapper.Services.ProcessWatcher.GetParentHandleAtPoint(mp);
                if(dropHandle == MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle) {
                    //Debugger.Break();
                    return -1;
                }
                MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(dropHandle);
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

        public override async Task StartDrop(PointerEventArgs e) {
            var ctvm = MpAvDragDropManager.DragData as MpAvClipTileViewModel;
            if(ctvm == null) {
                Debugger.Break();
                return;
            }

            MpMessenger.SendGlobal(MpMessageType.ExternalDragBegin);

            MpPortableDataObject mpdo = await ctvm.ConvertToPortableDataObject(false);
            DataObject wpfdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformClipboardDataObject(mpdo) as DataObject;
            
            if (ctvm.HasTemplates) {
                IsPreExternalTemplateDrop = true;
                AssociatedObject?.AddHandler(DragDrop.DragOverEvent, OnQueryContinueDrag, RoutingStrategies.Tunnel);

                //DragDrop.AddPreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
            } else {
                AssociatedObject?.AddHandler(DragDrop.DragOverEvent, OnQueryContinueDrag, RoutingStrategies.Direct);
                //DragDrop.AddQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
            }

            DragDrop.DoDragDrop(e, wpfdo, DragDropEffects.Copy).FireAndForgetSafeAsync(MpAvMainWindowViewModel.Instance);
        }

        private void OnQueryContinueDrag(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("Action: " + e.Action);
            //e.Handled = true;
            if (_wasReset) {
                MpConsole.WriteLine("External drop reset");
                _wasReset = false;
                AssociatedObject?.RemoveHandler(DragDrop.DragOverEvent, OnQueryContinueDrag);
                //DragDrop.RemovePreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
                e.Handled = true;
                //e.Action = DragAction.Cancel;
                e.DragEffects = DragDropEffects.None;
                return;
            }  else if(MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                MpConsole.WriteLine("External drop L mouse button is down");
                //e.Action = DragAction.Continue;  
                e.DragEffects = DragDropEffects.Copy;
            } else {
                MpConsole.WriteLine("External drop L mouse button is up");
                if (IsPreExternalTemplateDrop) {
                    Dispatcher.UIThread.Post(async () => {
                        e.Handled = true;
                        //e.Action = DragAction.Cancel;
                        e.DragEffects = DragDropEffects.None;
                        //DragDrop.RemovePreviewQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
                        AssociatedObject?.RemoveHandler(DragDrop.DragOverEvent, OnQueryContinueDrag);

                        var ctvm = MpAvClipTrayViewModel.Instance.SelectedItem;

                        if (ctvm == null) {
                            var dd = MpAvDragDropManager.DragData;
                            Debugger.Break();
                        }
                        //MpDragDropManager.DragData as MpClipTileViewModel;

                        MpPortableDataObject mpdo = await ctvm.ConvertToPortableDataObject(true);

                        if(mpdo == null) {
                            // template paste was canceled
                            Reset();
                            return;
                        }

                        MpAvClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(mpdo);

                        while (MpAvClipTrayViewModel.Instance.SelectedItem.IsPasting) {
                            await Task.Delay(100);
                        }
                        Reset();
                    });
                } else {
                    //e.Action = DragAction.Drop;
                    e.DragEffects = DragDropEffects.Copy;
                }                
            }
        }

        public override async Task Drop(bool isCopy, object dragData) {
            //Application.Current.MainWindow.Activate();
            //Application.Current.MainWindow.Focus();
            //Application.Current.MainWindow.Topmost = true;
            //Application.Current.MainWindow.Top = 0;
            MpAvMainWindow.Instance.Activate();
            MpAvMainWindow.Instance.Focus();
            MpAvMainWindow.Instance.Topmost = true;

            while (IsPreExternalTemplateDrop) {
                await Task.Delay(100);
            }
            MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);

            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.ExternalDragEnd);

            Reset();
        }

        public override void CancelDrop() {
            base.CancelDrop();
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.ExternalDragEnd);
        }

        public override void AutoScrollByMouse() {
            return;
        }

        public override void Reset() {
            base.Reset();

            IsPreExternalTemplateDrop = false;
            _wasReset = true;

            
        }
    }

}
