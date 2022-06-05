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
using MpProcessHelper;
using System.Windows.Data;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpAppCollectionViewModel : 
        MpSelectorViewModelBase<object,MpAppViewModel>, 
        MpISingletonViewModel<MpAppCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAppViewModel> FilteredApps { get; set; }

        #endregion

        #endregion

        #region Constructors

        private static MpAppCollectionViewModel _instance;
        public static MpAppCollectionViewModel Instance => _instance ?? (_instance = new MpAppCollectionViewModel());

        public MpAppCollectionViewModel() : base(null) {
            //MpHelpers.RunOnMainThreadAsync(Init);
            PropertyChanged += MpAppCollectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task Init() {
            IsBusy = true;

            var appl = await RegisterWithProcessesManager();
            Items.Clear();
            foreach (var app in appl) {
                var avm = await CreateAppViewModel(app);
                Items.Add(avm);
            }

            while(Items.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            await RegisterWithProcessesManager();

            OnPropertyChanged(nameof(Items));

            if(Items.Count > 0) {
                Items[0].IsSelected = true;
            }


            IsBusy = false;
        }

        public async Task<MpAppViewModel> CreateAppViewModel(MpApp app) {
            var avm = new MpAppViewModel(this);
            await avm.InitializeAsync(app);
            return avm;
        }

        public bool IsAppRejected(string processPath) {
            return Items.FirstOrDefault(x => x.AppPath.ToLower() == processPath.ToLower() && x.IsRejected) != null;
        }

        public MpAppClipboardFormatInfoCollectionViewModel GetInteropSettingByAppId(int appId) {
            var aivm = Items.FirstOrDefault(x => x.AppId == appId);
            if(aivm == null) {
                return null;
            }
            return aivm.ClipboardFormatInfos;
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpApp a) {
                Task.Run(async () => {
                    var avm = await CreateAppViewModel(a);
                    Items.Add(avm);
                });
            }
        }


        #endregion

        #region Private Methods

        private void MpAppCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    if (SelectedItem != null) {
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.IconId));

                        CollectionViewSource.GetDefaultView(SelectedItem.ClipboardFormatInfos.Items).Refresh();
                    }
                    break;
            }
        }
        private async Task<List<MpApp>> RegisterWithProcessesManager() {
            MpProcessManager.OnAppActivated += MpProcessManager_OnAppActivated;

            var al = await MpDb.GetItemsAsync<MpApp>();
            var unknownApps = MpProcessManager.CurrentProcessWindowHandleStackDictionary.Keys
                                    .Where(x => !al.Any(y => y.AppPath.ToLower() == x.ToLower())).ToList();

            foreach(var uap in unknownApps) {
                var handle = MpProcessManager.CurrentProcessWindowHandleStackDictionary[uap][0];
                string appName = MpProcessManager.GetProcessApplicationName(handle);

                var iconStr = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(uap);
                var icon = await MpIcon.Create(iconStr);
                var app = await MpApp.Create(uap, appName, icon);
                al.Add(app);
            }
            return al;
        }

        private void MpProcessManager_OnAppActivated(object sender, MpProcessActivatedEventArgs e) {
            // if app is unknown add it
            // TODO device logic
            bool isUnknown = Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower()) == null;

            if(isUnknown) {
                MpHelpers.RunOnMainThread(async () => {
                    var iconStr = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(e.ProcessPath);
                    var icon = await MpIcon.Create(iconStr);
                    var app = await MpApp.Create(e.ProcessPath, e.ApplicationName, icon);

                    var avm = await CreateAppViewModel(app);
                    Items.Add(avm);
                });
            }
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
                        appPath = MpHelpers.GetShortcutTargetPath(openFileDialog.FileName);
                    }
                    MpApp app = null;
                    var avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == appPath.ToLower());
                    if (avm == null) {
                        var iconBmpSrc = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(appPath).ToBitmapSource();
                        var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                        app = await MpApp.Create(appPath, Path.GetFileName(appPath), icon);
                        avm = await CreateAppViewModel(app);
                        Items.Add(avm);
                    }

                    SelectedItem = avm;
                }
            });

        #endregion
    }
}
