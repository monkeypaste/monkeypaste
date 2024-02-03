using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using PropertyChanged;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// A control which pops up a hint when a control is hovered.
    /// </summary>
    /// <remarks>
    /// You will probably not want to create a <see cref="MpToolTip"/> control directly: if added to
    /// the tree it will act as a simple <see cref="ContentControl"/> styled to look like a tooltip.
    /// To add a tooltip to a control, use the <see cref="TipProperty"/> attached property,
    /// assigning the content that you want displayed.
    /// </remarks>
    [DoNotNotify]
    [PseudoClasses(":open")]
    public class MpToolTip : ToolTip, IPopupHostProvider {
        /// <summary>
        /// Defines the MpToolTip.Tip attached property.
        /// </summary>
        public static new readonly AttachedProperty<object?> TipProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, object?>("Tip");

        /// <summary>
        /// Defines the MpToolTip.IsOpen attached property.
        /// </summary>
        public static new readonly AttachedProperty<bool> IsOpenProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, bool>("IsOpen");

        /// <summary>
        /// Defines the MpToolTip.Placement property.
        /// </summary>
        public static new readonly AttachedProperty<PlacementMode> PlacementProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, PlacementMode>("Placement", defaultValue: PlacementMode.Pointer);

        /// <summary>
        /// Defines the MpToolTip.HorizontalOffset property.
        /// </summary>
        public static new readonly AttachedProperty<double> HorizontalOffsetProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, double>("HorizontalOffset");

        /// <summary>
        /// Defines the MpToolTip.VerticalOffset property.
        /// </summary>
        public static new readonly AttachedProperty<double> VerticalOffsetProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, double>("VerticalOffset", 20);

        /// <summary>
        /// Defines the MpToolTip.ShowDelay property.
        /// </summary>
        public static new readonly AttachedProperty<int> ShowDelayProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, int>("ShowDelay", 400);

        /// <summary>
        /// Stores the current <see cref="MpToolTip"/> instance in the control.
        /// </summary>
        internal static readonly AttachedProperty<MpToolTip?> ToolTipProperty =
            AvaloniaProperty.RegisterAttached<MpToolTip, Control, MpToolTip?>("MpToolTip");

        private IPopupHost? _popupHost;
        private Action<IPopupHost?>? _popupHostChangedHandler;

        /// <summary>
        /// Initializes static members of the <see cref="MpToolTip"/> class.
        /// </summary>
        static MpToolTip() {
            TipProperty.Changed.Subscribe(MpToolTipService.Instance.TipChanged);
            IsOpenProperty.Changed.Subscribe(MpToolTipService.Instance.TipOpenChanged);
            IsOpenProperty.Changed.Subscribe(IsOpenChanged);

            HorizontalOffsetProperty.Changed.Subscribe(RecalculatePositionOnPropertyChanged);
            VerticalOffsetProperty.Changed.Subscribe(RecalculatePositionOnPropertyChanged);
            PlacementProperty.Changed.Subscribe(RecalculatePositionOnPropertyChanged);
        }

        internal Control? AdornedControl { get; private set; }
        internal event EventHandler? Closed;

        /// <summary>
        /// Gets the value of the MpToolTip.Tip attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// The content to be displayed in the control's tooltip.
        /// </returns>
        public static new object? GetTip(Control element) {
            return element.GetValue(TipProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.Tip attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">The content to be displayed in the control's tooltip.</param>
        public static new void SetTip(Control element, object? value) {
            element.SetValue(TipProperty, value);
        }

        /// <summary>
        /// Gets the value of the MpToolTip.IsOpen attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// A value indicating whether the tool tip is visible.
        /// </returns>
        public static new bool GetIsOpen(Control element) {
            return element.GetValue(IsOpenProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.IsOpen attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">A value indicating whether the tool tip is visible.</param>
        public static new void SetIsOpen(Control element, bool value) {
            element.SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Gets the value of the MpToolTip.Placement attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// A value indicating how the tool tip is positioned.
        /// </returns>
        public static new PlacementMode GetPlacement(Control element) {
            return element.GetValue(PlacementProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.Placement attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">A value indicating how the tool tip is positioned.</param>
        public static new void SetPlacement(Control element, PlacementMode value) {
            element.SetValue(PlacementProperty, value);
        }

        /// <summary>
        /// Gets the value of the MpToolTip.HorizontalOffset attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// A value indicating how the tool tip is positioned.
        /// </returns>
        public static new double GetHorizontalOffset(Control element) {
            return element.GetValue(HorizontalOffsetProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.HorizontalOffset attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">A value indicating how the tool tip is positioned.</param>
        public static new void SetHorizontalOffset(Control element, double value) {
            element.SetValue(HorizontalOffsetProperty, value);
        }

        /// <summary>
        /// Gets the value of the MpToolTip.VerticalOffset attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// A value indicating how the tool tip is positioned.
        /// </returns>
        public static new double GetVerticalOffset(Control element) {
            return element.GetValue(VerticalOffsetProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.VerticalOffset attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">A value indicating how the tool tip is positioned.</param>
        public static new void SetVerticalOffset(Control element, double value) {
            element.SetValue(VerticalOffsetProperty, value);
        }

        /// <summary>
        /// Gets the value of the MpToolTip.ShowDelay attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <returns>
        /// A value indicating the time, in milliseconds, before a tool tip opens.
        /// </returns>
        public static new int GetShowDelay(Control element) {
            return element.GetValue(ShowDelayProperty);
        }

        /// <summary>
        /// Sets the value of the MpToolTip.ShowDelay attached property.
        /// </summary>
        /// <param name="element">The control to get the property from.</param>
        /// <param name="value">A value indicating the time, in milliseconds, before a tool tip opens.</param>
        public static new void SetShowDelay(Control element, int value) {
            element.SetValue(ShowDelayProperty, value);
        }
        private static void RecalculatePositionOnPropertyChanged(AvaloniaPropertyChangedEventArgs args) {
            var control = (Control)args.Sender;
            var tooltip = control.GetValue(ToolTipProperty);
            if (tooltip == null) {
                return;
            }

            tooltip.RecalculatePosition(control);
        }

        IPopupHost? IPopupHostProvider.PopupHost => _popupHost;

        event Action<IPopupHost?>? IPopupHostProvider.PopupHostChanged {
            add => _popupHostChangedHandler += value;
            remove => _popupHostChangedHandler -= value;
        }

        internal void RecalculatePosition(Control control) {
            _popupHost?.ConfigurePosition(control, GetPlacement(control), new Point(GetHorizontalOffset(control), GetVerticalOffset(control)));
        }

        private void Open(Control control) {
            Close();

            _popupHost = OverlayPopupHost.CreatePopupHost(control, null);
            _popupHost.SetChild(this);
            ((ISetLogicalParent)_popupHost).SetParent(control);
            ApplyTemplatedParent_override(this, control.TemplatedParent);

            _popupHost.ConfigurePosition(control, GetPlacement(control),
                new Point(GetHorizontalOffset(control), GetVerticalOffset(control)));

            WindowManagerAddShadowHintChanged(_popupHost, false);

            _popupHost.Show();
            _popupHostChangedHandler?.Invoke(_popupHost);
        }

        private void Close() {
            if (_popupHost != null) {
                _popupHost.SetChild(null);
                _popupHost.Dispose();
                _popupHost = null;
                _popupHostChangedHandler?.Invoke(null);
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }


        private void UpdatePseudoClasses(bool newValue) {
            PseudoClasses.Set(":open", newValue);
        }

        // overrides
        static void ApplyTemplatedParent_override(StyledElement control, AvaloniaObject? templatedParent) {
            //control.TemplatedParent = templatedParent;
            control.SetValue(Control.TemplatedParentProperty, templatedParent);

            //var children = control.LogicalChildren;
            var children = control.GetLogicalChildren().ToList();
            var count = children.Count;

            for (var i = 0; i < count; i++) {
                if (children[i] is StyledElement child && child.TemplatedParent is null) {
                    ApplyTemplatedParent_override(child, templatedParent);
                }
            }
        }


        #region Baddies
        private void WindowManagerAddShadowHintChanged(IPopupHost host, bool hint) {
            if (host is PopupRoot pr) {
                //pr.PlatformImpl?.SetWindowManagerAddShadowHint(hint);
            }
        }
        private static void IsOpenChanged(AvaloniaPropertyChangedEventArgs e) {
            var control = (Control)e.Sender;
            var newValue = (bool)e.NewValue!;

            if (newValue) {
                var tip = GetTip(control);
                if (tip == null) return;

                var toolTip = control.GetValue(ToolTipProperty);
                if (toolTip == null || (tip != toolTip && tip != toolTip.Content)) {
                    toolTip?.Close();

                    toolTip = tip as MpToolTip ?? new MpToolTip { Content = tip };
                    control.SetValue(ToolTipProperty, toolTip);
                    //toolTip.SetValue(ThemeVariant.RequestedThemeVariantProperty, control.ActualThemeVariant);
                }

                toolTip.AdornedControl = control;
                toolTip.Open(control);
                toolTip?.UpdatePseudoClasses(newValue);
            } else if (control.GetValue(ToolTipProperty) is { } toolTip) {
                toolTip.AdornedControl = null;
                toolTip.Close();
                toolTip?.UpdatePseudoClasses(newValue);
            }
        }
        #endregion
    }
}
