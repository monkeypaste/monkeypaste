using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using PropertyChanged;
using System.Diagnostics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;
using Avalonia.Input;
using Avalonia.Styling;
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

        public MpAvClearTextButton() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
