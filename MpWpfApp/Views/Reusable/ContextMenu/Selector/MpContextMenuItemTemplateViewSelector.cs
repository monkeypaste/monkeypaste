﻿using System;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContextMenuItemTemplateViewSelector : DataTemplateSelector {
        public DataTemplate MenuItemTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }
        public DataTemplate ColorPalleteTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item == null) {
                return null;
            }

            var cmivm = item as MpContextMenuItemViewModel;
            if(cmivm.IsSeparator) {
                return SeparatorTemplate;
            }
            return MenuItemTemplate;
        }
    }
}