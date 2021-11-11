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
        public ObservableCollection<MpAnalyticItemParameterViewModel> ParameterViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();
        
        public MpAnalyticItemParameterViewModel SelectedParameter => ParameterViewModels.FirstOrDefault(x => x.IsSelected);
        
        public MpResultParameterViewModel ResultViewModel => ParameterViewModels.FirstOrDefault(x=>x.Parameter.IsResult) as MpResultParameterViewModel;

        public MpExecuteParameterViewModel ExecuteViewModel => ParameterViewModels.FirstOrDefault(x => x.Parameter.IsExecute) as MpExecuteParameterViewModel;
        #endregion

        #region Appearance

        public string ItemIconSourcePath { get; protected set; }

        public Brush ItemBackgroundBrush => IsHovering ? Brushes.Yellow : Brushes.Transparent;
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        protected string UnformattedResponse { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

        #endregion

        #region Model

        public int RuntimeId { get; set; } = 0;

        //public bool HasChildren { get; set; } = false;

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

        public async Task InitializeDefaultsAsync(MpAnalyticItem ai) {
            IsBusy = true;
            AnalyticItem = ai;


            MpAnalyticItemParameter eaip = new MpAnalyticItemParameter() {
                ParameterType = MpAnalyticParameterType.Button,
                Label = "Execute",
                IsExecute = true
            };
            MpAnalyticItemParameterViewModel eaipvm = await CreateParameterViewModel(eaip);
            ParameterViewModels.Add(eaipvm);

            MpAnalyticItemParameter raip = new MpAnalyticItemParameter() {
                ParameterType = MpAnalyticParameterType.Text,
                Label = "",
                IsReadOnly = true,
                IsResult = true,
                ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                    new MpAnalyticItemParameterValue() {
                        IsDefault = true,
                        ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText
                    }
                }
            };
            MpAnalyticItemParameterViewModel raipvm = await CreateParameterViewModel(raip);
            ParameterViewModels.Add(raipvm);

            IsBusy = false;
        }

        public virtual async Task LoadChildren() {
            if (AnalyticItem == null) {
                return;
            }
            IsBusy = true;

            foreach (var aip in AnalyticItem.Parameters.OrderByDescending(x => x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                ParameterViewModels.Insert(0,naipvm);
            }

            //int exIdx = ParameterViewModels.IndexOf(GetExecuteParam());
            //ParameterViewModels.Move(exIdx, ParameterViewModels.Count - 2);

            //int rIdx = ParameterViewModels.IndexOf(GetResultParam());
            //ParameterViewModels.Move(exIdx, ParameterViewModels.Count - 1);

            OnPropertyChanged(nameof(ParameterViewModels));

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aip.ParameterType) {
                case MpAnalyticParameterType.ComboBox:
                    naipvm = new MpComboBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Text:
                    if (aip.IsResult) {
                        naipvm = new MpResultParameterViewModel(this);
                    } else {
                        naipvm = new MpTextBoxParameterViewModel(this);
                    }                    
                    break;
                case MpAnalyticParameterType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Button:
                    if (aip.IsExecute) {
                        naipvm = new MpExecuteParameterViewModel(this);
                    } else {
                        throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticParameterType), aip.ParameterType));
                    }
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticParameterType), aip.ParameterType));
                    break;
            }

            await naipvm.InitializeAsync(aip);

            return naipvm;
        }

        public MpAnalyticItemParameterViewModel GetParam(Enum paramId) {
            return ParameterViewModels.FirstOrDefault(x => x.Parameter.ParameterEnumId.Equals(paramId));
        }

        public List<MpAnalyticItemParameterViewModel> GetParams(Enum paramId) {
            return ParameterViewModels.Where(x => x.Parameter.ParameterEnumId.Equals(paramId)).ToList();
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
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
            return ParameterViewModels.All(x => x.IsValid);
        }
        #endregion
    }
}
