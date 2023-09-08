using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClearTextButton : MpAvUserControl<object> {

        #region Command Property

        private ICommand _ClearCommand = default;

        public static readonly DirectProperty<MpAvClearTextButton, ICommand> ClearCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvClearTextButton, ICommand>
            (
                nameof(ClearCommand),
                o => o.ClearCommand,
                (o, v) => o.ClearCommand = v
            );

        public ICommand ClearCommand {
            get => _ClearCommand;
            set {
                SetAndRaise(ClearCommandProperty, ref _ClearCommand, value);
            }
        }

        #endregion

        #region CommandParameter Property

        private object _ClearCommandParameter = default;

        public static readonly DirectProperty<MpAvClearTextButton, object> ClearCommandParameterProperty =
            AvaloniaProperty.RegisterDirect<MpAvClearTextButton, object>
            (
                nameof(ClearCommandParameter),
                o => o.ClearCommandParameter,
                (o, v) => o.ClearCommandParameter = v
            );

        public object ClearCommandParameter {
            get => _ClearCommandParameter;
            set {
                SetAndRaise(ClearCommandParameterProperty, ref _ClearCommandParameter, value);
            }
        }

        #endregion 

        public MpAvClearTextButton() {
            InitializeComponent();

            var cb = this.FindControl<Button>("ClearButton");
            cb.PointerPressed += Cb_PointerPressed;
            cb.AddHandler(Button.PointerPressedEvent, Cb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void Cb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (ClearCommand == null) {
                // clear is overriden ignore click
                return;
            }
            TextBox tb = null;
            var cur_parent = this.Parent as Control;
            // find nearest relative textbox to this control
            while (true) {
                if (cur_parent == null) {
                    break;
                }
                if (cur_parent.TryGetVisualDescendant<TextBox>(out var child_tb)) {
                    tb = child_tb;
                    break;
                }
                cur_parent = cur_parent.Parent as Control;
            }
            if (tb == null) {
                return;
            }
            // clear text w/o breaking binding
            tb.SetCurrentValue(TextBox.TextProperty, string.Empty);
        }

    }
}
