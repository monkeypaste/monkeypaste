using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTriggerCollectionViewModel :
        MpAvViewModelBase,
        MpIZoomFactorViewModel,
        MpIPopupMenuViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIAsyncCollectionObject,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpISelectableViewModel,
        MpISidebarItemViewModel,
        MpIDesignerSettingsViewModel,
        MpAvIFocusHeaderMenuViewModel 
        {
        #region Private Variables

        #endregion

        #region Constants
        public const string DEFAULT_ANNOTATOR_TRIGGER_GUID = "561c8b4d-9b78-4e79-ac01-dc831c55e88c";
        public const string DEFAULT_ANNOTATOR_ANALYZE_GUID = "6cfa5188-5c9a-48aa-aa01-6c4cad8af3e4";

        public const double DEFAULT_MIN_SCALE = 0.1;
        public const double DEFAULT_MAX_SCALE = 3.0d;
        const double DEFAULT_SCALE = 1.0d;
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpAvIFocusHeaderMenuViewModel Implementation
        IBrush MpAvIHeaderMenuViewModel.HeaderBackground =>
            Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeCompliment2Color);
        IBrush MpAvIHeaderMenuViewModel.HeaderForeground =>
            (this as MpAvIHeaderMenuViewModel).HeaderBackground.ToHex().ToContrastForegoundColor().ToAvBrush();
        string MpAvIHeaderMenuViewModel.HeaderTitle =>
            UiStrings.ActionDesignerWindowTitle.Replace("'{0}'",string.Empty);
        IEnumerable<MpAvIMenuItemViewModel> MpAvIHeaderMenuViewModel.HeaderMenuItems =>
            [
            new MpAvMenuItemViewModel() {
                    IconSourceObj = "PlusImage",
                    Command = ShowTriggerSelectorMenuCommand
                },
            ];
        ICommand MpAvIHeaderMenuViewModel.BackCommand =>
            null;
        object MpAvIHeaderMenuViewModel.BackCommandParameter =>
            null;

        #endregion

        #region MpIZoomFactorViewModel Implementation

        public double MinZoomFactor =>
            DEFAULT_MIN_SCALE;
        public double MaxZoomFactor =>
            DEFAULT_MAX_SCALE;
        public double DefaultZoomFactor =>
            DEFAULT_SCALE;
        public double StepDelta =>
            MpAvZoomBorder.WHEEL_ZOOM_DELTA;

        #endregion

        #region MpISelectableViewModel Implementation
        private bool _isSelected;
        public bool IsSelected {
            get => _isSelected;
            set {
                if (IsSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
                if (IsSelected) {
                    // always update datetime
                    LastSelectedDateTime = DateTime.Now;
                }
            }
        }
        public DateTime LastSelectedDateTime { get; set; } = DateTime.MinValue;

        #endregion

        #region MpIWindowViewModel Implementation
        MpWindowType MpIWindowViewModel.WindowType =>
            MpWindowType.PopOut;


        #endregion

        #region MpICloseWindowViewModel Implementation
        public bool IsWindowOpen { get; set; }

        #endregion

        #region MpIActiveWindowViewModel Implementation        
        public bool IsWindowActive { get; set; }
        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation        
        bool MpIWantsTopmostWindowViewModel.WantsTopmost => true;
        #endregion

        #region MpIPopupMenuViewModel Implementation
        bool MpIPopupMenuViewModel.IsPopupMenuOpen { get; set; }
        public MpAvMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    SubItems =
                                typeof(MpTriggerType)
                                .EnumerateEnum<MpTriggerType>()
                                .Where(x => x != MpTriggerType.None)
                                .OrderBy(x => x.EnumToUiString())
                                .Select(x =>
                                    new MpAvMenuItemViewModel() {
                                        Header = x.EnumToUiString(),
                                        IconResourceKey = MpAvActionViewModelBase.GetDefaultActionIconResourceKey(x),
                                        IconTintHexStr = MpAvActionViewModelBase.GetActionHexColor(MpActionType.Trigger, x),
                                        Command = AddTriggerCommand,
                                        CommandParameter = x
                                    }).ToList()
                };
            }
        }

        #endregion

        #region MpIDesignerSettingsViewModel Implementation

        public bool IsGridVisible {
            get {
                return ParseShowGrid(DesignerSettingsArg);
            }
            set {
                if (IsGridVisible != value) {
                    SetDesignerItemSettings(ZoomFactor, TranslateOffsetX, TranslateOffsetY, value);
                    OnPropertyChanged(nameof(IsGridVisible));
                }
            }
        }
        public double ZoomFactor {
            get {
                return ParseScale(DesignerSettingsArg);
            }
            set {
                if (ZoomFactor != value) {
                    SetDesignerItemSettings(Math.Clamp(value, MinZoomFactor, MaxZoomFactor), TranslateOffsetX, TranslateOffsetY, IsGridVisible);
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }
        public double TranslateOffsetX {
            get {
                return ParseTranslationOffset(DesignerSettingsArg).X;
            }
            set {
                if (Math.Abs(TranslateOffsetX - value) > 0.1) {
                    SetDesignerItemSettings(ZoomFactor, Math.Round(value, 1), TranslateOffsetY, IsGridVisible);
                    OnPropertyChanged(nameof(TranslateOffsetX));
                }
            }
        }
        public double TranslateOffsetY {
            get {
                return ParseTranslationOffset(DesignerSettingsArg).Y;
            }
            set {
                if (Math.Abs(TranslateOffsetY - value) > 0.1) {
                    SetDesignerItemSettings(ZoomFactor, TranslateOffsetX, Math.Round(value, 1), IsGridVisible);
                    OnPropertyChanged(nameof(TranslateOffsetY));
                }
            }
        }
        public ICommand ResetDesignerViewCommand => new MpCommand(() => {

            MpAvZoomBorder zb = null;
            if (IsWindowOpen &&
                MpAvWindowManager.LocateWindow(this) is MpAvWindow dsw &&
                dsw.GetVisualDescendant<MpAvZoomBorder>() is MpAvZoomBorder dsw_zb) {
                zb = dsw_zb;
            } else if (App.MainView is Control mv &&
                        mv.GetVisualDescendant<MpAvZoomBorder>() is MpAvZoomBorder mw_zb) {
                zb = mw_zb;
            }

            //if (zb == null) {
            ZoomFactor = 1;
            var focus_loc = GetDesignerItemCenter(FocusAction);
            TranslateOffsetX = focus_loc.X;
            TranslateOffsetY = focus_loc.Y;
            return;
            // }

            //MpRect total_rect = DesignerItemsRect;
            ////var lb = zb.GetVisualDescendant<ListBox>();
            ////for (int i = 0; i < lb.ItemCount; i++) {
            ////    if (lb.ContainerFromIndex(i) is ListBoxItem lbi
            ////        && lbi.GetVisualDescendant<Shape>() is Shape s) {
            ////        MpRect cur_rect2 = s.Bounds.ToPortableRect();
            ////        var new_origin2 = s.TranslatePoint(new Point(), zb).Value.ToPortablePoint();
            ////        cur_rect2.Move(new_origin2);
            ////        total_rect = total_rect.Union(cur_rect2);
            ////    }
            ////}
            ////total_rect = total_rect.Inflate(20, 20);

            //double x_scale = ObservedDesignerBounds.Width / total_rect.Width;
            //double y_scale = ObservedDesignerBounds.Height / total_rect.Height;
            //// 2.7
            //Scale = Math.Min(x_scale, y_scale);

            ////var container_size = ObservedDesignerBounds.Size.ToPortablePoint();
            ////var rect_size = total_rect.Size.ToPortablePoint();
            ////var trans = container_size - rect_size - (rect_size * 0.5);

            ////var total_screen_tl = total_rect.TopLeft * Scale;
            //var trans = total_rect.TopLeft; //DesignerCenterLocation - total_rect.Centroid();

            ////var trans = (centeroid * Scale) + (DesignerCenterLocation * Scale);

            //TranslateOffsetX = -trans.X;
            //TranslateOffsetY = -trans.Y;

        }, () => FocusAction != null);

        private MpPoint GetDesignerItemCenter(MpAvActionViewModelBase avm) {
            return new MpPoint(
                            (DesignerCenterLocation.X - FocusAction.X - (DesignerItemDiameter / 2)),
                            (DesignerCenterLocation.Y - FocusAction.Y - (DesignerItemDiameter / 2)));
        }

        private MpPoint GetDesignerItemOrigin(MpAvActionViewModelBase avm) {
            return GetDesignerItemCenter(avm) - new MpPoint(DesignerItemDiameter / 2, DesignerItemDiameter / 2);
        }

        #region Designer Measure Properties

        public MpRect DesignerItemsRect {
            get {
                MpRect total_rect = null;
                SelectedTrigger.SelfAndAllDescendants.ForEach(x =>
                    total_rect = total_rect.Union(
                        new MpRect(
                            GetDesignerItemOrigin(x),
                            new MpSize(DesignerItemDiameter, DesignerItemDiameter))));
                return total_rect;
            }
        }
        public MpRect ObservedDesignerBounds { get; set; } = MpRect.Empty;
        public double DesignerItemDiameter => 50;

        public MpPoint DesignerCenterLocation => ObservedDesignerBounds.Size.ToPortablePoint() / 2;
        public MpPoint DefaultDesignerItemLocationLocation => new MpPoint(-DesignerItemDiameter / 2, -DesignerItemDiameter / 2);


        public double FocusActionScreenX => FocusAction == null ? 0 : (DesignerCenterLocation.X - FocusAction.X) + (DesignerItemDiameter / 2);
        public double FocusActionScreenY => FocusAction == null ? 0 : (DesignerCenterLocation.Y - FocusAction.Y) + (DesignerItemDiameter / 2);
        #endregion

        #region Designer Model Parsing Helpers

        string DesignerSettingsArg {
            get => SelectedTrigger == null ? null : SelectedTrigger.Arg1;
            set {
                if (SelectedTrigger == null) {
                    return;
                }
                SelectedTrigger.Arg1 = value;
            }
        }
        private void SetDesignerItemSettings(double scale, double offsetX, double offsetY, bool showGrid) {
            string arg2 = string.Join(
                ",",
                new string[] {
                    scale.ToString(),
                    offsetX.ToString(), offsetY.ToString(),
                    showGrid.ToString()});
            DesignerSettingsArg = arg2;
        }

        private double ParseScale(string text) {
            if (string.IsNullOrEmpty(DesignerSettingsArg)) {
                return 1.0d;
            }
            var arg2Parts = DesignerSettingsArg.SplitNoEmpty(",");
            if (arg2Parts.Length > 0) {
                return double.Parse(arg2Parts[0]);
            }
            return 1.0d;
        }

        private MpPoint ParseTranslationOffset(string text) {
            if (string.IsNullOrEmpty(DesignerSettingsArg)) {
                return DesignerCenterLocation;
            }
            var arg2Parts = DesignerSettingsArg.SplitNoEmpty(",");
            if (arg2Parts.Length >= 3) {
                return new MpPoint() {
                    X = double.Parse(arg2Parts[1]),
                    Y = double.Parse(arg2Parts[2])
                };
            }
            return DesignerCenterLocation;
        }
        private bool ParseShowGrid(string text) {
            if (SelectedTrigger == null) {
                return false;
            }
            if (string.IsNullOrEmpty(DesignerSettingsArg)) {
                return true;
            }
            var arg2Parts = DesignerSettingsArg.SplitNoEmpty(",");
            if (arg2Parts.Length >= 4) {
                return arg2Parts[3].ToLowerInvariant() == "true";
            }
            return true;
        }

        #endregion

        #endregion

        #region MpISidebarItemViewModel Implementation

        private double _defaultSelectorColumnVarDimLength_horiz = 800;
        private double _defaultSelectorColumnVarDimLength_vert = 400;
        public double DefaultSidebarWidth {
            get {
                if (IsWindowOpen) {
                    return 0;
                }
                if (MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    return MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
                double def_w = Math.Min(_defaultSelectorColumnVarDimLength_horiz, MpAvMainWindowViewModel.Instance.MainWindowWidth);
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    def_w = Math.Min(def_w, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width / 2);
                }
                return def_w;
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (IsWindowOpen) {
                    return 0;
                }
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvClipTrayViewModel.Instance.ObservedQueryTrayScreenHeight;
                }
                double def_h = _defaultSelectorColumnVarDimLength_vert;
                //if (SelectedItem != null) {
                //    //h += _defaultParameterColumnVarDimLength;
                //}
                if (MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    def_h = Math.Min(def_h, Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Height / 2);
                }
                return def_h;
            }
        }
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; } = 0;
        public string SidebarBgHexColor =>
            (Mp.Services.PlatformResource.GetResource("ActionPropertyListBgBrush") as IBrush).ToHex();

        bool MpISidebarItemViewModel.CanResize =>
            !IsWindowOpen;
        #endregion

        #endregion

        #region Properties

        #region View Models       

        public ObservableCollection<MpAvActionViewModelBase> Items { get; private set; } = new ObservableCollection<MpAvActionViewModelBase>();

        public ObservableCollection<MpAvTriggerActionViewModelBase> Triggers { get; private set; } = new ObservableCollection<MpAvTriggerActionViewModelBase>();
        //public IEnumerable<MpAvTriggerActionViewModelBase> SortedTriggers =>
        //    Items
        //    .Where(x => x is MpAvTriggerActionViewModelBase)
        //    .Cast<MpAvTriggerActionViewModelBase>()
        //    .OrderBy(x => x.LabelText);

        public MpAvTriggerActionViewModelBase SelectedTrigger { get; set; }
        public MpAvActionViewModelBase FocusAction { get; set; }

        public MpAvIHeaderMenuViewModel FocusHeaderViewModel {
            get {
                if(FocusAction == null) {
                    return this;
                }
                return FocusAction;
            }
        }

        #endregion

        #region Appearance
        #endregion

        #region Layout

        public Orientation SidebarOrientation {
            get {
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    return MpAvMainWindowViewModel.Instance.MainWindowLayoutOrientation;
                }
                if (IsWindowOpen) {
                    return Orientation.Horizontal;
                }
                if (MpAvSidebarItemCollectionViewModel.Instance.SelectedItemWidth <= _defaultSelectorColumnVarDimLength_vert * 1.75) {
                    return Orientation.Vertical;
                }
                return Orientation.Horizontal;
            }
        }

        #endregion

        #region State
        private bool _isPropertyViewExpanded = true;
        public bool IsPropertyViewExpanded {
            get {
                if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                    return true;
                }
                return _isPropertyViewExpanded;
            }
            set {
                if(_isPropertyViewExpanded != value) {
                    _isPropertyViewExpanded = value;
                    OnPropertyChanged(nameof(IsPropertyViewExpanded));
                }
            }
        }
        public bool HasShown { get; set; }
        public string[] DefaultActionGuids => new string[] {
            DEFAULT_ANNOTATOR_ANALYZE_GUID,
            DEFAULT_ANNOTATOR_TRIGGER_GUID
        };

        public string FocusActionName =>
            FocusAction == null ? string.Empty : FocusAction.Label;

        public bool IsHorizontal =>
                SidebarOrientation == Orientation.Horizontal;


        public int SelectedTriggerIdx {
            get => Triggers.IndexOf(SelectedTrigger);
            set => SelectedTrigger = Triggers.ElementAtOrDefault(value);
        }
        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                return Items.Any(x => x.IsAnyBusy);
            }
        }

        public bool IsRestoringEnabled { get; private set; } = false;
        #endregion

        #endregion

        #region Constructors

        private static MpAvTriggerCollectionViewModel _instance;
        public static MpAvTriggerCollectionViewModel Instance => _instance ?? (_instance = new MpAvTriggerCollectionViewModel());


        public MpAvTriggerCollectionViewModel() : base(null) {
            PropertyChanged += MpAvTriggerCollectionViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            IsBusy = true;

            Items.Clear();

            await CreateCoreAnnotateTriggerAsync();

            var tal = await MpDataModelProvider.GetAllTriggerActionsAsync();

            foreach (var ta in tal) {
                var tavm = await CreateTriggerViewModel(ta);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.ParentActionViewModel)));
            OnPropertyChanged(nameof(Items));

            // restore all blocks until core fully loaded (needs to wait to attach shortcuts or any other later loaded components)
            RestoreAllEnabledAsync().FireAndForgetSafeAsync();

            if (Items.Count() > 0) {
                // select most recent action
                MpAvActionViewModelBase actionToSelect = Items
                                .AggregateOrDefault((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);

                if (actionToSelect != null) {
                    SelectActionCommand.Execute(actionToSelect);
                }
            }

            IsBusy = false;
        }
        public async Task<MpAvTriggerActionViewModelBase> CreateTriggerViewModel(MpAction a) {
            if (a.ActionType != MpActionType.Trigger) {
                throw new Exception("This is only supposed to load root level triggers");
            }
            MpAvTriggerActionViewModelBase tavm = null;
            switch (a.Arg3.ToEnum<MpTriggerType>()) {
                case MpTriggerType.ClipAdded:
                    tavm = new MpAvContentAddTriggerViewModel(this);
                    break;
                case MpTriggerType.ClipTagged:
                    tavm = new MpAvContentTaggedTriggerViewModel(this);
                    break;
                case MpTriggerType.FileSystemChanged:
                    tavm = new MpAvFolderWatcherTriggerViewModel(this);
                    break;
                case MpTriggerType.Shortcut:
                    tavm = new MpAvShortcutTriggerViewModel(this);
                    break;
                case MpTriggerType.MonkeyCopyShortcut:
                    tavm = new MpAvMonkeyCopyTriggerViewModel(this);
                    break;
            }

            await tavm.InitializeAsync(a);

            return tavm;
        }


        public string GetUniqueTriggerName(string given_name) {
            if (!Triggers.Any(x => x.Label == given_name)) {
                return given_name;
            }
            int uniqueIdx = 1;
            string testName = string.Format(
                                        @"{0}{1}",
                                        given_name,
                                        uniqueIdx);

            while (Triggers.Any(x => x.Label == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        given_name,
                                        uniqueIdx);
            }
            return given_name + uniqueIdx;
        }


        public void SetErrorToolTip(int actionId, int argNum, string text) {
            // NOTE COMMENTED BELOW NEEDS MORE TESTING...
            return;
            //Dispatcher.UIThread.Post(async () => {
            //    if (FocusAction != null && FocusAction.ActionId != actionId) {
            //        // ignore non-visible tooltip validation changes
            //        return;
            //    }
            //    // wait for content control to bind to primary action...
            //    await Task.Delay(300);
            //    var apv = MpAvMainView.Instance.GetVisualDescendant<MpAvActionPropertyView>();
            //    if (apv != null) {
            //        var rapcc = apv.FindControl<ContentControl>("RootActionPropertyContentControl");
            //        if (rapcc != null) {

            //            var allArgControls =
            //                rapcc.GetVisualDescendants<Control>()
            //                .Where(x => x.Classes.Any(x => x.StartsWith("arg")));

            //            var argToolTip = new MpAvToolTipView() {
            //                ToolTipText = text,
            //                Classes = Classes.Parse("error")
            //            };
            //            foreach (var arg_control in allArgControls) {
            //                if (arg_control.Classes.Any(x => x == $"arg{argNum}")) {
            //                    ToolTip.SetTip(arg_control, argToolTip);
            //                    if (!arg_control.Classes.Contains("invalid")) {
            //                        arg_control.Classes.Add("invalid");
            //                    }
            //                } else {
            //                    ToolTip.SetTip(arg_control, null);
            //                    arg_control.Classes.Remove("invalid");
            //                }
            //            }
            //        }
            //    }
            //});
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvTriggerCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    break;
                case nameof(FocusAction):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    OnPropertyChanged(nameof(SelectedTriggerIdx));
                    if (FocusAction != null) {
                        // ntf popout window bindings of changes
                        FocusAction.OnPropertyChanged(nameof(FocusAction.ActionBackgroundHexColor));
                        FocusAction.OnPropertyChanged(nameof(FocusAction.Label));
                        if (FocusAction is MpAvIParameterCollectionViewModel pcvm) {
                            pcvm.OnPropertyChanged(nameof(pcvm.Items));
                            pcvm.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsVisible)));
                        }
                    }
                    OnPropertyChanged(nameof(FocusActionName));
                    if(this is MpAvIHeaderMenuViewModel hmvm) {
                        hmvm.OnPropertyChanged(nameof(hmvm.HeaderBackground));
                        hmvm.OnPropertyChanged(nameof(hmvm.HeaderForeground));
                        hmvm.OnPropertyChanged(nameof(hmvm.HeaderMenuItems));
                    }
                    OnPropertyChanged(nameof(FocusHeaderViewModel));
                    MpMessenger.SendGlobal(MpMessageType.FocusItemChanged);
                    break;
                case nameof(SelectedTrigger):
                    FocusAction = SelectedTrigger;

                    OnPropertyChanged(nameof(SelectedTriggerIdx));
                    OnPropertyChanged(nameof(MinZoomFactor));
                    OnPropertyChanged(nameof(MaxZoomFactor));
                    OnPropertyChanged(nameof(ZoomFactor));
                    OnPropertyChanged(nameof(TranslateOffsetX));
                    OnPropertyChanged(nameof(TranslateOffsetY));
                    OnPropertyChanged(nameof(IsGridVisible));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActionDesignerVisible)));
                    OnPropertyChanged(nameof(Items));
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsTrigger)));
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActionDesignerVisible)));

            if (e.NewItems != null) {
                foreach (MpAvActionViewModelBase avm in e.NewItems) {
                    if (avm is MpAvTriggerActionViewModelBase tvmb &&
                        !Triggers.Contains(tvmb)) {
                        Triggers.Add(tvmb);
                    }
                }
            }
            if (e.OldItems != null) {
                foreach (MpAvActionViewModelBase avm in e.OldItems) {
                    if (avm is MpAvTriggerActionViewModelBase tvmb) {
                        Triggers.Remove(tvmb);
                    }
                }
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SidebarItemSizeChanged:
                    OnPropertyChanged(nameof(SidebarOrientation));
                    OnPropertyChanged(nameof(IsHorizontal));
                    break;
                case MpMessageType.SelectedSidebarItemChangeEnd:
                    if (!IsSelected) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        await Task.Delay(150);
                        ResetDesignerViewCommand.Execute(null);
                    });
                    break;
            }
        }


        public async Task RestoreAllEnabledAsync() {
            // NOTE this is only called on init and needs to wait for dependant vm's to load so wait here
            while (!Mp.Services.StartupState.IsCoreLoaded) {
                await Task.Delay(100);
            }

            IsRestoringEnabled = true;

            var enabled_triggers =
            Items
            .Where(x => x is MpAvTriggerActionViewModelBase)
            .Cast<MpAvTriggerActionViewModelBase>()
            .Where(x => x.IsEnabled);

            enabled_triggers
            .ForEach(x => x.EnableTriggerCommand.Execute(null));

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }
            while (enabled_triggers.Any(x => !x.IsEnabled)) {
                // wait to enable
                await Task.Delay(100);
            }
            IsRestoringEnabled = false;
        }
        private async Task UpdateSortOrderAsync() {
            Items.ForEach(x => x.SortOrderIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Action.WriteToDatabaseAsync()));
        }

        private MpAvWindow CreateDesignerWindow() {
            // WINDOW
            var tacv = new MpAvTriggerActionChooserView();
            Control content = tacv;
            if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                content = new Viewbox() {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Child = tacv
                };
            }

            var dw = new MpAvWindow() {
                Width = MpAvThemeViewModel.Instance.IsMobileOrWindowed ? double.NaN : 1000,
                Height = MpAvThemeViewModel.Instance.IsMobileOrWindowed ? double.NaN : 500,
                ShowInTaskbar = true,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("BoltImage", typeof(MpAvWindowIcon), null, null) as MpAvWindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = content,
                DataContext = this,
                Padding = MpAvThemeViewModel.Instance.IsMobileOrWindowed ? new(): new Thickness(10),
                Background = Brushes.DimGray
            };
            dw.Classes.Add("fadeIn");
            dw.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(FocusActionName),
                    StringFormat = UiStrings.ActionDesignerWindowTitle.ToWindowTitleText()
                });

            void OnOpened(object sender, EventArgs e) {
                dw.Opened -= OnOpened;
                ResetDesignerViewCommand.Execute(null);
                MpAvSidebarItemCollectionViewModel.Instance.ToggleIsSidebarItemSelectedCommand.Execute(this);
            }
#if MOBILE_OR_WINDOWED

            dw.Bind(
                MpAvChildWindow.HeaderViewModelProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(FocusHeaderViewModel)
                }
                );

#endif

            dw.Opened += OnOpened;
            return dw;
        }

        private async Task CreateCoreAnnotateTriggerAsync() {
            int def_ann_check = await MpDataModelProvider.GetItemIdByGuidAsync<MpAction>(DEFAULT_ANNOTATOR_TRIGGER_GUID);
            if (def_ann_check != 0) {
                // already created 
                return;
            }
            while (true) {
                bool can_create =
                    MpAvAnalyticItemCollectionViewModel.Instance.IsLoaded;
                if (can_create) {
                    break;
                }
                await Task.Delay(100);
            }


            int def_annotate_preset_id = 0;
            if (MpAvAnalyticItemCollectionViewModel
                .Instance
                .AllPresets
                .FirstOrDefault(x => x.PresetGuid == MpPluginLoader.CoreAnnotatorDefaultPresetGuid) is MpAvAnalyticItemPresetViewModel aipvm) {
                def_annotate_preset_id = aipvm.AnalyticItemPresetId;
            }
            if (def_annotate_preset_id == 0) {
                MpDebug.Break("Warning creating Default annotate trigger, can't find default annotator :( But creating anyways which should notify user somethings wrong");
                return;
            }

            // NOTE this must be called after analyzer collection has initialized

            #region Annotate new text

            // NOTE forcing trigger to constant guid so creation doesn't depend on initial startup
            MpAction annotate_trigger_action = await MpAction.CreateAsync(
                guid: DEFAULT_ANNOTATOR_TRIGGER_GUID,
                label: UiStrings.TriggerAnnTriggerLabel,
                actionType: MpActionType.Trigger,
                sortOrderIdx: 0,
                arg2: true.ToString(),
                arg3: MpTriggerType.ClipAdded.ToString(),
                location: DefaultDesignerItemLocationLocation);

            var annotate_trigger_action_text_type_param = await MpParameterValue.CreateAsync(
                hostType: MpParameterHostType.Action,
                hostId: annotate_trigger_action.Id,
                paramId: MpAvContentAddTriggerViewModel.CONTENT_TYPE_PARAM_ID.ToString(),
                value: MpCopyItemType.Text.ToString());


            MpAction annotate_analyze_action = await MpAction.CreateAsync(
                    guid: DEFAULT_ANNOTATOR_ANALYZE_GUID,
                     label: UiStrings.TriggerAnnAnalyzeLabel,
                     actionType: MpActionType.Analyze,
                     parentId: annotate_trigger_action.Id,
                     sortOrderIdx: 0,
                     location: DefaultDesignerItemLocationLocation + new MpPoint(DesignerItemDiameter * 3, 0));


            var annotate_analyze_action_def_annotator_preset_param = await MpParameterValue.CreateAsync(
                hostType: MpParameterHostType.Action,
                hostId: annotate_analyze_action.Id,
                paramId: MpAvAnalyzeActionViewModel.SELECTED_ANALYZER_PARAM_ID.ToString(),
                value: def_annotate_preset_id.ToString());

            #endregion
        }
        #endregion

        #region Commands
        public ICommand ShowTriggerSelectorMenuCommand => new MpCommand<object>(
             (args) => {

                 MpAvMenuView.ShowMenu(
                     target: args as Control,
                     dc: PopupMenuViewModel);
             });

        public ICommand AddTriggerCommand => new MpCommand<object>(
             async (args) => {
                 IsBusy = true;

                 MpTriggerType tt = args == null ? MpTriggerType.None : (MpTriggerType)args;

                 MpAction na = await MpAction.CreateAsync(
                         label: GetUniqueTriggerName(string.Format(UiStrings.ActionTriggerDefaultLabel, tt.EnumToUiString())),
                         actionType: MpActionType.Trigger,
                         sortOrderIdx: Items.Count,
                         arg2: false.ToString(),
                         arg3: ((int)tt).ToString(),
                         location: DefaultDesignerItemLocationLocation);

                 var new_trigger_vm = await CreateTriggerViewModel(na);

                 while (new_trigger_vm.IsBusy) {
                     await Task.Delay(100);
                 }

                 await Task.Delay(300);

                 OnPropertyChanged(nameof(Items));
                 new_trigger_vm.EnableTriggerCommand.Execute(null);

                 bool was_empty = SelectedTrigger == null;
                 SelectedTrigger = new_trigger_vm;
                 new_trigger_vm.OnPropertyChanged(nameof(new_trigger_vm.IsTrigger));
                 OnPropertyChanged(nameof(SelectedTrigger));

                 await Task.Delay(300);
                 ResetDesignerViewCommand.Execute(null);

                 if (was_empty) {
                     // when empty resetting twice puts trigger in center
                     ResetDesignerViewCommand.Execute(null);
                 }
                 IsBusy = false;
             });

        public MpIAsyncCommand<object> DeleteActionCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsBusy = true;

                bool is_cut = false;
                var child_to_delete_avm = args as MpAvActionViewModelBase;
                if (child_to_delete_avm == null) {
                    if (args is object[] argParts) {
                        child_to_delete_avm = argParts[0] as MpAvActionViewModelBase;
                        is_cut = (bool)argParts[1];
                    }
                    if (child_to_delete_avm == null) {
                        // link error (this cmd is called from arg vm using parentacvm)
                        MpDebug.Break();
                        IsBusy = false;
                        return;
                    }
                }

                List<MpAvActionViewModelBase> to_remove_list = new List<MpAvActionViewModelBase>() {
                    child_to_delete_avm
                };

                bool remove_descendants = false;
                if (is_cut) {
                    // cut only allowed for non-trigger actions
                    remove_descendants = true;
                } else if (child_to_delete_avm.ParentActionId == 0) {
                    // trigger deletes children by default
                    remove_descendants = true;
                } else if (child_to_delete_avm.Children.Count() > 0) {
                    //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;
                    MpAvWindow anchor_win = IsWindowOpen ? MpAvWindowManager.LocateWindow(this) : MpAvWindowManager.MainWindow;
                    Control anchor = anchor_win == null ? null : anchor_win.GetVisualDescendant<MpAvActionDesignerView>();

                    bool? remove_descendants_result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                        title: UiStrings.TriggerRemoveActionTitle,
                        message: string.Format(UiStrings.TriggerRemoveActionText, child_to_delete_avm.Label, child_to_delete_avm.ParentActionViewModel.Label),
                        iconResourceObj: "ChainImage",
                        anchor: anchor);
                    //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
                    if (remove_descendants_result.IsNull()) {
                        // cancel
                        IsBusy = false;
                        return;
                    }
                    remove_descendants = remove_descendants_result.IsTrue();
                }

                FocusAction = child_to_delete_avm.ParentActionViewModel;

                if (remove_descendants) {
                    to_remove_list.AddRange(child_to_delete_avm.AllDescendants);
                } else {
                    child_to_delete_avm.Children.ForEach(x => x.ParentActionId = child_to_delete_avm.ParentActionId);
                }

                if (FocusAction != null) {
                    await FocusAction.SetChildRestoreStateAsync();
                }

                foreach (var to_remove_avm in to_remove_list.OrderByDescending(x => x.TreeLevel).ThenBy(x => x.SortOrderIdx)) {
                    while (to_remove_avm.IsBusy) {
                        await Task.Delay(100);
                    }
                    await to_remove_avm.Action.DeleteFromDatabaseAsync();
                    Items.Remove(to_remove_avm);
                }
                if (child_to_delete_avm.ParentActionId == 0) {
                    await UpdateSortOrderAsync();
                    SelectedTrigger = null;
                } else {
                    child_to_delete_avm.ParentActionViewModel.OnActionComplete -= child_to_delete_avm.OnActionInvoked;
                    await child_to_delete_avm.ParentActionViewModel.UpdateSortOrderAsync();
                }

                if (FocusAction != null) {
                    await FocusAction.SetChildRestoreStateAsync();
                }
                IsBusy = false;
            });

        public ICommand SelectActionCommand => new MpCommand<object>(
            (args) => {
                MpAvActionViewModelBase toSelect_avmb = null;
                int focusArgNum = 0;
                string error_text = null;
                if (args is MpAvActionViewModelBase) {
                    toSelect_avmb = args as MpAvActionViewModelBase;
                } else if (args is int actionId) {
                    toSelect_avmb = Items.FirstOrDefault(x => x.ActionId == actionId);
                } else if (args is object[] argParts) {
                    if (argParts[0] is int actionIdPart) {
                        toSelect_avmb = Items.FirstOrDefault(x => x.ActionId == actionIdPart);
                    }
                    if (argParts[1] is int) {
                        focusArgNum = (int)argParts[1];
                    }
                    if (argParts[2] is string) {
                        error_text = argParts[2] as string;
                    }
                }
                SelectedTrigger = toSelect_avmb.RootTriggerActionViewModel;
                FocusAction = toSelect_avmb;

                OnPropertyChanged(nameof(FocusAction));

                if (focusArgNum > 0) {
                    // this should only occur from clicking 'fix' ntf button
                    if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                        MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.Execute(null);
                    }
                    MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(this);
                }

                if (toSelect_avmb != null) {
                    SetErrorToolTip(toSelect_avmb.ActionId, focusArgNum, error_text);
                }
            }, (args) => {
                MpAvActionViewModelBase toSelect_avmb = null;
                int focusArgNum = -1;
                if (args is MpAvActionViewModelBase) {
                    toSelect_avmb = args as MpAvActionViewModelBase;
                } else if (args is int actionId) {
                    toSelect_avmb = Items.FirstOrDefault(x => x.ActionId == actionId);
                } else if (args is object[] argParts) {
                    if (argParts[0] is int actionIdPart) {
                        toSelect_avmb = Items.FirstOrDefault(x => x.ActionId == actionIdPart);
                    }
                    if (argParts[1] is int) {
                        focusArgNum = (int)argParts[1];
                    }
                }
                if (toSelect_avmb != null && focusArgNum >= 0) {
                    // always allow execute when focus is passed (should only be from validation ntf)
                    return true;
                }
                if (toSelect_avmb == null && FocusAction != null) {
                    return true;
                }
                if (toSelect_avmb != null && FocusAction == null) {
                    return true;
                }
                if (toSelect_avmb == null && FocusAction == null) {
                    return true;
                }
                return FocusAction.ActionId != toSelect_avmb.ActionId;
            });

        public ICommand InvokeActionCommand => new MpCommand<object>(
            (args) => {
                int actionId = 0;
                if (args is string) {
                    actionId = int.Parse(args.ToString());
                } else if (args is int) {
                    actionId = (int)args;
                } else if (args is MpAvActionViewModelBase argAvm) {
                    actionId = argAvm.ActionId;
                }
                var avm = Items.FirstOrDefault(x => x.ActionId == actionId);
                if (avm == null) {
                    return;
                }
                avm.InvokeThisActionCommand.Execute(null);

            });

        public ICommand ShowDesignerWindowCommand => new MpCommand(
            () => {
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    if (IsWindowOpen) {
                        IsWindowActive = true;
                        return;
                    }

                    var dw = CreateDesignerWindow();
                    dw.Show();

                    OnPropertyChanged(nameof(SidebarOrientation));
                } else {
                    // Some kinda view nav here
                    // see https://github.com/AvaloniaUI/Avalonia/discussions/9818

                }
                IsWindowOpen = true;
            });

        public ICommand ZoomInCommand => new MpCommand(
            () => {
                ZoomFactor = Math.Min(MaxZoomFactor, ZoomFactor + StepDelta);
            });
        public ICommand ZoomOutCommand => new MpCommand(
            () => {
                ZoomFactor = Math.Max(MinZoomFactor, ZoomFactor - StepDelta);
            });

        public ICommand ResetZoomCommand => new MpCommand(
            () => {
                ZoomFactor = DefaultZoomFactor;
            });
        public ICommand SetZoomCommand => new MpCommand<object>(
            (args) => {
                if (args is not double newZoomFactor) {
                    return;
                }
                ZoomFactor = Math.Clamp(newZoomFactor, MinZoomFactor, MaxZoomFactor);
            });
        #endregion
    }
}
