using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpAnalyticItemParameterValueViewModel : MpViewModelBase<MpAnalyticItemParameterViewModel>  {
        #region Private Variables

        #endregion

        #region Properties

        #region Appearance

        public Brush BackgroundBrush {
            get {
                if(IsSelected) {
                    return Brushes.Blue;
                }
                return Brushes.Transparent;
            }
        }

        public Brush BorderBrush {
            get {
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public int ValueIdx { get; set; } = 0;

        public bool HasChanged { get; set; } = false;

        #endregion

        #region Model

        public bool IsDefault {
            get {
                if(AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsDefault;
            }
        }

        public bool IsMaximum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsMaximum;
            }
        }

        public bool IsMinimum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.IsMinimum;
            }
        }

        public string Label {
            get {
                if (AnalyticItemParameterValue == null) {
                    return string.Empty;
                }
                return AnalyticItemParameterValue.Label;
            }
        }

        public string Value {
            get {
                if (AnalyticItemParameterValue == null) {
                    return null;
                }
                return AnalyticItemParameterValue.Value;
            }
            set {
                if (Value != value) {
                    HasChanged = AnalyticItemParameterValue.Value != value;
                    AnalyticItemParameterValue.Value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public MpAnalyticItemParameterValue AnalyticItemParameterValue { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterValueViewModel() : base(null) { }

        public MpAnalyticItemParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, MpAnalyticItemParameterValue valueSeed) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            AnalyticItemParameterValue = valueSeed;

            IsBusy = false;
        }

        public override string ToString() {
            return Value;
        }

        #region Equals Override

        public bool Equals(MpAnalyticItemParameterValueViewModel other) {
            if (other == null)
                return false;

            if (this.Value == other.Value)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj) {
            if (obj == null)
                return false;

            MpAnalyticItemParameterValueViewModel personObj = obj as MpAnalyticItemParameterValueViewModel;
            if (personObj == null)
                return false;
            else
                return Equals(personObj);
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(MpAnalyticItemParameterValueViewModel person1, MpAnalyticItemParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(MpAnalyticItemParameterValueViewModel person1, MpAnalyticItemParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Value):
                    break;
            }
            Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAllValid));
            //(Parent.Parent.ExecuteAnalysisCommand as RelayCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
