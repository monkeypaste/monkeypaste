using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpOneWayCheckBox : CheckBox {        
        protected override void OnClick() {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
    }
}
