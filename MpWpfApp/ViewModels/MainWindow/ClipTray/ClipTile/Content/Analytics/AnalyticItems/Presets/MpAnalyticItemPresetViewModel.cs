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
                if (Preset == null || Preset.Icon == null) {
                    if(Parent == null) {
                        return null;
                    }
                    return Parent.ItemIconBase64;
                }
                return Preset.Icon.IconImage.ImageBase64;
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

            Preset = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);

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
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.SelectedItem));
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

        public ICommand ManagePresetCommand => new RelayCommand(
            async() => {
                if(!Parent.IsLoaded) {
                    throw new Exception("Item should be loaded on startup");
                }
                Parent.PresetViewModels.ForEach(x => x.IsSelected = x == this);
                Parent.PresetViewModels.ForEach(x => x.IsEditing = x == this);
                Parent.OnPropertyChanged(nameof(Parent.SelectedPresetViewModel));
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
