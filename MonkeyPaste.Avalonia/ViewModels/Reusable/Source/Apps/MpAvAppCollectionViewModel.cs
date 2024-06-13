using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvAppCollectionViewModel :
        MpAvViewModelBase<MpAvAppViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvAppViewModel> Items { get; set; } = new ObservableCollection<MpAvAppViewModel>();

        public IEnumerable<MpAvAppViewModel> FilteredItems =>
            Items
            .Where(x => (x as MpIFilterMatch).IsFilterMatch(MpAvSettingsViewModel.Instance.FilterText));

        public IEnumerable<MpAvAppViewModel> FilteredExternalItems =>
            FilteredItems.Where(x => !x.IsThisApp);

        public IEnumerable<MpAvAppViewModel> CustomClipboardShortcutItems =>
            FilteredExternalItems
            .Where(x => x.HasAnyShortcut);

        public IEnumerable<MpAvAppViewModel> CustomClipboardFormatItems =>
            FilteredExternalItems
            .Where(x => x.HasCustomOle);

        public IEnumerable<MpAvAppViewModel> RejectedItems =>
            FilteredExternalItems
            .Where(x => x.IsRejected);

        public MpAvAppViewModel ThisAppViewModel =>
            Items.FirstOrDefault(x => x.AppId == MpDefaultDataModelTools.ThisAppId);


        public MpAvAppViewModel SelectedCustomClipboardFormatItem { get; set; }
        public MpAvAppViewModel SelectedItem { get; set; }
        #endregion

        #region State
        public bool CanRejectApps =>
            !MpAvThemeViewModel.Instance.IsMobileOrWindowed;
        public bool CanNavigateToApp =>
            !MpAvThemeViewModel.Instance.IsMobileOrWindowed;
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        public bool DoSelectedPulse {
            get {
                if (SelectedItem == null) {
                    return false;
                }
                return SelectedItem.DoFocusPulse;
            }
        }
        public bool IsCustomClipboardDataGridExpanded { get; set; }
        #endregion

        #endregion

        #region Constructors

        private static MpAvAppCollectionViewModel _instance;
        public static MpAvAppCollectionViewModel Instance => _instance ??= new MpAvAppCollectionViewModel();

        public MpAvAppCollectionViewModel() : base(null) {
            //Dispatcher.UIThread.InvokeAsync(CheckEnumUiStrings);
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

            // wait to add activated handler until all apps at startup are syncd

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                SelectedItem = Items[0];
            }

            ValidateAppViewModels();
            UpdateComponentSources();

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

        public MpAvAppOleFormatInfoCollectionViewModel GetInteropSettingByAppId(int appId) {
            var aivm = Items.FirstOrDefault(x => x.AppId == appId);
            if (aivm == null) {
                return null;
            }
            return aivm.OleFormatInfos;
        }

        public async Task<MpAvAppViewModel> AddOrGetAppByArgAsync(object arg) {
            // NOTE arg should only process info or an app
            if (arg is MpPortableProcessInfo ppi) {
                var result = await AddOrGetAppByProcessInfoAsync(ppi);
                return result;
            }
            if (arg is MpAvAppViewModel avm) {
                return avm;
            }
            return null;
        }

        public async Task<MpAvAppViewModel> AddOrGetAppByProcessInfoAsync(MpPortableProcessInfo ppi) {
            if (ppi == null) {
                return null;
            }
            if (GetAppByProcessInfo(ppi) is MpAvAppViewModel existing_avm) {
                return existing_avm;
            }
            var sw = Stopwatch.StartNew();
            var app = await Mp.Services.AppBuilder.CreateAsync(ppi);
            var avm = GetAppByProcessInfo(ppi);
            while (avm == null) {
                await Task.Delay(100);
                avm = GetAppByProcessInfo(ppi);
                MpDebug.Assert(sw.ElapsedMilliseconds < 5_000, $"Add app timeout for pi '{ppi}'");
            }
            return avm;
        }

        public MpAvAppViewModel GetAppByProcessInfo(MpPortableProcessInfo ppi) {
            if (ppi == null) {
                return null;
            }
            return
                Items
                .FirstOrDefault(x =>
                x.ToProcessInfo().IsValueEqual(ppi) &&
                x.UserDeviceId == MpDefaultDataModelTools.ThisUserDeviceId);
        }

        public string GetAppClipboardKeysByProcessInfo(MpPortableProcessInfo pi, bool isCopy) {
            string keys = isCopy ?
                        Mp.Services.PlatformShorcuts.CopyKeys :
                        Mp.Services.PlatformShorcuts.PasteKeys;
            if (GetAppByProcessInfo(pi) is MpAvAppViewModel avm) {
                if (isCopy && avm.CopyShortcutViewModel.HasShortcut) {
                    keys = avm.CopyShortcutViewModel.ShortcutCmdKeyString;
                } else if (!isCopy && avm.PasteShortcutViewModel.HasShortcut) {
                    keys = avm.PasteShortcutViewModel.ShortcutCmdKeyString;
                }
            }
            return keys;
        }

        public int[] GetAppCustomOlePresetsByWatcherState(bool isRead, bool isDnd, ref MpPortableProcessInfo active_pi) {
            if (active_pi == null) {
                // is only not null for dnd updates atm
                if (isDnd) {
                    if (isRead) {
                        active_pi = Mp.Services.DragProcessWatcher.DragProcess;
                    } else {
                        // this should probably not be used (yet) cause
                        // cause its handle by drop widget?
                        active_pi = Mp.Services.DropProcessWatcher.DropProcess;
                    }
                } else {
                    active_pi = Mp.Services.ProcessWatcher.LastProcessInfo;
                }
            }
            return GetAppCustomOlePresetsByProcessInfo(active_pi, isRead);
        }

        public int[] GetAppCustomOlePresetsByProcessInfo(MpPortableProcessInfo pi, bool isRead) {
            if (GetAppByProcessInfo(pi) is not MpAvAppViewModel app_vm) {
                return null;
            }
            if (isRead) {
                if (app_vm.OleFormatInfos.IsReaderDefault) {
                    return null;
                }
                if (app_vm.OleFormatInfos.IsReadersOnlyNoOp) {
                    return new int[] { };
                }
                return app_vm.OleFormatInfos.Readers.Select(x => x.PresetId).ToArray();
            }
            // write presets
            if (app_vm.OleFormatInfos.IsWriterDefault) {
                return null;
            }
            if (app_vm.OleFormatInfos.IsWritersOnlyNoOp) {
                return new int[] { };
            }
            return app_vm.OleFormatInfos.Writers.Select(x => x.PresetId).ToArray();
        }


        public void UpdateComponentSources() {
            OnPropertyChanged(nameof(CustomClipboardFormatItems));
            OnPropertyChanged(nameof(CustomClipboardShortcutItems));
            OnPropertyChanged(nameof(RejectedItems));
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
                    UpdateComponentSources();
                });
            } else if (e is MpAppClipboardShortcuts apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    UpdateComponentSources();
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAppClipboardShortcuts apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomClipboardShortcutItems));
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpApp a && Items.FirstOrDefault(x => x.AppId == a.Id) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    Items.Remove(avm);
                });
            } else if (e is MpAppClipboardShortcuts apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel shortcut_avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomClipboardShortcutItems));
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
                        SelectedItem.OleFormatInfos.OnPropertyChanged(nameof(SelectedItem.OleFormatInfos.Items));
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
                //case nameof(LastActiveAppViewModel):
                //    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActiveProcess)));
                //    break;
                case nameof(CustomClipboardShortcutItems):
                    MpAvDataGridRefreshExtension.RefreshDataGrid(this);
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredExternalItems));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(CustomClipboardShortcutItems));


            ValidateAppViewModels();
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SettingsFilterTextChanged:
                    OnPropertyChanged(nameof(FilteredItems));
                    OnPropertyChanged(nameof(FilteredExternalItems));
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
            MpDebug.Assert(dups == null, "Dup apps found", true);
        }


        private MpAvAppOleRootMenuViewModel GetPasteInfoMenuItemsByProcessInfo(MpPortableProcessInfo ppi, string show_type) {
            object menuArg = ppi;
            if (GetAppByProcessInfo(ppi) is MpAvAppViewModel avm) {
                menuArg = avm;
            }

            var root_menu = new MpAvAppOleRootMenuViewModel(menuArg, show_type);
            return root_menu;
        }

        private async Task AddOrRemoveAppComponentByTypeAsync(MpAvAppViewModel avm, string comp_type, bool isRemove) {
            if (isRemove) {
                switch (comp_type) {
                    case "shortcuts":
                        if (avm.ClipboardShortcutsId == 0) {
                            break;
                        }
                        var cs = await MpDataModelProvider.GetItemAsync<MpAppClipboardShortcuts>(avm.ClipboardShortcutsId);
                        if (cs == null) {
                            // nothign to remove
                            break;
                        }
                        await cs.DeleteFromDatabaseAsync();
                        break;
                    case "formats":
                        await avm.OleFormatInfos.RemoveCustomInfosCommand.ExecuteAsync();
                        break;
                    case "rejects":
                        await avm.ToggleIsRejectedCommand.ExecuteAsync();
                        break;
                }
            } else {
                // add
                switch (comp_type) {
                    case "shortcuts":
                        _ = await MpAppClipboardShortcuts.CreateAsync(appId: avm.AppId);
                        break;
                    case "formats":
                        await avm.OleFormatInfos.CreateDefaultInfosCommand.ExecuteAsync();
                        break;
                    case "rejects":
                        await avm.ToggleIsRejectedCommand.ExecuteAsync();
                        break;
                }

            }


            await avm.InitializeAsync(avm.App);
            UpdateComponentSources();
        }
        #endregion

        #region Commands
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

        public MpIAsyncCommand<object> RemoveAppComponentCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts) {
                    return;
                }
                var avm = argParts[0] as MpAvAppViewModel;
                string comp_type_to_remove = argParts[1].ToStringOrEmpty();

                string confirm_body = null;
                switch (comp_type_to_remove) {
                    case "rejects":
                        // no confirm
                        break;
                    case "shortcuts":
                        confirm_body = UiStrings.SettingsInteropAppConfirmRemoveShortcutsBody;
                        break;
                    case "formats":
                        confirm_body = UiStrings.SettingsInteropAppConfirmRemoveFormatsBody;
                        break;
                }
                if (confirm_body != null) {
                    bool confirmed = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                        title: UiStrings.CommonConfirmLabel,
                        message: string.Format(confirm_body, avm.AppDisplayName),
                        iconResourceObj: "QuestionMarkImage");
                    if (!confirmed) {
                        // canceled
                        return;
                    }
                }

                await AddOrRemoveAppComponentByTypeAsync(avm, comp_type_to_remove, true);
                if (comp_type_to_remove == "formats") {
                    MpAvClipTrayViewModel.Instance.UpdatePasteInfoMessageCommand.Execute(null);
                }
            });

        public MpIAsyncCommand<object> AddAppComponentCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpPortableProcessInfo pi_to_add = null;

                string comp_type_to_add = null;
                if (args is string) {
                    // show picker
                    comp_type_to_add = args.ToString();
                } else if (args is object[] argParts) {
                    pi_to_add = argParts[0] as MpPortableProcessInfo;
                    comp_type_to_add = argParts[1].ToString();
                }
                if (comp_type_to_add == null) {
                    return;
                }
                if (pi_to_add == null) {
                    List<FilePickerFileType> app_filter = null;
                    if (OperatingSystem.IsWindows()) {
                        app_filter = new List<FilePickerFileType>
                            {
                                new(UiStrings.CommonApplicationLabel)
                                {
                                    Patterns = new[] { "*.exe","*.lnk" }
                                }
                            };
                    }
                    string appPath = await Mp.Services.NativePathDialog.ShowFileDialogAsync(
                        title: UiStrings.SettingsInteropAppBrowseToAppPickerTitle,
                        filters: app_filter,
                        resolveShortcutPath: true);

                    if (string.IsNullOrEmpty(appPath)) {
                        // canceled
                        return;
                    }
                    pi_to_add = MpPortableProcessInfo.FromPath(appPath);
                }
                if (pi_to_add == null) {
                    return;
                }

                var avm = await AddOrGetAppByProcessInfoAsync(pi_to_add);

                await AddOrRemoveAppComponentByTypeAsync(avm, comp_type_to_add, false);

                SelectAppCommand.Execute(avm);
            });


        public ICommand ShowAddAppPopupMenuCommand => new MpCommand<object>(
            (args) => {
                Control source_control = args as Control;
                string add_type = "any";
                if (source_control == null &&
                    args is object[] argParts) {
                    source_control = argParts[0] as Control;
                    add_type = argParts[1] as string;
                }
                if (source_control == null) {
                    return;
                }

                // always show running apps minus whats present by add type
                IEnumerable<MpAvAppViewModel> exisiting_avml = null;
                switch (add_type) {
                    case "shortcuts":
                        exisiting_avml = CustomClipboardShortcutItems;
                        break;
                    case "formats":
                        exisiting_avml = CustomClipboardFormatItems;
                        break;
                    case "rejects":
                        exisiting_avml = RejectedItems;
                        break;
                }
                var awpil = Mp.Services.ProcessWatcher.AllWindowProcessInfos;

                var mivml = new MpAvMenuItemViewModel() {
                    SubItems =
                        awpil
                        .Where(x => exisiting_avml.All(y => !y.ToProcessInfo().IsValueEqual(x)) && !x.IsValueEqual(ThisAppViewModel.ToProcessInfo()))
                        .OrderBy(x => x.ApplicationName)
                        .Select(x => new MpAvMenuItemViewModel() {
                            IconSourceObj = Mp.Services.IconBuilder.GetPathIconBase64(x.ProcessPath, x.Handle, MpIconSize.MediumIcon32),
                            Header = x.ApplicationName,
                            Command = AddAppComponentCommand,
                            CommandParameter = new object[] { x, add_type }
                        }).Union(new[] {
                            new MpAvMenuItemViewModel() {
                                HasLeadingSeparator = true,
                                IconSourceObj = "Dots1x3Image",
                                Header = UiStrings.SettingsInteropAppBrowseToAppMenuItemLabel,
                                Command = AddAppComponentCommand,
                                CommandParameter = add_type
                            }
                        }).ToList()
                };
                MpAvMenuView.ShowMenu(
                    target: source_control,
                    dc: mivml,
                    showByPointer: false,
                    placementMode: PlacementMode.RightEdgeAlignedTop,
                    popupAnchor: PopupAnchor.TopLeft);
            });
        public MpAvAppOleRootMenuViewModel CurAppOleMenuViewModel =>
            GetPasteInfoMenuItemsByProcessInfo(MpAvClipTrayViewModel.Instance.PasteProcessInfo, "full");

        public ICommand ShowAppPresetsContextMenuCommand => new MpCommand<object>(
            (args) => {
                string show_type = "full";
                if (args is not IEnumerable<object> args2 ||
                    args2.ToArray() is not { } argParts ||
                    argParts[0] is not Control source_control ||
                    argParts[1] is not MpPortableProcessInfo pi) {
                    return;
                }
                if (argParts.OfType<string>().FirstOrDefault() is { } show_arg) {
                    show_type = show_arg;
                }
                MpPoint offset = argParts.OfType<MpPoint>().FirstOrDefault() ?? MpPoint.Zero;

                MpAvAppOleRootMenuViewModel mivm = GetPasteInfoMenuItemsByProcessInfo(pi, show_type);
                Control anchor_control = source_control;

                if (source_control.GetVisualAncestor<MpAvOleFormatStripView>() is MpAvOleFormatStripView ofsv) {
                    // settings format strip click
                    // provided anchor_control is clicked button, use strip as anchor_control
                    anchor_control = ofsv;
                }

                var cm = MpAvMenuView.ShowMenu(
                    anchor_control,
                    mivm,
                    showByPointer: false,
                    PlacementMode.TopEdgeAlignedLeft,
                    PopupAnchor.BottomRight,
                    offset);

                void _cmInstance_MenuClosed(object sender, RoutedEventArgs e) {
                    cm.Closed -= _cmInstance_MenuClosed;

                    if (source_control.DataContext is MpAvClipTileViewModel ctvm &&
                        ctvm.GetContentView() is MpAvContentWebView cwv) {
                        cwv.SendMessage("unexpandPasteButtonPopup_ext()");
                    }
                }
                cm.Closed += _cmInstance_MenuClosed;
            },
            (args) => {
                return CanRejectApps;
            });

        #endregion
    }
}
