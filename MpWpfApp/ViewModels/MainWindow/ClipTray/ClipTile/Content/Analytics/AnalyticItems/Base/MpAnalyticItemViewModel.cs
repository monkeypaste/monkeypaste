using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Net;
using System.Windows.Media;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        [MpChildViewModel(typeof(MpAnalyticItemParameterViewModel),true)]
        public ObservableCollection<MpAnalyticItemParameterViewModel> Parameters { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();
        
        public MpAnalyticItemParameterViewModel SelectedParameter => Parameters.FirstOrDefault(x => x.IsSelected);
        
        #endregion

        #region Appearance

        public string ItemIconSourcePath { get; protected set; }

        public Brush ItemBackgroundBrush => IsHovering ? Brushes.Yellow : Brushes.Transparent;
        #endregion

        #region State

        public bool WasExecuteClicked { get; set; } = false;

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        protected string UnformattedResponse { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

        #endregion

        #region Model


        public int RuntimeId { get; set; } = 0;

        public bool HasChildren { get; set; } = false;

        public string Title {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Title;
            }
            set {
                if (Title != value) {
                    Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Description;
            }
        }

        public int AnalyticItemId {
            get {
                if (AnalyticItem == null) {
                    return 0;
                }
                return AnalyticItem.Id;
            }
        }

        public MpAnalyticItem AnalyticItem { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemViewModel() : base(null) { }

        public MpAnalyticItemViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public virtual async Task Initialize() { await Task.Delay(1); }

        public async Task InitializeAsync(MpAnalyticItem ai) {
            IsBusy = true;
            await Task.Delay(1);
            AnalyticItem = ai;

            IsBusy = false;
        }

        public virtual async Task LoadChildren() {
            if (AnalyticItem == null || !HasChildren) {
                return;
            }
            IsBusy = true;

            Parameters = new ObservableCollection<MpAnalyticItemParameterViewModel>();
            foreach (var aip in AnalyticItem.Parameters.OrderBy(x => x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                Parameters.Add(naipvm);
            }

            if (Parameters.All(x => x.Parameter.ParameterType != MpAnalyticParameterType.Execute)) {
                MpAnalyticItemParameter eaip = new MpAnalyticItemParameter() {
                    Id = Parameters.Count + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = AnalyticItemId,
                    AnalyticItem = this.AnalyticItem,
                    ParameterType = MpAnalyticParameterType.Execute,
                    Key = "Execute",
                    ValueCsv = null,
                    SortOrderIdx = Parameters.Count
                };
                MpAnalyticItemParameterViewModel eaipvm = await CreateParameterViewModel(eaip);
                Parameters.Add(eaipvm);
            }

            if (Parameters.All(x => x.Parameter.ParameterType != MpAnalyticParameterType.Result)) {
                var raip = new MpAnalyticItemParameter() {
                    Id = Parameters.Count + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = AnalyticItemId,
                    AnalyticItem = this.AnalyticItem,
                    ParameterType = MpAnalyticParameterType.Result,
                    Key = "Result",
                    ValueCsv = string.Empty,
                    SortOrderIdx = Parameters.Count
                };
                var raipvm = await CreateParameterViewModel(raip);
                Parameters.Add(raipvm);
            }

            OnPropertyChanged(nameof(Parameters));

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aip.ParameterType) {
                case MpAnalyticParameterType.ComboBox:
                    naipvm = new MpComboBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Text:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Execute:
                    naipvm = new MpExecuteParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Result:
                    naipvm = new MpResultParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticParameterType), aip.ParameterType));
            }

            await naipvm.InitializeAsync(aip);

            return naipvm;
        }

        public MpAnalyticItemParameterViewModel GetParam(Enum paramId) {
            return Parameters.FirstOrDefault(x => x.Parameter.ParamEnumId.Equals(paramId));
        }

        public List<MpAnalyticItemParameterViewModel> GetParams(Enum paramId) {
            return Parameters.Where(x => x.Parameter.ParamEnumId.Equals(paramId)).ToList();
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasChildren):
                    if(HasChildren) {
                        Parameters.Add(null);
                    }
                    break;
                case nameof(IsExpanded):
                    if(IsExpanded) {
                        MpHelpers.Instance.RunOnMainThread(async () => {
                            await LoadChildren();
                        });
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ExecuteAnalysisCommand => new RelayCommand(
            async () => {
                await ExecuteAnalysis();
            },
            CanExecuteAnalysis);

        protected virtual async Task ExecuteAnalysis() {
            await Task.Delay(1);
            MpConsole.WriteLine("Base execute, no implementation");
        }

        protected virtual bool CanExecuteAnalysis() {
            return Parameters.All(x => x.IsValid);
        }
        #endregion
    }
}
