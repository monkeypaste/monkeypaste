using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIconViewModel : MpViewModelBase<MpIconCollectionViewModel> {
        #region Properties

        #region Appearance

        public BitmapSource IconBitmapSource { get; set; }

        public BitmapSource IconBorderBitmapSource { get; set; }

        #endregion

        #region Model

        public ObservableCollection<string> PrimaryIconColorList {
            get {
                if (Icon == null) {
                    return new ObservableCollection<string>();
                }
                return new ObservableCollection<string>(Icon.HexColors);
            }
        }

        public int IconId {
            get {
                if(Icon == null) {
                    return 0;
                }
                return Icon.Id;
            }
        }

        public MpIcon Icon { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpIconViewModel() : base(null) { }

        public MpIconViewModel(MpIconCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpIconViewModel_PropertyChanged;
        }
        
        public async Task InitializeAsync(MpIcon i) {
            IsBusy = true;

            Icon = i;

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpIconViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Icon):
                    IconBitmapSource = Icon.IconImage.ImageBase64.ToBitmapSource();
                    IconBorderBitmapSource = Icon.IconBorderImage.ImageBase64.ToBitmapSource();

                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ChangeIconCommand => new RelayCommand<object>(
            (args) => {
                //var iconColorChooserMenuItem = new MenuItem();
                //var iconContextMenu = new ContextMenu();
                //iconContextMenu.Items.Add(iconColorChooserMenuItem);
                //MpHelpers.SetColorChooserMenuItem(
                //    iconContextMenu,
                //    iconColorChooserMenuItem,
                //    (s1, e1) => {
                //        MpHelpers.RunOnMainThread(async () => {
                //            var brush = (Brush)((Border)s1).Tag;
                //            var bmpSrc = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/texture.png"));
                //            var presetIcon = MpWpfImagingHelper.TintBitmapSource(bmpSrc, ((SolidColorBrush)brush).Color);
                //            Preset.Icon = await MpIcon.Create(presetIcon.ToBase64String(), false);
                //            Preset.IconId = Preset.Icon.Id;
                //            await Preset.WriteToDatabaseAsync();

                //            OnPropertyChanged(nameof(IconId));
                //        });
                //    }
                //);
                //var iconImageChooserMenuItem = new MenuItem();
                //iconImageChooserMenuItem.Header = "Choose Image...";
                //iconImageChooserMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/image_icon.png")) };
                //iconImageChooserMenuItem.Click += async (s, e) => {
                //    var openFileDialog = new OpenFileDialog() {
                //        Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                //        Title = "Select Image for " + Label,
                //        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                //    };
                //    bool? openResult = openFileDialog.ShowDialog();
                //    if (openResult != null && openResult.Value) {
                //        string imagePath = openFileDialog.FileName;
                //        var presetIcon = (BitmapSource)new BitmapImage(new Uri(imagePath));
                //        Preset.Icon = await MpIcon.Create(presetIcon.ToBase64String());
                //        Preset.IconId = Preset.Icon.Id;
                //        await Preset.WriteToDatabaseAsync();

                //        OnPropertyChanged(nameof(IconId));
                //    }
                //};
                //iconContextMenu.Items.Add(iconImageChooserMenuItem);
                //((Button)args).ContextMenu = iconContextMenu;
                //iconContextMenu.PlacementTarget = ((Button)args);
                //iconContextMenu.IsOpen = true;
            }, (args) => args is Button);

        #endregion
    }
}
