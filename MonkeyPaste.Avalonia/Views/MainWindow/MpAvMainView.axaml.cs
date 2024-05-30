using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainView :
#if WINDOWED
        MpAvWindow<MpAvMainWindowViewModel>,
#else
        MpAvUserControl<MpAvMainWindowViewModel>,
#endif
        MpAvIResizableControl,
        MpIAsyncObject {
        #region Private Variables

        #endregion

        #region Constants

        public const double MIN_TRAY_VAR_DIM_LENGTH = 150.0d;

        #endregion

        #region Statics

        private static MpAvMainView _instance;
        public static MpAvMainView Instance => _instance;

        public static void Init(MpAvMainWindowViewModel mwvm) {
#if DESKTOP
            var mw = new MpAvMainWindow() {
                DataContext = mwvm,
#if MOBILE_OR_WINDOWED
                ShowHeader = false 
#endif
            };
            _instance = mw.Content as MpAvMainView;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
#if WINDOWED

                App.SetPrimaryView(_instance);
#else
                desktop.MainWindow = mw;
#endif
            }
#else
            _instance = new MpAvMainView() {
                DataContext = mwvm
            };
#endif

#if MOBILE_OR_WINDOWED
            //_instance.BackCommand = MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand;
            //_instance.BackCommandParameter = MpSettingsTabType.Preferences;
            //_instance.MenuItems = [
            //    new MpAvMenuItemViewModel() {
            //        IconSourceObj = "SearchImage",
            //        Command = MpAvSearchBoxViewModel.Instance.ExpandSearchBoxCommand
            //    }
            //    ];
            _instance.ShowHeader = false;
#endif
        }

        #endregion

        #region Interfaces

        #region MpIAsyncObject I

        public bool IsBusy { get; set; }

        #endregion

        #region MpAvIResizableControl Implementation

        private Control _resizerControl;

        Control MpAvIResizableControl.ResizerControl {
            get {
                if (_resizerControl == null) {
                    var mwtmv = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
                    _resizerControl = mwtmv.FindControl<Border>("MainWindowResizerBorder");
                }

                return _resizerControl;
            }
        }

        #endregion

        #endregion

        #region Properties

        public Grid RootGrid { get; }

        #endregion

        #region Constructors

        public MpAvMainView() {
            if (_instance != null) {
                MpDebug.Break("Duplicate singleton");
                return;
            }

            InitializeComponent();
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            RootGrid = this.FindControl<Grid>("MainWindowContainerGrid");

            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
            //sidebarSplitter.DragStarted += (s, e) => MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChangeBegin);
            //sidebarSplitter.DragStarted += (s, e) => MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChangeEnd);
            sidebarSplitter.DragDelta += SidebarSplitter_DragDelta;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        #region Orientation Updates

        public void UpdateContentLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            //var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");

            var sbbg = this.FindControl<MpAvSidebarButtonGroupView>("SidebarButtonGroup");
            var ssbcb = this.FindControl<Border>("SelectedSidebarContainerBorder");
            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");
            var ctrcb = this.FindControl<Border>("ClipTrayContainerBorder");

            var ctrcv = this.FindControl<MpAvClipTrayContainerView>("ClipTrayContainerView");
            var ctrcv_cg = ctrcv.FindControl<Grid>("ClipTrayContainerGrid");
            var ctrcv_ptrv = ctrcv.FindControl<MpAvPinTrayView>("PinTrayView");
            var ctrcv_ptr_lb = ctrcv_ptrv.FindControl<ListBox>("PinTrayListBox");
            var ctrcv_ptr_cg = ctrcv_ptrv.FindControl<Border>("PinTrayContainerBorder").Child as Grid;
            var ctrcv_gs = ctrcv.FindControl<GridSplitter>("ClipTraySplitter");
            var ctrcv_ctrv = ctrcv.FindControl<MpAvQueryTrayView>("ClipTrayView");
            var ctrcv_ctrv_cg = ctrcv_ctrv.FindControl<Grid>("QueryTrayContainerGrid");
            var ctrcv_ctr_lb = ctrcv_ctrv.FindControl<ListBox>("ClipTrayListBox");

            var pin_tray_ratio = MpAvClipTrayViewModel.Instance.GetCurrentPinTrayRatio();
            mwtg.RowDefinitions.Clear();
            mwtg.ColumnDefinitions.Clear();
            double gs_fixed_length =
#if MULTI_WINDOW
                MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength; 
#else
                0;
#endif
            double dbl_gs_fixed_length = gs_fixed_length * 2;

            if (mwvm.IsHorizontalOrientation) {
                // HORIZONTAL

                mwtg.RowDefinitions.Add(new RowDefinition(new GridLength(0, GridUnitType.Auto)));
                mwtg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

                var sbbg_cd = new ColumnDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));

                var ssbcb_cd = new ColumnDefinition(Math.Max(0, sbicvm.ContainerBoundWidth), GridUnitType.Pixel);
                ssbcb_cd.Bind(
                    ColumnDefinition.WidthProperty,
                    new Binding() {
                        Source = sbicvm,
                        Path = nameof(sbicvm.ContainerBoundWidth),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                var ctrcb_cd = new ColumnDefinition(Math.Max(0, ctrvm.ContainerBoundWidth), GridUnitType.Pixel);
                ctrcb_cd.Bind(
                    ColumnDefinition.WidthProperty,
                    new Binding() {
                        Source = ctrvm,
                        Path = nameof(ctrvm.ContainerBoundWidth),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                mwtg.ColumnDefinitions.Add(sbbg_cd);
                mwtg.ColumnDefinitions.Add(ssbcb_cd);
                mwtg.ColumnDefinitions.Add(ctrcb_cd);

                // sidebar buttons
                Grid.SetRow(sbbg, 0);
                Grid.SetRowSpan(sbbg, 2);
                Grid.SetColumn(sbbg, 0);

                // sidebar content
                Grid.SetRow(ssbcb, 0);
                Grid.SetRowSpan(ssbcb, 2);
                Grid.SetColumn(ssbcb, 1);

                // sidebar splitter
                Grid.SetRow(sbgs, 0);
                Grid.SetRowSpan(sbgs, 2);
                Grid.SetColumn(sbgs, 1);
                sbgs.Height = double.NaN;
                sbgs.VerticalAlignment = VerticalAlignment.Stretch;
                sbgs.Width = gs_fixed_length;
                sbgs.HorizontalAlignment = HorizontalAlignment.Right;
                sbgs.ResizeDirection = GridResizeDirection.Columns;

                // cliptray container border
                Grid.SetRow(ctrcb, 1);
                Grid.SetColumn(ctrcb, 2);

                // cliptraycontainer column definitions (horizontal)
                ctrcv_cg.RowDefinitions.Clear();

                // pintray column definition
                var ptrv_cd = new ColumnDefinition(new GridLength(pin_tray_ratio.Width, GridUnitType.Star));
                ptrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenWidth)
                    });
                ptrv_cd.Bind(
                    ColumnDefinition.MaxWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MaxPinTrayScreenWidth)
                    });

                // cliptray column definition
                var ctrv_cd = new ColumnDefinition(new GridLength(1 - pin_tray_ratio.Width, GridUnitType.Star));
                ctrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenWidth)
                    });

                ctrcv_cg.ColumnDefinitions = new ColumnDefinitions() { ptrv_cd, ctrv_cd };

                // pin tray listbox padding (horizontal) for head/tail drop adorners
                if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                    ctrcv_ptr_lb.Padding = new Thickness(dbl_gs_fixed_length, 0, dbl_gs_fixed_length, 0);
                } else {
                    ctrcv_ptr_lb.Padding = new Thickness();
                }

                // add margin for grid splitter size so boxshadow is symmetrical
                ctrcv_ptrv.Margin = new Thickness(0, 0, gs_fixed_length, 0);

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Right;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Stretch;
                ctrcv_gs.Width = gs_fixed_length;
                ctrcv_gs.Height = double.NaN;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Columns;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeWestEast);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 0);
                Grid.SetColumn(ctrcv_ctrv, 1);

                ctrcv_ctr_lb.Margin = new Thickness(0);
            } else {
                // VERTICAL 
                var ctrcb_rd = new RowDefinition(1, GridUnitType.Star);
                ctrcb_rd.Bind(
                    RowDefinition.HeightProperty,
                    new Binding() {
                        Source = ctrvm,
                        Path = nameof(ctrvm.ContainerBoundHeight),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                ctrcb_rd.Bind(
                    RowDefinition.MaxHeightProperty,
                    new Binding() { Source = ctrvm, Path = nameof(ctrvm.MaxContainerScreenHeight) });

                var ssbcb_rd = new RowDefinition(Math.Max(0, sbicvm.ContainerBoundHeight), GridUnitType.Pixel);
                ssbcb_rd.Bind(
                    RowDefinition.HeightProperty,
                    new Binding() {
                        Source = sbicvm,
                        Path = nameof(sbicvm.ContainerBoundHeight),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                var sbbg_rd = new RowDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));
                sbbg_rd.MinHeight = sbicvm.ButtonGroupFixedDimensionLength;

                mwtg.RowDefinitions.Add(ctrcb_rd);
                mwtg.RowDefinitions.Add(ssbcb_rd);
                mwtg.RowDefinitions.Add(sbbg_rd);

                // cliptray container view
                Grid.SetRow(ctrcb, 0);
                Grid.SetColumn(ctrcb, 0);

                // sidebar content
                Grid.SetRow(ssbcb, 1);
                Grid.SetRowSpan(ssbcb, 1);
                Grid.SetColumn(ssbcb, 0);

                // sidebar splitter
                Grid.SetRow(sbgs, 1);
                Grid.SetRowSpan(sbgs, 1);
                Grid.SetColumn(sbgs, 0);
                sbgs.Height = gs_fixed_length;
                sbgs.Width = double.NaN;
                sbgs.VerticalAlignment = VerticalAlignment.Top;
                sbgs.HorizontalAlignment = HorizontalAlignment.Stretch;
                sbgs.ResizeDirection = GridResizeDirection.Rows;

                // sidebar buttons
                Grid.SetRow(sbbg, 2);
                Grid.SetRowSpan(sbbg, 1);
                Grid.SetColumn(sbbg, 0);

                // cliptraycontainer column definitions (vertical)
                ctrcv_cg.ColumnDefinitions.Clear();

                // pintray row definitions
                var ptrv_rd = new RowDefinition(new GridLength(pin_tray_ratio.Height, GridUnitType.Star));
                ptrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenHeight)
                    });
                ptrv_rd.Bind(
                    RowDefinition.MaxHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MaxPinTrayScreenHeight)
                    });

                //cliptray row definitions
                var ctrv_rd = new RowDefinition(new GridLength(1 - pin_tray_ratio.Height, GridUnitType.Star));
                ctrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenHeight)
                    });

                ctrcv_cg.RowDefinitions = new RowDefinitions() { ptrv_rd, ctrv_rd };

                if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                    ctrcv_ptr_lb.Padding = new Thickness(dbl_gs_fixed_length);
                } else {
                    ctrcv_ptr_lb.Padding = new Thickness();
                }

                // add margin for grid splitter size so boxshadow is symmetrical
                ctrcv_ptrv.Margin = new Thickness(0, 0, 0, gs_fixed_length);

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Stretch;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Bottom;
                ctrcv_gs.Width = double.NaN;
                ctrcv_gs.Height = gs_fixed_length;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Rows;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 1);
                Grid.SetColumn(ctrcv_ctrv, 0);

                // adj lb to line up w/ pin tiles 
#if MULTI_WINDOW
                ctrcv_ctr_lb.Margin = new Thickness(10, 0, 0, 0); 
#endif
            }

            UpdateSidebarGridsplitter();
            UpdateTitleLayout();
            UpdateEdgyTooltips();
            ClampContentSizes();
            //UpdateResizerLayout();
            mwtg.InvalidateAll();

            // NOTE this is trying to fix empty message centering...
            ctrcv_ctrv_cg.InvalidateAll();
            ctrcv_ptr_cg.InvalidateAll();
        }

        private void UpdateEdgyTooltips() {
            var sbg = this.FindControl<MpAvSidebarButtonGroupView>("SidebarButtonGroup");
            var sb_edgies = sbg
                .GetVisualDescendants<Control>()
                .Where(x => x.Classes.Any(x => x.StartsWith("tt_")));

            // clear old edgies
            foreach (var sbc in sb_edgies) {
                var to_remove_classes = sbc.Classes.Where(x => x.StartsWith("tt_")).ToList();
                ;
                foreach (var to_remove_class in to_remove_classes) {
                    sbc.Classes.Remove(to_remove_class);
                }
            }

            IEnumerable<Control> new_edgies = null;

            switch (BindingContext.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Left:
                case MpMainWindowOrientationType.Right:
                    new_edgies = sbg.GetVisualDescendants<Button>();
                    break;
                case MpMainWindowOrientationType.Top:
                    new_edgies = new[] { sbg.GetVisualDescendants<Button>().FirstOrDefault() };
                    break;
                case MpMainWindowOrientationType.Bottom:
                    new_edgies = new[] { sbg.GetVisualDescendants<Button>().LastOrDefault() };
                    break;
            }

            string edgy_tooltip_class =
                $"tt_near_{BindingContext.MainWindowOrientationType.ToString().ToLowerInvariant()}";

            foreach (var sbc in new_edgies.Where(x => x != null)) {
                sbc.Classes.Add(edgy_tooltip_class);
            }
        }

        private void UpdateTitleLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var tmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            var mwcg = this.FindControl<Grid>("MainWindowContainerGrid");
            var tmv = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
            var fmv = this.FindControl<MpAvFilterMenuView>("FilterMenuView");
            var ttrv = fmv.FindControl<MpAvTagTrayView>("TagTrayView");
            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");

            var tmv_cg = tmv.FindControl<Grid>("TitlePanel");
            var tmv_lsp = tmv.FindControl<StackPanel>("LeftStackPanel");
            var tmv_wohb = tmv.FindControl<Button>("WindowOrientationHandleButton");
            var tmv_rsp = tmv.FindControl<StackPanel>("RightStackPanel");
            var tmv_gltb = tmv.FindControl<Control>("GridLayoutToggleButton");
            var tmv_min_btn = tmv.FindControl<Button>("MinimizeMainWindowButton");

            var tmv_zfv = tmv.FindControl<MpAvZoomFactorView>("ZoomFactorView");
            var tmv_zoom_slider_track_b = tmv_zfv.FindControl<Border>("ZoomTrackLine");
            var tmv_zoom_slider_min_b = tmv_zfv.FindControl<Border>("ZoomMinLine");
            var tmv_zoom_slider_def_b = tmv_zfv.FindControl<Border>("ZoomDefaultLine");
            var tmv_zoom_slider_max_b = tmv_zfv.FindControl<Border>("ZoomMaxLine");
            var tmv_zoom_slider_val_b = tmv_zfv.FindControl<Border>("CurValLine");

            double resizer_short_side = 0;

            mwcg.ColumnDefinitions.Clear();
            mwcg.RowDefinitions.Clear();

            bool is_horiz =
#if MOBILE_OR_WINDOWED
                true;// !mwvm.IsHorizontalOrientation;
#else
                mwvm.IsHorizontalOrientation;
#endif
            MpMainWindowOrientationType anchor_orientation =
#if MOBILE_OR_WINDOWED
                is_horiz ? MpMainWindowOrientationType.Left : MpMainWindowOrientationType.Bottom;
#else
                mwvm.MainWindowOrientationType;
#endif

            if (is_horiz) {
                // HORIZONTAL
                tmv.MaxHeight = MpAvMainWindowTitleMenuViewModel.Instance.DefaultTitleMenuFixedLength;
                tmv.MaxWidth = double.PositiveInfinity;

                tmvm.TitleMenuWidth = mwvm.MainWindowWidth;
                tmvm.TitleMenuHeight = tmvm.DefaultTitleMenuFixedLength;
                var tmv_rd = new RowDefinition(Math.Max(0, tmvm.TitleMenuHeight), GridUnitType.Pixel) {
                    MaxHeight = tmv.MaxHeight
                };

                fmvm.FilterMenuHeight = fmvm.DefaultFilterMenuFixedSize;
                var fmv_rd = new RowDefinition(Math.Max(0, fmvm.FilterMenuHeight), GridUnitType.Pixel);


                if (anchor_orientation == MpMainWindowOrientationType.Top) {
                    // TOP
                    mwcg.RowDefinitions.Add(fmv_rd);
                    mwcg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
                    mwcg.RowDefinitions.Add(tmv_rd);

                    Grid.SetRow(fmv, 0);
                    Grid.SetRow(mwtg, 1);
                    Grid.SetRow(tmv, 2);

                    tmv.Margin = new Thickness(0, 0, 0, resizer_short_side);
                } else {
                    // BOTTOM
                    mwcg.RowDefinitions.Add(tmv_rd);
                    mwcg.RowDefinitions.Add(fmv_rd);
                    mwcg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

                    Grid.SetRow(tmv, 0);
                    Grid.SetRow(fmv, 1);
                    Grid.SetRow(mwtg, 2);

                    tmv.Margin = new Thickness(0, resizer_short_side, 0, 0);
                }

                Grid.SetColumn(fmv, 0);
                Grid.SetColumn(mwtg, 0);
                Grid.SetColumn(tmv, 0);
                Grid.SetRowSpan(tmv, 1);

                tmv_lsp.Orientation = Orientation.Horizontal;
                tmv_lsp.HorizontalAlignment = HorizontalAlignment.Left;
                tmv_lsp.VerticalAlignment = VerticalAlignment.Stretch;

                tmv_wohb.VerticalAlignment = VerticalAlignment.Stretch;
                tmv_wohb.HorizontalAlignment = HorizontalAlignment.Center;
                if (tmv_wohb.RenderTransform is RotateTransform rott) {
                    rott.Angle = 0;
                }

                tmv_rsp.Orientation = Orientation.Horizontal;
                tmv_rsp.HorizontalAlignment = HorizontalAlignment.Right;
                tmv_rsp.VerticalAlignment = VerticalAlignment.Stretch;

                tmv_zfv.Width = tmvm.ZoomSliderLength;
                tmv_zfv.Height = tmvm.DefaultTitleMenuFixedLength;

                tmv_zoom_slider_track_b.Width = double.NaN;
                tmv_zoom_slider_track_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_track_b.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_zoom_slider_track_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_min_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_min_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_min_b.HorizontalAlignment = HorizontalAlignment.Left;
                tmv_zoom_slider_min_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_def_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_def_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.25;

                tmv_zoom_slider_max_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_max_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_max_b.HorizontalAlignment = HorizontalAlignment.Right;
                tmv_zoom_slider_max_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_val_b.Width = tmvm.ZoomSliderValueLength;
                tmv_zoom_slider_val_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;

                tmv_gltb.Margin = new Thickness(10, 0, 0, 0);
                tmv_min_btn.Margin = new Thickness(5, 0, 0, 0);
                //tmv_zoom_slider_val_btn.HorizontalAlignment = HorizontalAlignment.Center;
                //tmv_zoom_slider_val_btn.VerticalAlignment = VerticalAlignment.Stretch;
            } else {
                // VERTICAL
                tmv.MaxHeight = double.PositiveInfinity; 
                tmv.MaxWidth = tmvm.DefaultTitleMenuFixedLength;

                tmvm.TitleMenuWidth = tmvm.DefaultTitleMenuFixedLength;
                tmvm.TitleMenuHeight = mwvm.MainWindowHeight;
                var tv_cd = new ColumnDefinition(Math.Max(0, tmvm.TitleMenuWidth), GridUnitType.Pixel);

                fmvm.FilterMenuHeight = fmvm.DefaultFilterMenuFixedSize;
                var fv_rd = new RowDefinition(Math.Max(0, fmvm.FilterMenuHeight), GridUnitType.Pixel);

                mwcg.RowDefinitions.Add(fv_rd);
                mwcg.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

                if (mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Left) {
                    // LEFT
                    mwcg.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                    mwcg.ColumnDefinitions.Add(tv_cd);

                    Grid.SetRow(fmv, 0);
                    Grid.SetColumn(fmv, 0);

                    Grid.SetRow(mwtg, 1);
                    Grid.SetColumn(mwtg, 0);

                    Grid.SetRow(tmv, 0);
                    Grid.SetColumn(tmv, 1);
                    Grid.SetRowSpan(tmv, 2);

                    tmv.Margin = new Thickness(0, 0, resizer_short_side, 0);
                } else {
                    // RIGHT
                    mwcg.ColumnDefinitions.Add(tv_cd);
                    mwcg.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

                    Grid.SetRow(fmv, 0);
                    Grid.SetColumn(fmv, 1);

                    Grid.SetRow(mwtg, 1);
                    Grid.SetColumn(mwtg, 1);

                    Grid.SetRow(tmv, 0);
                    Grid.SetColumn(tmv, 0);
                    Grid.SetRowSpan(tmv, 2);

                    tmv.Margin = new Thickness(resizer_short_side, 0, 0, 0);
                }

                tmv_lsp.Orientation = Orientation.Vertical;
                tmv_lsp.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_lsp.VerticalAlignment = VerticalAlignment.Top;

                tmv_wohb.VerticalAlignment = VerticalAlignment.Center;
                tmv_wohb.HorizontalAlignment = HorizontalAlignment.Stretch;
                if (tmv_wohb.RenderTransform is RotateTransform rott) {
                    rott.Angle = 90;
                }

                tmv_rsp.Orientation = Orientation.Vertical;
                tmv_rsp.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_rsp.VerticalAlignment = VerticalAlignment.Bottom;

                tmv_zfv.Width = tmvm.DefaultTitleMenuFixedLength;
                tmv_zfv.Height = tmvm.ZoomSliderLength;

                tmv_zoom_slider_track_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_track_b.Height = double.NaN;
                tmv_zoom_slider_track_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_track_b.VerticalAlignment = VerticalAlignment.Stretch;

                tmv_zoom_slider_min_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_min_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_min_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_min_b.VerticalAlignment = VerticalAlignment.Top;

                tmv_zoom_slider_def_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.25;
                tmv_zoom_slider_def_b.Height = tmvm.ZoomSliderLineWidth;

                tmv_zoom_slider_max_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.5;
                ;
                tmv_zoom_slider_max_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_max_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_max_b.VerticalAlignment = VerticalAlignment.Bottom;

                tmv_zoom_slider_val_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.5;
                ;
                tmv_zoom_slider_val_b.Height = tmvm.ZoomSliderValueLength;

                tmv_gltb.Margin = new Thickness(0, 10, 0, 0);
                tmv_min_btn.Margin = new Thickness(0, 0, 0, 0);
            }

            tmv_zfv.UpdateMarkerPositions();
        }

        private void UpdateSidebarGridsplitter() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            var mwtmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");
            //var sbcv = this.FindControl<MpAvSidebarContainerView>("SidebarContainerView");
            var ssbcb = this.FindControl<Border>("SelectedSidebarContainerBorder");
            var sbbg = this.FindControl<MpAvSidebarButtonGroupView>("SidebarButtonGroup");
            var ctrcb = this.FindControl<Border>("ClipTrayContainerBorder");
            var ctrcv = this.FindControl<MpAvClipTrayContainerView>("ClipTrayContainerView");
            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");

            var ssbivm = MpAvSidebarItemCollectionViewModel.Instance.SelectedItem;

            bool is_opening = ssbivm != null;
            bool is_closing = BindingContext.IsHorizontalOrientation ? ssbcb.Bounds.Width > 0 : ssbcb.Bounds.Height > 0;
            double mw_w = mwvm.MainWindowWidth;
            double mw_h = mwvm.MainWindowHeight;

            double nsbi_w, nsbi_h;
            double nctrcb_w, nctrcb_h;
            double avail_w, avail_h;

            // get new sidebar size and calculate new cliptray size from sidebar size
            if (BindingContext.IsHorizontalOrientation) {
                avail_w = mw_w - sbicvm.ButtonGroupFixedDimensionLength;
                avail_h = mwtg.Bounds.Height;

                nsbi_w = is_opening ? ssbivm.DefaultSidebarWidth : 0;
                nsbi_h = mwtg.Bounds.Height;

                nctrcb_w = mw_w - sbicvm.ButtonGroupFixedDimensionLength - nsbi_w;
                nctrcb_h = mwtg.Bounds.Height;
            } else {
                avail_w = mwtg.Bounds.Width;
                avail_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength; //  - fmvm.FilterMenuHeight;

                nsbi_w = mwtg.Bounds.Width;
                nsbi_h = is_opening ? ssbivm.DefaultSidebarHeight : 0;

                nctrcb_w = mwtg.Bounds.Width;
                nctrcb_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength; // - fmvm.FilterMenuHeight; // - nsbi_h
            }

            nsbi_w = Math.Max(0, nsbi_w);
            nsbi_h = Math.Max(0, nsbi_h);
            nctrcb_w = Math.Max(0, nctrcb_w);
            nctrcb_h = Math.Max(0, nctrcb_h);

            ssbcb.IsVisible = true;
            sbgs.IsVisible = is_opening && ssbivm.CanResize;

            bool skip_anim =
#if DESKTOP
                true; // mwvm.IsMainWindowOrientationDragging;
#else
                true;
#endif

            if (skip_anim) {
                // skip animate on orientation change because current values maybe transitioning 
                // otherwise random flickering or target size is wrong
                sbicvm.ContainerBoundWidth = nsbi_w;
                sbicvm.ContainerBoundHeight = nsbi_h;

                ctrvm.ContainerBoundWidth = nctrcb_w;
                ctrvm.ContainerBoundHeight = nctrcb_h;
                if (mwvm.IsMainWindowOrientationDragging) {
                    ClampContentSizes();
                } else {
                    UpdateClipTrayContainerSize(null);
                }

                return;
            }

            //sbicvm.ContainerBoundWidth = nsbi_w;
            //sbicvm.ContainerBoundHeight = nsbi_h;

            //ctrvm.ContainerBoundWidth = nctrcb_w;
            //ctrvm.ContainerBoundHeight = nctrcb_h;
            //UpdateClipTrayContainerSize(null);

            sbicvm.AnimateSize(new MpSize(nsbi_w, nsbi_h));
            ctrvm.AnimateSize(
                new MpSize(nctrcb_w, nctrcb_h),
                () => {
                    // onComplete handler
                    UpdateClipTrayContainerSize(null);
                    return true;
                });
        }

        private void UpdateClipTrayContainerSize(MpPoint splitter_delta) {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            var mwtmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            if (mwvm.IsMainWindowOrientationDragging) {
                return;
            }

            if (mwvm.IsVerticalOrientation &&
                splitter_delta != null) {
                // BUG grid splitter doesn't drag in vertical automatically, must manually adjust
                sbicvm.ContainerBoundHeight -= splitter_delta.Y;
            }

            double mw_w = mwvm.MainWindowWidth;
            double mw_h = mwvm.MainWindowHeight;

            if (BindingContext.IsHorizontalOrientation) {
                ctrvm.ContainerBoundWidth = Math.Max(0,
                    mw_w - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.ContainerBoundWidth);
            } else {
                ctrvm.ContainerBoundHeight = Math.Max(0,
                    mw_h - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.ContainerBoundHeight -
                    fmvm.FilterMenuHeight);
            }

            ClampContentSizes();
        }

        public void ClampContentSizes() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            var mwtmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            double tw = ctrvm.ContainerBoundWidth + sbicvm.ContainerBoundWidth;
            double th = ctrvm.ContainerBoundHeight + sbicvm.ContainerBoundHeight;

            double w_diff = tw - mwvm.AvailableContentAndSidebarWidth;
            if (w_diff > 0) {
                // too big
                //double nctr_w = Math.Max(0, ctrvm.ContainerBoundWidth - (w_diff / 2));
                //double nsbic_w = Math.Max(0, sbicvm.ContainerBoundWidth - (w_diff / 2));
                //ctrvm.ContainerBoundWidth = nctr_w;
                //sbicvm.ContainerBoundWidth = nsbic_w;
                //if (ctrvm.ContainerBoundWidth <= 0) {
                //    // NOTE this only clamps when it becomes 0, which seems to be in 
                //    // orientation change w/ sidebar visible
                //    ctrvm.ContainerBoundWidth = MIN_TRAY_VAR_DIM_LENGTH;
                //    sbicvm.ContainerBoundWidth = tw - MIN_TRAY_VAR_DIM_LENGTH;
                //}

                ctrvm.ContainerBoundWidth -= Math.Abs(w_diff);
            } else if (w_diff < 0) {
                // too small
                ctrvm.ContainerBoundWidth += Math.Abs(w_diff);
            }

            double h_diff = th - mwvm.AvailableContentAndSidebarHeight;
            if (h_diff > 0) {
                // too big
                double nctr_h = Math.Max(0, ctrvm.ContainerBoundHeight - (h_diff / 2));
                double nsbic_h = Math.Max(0, sbicvm.ContainerBoundHeight - (h_diff / 2));
                ctrvm.ContainerBoundHeight = nctr_h;
                sbicvm.ContainerBoundHeight = nsbic_h;

                if (ctrvm.ContainerBoundHeight <= 0) {
                    // NOTE this only clamps when it becomes 0, which seems to be in 
                    // orientation change w/ sidebar visible
                    ctrvm.ContainerBoundHeight = MIN_TRAY_VAR_DIM_LENGTH;
                    sbicvm.ContainerBoundHeight = th - MIN_TRAY_VAR_DIM_LENGTH;
                }
                //sbicvm.ContainerBoundHeight -= Math.Abs(h_diff);
            } else if (h_diff < 0) {
                // too small
                sbicvm.ContainerBoundHeight += Math.Abs(h_diff);
            }
        }

        #endregion

        #endregion

        #region Protected Overrides

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.PinTraySizeChanged:
                case MpMessageType.PinTrayResizeEnd:
                case MpMessageType.SidebarItemSizeChanged:
                case MpMessageType.SidebarItemSizeChangeEnd:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    if (msg == MpMessageType.MainWindowOrientationChangeEnd) {
                        if (this.GetVisualDescendant<MpAvZoomFactorView>() is { } zfv) {
                            zfv.UpdateMarkerPositions();
                        }
                    }

                    Dispatcher.UIThread.Post(async () => {
                        await Task.Delay(300);
                        MpAvMainView.Instance.ClampContentSizes();
                    });
                    break;
            }
        }


        #region Event Handlers

        private void SidebarSplitter_DragDelta(object sender, VectorEventArgs e) {
            //if (!e.Vector.X.IsNumber() || !e.Vector.Y.IsNumber()) {
            //    MpDebug.Break();
            //}
            //UpdateClipTrayContainerSize(e.Vector.ToPortablePoint());
            ClampContentSizes();
        }

        #endregion

        #endregion
    }
}