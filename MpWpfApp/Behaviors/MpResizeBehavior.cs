using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MpWpfApp {
    [Flags]
    public enum MpRectEdgeType {
        None = 0b_0000,
        Left = 0b_0001,
        Top = 0b_0010,
        Right = 0b_0100,
        Bottom = 0b_1000
    }

    public class MpResizeBehavior : MpDropBehaviorBase<FrameworkElement> {
        #region Private Variables
        private static List<MpIResizableViewModel> _allResizers = new List<MpIResizableViewModel>();

        public static bool IsAnyResizing => _allResizers.Any(x => x.IsResizing);
        public static bool CanAnyResize => _allResizers.Any(x => x.CanResize);

        private Point _lastMousePosition;
        private Point _mouseDownPosition;


        private MpCursorType _curCursor = MpCursorType.None;

        #endregion

        #region Properties

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

        #region DoubleClickControl DependencyProperty

        public Control DoubleClickControl {
            get { return (Control)GetValue(DoubleClickControlProperty); }
            set { SetValue(DoubleClickControlProperty, value); }
        }

        public static readonly DependencyProperty DoubleClickControlProperty =
            DependencyProperty.Register(
                "DoubleClickControl", typeof(Control),
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

        public virtual MpRectEdgeType ResizableEdge1 { get; set; }

        public virtual MpRectEdgeType ResizableEdge2 { get; set; }

        public virtual MpRectEdgeType ResizableEdges => ResizableEdge1 | ResizableEdge2;

        #endregion

        #region DropBehaviorBase Implementation

        public override bool IsEnabled { get; set; }
        public override MpDropType DropType => MpDropType.Resize;
        public override UIElement RelativeToElement => AssociatedObject;
        public override FrameworkElement AdornedElement => AssociatedObject;
        public override Orientation AdornerOrientation => Orientation.Horizontal;
        public override MpCursorType MoveCursor { get; }
        public override MpCursorType CopyCursor { get; }

        public override List<Rect> GetDropTargetRects() {
            var r = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
            var edgeRects = new List<Rect>();

            if (ResizableEdges.HasFlag(MpRectEdgeType.Left)) {
                Rect lr = new Rect(r.Left, r.Top, r.Left, r.Bottom);
                lr.Inflate(new Size(MaxDistance, MaxDistance));
                edgeRects.Add(lr);
            }
            if (ResizableEdges.HasFlag(MpRectEdgeType.Right)) {
                Rect rr = new Rect(r.Right, r.Top, r.Right, r.Bottom);
                rr.Inflate(new Size(MaxDistance, MaxDistance));
                edgeRects.Add(rr);
            }
            if (ResizableEdges.HasFlag(MpRectEdgeType.Top)) {
                Rect tr = new Rect(r.Left, r.Top, r.Right, r.Top);
                tr.Inflate(new Size(MaxDistance, MaxDistance));
                edgeRects.Add(tr);
            }
            if (ResizableEdges.HasFlag(MpRectEdgeType.Bottom)) {
                Rect br = new Rect(r.Left, r.Bottom, r.Right, r.Bottom);
                br.Inflate(new Size(MaxDistance, MaxDistance));
                edgeRects.Add(br);
            }
            return edgeRects;
        }

        public override async Task StartDrop() {
            await Task.Delay(1);
        }

        public override bool IsDragDataValid(bool isCopy, object dragData) {
            return base.IsDragDataValid(isCopy, dragData);
        }

        public override void AutoScrollByMouse() {
            
        }
        #endregion

        protected override void OnLoad() {
            base.OnLoad();
            if(AssociatedObject == null) {
                return;
            }
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            if(DoubleClickControl != null) {
                DoubleClickControl.MouseDoubleClick += DoubleClickButton_MouseDoubleClick;
            }

            MpMessenger.Register(this, MpClipTrayViewModel.Instance.ReceivedResizerBehaviorMessage);
            MpMessenger.Register(this, MpMainWindowViewModel.Instance.ReceivedResizerBehaviorMessage);

            if(AssociatedObject.DataContext is MpIResizableViewModel rvm) {
                if(_allResizers.Contains(rvm)) {
                    var old = _allResizers.FirstOrDefault(x => x == rvm);
                    MpConsole.WriteLine($"Duplicate resizer detected while loading, swapping for new... (old: '{old.GetType()}' new:'{rvm.GetType()}'");
                    _allResizers.Remove(rvm);
                }
                _allResizers.Add(rvm);
            }
        }

        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;


                if (AssociatedObject.DataContext is MpIResizableViewModel rvm) {
                    if (_allResizers.Contains(rvm)) {
                        _allResizers.Remove(rvm);
                    }
                }
            }
            

            if (DoubleClickControl != null) {
                DoubleClickControl.MouseDoubleClick -= DoubleClickButton_MouseDoubleClick;
            }

            MpMessenger.Unregister<MpMessageType>(this, MpClipTrayViewModel.Instance.ReceivedResizerBehaviorMessage);
            MpMessenger.Unregister<MpMessageType>(this, MpMainWindowViewModel.Instance.ReceivedResizerBehaviorMessage);

        }

        #region Public Methods

        public void ResetToDefault() {
            Point curSize = new Point(AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
            Point defaultSize = new Point(DefaultWidth == default ? curSize.X : DefaultWidth, DefaultHeight == default ? curSize.Y : DefaultHeight);
            Vector delta = curSize - defaultSize;
            Resize(delta.X, -delta.Y);
        }

        public void Resize(double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            if (ResizableEdges.HasFlag(MpRectEdgeType.Left) || ResizableEdges.HasFlag(MpRectEdgeType.Right)) {
                double nw = BoundWidth - dx;
                BoundWidth = Math.Min(Math.Max(nw, MinWidth), MaxWidth);
            }
            if (ResizableEdges.HasFlag(MpRectEdgeType.Top) || ResizableEdges.HasFlag(MpRectEdgeType.Bottom)) {
                double nh = BoundHeight + dy;
                BoundHeight = Math.Min(Math.Max(nh, MinHeight), MaxHeight);
            }

            if(AffectsContent) {
                MpMessenger.Send(MpMessageType.ResizingContent);
            }
        }

        #endregion

        #region Private Methods


        #region Manual Resize Event Handlers


        private void DoubleClickButton_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ResetToDefault();
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if(!IsResizing) {
                //MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                CanResize = false;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop) {
                return;
            }
            if(Mouse.LeftButton == MouseButtonState.Released) {
                IsResizing = false;
                AssociatedObject.ReleaseMouseCapture();
                return;
            }
            var mwmp = e.GetPosition(Application.Current.MainWindow);

            Vector delta = _lastMousePosition - mwmp;
            _lastMousePosition = mwmp;

            if (IsResizing) {
                Resize(delta.X, delta.Y);
            } else if(!IsAnyResizing) {

                var rect = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
                var lmp = e.GetPosition(AssociatedObject);
                MpRectEdgeType edgeFlags = GetClosestEdgeOrCorner(rect, lmp);
                _curCursor = GetCursorByRectFlags(edgeFlags);
                if (_curCursor != MpCursorType.Default) {
                    CanResize = true;
                    double totalDist = mwmp.Distance(_mouseDownPosition);
                    if(totalDist >= 5) {
                        IsResizing = AssociatedObject.CaptureMouse();

                        if (IsResizing) {
                            Resize(delta.X, delta.Y);
                        }
                    }
                } else {
                    CanResize = false;
                    if(!CanAnyResize) {
                        //MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                    }                    
                }
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            _mouseDownPosition = e.GetPosition(AssociatedObject);

            var rect = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
            var lmp = e.GetPosition(AssociatedObject);
            MpRectEdgeType edgeFlags = GetClosestEdgeOrCorner(rect, lmp);
            _curCursor = GetCursorByRectFlags(edgeFlags);
            if (_curCursor != MpCursorType.Default) {
                e.Handled = true;
            }
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();

            if (IsResizeCursor(MpCursorViewModel.Instance.CurrentCursor)) {
                //MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }

            if (IsResizing) {
                _curCursor = MpCursorType.None;
                IsResizing = false;
                if(AffectsContent) {
                    MpMessenger.Send(MpMessageType.ResizeContentCompleted);
                }
            }
        }

        #endregion

        private MpRectEdgeType GetClosestEdgeOrCorner(Rect rect, Point p) {
            if(!rect.Contains(p)) {
                return MpRectEdgeType.None;
            }
            Point[] corners = new Point[] { rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft };
            double[] edges = new double[] { rect.Left, rect.Top, rect.Right, rect.Bottom };

            List<double> distLookup = Enumerable.Repeat(double.MaxValue,8).ToList();

            foreach (MpRectEdgeType e in Enum.GetValues(typeof(MpRectEdgeType))) {
                if(e == MpRectEdgeType.None || !ResizableEdges.HasFlag(e)) {
                    continue;
                }
                int idx = Array.IndexOf(Enum.GetValues(typeof(MpRectEdgeType)), e) - 1;
                distLookup[idx] = p.Distance(corners[idx]);
                distLookup[idx + 4] = idx % 2 == 0 ? Math.Abs(p.X - edges[idx]) : Math.Abs(p.Y - edges[idx]);
            }


            double minDist = distLookup.Min();
            int[] minDistIdxs = distLookup.Where(x => x == minDist && x <= MaxDistance).Select(x=>distLookup.IndexOf(minDist)).ToArray();
            if (minDistIdxs.Contains(0)) {
                return MpRectEdgeType.Top | MpRectEdgeType.Left;
            }
            if (minDistIdxs.Contains(1)) {
                return MpRectEdgeType.Top | MpRectEdgeType.Right;
            }
            if (minDistIdxs.Contains(2)) {
                return MpRectEdgeType.Bottom | MpRectEdgeType.Right;
            }
            if (minDistIdxs.Contains(3)) {
                return MpRectEdgeType.Bottom | MpRectEdgeType.Left;
            }

            if (minDistIdxs.Contains(4)) {
                return MpRectEdgeType.Left;
            }
            if (minDistIdxs.Contains(5)) {
                return MpRectEdgeType.Top;
            }
            if (minDistIdxs.Contains(6)) {
                return MpRectEdgeType.Right;
            }
            if (minDistIdxs.Contains(7)) {
                return MpRectEdgeType.Bottom;
            }
            return MpRectEdgeType.None;
        }

        private MpCursorType GetCursorByRectFlags(MpRectEdgeType ret) {
            if(ret.HasFlag(MpRectEdgeType.Left) && ret.HasFlag(MpRectEdgeType.Top)) {
                return MpCursorType.ResizeNESW;
            }
            if (ret.HasFlag(MpRectEdgeType.Right) && ret.HasFlag(MpRectEdgeType.Bottom)) {
                return MpCursorType.ResizeNESW;
            }
            if (ret.HasFlag(MpRectEdgeType.Left) && ret.HasFlag(MpRectEdgeType.Bottom)) {
                return MpCursorType.ResizeNWSE;
            }
            if (ret.HasFlag(MpRectEdgeType.Right) && ret.HasFlag(MpRectEdgeType.Top)) {
                return MpCursorType.ResizeNWSE;
            }

            if (ret.HasFlag(MpRectEdgeType.Left) || ret.HasFlag(MpRectEdgeType.Right)) {
                return MpCursorType.ResizeWE;
            }
            if (ret.HasFlag(MpRectEdgeType.Top) || ret.HasFlag(MpRectEdgeType.Bottom)) {
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
