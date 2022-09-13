namespace MonkeyPaste.Avalonia {
    public class MpStaticTextTemplateViewModel : MpAvTextTemplateViewModelBase {
        #region Constructors
        public MpStaticTextTemplateViewModel() : base(null) { }

        public MpStaticTextTemplateViewModel(MpAvTemplateCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override void FillAutoTemplate() {
            TemplateText = TemplateData;
            OnPropertyChanged(nameof(TemplateDisplayValue));
        }

        #endregion
    }
}
