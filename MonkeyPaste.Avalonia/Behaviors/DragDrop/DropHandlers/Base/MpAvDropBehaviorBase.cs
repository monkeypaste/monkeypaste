using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
using System.Diagnostics;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls.Primitives;

namespace MonkeyPaste.Avalonia {
    public enum MpDropType {
        None,
        Content,
        PinTray,
        External,
        Action
    }

    public abstract class MpAvDropBehaviorBase<T> : MpAvBehavior<T>, MpAvIContentDropTargetAsync where T : Control {
        #region Private Variables
        
        private AdornerLayer adornerLayer;

        #endregion

        

        #region Properties

        public MpAvContentAdorner DropLineAdorner { get; set; }

        public int DropIdx { get; set; } = -1;

        public object DataContext => AssociatedObject?.DataContext;

        //public List<MpRect> DropRects => GetDropTargetRectsAsync();

        private bool _isDebugEnabled = false;
        public bool IsDebugEnabled {
            get => _isDebugEnabled;
            set {
                if(_isDebugEnabled != value) {
                    _isDebugEnabled = value;
                    Task.Run(async () => {
                        while (!_isLoaded) { await Task.Delay(100); }

                        Dispatcher.UIThread.Post(()=> {
                            UpdateAdorner();
                        });
                    });                    
                }
            }
        }
        #endregion

        #region Abstracts

        public abstract bool IsDropEnabled { get; set; }
        public abstract MpDropType DropType { get; }

        public abstract Control RelativeToElement { get; }

        public abstract Control AdornedElement { get; }
        public abstract Orientation AdornerOrientation { get; }

        public abstract MpCursorType MoveCursor { get; }
        public abstract MpCursorType CopyCursor { get; }
        public abstract Task<List<MpRect>> GetDropTargetRectsAsync();
        public abstract void AutoScrollByMouse();

        #endregion

        public MpAvDropBehaviorBase() {
            IsDebugEnabled = false;
            MpConsole.WriteLine(GetType() + " behavior created");
        }

        protected override void OnAttached() {
            base.OnAttached();

            //MpMainWindowViewModel.Instance.OnMainWindowHidden += MainWindowViewModel_OnMainWindowHide;

            AssociatedObject.AttachedToVisualTree += AssociatedObject_Loaded;
            AssociatedObject.DetachedFromVisualTree += AssociatedObject_Unloaded;

            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }
        private void AssociatedObject_DataContextChanged(object sender, EventArgs e) {
            if(AssociatedObject?.DataContext != null) {
                Attach(AssociatedObject);
            }
        }

        private void AssociatedObject_Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Detach();
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            if(AssociatedObject != null) {
                AssociatedObject.AttachedToVisualTree -= AssociatedObject_Loaded;
                AssociatedObject.DetachedFromVisualTree -= AssociatedObject_Unloaded;
            }
            OnUnloaded();
        }

        private void AssociatedObject_Loaded(object sender, VisualTreeAttachmentEventArgs e) {            
            OnLoaded();
        }

        protected override void OnMainWindowHide(object sender, EventArgs e) {
            if(DropType == MpDropType.External) {
                return;
            }
            Reset();
        }

        protected virtual void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.JumpToIdxCompleted:
                    DropIdx = -1;
                    RefreshDropRects();
                    break;
                case MpMessageType.ResizeContentCompleted:
                    //comes from BOTH mainwindow resize and tile resize
                    RefreshDropRects();
                    break;
            }
        }
        
        public virtual void OnLoaded() {
            _dataContext = AssociatedObject.DataContext;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            InitAdorner();
        }

        public virtual void OnUnloaded() {
            MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);

        }

        #region External Drop Event Handlers

        public void OnDrop(object sender, DragEventArgs e) {
            if (e.Handled) {
                return;
            }
        }

        public void OnDragOver(object sender, DragEventArgs e) {
            //this is unset in drag drop manager global mouse move when mouse up
            MpAvDragDropManager.IsDraggingFromExternal = true;

            e.DragEffects = DragDropEffects.None;

            bool isValid = true;
            if (MpAvDragDropManager.DragData == null) {
                if (MpAvCefNetWebView.DraggingRtb != null) {
                    MpAvDragDropManager.IsDraggingFromExternal = false;
                    MpAvDragDropManager.SetDragData(MpAvCefNetWebView.DraggingRtb.DataContext);
                } else {
                    isValid = MpAvDragDropManager.PrepareDropDataFromExternalSource(e.Data);
                }                
            }

            if (isValid) {
                if (e.KeyModifiers.HasAnyFlag(KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift)) {
                    e.DragEffects = DragDropEffects.Copy;
                } else {
                    e.DragEffects = DragDropEffects.Move;
                }

                if (!MpAvDragDropManager.IsCheckingForDrag) {
                    MpAvDragDropManager.StartDragCheck(MpAvDragDropManager.DragData);
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.IsAnyItemDragging));
                }
            }
            e.Handled = true;
        }

        public void OnDragLeave(object sender, DragEventArgs e) {
            Reset();
        }

        #endregion

        #region MpIDropTarget Implementation        

        public virtual async Task DropAsync(bool isCopy, object dragData) {
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
            MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.IsAnyItemDragging));
        }

        public abstract Task StartDropAsync(); 

        public async Task ContinueDragOverTargetAsync() {
            int newDropIdx = await GetDropTargetRectIdxAsync();
            if(newDropIdx != DropIdx) {
                MpConsole.WriteLine("New dropIdx: " + newDropIdx);
            }
            DropIdx = newDropIdx;
            UpdateAdorner();
        }

        public virtual async Task<bool> IsDragDataValidAsync(bool isCopy, object dragData) {
            await Task.Delay(1);

            if (dragData == null) {
                return false;
            }
            if(dragData is MpPortableDataObject) {
                return true;
            }
            if(dragData is MpAvClipTileViewModel ctvm) {
                return ctvm.ItemType != MpCopyItemType.Image;
            }            
            return false;
        }

        public abstract Task<int> GetDropTargetRectIdxAsync();

        public abstract Task<MpShape[]> GetDropTargetAdornerShapeAsync();

        public void InitAdorner() {
            if(AdornedElement != null) {
                adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
                if(adornerLayer != null) {
                    var content_adorner = adornerLayer.Children.FirstOrDefault(x => x is MpAvContentAdorner ca && ca.AdornedControl == AdornedElement);
                    if(content_adorner == null) {
                        DropLineAdorner = new MpAvContentAdorner(AdornedElement, this);
                        adornerLayer.Children.Add(DropLineAdorner);
                        AdornerLayer.SetAdornedElement((Visual)DropLineAdorner, AdornedElement);
                        RefreshDropRects();
                    } else {
                        
                    }
                }
            }
        }

        public async Task UpdateRectsAsync() {
            if(adornerLayer == null) {
                return;
            }
            var content_adorner = adornerLayer.Children.FirstOrDefault(x => x is MpAvContentAdorner ca && ca.AdornedControl == AdornedElement);
            if(content_adorner is MpAvContentAdorner ca) {
                ca.DropRects = await GetDropTargetRectsAsync();
            }
        }
        public void UpdateAdorner() {
            Dispatcher.UIThread.Post(() => {
                if (adornerLayer == null) {
                    InitAdorner();
                }
                if(adornerLayer != null) {
                    var content_adorner = adornerLayer.Children.FirstOrDefault(x => x is MpAvContentAdorner ca && ca.AdornedControl == AdornedElement);
                    content_adorner?.InvalidateVisual();
                }
            });
        }

        #endregion


        protected void RefreshDropRects() {
            UpdateAdorner();
        }
        // sync


        //public bool IsDragDataValid(bool isCopy, object dragData) {
        //    if (dragData == null) {
        //        return false;
        //    }
        //    if (dragData is MpPortableDataObject) {
        //        return true;
        //    }
        //    if (dragData is MpAvClipTileViewModel ctvm) {
        //        return ctvm.ItemType != MpCopyItemType.Image;
        //    }
        //    return false;
        //}

        //public virtual void StartDrop() {
        //}

        //public void Drop(bool isCopy, object dragData) {
        //    throw new NotImplementedException();
        //}

        //public List<MpRect> GetDropTargetRects() {
        //    throw new NotImplementedException();
        //}

        //public int GetDropTargetRectIdx() {
        //    throw new NotImplementedException();
        //}

        //public MpShape[] GetDropTargetAdornerShape() {
        //    throw new NotImplementedException();
        //}

        //public void ContinueDragOverTarget() {
        //    throw new NotImplementedException();
        //}
    }

}
