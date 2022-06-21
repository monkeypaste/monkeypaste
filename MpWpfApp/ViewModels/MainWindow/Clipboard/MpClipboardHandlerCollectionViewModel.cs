
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
        MpISidebarItemViewModel { //
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

        public ObservableCollection<MpClipboardFormatPresetViewModel> DefaultFormatPresetViewModels { get; set; } = new ObservableCollection<MpClipboardFormatPresetViewModel>();

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

        public async Task Init() {
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
                // select most recent preset
                
                MpClipboardFormatPresetViewModel presetToSelect = null;
                foreach(var chivm in Items) {
                    foreach(var hivm in chivm.Items) {
                        foreach(var hipvm in hivm.Items) {
                            if(presetToSelect == null) {
                                presetToSelect = hipvm;
                            } else if(hipvm.LastSelectedDateTime > presetToSelect.LastSelectedDateTime) {
                                presetToSelect = hipvm;
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

        public ICommand UpdateDefaultFormatPresetCommand => new RelayCommand<object>(
            (presetVmArg) => {
                // TODO need a central view of current handled formats
                // its too confusing having 'Ignore' parameter and 'IsDefault' when multiple handlers are present
                var presetVm = presetVmArg as MpClipboardFormatPresetViewModel;
                if (presetVm == null) {
                    return;
                }
                foreach(var cihvm in Items) {
                    var hfvm = cihvm.Items.FirstOrDefault(x => x.HandledFormat == presetVm.Parent.HandledFormat);
                    if(hfvm == null || hfvm == presetVm.Parent) {
                        continue;
                    }
                }
            });
        #endregion
    }
}
