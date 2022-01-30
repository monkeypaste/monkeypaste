using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Core;

namespace MpWpfApp {
    [Flags]
    public enum MpRectEdgeType {
        None = 0b_0000,
        Left = 0b_0001,
        Top = 0b_0010,
        Right = 0b_0100,
        Bottom = 0b_1000
    }

    public class MpResizeBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables

        private static bool _isAnyResizing = false;

        private DateTime _lastUpTime = DateTime.MinValue;

        private Size _startSize;

        private Point _lastMousePosition;

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

        public virtual MpRectEdgeType ResizableEdges { get; set; } = MpRectEdgeType.Left | MpRectEdgeType.Right | MpRectEdgeType.Top | MpRectEdgeType.Bottom;

        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            //DefaultWidth = AssociatedObject.RenderSize.Width;
            //DefaultHeight = AssociatedObject.RenderSize.Height;

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            MpMessenger.Register(this, MpClipTrayViewModel.Instance.ReceivedResizerBehaviorMessage);
            MpMessenger.Register(this, MpMainWindowViewModel.Instance.ReceivedResizerBehaviorMessage);
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

            MpMessenger.Send(MpMessageType.Resizing);
        }

        #endregion

        #region Private Methods


        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if(!IsResizing) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                CanResize = false;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (MpDragDropManager.Instance.IsDragAndDrop || (!IsResizing && _isAnyResizing)) {
                return;
            }

            var mwmp = e.GetPosition(Application.Current.MainWindow);

            Vector delta = _lastMousePosition - mwmp;
            _lastMousePosition = mwmp;

            if (IsResizing) {
                Resize(delta.X, delta.Y);
            } else if(!_isAnyResizing) {
                var rect = new Rect(0, 0, AssociatedObject.RenderSize.Width, AssociatedObject.RenderSize.Height);
                MpRectEdgeType edgeFlags = GetClosestEdgeOrCorner(rect, e.GetPosition(AssociatedObject));
                _curCursor = GetCursorByRectFlags(edgeFlags);
                if (_curCursor != MpCursorType.Default) {
                    MpCursorViewModel.Instance.CurrentCursor = _curCursor;
                    CanResize = true;
                } else {

                    MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                    CanResize = false;
                }
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (_curCursor != MpCursorType.Default) {
                IsResizing = AssociatedObject.CaptureMouse();

                if (IsResizing) {
                    _isAnyResizing = true;
                    _startSize = AssociatedObject.RenderSize;
                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //check for double click
            if (CanResize) {
                var lastClickDiff = DateTime.Now - _lastUpTime;
                if (lastClickDiff < TimeSpan.FromMilliseconds(1000) && lastClickDiff > TimeSpan.FromMilliseconds(1)) {
                    ResetToDefault();
                }
            }
            AssociatedObject.ReleaseMouseCapture();

            if (IsResizeCursor(MpCursorViewModel.Instance.CurrentCursor)) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }

            if (IsResizing) {
                _isAnyResizing = false;
                _curCursor = MpCursorType.None;
                IsResizing = false;
                MpMessenger.Send(MpMessageType.ResizeCompleted);
            }

            _lastUpTime = DateTime.Now;
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
