using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xamarin.Forms;

namespace MpWpfApp {
    public class MpOneWayToggleButton : ToggleButton {
        protected override void OnClick() {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
    }
}
