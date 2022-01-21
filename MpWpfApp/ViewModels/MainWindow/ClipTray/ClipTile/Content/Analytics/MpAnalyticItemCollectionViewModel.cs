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

namespace MpWpfApp {
    public enum MpAnalyzerType {
        None = 0,
        LanguageTranslator,
        OpenAi,
        Yolo,
        AzureImageAnalysis
    }

    public class MpAnalyticItemCollectionViewModel : MpViewModelBase, MpISingletonViewModel<MpAnalyticItemCollectionViewModel>, MpITreeItemViewModel  { //
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedItem) {
                    Items.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                }
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> ContextMenuItems {
            get {
                if(MpIconCollectionViewModel.Instance == null) {
                    return new ObservableCollection<MpContextMenuItemViewModel>();
                }
                var pmic = new List<MpContextMenuItemViewModel>();
                foreach (var item in Items) {
                    var imivm = new MpContextMenuItemViewModel() {
                        Header = item.Title,
                        IconId = item.IconId,
                        SubItems = item.ContextMenuItems
                    };

                    imivm.SubItems.Add(
                        new MpContextMenuItemViewModel() {
                            Header = "Manage",
                            Command = ManageItemCommand,
                            CommandParameter = item.AnalyticItemId
                        });

                    pmic.Add(imivm);
                }
                //if(QuickActionContextMenuItems.Count > 0) {                    
                //    pmic.InsertRange(0, QuickActionContextMenuItems);
                //    pmic.Insert(QuickActionContextMenuItems.Count, new MpContextMenuItemViewModel());
                //}
                return new ObservableCollection<MpContextMenuItemViewModel>(pmic);
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> QuickActionContextMenuItems {
            get {
                var qamivml = new List<MpContextMenuItemViewModel>();
                foreach (var item in Items) {
                    if (item.PresetViewModels.Any(x => x.IsQuickAction)) {
                        qamivml.AddRange(item.PresetViewModels.Where(x => x.IsQuickAction).Select(x => x.ContextMenuItemViewModel));
                    }
                }
                return new ObservableCollection<MpContextMenuItemViewModel>(qamivml);
            }
        }

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpITreeItemViewModel ParentTreeItem => MpSideBarTreeCollectionViewModel.Instance;

        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

        public bool IsSelected { get; set; }

        public bool IsAnySelected => SelectedItem != null;

        public bool IsHovering { get; set; }

        public bool IsLoaded => Items.Count > 0;

        public bool IsExpanded { get; set; }

        public bool IsVisible { get; set; } = false;
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

            var ail = await MpDb.GetItemsAsync<MpAnalyticItem>();
            ail.Reverse();
            foreach (var ai in ail) {
                var aivm = await CreateAnalyticItemViewModel(ai);
                Items.Add(aivm);
            }

            OnPropertyChanged(nameof(Items));
            
            if (Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            IsBusy = false;
        }

        public MpAnalyticItemPresetViewModel GetPresetViewModelById(int aipid) {
            var aipvm = Items.SelectMany(x => x.PresetViewModels).FirstOrDefault(x => x.AnalyticItemPresetId == aipid);
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

        private async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpAnalyticItem ai) {
            MpAnalyticItemViewModel aivm = null;
            switch(ai.Title) {
                case "Open Ai":
                    aivm = new MpOpenAiViewModel(this);
                    break;
                case "Language Translator":
                    aivm = new MpTranslatorViewModel(this);
                    break;
                case "Yolo":
                    aivm = new MpYoloViewModel(this);
                    break;
                case "Azure Image Analysis":
                    aivm = new MpAzureImageAnalysisViewModel(this);
                    break;
            }
            await aivm.InitializeAsync(ai);
            return aivm;
        }

        private void MpAnalyticItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                case nameof(IsVisible):
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.IsGridSplitterEnabled));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeButtonGridMinWidth));
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    if (IsVisible) {
                        MpTagTrayViewModel.Instance.IsVisible = false;
                        MpMatcherCollectionViewModel.Instance.IsVisible = false;
                    }
                    if(SelectedItem == null) {
                        Items[0].IsSelected = true;
                        SelectedItem.PresetViewModels.ForEach(x => x.IsEditingParameters = false);
                    }
                    if(!SelectedItem.IsAnyEditingParameters) {
                        SelectedItem.PresetViewModels.ForEach(x => x.IsSelected = x == SelectedItem.PresetViewModels[0]);
                        //SelectedItem.PresetViewModels.ForEach(x => x.IsEditing = x == SelectedItem.PresetViewModels[0]);
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
            (itemId) => {
                Items.ForEach(x => x.IsSelected = x.AnalyticItemId == (int)itemId);
                SelectedItem.ManageAnalyticItemCommand.Execute(null);
            }, (itemId) => itemId != null);

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
