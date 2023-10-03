using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContrastEffectiveBackgroundExtension {

        static MpAvContrastEffectiveBackgroundExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties


        #region ContrastedPropertyNameCsv AvaloniaProperty
        // NOTE properties are separated by pipe | not comma

        public static string GetContrastedPropertyNameCsv(AvaloniaObject obj) {
            return obj.GetValue(ContrastedPropertyNameCsvProperty);
        }

        public static void SetContrastedPropertyNameCsv(AvaloniaObject obj, string value) {
            obj.SetValue(ContrastedPropertyNameCsvProperty, value);
        }

        public static readonly AttachedProperty<string> ContrastedPropertyNameCsvProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "ContrastedPropertyNameCsv",
                TemplatedControl.ForegroundProperty.Name);
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
            if (e.NewValue is bool isEnabledVal &&
                isEnabledVal) {
                element.Loaded += AttachedControl_Loaded;
                if (element.IsLoaded) {
                    AttachedControl_Loaded(element, null);
                }
            } else {
                AttachedControl_Unloaded(element, null);
            }


        }

        private static void AttachedControl_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            c.Unloaded += AttachedControl_Unloaded;

            EstablishEffectBackgroundAsync(c).FireAndForgetSafeAsync();
        }

        private static void AttachedControl_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control c) {
                return;
            }

            c.Loaded -= AttachedControl_Loaded;
            c.Unloaded -= AttachedControl_Unloaded;
        }


        #endregion

        private static async Task EstablishEffectBackgroundAsync(Control attached_control) {
            // loop until control unloaded or self or ancestor has visible bg
            // then subscribe to changes on that controls bg
            while (true) {
                if (attached_control == null ||
                    !attached_control.IsLoaded) {
                    return;
                }

                IBrush eff_bg = attached_control.GetEffectiveBackground(out Control eff_bg_c);
                if (eff_bg != null) {
                    UpdateContrastProperties(attached_control, eff_bg);

                    eff_bg_c
                        .GetObservable(TemplatedControl.BackgroundProperty)
                        .Subscribe(value => OnEffectiveBgChanged(attached_control, value));
                    return;
                }
                await Task.Delay(100);
            }
        }

        private static void OnEffectiveBgChanged(Control attached_control, IBrush eff_bg) {
            if (eff_bg == null ||
                eff_bg.Opacity == 0) {
                // when effective bg control unloads or has no color reestablish
                EstablishEffectBackgroundAsync(attached_control).FireAndForgetSafeAsync();
                return;
            }
            UpdateContrastProperties(attached_control, eff_bg);
        }

        private static void UpdateContrastProperties(Control attached_control, IBrush eff_bg) {
            if (attached_control == null ||
                !attached_control.IsLoaded ||
                eff_bg == null ||
                GetContrastedPropertyNameCsv(attached_control) is not string propsCsv ||
                propsCsv.SplitNoEmpty("|") is not string[] props ||
                MpAvBrushToContrastBrushConverter.Instance.Convert(eff_bg, null, null, null)
                    is not IBrush contrast_brush) {
                return;
            }
            if (attached_control is TemplatedControl tc) {
                tc.SetValue(TemplatedControl.ForegroundProperty, contrast_brush.AdjustOpacity(1));
            }
            //foreach (string prop in props) {
            //    attached_control.SetPropertyValue(prop, contrast_brush);
            //}
        }
        #endregion
    }
}
