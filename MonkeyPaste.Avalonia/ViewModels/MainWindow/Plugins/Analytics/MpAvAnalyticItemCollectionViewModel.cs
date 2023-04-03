using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvAnalyticItemCollectionViewModel :
        MpAvTreeSelectorViewModelBase<object, MpAvAnalyticItemViewModel>,
        MpIMenuItemViewModel,
        MpIAsyncComboBoxViewModel,
        MpISidebarItemViewModel,
        MpIPopupMenuPicker {
        #region Private Variables

        private string _processAutomationGuid = "e7e25c85-1c8f-4e79-be8f-2ebfcb5bb94e";
        private string _httpAutomationGuid = "084abd2e-801d-4637-9054-b42f1b159c32";

        #endregion

        #region Interfaces

        #region MpIPopupMenuPicker Implementation

        public MpMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedAnalyticItemPresetIds, bool recursive) {
            return new MpMenuItemViewModel() {
                SubItems = Items.Select(x =>
                new MpMenuItemViewModel() {
                    Header = x.Title,
                    IconId = x.PluginIconId,
                    SubItems = x.Items.Select(y => y.GetMenu(cmd, cmdArg, selectedAnalyticItemPresetIds, recursive)).ToList()
                }).ToList()
            };
        }

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
                double w = DefaultSelectorColumnVarDimLength;
                if (SelectedPresetViewModel != null) {
                    w += DefaultParameterColumnVarDimLength;
                }
                return w;
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ObservedQueryTrayScreenHeight;
                }
                double h = DefaultSelectorColumnVarDimLength;
                //if (SelectedPresetViewModel != null) {
                //    h += _defaultParameterColumnVarDimLength;
                //}
                return h;
            }
        }
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; } = 0;

        public string SidebarBgHexColor =>
            (Mp.Services.PlatformResource.GetResource("AnalyzerSidebarBgBrush") as IBrush).ToHex();

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region MpIAsyncComboBoxViewModel Implementation

        IEnumerable<MpIAsyncComboBoxItemViewModel> MpIAsyncComboBoxViewModel.Items => Items;
        MpIAsyncComboBoxItemViewModel MpIAsyncComboBoxViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = (MpAvAnalyticItemViewModel)value;
        }
        bool MpIAsyncComboBoxViewModel.IsDropDownOpen {
            get => IsAnalyticItemSelectorDropDownOpen;
            set => IsAnalyticItemSelectorDropDownOpen = value;
        }

        #endregion

        #endregion

        #region Properties

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpITreeItemViewModel ParentTreeItem => null;

        #endregion


        #region View Models

        public MpAvAnalyticItemViewModel ProcessAutomationViewModel => Items.FirstOrDefault(x => x.PluginGuid == _processAutomationGuid);

        public MpAvAnalyticItemViewModel HttpAutomationViewModel => Items.FirstOrDefault(x => x.PluginGuid == _httpAutomationGuid);

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                MpCopyItemType contentType = MpAvClipTrayViewModel.Instance.SelectedItem == null ?
                    MpCopyItemType.None : MpAvClipTrayViewModel.Instance.SelectedItem.CopyItemType;
                return GetContentContextMenuItem(contentType);
            }
        }

        public IEnumerable<MpAvAnalyticItemPresetViewModel> AllPresets => Items.OrderBy(x => x.Title).SelectMany(x => x.Items);

        public MpAvAnalyticItemPresetViewModel SelectedPresetViewModel {
            get {
                if (SelectedItem == null) {
                    return null;
                }
                return SelectedItem.SelectedItem;
            }
        }

        #endregion


        #region Layout


        public double DefaultSelectorColumnVarDimLength =>
            400;

        public double DefaultParameterColumnVarDimLength =>
            450;
        #endregion

        #region Appearance


        #endregion

        #region State

        public int SelectedItemIdx {
            get => Items.IndexOf(SelectedItem);
            set {
                if (SelectedItemIdx != value) {
                    SelectedItem = value < 0 || value >= Items.Count ? null : Items[value];
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }
        public bool IsHovering { get; set; }

        public bool IsLoaded => Items.Count > 0;

        public bool IsAnalyticItemSelectorDropDownOpen { get; set; }
        //public bool IsAnyEditingParameters => Items.Any(x => x.IsAnyEditingParameters);

        #endregion

        #region Model

        public object Content { get; private set; }

        #endregion

        #endregion

        #region Constructors

        private static MpAvAnalyticItemCollectionViewModel _instance;
        public static MpAvAnalyticItemCollectionViewModel Instance => _instance ?? (_instance = new MpAvAnalyticItemCollectionViewModel());


        public MpAvAnalyticItemCollectionViewModel() : base(null) {
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

            var pail = MpPluginLoader.Plugins.Where(x => x.Value.Component is MpIAnalyzeAsyncComponent || x.Value.Component is MpIAnalyzeComponent);
            foreach (var pai in pail) {
                var paivm = await CreateAnalyticItemViewModel(pai.Value);
                Items.Add(paivm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            if (Items.Count > 0) {
                // select most recent preset
                MpAvAnalyticItemPresetViewModel presetToSelect = Items
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

        public MpMenuItemViewModel GetContentContextMenuItem(MpCopyItemType contentType) {
            var availItems = Items.Where(x => x.IsContentTypeValid(contentType));
            List<MpMenuItemViewModel> sub_items = availItems.SelectMany(x => x.QuickActionPresetMenuItems).ToList();
            if (sub_items.Count > 0) {
                sub_items.Add(new MpMenuItemViewModel() { IsSeparator = true });
            }
            if (availItems.Count() > 0) {

                sub_items.AddRange(availItems.Select(x => x.ContextMenuItemViewModel));
            }

            return new MpMenuItemViewModel() {
                Header = @"Analyze",
                AltNavIdx = 0,
                IconResourceKey = Mp.Services.PlatformResource.GetResource("BrainImage") as string,
                SubItems = sub_items
            };
        }
        #endregion

        #region Private Methods

        private async Task<MpAvAnalyticItemViewModel> CreateAnalyticItemViewModel(MpPluginFormat plugin) {
            MpAvAnalyticItemViewModel aivm = new MpAvAnalyticItemViewModel(this);

            await aivm.InitializeAsync(plugin);
            return aivm;
        }


        private void MpAnalyticItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                case nameof(IsAnySelected):
                    break;
                //case nameof(IsSidebarVisible):                    
                //    if (IsSidebarVisible) {
                //        MpAvTagTrayViewModel.Instance.IsSidebarVisible = false;
                //        MpAvTriggerCollectionViewModel.Instance.IsSidebarVisible = false;
                //        MpAvClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                //    }
                //    OnPropertyChanged(nameof(SelectedItem));
                //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel));
                //    break;
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
                case nameof(IsAnalyticItemSelectorDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsAnalyticItemSelectorDropDownOpen;
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ApplyCoreAnnotatorCommand => new MpCommand<object>(
            (args) => {
                var ctvm = args as MpAvClipTileViewModel;
                if (ctvm == null) {
                    return;
                }

                var core_aipvm = AllPresets
                    .FirstOrDefault(x => x.PresetGuid == MpPrefViewModel.Instance.CoreAnnotatorDefaultPresetGuid);

                if (core_aipvm == null) {
                    return;
                }
                core_aipvm.Parent.ExecuteAnalysisCommand.Execute(new object[] { core_aipvm, ctvm.CopyItem });
            });
        #endregion
    }
}
