using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconCollectionViewModel : 
        MpViewModelBase, 
        MpIAsyncSingletonViewModel<MpAvIconCollectionViewModel>,
        MpIUserColorViewModel {
        #region Private Variables

        private MpIUserIconViewModel _currentIconViewModel;

        #endregion
         
        #region Properties

        #region View Models

        public ObservableCollection<MpAvIconViewModel> IconViewModels { get; set; } = new ObservableCollection<MpAvIconViewModel>();
        #endregion

        #region MpIUserColorViewModel Implementation

        //this is used to allow ChangeIconCommand to select a color and convert it to an icon 
        public string UserHexColor { get; set; }

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || IconViewModels.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors

        private static MpAvIconCollectionViewModel _instance;
        public static MpAvIconCollectionViewModel Instance => _instance ?? (_instance = new MpAvIconCollectionViewModel());


        public MpAvIconCollectionViewModel() : base(null) {
            PropertyChanged += MpIconCollectionViewModel_PropertyChanged;
            Dispatcher.UIThread.InvokeAsync(InitAsync);
        }

        public async Task InitAsync() {
            IsBusy = true;

            IconViewModels.Clear();
            var il = await MpDb.GetItemsAsync<MpIcon>();
            foreach(var i in il) {
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
            }

            while(IconViewModels.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(IconViewModels));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpAvIconViewModel> CreateIconViewModel(MpIcon i) {
            var ivm = new MpAvIconViewModel(this);
            await ivm.InitializeAsync(i);
            return ivm;
        }

        #endregion

        #region Protected Methods

        protected override async void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpIcon i) {
                IsBusy = true;
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
                while(ivm.IsBusy) {
                    await Task.Delay(100);
                }
                IsBusy = false;
            }
        }

        protected override async void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                IsBusy = true;

                if(ivm == null) {
                    ivm = await CreateIconViewModel(i);
                    IconViewModels.Add(ivm);
                } else {
                    await ivm.InitializeAsync(i);
                }
                while (ivm.IsBusy) {
                    await Task.Delay(100);
                }
                IsBusy = false;
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                IsBusy = true;

                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                IconViewModels.Remove(ivm);
                OnPropertyChanged(nameof(IconViewModels));

                IsBusy = false;
            }
        }

        #endregion

        #region Private Methods

        private void MpIconCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(UserHexColor):
                    if(_currentIconViewModel == null) {
                        // is not a custom color so set in ChangeIconCommand
                        return;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        await SetUserIconToCurrentHexColorAsync(UserHexColor, _currentIconViewModel);

                        _currentIconViewModel = null;
                        UserHexColor = null;
                    });
                    break;
            }
        }

        private async Task SetUserIconToCurrentHexColorAsync(string hexColor, MpIUserIconViewModel uivm) {
            await Task.Delay(1);
            //var bmpSrc = (Bitmap)new BitmapImage(new Uri(MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/texture.png"));
            //bmpSrc = bmpSrc.Tint(hexColor.ToWinMediaColor());
            //MpIcon icon ;
            //if (uivm.IconId == 0) {
            //    // likely means its current icon is a default reference to a parent
            //    icon = await MpIcon.Create(
            //        iconImgBase64: bmpSrc.ToBase64String(),
            //        createBorder: false);
            //    uivm.IconId = icon.Id;
            //} else {
            //    icon = await MpDb.GetItemAsync<MpIcon>(uivm.IconId);
            //    var img = await MpDb.GetItemAsync<MpDbImage>(icon.IconImageId);
            //    img.ImageBase64 = bmpSrc.ToBase64String();
            //    await img.WriteToDatabaseAsync();
            //    await icon.CreateOrUpdateBorderAsync(forceHexColor: hexColor);
            //}
            //uivm.OnPropertyChanged(nameof(uivm.IconId));
        }
        #endregion

        #region Commands

        public ICommand SelectImagePathCommand => new MpAsyncCommand<object>(
            async (args) => {
                var uivm = args as MpIUserIconViewModel;
                if (uivm == null) {
                    throw new Exception("SelectImagePathCommand require MpIUserIconViewModel argument");
                }

                //var openFileDialog = new OpenFileDialog() {
                //    Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                //    Title = "Select Image for Icon",
                //    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                //};

                MpAvMainWindowViewModel.Instance.IsShowingDialog = true;

                var selectedImagePath = await new OpenFileDialog() {
                    Filters = new List<FileDialogFilter>() {
                        new FileDialogFilter() {
                            Name = "Image",
                            Extensions = new List<string>("png,gif,jpg,jpeg,bmp".Split(","))
                            }},
                    Title = "Select Image for Icon",
                    Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                }.ShowAsync(MpAvMainWindow.Instance);
                
                MpAvMenuExtension.CloseMenu();
                
                if (selectedImagePath != null && selectedImagePath.Length == 1 && string.IsNullOrEmpty(selectedImagePath[0])) {
                    string imagePath = selectedImagePath[0];
                    var bmpSrc = new Bitmap(imagePath); 

                    MpIcon icon = null;
                    if (uivm.IconId == 0) {
                        // likely means its current icon is a default reference to a parent
                        icon = await MpIcon.Create(
                            iconImgBase64: bmpSrc.ToBase64String(),
                            createBorder: false);
                        uivm.IconId = icon.Id;
                    } else {
                        var img = await MpDb.GetItemAsync<MpDbImage>(icon.IconImageId);
                        img.ImageBase64 = bmpSrc.ToBase64String();
                        await img.WriteToDatabaseAsync();

                        await icon.CreateOrUpdateBorderAsync();
                    }
                    uivm.OnPropertyChanged(nameof(uivm.IconId));

                    //uivm.SetIconCommand.Execute(icon);
                }
                MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            });

        public ICommand ChangeIconCommand => new MpCommand<object>(
             (args) => {
                 //FrameworkElement fe = args as FrameworkElement;
                 //dynamic fe = args;
                 //MpMenuItemViewModel mivm = new MpMenuItemViewModel();

                 //if(fe.DataContext is MpISelectableViewModel svm) {
                 //    svm.IsSelected = true;
                 //}

                 //if (fe.DataContext is MpIUserColorViewModel ucvm) {
                 //    mivm.SubItems = new ObservableCollection<MpMenuItemViewModel>() {
                 //        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(ucvm)
                 //    };
                 //} else if (fe.DataContext is MpIUserIconViewModel uivm) {
                 //    _currentIconViewModel = uivm;

                 //    mivm.SubItems = new ObservableCollection<MpMenuItemViewModel>() {
                 //        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(this),
                 //        new MpMenuItemViewModel() { IsSeparator = true },
                 //        new MpMenuItemViewModel() {
                 //            Header = "Choose Image...",
                 //            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ImageIcon") as string,
                 //            Command = SelectImagePathCommand,
                 //            CommandParameter = uivm
                 //        }
                 //    };
                 //}

                 //MpContextMenuView.Instance.DataContext = mivm;
                 //MpContextMenuView.Instance.PlacementTarget = fe;
                 //MpContextMenuView.Instance.IsOpen = true;

                 //if(fe.DataContext is MpIUserIconViewModel) {
                 //    var uivm = fe.DataContext as MpIUserIconViewModel;
                 //    //wait for selection, if color then conver to icon
                 //    Dispatcher.UIThread.Post(async () => {
                 //        while (MpContextMenuView.Instance.IsOpen) {
                 //            await Task.Delay(100);
                 //        }
                 //        if (string.IsNullOrEmpty(UserHexColor)) {
                 //            return;
                 //        }
                 //        await SetUserIconToCurrentHexColorAsync(UserHexColor, uivm);

                 //        _currentIconViewModel = null;
                 //        UserHexColor = null;
                 //    });
                 //} else {

                 //    _currentIconViewModel = null;
                 //    UserHexColor = null;
                 //}
            },(args)=> {
                //if(args !=  null) {
                //    object dc = args;
                //    if(args is FrameworkElement fe) {
                //        dc = fe.DataContext;
                //    }
                //    if(dc is MpIUserIconViewModel uivm) {
                //        var icon = MpDataModelProvider.GetIconById(uivm.IconId);
                //        return !icon.IsReadOnly;
                //    }
                //    if (dc is MpIUserColorViewModel ucvm) {
                //        return true;
                //    }
                //}
                return false;
            });


        #endregion
    }
}
