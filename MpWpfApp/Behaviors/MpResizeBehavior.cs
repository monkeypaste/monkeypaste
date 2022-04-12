using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    [Flags]
    public enum MpRectEdgeFlags {
        None = 0b_0000,
        Left = 0b_0001,
        Top = 0b_0010,
        Right = 0b_0100,
        Bottom = 0b_1000
    }

    public class MpResizeBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables
        private static List<MpResizeBehavior> _allResizers = new List<MpResizeBehavior>();

        private Point _lastMousePosition;
        private Point _mouseDownPosition;


        private MpCursorType _curCursor = MpCursorType.None;

        #endregion

        #region Properties

        public static bool IsAnyResizing => _allResizers
            .Where(x => x != null && x.AssociatedObject != null && x.AssociatedObject.DataContext != null)
            .Any(x => (x.AssociatedObject.DataContext as MpIResizableViewModel).IsResizing);

        public static bool CanAnyResize => _allResizers
            .Where(x => x != null && x.AssociatedObject != null && x.AssociatedObject.DataContext != null)
            .Any(x => (x.AssociatedObject.DataContext as MpIResizableViewModel).CanResize);

        #region IsResizing DependencyProperty

        public bool IsResizing {
            get { return (bool)GetValue(IsResizingProperty); }
            set { SetValue(IsResizingProperty, value); }
        }

        public static readonly DependencyProperty IsResizingProperty =
             DependencyProperty.Register(
                 "IsResizing", typeof(bool),
                 typeof(MpResizeBehavior),
                 new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region CanResize DependencyProperty

        public bool CanResize {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register(
                "CanResize", typeof(bool),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region MaxDistance DependencyProperty

        public double MaxDistance {
            get { return (double)GetValue(MaxDistanceProperty); }
            set { SetValue(MaxDistanceProperty, value); }
        }

        public static readonly DependencyProperty MaxDistanceProperty =
            DependencyProperty.Register(
                "MaxDistance", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(10.0d));

        #endregion

        #region DoubleClickFrameworkElement DependencyProperty

        public FrameworkElement DoubleClickFrameworkElement {
            get { return (FrameworkElement)GetValue(DoubleClickFrameworkElementProperty); }
            set { SetValue(DoubleClickFrameworkElementProperty, value); }
        }

        public static readonly DependencyProperty DoubleClickFrameworkElementProperty =
            DependencyProperty.Register(
                "DoubleClickFrameworkElement", typeof(FrameworkElement),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region BoundElement DependencyProperty

        public FrameworkElement BoundElement {
            get { return (FrameworkElement)GetValue(BoundElementProperty); }
            set { SetValue(BoundElementProperty, value); }
        }

        public static readonly DependencyProperty BoundElementProperty =
            DependencyProperty.Register(
                "BoundElement", typeof(FrameworkElement),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region Bound,Min,Max,Default Width/Height Dependency Properties

        #region BoundWidth DependencyProperty

        public double BoundWidth {
            get { return (double)GetValue(BoundWidthProperty); }
            set { SetValue(BoundWidthProperty, value); }
        }

        public static readonly DependencyProperty BoundWidthProperty =
            DependencyProperty.Register(
                "BoundWidth", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region BoundHeight DependencyProperty

        public double BoundHeight {
            get { return (double)GetValue(BoundHeightProperty); }
            set { SetValue(BoundHeightProperty, value); }
        }

        public static readonly DependencyProperty BoundHeightProperty =
            DependencyProperty.Register(
                "BoundHeight", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region MinWidth DependencyProperty

        public double MinWidth {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }

        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register(
                "MinWidth", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double)));

        #endregion

        #region MinHeight DependencyProperty

        public double MinHeight {
            get { return (double)GetValue(MinHeightProperty); }
            set { SetValue(MinHeightProperty, value); }
        }

        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register(
                "MinHeight", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double)));

        #endregion

        #region MaxWidth DependencyProperty

        public double MaxWidth {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.Register(
                "MaxWidth", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(double.MaxValue));

        #endregion

        #region MaxHeight DependencyProperty

        public double MaxHeight {
            get { return (double)GetValue(MaxHeightProperty); }
            set { SetValue(MaxHeightProperty, value); }
        }

        public static readonly DependencyProperty MaxHeightProperty =
            DependencyProperty.Register(
                "MaxHeight", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(double.MaxValue));

        #endregion

        #region DefaultWidth DependencyProperty

        public double DefaultWidth {
            get { return (double)GetValue(DefaultWidthProperty); }
            set { SetValue(DefaultWidthProperty, value); }
        }

        public static readonly DependencyProperty DefaultWidthProperty =
            DependencyProperty.Register(
                "DefaultWidth", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double)));

        #endregion

        #region DefaultHeight DependencyProperty

        public double DefaultHeight {
            get { return (double)GetValue(DefaultHeightProperty); }
            set { SetValue(DefaultHeightProperty, value); }
        }

        public static readonly DependencyProperty DefaultHeightProperty =
            DependencyProperty.Register(
                "DefaultHeight", typeof(double),
                typeof(MpResizeBehavior),
                new FrameworkPropertyMetadata(default(double)));

        #endregion

        #endregion

        public bool AffectsContent { get; set; } = false;

        public virtual MpRectEdgeFlags ResizableEdge1 { get; set; }

        public virtual MpRectEdgeFlags ResizableEdge2 { get; set; }

        public virtual MpRectEdgeFlags ResizableEdges => ResizableEdge1 | ResizableEdge2;

        #endregion

        //#region DropBehaviorBase Implementation

        //public override bool IsEnabled { get; set; } = true;
        //public override MpDropType DropType => MpDropType.Resize;
        //public override UIElement RelativeToElement => BoundElement;
        //public override FrameworkElement AdornedElement => AssociatedObject;
        //public override Orientation AdornerOrientation => Orientation.Horizontal;
        //public override MpCursorType MoveCursor { get; }
        //public override MpCursorType CopyCursor { get; }

        //public override List<Rect> GetDropTargetRects() {
        //    var r = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
        //    var edgeRects = new List<Rect>();

        //    if (ResizableEdges.HasFlag(MpRectEdgeFlags.Left)) {
        //        Rect lr = new Rect(r.Left, r.Top, r.Left, r.Bottom);
        //        lr.Inflate(new Size(MaxDistance, MaxDistance));
        //        edgeRects.Add(lr);
        //    }
        //    if (ResizableEdges.HasFlag(MpRectEdgeFlags.Right)) {
        //        Rect rr = new Rect(r.Right, r.Top, r.Right, r.Bottom);
        //        rr.Inflate(new Size(MaxDistance, MaxDistance));
        //        edgeRects.Add(rr);
        //    }
        //    if (ResizableEdges.HasFlag(MpRectEdgeFlags.Top)) {
        //        Rect tr = new Rect(r.Left, r.Top, r.Right, r.Top);
        //        tr.Inflate(new Size(MaxDistance, MaxDistance));
        //        edgeRects.Add(tr);
        //    }
        //    if (ResizableEdges.HasFlag(MpRectEdgeFlags.Bottom)) {
        //        Rect br = new Rect(r.Left, r.Bottom, r.Right, r.Bottom);
        //        br.Inflate(new Size(MaxDistance, MaxDistance));
        //        edgeRects.Add(br);
        //    }
        //    return edgeRects;
        //}

        //public override async Task StartDrop() {
        //    await Task.Delay(1);
        //}

        //public override bool IsDragDataValid(bool isCopy, object dragData) {
        //    return base.IsDragDataValid(isCopy, dragData);
        //}

        //public override void AutoScrollByMouse() {
            
        //}
        //#endregion

        protected override void OnLoad() {
            base.OnLoad();
            if(AssociatedObject == null) {
                return;
            }
            if (BoundElement == null) {
                BoundElement = AssociatedObject;
            }
            var resizableViewModel = AssociatedObject.DataContext as MpIResizableViewModel;
            if(resizableViewModel == null) {
                throw new Exception("This behavior requires data context to implement MpIResizableViewModel");
            }

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            if(DoubleClickFrameworkElement != null && DoubleClickFrameworkElement != AssociatedObject) {
                DoubleClickFrameworkElement.MouseLeftButtonDown += DoubleClickFrameworkElement_MouseLeftButtonDown;
            }            

            MpMessenger.Register(this, MpClipTrayViewModel.Instance.ReceivedResizerBehaviorMessage);
            MpMessenger.Register(this, MpMainWindowViewModel.Instance.ReceivedResizerBehaviorMessage);

            if (_allResizers.Contains(this)) {
                var old = _allResizers.FirstOrDefault(x => x == this);
                MpConsole.WriteLine($"Duplicate resizer detected while loading, swapping for new... (old: '{old.AssociatedObject.GetType()}' new:'{AssociatedObject.GetType()}'");
                _allResizers.Remove(old);
            }
            _allResizers.Add(this);
        }

        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            }

            if (_allResizers.Contains(this)) {
                _allResizers.Remove(this);
                MpConsole.WriteLine(@"Resizer of type " + AssociatedObject.GetType() + " unloaded");
            }

            if (DoubleClickFrameworkElement != null) {
                DoubleClickFrameworkElement.MouseLeftButtonDown -= DoubleClickFrameworkElement_MouseLeftButtonDown;
            }

            MpMessenger.Unregister<MpMessageType>(this, MpClipTrayViewModel.Instance.ReceivedResizerBehaviorMessage);
            MpMessenger.Unregister<MpMessageType>(this, MpMainWindowViewModel.Instance.ReceivedResizerBehaviorMessage);

        }

        #region Public Methods

        public void ResetToDefault() {
            Point curSize = new Point(BoundWidth,BoundHeight);
            Point defaultSize = new Point(
                DefaultWidth == default ? curSize.X : DefaultWidth, 
                DefaultHeight == default ? curSize.Y : DefaultHeight);

            Vector delta = curSize - defaultSize;
            Resize(-delta.X, -delta.Y);
        }

        public void Resize(double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            if(BoundWidth + dx < 0) {
                ResetToDefault();
                return;
            }
            double nw = BoundWidth + dx;
            BoundWidth = Math.Min(Math.Max(nw, MinWidth), MaxWidth);

            double nh = BoundHeight + dy;
            BoundHeight = Math.Min(Math.Max(nh, MinHeight), MaxHeight);

            //if (ResizableEdges.HasFlag(MpRectEdgeFlags.Left) || ResizableEdges.HasFlag(MpRectEdgeFlags.Right)) {
            //    double nw = BoundWidth + dx;
            //    BoundWidth = Math.Min(Math.Max(nw, MinWidth), MaxWidth);
            //}
            //if (ResizableEdges.HasFlag(MpRectEdgeFlags.Top) || ResizableEdges.HasFlag(MpRectEdgeFlags.Bottom)) {
            //    double nh = BoundHeight + dy;
            //    BoundHeight = Math.Min(Math.Max(nh, MinHeight), MaxHeight);
            //}

            if(AffectsContent) {
                MpMessenger.Send(MpMessageType.ResizingContent);
                if(!IsResizing) {
                    MpMessenger.Send(MpMessageType.ResizeContentCompleted);
                }
            }
        }

        #endregion

        #region Private Methods


        #region Manual Resize Event Handlers

        private void DoubleClickFrameworkElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop || AssociatedObject == null || !IsEnabled || MpIsFocusedExtension.IsAnyTextBoxFocused) {
                return;
            }
            if (e.ClickCount == 2) {
                ResetToDefault();
                if (AffectsContent) {
                    MpMessenger.Send(MpMessageType.ResizeContentCompleted);
                }
            }
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop || AssociatedObject == null || !IsEnabled) {
                return;
            }
            if (!IsResizing && !AssociatedObject.IsMouseCaptured) {
                //MpCursorStack.CurrentCursor = MpCursorType.Default;
                CanResize = false;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop || AssociatedObject == null || !IsEnabled) {
                return;
            }
            if(Mouse.LeftButton == MouseButtonState.Released) {
                if(IsResizing) {
                    IsResizing = false;
                }
                if(AssociatedObject.IsMouseCaptured) {
                    AssociatedObject.ReleaseMouseCapture();
                }
            }
            var mwmp = e.GetPosition(Application.Current.MainWindow);

            Vector delta =  mwmp - _lastMousePosition;
            _lastMousePosition = mwmp;

            if (AssociatedObject.DataContext is MpClipTileViewModel || AssociatedObject.DataContext is MpImageAnnotationViewModel) {
                //Debugger.Break();
            }
            if (IsResizing) {
                Resize(delta.X, -delta.Y);
            } else if(!IsAnyResizing) {
                var rect = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
                MpRectEdgeFlags edgeFlags = GetClosestEdgeOrCorner(rect, e.GetPosition(AssociatedObject));
                _curCursor = GetCursorByRectFlags(edgeFlags);
                if (_curCursor != MpCursorType.Default) {
                    CanResize = true;
                    if(Mouse.LeftButton == MouseButtonState.Pressed) {
                        double totalDist = mwmp.Distance(_mouseDownPosition);

                        if (totalDist >= 1) {
                            IsResizing = AssociatedObject.CaptureMouse();

                            if (IsResizing) {
                                Resize(delta.X, delta.Y);
                            }
                        }
                    }
                    
                } else {
                    CanResize = false;
                    if(!CanAnyResize) {
                        //MpCursorStack.CurrentCursor = MpCursorType.Default;
                    }                    
                }
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop || AssociatedObject == null || !IsEnabled) {
                return;
            }
            if (AssociatedObject == null) {
                return;
            }

            if (DoubleClickFrameworkElement != AssociatedObject && e.ClickCount == 2) {
                ResetToDefault();
            }
            _lastMousePosition = _mouseDownPosition = e.GetPosition(Application.Current.MainWindow);

            var rect = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);

            MpRectEdgeFlags edgeFlags = GetClosestEdgeOrCorner(rect, e.GetPosition(AssociatedObject));
            _curCursor = GetCursorByRectFlags(edgeFlags);
            if (_curCursor != MpCursorType.Default) {
                e.Handled = true;
            }
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!IsEnabled) {
                return;
            }
            if (AssociatedObject == null) {
                return;
            }
            AssociatedObject.ReleaseMouseCapture();

            if (IsResizing) {
                
                _curCursor = MpCursorType.None;
                IsResizing = false;
                if(AffectsContent) {
                    MpMessenger.Send(MpMessageType.ResizeContentCompleted);
                }
            }
        }

        #endregion

        private MpRectEdgeFlags GetClosestEdgeOrCorner(Rect rect, Point p) {
            if(!rect.Contains(p)) {
                return MpRectEdgeFlags.None;
            }
            Point[] corners = new Point[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft };
            double[] edges = new double[] { rect.Left, rect.Top, rect.Right, rect.Bottom };

            List<double> distLookup = Enumerable.Repeat(double.MaxValue,8).ToList();

            foreach (MpRectEdgeFlags e in Enum.GetValues(typeof(MpRectEdgeFlags))) {
                if(e == MpRectEdgeFlags.None || !ResizableEdges.HasFlag(e)) {
                    continue;
                }
                int idx = Array.IndexOf(Enum.GetValues(typeof(MpRectEdgeFlags)), e) - 1;
                distLookup[idx] = p.Distance(corners[idx]);
                distLookup[idx + 4] = idx % 2 == 0 ? Math.Abs(p.X - edges[idx]) : Math.Abs(p.Y - edges[idx]);
            }


            double minDist = distLookup.Min();
            int[] minDistIdxs = distLookup.Where(x => x == minDist && x <= MaxDistance).Select(x=>distLookup.IndexOf(minDist)).ToArray();
            if (minDistIdxs.Contains(0)) {
                return MpRectEdgeFlags.Top | MpRectEdgeFlags.Left;
            }
            if (minDistIdxs.Contains(1)) {
                return MpRectEdgeFlags.Top | MpRectEdgeFlags.Right;
            }
            if (minDistIdxs.Contains(2)) {
                return MpRectEdgeFlags.Bottom | MpRectEdgeFlags.Right;
            }
            if (minDistIdxs.Contains(3)) {
                return MpRectEdgeFlags.Bottom | MpRectEdgeFlags.Left;
            }

            if (minDistIdxs.Contains(4)) {
                return MpRectEdgeFlags.Left;
            }
            if (minDistIdxs.Contains(5)) {
                return MpRectEdgeFlags.Top;
            }
            if (minDistIdxs.Contains(6)) {
                return MpRectEdgeFlags.Right;
            }
            if (minDistIdxs.Contains(7)) {
                return MpRectEdgeFlags.Bottom;
            }
            return MpRectEdgeFlags.None;
        }


        private MpCursorType GetCursorByRectFlags(MpRectEdgeFlags ret) {
            if(ret.HasFlag(MpRectEdgeFlags.Left) && ret.HasFlag(MpRectEdgeFlags.Top)) {
                return MpCursorType.ResizeNESW;
            }
            if (ret.HasFlag(MpRectEdgeFlags.Right) && ret.HasFlag(MpRectEdgeFlags.Bottom)) {
                return MpCursorType.ResizeNESW;
            }
            if (ret.HasFlag(MpRectEdgeFlags.Left) && ret.HasFlag(MpRectEdgeFlags.Bottom)) {
                return MpCursorType.ResizeNWSE;
            }
            if (ret.HasFlag(MpRectEdgeFlags.Right) && ret.HasFlag(MpRectEdgeFlags.Top)) {
                return MpCursorType.ResizeNWSE;
            }

            if (ret.HasFlag(MpRectEdgeFlags.Left) || ret.HasFlag(MpRectEdgeFlags.Right)) {
                return MpCursorType.ResizeWE;
            }
            if (ret.HasFlag(MpRectEdgeFlags.Top) || ret.HasFlag(MpRectEdgeFlags.Bottom)) {
                return MpCursorType.ResizeNS;
            }

            return MpCursorType.Default;
        }

        private bool IsResizeCursor(MpCursorType ct) {
            return ct == MpCursorType.ResizeNESW || ct == MpCursorType.ResizeNS || ct == MpCursorType.ResizeNWSE || ct == MpCursorType.ResizeWE;
        }

        #endregion
    }
}
