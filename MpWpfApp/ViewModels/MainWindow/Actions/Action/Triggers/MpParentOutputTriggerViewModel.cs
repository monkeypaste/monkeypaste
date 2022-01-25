namespace MpWpfApp {
    public class MpParentOutputTriggerViewModel : MpTriggerActionViewModelBase {
        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpParentOutputTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Public Methods

        public override void Enable() {
            if(ParentActionViewModel == null) {
                throw new System.Exception("Parent should be found in init");
            }
            ParentActionViewModel.RegisterTrigger(this);

            base.Enable(); 
        }

        public override void Disable() {
            if (ParentActionViewModel == null) {
                throw new System.Exception("Parent should be found in init");
            }
            ParentActionViewModel.UnregisterTrigger(this);

            base.Disable();
        }

        #endregion
    }
}
