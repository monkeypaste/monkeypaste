using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpPopupRoot : WindowBase, IHostedVisualTreeRoot, IDisposable, IStyleHost, IPopupHost {
        /// <summary>
        /// Defines the <see cref="Transform"/> property.
        /// </summary>
        public static readonly StyledProperty<Transform?> TransformProperty =
            AvaloniaProperty.Register<MpPopupRoot, Transform?>(nameof(Transform));

        private PopupPositionerParameters _positionerParameters;

        /// <summary>
        /// Initializes static members of the <see cref="MpPopupRoot"/> class.
        /// </summary>
        static MpPopupRoot() {
            BackgroundProperty.OverrideDefaultValue(typeof(MpPopupRoot), Brushes.White);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpPopupRoot"/> class.
        /// </summary>
        public MpPopupRoot(TopLevel parent, IPopupImpl impl)
            : this(parent, impl, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpPopupRoot"/> class.
        /// </summary>
        /// <param name="parent">The popup parent.</param>
        /// <param name="impl">The popup implementation.</param>
        /// <param name="dependencyResolver">
        /// The dependency resolver to use. If null the default dependency resolver will be used.
        /// </param>
        public MpPopupRoot(TopLevel parent, IPopupImpl impl, IAvaloniaDependencyResolver? dependencyResolver)
            : base(impl, dependencyResolver) {
            ParentTopLevel = parent;
#if DEBUG
            this.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
#endif
        }

        /// <summary>
        /// Gets the platform-specific window implementation.
        /// </summary>
        public new IPopupImpl? PlatformImpl => (IPopupImpl?)base.PlatformImpl;

        /// <summary>
        /// Gets or sets a transform that will be applied to the popup.
        /// </summary>
        public Transform? Transform {
            get => GetValue(TransformProperty);
            set => SetValue(TransformProperty, value);
        }

        /// <summary>
        /// Gets the parent control in the event route.
        /// </summary>
        /// <remarks>
        /// Popup events are passed to their parent window. This facilitates this.
        /// </remarks>
        //internal override Interactive? InteractiveParent => (Interactive?)Parent;

        /// <summary>
        /// Gets the control that is hosting the popup root.
        /// </summary>
        Visual? IHostedVisualTreeRoot.Host {
            get {
                // If the parent is attached to a visual tree, then return that. However the parent
                // will possibly be a standalone Popup (i.e. a Popup not attached to a visual tree,
                // created by e.g. a ContextMenu): if this is the case, return the ParentTopLevel
                // if set. This helps to allow the focus manager to restore the focus to the outer
                // scope when the popup is closed.
                var parentVisual = Parent as Visual;
                if (parentVisual?.IsAttachedToVisualTree() == true)
                    return parentVisual;
                return ParentTopLevel ?? parentVisual;
            }
        }

        /// <summary>
        /// Gets the styling parent of the popup root.
        /// </summary>
        IStyleHost? IStyleHost.StylingParent => Parent;

        public TopLevel ParentTopLevel { get; }

        /// <inheritdoc/>
        public void Dispose() {
            PlatformImpl?.Dispose();
        }



        public void ConfigurePosition(Visual target, PlacementMode placement, Point offset,
            PopupAnchor anchor = PopupAnchor.None,
            PopupGravity gravity = PopupGravity.None,
            PopupPositionerConstraintAdjustment constraintAdjustment = PopupPositionerConstraintAdjustment.All,
            Rect? rect = null) {
            ConfigurePosition_override(ref _positionerParameters, ParentTopLevel, target,
                placement, offset, anchor, gravity, constraintAdjustment, rect, FlowDirection);
            //_positionerParameters.ConfigurePosition(ParentTopLevel, target,
            //    placement, offset, anchor, gravity, constraintAdjustment, rect, FlowDirection);

            if (_positionerParameters.Size != default)
                UpdatePosition();
        }

        public void SetChild(Control? control) => Content = control;

        Visual IPopupHost.HostedVisualTreeRoot => this;


        protected override sealed Size ArrangeSetBounds(Size size) {
            _positionerParameters.Size = size;
            UpdatePosition();
            return ClientSize;
        }
        #region Baddies

        protected override Size MeasureOverride(Size availableSize) {
            //var maxAutoSize = PlatformImpl?.MaxAutoSizeHint ?? Size.Infinity;
            var maxAutoSize = Size.Infinity;
            var constraint = availableSize;

            if (double.IsInfinity(constraint.Width)) {
                constraint = constraint.WithWidth(maxAutoSize.Width);
            }

            if (double.IsInfinity(constraint.Height)) {
                constraint = constraint.WithHeight(maxAutoSize.Height);
            }

            var measured = base.MeasureOverride(constraint);
            var width = measured.Width;
            var height = measured.Height;
            var widthCache = Width;
            var heightCache = Height;

            if (!double.IsNaN(widthCache)) {
                width = widthCache;
            }

            width = Math.Min(width, MaxWidth);
            width = Math.Max(width, MinWidth);

            if (!double.IsNaN(heightCache)) {
                height = heightCache;
            }

            height = Math.Min(height, MaxHeight);
            height = Math.Max(height, MinHeight);

            return new Size(width, height);
        }
        private void UpdatePosition() {

            //PlatformImpl?.PopupPositioner?.Update(_positionerParameters);
        }
        protected override AutomationPeer OnCreateAutomationPeer() {
            //return new PopupRootAutomationPeer(this);
            return null;
        }
        #endregion

        static void ConfigurePosition_override(ref PopupPositionerParameters positionerParameters,
            TopLevel topLevel,
            Visual target, PlacementMode placement, Point offset,
            PopupAnchor anchor, PopupGravity gravity,
            PopupPositionerConstraintAdjustment constraintAdjustment, Rect? rect,
            FlowDirection flowDirection) {
            positionerParameters.Offset = offset;
            positionerParameters.ConstraintAdjustment = constraintAdjustment;
            if (placement == PlacementMode.Pointer) {
                // We need a better way for tracking the last pointer position
                //var position = topLevel.PointToClient(topLevel.LastPointerPosition ?? default);
                PixelPoint lpp = MpAvShortcutCollectionViewModel.Instance.GlobalUnscaledMouseLocation;
                var position = topLevel.PointToClient(lpp);

                positionerParameters.AnchorRectangle = new Rect(position, new Size(1, 1));
                positionerParameters.Anchor = PopupAnchor.TopLeft;
                positionerParameters.Gravity = PopupGravity.BottomRight;
            } else {
                if (target == null)
                    throw new InvalidOperationException("Placement mode is not Pointer and PlacementTarget is null");
                var matrix = target.TransformToVisual(topLevel);
                if (matrix == null) {
                    if (target.GetVisualRoot() == null)
                        throw new InvalidOperationException("Target control is not attached to the visual tree");
                    throw new InvalidOperationException("Target control is not in the same tree as the popup parent");
                }

                var bounds = new Rect(default, target.Bounds.Size);
                var anchorRect = rect ?? bounds;
                positionerParameters.AnchorRectangle = anchorRect.Intersect(bounds).TransformToAABB(matrix.Value);

                var parameters = placement switch {
                    PlacementMode.Bottom => (PopupAnchor.Bottom, PopupGravity.Bottom),
                    PlacementMode.Right => (PopupAnchor.Right, PopupGravity.Right),
                    PlacementMode.Left => (PopupAnchor.Left, PopupGravity.Left),
                    PlacementMode.Top => (PopupAnchor.Top, PopupGravity.Top),
                    PlacementMode.Center => (PopupAnchor.None, PopupGravity.None),
                    PlacementMode.AnchorAndGravity => (anchor, gravity),
                    PlacementMode.TopEdgeAlignedRight => (PopupAnchor.TopRight, PopupGravity.TopLeft),
                    PlacementMode.TopEdgeAlignedLeft => (PopupAnchor.TopLeft, PopupGravity.TopRight),
                    PlacementMode.BottomEdgeAlignedLeft => (PopupAnchor.BottomLeft, PopupGravity.BottomRight),
                    PlacementMode.BottomEdgeAlignedRight => (PopupAnchor.BottomRight, PopupGravity.BottomLeft),
                    PlacementMode.LeftEdgeAlignedTop => (PopupAnchor.TopLeft, PopupGravity.BottomLeft),
                    PlacementMode.LeftEdgeAlignedBottom => (PopupAnchor.BottomLeft, PopupGravity.TopLeft),
                    PlacementMode.RightEdgeAlignedTop => (PopupAnchor.TopRight, PopupGravity.BottomRight),
                    PlacementMode.RightEdgeAlignedBottom => (PopupAnchor.BottomRight, PopupGravity.TopRight),
                    _ => throw new ArgumentOutOfRangeException(nameof(placement), placement,
                        "Invalid value for Popup.PlacementMode")
                };
                positionerParameters.Anchor = parameters.Item1;
                positionerParameters.Gravity = parameters.Item2;
            }

            // Invert coordinate system if FlowDirection is RTL
            if (flowDirection == FlowDirection.RightToLeft) {
                if ((positionerParameters.Anchor & PopupAnchor.Right) == PopupAnchor.Right) {
                    positionerParameters.Anchor ^= PopupAnchor.Right;
                    positionerParameters.Anchor |= PopupAnchor.Left;
                } else if ((positionerParameters.Anchor & PopupAnchor.Left) == PopupAnchor.Left) {
                    positionerParameters.Anchor ^= PopupAnchor.Left;
                    positionerParameters.Anchor |= PopupAnchor.Right;
                }

                if ((positionerParameters.Gravity & PopupGravity.Right) == PopupGravity.Right) {
                    positionerParameters.Gravity ^= PopupGravity.Right;
                    positionerParameters.Gravity |= PopupGravity.Left;
                } else if ((positionerParameters.Gravity & PopupGravity.Left) == PopupGravity.Left) {
                    positionerParameters.Gravity ^= PopupGravity.Left;
                    positionerParameters.Gravity |= PopupGravity.Right;
                }
            }
        }
    }
}
