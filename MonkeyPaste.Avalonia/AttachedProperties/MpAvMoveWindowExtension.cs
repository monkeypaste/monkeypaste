using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMoveWindowExtension {
        #region Private Variables
        private static PixelPoint? _downPos;
        private static PixelPoint _initialWindowPos;

        #endregion

        #region Statics
        static MpAvMoveWindowExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion
        #region Properties

        #region RejectedControlTypeNames AvaloniaProperty
        public static string GetRejectedControlTypeNames(AvaloniaObject obj) {
            return obj.GetValue(RejectedControlTypeNamesProperty);
        }

        public static void SetRejectedControlTypeNames(AvaloniaObject obj, string value) {
            obj.SetValue(RejectedControlTypeNamesProperty, value);
        }

        public static readonly AttachedProperty<string> RejectedControlTypeNamesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "RejectedControlTypeNames",
                $"{typeof(TextBox).ToAssemblyReferencedString()}|{typeof(Button).ToAssemblyReferencedString()}");

        #endregion

        #region MoveCursorType AvaloniaProperty
        public static StandardCursorType GetMoveCursorType(AvaloniaObject obj) {
            return obj.GetValue(MoveCursorTypeProperty);
        }

        public static void SetMoveCursorType(AvaloniaObject obj, StandardCursorType value) {
            obj.SetValue(MoveCursorTypeProperty, value);
        }

        public static readonly AttachedProperty<StandardCursorType> MoveCursorTypeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, StandardCursorType>(
                "MoveCursorType",
                StandardCursorType.SizeAll);

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
                element is Control w) {
                if (isEnabledVal) {
                    w.AttachedToVisualTree += Control_AttachedToVisualTree;
                    w.DetachedFromVisualTree += Control_DetachedToVisualHandler;
                    if (w.IsAttachedToVisualTree()) {
                        Control_AttachedToVisualTree(w, null);
                    }
                } else {
                    Control_DetachedToVisualHandler(element, null);
                }
            }
        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not Control w) {
                return;
            }

            w.PointerPressed += Control_PointerPressed;
            w.PointerMoved += Control_PointerMoved;
            w.PointerReleased += Control_PointerReleased;
        }
        private static void Control_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is not Control w) {
                return;
            }
            w.AttachedToVisualTree -= Control_AttachedToVisualTree;
            w.PointerPressed -= Control_PointerPressed;
            w.PointerMoved -= Control_PointerMoved;
            w.PointerReleased -= Control_PointerReleased;
        }



        private static void Control_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            _downPos = null;
            if (e.Source is not Control source_control ||
                sender is not Control attached_control ||
                TopLevel.GetTopLevel(attached_control) is not Window w ||
                source_control.GetSelfAndVisualAncestors().Any(x => IsWindowMoveRejected(attached_control, x))) {
                return;
            }
            _initialWindowPos = w.Position;
            _downPos = w.PointToScreen(e.GetPosition(w));
            e.Pointer.Capture(w);
            w.Cursor = new Cursor(GetMoveCursorType(attached_control));
        }

        private static void Control_PointerMoved(object sender, PointerEventArgs e) {
            if (!_downPos.HasValue ||
                sender is not Control attached_control ||
                !e.IsLeftDown(attached_control) ||
                TopLevel.GetTopLevel(attached_control) is not Window w) {
                _downPos = null;
                return;
            }

            var curMousePos = w.PointToScreen(e.GetPosition(w));
            w.Position = _initialWindowPos + (curMousePos - _downPos.Value);
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            _downPos = null;
            e.Pointer.Capture(null);
            if (sender is not Control attached_control ||
                TopLevel.GetTopLevel(attached_control) is not Window w) {
                return;
            }
            w.Cursor = new Cursor(StandardCursorType.Arrow);
        }

        private static bool IsWindowMoveRejected(Control attached_control, Visual v) {
            if (v.Classes.Contains("rejectWindowMove")) {
                return true;
            }
            if (GetRejectedControlTypeNames(attached_control) is not string rejectTypesStr ||
                rejectTypesStr.Split("|") is not string[] rejectTypesStrParts) {
                return false;
            }
            try {
                if (rejectTypesStrParts.Select(x => Type.GetType(x)) is not IEnumerable<Type> rejectTypes) {
                    return false;
                }
                return rejectTypes.Any(x => v.GetType().IsClassSubclassOfOrImplements(x));
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error parsing rejected control types from '{rejectTypesStr}'.", ex);
            }
            return false;
        }
        #endregion


        #endregion
    }
}
