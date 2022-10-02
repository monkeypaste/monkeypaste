using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpActionCollectionViewModel : 
        MpSelectorViewModelBase<object,MpTriggerActionViewModelBase>, 
        MpIMenuItemViewModel,
        MpIAsyncSingletonViewModel<MpActionCollectionViewModel>,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIDesignerSettingsViewModel {
        #region Private Variables

        #endregion

        #region Properties
        public static CancellationTokenSource CTS = new CancellationTokenSource();

        #region View Models

        public MpMenuItemViewModel ContextMenuItemViewModel {
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

        public override MpTriggerActionViewModelBase SelectedItem {
            get {
                if (PrimaryAction == null) {
                    return null;
                }
                if (PrimaryAction is MpTriggerActionViewModelBase) {
                    return PrimaryAction as MpTriggerActionViewModelBase;
                }
                return PrimaryAction.FindRootParent() as MpTriggerActionViewModelBase;
            }
            set {
                if (SelectedItem != value) {
                    AllSelectedTriggerActions.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(AllSelectedTriggerActions));
                    OnPropertyChanged(nameof(SelectedActions));
                    OnPropertyChanged(nameof(PrimaryAction));
                    OnPropertyChanged(nameof(IsAnySelected));
                }
            }
        }

        public IList<MpActionViewModelBase> AllActions {
            get {
                if (Items == null) {
                    return new List<MpActionViewModelBase>();
                }
                var avml = new List<MpActionViewModelBase>();
                foreach (var tavm in Items.ToList()) {
                    avml.Add(tavm);
                    avml.AddRange(tavm.FindAllChildren());
                }
                return avml;
            }
        }

        public IList<MpActionViewModelBase> AllSelectedTriggerActions {
            get {
                if (SelectedItem == null) {
                    return new List<MpActionViewModelBase>();
                }
                var avml = SelectedItem.FindAllChildren().ToList();
                avml.Insert(0, SelectedItem);
                return avml;
            }
        }

        public IList<MpActionViewModelBase> SelectedActions =>
            AllActions.Where(x => x.IsSelected).OrderByDescending(x => x.LastSelectedDateTime).ToList();

        public MpActionViewModelBase PrimaryAction => SelectedActions.Count == 0 ? null : SelectedActions[0];

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

        #region MpIDesignerSettingsViewModel Implementation

        public double ScaleX { get; set; } = 1;
        public double ScaleY { get; set; } = 1;

        public double DesignerWidth { get; set; } = MpMeasurements.Instance.DefaultDesignerWidth;
        public double DesignerHeight { get; set; } = MpClipTrayViewModel.Instance.ClipTrayHeight;

        public double ViewportWidth {
            get => DesignerWidth / ScaleX;
            set => DesignerWidth = value * ScaleX;
        }
        public double ViewportHeight {
            get => DesignerHeight / ScaleY;
            set => DesignerHeight = value * ScaleY;
        }

        public Point DefaultTriggerLocation => new Point(
            (ViewportWidth / 2) - (MpMeasurements.Instance.DesignerItemDiameter / 2),
            (ViewportHeight / 2) - (MpMeasurements.Instance.DesignerItemDiameter / 2));

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
        #endregion

        #endregion

        #region Constructors

        private static MpActionCollectionViewModel _instance;
        public static MpActionCollectionViewModel Instance => _instance ?? (_instance = new MpActionCollectionViewModel());


        public MpActionCollectionViewModel() : base(null) {
            PropertyChanged += MpMatcherCollectionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            MpConsole.WriteLine("Action Collectoin Init!");

            Items.Clear();
            var tal = await MpDataModelProvider.GetAllTriggerActionsAsync();

            foreach (var ta in tal) {
                var tavm = await CreateTriggerViewModelAsync(ta);
                Items.Add(tavm);
            }

            while (AllActions.Any(x => x.IsBusy)) {
                // wait for all action trees to initialize before enabling
                await Task.Delay(100);
            }

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.Children)));
            OnPropertyChanged(nameof(Items));

            await RestoreAllEnabledAsync();//.FireAndForgetSafeAsync(this);

            // select most recent action
            MpActionViewModelBase actionToSelect = AllActions
                            .Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

            if (actionToSelect != null) {
                SelectedItem = actionToSelect.RootTriggerActionViewModel;
                actionToSelect.IsSelected = true;
                OnPropertyChanged(nameof(SelectedItem));
                SelectedItem.OnPropertyChanged(nameof(SelectedActions));
            }

            IsBusy = false;
        }
        public async Task<MpTriggerActionViewModelBase> CreateTriggerViewModelAsync(MpAction a) {
            
            if(a.ActionType != MpActionType.Trigger || 
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
            }

            await tavm.InitializeAsync(a);

            return tavm;
        }

        public async Task RestoreAllEnabledAsync() {
            // NOTE this is only called on init and needs to wait for dependant vm's to load so wait here

            //while(MpTagTrayViewModel.Instance.IsBusy)

            foreach(var avm in AllActions) {
                // TODO this could be optimized by toggling enabled in parallel
                if(avm.IsEnabledDb) {
                    avm.ToggleIsEnabledCommand.Execute(true);
                    while(avm.IsBusy) {
                        await Task.Delay(100);
                    }
                }
            }

            //while(AllActions.Any(x=>x.IsBusy)) {
            //    await Task.Delay(100);
            //}
        }

        public async Task DisableAllAsync() {
            await Task.Delay(1);
            Items.ForEach(x => x.ToggleIsEnabledCommand.Execute(false));
        }

        #region DesignerItem Placement Methods

        public Point FindOpenDesignerLocation(Point anchorPoint, object ignoreItem = null) {
            int attempts = 0;
            int maxAttempts = 10;
            int count = 4;
            double dtheta = (2 * Math.PI) / count;
            double r = MpMeasurements.Instance.DesignerItemDiameter * 2;
            while (attempts <= maxAttempts) {
                double theta = 0;
                for (int i = 0; i < count; i++) {
                    var tp = new Point();
                    tp.X = (double)(anchorPoint.X + r * Math.Cos(theta));
                    tp.Y = (double)(anchorPoint.Y + r * Math.Sin(theta));
                    if (!OverlapsItem(tp)) {
                        return tp;
                    }
                    theta += dtheta;
                }
                r += MpMeasurements.Instance.DesignerItemDiameter * 2;

                attempts++;
            }

            return new Point(MpHelpers.Rand.NextDouble() * DesignerWidth, MpHelpers.Rand.NextDouble() * DesignerHeight);
        }

        public bool OverlapsItem(Point targetTopLeft) {
            return GetItemNearPoint(targetTopLeft) != null;
        }

        public MpActionViewModelBase GetItemNearPoint(Point targetTopLeft, object ignoreItem = null, double radius = 50) {
            Point targetMid = new Point(targetTopLeft.X, targetTopLeft.Y);
            foreach (var avm in AllSelectedTriggerActions) {
                Point sourceMid = new Point(avm.X, avm.Y);
                double dist = targetMid.Distance(sourceMid);
                if (dist < radius && avm != ignoreItem) {
                    return avm;
                }
            }
            return null;
        }

        //public void ClearAreaAtPoint(Point p, object ignoreItem = null) {
        //    var overlapItem = GetItemNearPoint(p, ignoreItem);
        //    if (overlapItem != null) {
        //        Point tempLoc = p;
        //        do {
        //            var overlapLoc = new Point(overlapItem.X, overlapItem.Y);
        //            double distToMove = overlapLoc.Distance(tempLoc) + 10;

        //            var dir = overlapLoc - tempLoc;
        //            dir.Normalize();
        //            dir = new Vector(-dir.Y, dir.X);
        //            overlapLoc += dir * distToMove;
        //            overlapItem.X = overlapLoc.X;
        //            overlapItem.Y = overlapLoc.Y;

        //            overlapItem = GetItemNearPoint(overlapLoc, overlapItem);
        //            tempLoc = overlapLoc;
        //        } while (overlapItem != null && overlapItem != ignoreItem);
        //    }
        //}

        //public void ClearAllOverlaps() {
        //    foreach (var avm in AllSelectedTriggerActions) {
        //        ClearAreaAtPoint(avm.Location, avm);
        //    }
        //}
        public void NotifyViewportChanged() {
            CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
            CollectionViewSource.GetDefaultView(AllSelectedTriggerActions).Refresh();
        }
        #endregion

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

        #region Protected Methods

        

        #endregion

        #region Private Methods

        private async Task UpdateSortOrderAsync() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private void MpMatcherCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSidebarVisible):
                    if (IsSidebarVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpTagTrayViewModel.Instance.IsSidebarVisible = false;
                        MpClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;

                    }
                    break;
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(IsAnySelected));
                    //OnPropertyChanged(nameof(AllSelectedTriggerActions));
                    break;
                //case nameof(DesignerWidth):
                //case nameof(DesignerHeight):
                //case nameof(CameraZoomFactor):
                //    OnPropertyChanged(nameof(ViewportWidth));
                //    OnPropertyChanged(nameof(ViewportHeight));
                //    break;
                //case nameof(ViewportWidth):
                //case nameof(ViewportHeight):
                //    OnPropertyChanged(nameof(DesignerWidth));
                //    OnPropertyChanged(nameof(DesignerHeight));
                //    break;
                case nameof(ScaleX):
                case nameof(ScaleY):
                    OnPropertyChanged(nameof(DesignerWidth));
                    OnPropertyChanged(nameof(DesignerHeight));
                    OnPropertyChanged(nameof(ViewportWidth));
                    OnPropertyChanged(nameof(ViewportHeight));
                    break;
                case nameof(PrimaryAction):
                    OnPropertyChanged(nameof(IsAnySelected));
                    break;
                case nameof(HasModelChanged):
                    if(SelectedItem == null) {
                        return;
                    }
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await SelectedItem.Action.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        }).FireAndForgetSafeAsync();
                    }
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ResetDesignerCommand => new RelayCommand(
            () => {

            });

        public ICommand ShowTriggerSelectorMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = MpContextMenuView.Instance;
                 cm.DataContext = ContextMenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
             });

        public ICommand AddTriggerCommand => new MpAsyncCommand<object>(
             async (args) => {
                 IsBusy = true;
                                  
                 MpTriggerType tt = args == null ? MpTriggerType.None : (MpTriggerType)args;
                 
                 MpAction na = await MpAction.CreateAsync(
                         label: GetUniqueTriggerName(tt.ToString()),
                         actionType: MpActionType.Trigger,
                         actionObjId: (int)tt,
                         sortOrderIdx: Items.Count,
                         location: DefaultTriggerLocation.ToPortablePoint());

                 var navm = await CreateTriggerViewModelAsync(na);

                 Items.Add(navm);
                 SelectedItem = navm;

                 OnPropertyChanged(nameof(AllSelectedTriggerActions));

                 OnPropertyChanged(nameof(Items));

                 OnPropertyChanged(nameof(IsAnySelected));

                 while(IsBusy) { await Task.Delay(100); }

                 if(SelectedItem != navm) {
                     SelectedItem = navm;
                 }
                 SelectedItem.ToggleIsEnabledCommand.Execute(null);

                 IsBusy = false;
             });

        public ICommand DeleteTriggerCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                var tavm = args as MpTriggerActionViewModelBase;
                tavm.ToggleIsEnabledCommand.Execute(false);

                var deleteTasks = tavm.FindAllChildren().Select(x => x.Action.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(tavm.Action.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(tavm);

                OnPropertyChanged(nameof(AllSelectedTriggerActions));

                await UpdateSortOrderAsync();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));

                IsBusy = false;
            });

        public ICommand SelectActionCommand => new RelayCommand<object>(
            (args) => {
                if(args == null) {
                    return;
                }
                if(args is int actionId) {
                    var actionToSelect = AllActions.FirstOrDefault(x => x.ActionId == actionId);
                    if(actionToSelect == null) {
                        return;
                    }
                    if (!IsSidebarVisible) {
                        IsSidebarVisible = true;
                    }
                    SelectedItem = actionToSelect.RootTriggerActionViewModel;
                    SelectedItem.AllChildren.ForEach(x => x.IsSelected = x.ActionId == actionId);
                }
            });


        #endregion
    }
}
