using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    public class MpAppCollectionViewModel : MpSingletonViewModel2<MpAppCollectionViewModel> {
        #region Properties

        #region View Models
                
        public ObservableCollection<MpAppViewModel> AppViewModels { get; set; } = new ObservableCollection<MpAppViewModel>();

        public MpAppViewModel SelectedAppViewModel {
            get => AppViewModels.FirstOrDefault(x => x.IsSelected);
            set {
                AppViewModels.ForEach(x => x.IsSelected = false);
                if(value != null) {
                    AppViewModels.ForEach(x => x.IsSelected = x.AppId == value.AppId);
                }
            }
        }

        #endregion
        #endregion

        #region Constructors


        public MpAppCollectionViewModel() : base() {
            Task.Run(Init);
        }

        public async Task Init() {
            IsBusy = true;

            var appl = await MpDb.Instance.GetItemsAsync<MpApp>();
            AppViewModels.Clear();
            foreach (var app in appl) {
                var avm = await CreateAppViewModel(app);
                await AddApp(avm);
            }
            OnPropertyChanged(nameof(AppViewModels));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public MpAppViewModel GetAppViewModelByAppId(int appId) {
            return AppViewModels.FirstOrDefault(x => x.AppId == appId);
        }

        public MpAppViewModel GetAppViewModelByProcessPath(string processPath) {
            return AppViewModels.FirstOrDefault(x => x.AppPath.ToLower() == processPath.ToLower());
        }

        public async Task<bool> UpdateRejection(MpAppViewModel app, bool rejectApp) {
            IsBusy = true;
            var avm = GetAppViewModelByProcessPath(app.AppPath);
            if (avm != null) {
                bool wasCanceled = false;
                if (rejectApp) {
                    var clipsFromApp = await MpDataModelProvider.Instance.GetCopyItemsByAppId(app.AppId);
                    IsBusy = false;
                    
                    if (clipsFromApp != null && clipsFromApp.Count > 0) {
                        MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + app.AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (confirmExclusionResult == MessageBoxResult.Cancel) {
                            wasCanceled = true;
                        } else {
                            MpApp appToReject = app.App;
                            if (confirmExclusionResult == MessageBoxResult.Yes) {
                                IsBusy = true;

                                await Task.WhenAll(clipsFromApp.Select(x => x.DeleteFromDatabaseAsync()));
                            }
                        }
                    }
                }
                if (wasCanceled) {
                    IsBusy = false;
                    return app.IsRejected;
                }

                avm.IsRejected = rejectApp;
                await avm.App.WriteToDatabaseAsync();

            } else {
                MonkeyPaste.MpConsole.WriteLine("AppCollection.UpdateRejection error, app: " + app.AppName + " is not in collection");
            }
            IsBusy = false;
            return rejectApp;
        }

        public async Task AddApp(MpAppViewModel avm) {
            var dupCheck = GetAppViewModelByProcessPath(avm.AppPath);
            if (dupCheck == null) {
                await avm.App.WriteToDatabaseAsync();
                AppViewModels.Add(avm);
            }

            await UpdateRejection(avm, avm.IsRejected);
        }

        public bool IsAppRejected(string processPath) {
            var avm = GetAppViewModelByProcessPath(processPath);
            if(avm == null) {
                return false;
            }
            return avm.IsRejected;
        }

        public void Remove(MpAppViewModel avm) {
            AppViewModels.Remove(avm);
        }

        public async Task<MpAppViewModel> CreateAppViewModel(MpApp app) {
            var avm = new MpAppViewModel(this);
            await avm.InitializeAsync(app);
            return avm;
        }
        #endregion

        #region Commands

        public ICommand AddAppCommand => new RelayCommand(
            async () => {
                string appPath = string.Empty;

                var openFileDialog = new System.Windows.Forms.OpenFileDialog() {
                    Filter = "Applications|*.lnk;*.exe",
                    Title = "Select application path",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                var openResult = openFileDialog.ShowDialog();
                if (openResult == System.Windows.Forms.DialogResult.Cancel) {
                    return;
                }
                if (openResult == System.Windows.Forms.DialogResult.OK) {
                    appPath = openFileDialog.FileName;
                    if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                        appPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                    }
                    MpApp app = null;
                    var avm = AppViewModels.FirstOrDefault(x => x.AppPath.ToLower() == appPath.ToLower());
                    if (avm == null) {
                        var iconBmpSrc = MpHelpers.Instance.GetIconImage(appPath);
                        var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                        app = await MpApp.Create(appPath, Path.GetFileName(appPath), icon);
                        avm = await CreateAppViewModel(app);
                        await AddApp(avm);
                    }

                    SelectedAppViewModel = avm;
                }
            });

        #endregion
    }
}
