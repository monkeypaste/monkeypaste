using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KeyGesture = Avalonia.Input.KeyGesture;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMenuExtension {
        //private static IBrush _hoverMenuItemBrush;
        //private static IBrush _hoverMenuItemPresenterBrush;

        private static bool _isCloseHandled = false;
        private static MpAvContextMenuView _cmInstance =>
            MpAvContextMenuView.Instance;

        private static List<MenuItem> openSubMenuItems = new List<MenuItem>();

        public static void CloseMenu() {
            if (_cmInstance == null) {
                return;
            }
            _cmInstance.Close();
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

        #region SuppressDefaultLeftClick AvaloniaProperty
        public static bool GetSuppressDefaultLeftClick(AvaloniaObject obj) {
            return obj.GetValue(SuppressDefaultLeftClickProperty);
        }

        public static void SetSuppressDefaultLeftClick(AvaloniaObject obj, bool value) {
            obj.SetValue(SuppressDefaultLeftClickProperty, value);
        }

        public static readonly AttachedProperty<bool> SuppressDefaultLeftClickProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SuppressDefaultLeftClick",
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

        #region SuppressMenuLeftClick AvaloniaProperty
        public static bool GetSuppressMenuLeftClick(AvaloniaObject obj) {
            return obj.GetValue(SuppressMenuLeftClickProperty);
        }

        public static void SetSuppressMenuLeftClick(AvaloniaObject obj, bool value) {
            obj.SetValue(SuppressMenuLeftClickProperty, value);
        }

        public static readonly AttachedProperty<bool> SuppressMenuLeftClickProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "SuppressMenuLeftClick",
                false);

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

        #region ForcedMenuItemViewModel AvaloniaProperty
        public static MpMenuItemViewModel GetForcedMenuItemViewModel(AvaloniaObject obj) {
            return obj.GetValue(ForcedMenuItemViewModelProperty);
        }

        public static void SetForcedMenuItemViewModel(AvaloniaObject obj, MpMenuItemViewModel value) {
            obj.SetValue(ForcedMenuItemViewModelProperty, value);
        }

        public static readonly AttachedProperty<MpMenuItemViewModel> ForcedMenuItemViewModelProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpMenuItemViewModel>(
                "ForcedMenuItemViewModel",
                null);

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
            if (e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    _cmInstance.Opened += _cmInstance_MenuOpened;
                    _cmInstance.Closing += _cmInstance_Closing;
                    _cmInstance.Closed += _cmInstance_MenuOpened;
                    if (element is Control control) {
                        control.AttachedToVisualTree += HostControl_AttachedToVisualHandler;
                        if (control.IsInitialized) {
                            HostControl_AttachedToVisualHandler(control, null);
                        }
                    }
                } else {
                    HostControl_DetachedToVisualHandler(element, null);
                }
            }
        }

        private static void _cmInstance_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = _isCloseHandled;
            _isCloseHandled = false;
        }

        private static void _cmInstance_MenuOpened(object sender, RoutedEventArgs e) {
            OnIsOpenChanged();
        }

        private static void OnIsOpenChanged() {
            var control = _cmInstance.PlacementTarget;
            if (control == null) {
                MpDebug.Break();
            }
            object dc = GetControlDataContext(control);
            bool is_open = _cmInstance.IsOpen;
            if (dc is MpIContextMenuViewModel cmvm) {
                cmvm.IsContextMenuOpen = is_open;
            }
            if (dc is MpIPopupMenuViewModel pumvm) {
                pumvm.IsPopupMenuOpen = is_open;
            }
            SetIsOpen(control, is_open);
            // MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = is_open;
            //if (is_open) {
            //    openSubMenuItems.AddRange(_cmInstance.ItemContainerGenerator.Containers.Where(x=>x.ContainerControl is MenuItem).Select(x=>x.ContainerControl).Cast<MenuItem>());
            //}else 
            if (!is_open) {
                control.ContextMenu = null;
                openSubMenuItems.Clear();
            }

        }

        private static void HostControl_AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                control.DetachedFromVisualTree += HostControl_DetachedToVisualHandler;
                control.AddHandler(Control.PointerPressedEvent, HostControl_PointerPressed, RoutingStrategies.Tunnel);
                //control.AddHandler(Control.HoldingEvent, HostControl_Holding, RoutingStrategies.Tunnel);
            }
        }


        private static void HostControl_DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control host_control) {
                host_control.AttachedToVisualTree -= HostControl_AttachedToVisualHandler;
                host_control.DetachedFromVisualTree -= HostControl_DetachedToVisualHandler;
                host_control.RemoveHandler(Control.PointerPressedEvent, HostControl_PointerPressed);
                //host_control.RemoveHandler(Control.HoldingEvent, HostControl_Holding);
            }
        }

        private static void HostControl_Holding(object sender, HoldingRoutedEventArgs e) {
            if (Mp.Services.PlatformInfo.IsDesktop) {
                return;
            }
            var control = sender as Control;
            object dc = GetControlDataContext(control);
            MpMenuItemViewModel mivm = null;
            if (dc is MpIContextMenuViewModel cmvm) {
                mivm = cmvm.ContextMenuViewModel;
            }
            if (GetForcedMenuItemViewModel(control) is MpMenuItemViewModel fmivm) {
                // used when host vm has multiple context menus
                mivm = fmivm;
            }
            if (mivm == null || mivm.SubItems == null) {
                //e.Handled = GetSuppressDefaultRightClick(control) && e.IsRightPress(control);
                e.Handled = false;
                return;
            }

            e.Handled = true;
            // CREATE & SHOW MENU

            ShowMenu(control, mivm, e.Position.ToPortablePoint(), GetPlacementMode(control));
        }
        private static async void HostControl_PointerPressed(object sender, PointerPressedEventArgs e) {
            var control = sender as Control;

            // VALIDATE CAN SHOW

            if (control == null ||
                !GetIsEnabled(control) ||
                !GetCanShowMenu(control)) {
                return;
            }
            e.Handled = false;

            object dc = GetControlDataContext(control);
            // HANDLE SELECTION

            bool wait_for_selection = false;
            bool can_select =
                e.IsLeftPress(control) ||
                (e.IsRightPress(control) && GetSelectOnRightClick(control));
            if (can_select &&
                dc is MpIConditionalSelectableViewModel csvm && !csvm.CanSelect) {
                can_select = false;
            }

            if (can_select) {
                if (dc is MpISelectableViewModel svm) {
                    if (!svm.IsSelected) {
                        wait_for_selection = true;
                    }
                    svm.IsSelected = true;
                }
            }
            if (wait_for_selection) {
                await Task.Delay(500);
            }

            // LOCATE MENU

            MpMenuItemViewModel mivm = null;

            if (e.IsLeftPress(control) && !GetSuppressMenuLeftClick(control)) {
                if (e.ClickCount == 2 && GetDoubleClickCommand(control) != null) {
                    GetDoubleClickCommand(control).Execute(null);
                } else if (dc is MpIPopupMenuViewModel pumvm) {
                    mivm = pumvm.PopupMenuViewModel;
                }
            } else if (e.IsRightPress(control)) {
                if (dc is MpIContextMenuViewModel cmvm) {
                    mivm = cmvm.ContextMenuViewModel;
                }
            }

            if (GetForcedMenuItemViewModel(control) is MpMenuItemViewModel fmivm) {
                // used when host vm has multiple context menus
                mivm = fmivm;
            }

            if (mivm == null || mivm.SubItems == null) {
                e.Handled = GetSuppressDefaultRightClick(control) &&
                    e.IsRightPress(control);
                return;
            }

            e.Handled = true;
            // CREATE & SHOW MENU

            ShowMenu(control, mivm, e.GetPosition(control).ToPortablePoint(), GetPlacementMode(control));
        }


        private static void MenuItem_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is MenuItem control) {
                control.PointerEntered -= MenuItem_PointerEnter;
                control.PointerReleased -= MenuItem_PointerReleased;
                control.RemoveHandler(Control.PointerReleasedEvent, MenuItem_PointerReleased);
            }
        }

        private static void MenuItem_PointerReleased(object sender, PointerReleasedEventArgs e) {
            MenuItem mi = null;
            if (e.Source is Visual sourceVisual &&
                sourceVisual.GetVisualAncestor<MenuItem>() is MenuItem smi) {
                mi = smi;
            } else if (sender is MenuItem sender_mi) {
                mi = sender_mi;
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

                mivm.Command?.Execute(mivm.CommandParameter);
                e.Handled = true;
                if (!hide_on_click) {
                    //mivm.IsChecked = mivm.IsChecked.DefaultToggleValue(true);
                    //var self_ancestor_mil = GetParentMenuItems(mi, true)
                    //            .Where(x => x.DataContext is MpMenuItemViewModel pmivm && pmivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME);

                    //foreach (var cur_mi in self_ancestor_mil) {
                    //    var cur_mi_mivm = cur_mi.DataContext as MpMenuItemViewModel;
                    //    bool? is_cur_mi_checked = false;
                    //    if (cur_mi_mivm.IsChecked.IsTrue()) {
                    //        is_cur_mi_checked = true;
                    //    } else if (cur_mi_mivm.SubItems != null && cur_mi_mivm.SubItems.Count > 0) {
                    //        is_cur_mi_checked = cur_mi_mivm.SubItems.Any(x => x.IsChecked.IsTrueOrNull()) ? null : false;
                    //    }
                    //    cur_mi_mivm.IsChecked = is_cur_mi_checked;
                    //    cur_mi.Icon = null;
                    //    cur_mi.Icon = CreateCheckableIcon(cur_mi_mivm);
                    //}
                    _isCloseHandled = true;
                }
            } else {
                e.Handled = false;
                // NOTE hide_on_click only matters for checkbox templates
                hide_on_click = true;
            }

            if (hide_on_click) {
                Mp.Services.ContextMenuCloser.CloseMenu();
            }

        }

        private static void MenuItem_PointerEnter(object sender, PointerEventArgs e) {
            if (e.Source is MenuItem mi && mi.DataContext is MpMenuItemViewModel mivm) {
                var _hoverMenuItemBrush = Mp.Services.PlatformResource.GetResource("MenuFlyoutItemBackgroundPointerOver") as IBrush;
                var _hoverMenuItemPresenterBrush = Mp.Services.PlatformResource.GetResource("MenuFlyoutPresenterBackground") as IBrush;

                var openMenusToRemove = new List<MenuItem>();
                foreach (var osmi in openSubMenuItems) {
                    // loop through open sub-menu items and close ones not in entered item tree
                    var cmil = GetChildMenuItems(osmi);
                    var pmil = GetParentMenuItems(mi);
                    if (cmil.Select(x => x.DataContext).Cast<MpMenuItemViewModel>().All(x => x != mivm)) {
                        osmi.Close();
                        osmi.Background = Brushes.Transparent;
                        var ccl = osmi.GetVisualDescendants<Control>();
                        //var child_border = ccl.FirstOrDefault(x => x is Border b && (b.Background.ToString() == "#19000000" || b.Background.ToString() == Brushes.LightBlue.ToString()) && b.Tag == null);
                        var child_border = ccl.FirstOrDefault(x => x is Border b && b.Background.Equals(_hoverMenuItemBrush) && b.Tag == null);
                        if (child_border is Border cb) {
                            cb.Background = Brushes.Transparent;
                        }
                        //mi.GetVisualAncestor<Panel>().Background = "#FFF2F2F2".ToAvBrush();
                        mi.GetVisualAncestor<Panel>().Background = _hoverMenuItemPresenterBrush;
                        openMenusToRemove.Add(osmi);
                    }
                }
                foreach (var mitr in openMenusToRemove) {
                    openSubMenuItems.Remove(mitr);
                }

                if (mivm.SubItems != null && mivm.SubItems.Count > 0 && !mivm.IsColorPallete) {
                    // show hover mi subitems

                    mi.InvalidateVisual();
                    mi.IsSubMenuOpen = true;
                    mi.Background = _hoverMenuItemBrush;
                    var ccl = mi.GetVisualDescendants<Control>();
                    var child_border = ccl.FirstOrDefault(x => x is Border b && b.Tag == null);

                    if (child_border is Border cb) {
                        //cb.Background = Brushes.LightBlue;
                        cb.Background = _hoverMenuItemBrush;
                    }
                    openSubMenuItems.Add(mi);
                    mi.Open();

                }
            }
        }

        private static void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e) {
            // NOTE when child menu opens to left of parent (on windows at least)
            // child doesn't overlap parent and pointer leave is triggered and 
            // in a lot of cases the menu can't be entered so this nudges right to 
            // keep this hack friendly
            if (sender is not MenuItem parent_mi) {
                return;
            }
            if (parent_mi.Items == null || parent_mi.Items.FirstOrDefault() is not Visual first_child_v) {
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                var parent_tl = TopLevel.GetTopLevel(parent_mi);
                var first_child_tl = TopLevel.GetTopLevel(first_child_v);
                while (first_child_tl == null) {
                    await Task.Delay(100);
                    first_child_tl = TopLevel.GetTopLevel(first_child_v);
                }
                double x_diff = parent_tl.PointToScreen(parent_tl.Bounds.TopLeft).X - first_child_v.PointToScreen(first_child_v.Bounds.TopRight).X;
                if (x_diff < 0) {
                    return;
                }
                // needs nudge
                //var new_offset =
                //    //first_child_v.PointToScreen(first_child_v.Bounds.TopLeft).ToPortablePoint(first_child_v.VisualPixelDensity()) + 
                //    new MpPoint(x_diff + 5 - (first_child_v.Bounds.Width * first_child_v.VisualPixelDensity()), 0);
                if (first_child_tl is PopupRoot pr) {
                    //pr.ConfigurePosition(
                    //    parent_tl,
                    //    _cmInstance.Placement,
                    //    new_offset.ToAvPoint());

                    //pr.RenderTransform = new TranslateTransform(x_diff + 5, 0);
                    pr.Width += x_diff + 5;
                    pr.InvalidateVisual();
                }
            });
        }
        #region Helpers

        private static Control CreateMenuItem(MpMenuItemViewModel mivm) {
            Control control = null;
            string itemType = new MpAvMenuItemDataTemplateSelector().GetTemplateName(mivm);
            KeyGesture inputGesture = null;
            if (!string.IsNullOrEmpty(mivm.InputGestureText) &&
                !mivm.InputGestureText.Contains(MpInputConstants.SEQUENCE_SEPARATOR)) {
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
                        ItemsSource = mivm.SubItems == null ? null : mivm.SubItems.Where(x => x != null && x.IsVisible).Select(x => CreateMenuItem(x))
                    };
                    mi.Classes.Add("gestureLabel");

                    if (mivm.Tooltip != null) {
                        // TODO tooltip screws up pointer enter or something, it works but doesn't highlight so its 
                        // some hit test in the tooltip tree thats enabled is my guess
                        //ToolTip.SetTip(mi, new MpAvToolTipView() { ToolTipText = mivm.Tooltip.ToString() });
                    }

                    mi.PointerEntered += MenuItem_PointerEnter;
                    mi.DetachedFromVisualTree += MenuItem_DetachedFromVisualTree;
                    mi.SubmenuOpened += MenuItem_SubmenuOpened;
                    if ((mi.Command != null && mivm.IsEnabled) || !GetHideOnClick(_cmInstance.PlacementTarget)) {
                        mi.AddHandler(Control.PointerReleasedEvent, MenuItem_PointerReleased, RoutingStrategies.Tunnel);
                    }
                    if (itemType == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                        ApplyAnyBindings(mi, mivm);
                    }
                    control = mi;
                    break;
                case MpMenuItemViewModel.SEPERATOR_TEMPLATE_NAME:
                    //control = new MenuItem() {
                    //    Icon = mivm.Header,
                    //    Header = "-",
                    //    DataContext = mivm
                    //};
                    if (string.IsNullOrEmpty(mivm.Header)) {
                        control = new Separator() { DataContext = mivm };
                    } else {
                        control = new MenuItem() {
                            Icon = new TextBlock() { Text = mivm.Header },
                            Header = new Border() { Background = Brushes.Black, Height = 5, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch, MinWidth = 100 },
                            DataContext = mivm
                        };
                    }
                    break;
                case MpMenuItemViewModel.COLOR_PALETTE_TEMPLATE_NAME:
                    control = new MenuItem() {
                        Header = new MpAvColorPaletteListBoxView() {
                            DataContext = mivm
                        },
                        DataContext = mivm
                    };
                    control.Classes.Add("colorPalette");
                    control.PointerEntered += MenuItem_PointerEnter;
                    control.DetachedFromVisualTree += MenuItem_DetachedFromVisualTree;
                    break;
            }
            return control;
        }


        public static object CreateIcon(MpMenuItemViewModel mivm) {
            if (mivm.ContentTemplateName == MpMenuItemViewModel.CHECKABLE_TEMPLATE_NAME) {
                return CreateCheckableIcon(mivm);
            }
            if (mivm.IconSourceObj == null) {
                return null;
            }
            var iconImg = new Image() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            if (string.IsNullOrEmpty(mivm.IconTintHexStr)) {
                //
                if (MpPrefViewModel.Instance.ThemeType != MpThemeType.Dark ||
                    MpAvThemeViewModel.Instance.IsColoredImageResource(mivm.IconSourceObj)) {
                    iconImg.Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(mivm.IconSourceObj, null, null, null) as Bitmap;
                } else {
                    // for
                    iconImg.Source = MpAvStringHexToBitmapTintConverter.Instance.Convert(mivm.IconSourceObj, null, Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeInteractiveColor.ToString()), null) as Bitmap;
                }

            } else {
                iconImg.Source = MpAvStringHexToBitmapTintConverter.Instance.Convert(mivm.IconSourceObj, null, mivm.IconTintHexStr, null) as Bitmap;
            }
            var iconBorder = GetIconBorder(mivm);
            if (mivm.IsChecked.IsFalse()) {
                iconBorder.BorderBrush = Brushes.Transparent;
            }
            iconBorder.Child = iconImg;
            return iconBorder;
        }
        private static object CreateCheckableIcon(MpMenuItemViewModel mivm) {
            var pi = new PathIcon() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = mivm.CheckResourceKey == "CheckSvg" ? 15 : 7,
                Height = mivm.CheckResourceKey == "CheckSvg" ? 15 : 7,
                //Margin = mivm.CheckResourceKey == "CheckSvg" ? new Thickness(10) : new Thickness(15),
                Data = Mp.Services.PlatformResource.GetResource(mivm.CheckResourceKey) as StreamGeometry,
                Foreground = mivm.IconHexStr.ToContrastForegoundColor().ToAvBrush()
            };
            var iconBorder = GetIconBorder(mivm);
            iconBorder.Child = pi;
            return iconBorder;
        }
        private static Border GetIconBorder(MpMenuItemViewModel mivm) {
            var ib = new Border() {
                //HorizontalAlignment = HorizontalAlignment.Center,
                //VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = mivm.IconMinWidth,
                MinHeight = mivm.IconMinHeight,
                BorderThickness = new Thickness(mivm.IconBorderThickness),
                BorderBrush = mivm.IconBorderHexColor.ToAvBrush(),
                CornerRadius = new CornerRadius(mivm.IconCornerRadius),
                Margin = mivm.IconMargin.ToAvThickness(),
                Background = mivm.IconHexStr.ToAvBrush()
            };
            if (mivm.IconShape.ToAvShape() is Shape icon_shape) {
                //Stroke="DarkBlue" StrokeThickness="1" Fill="Violet" Canvas.Left="150" Canvas.Top="31"/>
                icon_shape.Stroke = ib.BorderBrush;
                icon_shape.StrokeThickness = mivm.IconBorderThickness;
                icon_shape.Fill = ib.Background;

                ib.BorderBrush = Brushes.Transparent;
                ib.BorderThickness = new Thickness(0);
                ib.Background = Brushes.Transparent;
                ib.Child = icon_shape;
            }

            return ib;
        }

        private static IEnumerable<MenuItem> GetChildMenuItems(MenuItem mi) {
            var items = new List<MenuItem>();
            if (mi != null) {
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

        private static void ApplyAnyBindings(MenuItem mi, MpMenuItemViewModel mivm) {
            if (mivm.IconSrcBindingObj != null &&
                mi.Icon is Border icon_border) {
                // NOTE this only handles unique case for search filter check box
                //var icon_border =
                //    icon.GetVisualDescendants<Border>()
                //    .FirstOrDefault(x => x.Tag != null && x.Tag.ToString() == "IconBorder");
                if (icon_border == null) {
                    MpDebug.Break();
                } else {
                    icon_border.Background = null;
                    icon_border.Bind(
                        Border.BackgroundProperty,
                        new Binding() {
                            Source = mivm.IconSrcBindingObj,
                            Path = mivm.IconPropPath,
                            Converter = MpAvStringHexToBrushConverter.Instance
                        });

                    if (icon_border.Child is PathIcon pi) {
                        pi.Data = null;
                        pi.Bind(
                            PathIcon.DataProperty,
                            new Binding() {
                                Source = mivm.CheckedResourceSrcObj,
                                Path = mivm.CheckedResourcePropPath,
                                Converter = MpAvIconSourceObjToBitmapConverter.Instance
                            });
                    } else {
                        MpDebug.Break();
                    }
                }
            }
            if (mivm.CommandSrcObj != null) {
                //mi.RemoveHandler(Control.PointerReleasedEvent, MenuItem_PointerReleased);
                mi.Command = null;
                mi.Bind(
                    MenuItem.CommandProperty,
                    new Binding() {
                        Source = mivm.CommandSrcObj,
                        Path = mivm.CommandPath,
                    });
            }
        }
        #endregion

        #endregion

        #endregion

        public static void ShowMenu(
            Control control,
            MpMenuItemViewModel mivm,
            MpPoint offset = null,
            PlacementMode placement = PlacementMode.Pointer,
            bool hideOnClick = true,
            bool selectOnRightClick = false,
            PopupAnchor anchor = PopupAnchor.None) {
            _cmInstance.Close();
            if (mivm == null) {
                return;
            }


            _cmInstance.DataContext = mivm;
            _cmInstance.PlacementTarget = control;
            _cmInstance.Placement = placement;
            _cmInstance.PlacementAnchor = anchor;
            _cmInstance.HorizontalOffset = 0;
            _cmInstance.VerticalOffset = 0;

            SetHideOnClick(control, hideOnClick);
            SetSelectOnRightClick(control, selectOnRightClick);

            if (_cmInstance.Placement == PlacementMode.Pointer) {
                if (offset == null) {
                    var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
                    offset = control.PointToClient(gmp.ToAvPixelPoint(control.VisualPixelDensity())).ToPortablePoint();
                }
            }
            if (offset != null) {
                _cmInstance.HorizontalOffset = offset.X;
                _cmInstance.VerticalOffset = offset.Y;
            } else {
                _cmInstance.HorizontalOffset = 0;
                _cmInstance.VerticalOffset = 0;
            }
            mivm = AddLeadingSeperators(mivm);
            if (mivm.SubItems == null || mivm.SubItems.Count == 0) {
                _cmInstance.ItemsSource = new[] { mivm };
            } else {
                // add leading seperators where necessary

                _cmInstance.ItemsSource = mivm.SubItems.Where(x => x != null && x.IsVisible).Select(x => CreateMenuItem(x));
            }

            try {
                _cmInstance.Open(control);
            }
            catch (Exception ex) {
                // BUG intermittently get dc exception saying can't convert anchor dc to mi dc
                // i think its a timing thing when right click is selecting item during cmd, 
                // just click again i guess
                MpConsole.WriteTraceLine($"Open menu exception: ", ex);

            }
        }

        #region Helpers

        private static object GetControlDataContext(Control control) {
            if (GetForcedMenuItemViewModel(control) is MpMenuItemViewModel mivm &&
                mivm.ParentObj is object forceDc) {
                return forceDc;
            }
            if (control == null) {
                return null;
            }
            return control.DataContext;
        }

        private static MpMenuItemViewModel AddLeadingSeperators(MpMenuItemViewModel mivm) {
            if (mivm.SubItems == null || mivm.SubItems.Count == 0) {
                return mivm;
            }
            // select direct child wanting leading sep where there is items before it and actual previous isn't seperator
            var leading_sep_items =
                mivm.SubItems
                .Where((x, idx1) =>
                    x.IsVisible &&
                    x.HasLeadingSeperator &&
                    mivm.SubItems
                    .Where((y, idx2) => idx2 < idx1 && y.IsVisible)
                    .OrderByDescending(y => mivm.SubItems.IndexOf(y))
                    .FirstOrDefault() is MpMenuItemViewModel actual_prev_mivm &&
                    !actual_prev_mivm.IsSeparator)
                .ToList();

            foreach (var lsi in leading_sep_items) {
                mivm.SubItems.Insert(mivm.SubItems.IndexOf(lsi), new MpMenuItemViewModel() { IsSeparator = true });
            }
            return mivm;
        }

        #endregion

    }

    public class MpAvContextMenuCloser : MpIContextMenuCloser {
        public void CloseMenu() {
            MpAvMenuExtension.CloseMenu();
        }
    }
}
