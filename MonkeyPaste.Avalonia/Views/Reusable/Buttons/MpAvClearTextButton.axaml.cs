using Avalonia;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClearTextButton : MpAvUserControl {

        #region Command Property

        private ICommand _Command = default;

        public static readonly DirectProperty<MpAvClearTextButton, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvClearTextButton, ICommand>
            (
                nameof(Command),
                o => o.Command,
                (o, v) => o.Command = v
            );

        public ICommand Command {
            get => _Command;
            set {
                SetAndRaise(CommandProperty, ref _Command, value);
            }
        }

        #endregion

        #region CommandParameter Property

        private object _CommandParameter = default;

        public static readonly DirectProperty<MpAvClearTextButton, object> CommandParameterProperty =
            AvaloniaProperty.RegisterDirect<MpAvClearTextButton, object>
            (
                nameof(CommandParameter),
                o => o.CommandParameter,
                (o, v) => o.CommandParameter = v
            );

        public object CommandParameter {
            get => _CommandParameter;
            set {
                SetAndRaise(CommandParameterProperty, ref _CommandParameter, value);
            }
        }

        #endregion 

        public MpAvClearTextButton() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
