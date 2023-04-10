using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvProgressSpinner :
        Canvas, IStyleable, MpIOverrideRender {

        #region Statics

        static MpAvProgressSpinner() {
            PercentProperty.Changed.AddClassHandler<MpAvProgressSpinner>((x, y) => HandlePercentChanged(x));
        }
        private static void HandlePercentChanged(MpAvProgressSpinner ps) {
            ps.InvalidateVisual();
        }

        #endregion

        #region Interfaces

        #region IStyleable Implementation
        Type IStyleable.StyleKey => typeof(Canvas);
        #endregion

        #region MpIOverrideRender Implementation
        public bool IgnoreRender { get; set; }
        #endregion

        #endregion

        #region Properties

        #region Percent AvaloniaProperty
        private double _percent = 0;
        public double Percent {
            get => _percent;
            set => SetAndRaise(PercentProperty, ref _percent, value);
        }

        public static readonly DirectProperty<MpAvProgressSpinner, double> PercentProperty =
            AvaloniaProperty.RegisterDirect<MpAvProgressSpinner, double>
            (
                nameof(Percent),
                o => o.Percent,
                (o, v) => o.Percent = v,
                0
            );

        #endregion

        #region ArcWidth AvaloniaProperty
        private double _ArcWidth = 7;
        public double ArcWidth {
            get => _ArcWidth;
            set => SetAndRaise(ArcWidthProperty, ref _ArcWidth, value);
        }

        public static readonly DirectProperty<MpAvProgressSpinner, double> ArcWidthProperty =
            AvaloniaProperty.RegisterDirect<MpAvProgressSpinner, double>
            (
                nameof(ArcWidth),
                o => o.ArcWidth,
                (o, v) => o.ArcWidth = v,
                7
            );

        #endregion

        #region ZeroAngle AvaloniaProperty
        private double _ZeroAngle = 0;
        public double ZeroAngle {
            get => _ZeroAngle;
            set => SetAndRaise(ZeroAngleProperty, ref _ZeroAngle, value);
        }

        public static readonly DirectProperty<MpAvProgressSpinner, double> ZeroAngleProperty =
            AvaloniaProperty.RegisterDirect<MpAvProgressSpinner, double>
            (
                nameof(ZeroAngle),
                o => o.ZeroAngle,
                (o, v) => o.ZeroAngle = v,
                0
            );

        #endregion

        #region PercentBrush AvaloniaProperty

        private IBrush _PercentBrush = Brushes.Silver;
        public IBrush PercentBrush {
            get => _PercentBrush;
            set => SetAndRaise(PercentBrushProperty, ref _PercentBrush, value);
        }

        public static readonly DirectProperty<MpAvProgressSpinner, IBrush> PercentBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvProgressSpinner, IBrush>
            (
                nameof(PercentBrush),
                o => o.PercentBrush,
                (o, v) => o.PercentBrush = v,
                Brushes.Silver
            );

        #endregion

        #region RingBrush AvaloniaProperty

        private IBrush _RingBrush = Brushes.DimGray;
        public IBrush RingBrush {
            get => _RingBrush;
            set => SetAndRaise(RingBrushProperty, ref _RingBrush, value);
        }

        public static readonly DirectProperty<MpAvProgressSpinner, IBrush> RingBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvProgressSpinner, IBrush>
            (
                nameof(RingBrush),
                o => o.RingBrush,
                (o, v) => o.RingBrush = v,
                Brushes.DimGray
            );

        #endregion

        #endregion

        #region Constructors
        public MpAvProgressSpinner() : base() { }

        #endregion


        public override void Render(DrawingContext context) {
            if (!IsVisible) {
                return;
            }
            if (IgnoreRender) {
                return;
            }
            base.Render(context);
            double arc_width = ArcWidth;
            double w = Bounds.Width;
            double h = Bounds.Height;
            double d = Math.Min(w, h) - (arc_width * 2);
            double cx = w / 2;
            double cy = h / 2;
            double r = d / 2;

            context.DrawEllipse(
                brush: Brushes.Transparent,
                pen: new Pen(RingBrush, arc_width),
                center: new Point(cx, cy),
                radiusX: r,
                radiusY: r);

            double deg = Percent * 360;
            var c = new Point(cx, cy);

            double sa = -90 * (Math.PI / 180.0d);
            double ea = (deg - 90) * (Math.PI / 180.0d);
            bool isLarge = Percent >= 0.5;
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

            context.DrawGeometry(
                brush: Brushes.Transparent,
                pen: new Pen(PercentBrush, arc_width),
                pg);
        }

    }
}