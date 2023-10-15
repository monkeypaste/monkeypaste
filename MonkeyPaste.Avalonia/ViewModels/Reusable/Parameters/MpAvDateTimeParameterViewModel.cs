using MonkeyPaste.Common.Plugin;
namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeParameterViewModel : MpAvParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region State
        public bool IsDatePicker =>
            UnitType == MpParameterValueUnitType.Date;
        public bool IsTimePicker =>
            UnitType == MpParameterValueUnitType.Time;
        #endregion

        #endregion

        #region Constructors

        public MpAvDateTimeParameterViewModel() : base(null) { }

        public MpAvDateTimeParameterViewModel(MpAvViewModelBase parent) : base(parent) { }


        #endregion
    }
}
