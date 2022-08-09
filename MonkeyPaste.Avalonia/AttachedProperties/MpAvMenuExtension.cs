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
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.Media;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMenuExtension {
        private static ContextMenu _cmInstance;

        public static bool IsChildDialogOpen { get; set; } = false;

        public static void CloseMenu() {
            if(_cmInstance == null) {
                return;
            }
            _cmInstance.Close();
            //_cmInstance = null;
        }

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

        #region SuppressDefaultRightClick AvaloniaProperty
        public static bool GetSuppressDefaultRightClick(AvaloniaObject obj) {
            return obj.GetValue(SuppressDefaultRightClickProperty);
        }

        public static void SetSuppressDefaultRightClick(AvaloniaObject obj, bool value) {
            obj.SetValue(SuppressDefaultRightClickProperty, value);
        }

        public static readonly AttachedProperty<bool> SuppressDefaultRightClickProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SuppressDefaultRightClick",
                true);

        #endregion

        #region DoubleClickCommand AvaloniaProperty
        public static ICommand GetDoubleClickCommand(AvaloniaObject obj) {
            return obj.GetValue(DoubleClickCommandProperty);
        }

        public static void SetDoubleClickCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DoubleClickCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DoubleClickCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DoubleClickCommand",
                null);

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
                if(_cmInstance == null) {
                    _cmInstance = new ContextMenu();
                }
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
                    control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);         
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
                    if (GetIsEnabled(control)) {
                        if (control.DataContext is MpISelectorItemViewModel sivm) {
                            if (e.IsLeftPress(control) || GetSelectOnRightClick(control)) {
                                sivm.Selector.SelectedItem = control.DataContext;
                            }
                        } else if (control.DataContext is MpISelectableViewModel svm) {
                            if (e.IsLeftPress(control) || GetSelectOnRightClick(control)) {
                                svm.IsSelected = true;
                            }
                        }

                        MpMenuItemViewModel mivm = null;

                        if (e.IsLeftPress(control)) {
                            if(e.ClickCount == 2 && GetDoubleClickCommand(control) != null) {
                                GetDoubleClickCommand(control).Execute(null);
                            } else if(control.DataContext is MpIPopupMenuViewModel pumvm) {
                                mivm = pumvm.PopupMenuViewModel;
                            }                            
                        } else if (e.IsRightPress(control)) {
                            if (control.DataContext is MpIContextMenuViewModel cmvm) {
                                mivm = cmvm.ContextMenuViewModel;
                            }
                        }

                        if (mivm == null) {
                            e.Handled = GetSuppressDefaultRightClick(control);                            
                            SetIsOpen(control, false);
                            return;
                        }
                        SetIsOpen(control, true);
                        e.Handled = true;

                        CancelEventHandler onOpenHandler = null;
                        CancelEventHandler onCloseHandler = null;

                        onCloseHandler = (s, e1) => {
                            SetIsOpen(control, false);
                            _cmInstance.ContextMenuClosing -= onCloseHandler;
                            _cmInstance.ContextMenuOpening -= onOpenHandler;
                            //_cmInstance = null;
                            control.ContextMenu = null;
                        };
                        
                        onOpenHandler = (s, e1) => {
                            e1.Cancel = false;
                        };

                        _cmInstance.Items = mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x));
                        _cmInstance.PlacementTarget = control;
                        _cmInstance.PlacementAnchor = PopupAnchor.TopRight;
                        _cmInstance.DataContext = mivm;

                        _cmInstance.ContextMenuOpening += onOpenHandler;
                        _cmInstance.ContextMenuClosing += onCloseHandler;

                        _cmInstance.Open(MpAvMainWindow.Instance);
                    }
                }

                Control CreateMenuItem(MpMenuItemViewModel mivm) {
                    Control control = null;
                    string itemType = new MpAvMenuItemDataTemplateSelector().GetTemplateName(mivm);

                    switch (itemType) {
                        case "CheckableMenuItemTemplate":
                            control = new MenuItem() {
                                Icon = new Border() {
                                    HorizontalAlignment = HorizontalAlignment.Stretch,
                                    VerticalAlignment = VerticalAlignment.Stretch,
                                    MinWidth = 20,
                                    MinHeight = 20,
                                    Background = mivm.IconHexStr.ToAvBrush(),
                                    Child = new PathIcon() {
                                        Data = MpPlatformWrapper.Services.PlatformResource.GetResource("CheckSvg") as StreamGeometry,
                                        Foreground = Brushes.Black,
                                        IsVisible = mivm.IsChecked
                                    }
                                },
                                Header = mivm.Header,
                                Items = mivm.SubItems == null ?
                                    null :
                                    mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x)),
                                Command = mivm.Command,
                                CommandParameter = mivm.CommandParameter,
                                InputGesture = string.IsNullOrWhiteSpace(mivm.InputGestureText) ?
                                    null :
                                    KeyGesture.Parse(mivm.InputGestureText)
                            };
                            control.PointerEnter += Control_PointerEnter;
                            control.PointerLeave += Control_PointerLeave;
                            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;

                            if (mivm.SubItems == null || mivm.SubItems.Count == 0) {
                                control.AddHandler(Control.PointerReleasedEvent, CheckableControl_PointerReleased, RoutingStrategies.Tunnel);
                            }
                            break;
                        case "DefaultMenuItemTemplate":
                            control = new MenuItem() {
                                Icon = mivm.IconSourceObj == null ?
                                    null :
                                    new Image() {
                                        HorizontalAlignment = HorizontalAlignment.Stretch,
                                        VerticalAlignment = VerticalAlignment.Stretch,
                                        Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(mivm.IconSourceObj, null, null, null) as Bitmap,
                                    },
                                Header = mivm.Header,
                                Items = mivm.SubItems == null ?
                                    null :
                                    mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x)),
                                Command = mivm.Command,
                                CommandParameter = mivm.CommandParameter,
                                InputGesture = string.IsNullOrWhiteSpace(mivm.InputGestureText) ?
                                    null :
                                    KeyGesture.Parse(mivm.InputGestureText)
                            };
                            control.PointerEnter += Control_PointerEnter;
                            control.PointerLeave += Control_PointerLeave;
                            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;

                            if (mivm.SubItems == null || mivm.SubItems.Count == 0) {
                                control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, RoutingStrategies.Tunnel);
                            }
                            break;
                        case "SeperatorMenuItemTemplate":
                            control = new MenuItem() {
                                Header = "-"
                            };
                            break;
                        case "ColorPalleteMenuItemTemplate":
                            control = new MenuItem() {
                                //DataContext = mivm,
                                Header = new MpAvColorPaletteListBoxView() {
                                    DataContext = mivm
                                }
                            };
                            break;
                        case "ColorPalleteItemMenuItemTemplate":

                            break;
                    }

                    return control;
                }
            }

        }
        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
            e.Handled = false;
        }
        private static void CheckableControl_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if(sender is MenuItem mi && mi.DataContext is MpMenuItemViewModel mivm) {
                mivm.IsChecked = !mivm.IsChecked;

                var pi = mi.GetVisualDescendant<PathIcon>();
                if(pi != null) {
                    pi.IsVisible = mivm.IsChecked;
                }
            }
            e.Handled = false;
        }


        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Control control) {
                control.PointerEnter -= Control_PointerEnter;
                control.PointerLeave -= Control_PointerLeave;
                control.DetachedFromVisualTree -= Control_DetachedFromVisualTree;
                control.PointerReleased -= Control_PointerReleased;
            }
        }

        private static void Control_PointerLeave(object sender, PointerEventArgs e) {
            if (sender is MenuItem mi) {
                mi.Background = Brushes.Transparent;
            }
        }

        private static void Control_PointerEnter(object sender, PointerEventArgs e) {
            if (sender is MenuItem mi) {
                mi.Background = Brushes.LightBlue;
            }
        }
    
        

        #endregion

        #endregion
    }

    public class MpAvContextMenuCloser : MpIContextMenuCloser {
        public void CloseMenu() {
            MpAvMenuExtension.CloseMenu();
        }
    }
}
