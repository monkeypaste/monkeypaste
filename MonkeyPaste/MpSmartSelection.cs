using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public static class MpSmartSelection {

        public static readonly BindableProperty HasSmartSelectionProperty =
                    BindableProperty.CreateAttached(
                        "HasSmartSelection", 
                        typeof(bool), 
                        typeof(MpSmartSelection), 
                        false,
                        BindingMode.TwoWay,
                        null,
                        (s,o,n)=> {
                            return;
                        });

        public static bool GetHasSmartSelection(BindableObject view) {
            return (bool)view.GetValue(HasSmartSelectionProperty);
        }

        public static void SetHasSmartSelection(BindableObject view, bool value) {
            view.SetValue(HasSmartSelectionProperty, value);
        }
    }
}
