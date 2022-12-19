
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia.Input;
using Avalonia;
using System.Runtime.InteropServices;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipboardHandlerCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvClipboardHandlerItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpAvClipboardHandlerCollectionViewModel>,
        MpIOrientedSidebarItemViewModel,
        MpISidebarItemViewModel,
        MpIAsyncComboBoxViewModel,
        MpIClipboardFormatDataHandlers,
        MpIPlatformDataObjectHelperAsync { //

        #region Statics

        private static List<string> _oleReqGuids = new List<string>();

        #endregion

        #region Properties       

        #region View Models

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = @"_Transform",
                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ButterflyImage") as string,
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

        public IEnumerable<MpAvClipboardFormatPresetViewModel> AllAvailableWriterPresets {
            get {
                var aawpl = new List<MpAvClipboardFormatPresetViewModel>();
                foreach(var handlerItem in Items) {
                    foreach(var writerFormat in handlerItem.Writers) {
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

        public IEnumerable<MpIClipboardWriterComponent> EnabledWriterComponents =>
            EnabledWriters
            .Select(x => x.Parent.ClipboardPluginComponent)
            .Distinct()
            .Cast<MpIClipboardWriterComponent>();

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

        #region MpITreeItemViewModel Implementation

        public override MpITreeItemViewModel ParentTreeItem => null;

        #endregion

        #region MpIAsyncComboBoxViewModel Implementation

        IEnumerable<MpIComboBoxItemViewModel> MpIAsyncComboBoxViewModel.Items => Items;
        MpIComboBoxItemViewModel MpIAsyncComboBoxViewModel.SelectedItem {
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

        #region MpIOrientedSidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; }

        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return 350;
                } else {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight;
                } else {
                    return 300;
                }
            }
        }
        public bool IsSidebarVisible { get; set; }

        public MpISidebarItemViewModel NextSidebarItem => MpAvAnalyticItemCollectionViewModel.Instance;
        public MpISidebarItemViewModel PreviousSidebarItem => MpAvTagTrayViewModel.Instance;


        #endregion

        #region MpIPlatformDataObjectHelperAsync Implementation

        bool MpIPlatformDataObjectHelperAsync.IsOleBusy => IsBusy;

        async Task<object> MpIPlatformDataObjectHelperAsync.WriteDragDropDataObject(object idoObj) {
            if(idoObj is IDataObject ido) {
                object pdo = await WriteClipboardOrDropObjectAsync(ido, false, false);
                return pdo;
            }
            return null;
        }

        async Task MpIPlatformDataObjectHelperAsync.SetPlatformClipboardAsync(object idoObj, bool ignoreClipboardChange) {
            if (idoObj is IDataObject ido) {
                await WriteClipboardOrDropObjectAsync(ido, true, ignoreClipboardChange);
            }
        }

        async Task<object> MpIPlatformDataObjectHelperAsync.ReadDragDropDataObject(object idoObj, int retryCount) {
            if (idoObj is IDataObject ido) {
                var mpdo = await ReadClipboardOrDropObjectAsync(ido);
                return mpdo;
            }
            return null;
        }
        async Task<object> MpIPlatformDataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(bool ignorePlugins) {
            var pdo = await ReadClipboardOrDropObjectAsync(null,ignorePlugins);
            return pdo;
        }

        async Task MpIPlatformDataObjectHelperAsync.UpdateDragDropDataObjectAsync(object source, object target) {
            // NOTE this is called during a drag drop when user toggles a format preset
            // source should be the initial output of ContentView dataObject and should have the highest fidelity of data on it for conversions
            // NOTE DO NOT re-instantiate target haven't tested but I imagine the reference must persist that which was given to .DoDragDrop in StartDragging
            if(source is IDataObject sido &&
                target is IDataObject tido) {

                var temp = await WriteClipboardOrDropObjectAsync(sido, false, false);
                if(temp is IDataObject temp_ido) {
                    tido.CopyFrom(temp_ido);
                }
            } else {
                // need to cast or whats goin on here?
                Debugger.Break();
                return;
            }

        }
        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

        public bool IsSelected { get; set; }

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



        #endregion

        #region Private Methods

        private async Task<MpAvClipboardHandlerItemViewModel> CreateClipboardHandlerItemViewModelAsync(MpPluginFormat plugin) {
            MpAvClipboardHandlerItemViewModel aivm = new MpAvClipboardHandlerItemViewModel(this);
            await aivm.InitializeAsync(plugin);
            return aivm;
        }

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExternalDragBegin:
                    if (SelectedItem == null) {
                        Debugger.Break();
                    }
                    SelectedItem.IsDraggingToExternal = true;
                    break;
                case MpMessageType.ExternalDragEnd:
                    if (SelectedItem == null) {
                        Debugger.Break();
                    }
                    SelectedItem.IsDraggingToExternal = false;
                    break;
            }
        }

        private void MpClipboardHandlerCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                case nameof(IsSidebarVisible):
                    if (IsSidebarVisible) {
                        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpAvActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpAvAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
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
                    break;
                case nameof(IsHandlerDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsHandlerDropDownOpen;
                    break;
            }
        }

        private async Task<MpAvDataObject> ReadClipboardOrDropObjectAsync(IDataObject forced_ido = null, bool ignorePlugins = false) {
            // NOTE forcedDataObject is used to read drag/drop, when null clipboard is read
            await WaitForBusyAsync();

            MpAvDataObject mpdo = new MpAvDataObject();

            foreach (var read_component in EnabledReaderComponents) {
                var reader_request = new MpClipboardReaderRequest() {
                    ignoreParams = ignorePlugins,
                    isAvalonia = true,
                    mainWindowImplicitHandle = MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle.ToInt32(),
                    platform = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                    readFormats = EnabledReaders.Where(x => x.Parent.ClipboardPluginComponent == read_component).Select(x => x.Parent.HandledFormat).Distinct().ToList(),
                    items = EnabledReaders.Where(x => x.Parent.ClipboardPluginComponent == read_component).SelectMany(x => x.Items.Cast<MpIParameterKeyValuePair>()).ToList(),
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
                    reader_response.dataObject.DataFormatLookup.ForEach(x => mpdo.DataFormatLookup.AddOrReplace(x.Key, x.Value));
                } else {
                    MpConsole.WriteLine("Invalid cb reader response: " + reader_response);
                }
            }
            mpdo.MapAllPseudoFormats();
            IsBusy = false;
            return mpdo;
        }

        private async Task<object> WriteClipboardOrDropObjectAsync(IDataObject ido, bool writeToClipboard, bool ignoreClipboardChange) {
            await WaitForBusyAsync();

            if (ignoreClipboardChange) {
                MpPlatformWrapper.Services.ClipboardMonitor.StopMonitor();
            }
            // pre-pass data object and remove disabled formats
            var formatsToRemove =
                ido.GetAllDataFormats()
                .Where(x => EnabledWriters.All(y => y.ClipboardFormat.clipboardName != x))
                .Select(x => x);

            formatsToRemove.ForEach(x => ido.TryRemove(x));


            var dobj = new MpAvDataObject();

            foreach (var write_component in EnabledWriterComponents) {
                var write_request = new MpClipboardWriterRequest() {
                    data = ido,
                    writeToClipboard = writeToClipboard,
                    writeFormats = EnabledWriters.Where(x => x.Parent.ClipboardPluginComponent == write_component).Select(x => x.Parent.HandledFormat).Distinct().ToList(),
                    items = EnabledWriters.Where(x => x.Parent.ClipboardPluginComponent == write_component).SelectMany(x => x.Items.Cast<MpIParameterKeyValuePair>()).ToList(),
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

            MpConsole.WriteLine("Data written to " + (writeToClipboard ? "CLIPBOARD" : "DATAOBJECT") + ":");
            dobj.GetAllDataFormats().ForEach(x => MpConsole.WriteLine("Format: " + x));

            if (ignoreClipboardChange) {
                MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();
            }
            IsBusy = false;
            return dobj;
        }


        private async Task WaitForBusyAsync() {
            if(IsBusy) {
                string req_guid = System.Guid.NewGuid().ToString();
                _oleReqGuids.Add(req_guid);
                while(true) {
                    if(!IsBusy && _oleReqGuids.First() == req_guid) {
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
                        Debugger.Break();
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
