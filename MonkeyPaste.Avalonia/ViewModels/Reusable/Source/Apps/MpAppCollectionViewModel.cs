
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


using MonkeyPaste.Common;



namespace MonkeyPaste.Avalonia {
    public class MpAppCollectionViewModel : 
        MpSelectorViewModelBase<object,MpAppViewModel>, 
        MpIAsyncSingletonViewModel<MpAppCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAppViewModel> FilteredApps { get; set; }

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

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
        public async Task InitAsync() {
            IsBusy = true;

            while (MpIconCollectionViewModel.Instance.IsAnyBusy) {
                // wait for icons to load since app vm depends on icon vm
                await Task.Delay(100);
            }

            var appl = await RegisterWithProcessesManager();
            Items.Clear();
            foreach (var app in appl) {
                if(Items.Any(x=>x.AppId == app.Id)) {
                    // unknown apps in register will already be added so no duppys
                    continue;
                }
                var avm = await CreateAppViewModel(app);

                Items.Add(avm);
            }

            while(Items.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            if(Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            MpProcessManager.OnAppActivated += MpProcessManager_OnAppActivated;

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
            if(e is MpApp a && Items.All(x=>x.AppId != a.Id)) {
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
            // This is only called during init to keep app storage in sync so any running apps are added if unknown

            var al = await MpDb.GetItemsAsync<MpApp>();
            var unknownApps = MpPlatformWrapper.Services.ProcessWatcher.RunningProcessLookup.Keys
                                    .Where(x => !al.Any(y => y.AppPath.ToLower() == x.ToLower())).ToList();

            foreach(var uap in unknownApps) {
                var handle = MpPlatformWrapper.Services.ProcessWatcher.RunningProcessLookup[uap][0];
                string appName = MpPlatformWrapper.Services.ProcessWatcher.GetProcessApplicationName(handle);

                var iconStr = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(uap);
                var icon = await MpIcon.Create(iconStr);
                var app = await MpApp.Create(uap, appName, icon.Id);
                al.Add(app);
            }
            

           
            return al;
        }

        private void MpProcessManager_OnAppActivated(object sender, MpWpfProcessActivatedEventArgs e) {
            // if app is unknown add it
            // TODO device logic
            bool isUnknown = Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower()) == null;

            if(isUnknown) {
                MpHelpers.RunOnMainThread(async () => {
                    var iconStr = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(e.ProcessPath);
                    var icon = await MpIcon.Create(iconStr);
                    var app = await MpApp.Create(e.ProcessPath, e.ApplicationName, icon.Id);
                    // vm is added in db add handler
                });
            }
        }

        #endregion

        #region Commands

        public ICommand AddAppCommand => new MpCommand(
            async () => {
                string appPath = string.Empty;

                var openFileDialog = new System.Windows.Forms.OpenFileDialog() {
                    Filter = "Applications|*.lnk;*.exe",
                    Title = "Select application path",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                MpAvMainWindowViewModel.Instance.IsShowingDialog = true;

                var openResult = openFileDialog.ShowDialog();

                MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
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
                        app = await MpApp.Create(appPath, Path.GetFileName(appPath), icon.Id);
                        if (Items.All(x => x.AppId != app.Id)) {
                            avm = await CreateAppViewModel(app);
                            Items.Add(avm);
                        }
                        
                    }

                    SelectedItem = avm;
                }
            });

        #endregion
    }
}
