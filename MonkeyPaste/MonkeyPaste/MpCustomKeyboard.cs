using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCustomKeyboard : Entry {
        public static readonly BindableProperty EnterCommandProperty = BindableProperty.Create(
            nameof(EnterCommand),
            typeof(ICommand),
            typeof(MpCustomKeyboard),
            default(ICommand),
            BindingMode.OneWay
        );

        public ICommand EnterCommand {
            get => (ICommand)GetValue(EnterCommandProperty);
            set => SetValue(EnterCommandProperty, value);
        }
    }
}
