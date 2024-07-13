using Avalonia;
using System.Collections.ObjectModel;

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
        public ObservableCollection<MpAvPluginParameterItemView> ParamViews { get; set; } = [];

        public MpAvParameterCollectionView() {
            InitializeComponent();
        }

        public void ParameterItemView_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is not MpAvPluginParameterItemView piv ||
                ParamViews.Contains(piv)) {
                return;
            }
            ParamViews.Add(piv);
        }
        public void ParameterItemView_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is not MpAvPluginParameterItemView piv) {
                return;
            }
            ParamViews.Remove(piv);
        }
    }
}
