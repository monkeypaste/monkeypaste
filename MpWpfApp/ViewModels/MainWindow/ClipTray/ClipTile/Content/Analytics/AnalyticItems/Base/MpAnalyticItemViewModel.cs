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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemPresetViewModel> PresetViewModels { get; set; } = new ObservableCollection<MpAnalyticItemPresetViewModel>();

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel => PresetViewModels.FirstOrDefault(x => x.IsSelected);

        public ObservableCollection<MpAnalyticItemParameterViewModel> ParameterViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();

        public MpAnalyticItemParameterViewModel SelectedParameter => ParameterViewModels.FirstOrDefault(x => x.IsSelected);

        public MpResultParameterViewModel ResultViewModel => ParameterViewModels.FirstOrDefault(x => x.Parameter.IsResult) as MpResultParameterViewModel;

        public MpExecuteParameterViewModel ExecuteViewModel => ParameterViewModels.FirstOrDefault(x => x.Parameter.IsExecute) as MpExecuteParameterViewModel;
        #endregion

        #region Appearance
        public string ResetLabel {
            get {
                if(SelectedPresetViewModel == null) {
                    return "Reset";
                }
                return SelectedPresetViewModel.ResetLabel;
            }
        }

        public string ManageLabel => $"Manage {Title}";

        public Brush ItemBackgroundBrush => IsHovering ? Brushes.Yellow : Brushes.Transparent;

        public Brush ItemTitleForegroundBrush {
            get {
                if(IsHovering) {
                    return Brushes.Red;
                }
                if(IsSelected) {
                    return Brushes.White;
                }
                return Brushes.Black;
            }
        }
        #endregion

        #region State

        public virtual bool IsLoaded => ParameterViewModels.Count > 2;

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
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public abstract Task Initialize();

        public async Task InitializeDefaultsAsync(MpAnalyticItem ai) {
            IsBusy = true;
            AnalyticItem = ai;

            // Init Execute & Result Parameters
            ParameterViewModels.Clear();
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
                        Value = string.Empty,
                        ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText
                    }
                }
            };
            MpAnalyticItemParameterViewModel raipvm = await CreateParameterViewModel(raip);
            ParameterViewModels.Add(raipvm);

            // Init Presets
            PresetViewModels.Clear();
            foreach(var preset in AnalyticItem.Presets.OrderBy(x=>x.SortOrderIdx)) {
                var naipvm = await CreatePresetViewModel(preset);
                PresetViewModels.Add(naipvm);
            }
            OnPropertyChanged(nameof(PresetViewModels));
            OnPropertyChanged(nameof(HasPresets));

            IsBusy = false;
        }

        public virtual async Task LoadChildren() {
            if (AnalyticItem == null || IsLoaded) {
                return;
            }
            IsBusy = true;

            foreach (var aip in AnalyticItem.Parameters.OrderByDescending(x => x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                ParameterViewModels.Insert(0,naipvm);
            }

            OnPropertyChanged(nameof(ParameterViewModels));

            if(PresetViewModels.Count > 0) {
                PresetViewModels[0].IsSelected = true;
            }

            IsBusy = false;
        }

        public async Task<MpAnalyticItemPresetViewModel> CreatePresetViewModel(MpAnalyticItemPreset aip) {
            MpAnalyticItemPresetViewModel naipvm = new MpAnalyticItemPresetViewModel(this);
            naipvm.PropertyChanged += Naipvm_PropertyChanged;
            await naipvm.InitializeAsync(aip);

            return naipvm;
        }

        private void Naipvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var aipvm = sender as MpAnalyticItemPresetViewModel;
            switch(e.PropertyName) {
                case nameof(aipvm.IsSelected):
                    UpdateParameterValues();
                    break;
            }
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

        private void UpdateParameterValues(bool isReset = false) {
            foreach (var paramVm in ParameterViewModels) {
                if (SelectedPresetViewModel == null) {
                    paramVm.ResetToDefault();
                } else {
                    var presetValue = SelectedPresetViewModel.Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == paramVm.ParamEnumId);

                    if (presetValue == null) {
                        paramVm.ResetToDefault();
                    } else {
                        if (isReset) {
                            paramVm.SetValue(presetValue.DefaultValue);
                        } else {
                            paramVm.SetValue(presetValue.Value);
                        }
                    }
                }
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

        public ICommand CreateNewPresetCommand => new RelayCommand(
            async () => {
                if(!IsLoaded) {
                    await LoadChildren();
                }

                MpMainWindowViewModel.Instance.IsShowingDialog = true;

                var dialog = new MpSavePresetModalWindow(GetUniquePresetName());
                bool? result = dialog.ShowDialog();

                MpMainWindowViewModel.Instance.IsShowingDialog = false;
                if (result.Value == false) {
                    return;
                }
                MpAnalyticItemPreset newPreset = await MpAnalyticItemPreset.Create(
                        AnalyticItem,
                        dialog.ResponseText,
                        AnalyticItem.Icon);

                foreach (var paramVm in ParameterViewModels.OrderBy(x => x.Parameter.SortOrderIdx)) {
                    if (paramVm.Parameter.IsRuntimeParameter) {
                        continue;
                    }
                    var naippv = await MpAnalyticItemPresetParameterValue.Create(
                        newPreset,
                        paramVm.Parameter.EnumId,
                        paramVm.CurrentValueViewModel.Value);

                    newPreset.PresetParameterValues.Add(naippv);
                }
                await newPreset.WriteToDatabaseAsync();

                var npvm = await CreatePresetViewModel(newPreset);
                PresetViewModels.Add(npvm);
                PresetViewModels.ForEach(x => x.IsSelected = false);
                npvm.IsSelected = true;
            });

        public ICommand SelectPresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (selectedPresetVm) => {
                if(!IsLoaded) {
                    await LoadChildren();
                }
                PresetViewModels.ForEach(x => x.IsSelected = false);
                selectedPresetVm.IsSelected = true;
            });

        public ICommand ResetCommand => new RelayCommand(
            () => {
                UpdateParameterValues(true);
            },
            () => HasAnyChanged);

        public ICommand ManageAnalyticItemCommand => new RelayCommand(
            async() => {
                if (!IsLoaded) {
                    await LoadChildren();
                }
                if (SelectedPresetViewModel == null && PresetViewModels.Count > 0) {
                    PresetViewModels.ForEach(x => x.IsSelected = false);
                    PresetViewModels[0].IsSelected = true;
                }
                MpMainWindowViewModel.Instance.IsShowingDialog = true;
                var manageWindow = new MpManageAnalyticItemModalWindow();
                manageWindow.DataContext = this;
                bool? result = manageWindow.ShowDialog();
                MpMainWindowViewModel.Instance.IsShowingDialog = false;
                if (result.Value == false) {
                    return;
                }

                await Task.WhenAll(PresetViewModels.Select(x => x.Preset.WriteToDatabaseAsync()).ToArray());
            });

        public ICommand DeletePresetCommand => new RelayCommand<MpAnalyticItemPresetViewModel>(
            async (presetVm) => {
                foreach(var presetVal in presetVm.Preset.PresetParameterValues) {
                    await MpDb.Instance.DeleteItemAsync<MpAnalyticItemPresetParameterValue>(presetVal);
                }
                await MpDb.Instance.DeleteItemAsync<MpAnalyticItemPreset>(presetVm.Preset);
            },
            (presetVm) => presetVm != null && !presetVm.IsReadOnly);

        public ICommand ShiftPresetCommand => new RelayCommand<object>(
            // [0] = shift dir [1] = presetvm
            async (args) => {
                var argParts = args as object[];
                int dir = (int)Convert.ToInt32(argParts[0].ToString());
                MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
                int curSortIdx = PresetViewModels.IndexOf(pvm);
                int newSortIdx = curSortIdx + dir;

                PresetViewModels.Move(curSortIdx, newSortIdx);
                for (int i = 0; i < PresetViewModels.Count; i++) {
                    PresetViewModels[i].SortOrderIdx = i;
                    await PresetViewModels[i].Preset.WriteToDatabaseAsync();
                }
            },
            (args) => {
                if (args == null) {
                    return false;
                }
                if(args is object[] argParts) {
                    int dir = (int)Convert.ToInt32(argParts[0].ToString());
                    MpAnalyticItemPresetViewModel pvm = argParts[1] as MpAnalyticItemPresetViewModel;
                    int curSortIdx = PresetViewModels.IndexOf(pvm);
                    int newSortIdx = curSortIdx + dir;
                    if(newSortIdx < 0 || newSortIdx >= PresetViewModels.Count || newSortIdx == curSortIdx) {
                        return false;
                    }
                    return true;
                }
                return false;
            });
        #endregion
    }
}
