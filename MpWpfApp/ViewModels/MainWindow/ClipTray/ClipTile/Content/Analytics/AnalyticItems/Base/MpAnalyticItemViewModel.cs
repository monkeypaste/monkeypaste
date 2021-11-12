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
using System.Web.UI;

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemPresetViewModel> PresetViewModels { get; set; } = new ObservableCollection<MpAnalyticItemPresetViewModel>();

        public MpAnalyticItemPresetViewModel SelectedPreseetViewModel => PresetViewModels.FirstOrDefault(x => x.IsSelected);

        public ObservableCollection<MpAnalyticItemParameterViewModel> ParameterViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();
        
        public MpAnalyticItemParameterViewModel SelectedParameter => ParameterViewModels.FirstOrDefault(x => x.IsSelected);
        
        public MpResultParameterViewModel ResultViewModel => ParameterViewModels.FirstOrDefault(x=>x.Parameter.IsResult) as MpResultParameterViewModel;

        public MpExecuteParameterViewModel ExecuteViewModel => ParameterViewModels.FirstOrDefault(x => x.Parameter.IsExecute) as MpExecuteParameterViewModel;
        #endregion

        #region Appearance
        public Brush ItemBackgroundBrush => IsHovering ? Brushes.Yellow : Brushes.Transparent;
        #endregion

        #region State
        public bool IsInit { get; set; }

        public bool HasPresets => PresetViewModels.Count > 0;

        public bool HasAnyChanged => ParameterViewModels.Any(x => x.HasChanged);

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        protected string UnformattedResponse { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

        #endregion

        #region Model

        public int RuntimeId { get; set; } = 0;


        public string ItemIconBase64 { 
            get {
                if(AnalyticItem == null || AnalyticItem.Icon == null) {
                    return null;
                }
                return AnalyticItem.Icon.IconImage.ImageBase64;
            }
        }

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
            IsInit = true;

            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public abstract Task Initialize();

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

            OnPropertyChanged(nameof(ParameterViewModels));

            foreach (var aip in AnalyticItem.Presets.OrderBy(x => x.SortOrderIdx)) {
                var naipvm = await CreatePresetViewModel(aip);
                PresetViewModels.Add(naipvm);
            }

            OnPropertyChanged(nameof(PresetViewModels));
            OnPropertyChanged(nameof(HasPresets));

            if(PresetViewModels.Count > 0) {
                PresetViewModels[0].IsSelected = true;
            }

            IsBusy = false;
            IsInit = false;
        }

        public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpAnalyticItemPresetViewModel naipvm = null;

            await naipvm.InitializeAsync(aip);

            return naipvm;
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
            }

            await naipvm.InitializeAsync(aip);

            return naipvm;
        }

        public MpAnalyticItemParameterViewModel GetParam(int paramId) {
            return ParameterViewModels.FirstOrDefault(x => x.Parameter.EnumId.Equals(paramId));
        }

        public string GetUniquePresetName() {
            int uniqueIdx = 1;
            string uniqueName = $"Preset";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);

            while(PresetViewModels.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            }
            return uniqueName + uniqueIdx;
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

        public ICommand AddPresetCommand => new RelayCommand(
            async () => {
                MpAnalyticItemPreset newPreset = null;
                if(SelectedPreseetViewModel == null) {
                    newPreset = await MpAnalyticItemPreset.Create(
                        AnalyticItem,
                        GetUniquePresetName(),
                        AnalyticItem.Icon);

                    foreach(var paramVm in ParameterViewModels.OrderBy(x=>x.Parameter.SortOrderIdx)) {
                        if(paramVm.Parameter.IsRuntimeParameter) {
                            continue;
                        }
                        var naippv = await MpAnalyticItemPresetParameterValue.Create(
                            newPreset,
                            paramVm.Parameter.EnumId,
                            paramVm.CurrentValueViewModel.Value);

                        newPreset.PresetParameterValues.Add(naippv);
                    }
                } else {
                    newPreset = SelectedPreseetViewModel.Preset.Clone() as MpAnalyticItemPreset;
                    await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemPreset>(newPreset);
                }
                var npvm = await CreatePresetViewModel(newPreset);
                PresetViewModels.Add(npvm);
                PresetViewModels.ForEach(x => x.IsSelected = false);
                npvm.IsSelected = true;
            });
        #endregion
    }
}
