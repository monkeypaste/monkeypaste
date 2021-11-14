using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using MonkeyPaste;
using Windows.UI.Xaml.Controls.Maps;

namespace MpWpfApp {
    public class MpAnalyticItemPresetViewModel : MpViewModelBase<MpAnalyticItemViewModel>, ICloneable {
        #region Properties

        #region View Models

        #endregion

        #region Appearance

        public string ResetLabel => $"Reset {Label}";

        public string DeleteLabel => $"Delete {Label}";
        #endregion

        #region State
        public bool IsSelected { get; set; }

        public bool IsEditing { get; set; }

        #endregion

        #region Model 

        public string ShortcutKeyString { get; set; } = string.Empty;

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
                if(Preset.Label != Label) {
                    Preset.Label = Label;
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

        public bool IsReadOnly {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsReadOnly;
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
                    return null;
                }
                return Preset.Icon.IconImage.ImageBase64;
            }
        }

        public MpAnalyticItemPreset Preset { get; protected set; }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemPresetViewModel() : base (null) { }

        public MpAnalyticItemPresetViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterViewModel_PropertyChanged;
        }

        public MpAnalyticItemPresetViewModel(MpAnalyticItemViewModel parent, MpAnalyticItemPreset aip) : this(parent) {
            Preset = aip;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPreset aip) {
            IsBusy = true;

            Preset = aip;

            await Task.Delay(1);

            IsBusy = false;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetViewModel(Parent);
            caipvm.Preset = Preset.Clone() as MpAnalyticItemPreset;
            return caipvm;
        }

        #endregion

        #region Private Methods

        private void MpPresetParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    
                    break;
            } 
        }

        #endregion

        #region Commands
        public ICommand ChangeIconCommand => new RelayCommand<object>(
            (param) => {
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
                ((Button)param).ContextMenu = iconContextMenu;
                iconContextMenu.PlacementTarget = ((Button)param);
                iconContextMenu.IsOpen = true;
            });

        public ICommand AssignHotkeyCommand => new RelayCommand(
            async () => {
                ShortcutKeyString = await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    this,
                    $"Use {Label} Analyzer",
                    ShortcutKeyString,
                    MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand, Preset.Id);
            });
        #endregion
    }
}
