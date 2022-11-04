
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
        MpAvSelectorViewModelBase<object, MpAvClipboardHandlerItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpAvClipboardHandlerCollectionViewModel>,
        MpITreeItemViewModel,
        MpISidebarItemViewModel,
        MpIClipboardFormatDataHandlers,
        MpIPlatformDataObjectHelperAsync { //
        #region Properties

        #region MpITreeItemViewModel Implementation

        public IEnumerable<MpITreeItemViewModel> Children => Items;// new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpITreeItemViewModel ParentTreeItem => null;

        public bool IsExpanded { get; set; }

        #endregion

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

        //private ObservableCollection<MpClipboardFormatViewModel> _formatViewModels;
        //public ObservableCollection<MpClipboardFormatViewModel> FormatViewModels {
        //    get {
        //        if(_formatViewModels == null) {
        //            _formatViewModels = new ObservableCollection<MpClipboardFormatViewModel>();
        //        }
        //        return _formatViewModels;
        //    }
        //}
        public IEnumerable<MpAvClipboardFormatViewModel> FormatViewModels =>
            MpPortableDataFormats.RegisteredFormats.Select(x => new MpAvClipboardFormatViewModel(this, x));

        //public Dictionary<string, MpClipboardFormatPresetViewModel> EnabledFormatHandlerLookup {
        //    get {
        //        var efl = new Dictionary<string, MpClipboardFormatPresetViewModel>();
        //        foreach(var chivm in Items) {
        //            foreach(var hcfvm in chivm.Items) {
        //                foreach(var cpvm in hcfvm.Items) {
        //                    if(cpvm.IsDefault) {
        //                        efl.Add(cpvm.Parent.HandledFormat, cpvm);
        //                    }
        //                }
        //            }
        //        }
        //        return efl;
        //    }
        //}

        public IEnumerable<MpAvClipboardFormatPresetViewModel> EnabledFormats {
            get {
                foreach (var i in Items) {
                    foreach (var j in i.Items) {
                        foreach (var k in j.Items) {
                            if (k.IsEnabled) {
                                yield return k;
                            }
                        }
                    }
                }
            }
        }


        //public Dictionary<string, MpClipboardFormatPresetViewModel> EnabledReaderLookup =>
        //    Enab
        //    .Where(x => x.Value.Parent.IsReader)
        //    .ToDictionary(
        //        x => x.Key,
        //        x => x.Value);

        //public Dictionary<string, MpClipboardFormatPresetViewModel> EnabledWriterLookup =>
        //    EnabledFormatHandlerLookup
        //    .Where(x => x.Value.Parent.IsWriter)
        //    .ToDictionary(
        //        x => x.Key,
        //        x => x.Value);
        #endregion

        #region MpIClipboardFormatHandlers Implementation
        public IEnumerable<MpIClipboardPluginComponent> Handlers => EnabledFormats.Select(x => x.Parent.ClipboardPluginComponent).Distinct();

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => 350;// MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;

        public MpISidebarItemViewModel NextSidebarItem => SelectedPresetViewModel;

        public MpISidebarItemViewModel PreviousSidebarItem => null;

        #endregion

        #region MpIPlatformDataObjectHelperAsync Implementation

        async Task<MpPortableDataObject> MpIPlatformDataObjectHelperAsync.ReadDragDropDataObject(object nativeDataObj, int retryCount) {
            var mpdo = await ReadClipboardOrDropObjectAsync(nativeDataObj);
            return mpdo;
        }

        async Task<object> MpIPlatformDataObjectHelperAsync.WriteDragDropDataObject(MpPortableDataObject mpdo) {
            object pdo = await WriteClipboardOrDropObjectAsync(mpdo, false);
            return pdo;
        }

        async Task MpIPlatformDataObjectHelperAsync.SetPlatformClipboardAsync(MpPortableDataObject portableObj) {
            await WriteClipboardOrDropObjectAsync(portableObj, true);
        }

        async Task<MpPortableDataObject> MpIPlatformDataObjectHelperAsync.GetPlatformClipboardDataObjectAsync() {
            var pdo = await ReadClipboardOrDropObjectAsync(null);
            return pdo;
        }

        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }


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

                            //if(hipvm.IsEnabled) {
                            //    if(hipvm.CanWrite && hipvm.CanRead) {
                            //        Debugger.Break();
                            //    }
                            //    if(hipvm.CanRead) {
                            //        ToggleFormatPresetIsReadEnabledCommand.Execute(hipvm);
                            //    }
                            //    if(hipvm.CanWrite) {
                            //        ToggleFormatPresetIsWriteEnabledCommand.Execute(hipvm);
                            //    }
                            //bool replace = true;
                            //if (EnabledFormatHandlerLookup.TryGetValue(hipvm.Parent.HandledFormat, out var curPreset)) {

                            //    string errorMsg = $"Warning clipboard format handler conflict for {curPreset.FullName} and {hipvm.FullName}";
                            //    // two handled formats are marked as default so check which was last selected and use that one if none override previous
                            //    if(curPreset.LastSelectedDateTime > hipvm.LastSelectedDateTime) {
                            //        replace = false;
                            //        errorMsg += $" {curPreset} is more recent so ignoring {hipvm}";
                            //    } else if(curPreset.LastSelectedDateTime == hipvm.LastSelectedDateTime) {
                            //        errorMsg += $" have same date time {hipvm.LastSelectedDateTime} so using {hipvm}";
                            //    } else {
                            //        errorMsg += $" {hipvm} is more recent so ignoring {curPreset}";
                            //    }
                            //    MpConsole.WriteTraceLine(errorMsg);
                            //    if(!replace) {
                            //        continue;
                            //    }
                            //}
                            //EnabledFormatHandlerLookup.AddOrReplace(hipvm.Parent.HandledFormat, hipvm);
                            //
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

        public async Task UpdateDragDropDataObjectAsync(MpPortableDataObject source, MpPortableDataObject target) {
            // NOTE this is called during a drag drop when user toggles a format preset
            // source should be the initial output of ContentView dataObject and should have the highest fidelity of data on it for conversions
            // NOTE DO NOT re-instantiate target haven't tested but I imagine the reference must persist that which was given to .DoDragDrop in StartDragging


        }

        public async Task<MpAvDataObject> ReadClipboardOrDropObjectAsync(object forcedDataObject = null) {
            // NOTE forcedDataObject is used to read drag/drop, when null clipboard is read
            MpAvDataObject mpdo = new MpAvDataObject();

            
            //only iterate through actual handlers 
            var handlers = EnabledFormats.Where(x => x.CanRead)
                                         .Select(x => x.Parent.ClipboardPluginComponent)
                                         .Distinct().Cast<MpIClipboardReaderComponentAsync>();
            //MpConsole.WriteLine("Handlers available: " + handlers.Count());
            foreach (var handler in handlers) {
                var req = new MpClipboardReaderRequest() {
                    isAvalonia = true,
                    mainWindowImplicitHandle = MpPlatformWrapper.Services.ProcessWatcher.ThisAppHandle.ToInt32(),
                    platform = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                    readFormats = EnabledFormats.Where(x => x.Parent.ClipboardPluginComponent == handler).Select(x => x.Parent.HandledFormat).Distinct().ToList(),
                    items = EnabledFormats.Where(x => x.Parent.ClipboardPluginComponent == handler).SelectMany(x => x.Items.Cast<MpIParameterKeyValuePair>()).ToList(),
                    forcedClipboardDataObject = forcedDataObject
                };
                
                var response = await handler.ReadClipboardDataAsync(req);

                bool isValid = MpPluginTransactor.ValidatePluginResponse(response);
                if (isValid) {
                    response.dataObject.DataFormatLookup.ForEach(x => mpdo.DataFormatLookup.AddOrReplace(x.Key, x.Value));
                } else {
                    MpConsole.WriteLine("Invalid cb reader response: " + response);
                }
            }
            mpdo.MapAllPseudoFormats();
            return mpdo;
        }

        public async Task<object> WriteClipboardOrDropObjectAsync(MpPortableDataObject mpdo, bool writeToClipboard) {
            var dobj = new MpAvDataObject();
            var handlers = EnabledFormats.Where(x => x.CanWrite && MpPortableDataFormats.RegisteredFormats.Contains(x.Parent.HandledFormat)).Distinct();
                                         //.Select(x => x.Parent.ClipboardPluginComponent).Distinct();//.Cast<MpIClipboardWriterComponentAsync>();

            foreach(var handler in handlers) {
                var writeRequest = new MpClipboardWriterRequest() {
                    data = mpdo,
                    writeToClipboard = writeToClipboard,
                    items = handler.Items.Cast<MpIParameterKeyValuePair>().ToList()
                };
                var writer_component = handler.Parent.ClipboardPluginComponent as MpIClipboardWriterComponentAsync;
                if (writer_component == null) {
                    Debugger.Break();
                }
                MpClipboardWriterResponse writerResponse = await writer_component.WriteClipboardDataAsync(writeRequest);
                bool isValid = MpPluginTransactor.ValidatePluginResponse(writerResponse);
                if (isValid && writerResponse.platformDataObject is MpPortableDataObject ido) {
                    ido.DataFormatLookup.Where(x => x.Value != null).ForEach(x => dobj.SetData(x.Key.Name, x.Value));
                }
            }

            MpConsole.WriteLine("Data written to " + (writeToClipboard ? "CLIPBOARD" : "DATAOBJECT")+":");
            mpdo.DataFormatLookup.ForEach(x => MpConsole.WriteLine("Format: " + x.Key.Name));

            return dobj;
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
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight));
                    if (IsSidebarVisible) {
                        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
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
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleFormatPresetIsEnabled => new MpCommand<object>(
            (presetVmArg) => {
                if (presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm) {
                    if (cfpvm.CanRead) {
                        ToggleFormatPresetIsReadEnabledCommand.Execute(cfpvm);
                    } else if (cfpvm.CanWrite) {
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
                    var otherEnabled = EnabledFormats.Where(x => x.CanRead).FirstOrDefault(x => x.Parent.HandledFormat == presetVm.Parent.HandledFormat && x.PresetId != presetVm.PresetId);
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
            }, (presetVmArg) => presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm && cfpvm.CanRead);


        public ICommand ToggleFormatPresetIsWriteEnabledCommand => new MpCommand<object>(
            (presetVmArg) => {
                var presetVm = presetVmArg as MpAvClipboardFormatPresetViewModel;
                if (presetVm == null) {
                    return;
                }
                var handlerVm = presetVm.Parent;

                if (presetVm.IsEnabled) {
                    //when toggled on, untoggle any other preset handling same format
                    var otherEnabled = EnabledFormats.Where(x => x.CanWrite).FirstOrDefault(x => x.Parent.HandledFormat == presetVm.Parent.HandledFormat && x.PresetId != presetVm.PresetId);
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
                OnPropertyChanged(nameof(FormatViewModels));
            }, (presetVmArg) => presetVmArg is MpAvClipboardFormatPresetViewModel cfpvm && cfpvm.CanRead);

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
                //    } else if (isReadUnregister && EnabledFormats.Any(x => x.CanWrite && x.Parent.HandledFormat == format)) {
                //        MpConsole.WriteTraceLine($"Note! Attempting to unregister '{format}' because read unregistered but a writer uses it so ignoring");
                //    } else if (isWriteUnregister && EnabledFormats.Any(x => x.CanRead && x.Parent.HandledFormat == format)) {
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
