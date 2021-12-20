using System;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Controls.Maps;

namespace MpWpfApp {
    public class MpAnalyticItemPresetViewModel : MpViewModelBase<MpAnalyticItemViewModel>, MpIShortcutCommand, ICloneable {
        #region Properties

        #region View Models

        #region Appearance

        public string ResetLabel => $"Reset {Label}";

        public string DeleteLabel => $"Delete {Label}";
        #endregion

        #region State

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
                    OnPropertyChanged(nameof(IsQuickAction));
                }
            }
        }

        public string PresetIcon {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.ItemIconBase64;
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

            Preset = aip;// await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

            OnPropertyChanged(nameof(ShortcutViewModel));

            await Task.Delay(1);

            IsBusy = false;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetViewModel(Parent);
            caipvm.Preset = Preset.Clone() as MpAnalyticItemPreset;
            return caipvm;
        }

        #endregion

        #region Protected Methods

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
                    break;
                case nameof(IsEditing):
                    if(IsEditing) {
                        ManagePresetCommand.Execute(null);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditing));
                    break;
            } 
        }

        #endregion

        #region Commands

        public ICommand ManagePresetCommand => new RelayCommand(
            async() => {
                if(!Parent.IsLoaded) {
                    await Parent.LoadChildren();
                }
                Parent.PresetViewModels.ForEach(x => x.IsSelected = x == this);
                Parent.PresetViewModels.ForEach(x => x.IsEditing = x == this);
            });

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
