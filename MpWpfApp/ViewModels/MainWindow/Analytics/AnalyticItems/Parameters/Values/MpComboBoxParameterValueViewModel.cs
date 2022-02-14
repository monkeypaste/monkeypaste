using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpComboBoxParameterValueViewModel : MpViewModelBase<MpAnalyticItemParameterViewModel>  {
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

        #endregion

        #region Model

        public bool IsDefault {
            get {
                if(AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isDefault;
            }
        }

        public bool IsMaximum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isMaximum;
            }
        }

        public bool IsMinimum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isMinimum;
            }
        }

        public string Label {
            get {
                if (AnalyticItemParameterValue == null) {
                    return string.Empty;
                }
                return AnalyticItemParameterValue.label;
            }
        }

        public string Value {
            get {
                if (AnalyticItemParameterValue == null) {
                    return null;
                }
                return AnalyticItemParameterValue.value;
            }
            set {
                if (Value != value) {
                    //HasModelChanged = AnalyticItemParameterValue.value != value;
                    HasModelChanged = true;
                    AnalyticItemParameterValue.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public MpAnalyticItemParameterValue AnalyticItemParameterValue { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpComboBoxParameterValueViewModel() : base(null) { }

        public MpComboBoxParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
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

        public bool Equals(MpComboBoxParameterValueViewModel other) {
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

            MpComboBoxParameterValueViewModel personObj = obj as MpComboBoxParameterValueViewModel;
            if (personObj == null)
                return false;
            else
                return Equals(personObj);
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(MpComboBoxParameterValueViewModel person1, MpComboBoxParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(MpComboBoxParameterValueViewModel person1, MpComboBoxParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.HasAnyParameterValueChanged));
                    break;
            }
            Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAllValid));
            //(Parent.Parent.ExecuteAnalysisCommand as RelayCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
