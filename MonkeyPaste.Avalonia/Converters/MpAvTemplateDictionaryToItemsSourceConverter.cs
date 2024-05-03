using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace MonkeyPaste.Avalonia {
    public class MpAvTemplateDictionaryToItemsSourceConverter : Dictionary<string, IDataTemplate>, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not IEnumerable<MpAvIMenuItemViewModel> mivml) {
                return null;
            }
            List<Control> mil = GetMenuItems(mivml);
            return mil;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
        private Control GetMenuItem(MpAvIMenuItemViewModel mivm) {
            string key = mivm.MenuItemType.ToString();
            if (mivm is MpAvMenuItemViewModel mivm_obj &&
                mivm_obj.IsSeparator) {
                key = "Separator";
            }
            if (!this.TryGetValue(key, out var result) ||
                result.Build(mivm) is not Control c) {
                return null;
            }
            c.DataContext = mivm;
            if (c is not MenuItem mi ||
                mivm.MenuItemType == MpMenuItemType.ColorPalette) {
                return c;
            }
            mi.ItemsSource = GetMenuItems(mivm.SubItems);
            InitChildMenuFix(mi);
            return mi;
        }
        private List<Control> GetMenuItems(IEnumerable<MpAvIMenuItemViewModel> mivml) {
            if (mivml == null) {
                return null;
            }
            List<Control> mil = new List<Control>();
            foreach (var mivm in mivml) {
                if (!mivm.IsVisible) {
                    continue;
                }
                if (mivm.HasLeadingSeparator &&
                    mil.Any() &&
                    this["Separator"].Build(mivm) is Control sep) {
                    mil.Add(sep);
                } else if (mivm is MpAvMenuItemViewModel mivm_obj &&
                            mivm_obj.IsSeparator) {
                    if (mil.Any() &&
                       this["Separator"].Build(mivm) is Control sep2) {
                        mil.Add(sep2);
                    }
                    continue;
                }
                mil.Add(GetMenuItem(mivm));
            }
            return mil;
        }


        #region Child Menu Fix
        private void InitChildMenuFix(MenuItem mi) {
            mi.SubmenuOpened += Mi_SubmenuOpened;
            mi.Unloaded += Mi_Unloaded;
        }

        private void Mi_PointerEntered(object sender, global::Avalonia.Input.PointerEventArgs e) {
            throw new NotImplementedException();
        }

        private void Mi_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not MenuItem mi) {
                return;
            }
            mi.SubmenuOpened -= Mi_SubmenuOpened;
            mi.Unloaded -= Mi_Unloaded;
        }

        private void Mi_SubmenuOpened(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (e.Source is not MenuItem mi) {
                return;
            }
            if (mi.Items.FirstOrDefault() is Control child_mi) {
                child_mi.Tag = mi;
                child_mi.EffectiveViewportChanged += Child_mi_EffectiveViewportChanged;
                child_mi.DetachedFromVisualTree += Child_mi_DetachedFromVisualTree;
            }
        }

        private void Child_mi_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            if (sender is not MenuItem child_mi ||
                child_mi.Tag is not MenuItem mi ||
                MpAvWindowManager.GetTopLevel(mi) is not PopupRoot pr ||
                MpAvWindowManager.GetTopLevel(child_mi) is not PopupRoot child_pr) {
                return;
            }
            var parent_tl = pr.PointToScreen(pr.Bounds.TopLeft);
            var child_tr = child_pr.PointToScreen(child_pr.Bounds.TopRight);
            double x_diff = parent_tl.X - child_tr.X;
            if (x_diff > 0 && mi.GetVisualDescendant<Popup>() is { } pu) {
                pu.HorizontalOffset = x_diff + 4;
            }
        }
        private void Child_mi_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not MenuItem child_mi) {
                return;
            }

            child_mi.EffectiveViewportChanged -= Child_mi_EffectiveViewportChanged;
            child_mi.DetachedFromVisualTree -= Child_mi_DetachedFromVisualTree;
        }
        #endregion

    }
}
