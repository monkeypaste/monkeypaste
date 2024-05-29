using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvProgressSpinner :
        UserControl, MpIOverrideRender {

        #region Statics

        static MpAvProgressSpinner() {
            PercentProperty.Changed.AddClassHandler<MpAvProgressSpinner>((x, y) => HandlePercentChanged(x));
        }
        private static void HandlePercentChanged(MpAvProgressSpinner ps) {
            ps.Redraw();
        }

        #endregion

        #region Interfaces

        #region MpIOverrideRender Implementation
        public bool IgnoreRender { get; set; }
        #endregion

        #endregion

        #region Properties

        #region Overrides
        //protected override Type StyleKeyOverride => typeof(UserControl);
        #endregion

        #region Style Properties
        #region Percent AvaloniaProperty
        public double Percent {
            get { return GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }
        public static readonly StyledProperty<double> PercentProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, double>(nameof(Percent), 0);
        #endregion

        #region ArcWidth AvaloniaProperty

        public double ArcWidth {
            get { return GetValue(ArcWidthProperty); }
            set { SetValue(ArcWidthProperty, value); }
        }
        public static readonly StyledProperty<double> ArcWidthProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, double>(nameof(ArcWidth), 7);

        #endregion


        #region ZeroAngle AvaloniaProperty
        public double ZeroAngle {
            get { return GetValue(ZeroAngleProperty); }
            set { SetValue(ZeroAngleProperty, value); }
        }
        public static readonly StyledProperty<double> ZeroAngleProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, double>(nameof(ZeroAngle), 0);
        #endregion

        #region PercentBrush AvaloniaProperty
        public IBrush PercentBrush {
            get { return GetValue(PercentBrushProperty); }
            set { SetValue(PercentBrushProperty, value); }
        }
        public static readonly StyledProperty<IBrush> PercentBrushProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, IBrush>(nameof(PercentBrush), Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeAccent3Color.ToString()));
        #endregion

        #region RingBrush AvaloniaProperty

        public IBrush RingBrush {
            get { return GetValue(RingBrushProperty); }
            set { SetValue(RingBrushProperty, value); }
        }
        public static readonly StyledProperty<IBrush> RingBrushProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, IBrush>(nameof(RingBrush), Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeGrayAccent1Color.ToString()));
        #endregion

        #region LabelBrush AvaloniaProperty

        public IBrush LabelBrush {
            get { return GetValue(LabelBrushProperty); }
            set { SetValue(LabelBrushProperty, value); }
        }
        public static readonly StyledProperty<IBrush> LabelBrushProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, IBrush>(nameof(LabelBrush), Brushes.Black);


        #endregion

        #region ShowBusyWhenDone AvaloniaProperty

        public bool ShowBusyWhenDone {
            get { return GetValue(ShowBusyWhenDoneProperty); }
            set { SetValue(ShowBusyWhenDoneProperty, value); }
        }
        public static readonly StyledProperty<bool> ShowBusyWhenDoneProperty =
            AvaloniaProperty.Register<MpAvProgressSpinner, bool>(nameof(ShowBusyWhenDone), false);
        #endregion
        #endregion

        bool IsDone =>
            Percent >= 1.0d;

        #endregion

        #region Constructors
        public MpAvProgressSpinner() : base() {
            Content = new MpAvBusySpinnerView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsVisible = false
            };
            this.GetObservable(PercentProperty).Subscribe(value => OnPercentChanged()).AddDisposable(this);
        }

        #endregion

        protected override void OnUnloaded(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnUnloaded(e);
            this.ClearDisposables();
        }


        public override void Render(DrawingContext context) {
            if (!IsVisible) {
                return;
            }
            if (IgnoreRender) {
                return;
            }
            base.Render(context);
            if (IsDone && ShowBusyWhenDone) {
                return;
            }
            double arc_width = ArcWidth;
            double w = Bounds.Width;
            double h = Bounds.Height;
            double d = Math.Min(w, h) - (arc_width * 2);
            double cx = w / 2;
            double cy = h / 2;
            double r = d / 2;

            // draw full circle
            context.DrawEllipse(
                brush: Brushes.Transparent,
                pen: new Pen(RingBrush, arc_width),
                center: new Point(cx, cy),
                radiusX: r,
                radiusY: r);

            double percent = Math.Min(0.9999, Percent);

            double deg = percent * 360;
            var c = new Point(cx, cy);

            double sa = -90 * (Math.PI / 180.0d);
            double ea = (deg - 90) * (Math.PI / 180.0d);
            bool isLarge = percent >= 0.5;
            Point p0 = c + new Vector(Math.Cos(sa), Math.Sin(sa)) * r;
            Point p1 = c + new Vector(Math.Cos(ea), Math.Sin(ea)) * r;

            var pg = new PathGeometry() {
                FillRule = FillRule.NonZero
            };

            using (var gc = pg.Open()) {
                gc.BeginFigure(p0, true);
                gc.ArcTo(p1, new Size(r, r), 0, isLarge, SweepDirection.Clockwise);
                gc.EndFigure(false);
            }

            // draw progress arc
            context.DrawGeometry(
                brush: Brushes.Transparent,
                pen: new Pen(PercentBrush, arc_width),
                pg);

            string percent_label = ((int)Math.Clamp(percent * 100, 0, 100)).ToString();
            if (percent_label.Length == 1) {
                // NOTE padding single digits so font size is consistent
                percent_label = "0" + percent_label;
            }
            double fs = Math.Max(7, d / (double)percent_label.Length);
            var ft = percent_label.ToFormattedText(
                    fontSize: fs,
                    foreground: LabelBrush,
                    textAlignment: TextAlignment.Center);
            var tl = c.ToPortablePoint() - (new MpPoint(ft.Width, ft.Height) * 0.5);

            // draw progress text
            context.DrawText(
                ft,
                tl.ToAvPoint());
        }

        void OnPercentChanged() {
            if (Content is not MpAvBusySpinnerView bspv) {
                return;
            }
            bspv.IsVisible = IsDone && ShowBusyWhenDone;

        }
    }
}