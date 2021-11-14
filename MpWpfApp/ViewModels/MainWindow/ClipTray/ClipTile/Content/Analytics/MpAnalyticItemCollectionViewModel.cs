using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpSingletonViewModel<MpAnalyticItemCollectionViewModel> { //
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; private set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem => Items.FirstOrDefault(x => x.IsSelected);

        public ObservableCollection<MpContextMenuItemViewModel> PresetMenuItems {
            get {
                var pmic = new List<MpContextMenuItemViewModel>();

                foreach(var item in Items) {
                    var imivm = new MpContextMenuItemViewModel(
                        header: item.Title,
                        command: null,
                        commandParameter: null,
                        isChecked: null,                        
                        iconSource: item.ItemIconBase64,
                        subItems: item.PresetMenuItems,
                        inputGestureText: string.Empty,
                        bgBrush: null);
                    pmic.Add(imivm);
                }
                if(pmic.Count == 0) {
                    return null;
                }
                return new ObservableCollection<MpContextMenuItemViewModel>(pmic);
            }
        }

        public ObservableCollection<MpContextMenuItemViewModel> QuickActionPresetMenuItems {
            get {
                var pmic = new List<MpContextMenuItemViewModel>();

                foreach (var item in Items) {
                    foreach(var qapmi in item.QuickActionPresetMenuItems) {
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

        public double AnalyticTreeViewMaxWidth { get; set; } = MpMeasurements.Instance.ClipTileInnerBorderSize;

        #endregion

        #region State

        public bool IsLoaded => Items.Count > 0;

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
                if(mi is Separator s) {
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
            while(itemsToRemove > 0) {
                cm.Items.RemoveAt(quickSepIdx + 1);
                itemsToRemove--;
            }
            var qapmic = QuickActionPresetMenuItems;
            if(qapmic == null || qapmic.Count == 0) {
                quickSep.Visibility = System.Windows.Visibility.Hidden;
            } else {
                quickSep.Visibility = System.Windows.Visibility.Visible;
                foreach (var qami in qapmic) {
                    //qami.ItemContainerStyle = cm.Resources["DefaultItemStyle"] as Style;
                    cm.Items.Add(qami);
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
                //case nameof(HostClipTileViewModel):
                //    HostClipTileViewModel.DoCommandSelection();
                //    break;
                
            }
        }
        #endregion

        #region Commands

        public ICommand RegisterContentCommand => new RelayCommand<object>(
            (args) => {
                Content = args;
            },
            (args) => args != null);
        #endregion
    }
}
