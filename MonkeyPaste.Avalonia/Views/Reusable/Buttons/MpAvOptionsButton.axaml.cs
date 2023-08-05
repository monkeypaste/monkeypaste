using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvOptionsButton : MpAvUserControl<object> {
        #region Overrides

        #endregion

        #region Command Property

        private ICommand _Command = default;

        public static readonly DirectProperty<MpAvOptionsButton, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvOptionsButton, ICommand>
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

        public static readonly DirectProperty<MpAvOptionsButton, object> CommandParameterProperty =
            AvaloniaProperty.RegisterDirect<MpAvOptionsButton, object>
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

        #region Properties

        #endregion
        public MpAvOptionsButton() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
