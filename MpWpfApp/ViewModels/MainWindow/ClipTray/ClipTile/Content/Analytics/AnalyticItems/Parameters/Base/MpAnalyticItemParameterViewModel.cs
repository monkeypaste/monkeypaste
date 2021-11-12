using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;
using SQLite;
using Windows.Foundation.Collections;

namespace MpWpfApp {

    public abstract class MpAnalyticItemParameterViewModel : MpViewModelBase<MpAnalyticItemViewModel>{
        #region Private Variables
        
        #endregion

        #region Properties

        #region View Models

        public virtual ObservableCollection<MpAnalyticItemParameterValueViewModel> ValueViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();

        public virtual MpAnalyticItemParameterValueViewModel CurrentValueViewModel {
            get => ValueViewModels.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != CurrentValueViewModel) {
                    ValueViewModels.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                }
            }
        }

        #endregion

        #region Business Logic

        public Enum ParamEnumId { get; private set; }

        #endregion

        #region Appearance

        public Brush ParameterBorderBrush {
            get {
                if (IsValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }


        public string ParameterTooltipText {
            get {
                if (!IsValid) {
                    return $"{Parameter.Label} is required";
                }
                if (Parameter != null && !string.IsNullOrEmpty(Parameter.Description)) {
                    return Parameter.Description;
                }
                return null;
            }
        }

        #endregion

        #region State
        private bool _isInit = false;
        public bool IsInit {
            get {
                if (Parent != null && Parent.IsInit) {
                    return true;
                }
                return _isInit;
            }
            set {
                if (_isInit != value) {
                    _isInit = value;
                    OnPropertyChanged(nameof(IsInit));
                }
            }
        }

        public bool HasChanged { get; set; } = false;

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public abstract bool IsValid { get; }

        public virtual string UserValue { get; set; }
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

        public string Label {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(Parameter.Label)) {
                    return Parameter.Label;
                }
                return Parameter.Label;
            }
        }

        public string FormatInfo {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                return Parameter.FormatInfo;
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
            IsInit = true;
            IsBusy = true;

            Parameter = aip;

            ValueViewModels.Clear();

            foreach (var valueSeed in Parameter.ValueSeeds) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(ValueViewModels.Count, valueSeed);
                ValueViewModels.Add(naipvvm);
            }

            MpAnalyticItemParameterValueViewModel defVal = ValueViewModels.FirstOrDefault(x => x.IsDefault);
            if (defVal != null) {
                defVal.IsSelected = true;
            } else if (ValueViewModels.Count > 0) {
                ValueViewModels[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(ValueViewModels));

            IsBusy = false;
            IsInit = false;
        }

        public async Task<MpAnalyticItemParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(int idx, MpAnalyticItemParameterValue valueSeed) {
            var naipvvm = new MpAnalyticItemParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, valueSeed);
            return naipvvm;
        }

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasChanged):
                    Parent.OnPropertyChanged(nameof(Parent.HasAnyChanged));
                    break;
            }
        }

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var aipvvm = sender as MpAnalyticItemParameterValueViewModel;
            switch(e.PropertyName) {
                case nameof(aipvvm.IsSelected):
                    OnPropertyChanged(nameof(CurrentValueViewModel));
                    if(!IsInit) {
                        HasChanged = true;
                    }
                    break;
            }
        }

        #endregion
    }
}
