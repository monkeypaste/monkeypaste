using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Controls;
using PropertyChanged;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public enum MpResizeEdgeType {
        None = 0,
        Left,
        Top,
        Right,
        Bottom
    }

    [DoNotNotify]
    public static class MpAvResizeExtension {
        #region Private Variables

        private static MpPoint? _lastMousePosition, _mouseDownPosition;
        #endregion

        #region Constants

        public const double MAX_RESIZE_EDGE_DISTANCE = 5.0d;

        #endregion

        #region Properties
        public static bool IsAnyResizing { get; private set; }

        #region Elements

        #endregion

        #region Layout

        #region BoundWidth Property
        public static double GetBoundWidth(AvaloniaObject obj) {
            return obj.GetValue(BoundWidthProperty);
        }

        public static void SetBoundWidth(AvaloniaObject obj, double value) {
            obj.SetValue(BoundWidthProperty, value);
        }

        public static readonly AttachedProperty<double>
            BoundWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "BoundWidth",
            0d,
            false,
            BindingMode.TwoWay);

        #endregion

        #region BoundHeight Property
        public static double GetBoundHeight(AvaloniaObject obj) {
            return obj.GetValue(BoundHeightProperty);
        }

        public static void SetBoundHeight(AvaloniaObject obj, double value) {
            obj.SetValue(BoundHeightProperty, value);
        }

        public static readonly AttachedProperty<double>
            BoundHeightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "BoundHeight",
            0d,
            false,
            BindingMode.TwoWay);

        #endregion

        #region MinWidth Property
        public static double GetMinWidth(AvaloniaObject obj) {
            return obj.GetValue(MinWidthProperty);
        }

        public static void SetMinWidth(AvaloniaObject obj, double value) {
            obj.SetValue(MinWidthProperty, value);
        }

        public static readonly AttachedProperty<double>
            MinWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "MinWidth",
            0d);

        #endregion

        #region MinHeight Property
        public static double GetMinHeight(AvaloniaObject obj) {
            return obj.GetValue(MinHeightProperty);
        }

        public static void SetMinHeight(AvaloniaObject obj, double value) {
            obj.SetValue(MinHeightProperty, value);
        }

        public static readonly AttachedProperty<double>
            MinHeightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "MinHeight",
            0d);

        #endregion

        #region MaxWidth Property
        public static double GetMaxWidth(AvaloniaObject obj) {
            return obj.GetValue(MaxWidthProperty);
        }

        public static void SetMaxWidth(AvaloniaObject obj, double value) {
            obj.SetValue(MaxWidthProperty, value);
        }

        public static readonly AttachedProperty<double>
            MaxWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "MaxWidth",
            0d);

        #endregion

        #region MaxHeight Property
        public static double GetMaxHeight(AvaloniaObject obj) {
            return obj.GetValue(MaxHeightProperty);
        }

        public static void SetMaxHeight(AvaloniaObject obj, double value) {
            obj.SetValue(MaxHeightProperty, value);
        }

        public static readonly AttachedProperty<double>
            MaxHeightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "MaxHeight",
            0d);

        #endregion

        #region DefaultWidth Property
        public static double GetDefaultWidth(AvaloniaObject obj) {
            return obj.GetValue(DefaultWidthProperty);
        }

        public static void SetDefaultWidth(AvaloniaObject obj, double value) {
            obj.SetValue(DefaultWidthProperty, value);
        }

        public static readonly AttachedProperty<double>
            DefaultWidthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "DefaultWidth",
            0d);

        #endregion

        #region DefaultHeight Property
        public static double GetDefaultHeight(AvaloniaObject obj) {
            return obj.GetValue(DefaultHeightProperty);
        }

        public static void SetDefaultHeight(AvaloniaObject obj, double value) {
            obj.SetValue(DefaultHeightProperty, value);
        }

        public static readonly AttachedProperty<double>
            DefaultHeightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
            "DefaultHeight",
            0d);

        #endregion

        #endregion

        #region State

        #region CanResize Property
        public static bool GetCanResize(AvaloniaObject obj) {
            return obj.GetValue(CanResizeProperty);
        }

        public static void SetCanResize(AvaloniaObject obj, bool value) {
            obj.SetValue(CanResizeProperty, value);
        }

        public static readonly AttachedProperty<bool>
            CanResizeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "CanResize",
            true,
            false,
            BindingMode.TwoWay);

        #endregion

        #region IsResizing Property
        public static bool GetIsResizing(AvaloniaObject obj) {
            return obj.GetValue(IsResizingProperty);
        }

        public static void SetIsResizing(AvaloniaObject obj, bool value) {
            obj.SetValue(IsResizingProperty, value);
        }

        public static readonly AttachedProperty<bool>
            IsResizingProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "IsResizing",
            false,
            false,
            BindingMode.TwoWay);

        private static void HandleIsResizingChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isResizing) {
                IsAnyResizing = isResizing;
            }
        }

        #endregion

        #region ResizerEdge Property
        public static MpResizeEdgeType GetResizerEdge(AvaloniaObject obj) {
            return obj.GetValue(ResizerEdgeProperty);
        }

        public static void SetResizerEdge(AvaloniaObject obj, MpResizeEdgeType value) {
            obj.SetValue(ResizerEdgeProperty, value);
        }

        public static readonly AttachedProperty<MpResizeEdgeType>
            ResizerEdgeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpResizeEdgeType>(
            "ResizerEdge",
            MpResizeEdgeType.None);

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
                false,
                BindingMode.OneWay);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    control.PointerEnter += PointerEnterHandler;
                    control.PointerLeave += PointerLeaveHandler;
                    control.PointerPressed += PointerPressedHandler;
                    control.PointerReleased += PointerReleasedHandler;
                    control.PointerMoved += PointerMovedHandler;
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.PointerEnter -= PointerEnterHandler;
                    control.PointerLeave -= PointerLeaveHandler;
                    control.PointerPressed -= PointerPressedHandler;
                    control.PointerReleased -= PointerReleasedHandler;
                    control.PointerMoved -= PointerMovedHandler;
                }
            }

            void PointerEnterHandler(object? s, PointerEventArgs e) {
                if (s is AvaloniaObject ao) {
                    if (!GetIsEnabled(ao) ||
                        IsAnyResizing) {
                        return;
                    }
                    SetCanResize(ao, true);
                }
            }

            void PointerLeaveHandler(object? s, PointerEventArgs e) {
                if (s is Control control) {
                    if (!GetIsEnabled(control) || GetIsResizing(control)) {
                        return;
                    }
                    if (!GetIsResizing(control)) {
                        SetCanResize(control, false);
                    }
                }
            }

            void PointerPressedHandler(object? s, PointerPressedEventArgs e) {
                if (s is Control control &&
                    e.GetCurrentPoint(control)
                    .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                    if (!GetIsEnabled(control)) {
                        return;
                    }
                    if(e.ClickCount > 1) {
                        e.Pointer.Capture(control);
                        ResetToDefault(control);

                        Dispatcher.UIThread.Post(async () => {
                            await Task.Delay(1000);
                            e.Pointer.Capture(null);
                        });

                        return;
                    }
                    if (control.DataContext is MpISelectableViewModel svm) {
                        svm.IsSelected = true;
                    }
                    e.Pointer.Capture(control);
                    if (e.Pointer.Captured == control) {
                        SetIsResizing(control, true);

                        _lastMousePosition = _mouseDownPosition = e.GetPosition(MainWindow.Instance).ToPortablePoint();
                    }
                }
            }

            void PointerReleasedHandler(object? s, PointerReleasedEventArgs e) {
                if (s is Control control &&
                    e.GetCurrentPoint(control)
                    .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased) {
                    if (!GetIsEnabled(control)) {
                        return;
                    }
                    
                    if (GetIsResizing(control)) {
                        Reset(control);
                        e.Pointer.Capture(null);
                    }
                }
            }

            void PointerMovedHandler(object? s, PointerEventArgs e) {
                if (s is Control control) {
                    if (e.Pointer.Captured != control ||
                       !GetIsEnabled(control) ||
                        _mouseDownPosition == null
                       //MpClipTrayViewModel.Instance.HasScrollVelocity
                       ) {
                        return;
                    }
                    if (!e.GetCurrentPoint(control)
                        .Properties.IsLeftButtonPressed) {
                        Reset(control);
                        return;
                    }

                    MpPoint mw_mp = null;
                    if (GetIsResizing(control)) {
                        mw_mp = e.GetPosition(MainWindow.Instance).ToPortablePoint();
                        var delta = mw_mp - _lastMousePosition;
                        Resize(control, delta.X, -delta.Y);
                    }
                    _lastMousePosition = mw_mp;
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Constructors
        static MpAvResizeExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsResizingProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsResizingChanged(x, y));
        }
        #endregion

        #region Public Methods
        public static void ResetToDefault(Control control) {
            MpPoint curSize = new MpPoint(GetBoundWidth(control), GetBoundHeight(control));
            MpPoint defSize = new MpPoint(GetDefaultWidth(control), GetDefaultHeight(control));

            MpPoint delta = defSize - curSize;

            bool wasResizing = GetIsResizing(control);
            if(!wasResizing) {
                SetIsResizing(control, true);
            }

            Resize(control, delta.X, delta.Y);
            Reset(control);

            //if (AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
            //    if (defaultSize.X != MpClipTileViewModel.DefaultBorderWidth ||
            //       defaultSize.Y != MpClipTileViewModel.DefaultBorderHeight) {
            //        //Debugger.Break();

            //        defaultSize = new Point(MpClipTileViewModel.DefaultBorderWidth, MpClipTileViewModel.DefaultBorderHeight);
            //    }
            //    bool resetToContentSize = false;
            //    if (curSize.X == defaultSize.X) {
            //        resetToContentSize = true;
            //        if (AssociatedObject.DataContext is MpClipTileViewModel ctvm1) {
            //            var ds = ctvm1.UnformattedContentSize;
            //            defaultSize = new Point(ds.Width, ds.Height);
            //        }

            //    }

            //    Vector delta = curSize - defaultSize;
            //    Resize(-delta.X, -delta.Y);
            //    Reset();

            //    if (!resetToContentSize && !ctvm.IsPlaceholder) {
            //        //MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup.Remove(ctvm.CopyItemId);
            //        MpClipTrayViewModel.Instance.RemovePersistentWidthById(ctvm.CopyItemId);
            //    }

            //} else if (AssociatedObject.DataContext is MpMainWindowViewModel mwvm) {
            //    Vector delta = curSize - defaultSize;
            //    Resize(-delta.X, -delta.Y);
            //    Reset();
            //}
        }

        public static void Resize(Control control, double dx, double dy) {
            if (GetResizerEdge(control) != MpResizeEdgeType.Left && 
                GetResizerEdge(control) != MpResizeEdgeType.Right) {
                dx = 0;
            }
            if (GetResizerEdge(control) != MpResizeEdgeType.Top &&
                GetResizerEdge(control) != MpResizeEdgeType.Bottom) {
                dy = 0;
            }
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }

            //MpConsole.WriteLine("dx " + dx + " dy " + dy);

            double bound_width = GetBoundWidth(control);
            double bound_height = GetBoundHeight(control);

            if (bound_width + dx < 0) {
                ResetToDefault(control);
                return;
            }

            double nw = bound_width + dx;
            bound_width = Math.Min(Math.Max(nw, GetMinWidth(control)), GetMaxWidth(control));

            double nh = bound_height + dy;
            bound_height = Math.Min(Math.Max(nh, GetMinHeight(control)), GetMaxHeight(control));

            SetBoundWidth(control, bound_width);
            SetBoundHeight(control, bound_height);

            MpMessenger.SendGlobal(MpMessageType.ResizingContent);
            if (!GetIsResizing(control)) {
                MpMessenger.SendGlobal(MpMessageType.ResizeContentCompleted);
            }
        }
        #endregion

        #region Private Methods
        private static void Reset(Control? control) {
            if (control == null) {
                return;
            }
            SetIsResizing(control, false);
            _lastMousePosition = _mouseDownPosition = null;
        }

        #endregion
    }

}
