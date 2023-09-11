using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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

        public IEnumerable<MpAvAppViewModel> CustomClipboardItems =>
            FilteredItems
            .Where(x => x.HasAnyShortcut);

        public MpAvAppViewModel ThisAppViewModel =>
            Items.FirstOrDefault(x => x.AppId == MpDefaultDataModelTools.ThisAppId);

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

            // wait to add activated handler until all apps at startup are syncd

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

        public MpAvAppOleFormatInfoCollectionViewModel GetInteropSettingByAppId(int appId) {
            var aivm = Items.FirstOrDefault(x => x.AppId == appId);
            if (aivm == null) {
                return null;
            }
            return aivm.OleFormatInfos;
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
                x.AppPath.ToLower() == ppi.ProcessPath.ToLower() &&
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
            } else if (e is MpAppClipboardShortcuts apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomClipboardItems));
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAppClipboardShortcuts apsc &&
                Items.FirstOrDefault(x => x.AppId == apsc.AppId) is MpAvAppViewModel avm) {
                Dispatcher.UIThread.Post(() => {
                    OnPropertyChanged(nameof(CustomClipboardItems));
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
                    OnPropertyChanged(nameof(CustomClipboardItems));
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
                case nameof(CustomClipboardItems):
                    MpAvDataGridRefreshExtension.RefreshDataGrid(this);
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(CustomClipboardItems));


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

        private MpAvIMenuItemViewModel GetPasteInfoMenuItemsByProcessInfo(MpPortableProcessInfo ppi) {
            object menuArg = ppi;
            if (GetAppByProcessInfo(ppi) is MpAvAppViewModel avm) {
                menuArg = avm;
            }

            var root_menu = new MpAvAppOleRootMenuViewModel(menuArg);
            return root_menu;
        }
        private MpAvMenuItemViewModel GetPasteInfoMenuItemsByProcessInfo_old(MpPortableProcessInfo ppi) {
            IBrush enabled_brush =
                Mp.Services.PlatformResource
                .GetResource<IBrush>(MpThemeResourceKey.ThemeAccent3Color.ToString())
                .AdjustOpacity(0.3);

            IBrush contains_enabled_brush =
                Mp.Services.PlatformResource
                .GetResource<IBrush>(MpThemeResourceKey.ThemeAccent4Color.ToString())
                .AdjustOpacity(0.3);

            MpAvAppViewModel avm = GetAppByProcessInfo(ppi);

            bool IsPresetEnabled(MpAvClipboardFormatPresetViewModel y, MpAvAppViewModel avm) {
                if (avm == null) {
                    return y.IsEnabled;
                }
                return avm.OleFormatInfos.IsFormatEnabledByPresetId(y.PresetId);
            }

            bool IsPluginContainEnabled(MpAvClipboardHandlerItemViewModel handler_plugin, string format, bool isReader, MpAvAppViewModel avm) {
                var plugin_reader_or_writer_presets_by_format =
                    handler_plugin.Items.Where(x => x.IsReader == isReader && x.HandledFormat == format).SelectMany(x => x.Items);
                if (avm == null) {
                    return plugin_reader_or_writer_presets_by_format.Any(x => x.IsEnabled);
                }
                return plugin_reader_or_writer_presets_by_format.Any(x => avm.OleFormatInfos.IsFormatEnabledByPresetId(x.PresetId));
            }

            bool IsFormatContainEnabled(string format, bool isReader, MpAvAppViewModel avm) {
                var all_reader_or_writer_presets_by_format =
                    MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets
                    .Where(x => x.IsReader == isReader && x.ClipboardFormat.formatName == format);

                if (avm == null) {
                    return all_reader_or_writer_presets_by_format.Any(x => x.IsEnabled);
                }
                return all_reader_or_writer_presets_by_format.Any(x => avm.OleFormatInfos.IsFormatEnabledByPresetId(x.PresetId));
            }

            MpAvMenuItemViewModel GetPresetMenuItem(MpAvClipboardFormatPresetViewModel y) {
                // NOTE when process is unknown or has no info, show default
                // NOTE2 when process is known and HAS infos, only reflect its info no default

                return new MpAvMenuItemViewModel() {
                    Identifier = y,
                    Header = y.Label,
                    //IconSourceObj = y.IconId,
                    //CheckedItemBgColor = enabled_brush,
                    IconBorderHexColor = MpSystemColors.Black,
                    IsCheckable = true,
                    IsChecked = IsPresetEnabled(y, avm),
                    Command = y.TogglePresetIsEnabledCommand,
                    CommandParameter = avm == null ? ppi : avm
                };
            }

            List<MpAvMenuItemViewModel> GetPluginPresets(bool isReader, MpAvHandledClipboardFormatViewModel hcfvm) {
                var plugin_presets = hcfvm.Items.Where(x => x.IsReader == isReader).OrderBy(x => x.Label);

                var plugin_presets_results = plugin_presets.Select(x => GetPresetMenuItem(x)).ToList();
                plugin_presets_results.Add(new MpAvMenuItemViewModel() {
                    HasLeadingSeperator = true,
                    IconSourceObj = "CogImage",
                    Header = "Manage...",
                    Command = hcfvm.ManageClipboardHandlerCommand
                });
                return plugin_presets_results;
            }

            List<MpAvMenuItemViewModel> GetPluginsTree(bool isReader, string format) {
                var presets = MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets.Where(x => x.IsReader == isReader && x.ClipboardFormat.formatName == format);
                var plugins = presets
                    .Select(x => x.Parent.Parent)
                    .Distinct()
                    .OrderBy(x => x.HandlerName)
                    .Select(x => new MpAvMenuItemViewModel() {
                        Identifier = x,
                        TagObj = isReader,
                        //IconSourceObj = x.PluginIconId,
                        //ItemBgColor = IsPluginContainEnabled(x, format, isReader, avm) ? contains_enabled_brush : Brushes.Transparent,
                        IsCheckable = true,
                        IsChecked = IsPluginContainEnabled(x, format, isReader, avm) ? null : false,
                        IconBorderHexColor = MpSystemColors.Black,
                        Header = x.HandlerName,
                        SubItems = GetPluginPresets(isReader, x.Items.FirstOrDefault(x => x.IsReader == isReader && x.HandledFormat == format))
                    }).ToList();
                return plugins;
            }

            List<MpAvMenuItemViewModel> GetFormatsTree(bool isReader) {
                var presets = MpAvClipboardHandlerCollectionViewModel.Instance.AllPresets.Where(x => x.IsReader == isReader);

                var formats = presets
                    .Select(x => x.ClipboardFormat.formatName)
                    .Distinct()
                    .OrderBy(x => x);

                var flat_formats = formats
                        .Select(x => new MpAvMenuItemViewModel() {
                            TagObj = presets.FirstOrDefault(y => y.ClipboardFormat.formatName == x).Parent.IsPrimaryFormat,
                            //IconSourceObj = presets.FirstOrDefault(y => y.ClipboardFormat.formatName == x).IconId,
                            //ItemBgColor = IsFormatContainEnabled(x, isReader, avm) ? contains_enabled_brush : Brushes.Transparent,
                            Header = x,
                            IconBorderHexColor = MpSystemColors.Black,
                            IsCheckable = true,
                            IsChecked = IsFormatContainEnabled(x, isReader, avm) ? null : false,
                            SubItems = GetPluginsTree(isReader, x)
                        }).ToList();

                //var more_mi = new MpAvMenuItemViewModel() {
                //    IconResourceKey = "PlusSolidImage",
                //    IconTintHexStr = Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeGrayAccent1Color.ToString()),
                //    Header = "More...",
                //    ItemBgColor = flat_formats.Where(x => ((bool)x.TagObj) == false).Any(x => IsFormatContainEnabled(x.Header, isReader, avm)) ?
                //        contains_enabled_brush : Brushes.Transparent,
                //    SubItems = flat_formats.Where(x => ((bool)x.TagObj) == false).ToList()
                //};
                //return flat_formats
                //        .Where(x => (bool)x.TagObj)
                //        .Union(new[] { more_mi }).ToList();
                return flat_formats;
            }

            /*
            Structure 

            Readers
            |__Text
                |__<Plugin Name>
                    |__<Preset Name>
                    |__Manage..
            |__Image
            |__Files
            |__More...

            Writers
            |__Text
                |__<Plugin Name>
                    |__<Preset Name>
                    |__Manage..
            |__Image
            |__Files
            |__More...
            */
            var readers = GetFormatsTree(true);
            var writers = GetFormatsTree(false);
            return new MpAvMenuItemViewModel() {
                SubItems = new List<MpAvMenuItemViewModel>() {
                    new MpAvMenuItemViewModel() {
                        IconResourceKey = "GlassesImage",
                        IconTintHexStr = MpSystemColors.mintcream,
                        Header = "Readers",
                        SubItems = readers
                    },
                    new MpAvMenuItemViewModel() {
                        HasLeadingSeperator = true,
                        IconResourceKey = "PenImage",
                        IconTintHexStr = MpSystemColors.peachpuff3,
                        Header = "Writers",
                        SubItems = writers
                    },
                }
            };
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

        public ICommand AddAppWithAssignClipboardShortcutCommand => new MpAsyncCommand(
            async () => {
                var avm = await AddOrSelectAppFromFileDialogAsync();
                if (avm == null) {
                    // canceled app chooser dialg
                    return;
                }
                AddOrUpdateAppClipboardShortcutCommand.Execute(avm);
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
                        .Where(x => !CustomClipboardItems.Contains(x) && !string.IsNullOrWhiteSpace(x.AppName))
                        .OrderBy(x => x.AppName)
                        .Select(x => new MenuItem() {
                            Icon = new Image() {
                                Width = 20,
                                Height = 20,
                                Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(x.IconId, null, null, null) as Bitmap
                            },
                            Header = x.AppName,
                            Command = AddOrUpdateAppClipboardShortcutCommand,
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
                                Command = AddAppWithAssignClipboardShortcutCommand
                            }
                        }).ToList()
                };
                var ddb = args as DropDownButton;
                Flyout.SetAttachedFlyout(ddb, appFlyout);
                Flyout.ShowAttachedFlyout(ddb);
            });

        public ICommand DeleteAppClipboardShortcutsCommand => new MpAsyncCommand<object>(
            async (args) => {
                var avm = args as MpAvAppViewModel;
                if (avm == null) {
                    return;
                }
                MpDebug.Assert(avm.HasAnyShortcut, $"'avm' should has clipboard shortcuts to delete");

                var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                    title: $"Confirm",
                    message: $"Are you sure want to remove the paste shortcut for '{avm.AppName}'",
                    iconResourceObj: avm.IconId);
                if (!result) {
                    // canceled
                    return;
                }
                await avm.PasteShortcutViewModel.ClipboardShortcuts.DeleteFromDatabaseAsync();
                await avm.InitializeAsync(avm.App);
                OnPropertyChanged(nameof(CustomClipboardItems));
            });

        public ICommand AddOrUpdateAppClipboardShortcutCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is MpAvAppViewModel avm) {
                    // app selected from shortcut dropdown
                    MpDebug.Assert(!CustomClipboardItems.Contains(avm), $"{avm} should have been filtered out from this menu");
                    await MpAppClipboardShortcuts.CreateAsync(
                        appId: avm.AppId);
                    await avm.InitializeAsync(avm.App);

                    OnPropertyChanged(nameof(CustomClipboardItems));

                    SelectAppCommand.Execute(avm);
                    return;
                }
                if (args is not MpAvAppClipboardShortcutViewModel acsvm) {
                    return;
                }
                await acsvm.ShowAssignDialogAsync();
            });

        public ICommand ShowAppPresetsContextMenuCommand => new MpCommand<object>(
            (args) => {
                if (args is not object[] argParts ||
                    argParts[0] is not Control c ||
                    argParts[1] is not MpPortableProcessInfo pi ||
                    argParts[2] is not MpPoint offset) {
                    return;
                }

                void _cmInstance_MenuClosed(object sender, RoutedEventArgs e) {
                    if (c.DataContext is MpAvClipTileViewModel ctvm &&
                        ctvm.GetContentView() is MpAvContentWebView cwv) {
                        cwv.SendMessage("unexpandPasteButtonPopup_ext()");
                    }
                    MpAvContextMenuView.Instance.Closed -= _cmInstance_MenuClosed;
                }

                var cm = MpAvMenuView.ShowMenu(
                    c,
                    GetPasteInfoMenuItemsByProcessInfo(pi),
                    PlacementMode.TopEdgeAlignedLeft,
                    PopupAnchor.BottomRight,
                    offset);
                cm.Closed += _cmInstance_MenuClosed;
            });
        #endregion
    }
}
