using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpActionCollectionViewModel : 
        MpSelectorViewModelBase<object,MpTriggerActionViewModelBase>, 
        MpIMenuItemViewModel,
        MpISingletonViewModel<MpActionCollectionViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIViewportCameraViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var tmivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpTriggerType).EnumToLabels("Select Trigger");
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch((MpTriggerType)i) {
                        case MpTriggerType.ContentAdded:
                            resourceKey = "ClipboardIcon";
                            break;
                        case MpTriggerType.ContentTagged:
                            resourceKey = "PinToCollectionIcon";
                            break;
                        case MpTriggerType.FileSystemChange:
                            resourceKey = "FolderEventIcon";
                            break;
                        case MpTriggerType.Shortcut:
                            resourceKey = "HotkeyIcon";
                            break;
                        case MpTriggerType.ParentOutput:
                            resourceKey = "ChainIcon";
                            break;
                    }
                    var tt = (MpTriggerType)i;
                    tmivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = AddTriggerCommand,
                        CommandParameter = tt,
                        IsVisible = tt != MpTriggerType.None && (tt != MpTriggerType.ParentOutput || (SelectedItem != null && !SelectedItem.IsRootAction))
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = tmivml
                };
            }
        }

        public IList<MpActionViewModelBase> AllSelectedActions {
            get {
                if(SelectedItem == null) {
                    return new List<MpActionViewModelBase>();
                }
                var avml = SelectedItem.FindAllChildren().ToList();
                avml.Insert(0, SelectedItem);
                return avml;
            }
        }

        private ObservableCollection<MpActionViewModelBase> _allActions;
        public ObservableCollection<MpActionViewModelBase> AllActions {
            get {
                if(_allActions == null) {
                    _allActions = new ObservableCollection<MpActionViewModelBase>();
                }
                _allActions.Clear();
                foreach(var tavm in Items) {
                    foreach(var cavm in tavm.FindAllChildren()) {
                        _allActions.Add(cavm);
                    }
                }
                return _allActions;
            }
        }

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultActionPanelWidth;
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultActionPanelWidth;
        public bool IsSidebarVisible { get; set; } = false;

        public MpISidebarItemViewModel NextSidebarItem => SelectedItem == null ? null : SelectedItem;
        public MpISidebarItemViewModel PreviousSidebarItem => null;

        #endregion

        #region MpIViewportCameraViewModel Implementation

        public bool IsPanning { get; set; } = false;
        public bool IsZooming { get; set; } = false;
        public bool CanZoom => SelectedItem != null;
        public bool CanPan => SelectedItem != null && AllSelectedActions.All(x => !x.IsMoving && !x.CanMove);

        public double MaxCameraZoomFactor => 10.0;
        public double MinCameraZoomFactor => 0.1;
        public double CameraZoomFactor { get; set; } = 1.0;

        public double CameraX { get; set; } = 0;
        public double CameraY { get; set; } = 0;

        public double DesignerWidth { get; set; } = MpMeasurements.Instance.DefaultDesignerWidth;
        public double DesignerHeight { get; set; }

        public double ViewportWidth {
            get {
                return DesignerWidth * CameraZoomFactor;
            }
            set {
                if(ViewportWidth != value) {
                    DesignerWidth = value / CameraZoomFactor;
                    OnPropertyChanged(nameof(ViewportWidth));
                    MpConsole.WriteLine("Viewport width: " + ViewportWidth);
                }
            }
        }
        public double ViewportHeight {
            get {
                return DesignerHeight * CameraZoomFactor;
            }
            set {
                if (ViewportHeight != value) {
                    DesignerHeight = value / CameraZoomFactor;
                    OnPropertyChanged(nameof(ViewportHeight));
                    MpConsole.WriteLine("Viewport heighy: " + ViewportHeight);
                }
            }
        }

        #endregion

        #region Appearance

        public string AddNewButtonBorderBrushHexColor {
            get {
                if (IsHovering) {
                    return MpSystemColors.LightGray;
                }
                return MpSystemColors.DarkGray;
            }
        }

        #endregion

        #region State

        public bool IsAnyTextBoxFocused => SelectedItem != null && SelectedItem.IsAnyTextBoxFocused;

        #endregion

        #endregion

        #region Constructors

        private static MpActionCollectionViewModel _instance;
        public static MpActionCollectionViewModel Instance => _instance ?? (_instance = new MpActionCollectionViewModel());


        public MpActionCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }

        public async Task Init() {
            IsBusy = true;

            var tal = await MpDataModelProvider.GetAllTriggerActions();

            foreach (var ta in tal) {
                var tavm = await CreateTriggerViewModel(ta);
                Items.Add(tavm);
            }

            EnabledAll();

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpTriggerActionViewModelBase> CreateTriggerViewModel(MpAction a) {
            
            if(a.ActionType != MpActionType.Trigger || 
               //(MpTriggerType) a.ActionObjId == MpTriggerType.None || 
               (MpTriggerType)a.ActionObjId == MpTriggerType.ParentOutput) {
                throw new Exception("This is only supposed to load root level triggers");
            }
            MpTriggerActionViewModelBase tavm = null;
            switch ((MpTriggerType)a.ActionObjId) {
                case MpTriggerType.None:
                    tavm = new MpTriggerActionViewModelBase(this);
                    break;
                case MpTriggerType.ContentAdded:
                    tavm = new MpContentAddTriggerViewModel(this);
                    break;
                case MpTriggerType.ContentTagged:
                    tavm = new MpContentTaggedTriggerViewModel(this);
                    break;
                case MpTriggerType.FileSystemChange:
                    tavm = new MpFileSystemTriggerViewModel(this);
                    break;
                case MpTriggerType.Shortcut:
                    tavm = new MpShortcutTriggerViewModel(this);
                    break;
                //case MpTriggerType.ParentOutput:
                //    tavm = new MpParentOutputTriggerViewModel(this);
                //    break;
            }

            await tavm.InitializeAsync(a);            

            return tavm;
        }

        public void EnabledAll() {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.Children)));
            Items.ForEach(x => x.Enable());
        }

        public void DisableAll() {
            Items.ForEach(x => x.Disable());
        }

        public MpPoint FindOpenDesignerLocation() {
            MpPoint p = new MpPoint();
            if(SelectedItem == null) {
                return p;
            }
            int attempts = 0;
            int maxAttempts = 100;
            int hw = (int)(DesignerWidth / 2);
            int hh = (int)(DesignerHeight / 2);
            while(OverlapsItem(p)) {
                p.X = MpHelpers.Rand.Next(-hw, hw);
                p.Y = MpHelpers.Rand.Next(-hh, hh);
                attempts++;
                if(attempts > maxAttempts) {
                    DesignerWidth += MpMeasurements.Instance.DefaultDesignerItemWidth * 2;
                    DesignerHeight += MpMeasurements.Instance.DefaultDesignerItemHeight * 2;
                    hw = (int)(DesignerWidth / 2);
                    hh = (int)(DesignerHeight / 2);
                    attempts = 0;
                }
            }
            return p;
        }

        public bool OverlapsItem(MpPoint targetTopLeft) {
            double w = MpMeasurements.Instance.DefaultDesignerItemWidth;
            double h = MpMeasurements.Instance.DefaultDesignerItemHeight;
            MpPoint targetMid = new MpPoint(targetTopLeft.X + (w / 2), targetTopLeft.Y + (h / 2));
            foreach(var avm in AllSelectedActions) {
                MpPoint sourceMid = new MpPoint(avm.X + (w / 2), avm.Y + (h / 2));
                double dist = targetMid.Distance(sourceMid);
                if(dist < w || dist < h) {
                    return true;
                }
            }
            return false;
        }

        public string GetUniqueTriggerName(string prefix) {
            int uniqueIdx = 1;
            string testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);

            while (Items.Any(x => x.Label.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        prefix.ToLower(),
                                        uniqueIdx);
            }
            return prefix + uniqueIdx;
        }

        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSidebarVisible):
                    MpSidebarViewModel.Instance.OnPropertyChanged(nameof(MpSidebarViewModel.Instance.IsAnySidebarOpen));
                    

                    if (IsSidebarVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;

                    }
                    break;
                case nameof(SelectedItem):
                    CameraZoomFactor = 1.0;
                    OnPropertyChanged(nameof(IsAnySelected));
                    OnPropertyChanged(nameof(AllSelectedActions));
                    break;
                case nameof(DesignerWidth):
                case nameof(DesignerHeight):
                case nameof(CameraZoomFactor):
                    OnPropertyChanged(nameof(ViewportWidth));
                    OnPropertyChanged(nameof(ViewportHeight));
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ShowActionSelectorMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = new MpContextMenuView();
                 cm.DataContext = MenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
             });

        public ICommand AddTriggerCommand => new RelayCommand<object>(
             async (args) => {
                 IsBusy = true;
                                  
                 MpTriggerType tt = args == null ? MpTriggerType.None : (MpTriggerType)args;
                 
                 MpAction na = await MpAction.Create(
                         label: GetUniqueTriggerName("Trigger"),
                         actionType: MpActionType.Trigger,
                         actionObjId: (int)tt,
                         sortOrderIdx: Items.Count);

                 var navm = await CreateTriggerViewModel(na);

                 Items.Add(navm);

                 SelectedItem = navm;

                 OnPropertyChanged(nameof(AllSelectedActions));

                 OnPropertyChanged(nameof(Items));

                 IsBusy = false;
             });

        public ICommand DeleteTriggerCommand => new RelayCommand<object>(
            async (args) => {
                var tavm = args as MpTriggerActionViewModelBase;
                tavm.Disable();
                var deleteTasks = tavm.FindAllChildren().Select(x => x.Action.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(tavm.Action.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(tavm);

                OnPropertyChanged(nameof(AllSelectedActions));

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));
            });

        #endregion
    }
}
