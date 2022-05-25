using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;
using System.Windows.Threading;
using System.Diagnostics;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public enum MpDropType {
        None,
        Content,
        Tile,
        Tray,
        External,
        Resize,
        Move,
        Action
    }

    public abstract class MpDropBehaviorBase<T> : MpBehavior<T>, MpIContentDropTarget where T : FrameworkElement {
        #region Private Variables
        
        private AdornerLayer adornerLayer;

        #endregion

        #region Properties

        public MpContentAdorner DropLineAdorner { get; set; }

        public int DropIdx { get; set; } = -1;

        public object DataContext => AssociatedObject?.DataContext;

        public List<Rect> DropRects => GetDropTargetRects();

        private bool _isDebugEnabled = false;
        public bool IsDebugEnabled {
            get => _isDebugEnabled;
            set {
                if(_isDebugEnabled != value) {
                    _isDebugEnabled = value;
                    Task.Run(async () => {
                        while (!_isLoaded) { await Task.Delay(100); }

                        MpHelpers.RunOnMainThread(UpdateAdorner);
                    });                    
                }
            }
        }
        #endregion

        #region Abstracts

        public abstract bool IsDropEnabled { get; set; }
        public abstract MpDropType DropType { get; }

        public abstract UIElement RelativeToElement { get; }

        public abstract FrameworkElement AdornedElement { get; }
        public abstract Orientation AdornerOrientation { get; }

        public abstract MpCursorType MoveCursor { get; }
        public abstract MpCursorType CopyCursor { get; }
        public abstract List<Rect> GetDropTargetRects();
        public abstract void AutoScrollByMouse();

        #endregion

        public MpDropBehaviorBase() { }

        protected override void OnAttached() {
            base.OnAttached();

            //MpMainWindowViewModel.Instance.OnMainWindowHidden += MainWindowViewModel_OnMainWindowHide;

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }

        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(AssociatedObject?.DataContext != null) {
                Attach(AssociatedObject);
            }
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            Detach();
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            if(AssociatedObject != null) {
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            }
            OnUnloaded();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {            
            OnLoaded();
        }

        protected override void OnMainWindowHide(object sender, EventArgs e) {
            if(DropType == MpDropType.External) {
                return;
            }
            Reset();
        }

        protected virtual void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.JumpToIdxCompleted:
                    DropIdx = -1;
                    RefreshDropRects();
                    break;
            }
        }
        
        protected virtual void ReceivedMainWindowResizeBehviorMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.ResizeContentCompleted:
                    //comes from BOTH mainwindow resize and tile resize
                    RefreshDropRects();
                    break;
            }
        }

        protected virtual void ReceivedMainWindowViewModelMessage(MpMessageType msg) { }

        public virtual void OnLoaded() {
            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Register<MpMessageType>(
                MpClipTrayViewModel.Instance, 
                ReceivedClipTrayViewModelMessage);


            MpMessenger.Register<MpMessageType>(
                MpMainWindowViewModel.Instance, 
                ReceivedMainWindowViewModelMessage);
                        

            MpMessenger.Register<MpMessageType>(
                (Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior,
                ReceivedMainWindowResizeBehviorMessage);

            InitAdorner();
        }

        public virtual void OnUnloaded() {
            MpMessenger.Unregister<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
            MpMessenger.Unregister<MpMessageType>(MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);

        }

        #region External Drop Event Handlers

        public void OnDrop(object sender, DragEventArgs e) {
            if (e.Handled) {
                return;
            }
            if (e.Data.GetDataPresent(MpPortableDataObject.InternalContentFormat)) {

            }
        }

        public void OnDragOver(object sender, DragEventArgs e) {
            //this is unset in drag drop manager global mouse move when mouse up
            MpDragDropManager.IsDraggingFromExternal = true;

            e.Effects = DragDropEffects.None;

            bool isValid = true;
            if (MpDragDropManager.DragData == null) {
                isValid = MpDragDropManager.PrepareDropDataFromExternalSource(e.Data);
            }

            if (isValid) {
                if (e.KeyStates == DragDropKeyStates.ControlKey ||
                   e.KeyStates == DragDropKeyStates.AltKey ||
                   e.KeyStates == DragDropKeyStates.ShiftKey) {
                    e.Effects = DragDropEffects.Copy;
                } else {
                    e.Effects = DragDropEffects.Move;
                }

                if (!MpDragDropManager.IsCheckingForDrag) {
                    MpDragDropManager.StartDragCheck(MpDragDropManager.DragData);
                }
            }
            e.Handled = true;
        }

        public void OnDragLeave(object sender, DragEventArgs e) {
            Reset();
        }

        #endregion

        #region MpIDropTarget Implementation        

        public virtual async Task Drop(bool isCopy, object dragData) {
            if (DropIdx < 0) {
                throw new Exception($"DropIdx {DropIdx} must be >= 0");
            }
            //MpClipTrayViewModel.Instance.PersistentSelectedModels = dragData as List<MpCopyItem>;
            
            
            await Task.Delay(1);
        }

        public virtual void CancelDrop() {
            Reset();
        }

        public virtual void Reset() {
            DropIdx = -1;
            UpdateAdorner();
        }

        public abstract Task StartDrop(); 

        public void ContinueDragOverTarget() {
            int newDropIdx = GetDropTargetRectIdx();
            if(newDropIdx != DropIdx) {
                MpConsole.WriteLine("New dropIdx: " + newDropIdx);
            }
            DropIdx = newDropIdx;
            UpdateAdorner();
        }

        public virtual bool IsDragDataValid(bool isCopy, object dragData) {
            if (dragData == null) {
                return false;
            }
            if(dragData is MpPortableDataObject) {
                return true;
            }
            if(dragData is MpClipTileViewModel ctvm) {
                return ctvm.ItemType != MpCopyItemType.Image;
            }
            if (dragData is List<MpCopyItem> dcil) {
                if (dcil.Count == 0) {
                    return false;
                }
                return dcil.All(x => x.ItemType == dcil[0].ItemType);
            } 
            return false;
        }

        public abstract int GetDropTargetRectIdx();

        public abstract MpShape[] GetDropTargetAdornerShape();

        public void InitAdorner() {
            if(AdornedElement != null) {
                adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
                if(adornerLayer != null) {
                    DropLineAdorner = new MpContentAdorner(AdornedElement, this);
                    adornerLayer.Add(DropLineAdorner);
                    RefreshDropRects();
                }
            }            

        }

        public void UpdateAdorner() {
            if (adornerLayer == null) {
                InitAdorner();
            }
            adornerLayer?.Update();
        }

        #endregion

        protected void RefreshDropRects() {
            UpdateAdorner();
        }

        protected async Task<List<MpCopyItem>> GetDragDataCopy(object dragData) {
            List<MpCopyItem> cil = null;
            if(dragData is List<MpCopyItem>) {
                cil = dragData as List<MpCopyItem>;
            } else if(dragData is MpClipTileViewModel ctvm) {
                cil = new List<MpCopyItem>() { ctvm.CopyItem };
            }
            var clones = (await Task.WhenAll(cil.Select(x => x.Clone(true)).ToArray())).Cast<MpCopyItem>().ToList();
            MpClipTrayViewModel.Instance.PersistentSelectedModels = clones;
            return clones;
        }
    }

}
