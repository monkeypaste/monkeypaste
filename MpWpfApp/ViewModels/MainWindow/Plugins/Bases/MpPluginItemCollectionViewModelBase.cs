using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public abstract class MpPluginItemCollectionViewModelBase<P,C,F,T> :
        MpSelectorViewModelBase<MpPluginCollectionViewModel, MpPluginItemViewModelBase<F, T>>,
        MpIMenuItemViewModel,
        MpITreeItemViewModel,
        MpISidebarItemViewModel
        where P: class
        where C: MpViewModelBase
        where T : MpIPluginComponentBase
        where F : MpPluginComponentBaseFormat {



        #region Properties

        #region View Models

        public abstract MpMenuItemViewModel MenuItemViewModel { get; }

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpITreeItemViewModel ParentTreeItem => MpPluginCollectionViewModel.Instance;

        public List<MpAnalyticItemPresetViewModel> AllPresets {
            get {
                return Items.OrderBy(x => x.Title).SelectMany(x => x.Items).ToList();
            }
        }

        public MpAnalyticItemPresetViewModel SelectedPresetViewModel {
            get {
                if (SelectedItem == null) {
                    return null;
                }
                return SelectedItem.SelectedItem;
            }
        }

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

        //public bool IsAnyEditingParameters => Items.Any(x => x.IsAnyEditingParameters);

        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpPluginItemCollectionViewModelBase(MpPluginCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpPluginItemCollectionViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task Init() {
            IsBusy = true;

            Items.Clear();

            var pail = MpPluginManager.Plugins.Where(x => x.Value.Component is T);
            foreach (var pai in pail) {
                var paivm = await CreatePluginItemViewModel(pai.Value);
                Items.Add(paivm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                // select most recent preset
                MpAnalyticItemPresetViewModel presetToSelect = Items
                            .Aggregate((a, b) => a.Items.Max(x => x.LastSelectedDateTime) > b.Items.Max(x => x.LastSelectedDateTime) ? a : b)
                            .Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if (presetToSelect != null) {
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

        #endregion

        #region Protected Methods
        protected abstract Task<MpPluginItemViewModelBase<T>> CreatePluginItemViewModel(MpPluginFormat plugin);

        #endregion

        #region Private Methods


        private void MpPluginItemCollectionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                case nameof(IsSidebarVisible):
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayHeight));
                    if (IsSidebarVisible) {
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(Children));
                    break;
                case nameof(SelectedPresetViewModel):
                    if (SelectedPresetViewModel == null) {
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
        #endregion
    }

}
