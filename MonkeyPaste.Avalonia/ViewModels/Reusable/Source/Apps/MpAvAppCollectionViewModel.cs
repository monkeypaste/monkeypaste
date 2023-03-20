
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIFilterMatch {
        bool IsMatch(string filter);
    }

    public class MpAvAppCollectionViewModel :
        MpViewModelBase<MpAvAppViewModel>,
        MpIAsyncSingletonViewModel<MpAvAppCollectionViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvAppViewModel> Items { get; set; } = new ObservableCollection<MpAvAppViewModel>();

        public IEnumerable<MpAvAppViewModel> FilteredItems =>
            Items
            .Where(x => (x as MpIFilterMatch).IsMatch(MpAvSettingsViewModel.Instance.FilterText));

        public MpAvAppViewModel ThisAppViewModel =>
            Items.FirstOrDefault(x => x.AppId == MpDefaultDataModelTools.ThisAppId);

        private MpAvAppViewModel _lastActiveAppViewModel;
        public MpAvAppViewModel LastActiveAppViewModel {
            get => _lastActiveAppViewModel == null ? ThisAppViewModel : _lastActiveAppViewModel;
            private set {
                if (LastActiveAppViewModel != value) {
                    _lastActiveAppViewModel = value;
                    OnPropertyChanged(nameof(LastActiveAppViewModel));
                }
            }
        }

        public MpAvAppViewModel SelectedItem { get; set; }
        #endregion

        #region State
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors

        private static MpAvAppCollectionViewModel _instance;
        public static MpAvAppCollectionViewModel Instance => _instance ??= new MpAvAppCollectionViewModel();

        public MpAvAppCollectionViewModel() : base(null) {
            //Dispatcher.UIThread.InvokeAsync(Init);
            PropertyChanged += MpAppCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }



        #endregion

        #region Public Methods
        public async Task InitAsync() {
            await Task.Delay(1);
            Dispatcher.UIThread.Post(async () => {
                //IsBusy = true;

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

                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

                await RegisterWithProcessesManager();

                OnPropertyChanged(nameof(Items));

                if (Items.Count > 0) {
                    SelectedItem = Items[0];
                }

                ValidateAppViewModels();

                //IsBusy = false;
            });
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
            if (aivm == null) {
                return null;
            }
            return aivm.ClipboardFormatInfos;
        }

        public MpAvAppViewModel GetAppByProcessInfo(MpPortableProcessInfo ppi) {
            // NOTE this assumes ppi is a running process and device is on this system
            return Items.FirstOrDefault(x => x.AppPath.ToLower() == ppi.ProcessPath.ToLower() && x.UserDeviceId == MpDefaultDataModelTools.ThisUserDeviceId);
        }

        public async Task<MpAvAppViewModel> GetOrCreateAppViewModelByProcessInfo(MpPortableProcessInfo ppi) {
            if (GetAppByProcessInfo(ppi) is MpAvAppViewModel avm) {
                return avm;
            }
            var app = await Mp.Services.AppBuilder.CreateAsync(ppi);
            if (app == null) {
                return null;
            }
            var new_avm = await CreateAppViewModel(app);
            Items.Add(new_avm);
            while (IsAnyBusy) {
                await Task.Delay(100);
            }
            return new_avm;
        }

        public MpAvAppViewModel GetAppViewModelFromScreenPoint(MpPoint gmp, double pixelDensity) {
            IntPtr handle = IntPtr.Zero;

            if (MpAvMainWindowViewModel.Instance.MainWindowScreenRect.Contains(gmp)) {
                // at least on windows (i think since its a tool window) the p/invoke doesn't return mw handle
                handle = Mp.Services.ProcessWatcher.ThisAppHandle;
            }
            if (handle == IntPtr.Zero) {
                var unscaled_gmp = gmp * pixelDensity;
                handle = Mp.Services.ProcessWatcher.GetParentHandleAtPoint(unscaled_gmp);
            }
            if (handle == IntPtr.Zero) {
                return null;
            }
            string handle_path = Mp.Services.ProcessWatcher.GetProcessPath(handle);
            MpConsole.WriteLine("Drop Path: " + handle_path, true, true);
            // TODO need to filter by current device here
            return Items.FirstOrDefault(x => x.AppPath.ToLower() == handle_path.ToLower());
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpApp a && Items.All(x => x.AppId != a.Id)) {
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
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    if (SelectedItem != null) {
                        SelectedItem.OnPropertyChanged(nameof(SelectedItem.IconId));

                        //CollectionViewSource.GetDefaultView(SelectedItem.ClipboardFormatInfos.Items).Refresh();
                        SelectedItem.ClipboardFormatInfos.OnPropertyChanged(nameof(SelectedItem.ClipboardFormatInfos.Items));
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                case nameof(LastActiveAppViewModel):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActiveProcess)));
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    break;
            }
        }
        private void ValidateAppViewModels() {
            var dups = Items.Where(x => Items.Any(y => y != x && x.IsValueEqual(y)));
            if (dups.Any()) {
                // dup app view models, check db to see if dup app model
                Debugger.Break();
            }

        }

        private async Task InitLastAppViewModel() {
            // wait for running processes to get created
            await Task.Delay(0);
            var la_pi = Mp.Services.ProcessWatcher.LastProcessInfo;
            if (la_pi == null) {
                // since application is being started from file system init LastActive to file system app
                la_pi = Mp.Services.ProcessWatcher.FileSystemProcessInfo;
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
            //PlatformWrapper.Services.ProcessWatcher.StartWatcher();

            var unknownApps = Mp.Services.ProcessWatcher.RunningProcessLookup.Keys
                                    .Where(x => Items.All(y => y.AppPath.ToLower() != x.ToLower()))
                                    .Select(x => new MpPortableProcessInfo() { ProcessPath = x }).ToList();

            MpConsole.WriteLine($"AppCollection RegisterWithProcessesManager '{unknownApps.Count}' unknown apps detected.");
            foreach (var uap in unknownApps) {
                _ = await Mp.Services.AppBuilder.CreateAsync(uap);
                // wait for db add callback to pickup db add event
                await Task.Delay(100);
                while (IsBusy) {
                    // wait for app to be added to items from db add callback
                    await Task.Delay(100);
                }
            }

            await InitLastAppViewModel();

            // wait to add activated handler until all apps at startup are syncd
            Mp.Services.ProcessWatcher.OnAppActivated += MpProcessManager_OnAppActivated;
        }

        private async void MpProcessManager_OnAppActivated(object sender, MpPortableProcessInfo e) {
            // if app is unknown add it
            // TODO device logic
            while (IsBusy) {
                await Task.Delay(100);
            }
            Dispatcher.UIThread.Post(async () => {
                var avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower());

                if (avm == null) {
                    // unknown app activated add like in registration
                    var new_app = await Mp.Services.AppBuilder.CreateAsync(e);
                    // wait for db add to pick up model
                    await Task.Delay(100);
                    while (IsBusy) {
                        // wait for vm to be added to items
                        await Task.Delay(100);
                    }

                    // BUG may need to add .ToList() on items here, hopefully fixed w/ dbAdd waiting
                    avm = Items.FirstOrDefault(x => x.AppId == new_app.Id);
                    if (avm == null) {
                        // somethings wrong check console for db add msgs
                        // this happen on initial startup sometimes, not sure why
                        // the db add callback is getting hit but add here and validate
                        Items.Add(avm);
                        OnPropertyChanged(nameof(Items));

                    }
                    ValidateAppViewModels();
                }
                LastActiveAppViewModel = avm;
            });

        }

        #endregion

        #region Commands

        public ICommand AddAppCommand => new MpAsyncCommand(
            async () => {
                string appPath = await Mp.Services.NativePathDialog.ShowFileDialogAsync(
                    title: "Select application path",
                    filters: null,
                    resolveShortcutPath: true);

                if (string.IsNullOrEmpty(appPath)) {
                    return;
                }
                var pi = new MpPortableProcessInfo() { ProcessPath = appPath };
                var avm = GetAppByProcessInfo(pi);
                if (avm == null) {
                    var app = await Mp.Services.AppBuilder.CreateAsync(pi);
                    while (avm == null) {
                        avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == appPath.ToLower());
                        await Task.Delay(300);
                    }
                } else {
                    MpNotificationBuilder.ShowMessageAsync(
                            title: "Duplicate",
                            body: $"App at path '{appPath}' already exists",
                            msgType: MpNotificationType.Message).FireAndForgetSafeAsync(this);
                }

                SelectedItem = avm;
            });

        #endregion
    }
}
