
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using FFImageLoading.Helpers.Exif;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {

    public class MpClipboardHandlerCollectionViewModel : 
        MpSelectorViewModelBase<object,MpClipboardHandlerItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpClipboardHandlerCollectionViewModel>, 
        MpITreeItemViewModel,
        MpISidebarItemViewModel,
        MpIClipboardFormatDataHandlers { //
        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = @"_Transform",
                    IconResourceKey = Application.Current.Resources["ButterflyIcon"] as string,
                    SubItems = Items.Select(x => x.MenuItemViewModel).ToList()
                };
            }
        }

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpITreeItemViewModel ParentTreeItem => null;

        public List<MpClipboardFormatPresetViewModel> AllPresets {
            get {
                return Items.SelectMany(x => x.Items.SelectMany(y=>y.Items)).ToList();
            }
        }

        public MpClipboardFormatPresetViewModel SelectedPresetViewModel {
            get {
                if(SelectedItem == null) {
                    return null;
                }
                if(SelectedItem.SelectedItem == null) {
                    if(SelectedItem.Items.Count > 0) {
                        SelectedItem.Items[0].IsSelected = true;
                    } else {
                        return null;
                    }
                }
                return SelectedItem.SelectedItem.SelectedItem;
            }
        }


        public Dictionary<string, MpClipboardFormatPresetViewModel> DefaultFormatHandlerLookup { get; private set; } = new Dictionary<string, MpClipboardFormatPresetViewModel>();

        public Dictionary<string, MpClipboardFormatPresetViewModel> DefaultReaders =>
            DefaultFormatHandlerLookup
            .Where(x => x.Value.Parent.ClipboardPluginComponent is MpIClipboardReaderComponent)
            .ToDictionary(
                x => x.Key,
                x => x.Value);

        public Dictionary<string, MpClipboardFormatPresetViewModel> DefaultWriters =>
            DefaultFormatHandlerLookup
            .Where(x => x.Value.Parent.ClipboardPluginComponent is MpIClipboardWriterComponent)
            .ToDictionary(
                x => x.Key,
                x => x.Value);
        #endregion

        #region MpIClipboardFormatHandlers Implementation
        public IEnumerable<MpIClipboardPluginComponent> Handlers => DefaultFormatHandlerLookup.Select(x => x.Value.Parent.ClipboardPluginComponent);

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;

        public MpISidebarItemViewModel NextSidebarItem => SelectedPresetViewModel;

        public MpISidebarItemViewModel PreviousSidebarItem => null;

        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsHovering { get; set; }

        public bool IsLoaded => Items.Count > 0;

        public bool IsExpanded { get; set; }


        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        private static MpClipboardHandlerCollectionViewModel _instance;
        public static MpClipboardHandlerCollectionViewModel Instance => _instance ?? (_instance = new MpClipboardHandlerCollectionViewModel());


        public MpClipboardHandlerCollectionViewModel() : base(null) {
            PropertyChanged += MpClipboardHandlerCollectionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            MpMessenger.Register<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            Items.Clear();

            var pail = MpPluginManager.Plugins.Where(x => x.Value.Component is MpIClipboardPluginComponent);
            foreach(var pai in pail) {
                var paivm = await CreateClipboardHandlerItemViewModel(pai.Value);
                Items.Add(paivm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));
            
            if (Items.Count > 0) {
                // select most recent preset and init default handlers

                DefaultFormatHandlerLookup.Clear();
                MpClipboardFormatPresetViewModel presetToSelect = null;
                foreach(var chivm in Items) {
                    foreach(var hivm in chivm.Items) {                        
                        foreach(var hipvm in hivm.Items) {
                            if(presetToSelect == null) {
                                presetToSelect = hipvm;
                            } else if(hipvm.LastSelectedDateTime > presetToSelect.LastSelectedDateTime) {
                                presetToSelect = hipvm;
                            }

                            if(hipvm.IsDefault) {
                                bool replace = true;
                                if (DefaultFormatHandlerLookup.TryGetValue(hipvm.Parent.HandledFormat, out var curPreset)) {
                                    
                                    string errorMsg = $"Warning clipboard format handler conflict for {curPreset.FullName} and {hipvm.FullName}";
                                    // two handled formats are marked as default so check which was last selected and use that one if none override previous
                                    if(curPreset.LastSelectedDateTime > hipvm.LastSelectedDateTime) {
                                        replace = false;
                                        errorMsg += $" {curPreset} is more recent so ignoring {hipvm}";
                                    } else if(curPreset.LastSelectedDateTime == hipvm.LastSelectedDateTime) {
                                        errorMsg += $" have same date time {hipvm.LastSelectedDateTime} so using {hipvm}";
                                    } else {
                                        errorMsg += $" {hipvm} is more recent so ignoring {curPreset}";
                                    }
                                    MpConsole.WriteTraceLine(errorMsg);
                                    if(!replace) {
                                        continue;
                                    }
                                }
                                DefaultFormatHandlerLookup.AddOrReplace(hipvm.Parent.HandledFormat, hipvm);
                            }
                        }
                    }
                }                

                if(presetToSelect != null) {
                    presetToSelect.Parent.SelectedItem = presetToSelect;
                    presetToSelect.Parent.Parent.SelectedItem = presetToSelect.Parent;
                    SelectedItem = presetToSelect.Parent.Parent;
                }




            }

            OnPropertyChanged(nameof(SelectedItem));

            IsBusy = false;
        }


        #endregion

        #region Private Methods
        
        private async Task<MpClipboardHandlerItemViewModel> CreateClipboardHandlerItemViewModel(MpPluginFormat plugin) {
            MpClipboardHandlerItemViewModel aivm = new MpClipboardHandlerItemViewModel(this);

            await aivm.InitializeAsync(plugin);
            return aivm;
        }

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExternalDragBegin:
                    if(SelectedItem == null) {
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
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    if (IsSidebarVisible) {
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(Children));
                    break;
                case nameof(SelectedPresetViewModel):
                    if(SelectedPresetViewModel == null) {
                        return;
                    }
                    CollectionViewSource.GetDefaultView(SelectedPresetViewModel.Items).Refresh();
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(SelectedPresetViewModel));
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleFormatPresetIsDefaultCommand => new RelayCommand<object>(
            (presetVmArg) => {
                
                var presetVm = presetVmArg as MpClipboardFormatPresetViewModel;
                if (presetVm == null) {
                    return;
                }
                var handlerVm = presetVm.Parent;

                if(presetVm.IsDefault) {
                    //when toggled  untoggle any other preset handling same format
                    if(DefaultFormatHandlerLookup.ContainsKey(handlerVm.HandledFormat)) {
                        var untoggled_preset = DefaultFormatHandlerLookup[handlerVm.HandledFormat];
                        // setting IsDefault triggers this command and removes the old handler
                        untoggled_preset.IsDefault = false;
                    }

                    DefaultFormatHandlerLookup.AddOrReplace(handlerVm.HandledFormat, presetVm);

                } else {
                    // when preset isDefault = false
                    if(DefaultFormatHandlerLookup.TryGetValue(handlerVm.HandledFormat, out var curHandlerPreset)) {
                        // when format has a handler
                        if(curHandlerPreset.PresetId == presetVm.PresetId) {
                            // when preset WAS the default handler remove it
                            DefaultFormatHandlerLookup.Remove(handlerVm.HandledFormat);
                        }
                    }
                    //otherwise ignore
                }

                OnPropertyChanged(nameof(DefaultFormatHandlerLookup));
            });
        #endregion
    }

    public interface MpIIsCheckable {
        bool? IsChecked { get; set; }
    }
}
