using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvZoomBorder : Border {
        // from https://stackoverflow.com/a/6782715/105028
        #region Private Variables
        private DispatcherTimer _renderTimer = null;

        //private UIElement Child = null;

        private MpPoint tt_origin;
        private MpPoint mp_start;
                
        private MpPoint grid_offset {
            get {
                if(Child == null) {
                    return new MpPoint();
                }
                var tt = GetTranslateTransform(Child);
                return new MpPoint(tt.X, tt.Y);
            }
        }

        #endregion

        #region Statics
        static MpAvZoomBorder() {
            IsEnabledProperty.Changed.AddClassHandler<MpAvZoomBorder>((x, y) => HandleDesignerItemChanged(x, y));
        }
        public static bool IsTranslating { get; private set; } = false;

        #endregion

        #region Properties

        #region DesignerItem AvaloniaProperty
        public MpIDesignerItemSettingsViewModel DesignerItem {
            get { return (MpIDesignerItemSettingsViewModel)GetValue(DesignerItemProperty); }
            set { SetValue(DesignerItemProperty, value); }
        }

        public static readonly AttachedProperty<MpIDesignerItemSettingsViewModel> DesignerItemProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpIDesignerItemSettingsViewModel>(
                "DesignerItem",
                null,
                false);

        private static void HandleDesignerItemChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is MpAvZoomBorder zb) {
                zb.Reset();
            }
        }

        #endregion

        #region ShowGrid AvaloniaProperty
        public bool ShowGrid {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }

        public static readonly AttachedProperty<bool> ShowGridProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "ShowGrid",
                true,
                false);

        #endregion

        #region Bg Grid Properties

        public double MinScale { get; set; } = 0.1;
        public double MaxScale { get; set; } = 3;

        public IBrush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public IBrush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        #endregion

        //public override UIElement Child {
        //    get { return base.Child; }
        //    set {
        //        if (value != null && value != this.Child) {
        //            this.Initialize(value);
        //        }
        //        base.Child = value;
        //    }
        //}

        #endregion

        #region Public Methods

        public void Initialize(IControl element) {
            this.Child = element;
            if (Child != null) {
                Child.PointerWheelChanged += child_MouseWheel;
                Child.PointerPressed += child_PreviewMouseLeftButtonDown;
                Child.PointerReleased += child_MouseLeftButtonUp;
                Child.PointerMoved += child_MouseMove;
                //Child.PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            e.Handled = true;
            child_PreviewMouseLeftButtonDown(Child, e);
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            e.Handled = true;
            child_MouseLeftButtonUp(Child, e);
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            e.Handled = true;
            child_MouseMove(Child, e);
        }
        public void Reset() {
            MpPoint scale = new MpPoint(1, 1);
            MpPoint offset = new MpPoint();
            if(DesignerItem != null) {
                scale = new MpPoint(DesignerItem.ScaleX, DesignerItem.ScaleY);
                offset = new MpPoint(DesignerItem.TranslateOffsetX, DesignerItem.TranslateOffsetY);
            }

            if (Child != null) {
                // reset zoom
                var st = GetScaleTransform(Child);
                st.ScaleX = scale.X;
                st.ScaleY = scale.Y;

                // reset pan
                var tt = GetTranslateTransform(Child);
                tt.X = offset.X;
                tt.Y = offset.Y;
            }
        }

        public void Translate(double x, double y) {
            // NOTE to be used by DesignerItem Drop Behavior
            var tt = GetTranslateTransform(Child);
            tt.X = tt_origin.X - x;
            tt.Y = tt_origin.Y - y;
        }

        #endregion

        #region Protected Methods

        public override void Render(DrawingContext dc) {
            base.Render(dc);

            if (_renderTimer == null) {
                _renderTimer = new DispatcherTimer();
                _renderTimer.Interval = TimeSpan.FromMilliseconds(50);
                _renderTimer.Tick += (s, e) => InvalidateVisual();

                _renderTimer.Start();
            }
            if (ShowGrid) {
                DrawGrid(dc);
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
            base.OnPointerWheelChanged(e);

            child_MouseWheel(Child,e);
        }

        #endregion

        #region Private Methods

        #region Child Events

        private void child_MouseWheel(object sender, PointerWheelEventArgs e) {
            if (Child != null) {
                var st = GetScaleTransform(Child);
                var tt = GetTranslateTransform(Child);

                double zoom = e.Delta.Y > 0 ? .2 : -.2;
                if (!(e.Delta.Y > 0) && (st.ScaleX < MinScale || st.ScaleY < MinScale)) {
                    return;
                }
                MpPoint relative = e.GetPosition(Child).ToPortablePoint();
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * st.ScaleX + tt.X;
                absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;

                double zoomCorrected = zoom * st.ScaleX;
                st.ScaleX += zoomCorrected;
                st.ScaleY += zoomCorrected;


                st.ScaleX = Math.Max(MinScale, st.ScaleX);
                st.ScaleY = Math.Max(MinScale, st.ScaleY);

                if (DesignerItem != null) {
                    DesignerItem.ScaleX = st.ScaleX;
                    DesignerItem.ScaleY = st.ScaleY;
                    if (DataContext is MpActionCollectionViewModel acvm) {
                        acvm.HasModelChanged = true;
                    }
                }
            }
        }
        
        private void child_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e) {
            if (Child != null && !MpAvMoveBehavior.IsAnyMoving) {
                var tt = GetTranslateTransform(Child);
                mp_start = e.GetPosition(this).ToPortablePoint();
                tt_origin = new MpPoint(tt.X, tt.Y);
                MpPlatformWrapper.Services.Cursor.SetCursor(Child, MpCursorType.Hand);

                e.Pointer.Capture(this);
                if(e.Pointer.Captured != this) {
                    var capturer = e.Pointer.Captured;
                    Debugger.Break();
                } else {
                    IsTranslating = true;
                    e.Handled = true;
                }
            }
        }

        private void child_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e) {
            if (Child != null) {
                IsTranslating = false;
                e.Pointer.Capture(null);
                MpPlatformWrapper.Services.Cursor.UnsetCursor(DataContext);
                if(DataContext is MpActionCollectionViewModel acvm) {
                    acvm.HasModelChanged = true;
                }
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, PointerPressedEventArgs e) {
            this.Reset();
        }

        private void child_MouseMove(object sender, PointerEventArgs e) {
            if (Child != null) {
                if (IsTranslating) {
                    var tt = GetTranslateTransform(Child);
                    var v = mp_start - e.GetPosition(this).ToPortablePoint();


                    tt.X = tt_origin.X - v.X;
                    tt.Y = tt_origin.Y - v.Y;

                    if (DesignerItem != null) {
                        DesignerItem.TranslateOffsetX = tt.X;
                        DesignerItem.TranslateOffsetY = tt.Y;
                    }
                } 
            }
        }

        #endregion

        private TranslateTransform GetTranslateTransform(IControl element) {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(IControl element) {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }
        
        private void DrawGrid(DrawingContext dc) {
            MpPoint offset = new MpPoint();// (grid_offset.ToMpPoint() * -1).ToWpfPoint(); //new MpPoint(10, 10);

            var st = GetScaleTransform(Child);
            int HorizontalGridLineCount = (int)((Bounds.Width / GridLineSpacing) * (1/st.ScaleX));
            int VerticalGridLineCount = (int)((Bounds.Height / GridLineSpacing) * (1/st.ScaleY));

            double xStep = Bounds.Width / HorizontalGridLineCount;
            double yStep = Bounds.Height / VerticalGridLineCount;

            double curX = 0;
            double curY = 0;

            for (int x = 0; x < HorizontalGridLineCount; x++) {
                MpPoint p1 = new MpPoint(curX, 0);
                p1 = (MpPoint)(p1 - offset);
                MpPoint p2 = new MpPoint(curX, Bounds.Height);
                p2 = (MpPoint)(p2 - offset);

                bool isOrigin = x == (int)(HorizontalGridLineCount / 2);
                if (isOrigin) {
                    DrawLine(dc,new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    DrawLine(dc,new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curX += xStep;
            }

            for (int y = 0; y < VerticalGridLineCount; y++) {
                MpPoint p1 = new MpPoint(0, curY);
                p1 = (MpPoint)(p1 - offset);
                MpPoint p2 = new MpPoint(Bounds.Width, curY);
                p2 = (MpPoint)(p2 - offset);

                bool isOrigin = y == (int)(VerticalGridLineCount / 2);
                if (isOrigin) {
                    DrawLine(dc,new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    DrawLine(dc,new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curY += yStep;
            }
        }

        private void DrawLine(DrawingContext dc, Pen p, MpPoint p1, MpPoint p2) {
            var offset = new MpPoint(); //grid_offset;
            p1.X -= offset.X;
            p1.Y -= offset.Y;

            p2.X -= offset.X;
            p2.Y -= offset.Y;

            dc.DrawLine(p, p1.ToAvPoint(), p2.ToAvPoint());
        }

        #endregion

    }
}
