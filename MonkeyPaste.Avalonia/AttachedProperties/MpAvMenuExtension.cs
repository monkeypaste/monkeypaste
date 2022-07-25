using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Linq;
using Avalonia.Input;
using System.ComponentModel;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace MonkeyPaste.Avalonia {

    public static class MpAvMenuExtension {
        static MpAvMenuExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region SelectOnRightClick AvaloniaProperty
        public static bool GetSelectOnRightClick(AvaloniaObject obj) {
            return obj.GetValue(SelectOnRightClickProperty);
        }

        public static void SetSelectOnRightClick(AvaloniaObject obj, bool value) {
            obj.SetValue(SelectOnRightClickProperty, value);
        }

        public static readonly AttachedProperty<bool> SelectOnRightClickProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SelectOnRightClick",
                true);

        #endregion

        #region IsOpen AvaloniaProperty
        public static bool GetIsOpen(AvaloniaObject obj) {
            return obj.GetValue(IsOpenProperty);
        }

        public static void SetIsOpen(AvaloniaObject obj, bool value) {
            obj.SetValue(IsOpenProperty, value);
        }

        public static readonly AttachedProperty<bool> IsOpenProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsOpen",
                false,
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
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                    control.DetachedFromVisualTree += DetachedToVisualHandler;

                    if(control.DataContext is MpIMenuItemViewModelBase cmvm) {
                        control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);

                        //if (control is MpAvTagView tv) {
                        //    var tvi = tv.GetVisualAncestor<TreeViewItem>();
                        //    tvi.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
                        //}
                    }
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.RemoveHandler(Control.PointerPressedEvent, Control_PointerPressed);
                }
            }

            void Control_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
                if(sender is Control control) {
                    //if(control is TreeViewItem tvi) {
                    //    control = tvi.GetVisualDescendant<MpAvTagView>();
                    //}
                    if (GetIsEnabled(control)) {
                        MpMenuItemViewModel mivm = null;

                        if (e.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                            if (control.DataContext is MpIPopupMenuViewModel pumvm) {
                                SetIsOpen(control, true);

                                mivm = pumvm.PopupMenuViewModel;

                                if (control.DataContext is MpISelectableViewModel svm) {
                                    svm.IsSelected = true;
                                }
                            }
                        } else if (e.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                            if (control.DataContext is MpIContextMenuViewModel cmvm) {
                                SetIsOpen(control, true);

                                mivm = cmvm.ContextMenuViewModel;

                                if (control.DataContext is MpISelectableViewModel svm &&
                                    GetSelectOnRightClick(control)) {
                                    svm.IsSelected = true;
                                }
                            }
                        }

                        if (mivm == null) {
                            e.Handled = false;
                            SetIsOpen(control, false);
                            return;
                        }

                        e.Handled = true;

                        CancelEventHandler onOpenHandler = null;
                        CancelEventHandler onCloseHandler = null;

                        onCloseHandler = (s, e1) => {
                            SetIsOpen(control, false);
                            control.ContextMenu.ContextMenuClosing -= onCloseHandler;
                            control.ContextMenu.ContextMenuOpening -= onOpenHandler;
                        };
                        
                        onOpenHandler = (s, e1) => {
                            e1.Cancel = false;
                        };


                        control.ContextMenu = MpAvContextMenuView.Instance;
                        control.ContextMenu.DataContext = mivm;
                        control.ContextMenu.PlacementTarget = control;
                        control.ContextMenu.PlacementAnchor = PopupAnchor.TopRight;

                        control.ContextMenu.ContextMenuOpening += onOpenHandler;
                        control.ContextMenu.ContextMenuClosing += onCloseHandler;

                        control.ContextMenu.Open(control);

                    }
                }
            }
        }


        #endregion

        public static void ShowContextMenu(Control control) {
            if(control != null && control.DataContext is MpIContextMenuViewModel cmvm) {
                CancelEventHandler onOpenHandler = null;
                CancelEventHandler onCloseHandler = null;

                onCloseHandler = (s, e1) => {
                    SetIsOpen(control, false);
                    control.ContextMenu.ContextMenuClosing -= onCloseHandler;
                    control.ContextMenu.ContextMenuOpening -= onOpenHandler;
                };

                onOpenHandler = (s, e1) => {
                    SetIsOpen(control, true);
                    e1.Cancel = false;
                };

                MpAvContextMenuView.Instance.DataContext = cmvm;
                control.ContextMenu = MpAvContextMenuView.Instance;
                //control.ContextMenu.PlacementTarget = control;
                control.ContextMenu.ContextMenuOpening += onOpenHandler;
                control.ContextMenu.ContextMenuClosing += onCloseHandler;

                MpAvContextMenuView.Instance.PlacementTarget = control;
                MpAvContextMenuView.Instance.Open(control);
            }
        }
        #endregion
    }
}
