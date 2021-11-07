using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public abstract class MpAnalyticItemParameterViewModel : MpViewModelBase<MpAnalyticItemViewModel>{
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public virtual MpAnalyticItemParameterValueViewModel SelectedValue { get; set; } = new MpAnalyticItemParameterValueViewModel();

        #endregion

        #region Appearance

        public Brush ParameterBorderBrush {
            get {
                if (IsValid) {
                    return Brushes.Transparent;
                }
                if (Parent.WasExecuteClicked) {
                    return Brushes.Red;
                }
                return Brushes.Transparent;
            }
        }


        public string ParameterTooltipText {
            get {
                if (!IsValid) {
                    return $"{Parameter.Key} is required";
                }
                if (Parameter != null && !string.IsNullOrEmpty(Parameter.Description)) {
                    return Parameter.Description;
                }
                return null;
            }
        }

        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public abstract bool IsValid { get; }

        #endregion

        #region Model
        
        public bool IsRequired {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.IsParameterRequired;
           }
        }
        public string Key {
            get {
                if(Parameter == null) {
                    return string.Empty;
                }
                return Parameter.Key;
            }
        }

        public virtual string UserValue {
            get {
                if (Parameter == null) {
                    return null;
                }
                return Parameter.UserValue;
            }
            set {
                if (Parameter.UserValue != value) {
                    Parameter.UserValue = value;
                    OnPropertyChanged(nameof(UserValue));
                }
            }
        }
        
        public MpAnalyticItemParameter Parameter { get; protected set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterViewModel() : base(null) { }

        public MpAnalyticItemParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;

            await Task.Delay(1);

            Parameter = aip;

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(int idx, string value) {
            var naipvvm = new MpAnalyticItemParameterValueViewModel(this);
            await naipvvm.InitializeAsync(idx, value);
            return naipvvm;
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

            }
        }
        #endregion
    }
}
