using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvListBoxItemBoundsObserverExtension {

        #region Constructors
        static MpAvListBoxItemBoundsObserverExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region ObservedBounds AvaloniaProperty
        public static MpRect GetObservedBounds(AvaloniaObject obj) {
            return obj.GetValue(ObservedBoundsProperty);
        }

        public static void SetObservedBounds(AvaloniaObject obj, MpRect value) {
            obj.SetValue(ObservedBoundsProperty, value);
        }

        public static readonly AttachedProperty<MpRect> ObservedBoundsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpRect>(
                "ObservedBounds",
               null,
                false,
                BindingMode.TwoWay);

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
                if (element is Control control) {
                    control.DetachedFromVisualTree += DetachedFromVisualHandler;
                    control.AttachedToVisualTree += Control_AttachedToVisualTree;
                    if(control.IsInitialized) {
                        Control_AttachedToVisualTree(control, null);
                    }

                    
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }
            
        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if(control == null) {
                return;
            }
            if(control.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                var boundsObserver = lbi.GetObservable(ListBoxItem.BoundsProperty);
                boundsObserver.Subscribe(x => BoundsChangedHandler(control, x));
            }

            //if (GetRelativeTo(control) is Control rt_control) {
            //    if (!_relativeToObserversLookup.ContainsKey(rt_control)) {
            //        _relativeToObserversLookup.Add(rt_control, new List<Control>());
            //    }
            //    if (!_relativeToObserversLookup[rt_control].Contains(control)) {
            //        _relativeToObserversLookup[rt_control].Add(control);
            //    }
            //    rt_control.DetachedFromVisualTree += DetachedFromVisualHandler;

            //    //rt_control.EffectiveViewportChanged += Rt_control_EffectiveViewportChanged;
            //    var relativeToBoundsObserver = rt_control.GetObservable(Control.BoundsProperty);
            //    relativeToBoundsObserver.Subscribe(x => BoundsChangedHandler(rt_control, x));
            //}
            //var boundsObserver = control.GetObservable(Control.BoundsProperty);
            //boundsObserver.Subscribe(x => BoundsChangedHandler(control, x));
            //control.EffectiveViewportChanged += Rt_control_EffectiveViewportChanged;
        }

        private static void Rt_control_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            if(sender is Control control) {
                BoundsChangedHandler(control, control.Bounds);
            }
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.DetachedFromVisualTree -= DetachedFromVisualHandler;
                control.AttachedToVisualTree -= Control_AttachedToVisualTree;
                control.EffectiveViewportChanged -= Rt_control_EffectiveViewportChanged;

                //if (GetRelativeTo(control) is Control rt_control) {
                //    // when specific relative to is bound remove this control from the lookup
                //    if (_relativeToObserversLookup.ContainsKey(rt_control)) {
                //        _relativeToObserversLookup[rt_control].Remove(control);
                //    }
                //}
                //if (_relativeToObserversLookup.ContainsKey(control)) {
                //    // when relative to is detached, presume observer is a descendant and remove key from lookup
                //    _relativeToObserversLookup.Remove(control);
                //}
            }
        }

        private static void BoundsChangedHandler(Control control, Rect e) {
            //if (_relativeToObserversLookup.ContainsKey(control)) {
            //    // this is a relative to control so notify all relative observers

            //    foreach (var observed_control in _relativeToObserversLookup[control]) {
            //        SetObservedBounds_safe(observed_control, control);
            //    }
            //} 
            //if(GetRelativeTo(control) is Control rt_control) {
            //    // this has a relative to control
            //    SetObservedBounds_safe(control, rt_control);
            //} else {
            //    // just cares about its parent-relative bounds
            //    SetObservedBounds_safe(control, null);
            //}
            if(control.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                //SetObservedBounds_safe(control, lbi);
                SetObservedBounds(control, lbi.Bounds.ToPortableRect());
            }
        }



        private static void SetObservedBounds_safe(Control control, Control relTo) {
            var cur_bounds = GetObservedBounds(control);
            MpRect new_bounds = relTo == null ? control.Bounds.ToPortableRect() : control.RelativeBounds(relTo);
            if (cur_bounds.FuzzyEquals(new_bounds)) {
                if(relTo == null) {
                    MpConsole.WriteLine($"Fuzzy Equal bounds change detected. Ignoring bounds changing for {control.DataContext} old val: {cur_bounds} new val: {new_bounds}");
                } else {
                    MpConsole.WriteLine($"Fuzzy Equal bounds change detected. Ignoring relative to {relTo.DataContext} bounds changing for {control.DataContext} old val: {cur_bounds} new val: {new_bounds}");
                }
                return;
            }
            SetObservedBounds(control, new_bounds);
        }

        #endregion

        #endregion
    }
}
