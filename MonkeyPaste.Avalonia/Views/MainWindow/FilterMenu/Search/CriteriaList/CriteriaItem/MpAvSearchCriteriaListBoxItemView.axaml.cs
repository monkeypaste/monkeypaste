using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticToolbarTreeView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaItemView : MpAvUserControl<MpAvSearchCriteriaItemViewModel> {
        public MpAvSearchCriteriaItemView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
