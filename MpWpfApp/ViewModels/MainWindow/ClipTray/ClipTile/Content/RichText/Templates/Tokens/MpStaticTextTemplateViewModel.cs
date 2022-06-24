namespace MpWpfApp {
    public class MpStaticTextTemplateViewModel : MpTextTemplateViewModelBase {
        #region Constructors
        public MpStaticTextTemplateViewModel() : base(null) { }

        public MpStaticTextTemplateViewModel(MpTemplateCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override void FillAutoTemplate() {
            TemplateText = TemplateData;
            OnPropertyChanged(nameof(TemplateDisplayValue));
        }

        #endregion
    }
}
