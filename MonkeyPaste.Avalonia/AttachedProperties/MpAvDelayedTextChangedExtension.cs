using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDelayedTextChangedExtension {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvDelayedTextChangedExtension() {
            //TextProperty.Changed.Subscribe(e => {
            //    ((MpAvDelayedTextChangedExtension)e.Sender).OnBindingValueChanged();
            //});
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            TextProperty.Changed.AddClassHandler<Control>((x, y) => HandleBoundTextChanged(x, y));
        }

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
        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is TextBox tb) {
                    tb.AttachedToVisualTree += Tb_AttachedToVisualTree;
                    tb.DetachedFromVisualTree += Tb_DetachedFromVisualTree;
                    if(tb.IsInitialized) {
                        Tb_AttachedToVisualTree(element, null);
                    }
                }
            } else {
                Tb_DetachedFromVisualTree(element, null);
            }
        }


        private static void Tb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if(sender is TextBox tb) {
                tb.GetObservable(TextBox.TextProperty).Subscribe(x => OnTextChanged(tb));
                //tb.KeyUp += (s,e) => OnTextChanged(tb);
            }
        }

        private static void Tb_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is TextBox tb) {
                tb.AttachedToVisualTree -= Tb_AttachedToVisualTree;
                tb.DetachedFromVisualTree -= Tb_DetachedFromVisualTree;
            }
        }
        #endregion

        #region Text AvaloniaProperty
        public static string GetText(AvaloniaObject obj) {
            return obj.GetValue(TextProperty);
        }

        public static void SetText(AvaloniaObject obj, string value) {
            obj.SetValue(TextProperty, value);
        }

        public static readonly AttachedProperty<string> TextProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "Text",
                string.Empty,
                false,
                BindingMode.TwoWay);

        static void HandleBoundTextChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            string newText = string.Empty;
            if (e.NewValue is string) {
                newText = e.NewValue as string;
            }
            if (element is TextBox tb) {
                tb.Text = newText;
                SetLastNotifiedText(tb, newText);
                SetLastNotifiedDt(tb, null);
            }
        }
        #endregion

        #region DelayMs AvaloniaProperty
        public static int GetDelayMs(AvaloniaObject obj) {
            return obj.GetValue(DelayMsProperty);
        }

        public static void SetDelayMs(AvaloniaObject obj, int value) {
            obj.SetValue(DelayMsProperty, value);
        }

        public static readonly AttachedProperty<int> DelayMsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "DelayMs",
                0);

        #endregion

        #region Private Properties

        #region LastNotifiedText AvaloniaProperty
        private static string GetLastNotifiedText(AvaloniaObject obj) {
            return obj.GetValue(LastNotifiedTextProperty);
        }

        private static void SetLastNotifiedText(AvaloniaObject obj, string value) {
            obj.SetValue(LastNotifiedTextProperty, value);
        }

        private static readonly AttachedProperty<string> LastNotifiedTextProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "LastNotifiedText",
                string.Empty,
                false,
                BindingMode.TwoWay);

        #endregion
        
        #region LastNotifiedDt AvaloniaProperty
        private static DateTime? GetLastNotifiedDt(AvaloniaObject obj) {
            return obj.GetValue(LastNotifiedDtProperty);
        }

        private static void SetLastNotifiedDt(AvaloniaObject obj, DateTime? value) {
            obj.SetValue(LastNotifiedDtProperty, value);
        }

        private static readonly AttachedProperty<DateTime?> LastNotifiedDtProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DateTime?>(
                "LastNotifiedDt",
                null,
                false,
                BindingMode.TwoWay);

        #endregion
        #endregion

        #region Private Methods

        private static void OnTextChanged(TextBox tb) {
            if(tb == null) {
                return;
            }
            int delayMs = GetDelayMs(tb);
            string lastNotifiedText = GetLastNotifiedText(tb);

            if (GetLastNotifiedDt(tb) == null) {
                MpConsole.WriteLine("Input recv'd delay started");
            }

            var this_input_change_dt = DateTime.Now;
            SetLastNotifiedDt(tb, this_input_change_dt);

            Dispatcher.UIThread.Post(async () => {
                while(true) {
                    var lndt = GetLastNotifiedDt(tb);
                    if(lndt == null) {
                        return;
                    }
                    if (lndt != this_input_change_dt) {
                        // new input was received, cancel ntf
                        MpConsole.WriteLine("Input recv'd update rejected");
                        return;
                    }
                    if (DateTime.Now - lndt > TimeSpan.FromMilliseconds(delayMs)) {
                        break;
                    }
                    await Task.Delay(10);
                }
                SetLastNotifiedDt(tb, null);
                if(tb.Text == lastNotifiedText) {
                    // ignore if text is the same
                    return;
                }
                MpConsole.WriteLine($"Input delay reached ntf change from '{lastNotifiedText}' to '{tb.Text}'");
                SetLastNotifiedText(tb, tb.Text);
                SetText(tb, tb.Text);
            });
        }


        #endregion
    }
}
