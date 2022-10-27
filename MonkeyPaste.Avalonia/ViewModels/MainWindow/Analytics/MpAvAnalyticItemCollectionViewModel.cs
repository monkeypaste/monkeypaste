using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;


namespace MonkeyPaste.Avalonia {
    public enum MpAnalyzerType {
        None = 0,
        LanguageTranslator,
        OpenAi,
        Yolo,
        AzureImageAnalysis
    }

    public class MpAnalyticItemCollectionViewModel : 
        MpAvSelectorViewModelBase<object,MpAnalyticItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpAnalyticItemCollectionViewModel>, 
        MpITreeItemViewModel,
        MpIOrientedSidebarItemViewModel {
        #region Private Variables

        private string _processAutomationGuid = "e7e25c85-1c8f-4e79-be8f-2ebfcb5bb94e";
        private string _httpAutomationGuid = "084abd2e-801d-4637-9054-b42f1b159c32";

        #endregion

        #region Properties

        #region MpITreeItemViewModel Implementation

        public IEnumerable<MpITreeItemViewModel> Children => Items;

        public MpITreeItemViewModel ParentTreeItem => null;
        public bool IsExpanded { get; set; }
        #endregion

        #region View Models

        public MpAnalyticItemViewModel ProcessAutomationViewModel => Items.FirstOrDefault(x => x.PluginGuid == _processAutomationGuid);

        public MpAnalyticItemViewModel HttpAutomationViewModel => Items.FirstOrDefault(x => x.PluginGuid == _httpAutomationGuid);

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                List<MpMenuItemViewModel> subItems = Items.SelectMany(x => x.QuickActionPresetMenuItems).ToList();
                if(subItems.Count > 0) {
                    subItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                subItems.AddRange(Items.Select(x => x.ContextMenuItemViewModel));

                return new MpMenuItemViewModel() {
                    Header = @"_Analyze",
                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BrainImage") as string,
                    SubItems = subItems
                };
            }
        }

        

        public List<MpAnalyticItemPresetViewModel> AllPresets => Items.OrderBy(x => x.Title).SelectMany(x => x.Items).ToList();

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel {
            get {
                if(SelectedItem == null) {
                    return null;
                }
                return SelectedItem.SelectedItem;
            }
        }

        #endregion

        #region MpIOrientedSidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;
        public double DefaultSidebarWidth => 350;// MpMeasurements.Instance.DefaultAnalyzerPresetPanelWidth;

        public double DefaultSidebarHeight => 350;
        public double SidebarHeight { get; set; } = 0;

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


        //public bool IsAnyEditingParameters => Items.Any(x => x.IsAnyEditingParameters);

        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        private static MpAnalyticItemCollectionViewModel _instance;
        public static MpAnalyticItemCollectionViewModel Instance => _instance ?? (_instance = new MpAnalyticItemCollectionViewModel());


        public MpAnalyticItemCollectionViewModel() : base(null) {
            PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            //while(MpIconCollectionViewModel.Instance.IsAnyBusy) {
            //    await Task.Delay(100);
            //}
            Items.Clear();

            var pail = MpPluginLoader.Plugins.Where(x => x.Value.Component is MpIAnalyzeAsyncComponent || x.Value.Component is MpIAnalyzerComponent);
            foreach(var pai in pail) {
                var paivm = await CreateAnalyticItemViewModel(pai.Value);
                Items.Add(paivm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));
            
            if (Items.Count > 0) {
                // select most recent preset
                MpAnalyticItemPresetViewModel presetToSelect = Items
                            .Aggregate((a, b) => a.Items.Max(x => x.LastSelectedDateTime) > b.Items.Max(x => x.LastSelectedDateTime) ? a : b)
                            .Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if(presetToSelect != null) {
                    presetToSelect.Parent.SelectedItem = presetToSelect;
                    SelectedItem = presetToSelect.Parent;
                }                
            }

            OnPropertyChanged(nameof(SelectedItem));

            IsBusy = false;
        }

        public MpAnalyticItemPresetViewModel GetPresetViewModelById(int aipid) {
            var aipvm = Items.SelectMany(x => x.Items).FirstOrDefault(x => x.AnalyticItemPresetId == aipid);
            return aipvm;
        }

        public MpAnalyticItemPresetViewModel GetPresetViewModelByGuid(string guid) {
            var aipvm = Items.SelectMany(x => x.Items).FirstOrDefault(x => x.PresetGuid == guid);
            return aipvm;
        }

        public MpAnalyticItemPresetViewModel GetDefaultPresetByAnalyzerType(MpAnalyzerType analyzerType) {
            string title = analyzerType.EnumToLabel();
            var aivm = Items.FirstOrDefault(x => x.Title.ToLower() == title.ToLower());
            if(aivm == null) {
                return null;
            }
            return aivm.DefaultPresetViewModel;
        }

        #endregion

        #region Private Methods
        
        private async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpPluginFormat plugin) {
            MpAnalyticItemViewModel aivm = new MpAnalyticItemViewModel(this);

            await aivm.InitializeAsync(plugin);
            return aivm;
        }

        private void MpAnalyticItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                case nameof(IsSidebarVisible):                    
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayScreenHeight));
                    if (IsSidebarVisible) {
                        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(Children));
                    break;
                case nameof(SelectedPresetViewModel):
                    if(SelectedPresetViewModel == null) {
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
        #endregion
    }
}
