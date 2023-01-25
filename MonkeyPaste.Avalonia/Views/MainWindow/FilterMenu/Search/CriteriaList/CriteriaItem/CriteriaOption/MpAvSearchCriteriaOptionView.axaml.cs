
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpCriteriaItemOptionView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaOptionView : MpAvUserControl<MpAvSearchCriteriaOptionViewModel> {
        public MpAvSearchCriteriaOptionView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
