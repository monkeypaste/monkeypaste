using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvBorderBase : Decorator {

        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<MpAvBorderBase, IBrush?>(nameof(Background));

        /// <summary>
        /// Defines the <see cref="BorderBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> BorderBrushProperty =
            AvaloniaProperty.Register<MpAvBorderBase, IBrush?>(nameof(BorderBrush));

        /// <summary>
        /// Defines the <see cref="BorderThickness"/> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> BorderThicknessProperty =
            AvaloniaProperty.Register<MpAvBorderBase, Thickness>(nameof(BorderThickness));

        /// <summary>
        /// Defines the <see cref="CornerRadius"/> property.
        /// </summary>
        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
            AvaloniaProperty.Register<MpAvBorderBase, CornerRadius>(nameof(CornerRadius));

        /// <summary>
        /// Defines the <see cref="BoxShadow"/> property.
        /// </summary>
        public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
            AvaloniaProperty.Register<MpAvBorderBase, BoxShadows>(nameof(BoxShadow));

        /// <summary>
        /// Defines the <see cref="BorderDashOffset"/> property.
        /// </summary>
        public static readonly StyledProperty<double> BorderDashOffsetProperty =
            AvaloniaProperty.Register<MpAvBorderBase, double>(nameof(BorderDashOffset));

        /// <summary>
        /// Defines the <see cref="BorderDashArray"/> property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<double>?> BorderDashArrayProperty =
            AvaloniaProperty.Register<MpAvBorderBase, AvaloniaList<double>?>(nameof(BorderDashArray));

        /// <summary>
        /// Defines the <see cref="BorderLineCap"/> property.
        /// </summary>
        public static readonly StyledProperty<PenLineCap> BorderLineCapProperty =
            AvaloniaProperty.Register<MpAvBorderBase, PenLineCap>(nameof(BorderLineCap), PenLineCap.Flat);

        /// <summary>
        /// Defines the <see cref="BorderLineJoin"/> property.
        /// </summary>
        public static readonly StyledProperty<PenLineJoin> BorderLineJoinProperty =
            AvaloniaProperty.Register<MpAvBorderBase, PenLineJoin>(nameof(BorderLineJoin), PenLineJoin.Miter);

        private Thickness? _layoutThickness;
        private double _scale;
        //private CompositionBorderVisual? _borderVisual;

        /// <summary>
        /// Initializes static members of the <see cref="MpAvBorderBase"/> class.
        /// </summary>
        static MpAvBorderBase() {
            AffectsRender<MpAvBorderBase>(
                BackgroundProperty,
                BorderBrushProperty,
                BorderThicknessProperty,
                CornerRadiusProperty,
                BorderDashArrayProperty,
                BorderLineCapProperty,
                BorderLineJoinProperty,
                BorderDashOffsetProperty,
                BoxShadowProperty);
            AffectsMeasure<MpAvBorderBase>(BorderThicknessProperty);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            switch (change.Property.Name) {
                case nameof(UseLayoutRounding):
                case nameof(BorderThickness):
                    _layoutThickness = null;
                    break;
                    //case nameof(CornerRadius):
                    //    if (_borderVisual != null)
                    //        _borderVisual.CornerRadius = CornerRadius;
                    //    break;
            }
        }

        /// <summary>
        /// Gets or sets a brush with which to paint the background.
        /// </summary>
        public IBrush? Background {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets a brush with which to paint the border.
        /// </summary>
        public IBrush? BorderBrush {
            get { return GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="double"/> values that indicate the pattern of dashes and gaps that is used to outline shapes.
        /// </summary>
        public AvaloniaList<double>? BorderDashArray {
            get { return GetValue(BorderDashArrayProperty); }
            set { SetValue(BorderDashArrayProperty, value); }
        }

        /// <summary>
        /// Gets or sets the thickness of the border.
        /// </summary>
        public Thickness BorderThickness {
            get { return GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that specifies the distance within the dash pattern where a dash begins.
        /// </summary>
        public double BorderDashOffset {
            get { return GetValue(BorderDashOffsetProperty); }
            set { SetValue(BorderDashOffsetProperty, value); }
        }

        /// <summary>
        /// Gets or sets a <see cref="PenLineCap"/> enumeration value that describes the shape at the ends of a line.
        /// </summary>
        public PenLineCap BorderLineCap {
            get { return GetValue(BorderLineCapProperty); }
            set { SetValue(BorderLineCapProperty, value); }
        }

        /// <summary>
        /// Gets or sets a <see cref="PenLineJoin"/> enumeration value that specifies the type of join that is used at the vertices of a Shape.
        /// </summary>
        public PenLineJoin BorderLineJoin {
            get { return GetValue(BorderLineJoinProperty); }
            set { SetValue(BorderLineJoinProperty, value); }
        }

        /// <summary>
        /// Gets or sets the radius of the border rounded corners.
        /// </summary>
        public CornerRadius CornerRadius {
            get { return GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Gets or sets the box shadow effect parameters
        /// </summary>
        public BoxShadows BoxShadow {
            get => GetValue(BoxShadowProperty);
            set => SetValue(BoxShadowProperty, value);
        }

        private Thickness LayoutThickness {
            get {
                VerifyScale();

                if (_layoutThickness == null) {
                    var borderThickness = BorderThickness;

                    if (UseLayoutRounding)
                        borderThickness = LayoutHelper.RoundLayoutThickness(borderThickness, _scale, _scale);

                    _layoutThickness = borderThickness;
                }

                return _layoutThickness.Value;
            }
        }

        private void VerifyScale() {
            var currentScale = LayoutHelper.GetLayoutScale(this);
            if (MathUtilities.AreClose(currentScale, _scale))
                return;

            _scale = currentScale;
            _layoutThickness = null;
        }

        /// <summary>
        /// Measures the control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size of the control.</returns>
        protected override Size MeasureOverride(Size availableSize) {
            return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
        }

        /// <summary>
        /// Arranges the control's child.
        /// </summary>
        /// <param name="finalSize">The size allocated to the control.</param>
        /// <returns>The space taken.</returns>
        protected override Size ArrangeOverride(Size finalSize) {
            return LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
        }

        //private protected override CompositionDrawListVisual CreateCompositionVisual(Compositor compositor) {
        //    return _borderVisual = new CompositionBorderVisual(compositor, this) {
        //        CornerRadius = CornerRadius
        //    };
        //}

        public CornerRadius ClipToBoundsRadius => CornerRadius;
    }
}
