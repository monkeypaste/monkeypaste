using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonkeyPaste;
using System.Windows.Input;
using System;
using System.Windows.Controls.Primitives;

namespace MpWpfApp {

    public class MpPopupMenuExtension : DependencyObject {
        private static MpContextMenuView _contextMenuView;

        #region IsLeftButtonEnabled DependencyProperty

        public static bool GetIsLeftButtonEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsLeftButtonEnabledProperty);
        }

        public static void SetIsLeftButtonEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsLeftButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsLeftButtonEnabledProperty =
            DependencyProperty.Register(
                "IsLeftButtonEnabled",
                typeof(bool),
                typeof(MpPopupMenuExtension),
                new PropertyMetadata(false, OnOnlyHexadecimalChanged));
                //new FrameworkPropertyMetadata {
                //    PropertyChangedCallback = (dpo, e) => {
                //        if (e.NewValue is bool isEnabled) {
                //            if (isEnabled) {
                //                EnablePopup(dpo, true, false);
                //            } else {
                //                DisablePopup(dpo, true, false);
                //            }
                //        }
                //    }
                //});
        static void OnOnlyHexadecimalChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabled) {
                if (isEnabled) {
                    EnablePopup(dpo, true, false);
                } else {
                    DisablePopup(dpo, true, false);
                }
            }
        }
        #endregion

        #region IsRightButtonEnabled DependencyProperty

        public static bool GetIsRightButtonEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsRightButtonEnabledProperty);
        }

        public static void SetIsRightButtonEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsRightButtonEnabledProperty, value);
        }

        public static readonly DependencyProperty IsRightButtonEnabledProperty =
            DependencyProperty.Register(
                "IsRightButtonEnabled", typeof(bool),
                typeof(MpPopupMenuExtension),
                new FrameworkPropertyMetadata {
                    PropertyChangedCallback = (dpo, e) => {
                        if (e.NewValue != null && e.NewValue is bool isEnabled) {
                            if(isEnabled) {
                                EnablePopup(dpo, false, true);
                            } else {
                                DisablePopup(dpo, false, true);
                            }
                        }
                    }
                });

        #endregion

        #region IsSelectionEnabled DependencyProperty

        public static bool GetIsSelectionEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsSelectionEnabledProperty);
        }

        public static void SetIsSelectionEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsSelectionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSelectionEnabledProperty =
            DependencyProperty.Register(
                "IsSelectionEnabled", typeof(bool),
                typeof(MpPopupMenuExtension),
                new FrameworkPropertyMetadata {
                    PropertyChangedCallback = (dpo, e) => {
                        if (dpo == null || !dpo.GetType().IsSubclassOf(typeof(MpISelectableViewModel))) {
                            throw new Exception("Error Popup requires MpISelectableViewModel in data context");
                        }
                    }
                });

        #endregion

        #region Placement DependencyProperty

        public static PlacementMode GetPlacement(DependencyObject obj) {
            return (PlacementMode)obj.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject obj, PlacementMode value) {
            obj.SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(
                "Placement", typeof(PlacementMode),
                typeof(MpPopupMenuExtension),
                new PropertyMetadata(PlacementMode.MousePoint));

        #endregion

        private static void EnablePopup(DependencyObject dpo, bool isLeftButton, bool isRightButton) {
            if (dpo == null || !dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                return;
            }
            var fe = dpo as FrameworkElement;
            if (fe == null || fe.DataContext == null) {
                return;
            }
            if(!fe.DataContext.GetType().IsSubclassOf(typeof(MpIMenuItemViewModel))) {
                throw new Exception("Error Popup requires MpIMenuItemInterface in data context");
            }
            if(isLeftButton) {
                fe.MouseLeftButtonDown += Fe_ShowPopup;
            } else if(isRightButton) {
                fe.PreviewMouseRightButtonDown += Fe_ShowPopup;
            }
        }

        private static void DisablePopup(DependencyObject dpo, bool isLeftButton, bool isRightButton) {
            if (dpo == null || !dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                return;
            }
            var fe = dpo as FrameworkElement;
            if (fe == null || !fe.GetType().IsSubclassOf(typeof(FrameworkElement)) || fe.DataContext == null) {
                return;
            }
            if (!fe.DataContext.GetType().IsSubclassOf(typeof(MpIMenuItemViewModel))) {
                throw new Exception("Error Popup requires MpIMenuItemInterface in data context");
            }
            if (isLeftButton) {
                fe.MouseLeftButtonDown -= Fe_ShowPopup;
            } else if (isRightButton) {
                fe.PreviewMouseRightButtonDown -= Fe_ShowPopup;
            }
        }

        private static void Fe_ShowPopup(object sender, MouseButtonEventArgs e) {
            var fe = sender as FrameworkElement;
            if(GetIsSelectionEnabled(fe)) {
                (fe as MpISelectableViewModel).IsSelected = true;
            }
            var mivm = fe.DataContext as MpIMenuItemViewModel;
            if(_contextMenuView == null) {
                _contextMenuView = new MpContextMenuView();
            }
            _contextMenuView.DataContext = mivm;

            e.Handled = true;
            fe.ContextMenu = _contextMenuView;
            fe.ContextMenu.PlacementTarget = fe;
            fe.ContextMenu.Placement = GetPlacement(fe);
            fe.ContextMenu.IsOpen = true;
        }
    }
}