using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpToolTipService {
        public static MpToolTipService Instance { get; } = new MpToolTipService();

        private DispatcherTimer? _timer;

        private MpToolTipService() { }

        /// <summary>
        /// called when the <see cref="MpToolTip.TipProperty"/> property changes on a control.
        /// </summary>
        /// <param name="e">The event args.</param>
        internal void TipChanged(AvaloniaPropertyChangedEventArgs e) {
            var control = (Control)e.Sender;

            if (e.OldValue != null) {
                control.PointerEntered -= ControlPointerEntered;
                control.PointerExited -= ControlPointerExited;
                control.RemoveHandler(InputElement.PointerPressedEvent, ControlPointerPressed);
            }

            if (e.NewValue != null) {
                control.PointerEntered += ControlPointerEntered;
                control.PointerExited += ControlPointerExited;
                control.AddHandler(InputElement.PointerPressedEvent, ControlPointerPressed,
                    RoutingStrategies.Bubble | RoutingStrategies.Tunnel | RoutingStrategies.Direct, true);
            }

            if (MpToolTip.GetIsOpen(control) && e.NewValue != e.OldValue && !(e.NewValue is MpToolTip)) {
                if (e.NewValue is null) {
                    Close(control);
                } else {
                    if (control.GetValue(MpToolTip.ToolTipProperty) is { } tip) {
                        tip.Content = e.NewValue;
                    }
                }
            }
        }

        internal void TipOpenChanged(AvaloniaPropertyChangedEventArgs e) {
            var control = (Control)e.Sender;

            if (e.OldValue is false && e.NewValue is true) {
                control.DetachedFromVisualTree += ControlDetaching;
                control.EffectiveViewportChanged += ControlEffectiveViewportChanged;
            } else if (e.OldValue is true && e.NewValue is false) {
                control.DetachedFromVisualTree -= ControlDetaching;
                control.EffectiveViewportChanged -= ControlEffectiveViewportChanged;
            }
        }

        private void ControlDetaching(object? sender, VisualTreeAttachmentEventArgs e) {
            var control = (Control)sender!;
            control.DetachedFromVisualTree -= ControlDetaching;
            control.EffectiveViewportChanged -= ControlEffectiveViewportChanged;
            Close(control);
        }

        /// <summary>
        /// Called when the pointer enters a control with an attached tooltip.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ControlPointerEntered(object? sender, PointerEventArgs e) {
            StopTimer();

            var control = (Control)sender!;
            var showDelay = MpToolTip.GetShowDelay(control);
            if (showDelay == 0) {
                Open(control);
            } else {
                StartShowTimer(showDelay, control);
            }
        }

        /// <summary>
        /// Called when the pointer leaves a control with an attached tooltip.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ControlPointerExited(object? sender, PointerEventArgs e) {
            var control = (Control)sender!;

            // If the control is showing a tooltip and the pointer is over the tooltip, don't close it.
            if (control.GetValue(MpToolTip.ToolTipProperty) is { } tooltip && tooltip.IsPointerOver)
                return;

            Close(control);
        }

        private void ControlPointerPressed(object? sender, PointerPressedEventArgs e) {
            StopTimer();
            (sender as AvaloniaObject)?.ClearValue(MpToolTip.IsOpenProperty);
        }

        private void ControlEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e) {
            var control = (Control)sender!;
            var toolTip = control.GetValue(MpToolTip.ToolTipProperty);
            toolTip?.RecalculatePosition(control);
        }

        private void ToolTipClosed(object? sender, EventArgs e) {
            if (sender is MpToolTip toolTip) {
                toolTip.Closed -= ToolTipClosed;
                toolTip.PointerExited -= ToolTipPointerExited;
            }
        }

        private void ToolTipPointerExited(object? sender, PointerEventArgs e) {
            // The pointer has exited the tooltip. Close the tooltip unless the pointer is over the
            // adorned control.
            if (sender is MpToolTip toolTip &&
                toolTip.AdornedControl is { } control &&
                !control.IsPointerOver) {
                Close(control);
            }
        }

        private void StartShowTimer(int showDelay, Control control) {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(showDelay) };
            _timer.Tick += (o, e) => Open(control);
            _timer.Start();
        }

        private void Open(Control control) {
            StopTimer();

            if (control.IsAttachedToVisualTree()) {
                MpToolTip.SetIsOpen(control, true);

                if (control.GetValue(MpToolTip.ToolTipProperty) is { } tooltip) {
                    tooltip.Closed += ToolTipClosed;
                    tooltip.PointerExited += ToolTipPointerExited;
                }
            }
        }

        private void Close(Control control) {
            StopTimer();

            MpToolTip.SetIsOpen(control, false);
        }

        private void StopTimer() {
            _timer?.Stop();
            _timer = null;
        }
    }
}
