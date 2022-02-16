using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFImageLoading.Helpers.Exif;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls.Maps;

namespace MpWpfApp {
    public class MpAnalyticItemPresetViewModel : 
        MpSelectorViewModelBase<MpAnalyticItemViewModel, MpAnalyticItemParameterViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel,
        MpITriggerActionViewModel, 
        MpISidebarItemViewModel,
        MpIUserIconViewModel,
        MpIShortcutCommand, 
        MpITreeItemViewModel, ICloneable {
        #region Properties

        #region View Models

        public Dictionary<int, MpAnalyticItemParameterViewModel> ParamLookup {
            get {
                var paraDict = new Dictionary<int, MpAnalyticItemParameterViewModel>();
                foreach (var pvm in Items) {
                    paraDict.Add(pvm.ParamEnumId, pvm);
                }
                return paraDict;
            }
        }
        public MpMenuItemViewModel MenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = Label,
                    Command = MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                    CommandParameter = AnalyticItemPresetId,
                    IconId = IconId,
                    ShortcutType = MpShortcutType.AnalyzeCopyItemWithPreset,
                    ShortcutObjId = AnalyticItemPresetId
                };
            }
        }

        public MpITreeItemViewModel ParentTreeItem => Parent;

        public ObservableCollection<MpITreeItemViewModel> Children { get; set; } = null;

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;

        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => MpAnalyticItemCollectionViewModel.Instance;

        #endregion

        #region Appearance

        public MpCursorType DeleteCursor => IsDefault ? MpCursorType.Invalid : MpCursorType.Default;

        public string ResetLabel => $"Reset {Label}";

        public string DeleteLabel => $"Delete {Label}";

        #endregion

        #region State

        public bool HasAnyParameterValueChanged => Items.Any(x => x.HasModelChanged);

        public bool IsAllValid => Items.All(x => x.IsValid);

        public bool IsEditingParameters { get; set; }

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsReadOnly => IsDefault;

        #endregion

        #region Model 

        public bool IsDefault {
            get {
                if(Preset == null) {
                    return false;
                }
                return Preset.IsDefault;
            }
        }

        public string Label {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Preset.Label)) {
                    return Preset.Label;
                }
                return Preset.Label;
            }
            set {
                if(Preset.Label != value) {
                    Preset.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public string Description {
            get {
                if (Preset == null) {
                    return null;
                }
                if (string.IsNullOrEmpty(Preset.Description)) {
                    return null;
                }
                return Preset.Description;
            }
            set {
                if (Description != value) {
                    Preset.Description = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int SortOrderIdx {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.SortOrderIdx;
            }
            set {
                if(Preset != null && SortOrderIdx != value) {
                    Preset.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public bool IsQuickAction {
            get {
                if (Preset == null) {
                    return true;
                }
                return Preset.IsQuickAction;
            }
            set {
                if(IsQuickAction != value) {
                    Preset.IsQuickAction = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsQuickAction));
                }
            }
        }

        public int IconId {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.IconId;
            }
            set {
                if(IconId != value) {
                    Preset.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        public string AnalyzerPluginGuid {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                return Preset.AnalyzerPluginGuid;
            }
        }

        public int AnalyticItemPresetId {
            get {
                if(Preset == null) {
                    return 0;
                }
                return Preset.Id;
            }
        }

        public MpAnalyticItemPreset Preset { get; protected set; }
        #endregion

        #region MpIShortcutCommand Implementation

        public MpShortcutType ShortcutType => MpShortcutType.AnalyzeCopyItemWithPreset;

        public MpShortcutViewModel ShortcutViewModel {
            get {
                if(Parent == null || Preset == null) {
                    return null;
                }
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.CommandId == Preset.Id && x.ShortcutType == ShortcutType);

                if(scvm == null) {
                    scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }

        public string ShortcutKeyString => ShortcutViewModel.KeyString;

        public ICommand AssignCommand => AssignHotkeyCommand;

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemPresetViewModel() : base (null) { }

        public MpAnalyticItemPresetViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPreset aip) {
            IsBusy = true;

            Items.Clear();

            Preset = await MpDb.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

            if(Preset == null) {
                // Probably modified a manifest file
                Debugger.Break();
            }

            foreach (var paramVal in Preset.PresetParameterValues) {
                // loop through each preset value and find matching parameter
                var paramFormat = Parent.AnalyzerPluginFormat.parameters.FirstOrDefault(x => x.enumId == paramVal.ParameterEnumId);
                //if(param == null) {
                //    throw new Exception($"Error no parameter matching enumId: {paramVal.ParameterEnumId}");
                //}
                //paramFormat.values.ForEach(x => x.isDefault = false);

                //if (paramFormat.values.Any(x => x.value == paramVal.Value)) {
                //    //if parameter has a default value it needs to be swapped with preset value
                //    paramFormat.values.FirstOrDefault(x => x.isDefault).value = paramVal.Value;
                //} else if(paramFormat.values.Count == 0) {
                //    paramFormat.values.Add(
                //        new MpAnalyticItemParameterValue() {
                //            value = paramVal.Value,
                //            isDefault = true
                //        });
                //} else {

                //}
                var naipvm = await CreateParameterViewModel(paramFormat,paramVal);
                Items.Add(naipvm);
            }

            OnPropertyChanged(nameof(ShortcutViewModel));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(HasAnyParameterValueChanged));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }



        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(
            MpAnalyticItemParameterFormat aipf,
            MpAnalyticItemPresetParameterValue aipv) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aipf.parameterControlType) {
                case MpAnalyticItemParameterControlType.ComboBox:
                    if(aipf.isMultiValue) {
                        naipvm = new MpMultiSelectComboBoxParameterViewModel(this);
                    } else {
                        naipvm = new MpComboBoxParameterViewModel(this);
                    }                    
                    break;
                case MpAnalyticItemParameterControlType.Text:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterControlType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterControlType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterControlType.Hidden:
                    naipvm = new MpContentParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticItemParameterControlType), aipf.parameterControlType));
            }

            naipvm.PropertyChanged += ParameterViewModels_PropertyChanged;
            naipvm.OnValidate += ParameterViewModel_OnValidate;

            if (aipf.isValueDeferred) {
                aipf = await Parent.DeferredCreateParameterModel(aipf);
            }

            await naipvm.InitializeAsync(aipf,aipv);

            return naipvm;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetViewModel(Parent);
            caipvm.Preset = Preset.Clone() as MpAnalyticItemPreset;
            return caipvm;
        }

        public void RegisterTrigger(MpActionViewModelBase mvm) {
            Parent.OnAnalysisCompleted += mvm.OnActionTriggered;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} Registered {mvm.Label} matcher");
        }


        public void UnregisterTrigger(MpActionViewModelBase mvm) {
            Parent.OnAnalysisCompleted -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} unregistered {mvm.Label} matcher");
        }

        public async Task<MpIcon> GetIcon() {
            if(Parent == null) {
                return null;
            }
            if(Parent.IconId == IconId) {
                // this ensures icon change will not propagate since its default reference
                return null;
            }
            await Task.Delay(1);
            var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId);
            if(ivm == null) {
                return null;
            }
            return ivm.Icon;
        }
        public ICommand SetIconCommand => new RelayCommand<object>(
            async (args) => {
                IconId = (args as MpIcon).Id;
            });

        #endregion

        #region Protected Methods
        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAnalyticItemParameterViewModel;
            if (aipvm.IsRequired && string.IsNullOrEmpty(aipvm.CurrentValue)) {
                aipvm.ValidationMessage = $"{aipvm.Label} is required";
            } else {
                aipvm.ValidationMessage = string.Empty;
            }
            Parent.Validate();
        }

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == AnalyticItemPresetId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == AnalyticItemPresetId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == AnalyticItemPresetId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private void MpPresetParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    Parent.OnPropertyChanged(nameof(Parent.IsSelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedPresetViewModel));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.NextSidebarItem));
                    break;
                case nameof(IsEditingParameters):
                    if(IsEditingParameters) {
                        Parent.Items.Where(x => x != this).ForEach(x => x.IsEditingParameters = false);
                        ManagePresetCommand.Execute(null);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingParameters));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.NextSidebarItem));
                    OnPropertyChanged(nameof(HasAnyParameterValueChanged));
                    OnPropertyChanged(nameof(HasModelChanged));
                    break;
                //case nameof(IsEditingMatchers):
                //    if (IsEditingMatchers) {
                //        Parent.Items.ForEach(x => x.IsEditingParameters = false);
                //        Parent.Items.Where(x => x != this).ForEach(x => x.IsEditingMatchers = false);
                //        ManageMatchersCommand.Execute(null);
                //    }
                //    OnPropertyChanged(nameof(MatcherViewModels));
                //    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingMatchers));
                //    break;
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => { 
                            await Preset.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            } 
        }


        private void ParameterViewModels_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var pvm = sender as MpAnalyticItemParameterViewModel;
            switch (e.PropertyName) {
                case nameof(pvm.CurrentValue):
                    if (!IsSelected) {
                        return;
                    }
                    MpAnalyticItemPresetParameterValue ppv = Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == pvm.ParamEnumId);
                    if (ppv != null) {
                        ppv.Value = pvm.CurrentValue;
                    }
                    OnPropertyChanged(nameof(HasAnyParameterValueChanged));
                    break;
            }
        }

        

        #endregion

        #region Commands

        public ICommand CancelChangesCommand => new RelayCommand(
            async () => {
                var aip = await MpDb.GetItemAsync<MpAnalyticItemPreset>(AnalyticItemPresetId);
                Preset = aip;

                Items.ForEach(x => x.CurrentValue = x.DefaultValue);
                OnPropertyChanged(nameof(HasAnyParameterValueChanged));
                Items.ForEach(x => x.HasModelChanged = false);
                Items.Where(x => x is MpComboBoxParameterViewModel).Cast<MpComboBoxParameterViewModel>().ForEach(x => x.Items.ForEach(y => y.HasModelChanged = false));
                HasModelChanged = false;
                IsEditingParameters = false;
            },
            HasAnyParameterValueChanged);

        public ICommand SaveChangesCommand => new RelayCommand(
            async () => {
                foreach (var paramVm in Items) {
                    var presetValue = Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == paramVm.ParamEnumId);
                    if(presetValue == null) {
                        presetValue = await MpAnalyticItemPresetParameterValue.Create(
                            Preset, paramVm.ParamEnumId, paramVm.CurrentValue);
                    } else {
                        presetValue.Value = paramVm.CurrentValue;
                        await presetValue.WriteToDatabaseAsync();
                    }
                    if(paramVm is MpComboBoxParameterViewModel cmbvm) {
                        paramVm.Parameter.values.ForEach(x => x.isDefault = x.value == paramVm.CurrentValue);
                    } else {
                        var defParam = paramVm.Parameter.values.FirstOrDefault(x => x.isDefault);
                        if(defParam != null) {
                            defParam.value = paramVm.CurrentValue;
                        } else if(paramVm.Parameter.values.Count > 0) {
                            paramVm.Parameter.values[0].value = paramVm.CurrentValue;
                        }
                    }
                }

                Items.ForEach(x => x.HasModelChanged = false);
                OnPropertyChanged(nameof(HasAnyParameterValueChanged));
            },
           HasAnyParameterValueChanged);

        public ICommand ManagePresetCommand => new RelayCommand(
            () => {
                Parent.Items.ForEach(x => x.IsSelected = x == this);
                Parent.Items.ForEach(x => x.IsEditingParameters = x == this);
                Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
            }, !IsEditingParameters && !Parent.IsAnyEditingParameters);


        //public ICommand ExecutePresetCommand => new RelayCommand(
        //    () => {
        //        Parent.Items.ForEach(x => x.IsSelected = x == this);
        //        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
        //        Parent.ExecuteAnalysisCommand.Execute(null);
        //    }, ()=>Parent.CanExecuteAnalysis(this));

        public ICommand AssignHotkeyCommand => new RelayCommand(
            async () => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    $"Use {Label} Analyzer",
                    MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                    MpShortcutType.AnalyzeCopyItemWithPreset,
                    Preset.Id,
                    ShortcutKeyString);

                OnPropertyChanged(nameof(ShortcutViewModel));

                OnPropertyChanged(nameof(ShortcutKeyString));


                if (ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });

        #endregion
    }
}
