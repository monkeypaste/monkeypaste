using Avalonia;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPluginParameterListBoxView : MpAvUserControl<MpAvIParameterCollectionViewModel> {
        #region CanShowSaveOrCancel Property

        private bool _CanShowSaveOrCancel = default;

        public static readonly DirectProperty<MpAvPluginParameterListBoxView, bool> CanShowSaveOrCancelProperty =
            AvaloniaProperty.RegisterDirect<MpAvPluginParameterListBoxView, bool>
            (
                nameof(CanShowSaveOrCancel),
                o => o.CanShowSaveOrCancel,
                (o, v) => o.CanShowSaveOrCancel = v
            );

        public bool CanShowSaveOrCancel {
            get => _CanShowSaveOrCancel;
            set {
                SetAndRaise(CanShowSaveOrCancelProperty, ref _CanShowSaveOrCancel, value);
            }
        }

        #endregion 

        public MpAvPluginParameterListBoxView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
