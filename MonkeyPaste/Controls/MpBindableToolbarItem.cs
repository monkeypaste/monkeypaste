using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpBindableToolbarItem : ToolbarItem {
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
            nameof(IsVisible), 
            typeof(bool), 
            typeof(MpBindableToolbarItem), 
            true, 
            BindingMode.TwoWay, 
            propertyChanged: OnIsVisibleChanged);

        public bool IsVisible {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public int ItemId { get; set; } = 0;

        private static void OnIsVisibleChanged(BindableObject bindable, object oldvalue, object newvalue) {
            var item = bindable as MpBindableToolbarItem;

            if (item == null || item.Parent == null)
                return;

            //var toolbarItems = ((ContentPage)item.Parent).ToolbarItems;

            
            Device.BeginInvokeOnMainThread(() => {
                bool hasItem = false;
                foreach (MpBindableToolbarItem tbi in ((ContentPage)item.Parent).ToolbarItems) {
                    if (tbi.ItemId == item.ItemId) {
                        hasItem = true;
                        break;
                    }
                }
                if ((bool)newvalue && !hasItem) {
                    ((ContentPage)item.Parent).ToolbarItems.Add(item);
                } else if (!(bool)newvalue && hasItem) {
                    ((ContentPage)item.Parent).ToolbarItems.Remove(item);
                }
            });
            
        }
    }
}
