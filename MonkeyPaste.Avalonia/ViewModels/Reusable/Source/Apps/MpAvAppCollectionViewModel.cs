
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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

    public class MpAvAppCollectionViewModel :
        MpViewModelBase<MpAvAppViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvAppViewModel> Items { get; set; } = new ObservableCollection<MpAvAppViewModel>();

        public IEnumerable<MpAvAppViewModel> FilteredItems =>
            Items
            .Where(x => (x as MpIFilterMatch).IsMatch(MpAvSettingsViewModel.Instance.FilterText));

        public IEnumerable<MpAvAppViewModel> CustomPasteItems =>
            FilteredItems
            .Where(x => x.PasteShortcutViewModel.HasPasteShortcut);

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
            Dispatcher.UIThread.VerifyAccess();

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

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            LastActiveAppViewModel = ThisAppViewModel;

            // wait to add activated handler until all apps at startup are syncd
            Mp.Services.ProcessWatcher.OnAppActivated += MpProcessManager_OnAppActivated;

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                SelectedItem = Items[0];
            }

            ValidateAppViewModels();

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
            if (aivm == null) {
                return null;
            }
            return aivm.ClipboardFormatInfos;
        }

        public MpAvAppViewModel GetAppByProcessInfo(MpPortableProcessInfo ppi) {
            if (ppi == null) {
                return null;
            }
            return
                Items
                .FirstOrDefault(x =>
                x.AppPath.ToLower() == ppi.ProcessPath.ToLower() &&
                x.UserDeviceId == MpDefaultDataModelTools.ThisUserDeviceId);
        }

        public MpAvAppViewModel GetAppViewModelFromScreenPoint(MpPoint gmp, double pixelDensity) {
            MpPortableProcessInfo ppi = null;

            if (MpAvMainWindowViewModel.Instance.MainWindowScreenRect.Contains(gmp)) {
                // at least on windows (i think since its a tool window) the p/invoke doesn't return mw handle
                //handle = Mp.Services.ProcessWatcher.ThisAppHandle;
                ppi = Mp.Services.ProcessWatcher.ThisAppProcessInfo;
            } else {
                var unscaled_gmp = gmp * pixelDensity;
                ppi = Mp.Services.ProcessWatcher.GetProcessInfoFromScreenPoint(unscaled_gmp);
            }
            return GetAppByProcessInfo(ppi);
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
            } else if (e is MpAppPasteShortcut apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomPasteItems));
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAppPasteShortcut apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomPasteItems));
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpApp a && Items.FirstOrDefault(x => x.AppId == a.Id) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    Items.Remove(avm);
                });
            } else if (e is MpAppPasteShortcut apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel shortcut_avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomPasteItems));
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
                        SelectedItem.ClipboardFormatInfos.OnPropertyChanged(nameof(SelectedItem.ClipboardFormatInfos.Items));
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                case nameof(LastActiveAppViewModel):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActiveProcess)));
                    break;
                case nameof(CustomPasteItems):
                    MpAvDataGridRefreshExtension.RefreshDataGrid(this);
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(CustomPasteItems));


            ValidateAppViewModels();
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    break;
            }
        }
        private void ValidateAppViewModels() {
            int count = Items.Count;
            List<MpAvAppViewModel> dups = null;
            for (int i = 0; i < count; i++) {
                for (int j = 0; j < count; j++) {
                    if (i == j || Items.Count <= i || Items.Count <= j) {
                        continue;
                    }
                    if (Items[i].IsValueEqual(Items[j])) {
                        if (dups == null) {
                            dups = new List<MpAvAppViewModel>();
                        }
                        dups.Add(Items[i]);
                        dups.Add(Items[j]);
                    }
                }
            }
            MpDebug.Assert(dups == null, "Dup apps found");
        }

        //private async Task InitLastAppViewModel() {
        //    // wait for running processes to get created
        //    await Task.Delay(0);
        //    var la_pi = Mp.Services.ProcessWatcher.LastProcessInfo;
        //    if (la_pi == null) {
        //        // since application is being started from file system init LastActive to file system app
        //        la_pi = Mp.Services.ProcessWatcher.FileSystemProcessInfo;
        //        if (la_pi == null) {
        //            // need to get this set on init in process watcher
        //            //MpDebug.Break();
        //        }
        //    }
        //    if (la_pi != null) {
        //        LastActiveAppViewModel = Items.FirstOrDefault(x => x.AppPath.ToLower() == la_pi.ProcessPath.ToLower());
        //        if (LastActiveAppViewModel == null) {
        //            // what's the deal?
        //            MpDebug.Break();
        //            LastActiveAppViewModel = ThisAppViewModel;
        //        }
        //    }
        //}
        //private async Task RegisterWithProcessesManager() {
        //    // This is only called during init to keep app storage in sync so any running apps are added if unknown
        //    //PlatformWrapper.Services.ProcessWatcher.StartWatcher();

        //    //var unknownApps = Mp.Services.ProcessWatcher.RunningProcessLookup.Keys
        //    //                        .Where(x => Items.All(y => y.AppPath.ToLower() != x.ToLower()))
        //    //                        .Select(x => new MpPortableProcessInfo() { ProcessPath = x }).ToList();

        //    //MpConsole.WriteLine($"AppCollection RegisterWithProcessesManager '{unknownApps.Count}' unknown apps detected.");
        //    //foreach (var uap in unknownApps) {
        //    //    _ = await Mp.Services.AppBuilder.CreateAsync(uap);
        //    //    // wait for db add callback to pickup db add event
        //    //    await Task.Delay(100);
        //    //    while (IsBusy) {
        //    //        // wait for app to be added to items from db add callback
        //    //        await Task.Delay(100);
        //    //    }
        //    //}

        //    await InitLastAppViewModel();

        //    // wait to add activated handler until all apps at startup are syncd
        //    Mp.Services.ProcessWatcher.OnAppActivated += MpProcessManager_OnAppActivated;
        //}

        private async void MpProcessManager_OnAppActivated(object sender, MpPortableProcessInfo e) {
            // if app is unknown add it
            // TODO device logic
            while (IsBusy) {
                await Task.Delay(100);
            }
            Dispatcher.UIThread.Post(async () => {
                var avm = GetAppByProcessInfo(e); //Items.FirstOrDefault(x => x.AppPath.ToLower() == e.ProcessPath.ToLower());

                if (avm == null) {
                    // unknown app activated add like in registration
                    var new_app = await Mp.Services.AppBuilder.CreateAsync(e);
                    var sw = Stopwatch.StartNew();
                    while (true) {
                        avm = GetAppByProcessInfo(e);
                        if (avm != null) {
                            break;
                        }
                        await Task.Delay(100);
                        MpDebug.Assert(sw.ElapsedMilliseconds < 10_000, $"Activating app error for process '{e}'");
                    }
                }
                LastActiveAppViewModel = avm;
            });

        }

        private async Task<MpAvAppViewModel> AddOrSelectAppFromFileDialogAsync() {
            string appPath = await Mp.Services.NativePathDialog.ShowFileDialogAsync(
                    title: "Select application path",
                    filters: null,
                    resolveShortcutPath: true);

            if (string.IsNullOrEmpty(appPath)) {
                return null;
            }
            var pi = new MpPortableProcessInfo() { ProcessPath = appPath };
            var avm = GetAppByProcessInfo(pi);
            if (avm == null) {
                var app = await Mp.Services.AppBuilder.CreateAsync(pi);
                while (avm == null) {
                    avm = Items.FirstOrDefault(x => x.AppPath.ToLower() == appPath.ToLower());
                    await Task.Delay(300);
                }
                avm.IsNew = true;
            } else {
                await Mp.Services.PlatformMessageBox.ShowOkMessageBoxAsync(
                        title: "Duplicate",
                        message: $"App at path '{appPath}' already exists",
                        iconResourceObj: "WarningImage");
            }
            return avm;
        }
        #endregion

        #region Commands

        public MpIAsyncCommand AddAppCommand => new MpAsyncCommand(
            async () => {
                var avm = await AddOrSelectAppFromFileDialogAsync();
                if (avm == null) {
                    return;
                }

                SelectAppCommand.Execute(avm);
            });

        public ICommand AddAppWithAssignPasteShortcutCommand => new MpAsyncCommand(
            async () => {
                var avm = await AddOrSelectAppFromFileDialogAsync();
                if (avm == null) {
                    // canceled app chooser dialg
                    return;
                }
                SelectAppCommand.Execute(avm);

                var assign_result = await avm.PasteShortcutViewModel.ShowAssignDialogAsync();
                if (!assign_result) {
                    //assign canceled
                    if (avm.IsNew) {
                        // remove new app if assignment was canceled
                        await avm.App.DeleteFromDatabaseAsync();
                    }
                }
            });

        public ICommand SelectAppCommand => new MpCommand<object>(
            (args) => {
                int appId = 0;
                if (args is int) {
                    appId = (int)args;
                } else if (args is MpAvAppViewModel avm) {
                    appId = avm.AppId;
                }

                if (appId <= 0) {
                    return;
                }
                SelectedItem = Items.FirstOrDefault(x => x.AppId == appId);
            });
        public ICommand ShowAppSelectorFlyoutCommand => new MpCommand<object>(
            (args) => {
                var appFlyout = new MenuFlyout() {
                    ItemsSource =
                        Items
                        .Where(x => !CustomPasteItems.Contains(x) && !string.IsNullOrWhiteSpace(x.AppName))
                        .OrderBy(x => x.AppName)
                        .Select(x => new MenuItem() {
                            Icon = new Image() {
                                Width = 20,
                                Height = 20,
                                Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(x.IconId, null, null, null) as Bitmap
                            },
                            Header = x.AppName,
                            Command = AssignPasteShortcutCommand,
                            CommandParameter = x
                        }).AsEnumerable<object>()
                        .Union(new object[] {
                            new Separator(),
                            new MenuItem() {
                                Icon = new Image() {
                                    Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert("Dots3x1Image", null, null, null) as Bitmap,
                                    RenderTransform = new RotateTransform() {
                                        Angle = 90
                                    }
                                },
                                Header = "Add App",
                                Command = AddAppWithAssignPasteShortcutCommand
                            }
                        }).ToList()
                };
                var ddb = args as DropDownButton;
                Flyout.SetAttachedFlyout(ddb, appFlyout);
                Flyout.ShowAttachedFlyout(ddb);
            });

        public ICommand DeletePasteShortcutCommand => new MpAsyncCommand<object>(
            async (args) => {
                var avm = args as MpAvAppViewModel;
                if (avm == null) {
                    return;
                }
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                    title: $"Confirm",
                    message: $"Are you sure want to remove the paste shortcut for '{avm.AppName}'",
                    iconResourceObj: avm.IconId);
                if (!result) {
                    // canceled
                    return;
                }
                avm.PasteShortcutViewModel.PasteCmdKeyString = null;
            });

        public ICommand AssignPasteShortcutCommand => new MpAsyncCommand<object>(
            async (args) => {
                var avm = args as MpAvAppViewModel;
                if (avm == null) {
                    return;
                }
                await avm.PasteShortcutViewModel.ShowAssignDialogAsync();
            });

        public ICommand ToggleIsRejectedCommand => new MpAsyncCommand<object>(
            async (args) => {
                var avm = args as MpAvAppViewModel;
                if (avm == null) {
                    return;
                }
                avm.IsRejected = !avm.IsRejected;
                if (avm.IsRejected) {
                    bool was_confirmed = await avm.VerifyRejectAsync();
                    if (!was_confirmed) {
                        // canceled from delete content msgbox
                        avm.IsRejected = false;
                    }
                }
            });
        #endregion
    }
}
