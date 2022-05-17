using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
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
        MpSelectorViewModelBase<MpAnalyticItemViewModel, MpAnalyticItemParameterViewModelBase>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel,
        MpIActionComponent, 
        MpISidebarItemViewModel,
        MpIUserIconViewModel,
        //MpIUserColorViewModel,
        MpIShortcutCommand, 
        MpITreeItemViewModel {
        #region Properties

        #region View Models

        public Dictionary<int, MpAnalyticItemParameterViewModelBase> ParamLookup {
            get {
                var paraDict = new Dictionary<int, MpAnalyticItemParameterViewModelBase>();
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
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
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

        public bool IsAllValid => Items.All(x => x.IsValid);

        //public bool IsEditingParameters { get; set; }

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsReadOnly => IsDefault;

        #endregion

        #region Model 

        public string FullName {
            get {
                if(Preset == null || Parent == null) {
                    return string.Empty;
                }
                return $"{Parent.Title}/{Label}";
            }
        }

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

        public DateTime LastSelectedDateTime {
            get {
                if(Preset == null) {
                    return DateTime.MinValue;
                }
                return Preset.LastSelectedDateTime;
            }
            set {
                if(LastSelectedDateTime != value) {
                    Preset.LastSelectedDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
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

            Preset = aip;//await MpDb.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

            var presetValues = await MpDataModelProvider.GetAnalyticItemPresetValuesByPresetId(AnalyticItemPresetId);
            foreach(var paramFormat in Preset.AnalyzerFormat.parameters) {
                if(!presetValues.Any(x=>x.ParamId == paramFormat.paramId)) {
                    string paramVal = string.Empty;
                    if(paramFormat.values != null && paramFormat.values.Count > 0) {
                        if (paramFormat.values.Any(x => x.isDefault)) {
                            paramVal = paramFormat.values.Where(x => x.isDefault).Select(x => x.value).ToList().ToCsv();
                        } else {
                            paramVal = paramFormat.values[0].value;
                        }
                    }
                    var newPresetVal = await MpAnalyticItemPresetParameterValue.Create(
                        presetId: Preset.Id, 
                        paramEnumId: paramFormat.paramId, 
                        value: paramVal,
                        format: paramFormat);

                    presetValues.Add(newPresetVal);
                }
            }
            presetValues.ForEach(x => x.ParameterFormat = Preset.AnalyzerFormat.parameters.FirstOrDefault(y => y.paramId == x.ParamId));

            foreach (var paramVal in presetValues) {                                
                var naipvm = await CreateParameterViewModel(paramVal);
                Items.Add(naipvm);
            }


            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(ShortcutViewModel));
            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModelBase> CreateParameterViewModel(MpAnalyticItemPresetParameterValue aipv) {
            MpAnalyticItemParameterViewModelBase naipvm = null;

            switch (aipv.ParameterFormat.controlType) {
                case MpPluginParameterControlType.List:
                case MpPluginParameterControlType.MultiSelectList:
                case MpPluginParameterControlType.EditableList:
                case MpPluginParameterControlType.ComboBox:
                    naipvm = new MpEnumerableParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.PasswordBox:
                case MpPluginParameterControlType.TextBox:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpPluginParameterControlType.DirectoryChooser:
                case MpPluginParameterControlType.FileChooser:
                    naipvm = new MpFileChooserParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpPluginParameterControlType), aipv.ParameterFormat.controlType));
            }
            naipvm.OnValidate += ParameterViewModel_OnValidate;


            await naipvm.InitializeAsync(aipv);

            return naipvm;
        }

        public void Register(MpIActionComponentHandler mvm) {
            Parent.OnAnalysisCompleted += mvm.OnActionTriggered;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} Registered {mvm.Label} matcher");
        }


        public void Unregister(MpIActionComponentHandler mvm) {
            Parent.OnAnalysisCompleted -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Analyzer {Parent.Title}-{Label} unregistered {mvm.Label} matcher");
        }

        #endregion

        #region Protected Methods

        protected virtual void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            var aipvm = sender as MpAnalyticItemParameterViewModelBase;
            if (aipvm.IsRequired && string.IsNullOrWhiteSpace(aipvm.CurrentValue)) {
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
                    if(IsSelected && Parent.Parent.IsSidebarVisible) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsSelected));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedPresetViewModel));
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.NextSidebarItem));
                    CollectionViewSource.GetDefaultView(Items).Refresh();
                    break;
                //case nameof(IsEditingParameters):
                //    if(IsEditingParameters) {
                //        Parent.Items.Where(x => x != this).ForEach(x => x.IsEditingParameters = false);
                //        ManagePresetCommand.Execute(null);
                //    }
                //    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingParameters));
                //    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.NextSidebarItem));
                //    OnPropertyChanged(nameof(HasModelChanged));
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
        #endregion

        #region Commands

        //public ICommand ManagePresetCommand => new RelayCommand(
        //    () => {
        //        Parent.Items.ForEach(x => x.IsSelected = x == this);
        //        Parent.Items.ForEach(x => x.IsEditingParameters = x == this);
        //        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
        //    }, !IsEditingParameters && !Parent.IsAnyEditingParameters);

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
