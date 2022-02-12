using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FFImageLoading.Helpers.Exif;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using Xamarin.Forms.Internals;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public enum MpAnalyzerType {
        None = 0,
        LanguageTranslator,
        OpenAi,
        Yolo,
        AzureImageAnalysis
    }

    public class MpAnalyticItemCollectionViewModel : 
        MpSelectorViewModelBase<object,MpAnalyticItemViewModel>,
        MpIMenuItemViewModel,
        MpISingletonViewModel<MpAnalyticItemCollectionViewModel>, 
        MpITreeItemViewModel,
        MpISidebarItemViewModel { //
        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                List<MpMenuItemViewModel> subItems = Items.SelectMany(x => x.QuickActionPresetMenuItems).ToList();
                if(subItems.Count > 0) {
                    subItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                subItems.AddRange(Items.Select(x => x.MenuItemViewModel));

                return new MpMenuItemViewModel() {
                    Header = @"_Analyze",
                    IconResourceKey = Application.Current.Resources["BrainIcon"] as string,
                    SubItems = subItems
                };
            }
        }

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpITreeItemViewModel ParentTreeItem => null;

        public List<MpAnalyticItemPresetViewModel> AllPresets {
            get {
                return Items.OrderBy(x => x.Title).SelectMany(x => x.Items).ToList();
            }
        }

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel {
            get {
                if(SelectedItem == null) {
                    return null;
                }
                return SelectedItem.SelectedItem;
            }
        }

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultAnalyzerPanelWidth;
        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultAnalyzerPanelWidth;
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


        public bool IsAnyEditingParameters => Items.Any(x => x.IsAnyEditingParameters);

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

        public async Task Init() {
            IsBusy = true;

            Items.Clear();

            //var ail = await MpDb.GetItemsAsync<MpAnalyticItem>();
            //ail.Reverse();
            //foreach (var ai in ail) {
            //    if(Uri.IsWellFormedUriString(ai.EndPoint,UriKind.Absolute)) {
            //        var aivm = await CreateAnalyticItemViewModel(ai);
            //        Items.Add(aivm);
            //    }                
            //}

            var pail = MpPluginManager.Plugins.Where(x => x.LoadedComponent is MpIAnalyzerPluginComponent);
            foreach(var pai in pail) {
                var paivm = await CreateAnalyticItemViewModel(pai);
                Items.Add(paivm);
            }
            OnPropertyChanged(nameof(Items));
            
            if (Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            IsBusy = false;
        }

        public MpAnalyticItemPresetViewModel GetPresetViewModelById(int aipid) {
            var aipvm = Items.SelectMany(x => x.Items).FirstOrDefault(x => x.AnalyticItemPresetId == aipid);
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

        //private async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpAnalyticItem ai) {
        //    MpAnalyticItemViewModel aivm = null; 
        //    switch(ai.Title) {
        //        case "Open Ai":
        //            aivm = new MpOpenAiViewModel(this);
        //            break;
        //        case "Language Translator":
        //            aivm = new MpTranslatorViewModel(this);
        //            break;
        //        case "Yolo":
        //            aivm = new MpYoloViewModel(this);
        //            break;
        //        case "Azure Image Analysis":
        //            aivm = new MpAzureImageAnalysisViewModel(this);
        //            break;
        //    }
        //    await aivm.InitializeAsync(ai);
        //    return aivm;
        //}

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
                    MpSidebarViewModel.Instance.OnPropertyChanged(nameof(MpSidebarViewModel.Instance.IsAnySidebarOpen));
                    
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    if (IsSidebarVisible) {
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    if(Items.Count > 0) {
                        if (SelectedItem == null) {
                            Items[0].IsSelected = true;
                            SelectedItem.Items.ForEach(x => x.IsEditingParameters = false);
                        }
                        if (!SelectedItem.IsAnyEditingParameters) {
                            SelectedItem.Items.ForEach(x => x.IsSelected = x == SelectedItem.Items[0]);
                            //SelectedItem.Items.ForEach(x => x.IsEditing = x == SelectedItem.Items[0]);
                        }
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(Children));
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ManageItemCommand => new RelayCommand<object>(
            (itemGuid) => {
                Items.ForEach(x => x.IsSelected = x.AnalyzerPluginGuid == itemGuid.ToString());
                SelectedItem.ManageAnalyticItemCommand.Execute(null);
            }, (itemId) => itemId != null);

        public ICommand ManagePresetCommand => new RelayCommand<object>(
            (presetId) => {
                var aipvm = AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == (int)presetId);
                if(aipvm == null) {
                    return;
                }

                aipvm.ManagePresetCommand.Execute(null);
            }, (presetId) => presetId != null);

        public ICommand RegisterContentCommand => new RelayCommand<object>(
            (args) => {
                Content = args;
            },
            (args) => args != null);

        public ICommand ExecuteAnalysisCommand => new RelayCommand<object>(
            (args) => {
                if(args is object[] argParts) {
                    MpAnalyticItemPresetViewModel aipvm = null;
                    if(argParts[0] is MpAnalyzerType analyzerType) {

                    }

                }
            },
            (args) => args != null);
        #endregion
    }
}
