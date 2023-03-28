using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTextBoxParameterView : MpAvUserControl<MpAvTextBoxParameterViewModel> {
        public MpAvTextBoxParameterView() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
