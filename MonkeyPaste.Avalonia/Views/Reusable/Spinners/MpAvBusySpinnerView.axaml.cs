using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvBusySpinnerView : UserControl {
        #region Private Variables

        #endregion
        public MpAvBusySpinnerView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
