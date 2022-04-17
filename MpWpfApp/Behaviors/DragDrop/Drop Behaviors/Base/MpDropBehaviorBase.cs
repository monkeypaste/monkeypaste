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

        public MpDropShapeAdorner DropLineAdorner { get; set; }

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

            MpMainWindowViewModel.Instance.OnMainWindowHidden += MainWindowViewModel_OnMainWindowHide;

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

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            if(DropType == MpDropType.External) {
                return;
            }
            Reset();
        }

        protected virtual void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch(msg) {
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

        #region MpIDropTarget Implementation        

        public virtual async Task Drop(bool isCopy, object dragData) {
            if (DropIdx < 0) {
                throw new Exception($"DropIdx {DropIdx} must be >= 0");
            }
            //MpClipTrayViewModel.Instance.PersistentSelectedModels = dragData as List<MpCopyItem>;
            
            
            await Task.Delay(1);
        }

        public void CancelDrop() {
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
            if(dragData is MpClipTileViewModel ctvm) {
                return true;
            }
            if (dragData is List<MpCopyItem> dcil) {
                if (dcil.Count == 0) {
                    return false;
                }
                return dcil.All(x => x.ItemType == dcil[0].ItemType);
            } 

            if (dragData is List<MpContentItemViewModel> dcivml) {
                if (dcivml.Count == 0) {
                    return false;
                }
                return dcivml.All(x => x.CopyItemType == dcivml[0].CopyItemType);
            } 

            return false;
        }

        public virtual int GetDropTargetRectIdx() {
            Point trayMp = Mouse.GetPosition(RelativeToElement);

            Rect targetRect = DropRects.FirstOrDefault(x => x.Contains(trayMp));
            if (targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return DropRects.IndexOf(targetRect);
        }

        public abstract MpShape GetDropTargetAdornerShape();

        public void InitAdorner() {
            if(AdornedElement != null) {
                adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
                if(adornerLayer != null) {
                    DropLineAdorner = new MpDropShapeAdorner(AdornedElement, this);
                    adornerLayer.Add(DropLineAdorner);
                    RefreshDropRects();
                }
            }            

            IsDebugEnabled = false;

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

            var clones = (await Task.WhenAll((dragData as List<MpCopyItem>).Select(x => x.Clone(true)).ToArray())).Cast<MpCopyItem>().ToList();
            MpClipTrayViewModel.Instance.PersistentSelectedModels = clones;
            return clones;
        }

        protected async Task<List<MpCopyItem>> Detach(List<MpCopyItem> dragModels, bool ignoreOffset = false) {
            for (int i = 0; i < dragModels.Count; i++) {
                if (dragModels[i].CompositeParentCopyItemId == 0) {
                    //if dropping a former composite parent into non-parent idx
                    var oldTile = MpClipTrayViewModel.Instance.GetClipTileViewModelById(dragModels[i].Id);
                    int oldIdx = oldTile == null ? -1 : MpClipTrayViewModel.Instance.Items.IndexOf(oldTile);

                    var newHead = await MpDataModelProvider.RemoveQueryItem(dragModels[i].Id);
                    bool wasRemoved = newHead == null;
                    if (!wasRemoved && dragModels.Any(x => x.Id == newHead.Id)) {
                        //if first child was substituted as parent and drag contains
                        //new parent update dragModels
                        dragModels[dragModels.IndexOf(dragModels.FirstOrDefault(x => x.Id == newHead.Id))] = newHead;
                    }
                    bool needToOffset = !ignoreOffset && wasRemoved && oldIdx < DropIdx;
                    if (needToOffset) {
                        DropIdx--;
                    }
                }
                dragModels[i].CompositeSortOrderIdx = i;
                if (i == 0) {
                    dragModels[i].CompositeParentCopyItemId = 0;
                } else {
                    dragModels[i].CompositeParentCopyItemId = dragModels[0].Id;
                }
                await dragModels[i].WriteToDatabaseAsync();
            }
            return dragModels;
        }
    }

}
