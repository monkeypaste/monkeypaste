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

        public static readonly StyledProperty<double> PercentProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(
                nameof(Percent),
                0);

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
            double arc_width = 7;
            double w = Bounds.Width;
            double h = Bounds.Height;
            double d = Math.Min(w, h) - (arc_width * 2);
            double cx = w / 2;
            double cy = h / 2;

            double r = d / 2;

            context.DrawEllipse(
                brush: Brushes.Transparent,
                pen: new Pen(Brushes.Silver, arc_width),
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
                gc.ArcTo(p1, new Size(r, r), 0, !isLarge, SweepDirection.Clockwise);
                gc.EndFigure(false);
            }

            context.DrawGeometry(
                brush: Brushes.Transparent,
                pen: new Pen(Brushes.DimGray, arc_width),
                pg);
        }

    }
}