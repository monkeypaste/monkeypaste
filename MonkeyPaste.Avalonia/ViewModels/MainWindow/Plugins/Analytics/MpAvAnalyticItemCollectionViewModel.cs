using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvAnalyticItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncCollectionObject,
        MpIAsyncComboBoxViewModel,
        MpISidebarItemViewModel,
        MpIPopupMenuPicker {
        #region Private Variables

        private MpAvPluginBrowserViewModel _pluginBrowser;
        #endregion

        #region Interfaces

        #region MpIPopupMenuPicker Implementation

        public MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedAnalyticItemPresetIds, bool recursive) {
            return new MpAvMenuItemViewModel() {
                SubItems = Items.Select(x =>
                new MpAvMenuItemViewModel() {
                    Header = x.Title,
                    IconId = x.PluginIconId,
                    SubItems = x.Items.Select(y => y.GetMenu(cmd, cmdArg, selectedAnalyticItemPresetIds, recursive)).ToList()
                }).ToList()
            };
        }

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
                double w = DefaultSelectorColumnVarDimLength;
                if (SelectedPresetViewModel != null) {
                    w += DefaultParameterColumnVarDimLength;
                }
                return w;
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ObservedQueryTrayScreenHeight;
                }
                double h = DefaultSelectorColumnVarDimLength;
                //if (SelectedPresetViewModel != null) {
                //    h += _defaultParameterColumnVarDimLength;
                //}
                return h;
            }
        }
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; } = 0;

        public string SidebarBgHexColor =>
            (Mp.Services.PlatformResource.GetResource("AnalyzerSidebarBgBrush") as IBrush).ToHex();
        bool MpISidebarItemViewModel.CanResize =>
            true;
        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIAsyncComboBoxViewModel Implementation

        IEnumerable<MpIAsyncComboBoxItemViewModel> MpIAsyncComboBoxViewModel.Items => Items;
        MpIAsyncComboBoxItemViewModel MpIAsyncComboBoxViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = (MpAvAnalyticItemViewModel)value;
        }
        bool MpIAsyncComboBoxViewModel.IsDropDownOpen {
            get => IsAnalyticItemSelectorDropDownOpen;
            set => IsAnalyticItemSelectorDropDownOpen = value;
        }

        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpITreeItemViewModel ParentTreeItem => null;

        #endregion


        #region View Models

        public MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                MpCopyItemType contentType = MpAvClipTrayViewModel.Instance.SelectedItem == null ?
                    MpCopyItemType.None : MpAvClipTrayViewModel.Instance.SelectedItem.CopyItemType;
                return GetContentContextMenuItem(contentType);
            }
        }

        public IList<MpAvAnalyticItemViewModel> SortedItems =>
            Items.OrderBy(x => x.Title).ToList();
        public IEnumerable<MpAvAnalyticItemPresetViewModel> AllPresets => SortedItems.SelectMany(x => x.Items);

        public MpAvAnalyticItemPresetViewModel SelectedPresetViewModel {
            get {
                if (SelectedItem == null) {
                    return null;
                }
                return SelectedItem.SelectedItem;
            }
        }

        #endregion


        #region Layout


        public double DefaultSelectorColumnVarDimLength =>
            400;

        public double DefaultParameterColumnVarDimLength =>
            450;
        #endregion

        #region Appearance


        #endregion

        #region State

        public bool IsAnyBusy =>
            Items.Any(x => x.IsAnyBusy) || IsBusy;

        public int SelectedItemIdx {
            get => SortedItems.IndexOf(SelectedItem);
            set {
                if (SelectedItemIdx != value) {
                    SelectedItem = value < 0 || value >= SortedItems.Count ? null : SortedItems[value];
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }
        public bool IsHovering { get; set; }


        public bool IsAnalyticItemSelectorDropDownOpen { get; set; }

        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        private static MpAvAnalyticItemCollectionViewModel _instance;
        public static MpAvAnalyticItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvAnalyticItemCollectionViewModel());


        public MpAvAnalyticItemCollectionViewModel() : base(null) {
            PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;
            IsLoaded = false;
            Items.Clear();

            while (!MpPluginLoader.IsLoaded) {
                await Task.Delay(100);
            }

            var pail =
                MpPluginLoader
                .Plugins.Where(x =>
                    x.Value.Components.Any(y => y is MpIAnalyzeAsyncComponent) ||
                    x.Value.Components.Any(y => y is MpIAnalyzeComponent));
            foreach (var pai in pail) {
                var paivm = await CreateAnalyticItemViewModelAsync(pai.Value);
                if (paivm.PluginFormat == null) {
                    // internal error/invalid issue with plugin, ignore it
                    continue;
                }
                Items.Add(paivm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                // select most recent preset
                MpAvAnalyticItemPresetViewModel presetToSelect = Items
                            .AggregateOrDefault((a, b) => a.Items.Max(x => x.LastSelectedDateTime) > b.Items.Max(x => x.LastSelectedDateTime) ? a : b)
                            .Items.AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if (presetToSelect != null) {
                    presetToSelect.Parent.SelectedItem = presetToSelect;
                    SelectedItem = presetToSelect.Parent;
                }
            }

            OnPropertyChanged(nameof(SelectedItem));

            IsLoaded = true;
            IsBusy = false;
        }

        public MpAvMenuItemViewModel GetContentContextMenuItem(MpCopyItemType contentType) {
            var availItems = Items.Where(x => x.IsContentTypeValid(contentType));
            List<MpAvMenuItemViewModel> sub_items = availItems.SelectMany(x => x.QuickActionPresetMenuItems).ToList();
            if (sub_items.Count > 0) {
                sub_items.Add(new MpAvMenuItemViewModel() { IsSeparator = true });
            }
            if (availItems.Count() > 0) {

                sub_items.AddRange(availItems.Select(x => x.ContextMenuItemViewModel));
            }

            return new MpAvMenuItemViewModel() {
                Header = UiStrings.CommonAnalyzeButtonLabel,
                HasLeadingSeparator = true,
                AltNavIdx = 0,
                IconResourceKey = Mp.Services.PlatformResource.GetResource("BrainImage") as string,
                SubItems = sub_items
            };
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(Children));
                    break;
                case nameof(SelectedPresetViewModel):
                    if (SelectedPresetViewModel == null) {
                        return;
                    }
                    //CollectionViewSource.GetDefaultView(SelectedPresetViewModel.Items).Refresh();
                    SelectedPresetViewModel.OnPropertyChanged(nameof(SelectedPresetViewModel.Items));
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(SelectedPresetViewModel));
                    OnPropertyChanged(nameof(SelectedItemIdx));
                    break;
                case nameof(IsAnalyticItemSelectorDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsAnalyticItemSelectorDropDownOpen;
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SortedItems));
        }

        private async Task<MpAvAnalyticItemViewModel> CreateAnalyticItemViewModelAsync(MpPluginFormat plugin) {
            MpAvAnalyticItemViewModel aivm = new MpAvAnalyticItemViewModel(this);

            await aivm.InitializeAsync(plugin);
            return aivm;
        }

        #endregion

        #region Commands

        public ICommand ApplyCoreAnnotatorCommand => new MpCommand<object>(
            (args) => {
                var ctvm = args as MpAvClipTileViewModel;
                if (ctvm == null) {
                    return;
                }

                var core_aipvm = AllPresets
                    .FirstOrDefault(x => x.PresetGuid == MpPluginLoader.CoreAnnotatorDefaultPresetGuid);

                if (core_aipvm == null) {
                    return;
                }
                core_aipvm.Parent.ExecuteAnalysisCommand.Execute(new object[] { core_aipvm, ctvm.CopyItem });
            });

        public ICommand ShowPluginBrowserCommand => new MpCommand(
            () => {
                if (_pluginBrowser == null) {
                    _pluginBrowser = new MpAvPluginBrowserViewModel();
                }
                _pluginBrowser.OpenPluginBrowserWindow(SelectedItem == null ? null : SelectedItem.PluginGuid);
            });
        public MpIAsyncCommand<object> InstallAnalyzerCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts ||
                    argParts[0] is not string plugin_guid ||
                    argParts[1] is not string package_url) {
                    return;
                }
                var plugin_format = await MpPluginLoader.InstallPluginAsync(plugin_guid, package_url);
                if (plugin_format == null) {
                    return;
                }
                var aivm = await CreateAnalyticItemViewModelAsync(plugin_format);
                Items.Add(aivm);
            });
        public MpIAsyncCommand<object> UninstallAnalyzerCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                string plugin_guid = args as string;

                var aivm = Items.FirstOrDefault(x => x.PluginGuid == plugin_guid);

                MpDebug.Assert(aivm != null, $"Error uninstalling plugin guid '{plugin_guid}' can't find analyer");

                // NOTE assume confirm handled in calling command (plugin browser)

                while (aivm.IsBusy) {
                    // wait if executing
                    await Task.Delay(100);
                }

                var running_triggers_with_this_analyzer =
                    MpAvTriggerCollectionViewModel.Instance
                        .Items
                        .OfType<MpAvAnalyzeActionViewModel>()
                        .Where(x =>
                            aivm.Items.Any(y => y.AnalyticItemPresetId == x.AnalyticItemPresetId) &&
                            x.RootTriggerActionViewModel.SelfAndAllDescendants.Any(y => y.IsPerformingAction));

                if (running_triggers_with_this_analyzer.Any()) {
                    while (running_triggers_with_this_analyzer.Any()) {
                        // wait for any action change w/ this analyzer to complete
                        await Task.Delay(100);
                        running_triggers_with_this_analyzer =
                            MpAvTriggerCollectionViewModel.Instance
                                .Items
                                .OfType<MpAvAnalyzeActionViewModel>()
                                .Where(x =>
                                    aivm.Items.Any(y => y.AnalyticItemPresetId == x.AnalyticItemPresetId) &&
                                    x.RootTriggerActionViewModel.SelfAndAllDescendants.Any(y => y.IsPerformingAction));
                    }
                }

                // remove from db
                await Task.WhenAll(aivm.Items.Select(x => x.Preset.DeleteFromDatabaseAsync()));

                // remove from plugin dir
                MpPluginLoader.DeletePluginByGuid(aivm.PluginGuid);


                // remove from collection
                Items.Remove(aivm);
                OnPropertyChanged(nameof(Items));

                IsBusy = false;
            });

        public MpIAsyncCommand<object> UpdatePluginCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;
                string plugin_guid = (args as object[])[0] as string;
                var aivm = Items.FirstOrDefault(x => x.PluginGuid == plugin_guid);
                MpDebug.Assert(aivm != null, $"Error upgrading plugin guid '{plugin_guid}' can't find analyer");

                // backup original plugin dir
                string backup_dir = MpPluginLoader.CreatePluginBackup(plugin_guid, out string org_dir);

                if (backup_dir == null) {
                    // somthing went wrong backing up
                    IsBusy = false;
                    return;
                }

                bool success = true;
                // remove from plugin dir (retain cache)
                if (!MpPluginLoader.DeletePluginByGuid(plugin_guid, false)) {
                    success = false;
                }

                if (success) {
                    try {
                        // 
                        string package_url = (args as object[])[1] as string;
                        var updated_pf = await MpPluginLoader.InstallPluginAsync(plugin_guid, package_url);
                        await aivm.InitializeAsync(updated_pf);
                    }
                    catch (Exception ex) {
                        // if something goes wrong 
                        MpConsole.WriteTraceLine($"Error updating plugin. ", ex);
                        success = false;
                    }
                }

                if (!success) {
                    bool revert_success = false;
                    // attempt to put backup back in plugin folder
                    if (backup_dir.IsDirectory() &&
                        Directory.GetFiles(backup_dir) is string[] bfl && bfl.Length > 0) {
                        if (org_dir.IsDirectory()) {
                            // delete orginal, maybe corrupt
                            MpFileIo.DeleteDirectory(org_dir);
                        }
                        if (!org_dir.IsDirectory()) {
                            // only continue if dir was deleted

                            string backup_plugin_dir = bfl[0];
                            MpFileIo.CopyDirectory(backup_plugin_dir, MpPluginLoader.PluginRootFolderPath, true);
                            revert_success = org_dir.IsDirectory();
                        }
                    }
                    if (!revert_success) {
                        Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.FileIoWarning,
                            body: UiStrings.CommonUnknownErrorText).FireAndForgetSafeAsync();
                    }
                }
                MpFileIo.DeleteDirectory(backup_dir);

                IsBusy = false;
            });

        #endregion
    }
}
