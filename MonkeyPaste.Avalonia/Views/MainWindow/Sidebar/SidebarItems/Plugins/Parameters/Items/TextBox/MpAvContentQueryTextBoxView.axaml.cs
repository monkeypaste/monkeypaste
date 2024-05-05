using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvContentQueryTextBoxView : MpAvUserControl<MpIContentQueryTextBoxViewModel> {

        #region IsPopOut Property

        private bool _IsPopOut = false;

        public static readonly DirectProperty<MpAvContentQueryTextBoxView, bool> IsPopOutProperty =
            AvaloniaProperty.RegisterDirect<MpAvContentQueryTextBoxView, bool>
            (
                nameof(IsPopOut),
                o => o.IsPopOut,
                (o, v) => o.IsPopOut = v,
                false
            );

        public bool IsPopOut {
            get => _IsPopOut;
            set {
                SetAndRaise(IsPopOutProperty, ref _IsPopOut, value);
            }
        }

        #endregion

        public MpAvContentQueryTextBoxView() {
            InitializeComponent();
        }
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (MpAvWindowManager.GetTopLevel(this) is not { } tl) {
                return;
            }
            IsPopOut = tl.Classes.Contains("popout");
            //if (!IsPopOut) {
            //    return;
            //}
            //string[] hidden_items = new[] {
            //    "ReadOnlyBlock",
            //    "ClearButton",
            //    "FilterButton",
            //    "PopOutTextBoxButton",
            //};
            //hidden_items.ForEach(x => this.FindControl<Control>(x).IsVisible = false);
        }
    }
}
