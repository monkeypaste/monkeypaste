using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvReceivesEventsExtension {
        #region Private Variables
        private static List<Control> _receivers = new List<Control>();
        #endregion

        static MpAvReceivesEventsExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region SenderFilter AvaloniaProperty
        public static MpIEmbedHost GetSenderFilter(AvaloniaObject obj) {
            return obj.GetValue(SenderFilterProperty);
        }

        public static void SetSenderFilter(AvaloniaObject obj, MpIEmbedHost value) {
            obj.SetValue(SenderFilterProperty, value);
        }

        public static readonly AttachedProperty<MpIEmbedHost> SenderFilterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpIEmbedHost>(
                "SenderFilter",
                null,
                false);
        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    control.AttachedToLogicalTree += AttachedToLogicalHandler;
                    control.DetachedFromLogicalTree += DetachedFromLogicalHandler;
                    if (control.IsInitialized) {
                        AttachedToLogicalHandler(control, null);
                    }
                }
            } else {
                DetachedFromLogicalHandler(element, null);
            }


        }

        private static void AttachedToLogicalHandler(object s, LogicalTreeAttachmentEventArgs e) {
            if (s is Control control) {
                if (e == null) {
                    control.AttachedToLogicalTree += AttachedToLogicalHandler;
                }
                control.DetachedFromLogicalTree += DetachedFromLogicalHandler;
                if (!_receivers.Contains(control)) {
                    _receivers.Add(control);
                    MpConsole.WriteLine($"Control '{control}' ADDED to event receiver");
                }
            }
        }
        private static void DetachedFromLogicalHandler(object s, LogicalTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.AttachedToLogicalTree -= AttachedToLogicalHandler;
                control.DetachedFromLogicalTree -= DetachedFromLogicalHandler;
                if (_receivers.Contains(control)) {
                    _receivers.Remove(control);
                    MpConsole.WriteLine($"Control '{control}' REMOVED to event receiver");
                }
            }
        }


        #endregion

        #endregion

        #region Public Methods

        public static void SendEvent(MpIEmbedHost host, float x, float y, MpPointerEventType eventType) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => SendEvent(host, x, y, eventType));
                return;
            }
            Control rel_to = App.MainView as Control;

            var mv_origin = rel_to.PointToScreen(new Point());//.ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
            var unscaled_mp = new PixelPoint((int)x - mv_origin.X, (int)y - mv_origin.Y);

            var gmp = unscaled_mp.ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
            MpConsole.WriteLine($"Touch ({x},{y}) converted to mw Point ({gmp.X},{gmp.Y})");

            var pe = MpAvPointerInputHelpers.SimulatePointerEventArgs(
                eventType.ToRoutedEvent(),
                App.MainView as Control,
                gmp,
                MpKeyModifierFlags.None,
                false);

            //var hits =
            //    rel_to
            //    .GetSelfAndVisualDescendants()
            //    .Reverse()
            //    .OfType<Control>()
            //    .Where(x => x.RelativeBounds(rel_to).Contains(gmp));
            //hits.ForEach(x => MpConsole.WriteLine($"Sending touch event to: {x}"));
            //hits
            //    .OfType<Interactive>()
            //    .ForEach(x => x.Raisto scaeEvent(pe));

            var hits =
            _receivers
                .Where(x => (GetSenderFilter(x) == null || GetSenderFilter(x) == host) && x.RelativeBounds(rel_to).Contains(gmp));

            hits
                .ForEach(x => x.RaiseEvent(pe));
        }

        #endregion
    }
}
