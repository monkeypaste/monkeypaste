﻿using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    
public class MpObservableCollection<T> : ObservableCollection<T> {
        private object _ItemsLock = new object();
        //public override event NotifyCollectionChangedEventHandler CollectionChanged;

        //public new void Add(T element) {
        //    base.Add(element);
        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,element,0));
        //}
        //public new void Insert(int idx, T element) {
        //    base.Insert(idx,element);
        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,element,idx));
        //}
        //public new void Clear() {
        //    base.Clear();
        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        //}

        //public new void Remove(T element) {
        //    int removedIdx = this.IndexOf(element);
        //    base.Remove(element);
        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,element,removedIdx));
        //}

        //public new void Move(int oldIdx, int newIdx) {
        //    //var movingItem = this[oldIdx];
        //    //var movedItem = this[newIdx];
        //    //var changedItems = new List<T> { this[oldIdx], this[newIdx] };
        //    //base.Move(oldIdx, newIdx);
        //    //CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        //    //CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movingItem, newIdx, oldIdx));
        //    //CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItems));
        //    //var changedItems = new List<T> { this[oldIdx], this[newIdx] };
        //    var movedItem = this[oldIdx];
        //    this.Remove(movedItem);
        //    if (newIdx < this.Count) {
        //        if(newIdx == 0) {
        //            this.Insert(0, movedItem);
        //        } else {
        //            this.Insert(newIdx-1, movedItem);
        //        }                
        //    } else if (newIdx >= this.Count) {
        //        this.Add(movedItem);
        //    }            
        //}

        public MpObservableCollection() : base() {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }
        public MpObservableCollection(List<T> list) : base(list) {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }
        public MpObservableCollection(IEnumerable<T> collection) : base(collection) {
            BindingOperations.EnableCollectionSynchronization(this, _ItemsLock);
        }

        //public override void OnCollectionChanged(NotifyCollectionChangedEventArgs args) {
        //    base.OnCollectionChanged(args);
        //}
    }
    public class MpObservableCollectionViewModel<T> : MpObservableCollection<T>, IDisposable where T : class {
        #region Private Variables
        
        #endregion

        #region View Models
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return (MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext;
            }
        }
        #endregion

        #region Properties

        #region Controls
        private ListBox _listBox = null;
        public ListBox ListBox {
            get {
                return _listBox;
            }
            set {
                if (_listBox != value) {
                    _listBox = value;
                    OnPropertyChanged(nameof(ListBox));
                }
            }
        }

        private ScrollViewer _scrollViewer = null;
        public ScrollViewer ScrollViewer {
            get {
                return _scrollViewer;
            }
            set {
                if(_scrollViewer != value) {
                    _scrollViewer = value;
                    OnPropertyChanged(nameof(ScrollViewer));
                }
            }
        }

        //public List<ListBoxItem> VisibleListBoxItems {
        //    get {
        //        var visibileItems = new List<ListBoxItem>();
        //        if (this.ListBox == null) {
        //            return visibileItems;
        //        }
        //        for (int i = 0; i < this.ListBox.Items.Count; i++) {
        //            var lbi = GetListBoxItem(i);
        //            if (lbi.Visibility == Visibility.Visible) {
        //                visibileItems.Add(lbi);
        //            }
        //        }
        //        return visibileItems;
        //    }
        //}
        #endregion

        #region Visibility
        
        #endregion

        #region State
        public bool IsHorizontal { get; set; } = false;

        private bool _isMouseDraggingFromScrollBar = false;
        public bool IsMouseDraggingFromScrollBar {
            get {
                return _isMouseDraggingFromScrollBar;
            }
            set {
                if (_isMouseDraggingFromScrollBar != value) {
                    _isMouseDraggingFromScrollBar = value;
                    OnPropertyChanged(nameof(IsMouseDraggingFromScrollBar));
                }
            }
        }


        private bool _isTrialExpired = Properties.Settings.Default.IsTrialExpired;
        public bool IsTrialExpired {
            get {
                return _isTrialExpired;
            }
            set {
                if (_isTrialExpired != value) {
                    _isTrialExpired = value;
                    Properties.Settings.Default.IsTrialExpired = _isTrialExpired;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(IsTrialExpired));
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy {
            get {
                return _isBusy;
            }
            protected set {
                if (_isBusy != value) {
                    _isBusy = value;
                    Application.Current.MainWindow.Cursor = IsBusy ? Cursors.Wait : Cursors.Arrow;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        private static bool _designMode = false;
        protected bool IsInDesignMode {
            get {
                return _designMode;
            }
        }

        public int IndexUnderDragCursor {
            get {
                if (this.ListBox == null) {
                    // means there is no index under the cursor
                    return -1;
                } 
                var mp = MpHelpers.Instance.GetMousePosition(ListBox);
                var listBoxRect = GetListBoxRect();
                if (listBoxRect.Contains(mp)) {
                    for (int i = 0; i < this.ListBox.Items.Count; i++) {
                        var item = this.GetListBoxItem(i);
                        var itemRect = GetListBoxItemRect(i);
                        mp = MpHelpers.Instance.GetMousePosition(item);
                        if (itemRect.Contains(mp)) {
                            if (IsHorizontal) {
                                if (mp.X > itemRect.Width / 2) {
                                    return i + 1;
                                }
                                return i;
                            } else {
                                if (mp.Y > itemRect.Height / 2) {
                                    return i + 1;
                                }
                                return i;
                            }
                        }
                    }
                    mp = MpHelpers.Instance.GetMousePosition(ListBox);
                    if(mp.X >= GetListBoxItemRect(0).Left && mp.X <= GetListBoxItemRect(ListBox.Items.Count-1).Right) {
                        double minDist = double.MaxValue;
                        int minIdx = -1;
                        for (int i = 0; i < this.ListBox.Items.Count; i++) {
                            var item = this.GetListBoxItem(i);
                            var itemRect = this.GetListBoxItemRect(i);
                            double lowDist = 0;
                            double highDist = 0;
                            double curDist = double.MaxValue;
                            if (IsHorizontal) {
                                double lbilx = itemRect.Left;
                                double lbirx = itemRect.Right;
                                lowDist = Math.Abs(mp.X - lbilx);
                                highDist = Math.Abs(mp.X - lbirx);
                                curDist = Math.Min(lowDist, highDist);
                            } else {
                                double lbity = itemRect.Top;
                                double lbiby = itemRect.Bottom;
                                lowDist = Math.Abs(mp.Y - lbity);
                                highDist = Math.Abs(mp.Y - lbiby);
                                curDist = Math.Min(lowDist, highDist);
                            }
                            if (curDist < minDist) {
                                minDist = curDist;
                                if (minDist == lowDist) {
                                    minIdx = i;
                                } else {
                                    minIdx = i + 1;
                                }
                            }
                        }
                        if (minIdx >= 0) {
                            return minIdx;
                        }
                    }
                    if(mp.X > GetListBoxItemRect(0).Left) {
                        return this.ListBox.Items.Count;
                    }
                    return 0;
                } else {
                    if (IsHorizontal) {
                        return ListBox.Items.Count;
                        //var mp = MpHelpers.Instance.GetMousePosition(ListBox);
                        //var listBoxBounds = VisualTreeHelper.GetContentBounds(ListBox);
                        //if (mp.X <= listBoxBounds.Left) {
                        //    return 0;
                        //}
                        //if(mp.X >= listBoxBounds.Right) {
                        //    return ListBox.Items.Count;
                        //}
                    }
                }
                return -1;
            }
        }
        #endregion

        #region Business Logic
        private string _name = string.Empty;
        public string Name {
            get {
                return _name;
            }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        #endregion

        #region Layout

        #endregion

        #endregion

        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Public Methods
        public MpObservableCollectionViewModel() : base() { }
        public MpObservableCollectionViewModel(List<T> list) : base(list) { }
        public MpObservableCollectionViewModel(IEnumerable<T> collection) : base(collection) { }

        public bool IsListBoxItemVisible(int index) {
            var lbi = GetListBoxItem(index);
            if (lbi != null && lbi.Visibility == Visibility.Visible) {
                if(GetListBoxItemRect(index).Left < ScrollViewer.HorizontalOffset) {
                    return false;
                }
                if (GetListBoxItemRect(index).Right > GetListBoxRect().Right + ScrollViewer.HorizontalOffset) {
                    return false;
                }
                return true;
            }
            return false;
        }

        public ListBoxItem GetListBoxItem(int index) {
            if (this.ListBox == null) {
                return null;
            }
            if (this.ListBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
                return null;
            }
            if (index < 0 || index >= this.ListBox.Items.Count) {
                return null;
            }
            return this.ListBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        public ListBoxItem GetListBoxItem(T item) {
            if (this.ListBox == null) {
                return null;
            }
            if (this.ListBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) {
                return null;
            }
            if(!ListBox.Items.Contains(item)) {
                return null;
            }
            return GetListBoxItem(ListBox.Items.IndexOf(item));
        }

        public Rect GetListBoxRect() {
            if(ListBox == null) {
                return new Rect();
            }
            return new Rect(new Point(0,0), new Size(ListBox.ActualWidth, ListBox.ActualHeight));
        }

        public Rect GetListBoxItemRect(int index) {
            var lbi = GetListBoxItem(index);
            if (lbi == null || lbi.Visibility != Visibility.Visible) {
                return new Rect();
            }
            Point origin = new Point();
            if(ScrollViewer.HorizontalOffset > 0 || ScrollViewer.VerticalOffset > 0) {
                origin = lbi.TranslatePoint(new Point(0, 0), ScrollViewer);
            } else {
                origin = lbi.TranslatePoint(new Point(0, 0), ListBox);
            }
            //origin.X -= ScrollViewer.HorizontalOffset;
           //origin.Y -= ScrollViewer.VerticalOffset;
            return new Rect(origin, new Size(lbi.ActualWidth,lbi.ActualHeight));
        }

        public Point[] GetAdornerPoints(int index) {
            var points = new Point[2];
            var itemRect = index >= this.Count ? GetListBoxItemRect(this.Count - 1) : GetListBoxItemRect(index);
            if(!IsHorizontal) {
                itemRect.Height = MpMeasurements.Instance.RtbCompositeItemMinHeight;
            }
            if (IsHorizontal) {
                if(index < this.Count) {
                    points[0] = itemRect.TopLeft;
                    points[1] = itemRect.BottomLeft;
                } else {
                    points[0] = itemRect.TopRight;
                    points[1] = itemRect.BottomRight;
                }
            }  else {
                if (index < this.Count) {
                    points[0] = itemRect.TopLeft;
                    points[1] = itemRect.TopRight;
                } else {
                    points[0] = itemRect.BottomLeft;
                    points[1] = itemRect.BottomRight;
                }
            }
            if (ScrollViewer != null && 
                (ScrollViewer.HorizontalOffset > 0 || ScrollViewer.VerticalOffset > 0)) {
                points[0].X += ScrollViewer.Margin.Right;
                //points[0].Y += ScrollViewer.VerticalOffset;
                points[1].X += ScrollViewer.Margin.Right;
                //points[1].Y += ScrollViewer.VerticalOffset;
            } 
            return points;
        }

        public void UpdateExtendedSelection(int index) {
            /*
            1    if the target item is not selected, select it
            2    if Ctrl key is down, add target item to selection 
            3    if Shift key is down
            4    if there is a previously selected item, add all items between target item and most recently selected item to selection, clearing any others
            5    else add item and all previous items
            6    if the target item is selected de-select only if Ctrl key is down         
            7    if neither ctrl nor shift are pressed clear any other selection
            8    if the target item is selected
            9    if Ctrl key is down, remove item from selection
            10   if Shift key is down
            11   if there is a previously selected item, clear selection and then add between target item and first previously selected item
            12   else remove any other item from selection
            */
            //if (ListBox.DataContext is MpClipTileRichTextBoxViewModelCollection) {
            //    var hctvm = (ListBox.DataContext as MpClipTileRichTextBoxViewModelCollection).HostClipTileViewModel;
            //    MainWindowViewModel.ClipTrayViewModel.UpdateExtendedSelection(MainWindowViewModel.ClipTrayViewModel.IndexOf(hctvm));
            //}
            bool isCtrlDown = MpHelpers.Instance.GetModKeyDownList().Contains(Key.LeftCtrl);
            bool isShiftDown = MpHelpers.Instance.GetModKeyDownList().Contains(Key.LeftShift);
            var lbi = GetListBoxItem(index);
            if (!lbi.IsSelected) {
                ListBoxItem lastSelectedItem = null;
                if (ListBox.SelectedItems.Count > 0) {
                    // NOTE this maybe the wrong item
                    lastSelectedItem = (ListBoxItem)GetListBoxItem((T)ListBox.SelectedItems[ListBox.SelectedItems.Count - 1]);
                }
                if (isShiftDown) {
                    if (lastSelectedItem == null) {
                        //5 else add item and all previous items
                        for (int i = 0; i <= index; i++) {
                            GetListBoxItem(i).IsSelected = true;
                        }
                        return;
                    } else {
                        //4 if there is a previously selected item, add all items between target
                        //  item and most recently selected item to selection, clearing any others
                        ListBox.SelectedItems.Clear();

                        int lastIdx = ListBox.Items.IndexOf(lastSelectedItem.DataContext);
                        if (lastIdx < index) {
                            for (int i = lastIdx; i <= index; i++) {
                                GetListBoxItem(i).IsSelected = true;
                            }
                        } else {
                            for (int i = index; i <= lastIdx; i++) {
                                GetListBoxItem(i).IsSelected = true;
                            }
                        }
                    }
                } else if (isCtrlDown) {
                    //2    if Ctrl key is down, add target item to selection 
                    //6    if the target item is selected de-select only if Ctrl key is down

                    lbi.IsSelected = !lbi.IsSelected;
                } else {
                    //7    if neither ctrl nor shift are pressed clear any other selection
                    // MainWindowViewModel.ClipTrayViewModel.ClearClipSelection(false);
                    //HostClipTileViewModel.IsSelected = true;
                    ListBox.SelectedItems.Clear();
                    lbi.IsSelected = true;
                }
            } else if (lbi.IsSelected) {
                if (isShiftDown) {
                    //10   if Shift key is down
                    if (ListBox.SelectedItems.Count > 0) {
                        //11   if there is a previously selected item, remove all items between target item and previous item from selection
                        var firstSelectedItem = GetListBoxItem((T)ListBox.SelectedItems[0]);
                        int firstIdx = ListBox.Items.IndexOf(firstSelectedItem.DataContext);
                        ListBox.SelectedItems.Clear();
                        if (firstIdx < index) {
                            for (int i = firstIdx; i <= index; i++) {
                                GetListBoxItem(i).IsSelected = true;
                            }
                            return;
                        } else {
                            for (int i = index; i <= firstIdx; i++) {
                                GetListBoxItem(i).IsSelected = true;
                            }
                            return;
                        }
                    }

                } else if (isCtrlDown) {
                    //9    if Ctrl key is down, remove item from selection
                    lbi.IsSelected = false;
                } else {
                    //12   else remove any other item from selection

                    //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection(false);
                    //HostClipTileViewModel.IsSelected = true;
                    ListBox.SelectedItems.Clear();
                    lbi.IsSelected = true;
                }
            }
        }
        #endregion

        #region Protected Methods
        //protected MpObservableCollectionViewModel() : base() {
        //    OnInitialize();
        //}

        //protected virtual void OnInitialize() {
        //    _designMode = DesignerProperties.GetIsInDesignMode(new Button())
        //        || Application.Current == null || Application.Current.GetType() == typeof(Application);

        //    if (!_designMode) {
        //        var designMode = DesignerProperties.IsInDesignModeProperty;
        //        _designMode = (bool)DependencyPropertyDescriptor.FromProperty(designMode, typeof(FrameworkElement)).Metadata.DefaultValue;
        //    }

        //    if (_designMode) {
        //        DesignData();
        //    }
        //}

        /// <summary>
        /// With this method, we can inject design time data into the view so that we can
        /// create a more Blendable application.
        /// </summary>
        protected virtual void DesignData() { }


        #endregion

        #region INotifyPropertyChanged 
        public bool ThrowOnInvalidPropertyName { get; private set; } = true;

        //private event PropertyChangedEventHandler _vmPropertyChanged;
        //public event PropertyChangedEventHandler VmPropertyChanged {
        //    add { _vmPropertyChanged += value; }
        //    remove { _vmPropertyChanged -= value; }
        //} 

        public new event PropertyChangedEventHandler PropertyChanged {
            add { base.PropertyChanged += value; }
            remove { base.PropertyChanged -= value; }
        }

        public virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            //PropertyChangedEventHandler handler = _vmPropertyChanged;
            //if (handler != null) {
            //    var e = new PropertyChangedEventArgs(propertyName);
            //    handler(this, e);
            //}
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName) {
                    throw new Exception(msg);
                } else {
                    Debug.Fail(msg);
                }
            }
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            ListBox = null;
        }
        #endregion
    }

    #region MpListBoxItemDragState

    /// <summary>
    /// Exposes attached properties used in conjunction with the ListBoxDragDropManager class.
    /// Those properties can be used to allow triggers to modify the appearance of ListBoxItems
    /// in a ListBox during a drag-drop operation.
    /// </summary>
    public static class MpListBoxItemDragState {
        #region IsBeingDragged

        /// <summary>
        /// Identifies the MpListBoxItemDragState's IsBeingDragged attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsBeingDraggedProperty =
            DependencyProperty.RegisterAttached(
                "IsBeingDragged",
                typeof(bool),
                typeof(MpListBoxItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListBoxItem is being dragged, else false.
        /// </summary>
        /// <param name="item">The ListBoxItem to check.</param>
        public static bool GetIsBeingDragged(ListBoxItem item) {
            return (bool)item.GetValue(IsBeingDraggedProperty);
        }

        /// <summary>
        /// Sets the IsBeingDragged attached property for the specified ListBoxItem.
        /// </summary>
        /// <param name="item">The ListBoxItem to set the property on.</param>
        /// <param name="value">Pass true if the element is being dragged, else false.</param>
        internal static void SetIsBeingDragged(ListBoxItem item, bool value) {
            item.SetValue(IsBeingDraggedProperty, value);
        }

        #endregion // IsBeingDragged

        #region IsUnderDragCursor

        /// <summary>
        /// Identifies the MpListBoxItemDragState's IsUnderDragCursor attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached(
                "IsUnderDragCursor",
                typeof(bool),
                typeof(MpListBoxItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListBoxItem is currently underneath the cursor 
        /// during a drag-drop operation, else false.
        /// </summary>
        /// <param name="item">The ListBoxItem to check.</param>
        public static bool GetIsUnderDragCursor(ListBoxItem item) {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        /// <summary>
        /// Sets the IsUnderDragCursor attached property for the specified ListBoxItem.
        /// </summary>
        /// <param name="item">The ListBoxItem to set the property on.</param>
        /// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
        internal static void SetIsUnderDragCursor(ListBoxItem item, bool value) {
            item.SetValue(IsUnderDragCursorProperty, value);
        }

        #endregion // IsUnderDragCursor
    }

    #endregion // MpListBoxItemDragState
}