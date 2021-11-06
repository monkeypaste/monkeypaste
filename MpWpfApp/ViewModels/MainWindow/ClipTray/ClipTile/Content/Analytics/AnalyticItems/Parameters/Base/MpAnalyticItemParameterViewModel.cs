using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public abstract class MpAnalyticItemParameterViewModel : MpViewModelBase<MpAnalyticItemViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region Appearance

        public Brush ParameterBorderBrush {
            get {
                return IsValid ? Brushes.Transparent : Brushes.Red;
            }
        }
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public abstract bool IsValid { get; }

        public string ParameterTooltipText {
            get {
                if(!IsValid) {
                    return $"{Parameter.Key} is required";
                }
                return string.Empty;
            }
        }
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


        public MpAnalyticItemParameter Parameter { get; set; }

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
