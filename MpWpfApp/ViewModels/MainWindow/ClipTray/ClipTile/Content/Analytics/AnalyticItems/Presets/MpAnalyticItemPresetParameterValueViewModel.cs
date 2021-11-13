using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemPresetParameterValueViewModel : MpViewModelBase<MpAnalyticItemViewModel>, ICloneable {
        #region Properties

        #region View Models

        #endregion

        #region State
        #endregion

        #region Model 

        public MpAnalyticItemPresetParameterValue PresetParameterValue { get; protected set; }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemPresetParameterValueViewModel() : base (null) { }

        public MpAnalyticItemPresetParameterValueViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPresetParameterValue aippv) {
            IsBusy = true;

            PresetParameterValue = aippv;

            await Task.Delay(1);

            IsBusy = false;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetParameterValueViewModel(Parent);
            caipvm.PresetParameterValue = PresetParameterValue.Clone() as MpAnalyticItemPresetParameterValue;
            return caipvm;
        }

        #endregion

        #region Private Methods

        private void MpPresetParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
            } 
        }

        #endregion
    }
}
