using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutDataGridView : MpAvUserControl<object> {
        #region ShowRouting Direct Avalonia Property

        private bool _ShowRouting = true;

        public static readonly DirectProperty<MpAvShortcutDataGridView, bool> ShowRoutingProperty =
            AvaloniaProperty.RegisterDirect<MpAvShortcutDataGridView, bool>
            (
                nameof(ShowRouting),
                o => o.ShowRouting,
                (o, v) => o.ShowRouting = v
            );

        public bool ShowRouting {
            get => _ShowRouting;
            set {
                SetAndRaise(ShowRoutingProperty, ref _ShowRouting, value);
            }
        }

        #endregion
        public MpAvShortcutDataGridView() {
            InitializeComponent();
        }

        private void Dg_Sorting(object sender, DataGridColumnEventArgs e) {
            MpAvShortcutCollectionViewModel.Instance.RefreshFilters();
        }

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (this.GetVisualDescendant<DataGrid>() is not DataGrid dg) {
                return;
            }
            ShowRouting = !this.Classes.Contains("internal");
        }
    }
}
