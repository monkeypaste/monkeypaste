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
    public enum MpMainViewUpdateType {
        None = 0,
        SidebarOpen,
        SidebarClose,
        //Animating
    }
    [DoNotNotify]
    public partial class MpAvMainView :
#if WINDOWED
        MpAvWindow<MpAvMainWindowViewModel>,
#else
        MpAvUserControl<MpAvMainWindowViewModel>,
#endif
        MpAvIResizableControl,
        MpAvIFocusHeaderMenuView,
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
                DataContext = mwvm
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

        

        MpSize BoundContentAndSidebarSize {
            get {
                var ctrvm = MpAvClipTrayViewModel.Instance;
                var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
                double bound_w = 0;
                double bound_h = 0;
                //if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                    if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        bound_w = ctrvm.ContainerBoundWidth + sbicvm.ContainerBoundWidth;
                        bound_h = Math.Max(ctrvm.ContainerBoundHeight, sbicvm.ContainerBoundHeight);
                    } else {
                        bound_w = Math.Max(ctrvm.ContainerBoundWidth, sbicvm.ContainerBoundWidth);
                        bound_h = ctrvm.ContainerBoundHeight + sbicvm.ContainerBoundHeight;
                    }
                //} else {
                //    // only use clip cntr on mobile since it spans into sidebar cell
                //    bound_w =  ctrvm.ContainerBoundWidth;
                //    bound_h = ctrvm.ContainerBoundHeight;
                //}
                
                return new MpSize(Math.Max(0,bound_w), Math.Max(0,bound_h));
            }
        }
        MpSize ActualContentAndSidebarSize {
            get {
                double actual_w = 0;
                double actual_h = 0;
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    actual_w = MainWindowTrayGrid.Bounds.Width -
                                SidebarButtonGroup.Bounds.Width;
                    actual_h = MainWindowTrayGrid.Bounds.Height;
                } else {
                    actual_w = MainWindowTrayGrid.Bounds.Width;
                    actual_h = MainWindowTrayGrid.Bounds.Height -
                                SidebarButtonGroup.Bounds.Height;
                }
                return new MpSize(Math.Max(0,actual_w), Math.Max(0,actual_h));
            }
        }
        public MpSize AvailableContentAndSidebarSize {
            get {
                // NOTE the only difference between mobile/multi-window are:
                // 1. In Multi-Window mode the title is always on the inside edge (for resize and drag)
                // 2. In Multi-window mode the sidebar is in its own grid cell.
                //    On mobile the clip cntr view spans both cells so sidebars float over clip cntr

                double avail_w = 0;
                double avail_h = 0;
                if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                    if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        avail_w = MainWindowContainerGrid.Bounds.Width -
                                    SidebarButtonGroup.Bounds.Width;
                        avail_h = MainWindowContainerGrid.Bounds.Height -
                                    MainWindowTitleView.Bounds.Height -
                                    FilterMenuView.Bounds.Height;
                    } else {
                        avail_w = MainWindowContainerGrid.Bounds.Width -
                                    MainWindowTitleView.Bounds.Width;
                        avail_h = MainWindowContainerGrid.Bounds.Height -
                                    FilterMenuView.Bounds.Height -
                                    SidebarButtonGroup.Bounds.Height;
                    }
                } else {
                    if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        avail_w = MainWindowContainerGrid.Bounds.Width -
                                    SidebarButtonGroup.Bounds.Width;
                        avail_h = MainWindowContainerGrid.Bounds.Height -
                                    MainWindowTitleView.Bounds.Height -
                                    FilterMenuView.Bounds.Height;
                    } else {
                        avail_w = MainWindowContainerGrid.Bounds.Width;
                        avail_h = MainWindowContainerGrid.Bounds.Height -
                                    MainWindowTitleView.Bounds.Height -
                                    FilterMenuView.Bounds.Height -
                                    SidebarButtonGroup.Bounds.Height;
                    }
                }
                return new MpSize(Math.Max(0,avail_w), Math.Max(0,avail_h));
            }
        }

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
            
            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
            //sidebarSplitter.DragStarted += (s, e) => MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChangeBegin);
            //sidebarSplitter.DragStarted += (s, e) => MpMessenger.SendGlobal(MpMessageType.SidebarItemSizeChangeEnd);
            sidebarSplitter.DragDelta += SidebarSplitter_DragDelta;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        #region Orientation Updates

        public void UpdateMainViewLayout() {
            UpdateContentLayout();
            UpdateContainerLayout();
            UpdateEdgyTooltips();
            UpdateGridBindings(); 

            
        }

        private void UpdateContentLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            //var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
            var mwtb = this.MainWindowTrayPanel;
            var mwtg = this.MainWindowTrayGrid;

            var sbbg = this.SidebarButtonGroup;
            var ssbcb = this.SelectedSidebarContainerBorder;
            var sbgs = this.SidebarGridSplitter;

            var ctrcv = this.ClipTrayContainerView;
            var ctrcv_cg = ctrcv.ClipTrayContainerGrid;
            var ctrcv_ptrv = ctrcv.PinTrayView;
            var ctrcv_ptr_lb = ctrcv_ptrv.PinTrayListBox;
            var ctrcv_ptr_cg = ctrcv_ptrv.PinTrayContainerGrid;
            var ctrcv_gs = ctrcv.ClipTraySplitter;
            var ctrcv_ctrv = ctrcv.QueryTrayView;
            var ctrcv_ctrv_cg = ctrcv_ctrv.QueryTrayContainerGrid;
            var ctrcv_ctr_lb = ctrcv_ctrv.QueryTrayListBox;

            var pin_tray_ratio = MpAvClipTrayViewModel.Instance.GetCurrentPinTrayRatio();
            mwtg.RowDefinitions.Clear();
            mwtg.ColumnDefinitions.Clear();
            double gs_fixed_length =
#if MULTI_WINDOW
                MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength; 
#else
                MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength; //0;
#endif
            double dbl_gs_fixed_length = gs_fixed_length * 2;

            void SetupHorizontal() {
                mwtg.RowDefinitions.Add(new RowDefinition(new GridLength(0, GridUnitType.Auto)));
                mwtg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

                var sbbg_cd = new ColumnDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));

                var ssbcb_cd = new ColumnDefinition(0, GridUnitType.Auto);
                var ctrcb_cd = new ColumnDefinition(1, GridUnitType.Star);

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
                Grid.SetRow(ctrcv, 1);
                Grid.SetColumn(ctrcv, 2);

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
                double ratio = MpAvThemeViewModel.Instance.IsMultiWindow ?
                    1 - pin_tray_ratio.Width : 1;
                var ctrv_cd = new ColumnDefinition(new GridLength(ratio, GridUnitType.Star));
                ctrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinQueryTrayScreenWidth)
                    });
                ctrv_cd.Bind(
                    ColumnDefinition.MaxWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MaxQueryTrayScreenWidth)
                    });

                ctrcv_cg.ColumnDefinitions = new ColumnDefinitions() { ptrv_cd, ctrv_cd };

                // pin tray listbox padding (horizontal) for head/tail drop adorners
                //if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                //    ctrcv_ptr_lb.Padding = new Thickness(dbl_gs_fixed_length, 0, dbl_gs_fixed_length, 0);
                //} else {
                //    ctrcv_ptr_lb.Padding = new Thickness();
                //}

                //// add margin for grid splitter size so boxshadow is symmetrical
                //ctrcv_ptrv.Margin = new Thickness(0, 0, gs_fixed_length, 0);

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
            }

            void SetupVertical() {
                var ctrcb_rd = new RowDefinition(1, GridUnitType.Star);
                var ssbcb_rd = new RowDefinition(0, GridUnitType.Auto);

                var sbbg_rd = new RowDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));
                sbbg_rd.MinHeight = sbicvm.ButtonGroupFixedDimensionLength;

                mwtg.RowDefinitions.Add(ctrcb_rd);
                mwtg.RowDefinitions.Add(ssbcb_rd);
                mwtg.RowDefinitions.Add(sbbg_rd);

                // cliptray container view
                Grid.SetRow(ctrcv, 0);
                Grid.SetColumn(ctrcv, 0);

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
                double ratio = MpAvThemeViewModel.Instance.IsMultiWindow ?
                    1 - pin_tray_ratio.Height : 1;
                var ctrv_rd = new RowDefinition(new GridLength(ratio, GridUnitType.Star));
                ctrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinQueryTrayScreenHeight)
                    });

                ctrcv_cg.RowDefinitions = new RowDefinitions() { ptrv_rd, ctrv_rd };

                //if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                //    ctrcv_ptr_lb.Padding = new Thickness(dbl_gs_fixed_length);
                //} else {
                //    ctrcv_ptr_lb.Padding = new Thickness();
                //}

                //// add margin for grid splitter size so boxshadow is symmetrical
                //ctrcv_ptrv.Margin = new Thickness(0, 0, 0, gs_fixed_length);

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

            if (mwvm.IsHorizontalOrientation) {
                SetupHorizontal();
            } else {
                SetupVertical();
            }
        }
        private void UpdateContainerLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var tmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            var mwcg = this.MainWindowContainerGrid;
            var tmv = this.MainWindowTitleView;
            var fmv = this.FilterMenuView;
            var ttrv = fmv.TagTrayView;
            var mwtp = this.MainWindowTrayPanel;
            var mwtg = this.MainWindowTrayGrid;

            var tmv_cg = tmv.TitlePanel;
            var tmv_lsp = tmv.LeftStackPanel;
            var tmv_wohb = tmv.WindowOrientationHandleButton;
            var tmv_rsp = tmv.RightStackPanel;
            var tmv_gltb = tmv.GridLayoutToggleButton;
            var tmv_min_btn = tmv.MinimizeMainWindowButton;

            var tmv_zfv = tmv.ZoomFactorView;
            var tmv_zoom_slider_track_b = tmv_zfv.ZoomTrackLine;
            var tmv_zoom_slider_min_b = tmv_zfv.ZoomMinLine;
            var tmv_zoom_slider_def_b = tmv_zfv.ZoomDefaultLine;
            var tmv_zoom_slider_max_b = tmv_zfv.ZoomMaxLine;
            var tmv_zoom_slider_val_b = tmv_zfv.CurValLine;

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
            void DoHorizontal() {
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
                    Grid.SetRow(mwtp, 1);
                    Grid.SetRow(tmv, 2);

                    tmv.Margin = new Thickness(0, 0, 0, resizer_short_side);
                } else {
                    // BOTTOM
                    mwcg.RowDefinitions.Add(tmv_rd);
                    mwcg.RowDefinitions.Add(fmv_rd);
                    mwcg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

                    Grid.SetRow(tmv, 0);
                    Grid.SetRow(fmv, 1);
                    Grid.SetRow(mwtp, 2);

                    tmv.Margin = new Thickness(0, resizer_short_side, 0, 0);
                }

                Grid.SetColumn(fmv, 0);
                Grid.SetColumn(mwtp, 0);
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
                //tmv_wohb.Width = MpAvMainWindowTitleMenuViewModel.Instance.TitleDragHandleLongLength;
                //tmv_wohb.Height = double.NaN;

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

                //tmv_gltb.Margin = new Thickness(10, 0, 0, 0);
                //tmv_min_btn.Margin = new Thickness(5, 0, 0, 0);
                //tmv_zoom_slider_val_btn.HorizontalAlignment = HorizontalAlignment.Center;
                //tmv_zoom_slider_val_btn.VerticalAlignment = VerticalAlignment.Stretch;
            }

            void DoVertical() {
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

                    Grid.SetRow(mwtp, 1);
                    Grid.SetColumn(mwtp, 0);

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

                    Grid.SetRow(mwtp, 1);
                    Grid.SetColumn(mwtp, 1);

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
                //tmv_wohb.Width = double.NaN;
                //tmv_wohb.Height = MpAvMainWindowTitleMenuViewModel.Instance.TitleDragHandleLongLength;

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

                //tmv_gltb.Margin = new Thickness(0, 10, 0, 0);
                //tmv_min_btn.Margin = new Thickness(0, 0, 0, 0);
            }

            if (is_horiz) {
                DoHorizontal();
            } else {
                DoVertical();
            }

            tmv_zfv.UpdateMarkerPositions();
        }

        private bool UpdateGridBindings() {
            // returns true if no clamping needed
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                ctrvm.ContainerBoundHeight = BoundContentAndSidebarSize.Height;
                sbicvm.ContainerBoundHeight = BoundContentAndSidebarSize.Height;
            } else {
                ctrvm.ContainerBoundWidth = BoundContentAndSidebarSize.Width;
                sbicvm.ContainerBoundWidth = BoundContentAndSidebarSize.Width;
            }
            var diff = AvailableContentAndSidebarSize - BoundContentAndSidebarSize;
            bool is_valid = diff.IsValueEqual(MpPoint.Zero,1);
            if (!is_valid) {
                // only clamp clip cntr since sidebar animates 
                MpConsole.WriteLine($"Invalid content size! Avail: {AvailableContentAndSidebarSize} Bound: {BoundContentAndSidebarSize} diff: {diff}", true);
               
                ctrvm.ContainerBoundWidth += diff.X;
                ctrvm.ContainerBoundHeight += diff.Y;
                var diff2 = AvailableContentAndSidebarSize - BoundContentAndSidebarSize;
                MpConsole.WriteLine($"Fixed diff: {diff2}", false, true);
                MpDebug.Assert(diff2.IsValueEqual(MpPoint.Zero), $"Clamp error. Bindings broken or something", silent: true);
            }
            return is_valid;
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
        #endregion

        #endregion

        #region Protected Overrides

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                return;
            }
            UpdateMainViewLayout();
        }
        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.FilterExpandedChanged:
                    Dispatcher.UIThread.Post(async () => {
                        // wait for animation to complete
                        UpdateMainViewLayout();
                        await Task.Delay(1_000);
                        UpdateMainViewLayout();
                        MpAvClipTrayViewModel.Instance.UpdateDefaultItemSize();

                        //UpdateGridBindings();
                    });
                    break;
                //case MpMessageType.SidebarItemSizeChanged:
                //case MpMessageType.SidebarItemSizeChangeEnd:
                case MpMessageType.PinTraySizeChanged:
                case MpMessageType.PinTrayResizeEnd:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    if (msg == MpMessageType.MainWindowOrientationChangeEnd) {
                        if (this.GetVisualDescendant<MpAvZoomFactorView>() is { } zfv) {
                            zfv.UpdateMarkerPositions();
                        }
                    }

                    Dispatcher.UIThread.Post(async () => {
                        await Task.Delay(300);
                        UpdateGridBindings();
                    });
                    break;
            }
        }


        #region Event Handlers

        private void SidebarSplitter_DragDelta(object sender, VectorEventArgs e) {
            double delta_x = e.Vector.X.IsNumber() ? e.Vector.X : 0;
            double delta_y = e.Vector.Y.IsNumber() ? e.Vector.Y : 0;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            //sbicvm.ContainerBoundWidth += delta_x;
            //sbicvm.ContainerBoundHeight += delta_y;
        }

        #endregion

        #endregion
    }
}