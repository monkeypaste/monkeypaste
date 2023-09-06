using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

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
            if (!this.TryGetValue(mivm.MenuItemType.ToString(), out var result) ||
                result.Build(mivm) is not Control c) {
                return null;
            }
            c.DataContext = mivm;
            if (c is not MenuItem mi) {
                return c;
            }
            mi.ItemsSource = GetMenuItems(mivm.SubItems);
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
                    this["Separator"].Build(mivm) is Control sep) {
                    mil.Add(sep);
                }
                mil.Add(GetMenuItem(mivm));
            }
            return mil;
        }

    }
}
