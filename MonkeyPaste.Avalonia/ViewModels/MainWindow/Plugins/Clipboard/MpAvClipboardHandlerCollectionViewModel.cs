
using Avalonia.Input;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipboardHandlerCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvClipboardHandlerItemViewModel>,
        MpIMenuItemViewModel,
        MpISidebarItemViewModel,
        MpIAsyncCollectionObject,
        MpIAsyncComboBoxViewModel,
        MpIClipboardFormatDataHandlers,
        MpIPlatformDataObjectTools { //

        #region Constants

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
        public IEnumerable<MpIClipboardPluginComponent> Handlers => EnabledFormats.Select(x => x.Parent.ClipboardPluginComponent).Distinct();

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

        async Task MpIPlatformDataObjectTools.WriteToClipboardAsync(object idoObj, bool ignoreClipboardChange, int[] force_writer_preset_ids) {
            MpDebug.Assert(idoObj is IDataObject, $"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
            await WriteClipboardOrDropObjectAsync(idoObj as IDataObject, true, ignoreClipboardChange, force_writer_preset_ids);
        }

        async Task<object> MpIPlatformDataObjectTools.ReadClipboardAsync(bool ignorePlugins) {
            MpPortableProcessInfo cb_pi = null;
            if (!ignorePlugins &&
                Mp.Services.ProcessWatcher.LastProcessInfo is MpPortableProcessInfo ppi) {
                // non-polling req when clipboard has changed
                // grab active process info to improve accuracy
                cb_pi = ppi;
            }
            var avdo = await ReadClipboardOrDropObjectAsync(null, ignorePlugins);
            if (cb_pi != null) {
                avdo.Set(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, cb_pi.Clone());
            }
            return avdo;
        }

        async Task<object> MpIPlatformDataObjectTools.ReadDragDropDataObjectAsync(object idoObj) {
            MpDebug.Assert(idoObj is IDataObject, $"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
            MpPortableProcessInfo drag_pi = null;
            if (Mp.Services.DragProcessWatcher.DragProcess is MpPortableProcessInfo ppi) {
                // grab active process info to improve accuracy
                drag_pi = ppi;
            }
            var avdo = await ReadClipboardOrDropObjectAsync(idoObj as IDataObject);
            if (drag_pi != null) {
                avdo.Set(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, drag_pi.Clone());
            }
            return avdo;
        }
        async Task<object> MpIPlatformDataObjectTools.ProcessDragDropDataObjectAsync(object idoObj, int[] force_writer_preset_ids) {
            MpDebug.Assert(idoObj is IDataObject, $"idoObj must be IDataObject. Is '{idoObj.GetType()}'");
            var result = await WriteClipboardOrDropObjectAsync(idoObj as IDataObject, false, false, force_writer_preset_ids);
            return result;
        }
        async Task MpIPlatformDataObjectTools.UpdateDragDropDataObjectAsync(object source, object target, int[] force_writer_preset_ids) {
            // NOTE this is called during a drag drop when user toggles a format preset
            // source should be the initial output of ContentView dataObject and should have the highest fidelity of data on it for conversions
            // NOTE DO NOT re-instantiate target haven't tested but I imagine the reference must persist that which was given to .DoDragDrop in StartDragging

            MpDebug.Assert(source is IDataObject, $"source idoObj must be IDataObject. Is '{source.GetType()}'");
            MpDebug.Assert(target is IDataObject, $"target idoObj must be IDataObject. Is '{target.GetType()}'");
            if (source is IDataObject sido &&
                target is IDataObject tido) {
                var source_clone = sido.Clone();
                var temp = await WriteClipboardOrDropObjectAsync(source_clone, false, false, force_writer_preset_ids);
                if (temp is IDataObject temp_ido) {

                    temp_ido.CopyTo(tido);
                    if (tido.TryGetData(MpPortableDataFormats.AvFiles, out IEnumerable<string> fnl)) {
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
                    Header = @"_Transform",
                    IconResourceKey = Mp.Services.PlatformResource.GetResource("ButterflyImage") as string,
                    SubItems = Items.Select(x => x.ContextMenuItemViewModel).ToList()
                };
            }
        }


        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllPresets => Items.SelectMany(x => x.Items.SelectMany(y => y.Items));

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllSelectedPresets => AllPresets.Where(x => x.IsSelected);

        public IEnumerable<MpAvClipboardFormatViewModel> FormatViewModels =>
            MpPortableDataFormats.RegisteredFormats.Select(x => new MpAvClipboardFormatViewModel(this, x));
        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledFormats => AllPresets.Where(x => x.IsEnabled);

        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledReaders => EnabledFormats.Where(x => x.IsReader);
        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledWriters => EnabledFormats.Where(x => x.IsWriter);

        public IEnumerable<MpAvClipboardFormatPresetViewModel> SortedAvailableEnabledWriters =>
            AllAvailableWriterPresets
            .OrderBy(x => x.ClipboardFormat.sortOrderIdx)
            .ToList();

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllAvailableWriterPresets {
            get {
                var aawpl = new List<MpAvClipboardFormatPresetViewModel>();
                foreach (var handlerItem in Items) {
                    foreach (var writerFormat in handlerItem.Writers) {
                        yield return writerFormat.Items.OrderByDescending(x => x.LastSelectedDateTime).First();
                    }
                }
            }
        }

        public IEnumerable<MpIClipboardReaderComponent> EnabledReaderComponents =>
            EnabledReaders
            .Select(x => x.Parent.ClipboardPluginComponent)
            .Distinct()
            .Cast<MpIClipboardReaderComponent>();

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
                return SelectedItem.SelectedItem.SelectedItem;
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

            //MpMessenger.Register<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            Items.Clear();

            var pail = MpPluginLoader.Plugins.Where(x => x.Value.Component is MpIClipboardPluginComponent);
            foreach (var pai in pail) {
                var paivm = await CreateClipboardHandlerItemViewModelAsync(pai.Value);
                Items.Add(paivm);
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
                    presetToSelect.Parent.SelectedItem = presetToSelect;
                    presetToSelect.Parent.Parent.SelectedItem = presetToSelect.Parent;
                    SelectedItem = presetToSelect.Parent.Parent;
                }
            }

            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(FormatViewModels));
            OnPropertyChanged(nameof(EnabledFormats));
            IsBusy = false;
        }

        public MpAvClipboardFormatPresetViewModel FindFormatPreset(string pluginGuid, string formatName, bool isReader) {
            return
                AllPresets.FirstOrDefault(x =>
                    x.Parent.PluginGuid == pluginGuid &&
                    x.IsReader == isReader &&
                    x.ClipboardFormat.clipboardName.ToLower() == formatName.ToLower());
        }


        #endregion

        #region Private Methods

        private async Task<MpAvClipboardHandlerItemViewModel> CreateClipboardHandlerItemViewModelAsync(MpPluginFormat plugin) {
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
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                //case nameof(IsSidebarVisible):
                //    if (IsSidebarVisible) {
                //        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                //        MpAvTriggerCollectionViewModel.Instance.IsSidebarVisible = false;
                //        MpAvAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                //    }
                //    OnPropertyChanged(nameof(SelectedItem));
                //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                //    break;
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
                    break;
                case nameof(IsHandlerDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsHandlerDropDownOpen;
                    break;
            }
        }

        private async Task<MpAvDataObject> ReadClipboardOrDropObjectAsync(
            IDataObject forced_ido = null,
            bool ignorePlugins = false) {
            // NOTE forcedDataObject is used to read drag/drop, when null clipboard is read
            await WaitForBusyAsync();

            MpAvDataObject mpdo = new MpAvDataObject();

            foreach (var read_component in EnabledReaderComponents) {
                var reader_request = new MpClipboardReaderRequest() {
                    ignoreParams = ignorePlugins,
                    readFormats =
                        EnabledReaders
                        .Where(x => x.Parent.ClipboardPluginComponent == read_component)
                        .Select(x => x.Parent.HandledFormat)
                        .Union(MpPortableDataFormats.InternalFormats)
                        .Distinct()
                        .ToList(),
                    items =
                        EnabledReaders
                        .Where(x => x.Parent.ClipboardPluginComponent == read_component)
                        .SelectMany(x => x.Items
                            .Select(y => new MpParameterRequestItemFormat(y.ParamId, y.CurrentValue)))
                        .ToList(),
                    forcedClipboardDataObject = forced_ido
                };

                Func<Task<MpClipboardReaderResponse>> retryHandlerReadFunc = async () => {
                    var result = await read_component.ReadClipboardDataAsync(reader_request);
                    return result;
                };

                MpClipboardReaderResponse reader_response = await read_component.ReadClipboardDataAsync(reader_request);

                reader_response = await MpPluginTransactor.ValidatePluginResponseAsync(
                    reader_request,
                    reader_response,
                    retryHandlerReadFunc);


                if (reader_response != null) {
                    reader_response.dataObject.ForEach(x => mpdo.Set(x.Key, x.Value));
                } else {
                    MpConsole.WriteLine("Invalid cb reader response: " + reader_response);
                }
            }
            await mpdo.MapAllPseudoFormatsAsync();
            IsBusy = false;
            return mpdo;
        }

        private async Task<MpAvDataObject> WriteClipboardOrDropObjectAsync(
            IDataObject ido,
            bool writeToClipboard,
            bool ignoreClipboardChange,
            int[] force_writer_preset_ids) {
            await WaitForBusyAsync();

            bool was_cb_monitoring = Mp.Services.ClipboardMonitor.IsMonitoring;
            if (ignoreClipboardChange &&
                was_cb_monitoring) {
                Mp.Services.ClipboardMonitor.StopMonitor();
            }
            IEnumerable<MpAvClipboardFormatPresetViewModel> writer_presets =
                force_writer_preset_ids == null ?
                    EnabledWriters :
                    AllAvailableWriterPresets.Where(x => force_writer_preset_ids.Contains(x.PresetId));

            IEnumerable<MpIClipboardWriterComponent> writer_components =
                writer_presets
                    .Select(x => x.Parent.ClipboardPluginComponent)
                    .Distinct()
                    .Cast<MpIClipboardWriterComponent>();

            // pre-pass data object and remove disabled formats
            var formatsToRemove =
                ido.GetAllDataFormats()
                .Where(x => !MpPortableDataFormats.InternalFormats.Contains(x))
                .Where(x => writer_presets.All(y => y.ClipboardFormat.clipboardName != x))
                .Select(x => x);

            if (formatsToRemove.Any()) {
                MpConsole.WriteLine($"Unrecognized clipboard formats found writing to clipboard: {string.Join(",", formatsToRemove)}");
                formatsToRemove.ForEach(x => ido.TryRemove(x));
            }

            var dobj = new MpAvDataObject();

            foreach (var write_component in writer_components) {
                var write_request = new MpClipboardWriterRequest() {
                    data = ido,
                    writeToClipboard = writeToClipboard,
                    writeFormats =
                        writer_presets
                            .Where(x => x.Parent.ClipboardPluginComponent == write_component)
                            .Select(x => x.Parent.HandledFormat)
                            .Distinct()
                            .ToList(),
                    items =
                        writer_presets
                            .Where(x => x.Parent.ClipboardPluginComponent == write_component)
                            .SelectMany(x => x.Items
                                .Select(y => new MpParameterRequestItemFormat(y.ParamId, y.CurrentValue)))
                            .ToList(),
                };

                Func<Task<MpClipboardWriterResponse>> retryHandlerWriteFunc = async () => {
                    var result = await write_component.WriteClipboardDataAsync(write_request);
                    return result;
                };

                MpClipboardWriterResponse writerResponse = await write_component.WriteClipboardDataAsync(write_request);

                writerResponse = await MpPluginTransactor.ValidatePluginResponseAsync(
                    write_request,
                    writerResponse,
                    retryHandlerWriteFunc);

                if (writerResponse != null && writerResponse.processedDataObject is IDataObject processed_ido) {
                    processed_ido.GetAllDataFormats().ForEach(x => dobj.SetData(x, processed_ido.Get(x)));
                }
            }

            //MpConsole.WriteLine("Data written to " + (writeToClipboard ? "CLIPBOARD" : "DATAOBJECT") + ":");
            //dobj.GetAllDataFormats().ForEach(x => MpConsole.WriteLine("Format: " + x));

            IsBusy = false;
            if (ignoreClipboardChange &&
                was_cb_monitoring) {
                Mp.Services.ClipboardMonitor.StartMonitor(true);
            }
            return dobj;
        }


        private async Task WaitForBusyAsync() {
            if (IsBusy) {
                string req_guid = System.Guid.NewGuid().ToString();
                _oleReqGuids.Add(req_guid);
                while (true) {
                    if (!IsBusy && _oleReqGuids.First() == req_guid) {
                        IsBusy = true;
                        _oleReqGuids.Remove(req_guid);
                        return;
                    }
                    await Task.Delay(100);
                }
            }
            IsBusy = true;
        }
        #endregion

        #region Commands

        public ICommand ToggleFormatPresetIsEnabled => new MpCommand<object>(
            (presetVmArg) => {
                if (presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm) {
                    if (cfpvm.IsReader) {
                        ToggleFormatPresetIsReadEnabledCommand.Execute(cfpvm);
                    } else if (cfpvm.IsWriter) {
                        ToggleFormatPresetIsWriteEnabledCommand.Execute(cfpvm);
                    } else {
                        MpDebug.Break();
                    }
                }
            });
        public ICommand ToggleFormatPresetIsReadEnabledCommand => new MpCommand<object>(
            (presetVmArg) => {
                var presetVm = presetVmArg as MpAvClipboardFormatPresetViewModel;
                if (presetVm == null) {
                    return;
                }
                var handlerVm = presetVm.Parent;

                if (presetVm.IsEnabled) {
                    //when toggled on, untoggle any other preset handling same format
                    var otherEnabled = EnabledFormats.Where(x => x.IsReader).FirstOrDefault(x => x.Parent.HandledFormat == presetVm.Parent.HandledFormat && x.PresetId != presetVm.PresetId);
                    if (otherEnabled == null) {
                        //no other preset was enabled so nothing to replace
                        return;
                    }
                    otherEnabled.IsEnabled = false;

                    MpPortableDataFormats.RegisterDataFormat(handlerVm.HandledFormat);
                } else {
                    // when preset isDisabled unregister format 
                    UnregisterClipboardFormatCommand.Execute(new object[] {
                                handlerVm.HandledFormat,
                                true,
                                false
                            });
                }

                OnPropertyChanged(nameof(EnabledFormats));
                OnPropertyChanged(nameof(FormatViewModels));
            }, (presetVmArg) => presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm && cfpvm.IsReader);


        public ICommand ToggleFormatPresetIsWriteEnabledCommand => new MpCommand<object>(
            (presetVmArg) => {
                var presetVm = presetVmArg as MpAvClipboardFormatPresetViewModel;
                if (presetVm == null) {
                    return;
                }
                var handlerVm = presetVm.Parent;

                if (presetVm.IsEnabled) {
                    //when toggled on, untoggle any other preset handling same format
                    var otherEnabled = EnabledFormats.Where(x => x.IsWriter)
                    .FirstOrDefault(x => x.Parent.HandledFormat == presetVm.Parent.HandledFormat && x.PresetId != presetVm.PresetId);
                    if (otherEnabled == null) {
                        //no other preset was enabled so nothing to replace
                        return;
                    }
                    otherEnabled.IsEnabled = false;

                    MpPortableDataFormats.RegisterDataFormat(handlerVm.HandledFormat);
                } else {
                    // when preset isDisabled unregister format 
                    UnregisterClipboardFormatCommand.Execute(new object[] {
                                handlerVm.HandledFormat,
                                true,
                                false
                            });
                }

                OnPropertyChanged(nameof(EnabledFormats));
                OnPropertyChanged(nameof(FormatViewModels));
                OnPropertyChanged(nameof(AllAvailableWriterPresets));
            }, (presetVmArg) => presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm && cfpvm.IsWriter);

        public ICommand UnregisterClipboardFormatCommand => new MpCommand<object>(
            (args) => {
                return;
                //if (args is object[] argParts &&
                //   argParts.Length == 3 &&
                //   argParts[0] is string format &&
                //   argParts[2] is bool isReadUnregister &&
                //   argParts[1] is bool isWriteUnregister) {

                //    bool canUnregister = false;
                //    if (isReadUnregister && isWriteUnregister) {
                //        // when both read and write are unregistered (i don't know when this would happen)
                //        // it doesn't matter if format is known anymore so just unregister
                //        canUnregister = true;
                //    } else if (isReadUnregister && EnabledFormats.Any(x => x.IsWriter && x.Parent.HandledFormat == format)) {
                //        MpConsole.WriteTraceLine($"Note! Attempting to unregister '{format}' because read unregistered but a writer uses it so ignoring");
                //    } else if (isWriteUnregister && EnabledFormats.Any(x => x.IsReader && x.Parent.HandledFormat == format)) {
                //        MpConsole.WriteTraceLine($"Note! Attempting to unregister '{format}' because writer unregistered but a reader uses it so ignoring");
                //    } else {
                //        canUnregister = true;
                //    }

                //    if (canUnregister) {
                //        MpPortableDataFormats.UnregisterDataFormat(format);
                //    }
                //    OnPropertyChanged(nameof(FormatViewModels));
                //}
            });

        #endregion
    }
}
