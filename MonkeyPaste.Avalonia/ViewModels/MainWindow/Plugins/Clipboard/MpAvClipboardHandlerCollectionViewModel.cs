
using Avalonia.Input;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardHandlerCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvClipboardHandlerItemViewModel>,
        MpITreeItemViewModel,
        MpIMenuItemViewModel,
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

        private static List<string> _oleReqGuids = new List<string>();

        #endregion

        #region Interfaces

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
        public IEnumerable<MpIOlePluginComponent> Handlers =>
            EnabledFormats.Select(x => x.Parent.ClipboardPluginComponent).Distinct();

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; }

        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return 750;
                } else {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ObservedQueryTrayScreenHeight;
                } else {
                    return 300;
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

        async Task<object> MpIPlatformDataObjectTools.ReadDragDropDataObjectAsync(object idoObj) {
            if (idoObj is not IDataObject ido) {
                MpDebug.Break($"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
                return null;
            }
            var avdo = await PerformOlePluginRequestAsync(
                isRead: true,
                isDnd: true,
                ido: ido,
                ignorePlugins: false);
            return avdo;
        }
        async Task<object> MpIPlatformDataObjectTools.WriteDragDropDataObjectAsync(object idoObj) {
            //MpDebug.Assert(idoObj is IDataObject, $"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
            //var result = await WriteClipboardOrDropObjectAsync(idoObj as IDataObject, false, false);
            //return result;
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
        async Task MpIPlatformDataObjectTools.UpdateDragDropDataObjectAsync(object source, object target) {
            // NOTE this is called during a drag drop when user toggles a format preset
            // source should be the initial output of ContentView dataObjectLookup and should
            // have the highest fidelity of data on it for conversions
            // NOTE DO NOT re-instantiate target haven't tested but I
            // imagine the reference must persist that which was given to .DoDragDrop in StartDragging

            MpDebug.Assert(source is IDataObject, $"source idoObj must be IDataObject. Is '{source.GetType()}'");
            MpDebug.Assert(target is IDataObject, $"target idoObj must be IDataObject. Is '{target.GetType()}'");
            if (source is IDataObject sido &&
                target is IDataObject tido) {
                var source_clone = sido.Clone();
                //var temp = await WriteClipboardOrDropObjectAsync(source_clone, false, false);
                var temp = await PerformOlePluginRequestAsync(
                                    isRead: false,
                                    isDnd: true,
                                    ido: source_clone,
                                    ignorePlugins: false);
                if (temp is IDataObject temp_ido) {

                    temp_ido.CopyTo(tido);
                    if (tido.TryGetData(MpPortableDataFormats.Files, out IEnumerable<string> fnl)) {
                        MpConsole.WriteLine($"dnd obj updated. target fns:");
                        fnl.ForEach(x => MpConsole.WriteLine(x));
                    }

                }
                //target = temp;
                //return temp;
            } else {
                // need to cast or whats goin on here?
                MpDebug.Break();
                return;
            }

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


        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllPresets =>
            Items.SelectMany(x => x.Items.SelectMany(y => y.Items));

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllSelectedPresets =>
            AllPresets.Where(x => x.IsSelected);

        public IEnumerable<MpAvClipboardFormatViewModel> FormatViewModels =>
            MpPortableDataFormats.RegisteredFormats.Select(x => new MpAvClipboardFormatViewModel(this, x));
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
        //public IEnumerable<MpAvClipboardFormatPresetViewModel> AllWriterPresets {
        //    get {
        //        var aawpl = new List<MpAvClipboardFormatPresetViewModel>();
        //        foreach (var handlerItem in Items) {
        //            foreach (var writerFormat in handlerItem.Writers) {
        //                yield return writerFormat.Items.OrderByDescending(x => x.LastSelectedDateTime).First();
        //            }
        //        }
        //    }
        //}

        public IEnumerable<MpIOleReaderComponent> EnabledReaderComponents =>
            EnabledReaders
            .Select(x => x.Parent.ClipboardPluginComponent)
            .Distinct()
            .Cast<MpIOleReaderComponent>();

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

            var pail =
                MpPluginLoader
                .Plugins.Where(x =>
                    x.Value.Components.Any(y => y is MpIOlePluginComponent));

            foreach (var pai in pail) {
                var paivm = await CreateClipboardHandlerItemViewModelAsync(pai.Value);
                bool success = await ValidateHandlerFormatsAsync(paivm);
                if (success) {
                    Items.Add(paivm);
                }

            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

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
                    x.ClipboardFormat.formatName.ToLower() == formatName.ToLower());
        }


        #endregion

        #region Private Methods

        private async Task<MpAvClipboardHandlerItemViewModel> CreateClipboardHandlerItemViewModelAsync(MpPluginWrapper plugin) {
            MpAvClipboardHandlerItemViewModel aivm = new MpAvClipboardHandlerItemViewModel(this);
            await aivm.InitializeAsync(plugin);
            return aivm;
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
                    sb.AppendLine($"Clipboard 'formatGuid' must be unique. ");
                    sb.AppendLine($"'formatGuid': ");
                    sb.AppendLine(dupGuid_group.Key);
                    sb.AppendLine($"Already exists for");
                    sb.AppendLine("Plugin:");
                    sb.AppendLine($"{loaded_hi.PluginFormat.title}");
                    sb.AppendLine("Format:");
                    sb.AppendLine($"{loaded_hf.ClipboardPluginFormat.formatName}");
                    sb.AppendLine("Type:");
                    sb.AppendLine($"{(loaded_hf.IsReader ? "Reader" : "Writer")}");
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

                //hivm.PluginFormat = await MpPluginLoader.ReloadPluginAsync(Path.Combine(hivm.PluginFormat.RootDirectory, "manifest.json"));
                hivm.PluginFormat = await MpPluginLoader.ReloadPluginAsync(hivm.PluginFormat.guid);
                // loop through another validation pass
                return await ValidateHandlerFormatsAsync(hivm);
            }

            return true;
        }

        private async Task<MpAvDataObject> PerformOlePluginRequestAsync(
            bool isRead,
            bool isDnd,
            IDataObject ido,
            bool ignorePlugins,
            bool ignoreClipboardChange = false) {
            if (isDnd) {

            }
            // if ido provided carry use provided pi if exits
            MpPortableProcessInfo active_pi =
                ido == null ? null : ido.Get(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT) as MpPortableProcessInfo;

            int[] custom_preset_ids = ignorePlugins ? null :
                    MpAvAppCollectionViewModel.Instance
                    .GetAppCustomOlePresetsByWatcherState(
                        isRead: isRead,
                        isDnd: isDnd,
                        active_pi: ref active_pi);

            // wait for any other clipboard comm to finish
            await WaitForBusyAsync($"ole req. isRead: {isRead} isDnd: {isDnd} ignorePlugins: {ignorePlugins}");

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

            IEnumerable<MpIOlePluginComponent> ole_components =
                preset_vms
                    .Select(x => x.Parent.ClipboardPluginComponent)
                    .Distinct();

            var ido_formats = ido.GetAllDataFormats().ToList();
            if (ido != null) {
                // pre-pass data object and remove disabled formats
                var formatsToRemove =
                    ido_formats
                    .Where(x => !MpPortableDataFormats.InternalFormats.Contains(x))
                    .Where(x => preset_vms.All(y => y.FormatName != x))
                    .Select(x => x)
                    .ToList();

                if (formatsToRemove.Any()) {
                    MpConsole.WriteLine($"Unrecognized clipboard formats found writing to clipboard: {string.Join(",", formatsToRemove)}");
                    formatsToRemove.ForEach(x => ido.TryRemove(x));
                    formatsToRemove.ForEach(x => ido_formats.Remove(x));
                }
            }
            // instantiate new ido for output
            Dictionary<string, object> dataLookup = ido.ToDictionary();
            var avdo = new MpAvDataObject();

            // only make 1 request per component
            foreach (var component in ole_components) {
                // req to component contains unprocessed input ido
                // with only the formats/params for the custom or def enabled presets 
                var req = new MpOlePluginRequest() {
                    dataObjectLookup = dataLookup,
                    isDnd = isDnd,
                    ignoreParams = ignorePlugins,
                    formats =
                    preset_vms
                        .Where(x => x.Parent.ClipboardPluginComponent == component)
                        .Select(x => x.FormatName)
                        .Union(ido_formats)
                        .Distinct()
                        .ToList(),
                    items =
                        preset_vms
                            .Where(x => x.Parent.ClipboardPluginComponent == component)
                            .SelectMany(x => x.Items
                                .Select(y =>
                                    new MpParameterRequestItemFormat(y.ParamId, y.CurrentValue))).ToList(),
                };

                Func<Task<MpOlePluginResponse>> retryHandlerFunc = async () => {
                    // this is mainly just testing retry from plugin
                    // like if format is > max length, change and retry..
                    var result = await component.ProcessOleRequestAsync(req);
                    return result;
                };

                // get response from request
                MpOlePluginResponse resp = await component.ProcessOleRequestAsync(req);

                // process response for any ntf or retry requests
                resp = await MpPluginTransactor.ValidatePluginResponseAsync(
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
                        (MpPortableDataFormats.InternalFormats.Contains(x.Key) ||
                        preset_vms.Any(y => y.FormatName == x.Key)))
                    .ForEach(kvp => avdo.SetData(kvp.Key, kvp.Value));
                }
            }
            // unmark busy for next ole comm
            IsBusy = false;

            if (active_pi != null &&
                !avdo.ContainsData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT)) {
                // only attach process info if not 
                avdo.Set(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, active_pi.Clone());
            }
            if (ido != null) {
                // merge any internal formats into output that were present in input AND
                // not handled by plugins
                ido
                    .GetAllDataFormats()
                    .Where(x => !avdo.ContainsData(x) && MpPortableDataFormats.InternalFormats.Contains(x))
                    .ForEach(x => avdo.SetData(x, ido.Get(x)));
            }
            if (ignoreClipboardChange && was_cb_monitoring) {
                Mp.Services.ClipboardMonitor.StartMonitor(true);
            }
            return avdo;
        }
        private async Task WaitForBusyAsync(string debug_label) {
            //if (IsBusy) {
            //    string req_guid = System.Guid.NewGuid().ToString();
            //    _oleReqGuids.Add(req_guid);
            //    while (true) {
            //        if (!IsBusy && _oleReqGuids.First() == req_guid) {
            //            IsBusy = true;
            //            _oleReqGuids.Remove(req_guid);
            //            return;
            //        }
            //        await Task.Delay(100);
            //    }
            //}
            //IsBusy = true;

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

        public MpIAsyncCommand<object> UninstallHandlerCommand => new MpAsyncCommand<object>(
            async (args) => {

                string plugin_guid = args as string;

                var aivm = Items.FirstOrDefault(x => x.PluginGuid == plugin_guid);
                if (aivm == null) {
                    MpDebug.Break($"Error uninstalling plugin guid '{plugin_guid}' can't find analyer");
                    return;
                }
                // NOTE assume confirm handled in calling command (plugin browser)

                while (aivm.IsBusy) {
                    // wait if executing
                    await Task.Delay(100);
                }
                IsBusy = true;

                Mp.Services.ClipboardMonitor.StopMonitor();

                await Task.WhenAll(
                    aivm.Items
                    .SelectMany(x => x.Items)
                    .Select(x => x.Preset.DeleteFromDatabaseAsync()));

                // remove from plugin dir
                MpPluginLoader.DeletePluginByGuid(aivm.PluginGuid);


                // remove from collection
                Items.Remove(aivm);
                OnPropertyChanged(nameof(Items));

                IsBusy = false;

                Mp.Services.ClipboardMonitor.StartMonitor(true);
            }) {


        };
        #endregion
    }
}
