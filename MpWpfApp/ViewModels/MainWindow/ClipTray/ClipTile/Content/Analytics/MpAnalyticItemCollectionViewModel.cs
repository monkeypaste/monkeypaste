using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MpAnalyticItemCollectionViewModel : MpSingletonViewModel<MpAnalyticItemCollectionViewModel> { //
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; private set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem {
            get {
                return Items.FirstOrDefault(x => x.IsSelected);
            }
            set {
                Items.ForEach(x => x.IsSelected = x == value);
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> ContextMenuItems {
            get {
                var pmic = new List<MpContextMenuItemViewModel>();

                foreach (var item in Items) {
                    var imivm = new MpContextMenuItemViewModel(
                        header: item.Title,
                        command: null,
                        commandParameter: null,
                        isChecked: null,
                        iconSource: item.ItemIconBase64,
                        subItems: item.ContextMenuItems,
                        inputGestureText: string.Empty,
                        bgBrush: null);


                    imivm.SubItems.Add(
                        new MpContextMenuItemViewModel(
                                    header: "Manage...",
                                    command: ManageItemCommand,
                                    commandParameter: item.AnalyticItemId,
                                    isChecked: null,
                                    iconSource: null,//Application.Current.Resources["CogIcon"] as string,
                                    subItems: null,
                                    inputGestureText: string.Empty,
                                    bgBrush: null));

                    pmic.Add(imivm);
                }

                if (pmic.Count == 0) {
                    return null;
                }
                return new ObservableCollection<MpContextMenuItemViewModel>(pmic);
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> QuickActionPresetMenuItems {
            get {
                var pmic = new List<MpContextMenuItemViewModel>();

                foreach (var item in Items) {
                    foreach (var qapmi in item.QuickActionPresetMenuItems) {
                        pmic.Add(qapmi);
                    }
                }
                if (pmic.Count == 0) {
                    return null;
                }
                return new ObservableCollection<MpContextMenuItemViewModel>(pmic);
            }
        }

        #endregion

        #region Layout

        #endregion

        #region Appearance


        #endregion

        #region State

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

        //public MpAnalyticItemCollectionViewModel() : base(null) { }

        //public MpAnalyticItemCollectionViewModel(MpContentItemViewModel parent) : base(parent) {
        //    PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
        //}

        #endregion

        #region Public Methods
        public async Task Init() {
            PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
            await InitDefaultItems();

            if (Items.Count > 0) {
                Items[0].IsSelected = true;
            }
        }

        public ContextMenu UpdateQuickActionMenuItem(ContextMenu cm) {
            Separator quickSep = null;
            foreach (var mi in cm.Items) {
                if (mi == null) {
                    continue;
                }
                if (mi is Separator s) {
                    if (s.Name == "QuickActionSeparator") {
                        quickSep = s;
                        break;
                    }
                }
            }
            if (quickSep == null) {
                return cm;
            }
            int quickSepIdx = cm.Items.IndexOf(quickSep);
            int itemsToRemove = cm.Items.Count - quickSepIdx - 1;
            while (itemsToRemove > 0) {
                cm.Items.RemoveAt(quickSepIdx + 1);
                itemsToRemove--;
            }
            var qapmic = QuickActionPresetMenuItems;
            if (qapmic == null || qapmic.Count == 0) {
                quickSep.Visibility = System.Windows.Visibility.Hidden;
            } else {
                quickSep.Visibility = System.Windows.Visibility.Visible;
                foreach (var qami in qapmic) {
                    //qami.ItemContainerStyle = cm.Resources["DefaultItemStyle"] as Style;
                    cm.Items.Add(new MenuItem() { DataContext = qami, ItemContainerStyle = cm.Resources["DefaultItemStyle"] as Style});
                }
            }
            return cm;
        }
        #endregion

        #region Private Methods

        private async Task InitDefaultItems() {
            IsBusy = true;

            Items.Clear();

            var translateVm = new MpTranslatorViewModel(this);
            await translateVm.Initialize();
            Items.Add(translateVm);

            var openAiVm = new MpOpenAiViewModel(this);
            await openAiVm.Initialize();
            Items.Add(openAiVm);

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
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
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ManageItemCommand => new RelayCommand<int>(
            (itemId) => {
                Items.ForEach(x => x.IsSelected = x.AnalyticItemId == itemId);
                SelectedItem.ManageAnalyticItemCommand.Execute(null);
            }, (itemId) => itemId != null);

        public ICommand RegisterContentCommand => new RelayCommand<object>(
            (args) => {
                Content = args;
            },
            (args) => args != null);
        #endregion
    }
}
