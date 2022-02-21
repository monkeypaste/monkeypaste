using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
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

        public ICommand SelectImagePathCommand => new RelayCommand<object>(
            async (args) => {
                var uivm = args as MpIUserIconViewModel;

                var openFileDialog = new OpenFileDialog() {
                    Filter = "Image|*.png;*.gif;*.jpg;*.jpeg;*.bmp",
                    Title = "Select Image for Icon",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                MpMainWindowViewModel.Instance.IsShowingDialog = true;

                MpContextMenuView.Instance.CloseMenu();
                bool? openResult = openFileDialog.ShowDialog();
                if (openResult != null && openResult.Value) {
                    string imagePath = openFileDialog.FileName;
                    var bmpSrc = (BitmapSource)new BitmapImage(new Uri(imagePath));

                    var icon = await uivm.GetIcon();
                    if(icon == null) {
                        // likely means its current icon is a default reference to a parent
                        icon = await MpIcon.Create(
                            iconImgBase64: bmpSrc.ToBase64String(), 
                            createBorder: false);
                    } else {
                        icon.IconImage.ImageBase64 = bmpSrc.ToBase64String();
                        await icon.CreateOrUpdateBorder();
                    }

                    uivm.SetIconCommand.Execute(icon);
                }
                MpMainWindowViewModel.Instance.IsShowingDialog = true;
            });

        public ICommand ChangeIconCommand => new RelayCommand<object>(
             (args) => {
                FrameworkElement fe = args as FrameworkElement;
                MpMenuItemViewModel mivm = new MpMenuItemViewModel();

                if (fe.DataContext is MpIUserColorViewModel ucvm) {
                    string hexColor = ucvm.GetColor();
                     mivm.SubItems = new ObservableCollection<MpMenuItemViewModel>() {
                         MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(ucvm)
                     };
                }

                 if (fe.DataContext is MpIUserIconViewModel uivm) {
                     if (mivm.SubItems == null) {
                         mivm.SubItems = new ObservableCollection<MpMenuItemViewModel>();
                     } else {
                         mivm.SubItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                     }
                     mivm.SubItems.Add(
                         new MpMenuItemViewModel() {
                             Header = "Choose Image...",
                             IconResourceKey = Application.Current.Resources["ImageIcon"] as string,
                             Command = SelectImagePathCommand,
                             CommandParameter = uivm
                         });
                 }

                 MpContextMenuView.Instance.DataContext = mivm;
                 MpContextMenuView.Instance.PlacementTarget = fe;
                 MpContextMenuView.Instance.IsOpen = true;
            },(args)=> {
                if(args !=  null) {
                    object dc = args;
                    if(args is FrameworkElement fe) {
                        dc = fe.DataContext;
                    }
                    if(dc is MpIUserIconViewModel uivm) {
                        return !uivm.IsReadOnly;
                    }
                    if (dc is MpIUserColorViewModel ucvm) {
                        return !ucvm.IsReadOnly;
                    }
                }
                return false;
            });

        #endregion
    }
}
