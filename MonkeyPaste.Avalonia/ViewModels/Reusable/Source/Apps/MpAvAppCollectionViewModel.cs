
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
using Avalonia.Threading;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppCollectionViewModel : 
        MpAvSelectorViewModelBase<object,MpAvAppViewModel>, 
        MpIAsyncSingletonViewModel<MpAvAppCollectionViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvAppViewModel> FilteredApps { get; set; }

        public MpAvAppViewModel ThisAppViewModel => Items.FirstOrDefault(x => x.AppId == MpDefaultDataModelTools.ThisAppId);

        public MpAvAppViewModel LastActiveAppViewModel { get; private set; }
        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors

        private static MpAvAppCollectionViewModel _instance;
        public static MpAvAppCollectionViewModel Instance => _instance ?? (_instance = new MpAvAppCollectionViewModel());

        public MpAvAppCollectionViewModel() : base(null) {
            //Dispatcher.UIThread.InvokeAsync(Init);
            PropertyChanged += MpAppCollectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitAsync() {
            IsBusy = true;

           

            var appl = await MpDataModelProvider.GetItemsAsync<MpApp>();
            Items.Clear();
            foreach (var app in appl) {
                //if(Items.Any(x=>x.AppId == app.Id)) {
                //    // unknown apps in register will already be added so no duppys
                //    continue;
                //}
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

        public async Task<MpAvAppViewModel> CreateAppViewModel(MpApp app) {
            var avm = new MpAvAppViewModel(this);
            while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                // wait for icons to load since app vm depends on icon vm
                await Task.Delay(100);
            }
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

        public MpAvAppViewModel GetAppViewModelFromScreenPoint(MpPoint gmp, double pixelDensity) {
            IntPtr handle = IntPtr.Zero;
            if (MpAvMainWindowViewModel.Instance.MainWindowScreenRect.Contains(gmp)) {
                // at least on windows (i think since its a tool window) the p/invoke doesn't return mw handle
                handle = MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle;
            }
            if(handle == IntPtr.Zero) {
                handle = MpPlatformWrapper.Services.ProcessWatcher.GetParentHandleAtPoint(gmp);
            }
            if(handle == IntPtr.Zero) {
                return null;
            }
            string handle_path = MpPlatformWrapper.Services.ProcessWatcher.GetProcessPath(handle);
            MpConsole.WriteLine("Drop Path: " + handle_path,true,true);
            // TODO need to filter by current device here
            return Items.FirstOrDefault(x => x.AppPath.ToLower() == handle_path.ToLower());
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpApp a && Items.All(x=>x.AppId != a.Id)) {
                Dispatcher.UIThread.Post(async () => {
                    IsBusy = true;
                    MpConsole.WriteLine($"Adding new app to app collection:");
                    MpConsole.WriteLine(a.ToString());
                    var avm = await CreateAppViewModel(a);
                    Items.Add(avm);
                    IsBusy = false;
                    MpConsole.WriteLine($"App w/ id: '{a.Id}' added to collection.");
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

                        //CollectionViewSource.GetDefaultView(SelectedItem.ClipboardFormatInfos.Items).Refresh();
                        SelectedItem.ClipboardFormatInfos.OnPropertyChanged(nameof(SelectedItem.ClipboardFormatInfos.Items));
                    }
                    break;
            }
        }



        private async Task InitLastAppViewModel() {
            // wait for running processes to get created
            await Task.Delay(0);
            var la_pi = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
            if (la_pi == null) {
                // since application is being started from file system init LastActive to file system app
                la_pi = MpPlatformWrapper.Services.ProcessWatcher.FileSystemProcessInfo;
                if (la_pi == null) {
                    // need to get this set on init in process watcher
                    //Debugger.Break();
                }
            }
            if (la_pi != null) {
                LastActiveAppViewModel = Items.FirstOrDefault(x => x.AppPath.ToLower() == la_pi.ProcessPath.ToLower());
                if (LastActiveAppViewModel == null) {
                    // what's the deal?
                    Debugger.Break();
                    LastActiveAppViewModel = ThisAppViewModel;
                }
            }
        }
        private async Task RegisterWithProcessesManager() {
            // This is only called during init to keep app storage in sync so any running apps are added if unknown
            MpPlatformWrapper.Services.ProcessWatcher.StartWatcher();

            var unknownApps = MpPlatformWrapper.Services.ProcessWatcher.RunningProcessLookup.Keys
                                    .Where(x => !Items.Any(y => y.AppPath.ToLower() == x.ToLower()))
                                    .Select(x => new MpPortableProcessInfo() { ProcessPath = x }).ToList();

            MpConsole.WriteLine($"AppCollection RegisterWithProcessesManager '{unknownApps.Count}' unknown apps detected.");
            foreach(var uap in unknownApps) {
                //var handle = MpPlatformWrapper.Services.ProcessWatcher.RunningProcessLookup[uap][0];
                //string appName = MpPlatformWrapper.Services.ProcessWatcher.GetProcessApplicationName(handle);

                //var iconStr = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(uap);
                //var icon = await MpIcon.Create(iconStr);
                //var app = await MpApp.CreateAsync(
                //    appPath: uap, 
                //    appName: appName, 
                //    iconId: icon.Id);
                //al.Add(app);
                var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(uap);
                // wait for db add callback to pickup db add event
                await Task.Delay(100);
                while(IsBusy) {
                    // wait for app to be added to items from db add callback
                    await Task.Delay(100);
                }
            }

            await InitLastAppViewModel();

            // wait to add activated handler until all apps at startup are syncd
            MpPlatformWrapper.Services.ProcessWatcher.OnAppActivated += MpProcessManager_OnAppActivated;
        }

        private async void MpProcessManager_OnAppActivated(object sender, MpPortableProcessInfo e) {
            // if app is unknown add it
            // TODO device logic
            while(IsBusy) {
                await Task.Delay(100);
            }
            Dispatcher.UIThread.Post(async () => {
                var avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower());

                if (avm == null) {
                    // unknown app activated add like in registration
                    var new_app =
                    await MpPlatformWrapper.Services.AppBuilder.CreateAsync(e);
                    // wait for db add to pick up model
                    await Task.Delay(100);
                    while(IsBusy) {
                        // wait for vm to be added to items
                        await Task.Delay(100);
                    }

                    // BUG may need to add .ToList() on items here, hopefully fixed w/ dbAdd waiting
                    avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower());
                    if(avm == null) {
                        // somethings wrong check console for db add msgs
                        Debugger.Break();
                    }
                } 
                LastActiveAppViewModel = avm;
            });
            
        }

        #endregion

        #region Commands

        public ICommand AddAppCommand => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
                //string appPath = string.Empty;

                //var openFileDialog = new System.Windows.Forms.OpenFileDialog() {
                //    Filter = "Applications|*.lnk;*.exe",
                //    Title = "Select application path",
                //    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                //};
                //MpAvMainWindowViewModel.Instance.IsShowingDialog = true;

                //var openResult = openFileDialog.ShowDialog();

                //MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
                //if (openResult == System.Windows.Forms.DialogResult.Cancel) {
                //    return;
                //}
                //if (openResult == System.Windows.Forms.DialogResult.OK) {
                //    appPath = openFileDialog.FileName;
                //    if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                //        appPath = MpHelpers.GetShortcutTargetPath(openFileDialog.FileName);
                //    }
                //    MpApp app = null;
                //    var avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == appPath.ToLower());
                //    if (avm == null) {
                //        var iconBmpSrc = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(appPath).ToBitmapSource();
                //        var icon = await MpIcon.Create(iconBmpSrc.ToBase64String());
                //        app = await MpApp.Create(appPath, Path.GetFileName(appPath), icon.Id);
                //        if (Items.All(x => x.AppId != app.Id)) {
                //            avm = await CreateAppViewModel(app);
                //            Items.Add(avm);
                //        }
                        
                //    }

                //    SelectedItem = avm;
                }
            );

        #endregion
    }
}
