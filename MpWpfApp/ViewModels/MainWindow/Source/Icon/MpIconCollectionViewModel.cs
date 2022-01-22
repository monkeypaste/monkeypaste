using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using MonkeyPaste;

namespace MpWpfApp {
    public interface MpIUserIconViewModel {
        bool IsReadOnly { get; }
        Task<MpIcon> GetIcon();
        Task SetIcon(MpIcon icon);
    }
    public class MpIconCollectionViewModel : MpViewModelBase, MpISingletonViewModel<MpIconCollectionViewModel> {
        #region Properties

        #region View Models

        public ObservableCollection<MpIconViewModel> IconViewModels { get; set; } = new ObservableCollection<MpIconViewModel>();
        #endregion

        #endregion

        #region Constructors

        private static MpIconCollectionViewModel _instance;
        public static MpIconCollectionViewModel Instance => _instance ?? (_instance = new MpIconCollectionViewModel());


        public MpIconCollectionViewModel() : base(null) {
            MpHelpers.RunOnMainThreadAsync(Init);
        }

        public async Task Init() {
            IsBusy = true;

            IconViewModels.Clear();
            var il = await MpDb.GetItemsAsync<MpIcon>();
            foreach(var i in il) {
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
            }
            OnPropertyChanged(nameof(IconViewModels));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpIconViewModel> CreateIconViewModel(MpIcon i) {
            var ivm = new MpIconViewModel(this);
            await ivm.InitializeAsync(i);
            return ivm;
        }

        #endregion

        #region Protected Methods

        protected override async void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpIcon i) {
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
            }
        }

        protected override async void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                await ivm.InitializeAsync(i);
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                IconViewModels.Remove(ivm);
                OnPropertyChanged(nameof(IconViewModels));
            }
        }

        #endregion

        #region Private Methods


        #endregion

        #region Commands

        public ICommand ChangeIconCommand => new RelayCommand<object>(
            async (args) => {
                FrameworkElement fe = args as FrameworkElement;
                var uivm = fe.DataContext as MpIUserIconViewModel;
                MpIcon icon = await uivm.GetIcon();
                var iconColorChooserMenuItem = new MenuItem();
                var iconContextMenu = new ContextMenu();
                iconContextMenu.Items.Add(iconColorChooserMenuItem);
                MpHelpers.SetColorChooserMenuItem(
                    iconContextMenu,
                    iconColorChooserMenuItem,
                    async (s1, e1) => {
                        var brush = (Brush)((Border)s1).Tag;
                        var bmpSrc = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/texture.png"));
                        bmpSrc = MpWpfImagingHelper.TintBitmapSource(bmpSrc, ((SolidColorBrush)brush).Color);
                        icon.IconImage.ImageBase64 = bmpSrc.ToBase64String();
                        await icon.CreateOrUpdateBorder(brush.ToHex());
                        await uivm.SetIcon(icon);
                    }
                );
                var iconImageChooserMenuItem = new MenuItem();
                iconImageChooserMenuItem.Header = "Choose Image...";
                iconImageChooserMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/image_icon.png")) };
                iconImageChooserMenuItem.Click += async(s, e) => {
                    var openFileDialog = new OpenFileDialog() {
                        Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                        Title = "Select Image for Icon",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                    };
                    bool? openResult = openFileDialog.ShowDialog();
                    if (openResult != null && openResult.Value) {
                        string imagePath = openFileDialog.FileName;
                        var bmpSrc = (BitmapSource)new BitmapImage(new Uri(imagePath));
                        icon.IconImage.ImageBase64 = bmpSrc.ToBase64String();
                        await icon.CreateOrUpdateBorder();
                        await uivm.SetIcon(icon);
                    }
                };
                iconContextMenu.Items.Add(iconImageChooserMenuItem);
                fe.ContextMenu = iconContextMenu;
                iconContextMenu.PlacementTarget = fe;
                iconContextMenu.IsOpen = true;
            });

        #endregion
    }
}
