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
            if (_cmInstance == null) {
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

        #region HideOnClick AvaloniaProperty
        public static bool GetHideOnClick(AvaloniaObject obj) {
            return obj.GetValue(HideOnClickProperty);
        }

        public static void SetHideOnClick(AvaloniaObject obj, bool value) {
            obj.SetValue(HideOnClickProperty, value);
        }

        public static readonly AttachedProperty<bool> HideOnClickProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "HideOnClick",
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

        #region PlacementMode AvaloniaProperty
        public static PlacementMode GetPlacementMode(AvaloniaObject obj) {
            return obj.GetValue(PlacementModeProperty);
        }

        public static void SetPlacementMode(AvaloniaObject obj, PlacementMode value) {
            obj.SetValue(PlacementModeProperty, value);
        }

        public static readonly AttachedProperty<PlacementMode> PlacementModeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, PlacementMode>(
                "PlacementMode",
                PlacementMode.Pointer);

        #endregion

        #region CanShowMenu AvaloniaProperty
        public static bool GetCanShowMenu(AvaloniaObject obj) {
            return obj.GetValue(CanShowMenuProperty);
        }

        public static void SetCanShowMenu(AvaloniaObject obj, bool value) {
            obj.SetValue(CanShowMenuProperty, value);
        }

        public static readonly AttachedProperty<bool> CanShowMenuProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "CanShowMenu",
                true,
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

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (_cmInstance == null) {
                    _cmInstance = new MpAvContextMenuView();
                }
                if (element is Control control) {
                    if (control.IsInitialized) {
                        HostControl_AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += HostControl_AttachedToVisualHandler;

                    }
                }
            } else {
                HostControl_DetachedToVisualHandler(element, null);
            }


        }

        private static void HostControl_AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                if (e == null) {
                    control.AttachedToVisualTree += HostControl_AttachedToVisualHandler;
                }
                control.DetachedFromVisualTree += HostControl_DetachedToVisualHandler;
                //mi.ContextMenu.ContextMenuOpening += ContextMenu_ContextMenuOpening;
                control.AddHandler(Control.PointerPressedEvent, HostControl_PointerPressed, RoutingStrategies.Tunnel);
            }
        }
        private static void HostControl_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control host_control) {
                host_control.AttachedToVisualTree -= HostControl_AttachedToVisualHandler;
                host_control.DetachedFromVisualTree -= HostControl_DetachedToVisualHandler;
                host_control.RemoveHandler(Control.PointerPressedEvent, HostControl_PointerPressed);
            }
        }

        private static async void HostControl_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var control = sender as Control;
            if (control == null || 
                !GetIsEnabled(control) || 
                !GetCanShowMenu(control)) {
                return;
            }
            e.Handled = false;

            bool wait_for_selection = false;
            if (control.DataContext is MpISelectorItemViewModel sivm) {
                if (e.IsLeftPress(control) || GetSelectOnRightClick(control)) {
                    if (sivm.Selector.SelectedItem != control.DataContext) {
                        wait_for_selection = true;
                    }
                    sivm.Selector.SelectedItem = control.DataContext;
                }
            } else if (control.DataContext is MpISelectableViewModel svm) {
                if (e.IsLeftPress(control) || GetSelectOnRightClick(control)) {
                    if (!svm.IsSelected) {
                        wait_for_selection = true;
                    }
                    svm.IsSelected = true;
                }
            }
            if (wait_for_selection) {
                await Task.Delay(500);
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

            if (mivm == null || mivm.SubItems == null) {
                e.Handled = GetSuppressDefaultRightClick(control) && e.IsRightPress(control);
                
                //SetIsOpen(control, false);
                return;
            }

            //SetIsOpen(control, true);
            e.Handled = true;

            CancelEventHandler onOpenHandler = null;
            CancelEventHandler onCloseHandler = null;

            onCloseHandler = (s, e1) => {
                if (control.DataContext is MpIContextMenuViewModel cmvm) {
                    cmvm.IsContextMenuOpen = false;
                }
                if (control.DataContext is MpIPopupMenuViewModel pumvm) {
                    pumvm.IsPopupMenuOpen = false;
                }
                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                _cmInstance.ContextMenuClosing -= onCloseHandler;
                _cmInstance.ContextMenuOpening -= onOpenHandler;
                control.ContextMenu = null;
                openSubMenuItems.Clear();
            };

            onOpenHandler = (s, e1) => {
                e1.Cancel = false;
                MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                if (control.DataContext is MpIContextMenuViewModel cmvm) {
                    cmvm.IsContextMenuOpen = true;
                }
                if (control.DataContext is MpIPopupMenuViewModel pumvm) {
                    pumvm.IsPopupMenuOpen = true;
                }
            };

            _cmInstance.Items = mivm.SubItems.Where(x => x.IsVisible).Select(x => CreateMenuItem(x));

            _cmInstance.PlacementTarget = control;
            _cmInstance.PlacementMode = GetPlacementMode(control);
            if (_cmInstance.PlacementMode == PlacementMode.Pointer) {
                _cmInstance.PlacementAnchor = PopupAnchor.AllMask;
                _cmInstance.PlacementMode = PlacementMode.Pointer;
                _cmInstance.DataContext = mivm;

                var ctrl_mp = e.GetPosition(control);
                _cmInstance.HorizontalOffset = ctrl_mp.X;
                _cmInstance.VerticalOffset = ctrl_mp.Y;
            }

            _cmInstance.ContextMenuOpening += onOpenHandler;
            _cmInstance.ContextMenuClosing += onCloseHandler;

            var w = control.GetVisualAncestor<Window>();
            if (w == null) {
                Debugger.Break();
            }
            _cmInstance.Open(w);
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
        }

        private static void MenuItem_PointerReleased(object sender, PointerReleasedEventArgs e) {
            MenuItem mi = null;
            if (e.Source is IVisual sourceVisual &&
                sourceVisual.GetVisualAncestor<MenuItem>() is MenuItem smi) {
                //MpConsole.WriteLine("Released (source): " + mi);
                mi = smi;
            } else if (sender is MenuItem sender_mi) {
                mi = sender_mi;
                //MpConsole.WriteLine("Released (sender): " + mi);
            }
            if (mi == null) {
                return;
            }
            MpMenuItemViewModel mivm = mi.DataContext as MpMenuItemViewModel;
            if (mivm == null) {
                return;
            }


            bool hide_on_click = GetHideOnClick(_cmInstance.PlacementTarget);
            if (mivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                
                if(hide_on_click) {
                    mivm.Command.Execute(mivm.CommandParameter);
                } else {
                    MenuItem_PointerReleased2(mi, e);
                }
                e.Handled = true;
            } else {
                e.Handled = false;
                // NOTE hide_on_click only matters for checkbox templates
                hide_on_click = true;
            }
            
            if(hide_on_click) {
                MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
            }
            
        }

        private static void MenuItem_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is MenuItem control) {
                control.PointerEnterItem -= MenuItem_PointerEnter;
                control.PointerReleased -= MenuItem_PointerReleased;
                control.RemoveHandler(Control.PointerReleasedEvent, MenuItem_PointerReleased);
            }
        }

        private static void MenuItem_PointerEnter(object sender, PointerEventArgs e) {
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

        #region Helpers

        private static Control CreateMenuItem(MpMenuItemViewModel mivm) {
            Control control = null;
            string itemType = new MpAvMenuItemDataTemplateSelector().GetTemplateName(mivm);
            KeyGesture inputGesture = null;
            if (!string.IsNullOrEmpty(mivm.InputGestureText)) {
                inputGesture = KeyGesture.Parse(mivm.InputGestureText);
            }

            switch (itemType) {
                case MpMenuItemViewModel.DEFAULT_TEMPLATE_NAME:
                case MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME:
                    var mi = new MenuItem() {
                        Header = mivm.Header,
                        Command = mivm.Command,
                        CommandParameter = mivm.CommandParameter,
                        IsEnabled = mivm.IsEnabled,
                        InputGesture = inputGesture,
                        DataContext = mivm,
                        Icon = CreateIcon(mivm),
                        Items = mivm.SubItems == null ? null : mivm.SubItems.Select(x=>CreateMenuItem(x))
                    };
                    mi.PointerEnterItem += MenuItem_PointerEnter;
                    mi.DetachedFromVisualTree += MenuItem_DetachedFromVisualTree;
                    if (mi.Command != null && mivm.IsEnabled) {
                        mi.AddHandler(Control.PointerReleasedEvent, MenuItem_PointerReleased, RoutingStrategies.Tunnel);
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

                    control.PointerEnter += MenuItem_PointerEnter;
                    control.DetachedFromVisualTree += MenuItem_DetachedFromVisualTree;
                    break;
            }
            return control;
        }

        private static object CreateIcon(MpMenuItemViewModel mivm) {
            if(mivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                return CreateCheckableIcon(mivm);
            }
            if(mivm.IconSourceObj == null) {
                return null;
            }
            return new Image() {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(mivm.IconSourceObj, null, null, null) as Bitmap,
                    };
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
                Width = mivm.CheckResourceKey == "CheckSvg" ? 15 : 7,
                Height = mivm.CheckResourceKey == "CheckSvg" ? 15 : 7,
                Data = MpPlatformWrapper.Services.PlatformResource.GetResource(mivm.CheckResourceKey) as StreamGeometry,
                Foreground = mivm.IconHexStr.HexColorToContrastingFgHexColor().ToAvBrush()
            };
            iconBorder.Child = pi;
            return iconBorder;
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
        #endregion

        #endregion

        #endregion

        // unused

        private static void MenuItem_PointerReleased2(object sender, PointerReleasedEventArgs e) {
            // unused can't consistently get checks to change w/o reloading...i think its from right click selecting 
            MenuItem mi = null;
            if (e.Source is IVisual sourceVisual &&
                sourceVisual.GetVisualAncestor<MenuItem>() is MenuItem smi) {
                //MpConsole.WriteLine("Released (source): " + mi);
                mi = smi;
            } else if (sender is MenuItem sender_mi) {
                mi = sender_mi;
                //MpConsole.WriteLine("Released (sender): " + mi);
            }
            if (mi == null) {
                return;
            }
            MpMenuItemViewModel mivm = mi.DataContext as MpMenuItemViewModel;
            if (mivm == null) {
                return;
            }

            if (mivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                //ToggleCheckableMenuItem(mi, e);
                mivm.Command.Execute(mivm.CommandParameter);
                e.Handled = true;
            }
            // update toggled cb
            mivm.IsChecked = mivm.IsChecked.DefaultToggleValue(true);
            var self_ancestor_mil = GetParentMenuItems(mi, true)
                        .Where(x => x.DataContext is MpMenuItemViewModel pmivm && pmivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME);

            foreach (var cur_mi in self_ancestor_mil) {
                var cur_mi_mivm = cur_mi.DataContext as MpMenuItemViewModel;
                bool? is_cur_mi_checked = false;
                if (cur_mi_mivm.IsChecked.IsTrue()) {
                    is_cur_mi_checked = true;
                } else if(cur_mi_mivm.SubItems != null && cur_mi_mivm.SubItems.Count > 0){
                    is_cur_mi_checked = cur_mi_mivm.SubItems.Any(x => x.IsChecked.IsTrueOrNull()) ? null : false;
                }
                cur_mi_mivm.IsChecked = is_cur_mi_checked;
                cur_mi.Icon = null;
                cur_mi.Icon = CreateCheckableIcon(cur_mi_mivm);
            }

            //EventHandler<PointerEventArgs> move_handler = null;
            //move_handler = (s, e1) => {
            //    _cmInstance.RemoveHandler(InputElement.PointerLeaveEvent, move_handler);
            //    CloseMenu();
            //};
            //_cmInstance.AddHandler(InputElement.PointerLeaveEvent, move_handler, RoutingStrategies.Tunnel, true);
        }
    }

    public class MpAvContextMenuCloser : MpIContextMenuCloser {
        public void CloseMenu() {
            MpAvMenuExtension.CloseMenu();
        }
    }}
