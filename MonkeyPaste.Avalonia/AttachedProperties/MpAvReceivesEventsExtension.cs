using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
            Dispatcher.UIThread.Post(() => {
                var gmp = new PixelPoint((int)x, (int)y).ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
                //var gmp = this.TranslatePoint(mp.ToAvPoint(), App.MainView as Control).Value.ToPortablePoint();

                var pe = MpAvPointerInputHelpers.SimulatePointerEventArgs(
                    eventType.ToRoutedEvent(),
                    App.MainView as Control,
                    gmp,
                    MpKeyModifierFlags.None,
                    false);

                _receivers
                .Where(x => GetSenderFilter(x) == null || GetSenderFilter(x) == host)
                .ForEach(x => x.RaiseEvent(pe));
            });
        }

        #endregion
    }
}
