using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvColorPaletteListBoxView : MpAvUserControl<MpMenuItemViewModel> {
        //Type IStyleable.StyleKey => typeof(MenuItem);
        public MpAvColorPaletteListBoxView() {
            InitializeComponent();
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
