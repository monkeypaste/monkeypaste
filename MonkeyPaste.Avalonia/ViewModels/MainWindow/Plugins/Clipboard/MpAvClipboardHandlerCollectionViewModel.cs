﻿
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS
using MonkeyPaste.Common.Wpf; 
#endif

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardHandlerCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvClipboardHandlerItemViewModel>,
        MpITreeItemViewModel,
        MpIMenuItemViewModel,
        MpIManagePluginComponents,
        MpISidebarItemViewModel,
        MpIAsyncCollectionObject,
        MpIAsyncComboBoxViewModel,
        MpIPlatformDataObjectTools { //
        #region Private Variables
        private static object _oleLock = new object();
        #endregion

        #region Constants
        const int OLE_WAIT_TIMEOUT_MS = 3_000;

        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIManagePluginComponents Implementation (unimplemented)

        async Task<bool> MpIManagePluginComponents.InstallAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {
            bool success = await MpPluginLoader.InstallPluginAsync(pluginGuid, packageUrl, false, cpivm);
            if (success) {
                IsBusy = true;

                var chivm = await CreateClipboardHandlerItemViewModelAsync(pluginGuid);
                Items.Add(chivm);

                IsBusy = false;
            }

            return success;
        }

        async Task<bool> MpIManagePluginComponents.UninstallAsync(string plugin_guid) {
            if (Items.FirstOrDefault(x => x.PluginGuid == plugin_guid) is not { } chivm) {
                MpDebug.Break($"Error uninstalling plugin guid '{plugin_guid}' can't find handler");
                return false;
            }
            // NOTE wait for processing to end BEFORE stopping monitor or last monitor req may restart itself (i think)
            await RemoveHandlerReferencesAsync(chivm, true);

            // remove from plugin dir
            bool success = await MpPluginLoader.DeletePluginByGuidAsync(plugin_guid);

            if (SelectedItem == null) {
                SelectedItem = SortedItems.FirstOrDefault();
            }
            return success;
        }
        async Task<bool> MpIManagePluginComponents.BeginUpdateAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {
            // NOTE not even trying to install, just wait for restart. can_reload will always be false
            bool can_reload = await MpPluginLoader.BeginUpdatePluginAsync(pluginGuid, packageUrl, cpivm, attemptInstall: false);
            return can_reload;
        }
        private async Task RemoveHandlerReferencesAsync(MpAvClipboardHandlerItemViewModel chivm, bool deletePresets) {
            IsOleProcessingBlocked = true;
            while (IsProcessingOleRequest) {
                await Task.Delay(100);
            }
            while (chivm.IsBusy) {
                // wait if executing
                await Task.Delay(100);
            }
            IsBusy = true;

            Mp.Services.ClipboardMonitor.StopMonitor();

            if (deletePresets) {
                await Task.WhenAll(
                chivm.Items
                .SelectMany(x => x.Items)
                .Select(x => x.Preset.DeleteFromDatabaseAsync()));
            }

            // remove from collection
            Items.Remove(chivm);
            OnPropertyChanged(nameof(Items));

            IsBusy = false;

            Mp.Services.ClipboardMonitor.StartMonitor(true);
            IsOleProcessingBlocked = false;
        }
        #endregion

        #region MpITreeItemViewModel Implementation

        public override MpITreeItemViewModel ParentTreeItem => null;
        #endregion

        #region MpIAsyncComboBoxViewModel Implementation

        IEnumerable<MpIAsyncComboBoxItemViewModel> MpIAsyncComboBoxViewModel.Items => Items;
        MpIAsyncComboBoxItemViewModel MpIAsyncComboBoxViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = (MpAvClipboardHandlerItemViewModel)value;
        }
        bool MpIAsyncComboBoxViewModel.IsDropDownOpen {
            get => IsHandlerDropDownOpen;
            set => IsHandlerDropDownOpen = value;
        }

        #endregion

        #region MpIClipboardFormatHandlers Implementation

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; }

        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    double def_w = 750;
                    if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                        def_w = Math.Min(def_w, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width / 2);
                    }
                    return def_w;
                } else {
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Width;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Height;
                } else {
                    double def_h = 300;
                    if (MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                        def_h = Math.Min(def_h, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Height / 3);
                    }
                    return def_h;
                }
            }
        }


        public string SidebarBgHexColor =>
            (Mp.Services.PlatformResource.GetResource("ClipboardSidebarBgBrush") as IBrush).ToHex();
        bool MpISidebarItemViewModel.CanResize =>
            true;
        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIPlatformDataObjectHelperAsync Implementation

        bool MpIPlatformDataObjectTools.IsOleBusy => IsBusy;

        async Task MpIPlatformDataObjectTools.WriteToClipboardAsync(object idoObj, bool ignoreClipboardChange) {
            if (idoObj is not IDataObject ido) {
                MpDebug.Break($"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
                return;
            }

            await PerformOlePluginRequestAsync(
                isRead: false,
                isDnd: false,
                ido: ido,
                ignorePlugins: false,
                ignoreClipboardChange: ignoreClipboardChange);
        }

        async Task<object> MpIPlatformDataObjectTools.ReadClipboardAsync(bool ignorePlugins) {
            var avdo = await PerformOlePluginRequestAsync(
                isRead: true,
                isDnd: false,
                ido: null,
                ignorePlugins: ignorePlugins);
            return avdo;
        }

        async Task<object> MpIPlatformDataObjectTools.ReadDataObjectAsync(object idoObj, MpDataObjectSourceType sourceType) {
            if (idoObj is not IDataObject ido) {
                MpDebug.Break($"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
                return null;
            }
            bool is_dnd =
                sourceType == MpDataObjectSourceType.ClipTileDrop ||
                sourceType == MpDataObjectSourceType.TagDrop ||
                sourceType == MpDataObjectSourceType.QueryTrayDrop ||
                sourceType == MpDataObjectSourceType.PinTrayDrop ||
                sourceType == MpDataObjectSourceType.ActionDrop;

            bool attachExtProcess = sourceType != MpDataObjectSourceType.PluginResponse;

            MpAvDataObject avdo = await PerformOlePluginRequestAsync(
                        isRead: true,
                        isDnd: is_dnd,
                        ido: ido,
                        ignorePlugins: false,
                        attachActiveProcessIfNone: attachExtProcess);
            avdo.SetDataObjectSourceType(sourceType);

            return avdo;
        }
        async Task<object> MpIPlatformDataObjectTools.WriteDragDropDataObjectAsync(object idoObj) {
            if (idoObj is not IDataObject ido) {
                MpDebug.Break($"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
                return null;
            }
            var avdo = await PerformOlePluginRequestAsync(
                isRead: false,
                isDnd: true,
                ido: ido,
                ignorePlugins: false);
            return avdo;
        }
        #endregion

        #endregion

        #region Properties       

        #region View Models
        public MpAvMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    Header = UiStrings.ClipTileTransformHeader,
                    IconResourceKey = Mp.Services.PlatformResource.GetResource("ButterflyImage") as string,
                    SubItems = Items.Select(x => x.ContextMenuItemViewModel).ToList()
                };
            }
        }

        public override ObservableCollection<MpAvClipboardHandlerItemViewModel> Items {
            get => base.Items;
            set => base.Items = value;
        }

        public override MpAvClipboardHandlerItemViewModel SelectedItem {
            get => base.SelectedItem;
            set => base.SelectedItem = value;
        }

        public IEnumerable<MpAvClipboardHandlerItemViewModel> SortedItems =>
            Items.OrderBy(x => x.HandlerName);

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllPresets =>
            Items.SelectMany(x => x.Items.SelectMany(y => y.Items));

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllSelectedPresets =>
            AllPresets.Where(x => x.IsSelected);

        public IEnumerable<MpAvClipboardFormatViewModel> FormatViewModels =>
            MpDataFormatRegistrar.RegisteredFormats.Select(x => new MpAvClipboardFormatViewModel(this, x));
        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledFormats => AllPresets.Where(x => x.IsEnabled);

        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledReaders => EnabledFormats.Where(x => x.IsReader);
        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledWriters => EnabledFormats.Where(x => x.IsWriter);

        public IEnumerable<MpAvClipboardFormatPresetViewModel> SortedAvailableEnabledWriters =>
            AllWriterPresets
            .OrderBy(x => x.ClipboardFormat.sortOrderIdx)
            .ToList();

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllWriterPresets =>
            AllPresets.Where(x => x.IsWriter);
        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllReaderPresets =>
            AllPresets.Where(x => x.IsReader);

        public MpAvClipboardFormatPresetViewModel SelectedPresetViewModel {
            get {
                if (SelectedItem == null) {
                    return null;
                }
                if (SelectedItem.SelectedItem == null) {
                    if (SelectedItem.Items.Count > 0) {
                        SelectedItem.Items[0].IsSelected = true;
                    } else {
                        return null;
                    }
                }
                if (SelectedItem.SelectedItem.SelectedItem == null) {
                    if (SelectedItem.SelectedItem.Items.Count > 0) {
                        SelectedItem.SelectedItem.Items[0].IsSelected = true;
                    } else {
                        return null;
                    }
                }
                return SelectedItem.SelectedItem.SelectedItem;
                //return AllPresets.OrderByDescending(x => x.LastSelectedDateTime).FirstOrDefault();
            }
        }
        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

        bool IsProcessingOleRequest { get; set; }
        bool IsOleProcessingBlocked { get; set; }
        public bool IsAnyBusy =>
            Items.Any(x => x.IsBusy) || IsBusy;

        public int SelectedItemIdx {
            get => Items.IndexOf(SelectedItem);
            set {
                if (SelectedItemIdx != value) {
                    SelectedItem = value < 0 || value >= Items.Count ? null : Items[value];
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }
        public bool IsHovering { get; set; }

        public bool IsHandlerDropDownOpen { get; set; }

        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        private static MpAvClipboardHandlerCollectionViewModel _instance;
        public static MpAvClipboardHandlerCollectionViewModel Instance => _instance ?? (_instance = new MpAvClipboardHandlerCollectionViewModel());

        public MpAvClipboardHandlerCollectionViewModel() : base(null) {
            PropertyChanged += MpClipboardHandlerCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
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

            MpPluginLoader.Plugins.CollectionChanged += Plugins_CollectionChanged;

            var ole_guids =
                MpPluginLoader.Plugins
                .Where(x => x.pluginType == MpPluginType.Clipboard)
                .Select(x => x.guid)
                .ToList();

            int count = ole_guids.Count;
            for (int i = 0; i < count; i++) {
                string ole_guid = ole_guids[i];

                var paivm = await CreateClipboardHandlerItemViewModelAsync(ole_guid);
                bool success = await ValidateHandlerFormatsAsync(paivm);
                if (success) {
                    Items.Add(paivm);
                } else {
                    await MpPluginLoader.DetachPluginByGuidAsync(ole_guid);
                }
            }

            //while (Items.Any(x => x.IsBusy)) {
            //    await Task.Delay(100);
            //}

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                // select most recent preset and init default handlers
                MpAvClipboardFormatPresetViewModel presetToSelect = null;
                foreach (var chivm in Items) {
                    foreach (var hivm in chivm.Items) {
                        foreach (var hipvm in hivm.Items) {
                            if (presetToSelect == null) {
                                presetToSelect = hipvm;
                            } else if (hipvm.LastSelectedDateTime > presetToSelect.LastSelectedDateTime) {
                                presetToSelect = hipvm;
                            }
                        }
                    }
                }
                if (presetToSelect != null) {
                    presetToSelect.IsSelected = true;
                    presetToSelect.Parent.SelectedItem = presetToSelect;
                    presetToSelect.Parent.Parent.SelectedItem = presetToSelect.Parent;
                    SelectedItem = presetToSelect.Parent.Parent;
                }
            }

            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(FormatViewModels));
            OnPropertyChanged(nameof(EnabledFormats));
            IsLoaded = true;
            IsBusy = false;
        }
        public MpAvClipboardFormatPresetViewModel FindFormatPreset(string pluginGuid, string formatName, bool isReader) {
            return
                AllPresets.FirstOrDefault(x =>
                    x.Parent.PluginGuid == pluginGuid &&
                    x.IsReader == isReader &&
                    x.ClipboardFormat.formatName.ToLowerInvariant() == formatName.ToLowerInvariant());
        }
        public bool ValidateAppOleInfos() {
            // NOTE unused just for diagnostics
            var dup_readers =
                EnabledReaders
                .GroupBy(x => x.FormatName)
                .Where(x => x.Count() > 1);
            var dup_writers =
                EnabledWriters
                .GroupBy(x => x.FormatName)
                .Where(x => x.Count() > 1);
            bool is_valid = !dup_readers.Any() && !dup_writers.Any();
            if (!is_valid) {
                MpDebug.Break($"Dup formats detected");
            }
            return is_valid;
        }

        #endregion

        #region Private Methods

        private async Task<MpAvClipboardHandlerItemViewModel> CreateClipboardHandlerItemViewModelAsync(string ole_guid) {
            MpAvClipboardHandlerItemViewModel aivm = new MpAvClipboardHandlerItemViewModel(this);
            await aivm.InitializeAsync(ole_guid);
            return aivm;
        }
        private void Plugins_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var plugin_guids_to_remove = Items.Where(x => !MpPluginLoader.PluginGuidLookup.ContainsKey(x.PluginGuid)).Select(x => x.PluginGuid).ToList();
            foreach (string guid in plugin_guids_to_remove) {
                // this should only find matches when plugins were removed for invalidation(s) during install. 
                if (Items.FirstOrDefault(x => x.PluginGuid == guid) is { } aivm) {
                    RemoveHandlerReferencesAsync(aivm, false).FireAndForgetSafeAsync();
                }
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SortedItems));
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:

                    // Update preset availability
                    EnabledWriters.ForEach(x => x.OnPropertyChanged(nameof(x.IsFormatOnSourceDragObject)));
                    break;
            }
        }

        private void MpClipboardHandlerCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Items):
                    //OnPropertyChanged(nameof(Children));
                    break;
                case nameof(SelectedPresetViewModel):
                    if (SelectedPresetViewModel == null) {
                        //AllPresets.OrderByDescending(x => x.LastSelectedDateTime).FirstOrDefault().Parent.IsSelected = true;
                        //OnPropertyChanged(nameof(SelectedPresetViewModel));
                        return;
                    }
                    SelectedPresetViewModel.OnPropertyChanged(nameof(SelectedPresetViewModel.Items));
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(SelectedPresetViewModel));
                    OnPropertyChanged(nameof(SelectedItemIdx));
                    break;
                case nameof(IsHandlerDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsHandlerDropDownOpen;
                    break;
            }
        }

        private async Task<bool> ValidateHandlerFormatsAsync(MpAvClipboardHandlerItemViewModel hivm) {
            if (hivm == null || hivm.PluginFormat == null || hivm.PluginFormat.oleHandler == null) {
                // internal error/invalid issue with plugin, ignore it
                return false;
            }
            var error_notifications = new List<MpNotificationFormat>();

            var all_plugin_formats =
                Items.Select(x => x.PluginFormat)
                .Union(new[] { hivm.PluginFormat });

            var allHandlers =
                all_plugin_formats.SelectMany(x => x.oleHandler.readers)
                .Union(all_plugin_formats.SelectMany(x => x.oleHandler.writers));

            var dupGuids = allHandlers.GroupBy(x => x.formatGuid).Where(x => x.Count() > 1);
            if (dupGuids.Count() > 0) {
                foreach (var dupGuid_group in dupGuids) {
                    var loaded_hi = Items.FirstOrDefault(x => x.Items.Any(y => y.FormatGuid == dupGuid_group.Key));
                    var loaded_hf = loaded_hi.Items.FirstOrDefault(x => x.FormatGuid == dupGuid_group.Key);

                    var sb = new StringBuilder();
                    //sb.AppendLine($"Clipboard 'formatGuid' must be unique. ");
                    //sb.AppendLine($"'formatGuid': ");
                    //sb.AppendLine(dupGuid_group.Key);
                    //sb.AppendLine($"Already exists for");
                    //sb.AppendLine("Plugin:");
                    //sb.AppendLine($"{loaded_hi.PluginFormat.title}");
                    //sb.AppendLine("Format:");
                    //sb.AppendLine($"{loaded_hf.ClipboardPluginFormat.formatName}");
                    //sb.AppendLine("Type:");
                    //sb.AppendLine($"{(loaded_hf.IsReader ? "Reader" : "Writer")}");

                    sb.AppendLine(UiStrings.InvalidFormatGuidEx1.Format(UiStrings.CommonFormatGuidLabel));
                    sb.AppendLine($"'{UiStrings.CommonFormatGuidLabel}': ");
                    sb.AppendLine(dupGuid_group.Key);
                    sb.AppendLine(UiStrings.InvalidFormatGuidEx2);
                    sb.AppendLine($"{UiStrings.CommonPluginLabel}:");
                    sb.AppendLine($"{loaded_hi.PluginFormat.title}");
                    sb.AppendLine($"{UiStrings.InvalidFormatGuidEx3}:");
                    sb.AppendLine($"{loaded_hf.ClipboardPluginFormat.formatName}");
                    sb.AppendLine($"{UiStrings.InvalidFormatGuidEx4}:");
                    sb.AppendLine($"{(loaded_hf.IsReader ? UiStrings.CommonReaderLabel : UiStrings.CommonWriterLabel)}");
                    error_notifications.Add(MpPluginLoader.CreateInvalidPluginNotification(sb.ToString(), hivm.PluginFormat));
                }
            }
            bool needs_fixing = error_notifications.Count > 0;
            if (needs_fixing) {
                // only need first error to recurse

                var invalid_nf = error_notifications[0];

                invalid_nf.RetryAction = (args) => {
                    needs_fixing = false;
                    return null;
                };

                var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(invalid_nf);
                if (result == MpNotificationDialogResultType.Ignore) {
                    // ignoring these errors flags plugin to be completely ignored
                    return false;
                }
                while (needs_fixing) {
                    await Task.Delay(100);
                }

                _ = await MpPluginLoader.ReloadPluginAsync(hivm.PluginFormat.guid);
                hivm.OnPropertyChanged(nameof(hivm.PluginFormat));
                // loop through another validation pass
                bool is_valid = await ValidateHandlerFormatsAsync(hivm);
                return is_valid;
            }

            return true;
        }

        private async Task<MpAvDataObject> PerformOlePluginRequestAsync(
            bool isRead,
            bool isDnd,
            IDataObject ido,
            bool ignorePlugins,
            bool attachActiveProcessIfNone = true,
            bool ignoreClipboardChange = false) {
            if (IsOleProcessingBlocked) {
                MpConsole.WriteLine($"Ole request attempt BLOCKED");
                return new MpAvDataObject();
            }
            // if ido provided carry use provided pi if exits
            MpPortableProcessInfo active_pi =
                ido == null ? null :
                ido.Contains(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT) ?
                    ido.Get(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT) as MpPortableProcessInfo :
                    null;

            if (attachActiveProcessIfNone && isRead && !isDnd && active_pi == null && !ignorePlugins) {
                active_pi = Mp.Services.ProcessWatcher.GetClipboardOwner();
            }

            int[] custom_preset_ids = ignorePlugins ? null :
                    MpAvAppCollectionViewModel.Instance
                    .GetAppCustomOlePresetsByWatcherState(
                        isRead: isRead,
                        isDnd: isDnd,
                        active_pi: ref active_pi);

            // wait for any other clipboard comm to finish
            await WaitForBusyAsync($"ole req. isRead: {isRead} isDnd: {isDnd} ignorePlugins: {ignorePlugins}");
            IsProcessingOleRequest = true;

            // when writing to clipboard disable internal monitor
            bool was_cb_monitoring = Mp.Services.ClipboardMonitor.IsMonitoring;
            if (ignoreClipboardChange && was_cb_monitoring) {
                Mp.Services.ClipboardMonitor.StopMonitor();
            }

            // get actual preset data and components
            IEnumerable<MpAvClipboardFormatPresetViewModel> preset_vms =
                custom_preset_ids != null ?
                    AllPresets.Where(x => x.IsReader == isRead && custom_preset_ids.Contains(x.PresetId)) :
                    isRead ?
                        EnabledReaders : EnabledWriters;

            var handled_formats =
                preset_vms.Select(x => x.Parent).Distinct();

            var ido_formats = ido.GetAllDataFormats().ToList();
            //if (ido != null) {
            //    // pre-pass data object and remove disabled formats
            //    var formatsToRemove =
            //        ido_formats
            //        .Where(x => !MpPortableDataFormats.InternalFormats.Contains(x))
            //        .Where(x => preset_vms.All(y => y.FormatName != x))
            //        .Select(x => x)
            //        .ToList();

            //    if (formatsToRemove.Any()) {
            //        MpConsole.WriteLine($"Unrecognized clipboard formats found writing to clipboard: {string.Join(",", formatsToRemove)}");
            //        formatsToRemove.ForEach(x => ido.TryRemove(x));
            //        formatsToRemove.ForEach(x => ido_formats.Remove(x));
            //    }
            //}
            // instantiate new ido for output
            Dictionary<string, object> dataLookup = ido.ToDictionary();
            var avdo = new MpAvDataObject();

            var format_handlers = handled_formats.Select(x => x.Parent).Distinct();
            // only make 1 request per component
            foreach (var hcfvm in format_handlers) {
                // req to component contains unprocessed input ido
                // with only the formats/params for the custom or def enabled presets 
                var req = new MpOlePluginRequest() {
                    Clipboard = Mp.Services.DeviceClipboard,
                    dataObjectLookup = dataLookup,
                    isDnd = isDnd,
                    ignoreParams = ignorePlugins,
                    formats =
                    preset_vms
                        .Where(x => x.Parent.Parent == hcfvm)
                        .Select(x => x.FormatName)
                        .Union(ido_formats)
                        .Distinct()
                        .ToList(),
                    items =
                        preset_vms
                            .Where(x => x.Parent.Parent == hcfvm)
                            .SelectMany(x => x.Items
                                .Select(y =>
                                    new MpParameterRequestItemFormat(y.ParamId, y.CurrentValue))).ToList(),
                };

                Func<Task<MpOlePluginResponse>> retryHandlerFunc = async () => {
                    // NOTE this isn't really implemented since clipboard handlers are passive
                    // but just stubbed out here...
                    var result = await hcfvm.IssueOleRequestAsync(req, isRead);
                    return result;
                };

                // get response from request
                MpOlePluginResponse resp = await hcfvm.IssueOleRequestAsync(req, isRead);

                // process response for any ntf or retry requests
                resp = await MpPluginTransactor.ValidatePluginResponseAsync(
                    hcfvm.HandlerName,
                    req,
                    resp,
                    retryHandlerFunc);

                if (resp != null && resp.dataObjectLookup != null) {
                    // set resp ido formats in output ido
                    // only include internal or requested formats
                    // NOTE this will deal w/ rtf<->html enabled but
                    // converted format is not enabled, then its removed here
                    resp.dataObjectLookup
                    .Where(x =>
                        x.Value != null &&
                        (MpDataFormatRegistrar.RegisteredInternalFormats.Contains(x.Key) ||
                        preset_vms.Any(y => y.FormatName == x.Key)))
                    .ForEach(kvp => avdo.SetData(kvp.Key, kvp.Value));
                }
            }
            // unmark busy for next ole comm
            IsBusy = false;

            if (attachActiveProcessIfNone &&
                active_pi != null &&
                !avdo.ContainsData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT)) {
                // only attach process info if not 
                avdo.Set(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, active_pi.Clone());
            }
            if (ido != null) {
                // merge any internal formats into output that were present in input AND
                // not handled by plugins
                ido
                    .GetAllDataFormats()
                    .Where(x => !avdo.ContainsData(x) && MpDataFormatRegistrar.RegisteredInternalFormats.Contains(x))
                    .ForEach(x => avdo.SetData(x, ido.Get(x)));
            }
            if (ignoreClipboardChange && was_cb_monitoring) {
                Mp.Services.ClipboardMonitor.StartMonitor(true);
            }
            IsProcessingOleRequest = false;
            return avdo;
        }
        private async Task WaitForBusyAsync(string debug_label) {
            try {
                await MpFifoAsyncQueue.WaitByConditionAsync(
                lockObj: _oleLock,
                time_out_ms: OLE_WAIT_TIMEOUT_MS,
                waitWhenTrueFunc: () => {
                    return IsBusy;
                },
                debug_label: debug_label);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Add content ex. Probably too hot outside. ", ex);
                return;
            }
            IsBusy = true;
        }
        #endregion

        #region Commands

        #endregion
    }
}
