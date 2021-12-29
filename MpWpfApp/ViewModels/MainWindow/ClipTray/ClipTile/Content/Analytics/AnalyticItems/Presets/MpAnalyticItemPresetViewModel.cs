using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFImageLoading.Helpers.Exif;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using MonkeyPaste;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls.Maps;

namespace MpWpfApp {
    public class MpAnalyticItemPresetViewModel : MpViewModelBase<MpAnalyticItemViewModel>, MpIShortcutCommand, ICloneable {
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemParameterViewModel> ParameterViewModels { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();

        public Dictionary<int, MpAnalyticItemParameterViewModel> ParamLookup {
            get {
                var paraDict = new Dictionary<int, MpAnalyticItemParameterViewModel>();
                foreach (var pvm in ParameterViewModels) {
                    paraDict.Add(pvm.ParamEnumId, pvm);
                }
                return paraDict;
            }
        }

        public MpAnalyticItemParameterViewModel SelectedParameter => ParameterViewModels.FirstOrDefault(x => x.IsSelected);

        public MpContextMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpContextMenuItemViewModel(
                        header: Label,
                        command: MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                        commandParameter: Preset.Id,
                        isChecked: null,
                        iconSource: PresetIcon,
                        subItems: null,
                        inputGestureText: ShortcutKeyString,
                        bgBrush: null);
            }
        }
        #endregion

        #region Appearance

        public string ResetLabel => $"Reset {Label}";

        public string DeleteLabel => $"Delete {Label}";

        #endregion

        #region State

        public bool CanSave => HasAnyParamValueChanged || HasModelChanged;

        public bool HasAnyParamValueChanged => ParameterViewModels.Any(x => x.HasValueChanged);

        public bool IsAllValid => ParameterViewModels.All(x => x.IsValid);

        public bool IsSelected { get; set; }

        public bool IsEditing { get; set; }

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

        public string PresetIcon {
            get {
                if (Preset == null || Preset.Icon == null) {
                    if(Parent == null) {
                        return null;
                    }
                    return Parent.ItemIconBase64;
                }
                return Preset.Icon.IconImage.ImageBase64;
            }
            set {
                if (PresetIcon != value) {
                    Preset.Icon.IconImage.ImageBase64 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PresetIcon));
                }
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


            ParameterViewModels.Clear();

            if(aip == null) {
                Preset = await MpAnalyticItemPreset.Create(Parent.AnalyticItem, "Default", Parent.AnalyticItem.Icon, true, false, 0);
            } else {
                Preset = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);
            }

            string formatJson = MonkeyPaste.MpHelpers.Instance.ReadTextFromResource(Parent.AnalyticItem.ParameterFormatResourcePath);

            var paramlist = JsonConvert.DeserializeObject<MpAnalyticItemFormat>(
                formatJson, new MpJsonEnumConverter()).ParameterFormats;

            foreach (var param in paramlist.OrderBy(x => x.SortOrderIdx)) {
                var presetVal = Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == param.EnumId);
                var naipvm = await CreateParameterViewModel(param,presetVal);
                ParameterViewModels.Add(naipvm);
            }

            OnPropertyChanged(nameof(ShortcutViewModel));
            OnPropertyChanged(nameof(ParameterViewModels));
            OnPropertyChanged(nameof(HasAnyParamValueChanged));
            ParameterViewModels.ForEach(x => x.Validate());
            HasModelChanged = false;

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip, MpAnalyticItemPresetParameterValue aippv) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aip.ParameterType) {
                case MpAnalyticItemParameterType.ComboBox:
                    if(aip.IsMultiValue) {
                        naipvm = new MpMultiSelectComboBoxParameterViewModel(this);
                    } else {
                        naipvm = new MpComboBoxParameterViewModel(this);
                    }                    
                    break;
                case MpAnalyticItemParameterType.Text:
                    naipvm = new MpTextBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticItemParameterType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                default:
                    throw new Exception(@"Unsupported Paramter type: " + Enum.GetName(typeof(MpAnalyticItemParameterType), aip.ParameterType));
            }

            naipvm.PropertyChanged += ParameterViewModels_PropertyChanged;
            naipvm.OnValidate += ParameterViewModel_OnValidate;

            if (aip.IsValueDeferred) {
                aip = await Parent.DeferredCreateParameterModel(aip);
            }

            if(naipvm is MpSliderParameterViewModel) {
                //Debugger.Break();
            }
            MpAnalyticItemParameterValue aipv = aippv == null ? null: aip.Values.FirstOrDefault(x => x.Value == aippv.Value);
            if (aipv != null) {
                aip.Values.ForEach(x => x.IsDefault = x.Value == aipv.Value);
            } else if (aip.Values.Count > 0) {
                if(aip.IsMultiValue) {
                    var mvl = aippv.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    aip.Values.ForEach(x => x.IsDefault = mvl.Any(y => y.ToLower() == x.Value.ToLower()));
                } else {
                    aip.Values.FirstOrDefault(x => x.IsDefault).Value = aippv.Value;
                }
            }

            await naipvm.InitializeAsync(aip);

            return naipvm;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetViewModel(Parent);
            caipvm.Preset = Preset.Clone() as MpAnalyticItemPreset;
            return caipvm;
        }

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
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
                    break;
                case nameof(IsEditing):
                    if(IsEditing) {
                        ManagePresetCommand.Execute(null);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditing));
                    OnPropertyChanged(nameof(HasAnyParamValueChanged));
                    OnPropertyChanged(nameof(HasModelChanged));
                    OnPropertyChanged(nameof(CanSave));
                    break;
                case nameof(HasModelChanged):
                    OnPropertyChanged(nameof(CanSave));
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
                    OnPropertyChanged(nameof(CanSave));
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ChangeIconCommand => new RelayCommand<object>(
            (args) => {
                var iconColorChooserMenuItem = new MenuItem();
                var iconContextMenu = new ContextMenu();
                iconContextMenu.Items.Add(iconColorChooserMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    iconContextMenu,
                    iconColorChooserMenuItem,
                    async (s1, e1) => {
                        var brush = (Brush)((Border)s1).Tag;
                        var bmpSrc = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/texture.png"));
                        var presetIcon = MpHelpers.Instance.TintBitmapSource(bmpSrc, ((SolidColorBrush)brush).Color);
                        Preset.Icon = await MpIcon.Create(presetIcon.ToBase64String());
                        Preset.IconId = Preset.Icon.Id;
                        //await Preset.WriteToDatabaseAsync();

                        OnPropertyChanged(nameof(PresetIcon));
                    }
                );
                var iconImageChooserMenuItem = new MenuItem();
                iconImageChooserMenuItem.Header = "Choose Image...";
                iconImageChooserMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/image_icon.png")) };
                iconImageChooserMenuItem.Click += async (s, e) => {
                    var openFileDialog = new OpenFileDialog() {
                        Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                        Title = "Select Image for " + Label,
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    };
                    bool? openResult = openFileDialog.ShowDialog();
                    if (openResult != null && openResult.Value) {
                        string imagePath = openFileDialog.FileName;
                        var presetIcon = (BitmapSource)new BitmapImage(new Uri(imagePath));
                        Preset.Icon = await MpIcon.Create(presetIcon.ToBase64String());
                        Preset.IconId = Preset.Icon.Id;
                        //await Preset.WriteToDatabaseAsync();

                        OnPropertyChanged(nameof(PresetIcon));
                    }
                };
                iconContextMenu.Items.Add(iconImageChooserMenuItem);
                ((Button)args).ContextMenu = iconContextMenu;
                iconContextMenu.PlacementTarget = ((Button)args);
                iconContextMenu.IsOpen = true;
            }, (args) => !IsDefault && args is Button);

        public ICommand CancelChangesCommand => new RelayCommand(
            async () => {
                var aip = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(AnalyticItemPresetId);
                Preset = aip;

                ParameterViewModels.ForEach(x => x.CurrentValue = x.DefaultValue);
                OnPropertyChanged(nameof(HasAnyParamValueChanged));
                HasModelChanged = false;
                IsEditing = false;
            },
            CanSave);

        public ICommand SaveChangesCommand => new RelayCommand(
            async () => {
                foreach (var paramVm in ParameterViewModels) {
                    var presetValue = Preset.PresetParameterValues.FirstOrDefault(x => x.ParameterEnumId == paramVm.ParamEnumId);
                    if(presetValue == null) {
                        presetValue = await MpAnalyticItemPresetParameterValue.Create(
                            Preset, paramVm.ParamEnumId, paramVm.CurrentValue);
                    } else {
                        presetValue.Value = paramVm.CurrentValue;
                        await presetValue.WriteToDatabaseAsync();
                    }
                    if(paramVm is MpComboBoxParameterViewModel cmbvm) {
                        paramVm.Parameter.Values.ForEach(x => x.IsDefault = x.Value == paramVm.CurrentValue);
                    } else {
                        paramVm.Parameter.Values.FirstOrDefault(x => x.IsDefault).Value = paramVm.CurrentValue;
                    }
                }
                if(HasModelChanged) {
                    await Preset.WriteToDatabaseAsync();
                    HasModelChanged = false;
                }
                OnPropertyChanged(nameof(CanSave));
            },
           CanSave);

        public ICommand ManagePresetCommand => new RelayCommand(
            () => {
                Parent.PresetViewModels.ForEach(x => x.IsSelected = x == this);
                Parent.PresetViewModels.ForEach(x => x.IsEditing = x == this);
                Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
            }, !IsEditing && !Parent.IsAnyEditing);

        public ICommand ExecutePresetCommand => new RelayCommand(
            () => {
                Parent.PresetViewModels.ForEach(x => x.IsSelected = x == this);
                Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
                Parent.ExecuteAnalysisCommand.Execute(null);
            }, Parent.CanExecuteAnalysis);

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
                

                if(ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });

        
        #endregion
    }
}
