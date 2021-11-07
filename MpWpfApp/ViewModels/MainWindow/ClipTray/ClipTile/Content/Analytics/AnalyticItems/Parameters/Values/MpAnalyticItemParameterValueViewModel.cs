using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemParameterValueViewModel : MpViewModelBase<MpAnalyticItemParameterViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Model

        public int ValueIdx { get; set; } = 0;

        public string Value {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.Parameter.UserValue;
            }
            set {
                if (Parent != null && Parent.Parameter.UserValue != value) {
                    Parent.Parameter.UserValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public double DoubleValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
                    return 0;
                }
                try {
                    return Convert.ToDouble(Value);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return 0;
                }
            }
        }

        public int IntValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
                    return 0;
                }
                try {
                    return Convert.ToInt32(Value);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return 0;
                }
            }
        }

        public bool BoolValue {
            get {
                if (string.IsNullOrWhiteSpace(Value)) {
                    return false;
                }
                if (Value != "0" && Value != "1") {
                    throw new Exception("Cannot convert value " + Value + " to boolean");
                }
                return Value == "1";
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterValueViewModel() : base(null) { }

        public MpAnalyticItemParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, string value) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            Value = value;

            IsBusy = false;
        }

        public override string ToString() {
            return Value;
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

            }
            (Parent.Parent.ExecuteAnalysisCommand as RelayCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
