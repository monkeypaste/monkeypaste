using MonkeyPaste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpScrollContentPresenter : ContentPresenter, IScrollInfo {
        private ScrollContentPresenter _scp;

        public MpScrollContentPresenter() {
            _scp = new ScrollContentPresenter();
        }

        #region IScrollInfo

        public void LineUp() {
            _scp.LineUp();
        }

        public void LineDown() {
            _scp.LineDown();
        }

        public void LineLeft() {
            _scp.LineLeft();
        }

        public void LineRight() {
            _scp.LineRight();
        }

        public void PageUp() {
            _scp.PageUp();
        }

        public void PageDown() {
            _scp.PageDown();
        }

        public void PageLeft() {
            _scp.PageLeft();
        }

        public void PageRight() {
            _scp.PageRight();
        }

        public void MouseWheelUp() {
            _scp.MouseWheelUp();
        }

        public void MouseWheelDown() {
            _scp.MouseWheelDown();
        }

        public void MouseWheelLeft() {
            _scp.MouseWheelLeft();
        }

        public void MouseWheelRight() {
            _scp.MouseWheelRight();
        }

        public void SetHorizontalOffset(double offset) {
            _scp.SetHorizontalOffset(offset);
        }

        public void SetVerticalOffset(double offset) {
            _scp.SetVerticalOffset(offset);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            return _scp.MakeVisible(visual, rectangle);
        }

        public bool CanVerticallyScroll {
            get => _scp.CanVerticallyScroll;
            set => _scp.CanVerticallyScroll = value;
        }

        public bool CanHorizontallyScroll {
            get => _scp.CanHorizontallyScroll;
            set => _scp.CanHorizontallyScroll = value;
        }

        public double ExtentWidth => _scp.ExtentWidth;

        public double ExtentHeight => _scp.ExtentHeight;

        public double ViewportWidth => _scp.ViewportWidth;

        public double ViewportHeight => _scp.ViewportHeight;

        public double HorizontalOffset => _scp.HorizontalOffset;

        public double VerticalOffset => _scp.VerticalOffset;

        #endregion

        public bool CanContentScroll {
            get { return (bool)GetValue(CanContentScrollProperty); }
            set { SetValue(CanContentScrollProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanContentScroll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanContentScrollProperty =
            DependencyProperty.Register("CanContentScroll", typeof(bool), typeof(MpScrollContentPresenter), new PropertyMetadata(false));

        public AdornerLayer AdornerLayer => _scp.AdornerLayer;

        public ScrollViewer ScrollOwner {
            get => _scp.ScrollOwner;
            set => _scp.ScrollOwner = value;
        }

    }

    public class MpScrollViewer : ScrollViewer {
        private MpScrollContentPresenter _mpscp;

        protected new internal IScrollInfo ScrollInfo {
            get {
                if (_mpscp == null) {
                    _mpscp = new MpScrollContentPresenter();
                    _mpscp.ScrollOwner = this;
                }
                return _mpscp;
            }
            set {
                base.ScrollInfo = value;
            }
        }

        public MpScrollViewer() : base() {
            MpConsole.WriteLine("YOYOYO custom scroll viewer in da hoooouse");
        }
    }

    public enum MpTileStackingStrategy {
        Wrap,
        Tray,
        List
    }

    public class MpVirtualizingStackPanel : VirtualizingStackPanel, IScrollInfo {
        public new double ViewportWidth {
            get {
                return 1900;
            }
        }
        public MpVirtualizingStackPanel() : base() {
            return;
        }
        protected override void BringIndexIntoView(int index) {
            base.BringIndexIntoView(index);
        }

        public new void BringIndexIntoViewPublic(int index) {
            base.BringIndexIntoViewPublic(index);
        }
        public new Rect MakeVisible(Visual visual, Rect rectangle) {
            return base.MakeVisible(visual, rectangle);
        }
        protected override double GetItemOffsetCore(UIElement child) {
            return base.GetItemOffsetCore(child);
        }
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args) {
            base.OnItemsChanged(sender, args);
            return;
            switch(args.Action) {
                case NotifyCollectionChangedAction.Move:
                    MpConsole.WriteLine("Ignoring move change");
                    break;
                default:
                    base.OnItemsChanged(sender, args);
                    break;
            }
        }
    }

    public class MpVirtualizingPanel : VirtualizingPanel, IScrollInfo {
        #region Private Variables

        private TranslateTransform _trans = new TranslateTransform();
        private ScrollViewer _owner;
        private bool _canHScroll = false;
        private bool _canVScroll = false;
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset;

        private MpTileStackingStrategy _stackType = MpTileStackingStrategy.Tray;

        #endregion

        #region Properties

        #region Dependency Properties

        #region ChildWidth

        public static readonly DependencyProperty ChildWidthProperty =
            DependencyProperty.RegisterAttached(
                nameof(ChildWidth),
                typeof(double),
                typeof(MpVirtualizingStackPanel),
                new FrameworkPropertyMetadata(
                    MpMeasurements.Instance.ClipTileMinSize,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ChildWidth {
            get { return (double)GetValue(ChildWidthProperty); }
            set { SetValue(ChildWidthProperty, value); }
        }

        #endregion

        #region ChildHeight

        public static readonly DependencyProperty ChildHeightProperty =
            DependencyProperty.RegisterAttached(
                nameof(ChildHeight),
                typeof(double),
                typeof(MpVirtualizingStackPanel),
                new FrameworkPropertyMetadata(
                    MpMeasurements.Instance.ClipTileMinSize,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ChildHeight {
            get { return (double)GetValue(ChildHeightProperty) + 10; }
            set { SetValue(ChildHeightProperty, value); }
        }

        #endregion

        #region ChildCount

        public static readonly DependencyProperty ChildCountProperty =
            DependencyProperty.RegisterAttached(
                nameof(ChildCount),
                typeof(int),
                typeof(MpVirtualizingStackPanel),
                new FrameworkPropertyMetadata(
                    defaultValue: 0,
                    flags: FrameworkPropertyMetadataOptions.AffectsMeasure |
                           FrameworkPropertyMetadataOptions.AffectsArrange,
                    propertyChangedCallback: (sender,e) => {
                        return;
                    }));

        public int ChildCount {
            get { return (int)GetValue(ChildCountProperty); }
            set { SetValue(ChildCountProperty, value); }
        }
        #endregion

        #endregion

        public Size ChildSize => new Size(ChildWidth, ChildHeight);

        #endregion
        public MpVirtualizingPanel() {
            // For use in the IScrollInfo implementation
            this.RenderTransform = _trans;
        }

        /// <summary>
        /// Measure the children
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns>Size desired</returns>
        protected override Size MeasureOverride(Size availableSize) {
            UpdateScrollInfo(availableSize);

            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true)) {
                for (int i = 0; i < lastVisibleItemIndex; i++) {
                    bool newlyRealized;

                    // Get or create the child
                    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized) {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= children.Count) {
                            base.AddInternalChild(child);
                        } else {
                            base.InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    } else {
                        // The child has already been created, let's be sure it's in the right spot
                        //Debug.Assert(child == children[childIndex], "Wrong child was generated");
                    }

                    // Measurements will depend on layout algorithm
                    child.Measure(GetChildSize());
                }
                //for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; ++itemIndex, ++childIndex) {
                //    bool newlyRealized;

                //    // Get or create the child
                //    UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                //    if (newlyRealized) {
                //        // Figure out if we need to insert the child at the end or somewhere in the middle
                //        if (childIndex >= children.Count) {
                //            base.AddInternalChild(child);
                //        } else {
                //            base.InsertInternalChild(childIndex, child);
                //        }
                //        generator.PrepareItemContainer(child);
                //    } else {
                //        // The child has already been created, let's be sure it's in the right spot
                //        Debug.Assert(child == children[childIndex], "Wrong child was generated");
                //    }

                //    // Measurements will depend on layout algorithm
                //    child.Measure(GetChildSize());
                //}
            }

            // Note: this could be deferred to idle time for efficiency
            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);

            if(double.IsInfinity(availableSize.Width)) {
                availableSize.Width = 0;
            }
            if (double.IsInfinity(availableSize.Height)) {
                availableSize.Height = 0;
            }

            return availableSize;
        }

        /// <summary>
        /// Arrange the children
        /// </summary>
        /// <param name="finalSize">Size available</param>
        /// <returns>Size used</returns>
        protected override Size ArrangeOverride(Size finalSize) {
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            UpdateScrollInfo(finalSize);

            for (int i = 0; i < this.Children.Count; i++) {
                UIElement child = this.Children[i];

                // Map the child offset to an item offset
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child, finalSize);
            }

            return finalSize;
        }

        /// <summary>
        /// Revirtualize items that are no longer visible
        /// </summary>
        /// <param name="minDesiredGenerated">first item index that should be visible</param>
        /// <param name="maxDesiredGenerated">last item index that should be visible</param>
        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated) {
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            for (int i = children.Count - 1; i >= 0; i--) {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated) {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        /// When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args) {
            switch (args.Action) {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        }

        #region Layout specific code
        // I've isolated the layout specific code to this region. If you want to do something other than tiling, this is
        // where you'll make your changes

        /// <summary>
        /// Calculate the extent of the view based on the available size
        /// </summary>
        /// <param name="availableSize">available size</param>
        /// <param name="itemCount">number of data items</param>
        /// <returns></returns>
        private Size CalculateExtent(Size availableSize, int itemCount) {
            int childrenPerRow = CalculateChildrenPerRow(availableSize);

            // See how big we are
            return new Size(
                childrenPerRow * this.ChildWidth,
                this.ChildHeight * Math.Ceiling((double)itemCount / childrenPerRow));
        }

        /// <summary>
        /// Get the range of children that are visible
        /// </summary>
        /// <param name="firstVisibleItemIndex">The item index of the first visible item</param>
        /// <param name="lastVisibleItemIndex">The item index of the last visible item</param>
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex) {
            int childrenPerRow = CalculateChildrenPerRow(_extent);

            firstVisibleItemIndex = (int)Math.Floor(_offset.X / this.ChildWidth);// * childrenPerRow;
            lastVisibleItemIndex = (int)Math.Floor((_offset.X + _viewport.Width) / this.ChildWidth);// * childrenPerRow - 1;

            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount) {
                lastVisibleItemIndex = itemCount - 1;
            } else {
                lastVisibleItemIndex += MpClipTrayViewModel.Instance.RemainingItemsCountThreshold - 1;
            }
        }

        /// <summary>
        /// Get the size of the children. We assume they are all the same
        /// </summary>
        /// <returns>The size</returns>
        private Size GetChildSize() {
            return new Size(this.ChildWidth, this.ChildHeight);
        }

        /// <summary>
        /// Position a child
        /// </summary>
        /// <param name="itemIndex">The data item index of the child</param>
        /// <param name="child">The element to position</param>
        /// <param name="finalSize">The size of the panel</param>
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize) {
            int childrenPerRow = CalculateChildrenPerRow(finalSize);

            int row = itemIndex / childrenPerRow;
            int column = itemIndex % childrenPerRow;

            child.Arrange(new Rect(column * this.ChildWidth, row * this.ChildHeight, this.ChildWidth, this.ChildHeight));
        }

        /// <summary>
        /// Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns></returns>
        private int CalculateChildrenPerRow(Size availableSize) {
            return MpClipTrayViewModel.Instance.TotalItemsInQuery;
            // Figure out how many children fit on each row
            int childrenPerRow;
            if (availableSize.Width == Double.PositiveInfinity) {
                childrenPerRow = this.Children.Count;
            } else {
                childrenPerRow = Math.Max(1, MpClipTrayViewModel.Instance.TotalItemsInQuery); //(int)Math.Ceiling(availableSize.Width / this.ChildWidth));
            }
            return childrenPerRow;
        }

        #endregion

        #region IScrollInfo implementation
        // See Ben Constable's series of posts at http://blogs.msdn.com/bencon/


        private void UpdateScrollInfo(Size availableSize) {
            // See how many items there are
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            Size extent = CalculateExtent(availableSize, itemCount);
            // Update extent
            if (extent != _extent) {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }

            // Update viewport
            if (availableSize != _viewport) {
                _viewport = availableSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
        }

        public ScrollViewer ScrollOwner {
            get { return _owner; }
            set { _owner = value; }
        }

        public bool CanHorizontallyScroll {
            get { return _canHScroll; }
            set { _canHScroll = value; }
        }

        public bool CanVerticallyScroll {
            get { return _canVScroll; }
            set { _canVScroll = value; }
        }

        public double HorizontalOffset {
            get { return _offset.X; }
        }

        public double VerticalOffset {
            get { return _offset.Y; }
        }

        public double ExtentHeight {
            get { return _extent.Height; }
        }

        public double ExtentWidth {
            get { return _extent.Width; }
        }

        public double ViewportHeight {
            get { return _viewport.Height; }
        }

        public double ViewportWidth {
            get { return _viewport.Width; }
        }

        public void LineUp() {
            SetVerticalOffset(this.VerticalOffset - 10);
        }

        public void LineDown() {
            SetVerticalOffset(this.VerticalOffset + 10);
        }

        public void PageUp() {
            SetVerticalOffset(this.VerticalOffset - _viewport.Height);
        }

        public void PageDown() {
            SetVerticalOffset(this.VerticalOffset + _viewport.Height);
        }

        public void MouseWheelUp() {
            SetVerticalOffset(this.VerticalOffset - 10);
        }

        public void MouseWheelDown() {
            SetVerticalOffset(this.VerticalOffset + 10);
        }

        public void LineLeft() {
            throw new InvalidOperationException();
        }

        public void LineRight() {
            throw new InvalidOperationException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            return new Rect();
        }

        public void MouseWheelLeft() {
            throw new InvalidOperationException();
        }

        public void MouseWheelRight() {
            throw new InvalidOperationException();
        }

        public void PageLeft() {
            throw new InvalidOperationException();
        }

        public void PageRight() {
            throw new InvalidOperationException();
        }

        public void SetHorizontalOffset(double offset) {
            if (offset < 0 || _viewport.Width >= _extent.Width) {
                offset = 0;
            } else {
                if (offset + _viewport.Width >= _extent.Width) {
                    offset = _extent.Width - _viewport.Width;
                }
            }

            _offset.X = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.X = -offset;

            // Force us to realize the correct children
            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset) {
            if (offset < 0 || _viewport.Height >= _extent.Height) {
                offset = 0;
            } else {
                if (offset + _viewport.Height >= _extent.Height) {
                    offset = _extent.Height - _viewport.Height;
                }
            }

            _offset.Y = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.Y = -offset;

            // Force us to realize the correct children
            InvalidateMeasure();
        }

        #endregion

    }

    public class MpListBox : ListBox {

    }

}
