using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMenuExtension {
        private static MpAvContextMenuView _cmInstance { get; set; }

        private static List<MenuItem> openSubMenuItems = new List<MenuItem>();

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
                    _cmInstance = new MpAvContextMenuView();
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
            

        }

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                if (e == null) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                }
                control.DetachedFromVisualTree += DetachedToVisualHandler;
                //mi.ContextMenu.ContextMenuOpening += ContextMenu_ContextMenuOpening;
                control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
            }
        }
        private static void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.AttachedToVisualTree -= AttachedToVisualHandler;
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
                control.RemoveHandler(Control.PointerPressedEvent, Control_PointerPressed);
            }
        }

        private static void Control_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (sender is Control control) {
                if (GetIsEnabled(control)) {
                    e.Handled = false;

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
                        if (e.ClickCount == 2 && GetDoubleClickCommand(control) != null) {
                            GetDoubleClickCommand(control).Execute(null);
                        } else if (control.DataContext is MpIPopupMenuViewModel pumvm) {
                            mivm = pumvm.PopupMenuViewModel;
                        }
                    } else if (e.IsRightPress(control)) {
                        if (control.DataContext is MpIContextMenuViewModel cmvm) {
                            mivm = cmvm.ContextMenuViewModel;
                        }
                    }

                    if (mivm == null) {
                        e.Handled = GetSuppressDefaultRightClick(control) && e.IsRightPress(control);
                        SetIsOpen(control, false);
                        return;
                    }
                    SetIsOpen(control, true);
                    e.Handled = true;

                    CancelEventHandler onOpenHandler = null;
                    CancelEventHandler onCloseHandler = null;

                    onCloseHandler = (s, e1) => {
                        MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
                        SetIsOpen(control, false);
                        _cmInstance.ContextMenuClosing -= onCloseHandler;
                        _cmInstance.ContextMenuOpening -= onOpenHandler;
                        control.ContextMenu = null;
                        openSubMenuItems.Clear();
                    };

                    onOpenHandler = (s, e1) => {
                        e1.Cancel = false;
                    };

                    _cmInstance.Items = mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x));

                    _cmInstance.PlacementTarget = control;
                    _cmInstance.PlacementAnchor = PopupAnchor.AllMask;
                    _cmInstance.PlacementMode = PlacementMode.Pointer;
                    _cmInstance.DataContext = mivm;

                    _cmInstance.ContextMenuOpening += onOpenHandler;
                    _cmInstance.ContextMenuClosing += onCloseHandler;

                    var ctrl_mp = e.GetPosition(control);
                    _cmInstance.HorizontalOffset = ctrl_mp.X;
                    _cmInstance.VerticalOffset = ctrl_mp.Y;

                    _cmInstance.Open(MpAvMainWindow.Instance);
                    MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
                    //flyout.ShowAt(mi);
                }
            }            
        }
        private static void UpdateMenu() {
            if(_cmInstance != null && _cmInstance.DataContext is MpMenuItemViewModel mivm) {
                _cmInstance.Items = mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x));
            }
        }
        private static Control CreateMenuItem(MpMenuItemViewModel mivm) {
            Control control = null;
            string itemType = new MpAvMenuItemDataTemplateSelector().GetTemplateName(mivm);
            KeyGesture inputGesture = null;
            if(!string.IsNullOrEmpty(mivm.InputGestureText)) {
                inputGesture = KeyGesture.Parse(mivm.InputGestureText);
            }

            switch (itemType) {
                case MpMenuItemViewModel.DEFAULT_TEMPLATE_NAME:
                case MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME:
                    var mi = new MenuItem() { 
                        Header = mivm.Header.EscapeMenuItemHeader(mivm.AltNavIdx),
                        Command = mivm.Command,
                        CommandParameter = mivm.CommandParameter,
                        InputGesture = inputGesture,
                        DataContext = mivm,
                    };
                    if(itemType == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                        mi.Icon = CreateCheckableIcon(mivm);
                    } else {
                        mi.Icon = mivm.IconSourceObj == null ?
                            null :
                            new Image() {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(mivm.IconSourceObj, null, null, null) as Bitmap,
                            };
                    }
                    if(mivm.SubItems != null && mivm.SubItems.Count > 0) {
                        var subItems = new ObservableCollection<Control>();
                        foreach(var si in mivm.SubItems) {
                            subItems.Add(CreateMenuItem(si));
                        }
                        mi.Items = subItems;
                    }
                    mi.PointerEnterItem += Control_PointerEnter;
                    mi.DetachedFromVisualTree += Control_DetachedFromVisualTree;
                    if (mi.Command != null) {
                        mi.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, RoutingStrategies.Tunnel);
                        //mi.GetVisualDescendants().Where(x => x is Control).Cast<Control>().ForEach(x => x.PointerReleased += Control_PointerReleased);
                    }
                    control = mi;
                    break;
                case MpMenuItemViewModel.SEPERATOR_TEMPLATE_NAME:
                    control = new MenuItem() {
                        Header = "-",
                        DataContext = mivm
                    };
                    break;
                case MpMenuItemViewModel.COLOR_PALETTE_TEMPLATE_NAME:
                    control = new MenuItem() {
                        Header = new MpAvColorPaletteListBoxView() {
                            DataContext = mivm
                        },
                        DataContext = mivm
                    };

                    control.PointerEnter += Control_PointerEnter;
                    control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
                    break;
            }
            return control;
        }

        private static object CreateCheckableIcon(MpMenuItemViewModel mivm) {
            var iconBorder = new Border() {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 20,
                MinHeight = 20,
                BorderThickness = new Thickness(1),
                BorderBrush = mivm.BorderHexColor.ToAvBrush(),
                CornerRadius = new CornerRadius(2.5),
                Margin = new Thickness(5, 0, 30, 0),
                Background = mivm.IconHexStr.ToAvBrush()
            };
            var pi = new PathIcon() {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = mivm.CheckResourceKey == "CheckSvg" ? 15:7,
                Height = mivm.CheckResourceKey == "CheckSvg" ? 15 : 7,
                Data = MpPlatformWrapper.Services.PlatformResource.GetResource(mivm.CheckResourceKey) as StreamGeometry,
                Foreground = mivm.IconHexStr.HexColorToContrastingFgHexColor().ToAvBrush()
            };
            iconBorder.Child = pi;
            return iconBorder;
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            MenuItem mi = null;
            if (e.Source is IVisual sourceVisual &&
                sourceVisual.GetVisualAncestor<MenuItem>() is MenuItem smi) {
                MpConsole.WriteLine("Released (source): " + mi);
                mi = smi;
            } else if (sender is MenuItem sender_mi) {
                mi = sender_mi;
                MpConsole.WriteLine("Released (sender): " + mi);
            }
            if (mi != null &&
                mi.DataContext is MpMenuItemViewModel mivm) {

                if (mivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                    e.Handled = true;
                    mivm.Command.Execute(mivm.CommandParameter);

                    // update toggled cb
                    mivm.IsChecked = mivm.IsChecked.DefaultToggleValue(true);
                    var self_ancestor_mil = GetParentMenuItems(mi, true)
                                .Where(x => x.DataContext is MpMenuItemViewModel pmivm && pmivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME);

                    foreach (var cur_mi in self_ancestor_mil) {
                        var cur_mi_mivm = cur_mi.DataContext as MpMenuItemViewModel;
                        bool? is_cur_mi_checked = false;
                        if (cur_mi_mivm.IsChecked.IsTrue()) {
                            is_cur_mi_checked = true;
                        } else {
                            is_cur_mi_checked = cur_mi_mivm.SubItems.Any(x => x.IsChecked.IsTrueOrNull()) ? null : false;
                        }
                        cur_mi_mivm.IsChecked = is_cur_mi_checked;
                        cur_mi.Icon = CreateCheckableIcon(cur_mi_mivm);
                    }

                    EventHandler<PointerEventArgs> move_handler = null;
                    move_handler = (s, e1) => {
                        _cmInstance.RemoveHandler(InputElement.PointerLeaveEvent, move_handler);
                        CloseMenu();
                    };
                    _cmInstance.AddHandler(InputElement.PointerLeaveEvent, move_handler, RoutingStrategies.Tunnel, true);
                } else {

                    MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
                    e.Handled = false;
                }
            }
        }

        private static void CheckableControl_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if (sender is MenuItem mi && mi.DataContext is MpMenuItemViewModel mivm) {
                mivm.IsChecked = mivm.IsChecked.DefaultToggleValue();

                
                // wait to close menu till off item, that way see the check but still closes? 
                EventHandler<PointerEventArgs> leave_handler = null;
                leave_handler = (s, e) => {
                    CloseMenu();
                    mi.PointerLeave -= leave_handler;
                };
                mi.PointerLeave += leave_handler;
            }
            e.Handled = false;
        }


        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is MenuItem control) {
                control.PointerEnterItem -= Control_PointerEnter;
                control.PointerReleased -= Control_PointerReleased;
                control.RemoveHandler(Control.PointerReleasedEvent, Control_PointerReleased);
            }
        }

        private static void Control_PointerEnter(object sender, PointerEventArgs e) {
            if (e.Source is MenuItem mi && mi.DataContext is MpMenuItemViewModel mivm) {
               // MpConsole.WriteLine("Pointer enter: " + mivm.Header);
               
                var openMenusToRemove = new List<MenuItem>();
                foreach(var osmi in openSubMenuItems) {
                    var cmil = GetChildMenuItems(osmi);
                    var pmil = GetParentMenuItems(mi);
                    if (cmil.Select(x => x.DataContext).Cast<MpMenuItemViewModel>().All(x => x != mivm)) {
                        osmi.Close();
                        osmi.Background = Brushes.Transparent;
                        //var pcl = openSubMenuItem.GetVisualAncestors<Control>();
                        var ccl = osmi.GetVisualDescendants<Control>();
                        var child_border = ccl.FirstOrDefault(x => x is Border b && (b.Background.ToString() == "#19000000" || b.Background.ToString() == Brushes.LightBlue.ToString()) && b.Tag == null);
                        if (child_border != null) {
                            (child_border as Border).Background = Brushes.Transparent;
                        }
                        mi.GetVisualAncestor<Panel>().Background = "#FFF2F2F2".ToAvBrush();
                        openMenusToRemove.Add(osmi);
                        //openSubMenuItem = null;
                    }
                }
                foreach(var mitr in openMenusToRemove) {
                    openSubMenuItems.Remove(mitr);
                }

                if (mivm.SubItems != null && mivm.SubItems.Count > 0 && !mivm.IsColorPallete) {
                    //mi.Items.Cast<Control>().ForEach(x => x.InvalidateVisual());
                    mi.InvalidateVisual();
                    mi.IsSubMenuOpen = true;
                    mi.Background = Brushes.LightBlue;
                    var ccl = mi.GetVisualDescendants<Control>();
                    var child_border = ccl.FirstOrDefault(x => x is Border b && b.Tag == null);
                    
                    if (child_border != null) {
                        (child_border as Border).Background = Brushes.LightBlue;
                    }
                    openSubMenuItems.Add(mi);
                    //MpConsole.WriteLine("Sub Menu Opened for: " + mi.Header.ToString());
                    mi.Open();
                }
            }
        }

        private static IEnumerable<MenuItem> GetChildMenuItems(MenuItem mi) {
            var items = new List<MenuItem>();
            if(mi != null) {
                items.Add(mi);
                if (mi.Items != null) {
                    foreach (var cmiObj in mi.Items) {
                        if (cmiObj is MenuItem cmi) {
                            items.Add(cmi);
                            var cmil = GetChildMenuItems(cmi);
                            items.AddRange(cmil);
                        }
                    }
                }
            }
            return items;
        }

        private static List<MenuItem> GetParentMenuItems(MenuItem mi, bool includeSelf = false) {
            var items = new List<MenuItem>();
            if (mi != null) {
                if (includeSelf) {
                    items.Add(mi);
                }
                object parentObj = mi.Parent;
                while (parentObj != null) {
                    if (parentObj is MenuItem pmi) {
                        items.Add(pmi);
                        parentObj = pmi.Parent;
                    } else {
                        parentObj = null;
                    }
                }
            }
            return items;
        }
        //private static List<MenuItem> GetParentMenuItems(MenuItem mi, bool includeSelf = false) {
        //    var items = new List<MenuItem>();
        //    if (mi != null) {
        //        if(includeSelf) {
        //            items.Add(mi);
        //        }
        //        object parentObj = mi.GetVisualAncestor<MenuItem>(false);
        //        while (parentObj != null) {
        //            if (parentObj is MenuItem pmi) {
        //                items.Add(pmi);
        //                parentObj = pmi.GetVisualAncestor<MenuItem>(false);
        //            } else {
        //                parentObj = null;
        //            }
        //        }
        //    }
        //    return items;
        //}
        #endregion

        #endregion
    }

    public class MpAvContextMenuCloser : MpIContextMenuCloser {
        public void CloseMenu() {
            MpAvMenuExtension.CloseMenu();
        }
    }

    [DoNotNotify]
    class TestContextMenu : ContextMenu {
        public TestContextMenu() : base() {

        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
        }
    }
}
