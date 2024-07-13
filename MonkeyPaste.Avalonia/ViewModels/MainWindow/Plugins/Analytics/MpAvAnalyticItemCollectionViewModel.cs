using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public interface MpIManagePluginComponents {
        Task<bool> InstallAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm);
        Task<bool> BeginUpdateAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm);
        Task<bool> UninstallAsync(string pluginGuid);
    }

    public class MpAvAnalyticItemCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvAnalyticItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncCollectionObject,
        MpIManagePluginComponents,
        MpIAsyncComboBoxViewModel,
        MpISidebarItemViewModel,
        MpIPopupMenuPicker {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIManagePluginComponents Implementation

        async Task<bool> MpIManagePluginComponents.InstallAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {
            bool success = await MpPluginLoader.InstallPluginAsync(pluginGuid, packageUrl, false, cpivm);
            if (success) {
                IsBusy = true;

                var aivm = await CreateAnalyticItemViewModelAsync(pluginGuid);
                Items.Add(aivm);

                IsBusy = false;
            }

            return success;
        }
        async Task<bool> MpIManagePluginComponents.UninstallAsync(string plugin_guid) {
            if (Items.FirstOrDefault(x => x.PluginGuid == plugin_guid) is not { } aivm) {
                MpDebug.Break($"Error uninstalling plugin guid '{plugin_guid}' can't find analyer");
                return false;
            }
            IsBusy = true;
            // NOTE assume confirm handled in calling command (plugin browser)
            await RemoveAnalyzerReferencesAsync(aivm, true);
            // clear local ref
            aivm = null;

            // remove from plugin dir
            bool success = await MpPluginLoader.DeletePluginByGuidAsync(plugin_guid);

            IsBusy = false;
            if (SelectedItem == null) {
                SelectedItem = SortedItems.FirstOrDefault();
            }
            return success;
        }

        async Task<bool> MpIManagePluginComponents.BeginUpdateAsync(string plugin_guid, string package_url, MpICancelableProgressIndicatorViewModel cpivm) {
            if (Items.FirstOrDefault(x => x.PluginGuid == plugin_guid) is not { } aivm) {
                MpDebug.Break($"Error updating plugin guid '{plugin_guid}' can't find analyer");
                return false;
            }
            IsBusy = true;

            await RemoveAnalyzerReferencesAsync(aivm, false);

            bool can_reload = await MpPluginLoader.BeginUpdatePluginAsync(plugin_guid, package_url, cpivm);
            if (can_reload) {
                can_reload = await AddOrReplaceAnalyzerViewModelByGuidAsync(plugin_guid);
            }
            IsBusy = false;
            return can_reload;
        }

        #endregion

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
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Width;
                }
                double def_w = DefaultSelectorColumnVarDimLength;
                if (SelectedPresetViewModel != null) {
                    def_w += DefaultParameterColumnVarDimLength;
                }
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    def_w = Math.Min(def_w, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width / 2);
                }
                return def_w;
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Height;
                }
                double def_h = DefaultSelectorColumnVarDimLength;
                if (MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    def_h = Math.Min(def_h, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Height / 3);
                }
                //if (SelectedPresetViewModel != null) {
                //    h += _defaultParameterColumnVarDimLength;
                //}
                return def_h;
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

        public override MpAvAnalyticItemViewModel SelectedItem {
            get => base.SelectedItem;
            set => base.SelectedItem = value;
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
            MpPluginLoader.Plugins.CollectionChanged += MpPluginLoader_Plugins_CollectionChanged;

            var analyzer_guids =
                MpPluginLoader.PluginManifestLookup.Where(x => x.Value.analyzer != null).Select(x => x.Value.guid);

            foreach (var analyzer_guid in analyzer_guids) {
                await AddOrReplaceAnalyzerViewModelByGuidAsync(analyzer_guid);
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
        private void MpPluginLoader_Plugins_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            // This only removes plugins not found in loader
            var plugin_guids_to_remove = Items.Where(x => !MpPluginLoader.PluginGuidLookup.ContainsKey(x.PluginGuid)).Select(x => x.PluginGuid).ToList();
            foreach (string guid in plugin_guids_to_remove) {
                // this should only find matches when plugins were removed for invalidation(s) during install. 
                if (Items.FirstOrDefault(x => x.PluginGuid == guid) is { } aivm) {
                    RemoveAnalyzerReferencesAsync(aivm, false).FireAndForgetSafeAsync();
                }
            }
        }

        private async Task<bool> AddOrReplaceAnalyzerViewModelByGuidAsync(string analyzer_guid) {
            if (Items.FirstOrDefault(x => x.PluginGuid == analyzer_guid) is { } aivm) {
                // shouldn't really happen
                await RemoveAnalyzerReferencesAsync(aivm, false);
            }
            var paivm = await CreateAnalyticItemViewModelAsync(analyzer_guid);
            if (paivm.PluginFormat == null) {
                // internal error/invalid issue with plugin, ignore it
                await MpPluginLoader.DetachPluginByGuidAsync(analyzer_guid);
                return false;
            }
            Items.Add(paivm);
            return true;
        }
        private async Task<MpAvAnalyticItemViewModel> CreateAnalyticItemViewModelAsync(string plugin_guid) {
            MpAvAnalyticItemViewModel aivm = new MpAvAnalyticItemViewModel(this);
            await aivm.InitializeAsync(plugin_guid);
            return aivm;
        }

        private async Task RemoveAnalyzerReferencesAsync(MpAvAnalyticItemViewModel aivm, bool deletePresets) {
            // NOTE since async calling method needs to null aivm param after this
            if (aivm == null) {
                return;
            }
            var preset_ids = aivm.Items.Select(x => x.AnalyticItemPresetId).ToList();

            while (aivm.IsBusy) {
                // wait if executing
                await Task.Delay(100);
            }

            var sw = Stopwatch.StartNew();
            while (true) {
                var running_action_analyzers = GetAnalyzerActions(preset_ids, true);
                if (!running_action_analyzers.Any()) {
                    break;
                }
                // wait for any running actions w/ this analyzer to complete
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 5_000) {
                    var sb = new StringBuilder($"Running timeout reached for analyzer '{aivm.Title}'. Running actions: ");
                    running_action_analyzers
                        .Select(x => x.RootTriggerActionViewModel)
                        .SelectMany(x => x.SelfAndAllDescendants.Where(y => y.IsPerformingAction))
                        .ForEach(x => sb.AppendLine(x.FullName));
                    MpDebug.Break(sb.ToString());
                    break;
                }
            }

            // remove from collection
            Items.Remove(aivm);
            OnPropertyChanged(nameof(Items));

            // get ALL action analyzer refs
            var all_aivm_actions = GetAnalyzerActions(preset_ids, false);

            if (deletePresets) {
                foreach (var aaivm in all_aivm_actions) {
                    aaivm.AnalyticItemPresetId = 0;
                }
                await Task.WhenAll(aivm.Items.Select(x => x.Preset.DeleteFromDatabaseAsync()));
            }
            // update actions to clear their preset vm ref
            all_aivm_actions.ForEach(x => x.OnPropertyChanged(nameof(x.SelectedPreset)));
        }

        private IEnumerable<MpAvAnalyzeActionViewModel> GetAnalyzerActions(IEnumerable<int> presetIds, bool onlyRunning) {
            var actions_w_presets =
                    MpAvTriggerCollectionViewModel.Instance
                        .Items
                        .OfType<MpAvAnalyzeActionViewModel>()
                        .Where(x =>
                            presetIds.Contains(x.AnalyticItemPresetId));
            if (onlyRunning) {
                return actions_w_presets.Where(x => x.IsPerformingAction);
            }
            return actions_w_presets;
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> ApplyCoreAnnotatorCommand => new MpAsyncCommand<object>(
            async (args) => {
                var ctvm = args as MpAvClipTileViewModel;
                if (ctvm == null) {
                    return;
                }

                var core_aipvm = AllPresets
                    .FirstOrDefault(x => x.PresetGuid == MpPluginLoader.CoreAnnotatorDefaultPresetGuid);

                if (core_aipvm == null) {
                    return;
                }
                await core_aipvm.Parent.PerformAnalysisCommand.ExecuteAsync(new object[] { core_aipvm, ctvm.CopyItem });
            });

        #endregion
    }
}
