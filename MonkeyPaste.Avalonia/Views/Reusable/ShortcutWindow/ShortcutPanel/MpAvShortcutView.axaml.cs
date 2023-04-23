using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutView : MpAvUserControl<MpAvIKeyGestureViewModel> {

        #region EmptyText Property

        private string _EmptyText = null;

        public static readonly DirectProperty<MpAvShortcutView, string> EmptyTextProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, string>
            (
                nameof(EmptyText),
                o => o.EmptyText,
                (o, v) => o.EmptyText = v
            );

        public string EmptyText {
            get => _EmptyText;
            set {
                SetAndRaise(EmptyTextProperty, ref _EmptyText, value);
            }
        }

        #endregion

        #region RecordCommandParameter Property

        private object _RecordCommandParameter = null;

        public static readonly DirectProperty<MpAvShortcutView, object> RecordCommandParameterProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, object>
            (
                nameof(RecordCommandParameter),
                o => o.RecordCommandParameter,
                (o, v) => o.RecordCommandParameter = v
            );

        public object RecordCommandParameter {
            get => _RecordCommandParameter;
            set {
                SetAndRaise(RecordCommandParameterProperty, ref _RecordCommandParameter, value);
            }
        }

        #endregion 

        #region RecordCommand Property

        private ICommand _RecordCommand = new MpCommand(() => { }, () => false);

        public static readonly DirectProperty<MpAvShortcutView, ICommand> RecordCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutView, ICommand>
            (
                nameof(RecordCommand),
                o => o.RecordCommand,
                (o, v) => o.RecordCommand = v
            );

        public ICommand RecordCommand {
            get => _RecordCommand;
            set {
                SetAndRaise(RecordCommandProperty, ref _RecordCommand, value);
            }
        }

        #endregion 

        public bool CanRecord =>
            RecordCommand != null && RecordCommand.CanExecute(RecordCommandParameter);
        public MpAvShortcutView() {
            AvaloniaXamlLoader.Load(this);
            this.PointerEntered += MpAvShortcutView_PointerEntered;
            this.PointerExited += MpAvShortcutView_PointerExited;

            var rb = this.FindControl<Control>("RecordButton");
            rb.AddHandler(PointerReleasedEvent, Rb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void Rb_PointerPressed(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (!CanRecord) {
                return;
            }
            RecordCommand.Execute(RecordCommandParameter);
        }

        private void MpAvShortcutView_PointerExited(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (!CanRecord) {
                return;
            }
            var sclb = this.FindControl<Control>("ShortcutListBox");
            var rb = this.FindControl<Control>("RecordButton");
            sclb.IsVisible = true;
            rb.IsVisible = false;
        }

        private void MpAvShortcutView_PointerEntered(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (!CanRecord) {
                return;
            }
            var sclb = this.FindControl<Control>("ShortcutListBox");
            var rb = this.FindControl<Control>("RecordButton");
            sclb.IsVisible = false;
            rb.IsVisible = true;
        }
    }
}
