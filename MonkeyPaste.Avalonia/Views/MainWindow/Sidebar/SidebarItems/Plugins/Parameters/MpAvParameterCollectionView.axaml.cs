using Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvParameterCollectionView : MpAvUserControl<MpAvIParameterCollectionViewModel> {
        #region IsSaveCancelHidden Property
        // NOTE hidden in transaction
        public bool IsSaveCancelHidden {
            get { return GetValue(IsSaveCancelHiddenProperty); }
            set { SetValue(IsSaveCancelHiddenProperty, value); }
        }

        public static readonly StyledProperty<bool> IsSaveCancelHiddenProperty =
            AvaloniaProperty.Register<MpAvParameterCollectionView, bool>(
                name: nameof(IsSaveCancelHidden),
                defaultValue: false);

        #endregion 

        public MpAvParameterCollectionView() {
            InitializeComponent();
        }
    }
}
