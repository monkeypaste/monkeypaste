using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Win32;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvMainView :
        MpAvUserControl<MpAvMainWindowViewModel>,
        MpAvIResizableControl,
        MpIMainView,
        MpIAsyncObject {
        #region Private Variables
        #endregion

        #region Constants

        public const double MIN_TRAY_VAR_DIM_LENGTH = 150.0d;

        #endregion

        #region Statics
        private static MpAvMainView _instance;
        public static MpAvMainView Instance => _instance;
        public static async Task Init() {
            await Task.Delay(1);
            if (Mp.Services.PlatformInfo.IsDesktop) {
                var mw = new MpAvMainWindow();

                if (mw.Content is MpAvMainView mv) {
                    _instance = mv;
                }
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    var loader_window = desktop.MainWindow;
                    desktop.MainWindow = mw;
                    if (loader_window != null) {
                        loader_window.Close();
                    }
                }
                //MpAvWindowManager.MainWindow = mw;
                if (MpAvWindowManager.MainWindow == null) {
                    // huh?
                    MpDebug.Break();
                } else {
                    if (!MpPrefViewModel.Instance.ShowInTaskSwitcher) {
                        if (OperatingSystem.IsWindows()) {
                            MpAvToolWindow_Win32.InitToolWindow(MpAvWindowManager.MainWindow.PlatformImpl.Handle.Handle);
                        } else {
                            // TODO or error, not sure if mac/linux supports
                        }
                    }

                }

                //mw.DataContext = MpAvMainWindowViewModel.Instance;
            } else {
                if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime lifetime) {
                    _instance = lifetime.MainView as MpAvMainView;
                    //lifetime.MainView = _instance;
                    //await MpAvMainWindowViewModel.Instance.InitializeAsync();
                }
            }
            //DataContext = MpAvMainWindowViewModel.Instance;
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

        #region MpIMainView Implementation

        public bool IsActive =>
            BindingContext.IsMainWindowActive;
        public nint Handle {
            get {
                //if(this.GetVisualRoot() is TopLevel tl &&
                //    tl.PlatformImpl != null &&
                //    tl.PlatformImpl.)
                //((WindowImpl)((TopLevel)this.GetVisualRoot()).PlatformImpl).Handle.Handle;
                return 0;
            }
        }

        public void Show() {
            //throw new NotImplementedException();
        }

        public void Hide() {
            //throw new NotImplementedException();
        }

        public void SetPosition(MpPoint p, double scale) {
            //throw new NotImplementedException();
        }

        #endregion
        #endregion

        #region Properties
        public Grid RootGrid { get; }

        #endregion

        #region Constructors

        public MpAvMainView() {
            if (_instance != null) {
                MpDebug.Break("Duplicat singleton");
                return;
            }
            AvaloniaXamlLoader.Load(this);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
#if !DESKTOP
            //Background = Brushes.Lime;
#endif
            RootGrid = this.FindControl<Grid>("MainWindowContainerGrid");

            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
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
            var ctrcv_gs = ctrcv.FindControl<GridSplitter>("ClipTraySplitter");
            var ctrcv_ctrv = ctrcv.FindControl<MpAvQueryTrayView>("ClipTrayView");
            var ctrcv_ctrv_cg = ctrcv_ctrv.FindControl<Grid>("QueryTrayContainerGrid");


            mwtg.RowDefinitions.Clear();
            mwtg.ColumnDefinitions.Clear();
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
                sbgs.Width = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
                sbgs.HorizontalAlignment = HorizontalAlignment.Right;
                sbgs.ResizeDirection = GridResizeDirection.Columns;

                // cliptray container border
                Grid.SetRow(ctrcb, 1);
                Grid.SetColumn(ctrcb, 2);


                // cliptraycontainer column definitions (horizontal)
                ctrcv_cg.RowDefinitions.Clear();

                // pintray column definition
                var ptrv_cd = new ColumnDefinition(new GridLength(0, GridUnitType.Auto));
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
                var ctrv_cd = new ColumnDefinition(new GridLength(1, GridUnitType.Star));
                ctrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenWidth)
                    });

                ctrcv_cg.ColumnDefinitions = new ColumnDefinitions() {
                     ptrv_cd,
                     ctrv_cd
                };

                // pin tray listbox padding (horizontal) for head/tail drop adorners
                if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                    ctrcv_ptr_lb.Padding = new Thickness(10, 0, 10, 0);
                } else {
                    ctrcv_ptr_lb.Padding = new Thickness();
                }
                // add margin for grid splitter size so boxshadow is symmetrical
                ctrcv_ptrv.Margin = new Thickness(0, 0, 5, 0);

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Right;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Stretch;
                ctrcv_gs.Width = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
                ctrcv_gs.Height = double.NaN;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Columns;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeWestEast);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 0);
                Grid.SetColumn(ctrcv_ctrv, 1);
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
                    new Binding() {
                        Source = ctrvm,
                        Path = nameof(ctrvm.MaxContainerScreenHeight)
                    });

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
                sbgs.Height = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
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
                var ptrv_rd = new RowDefinition(new GridLength(0, GridUnitType.Auto));
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
                var ctrv_rd = new RowDefinition(new GridLength(1, GridUnitType.Star));
                ctrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenHeight)
                    });

                ctrcv_cg.RowDefinitions = new RowDefinitions() {
                     ptrv_rd,
                     ctrv_rd
                };

                if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                    ctrcv_ptr_lb.Padding = new Thickness(10, 10, 10, 10);
                } else {
                    ctrcv_ptr_lb.Padding = new Thickness();
                }

                // add margin for grid splitter size so boxshadow is symmetrical
                ctrcv_ptrv.Margin = new Thickness(0, 0, 0, 5);

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Stretch;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Bottom;
                ctrcv_gs.Width = double.NaN;
                ctrcv_gs.Height = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Rows;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 1);
                Grid.SetColumn(ctrcv_ctrv, 0);
            }

            UpdateSidebarGridsplitter();
            UpdateTitleLayout();
            //UpdateResizerLayout();
            mwtg.InvalidateAll();
        }

        private void UpdateTitleLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var tmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            var mwcg = this.FindControl<Grid>("MainWindowContainerGrid");
            var tmv = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
            var fmv = this.FindControl<MpAvFilterMenuView>("FilterMenuView");
            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");

            var tmv_cg = tmv.FindControl<Grid>("TitlePanel");
            var tmv_lsp = tmv.FindControl<StackPanel>("LeftStackPanel");
            var tmv_wohb = tmv.FindControl<Button>("WindowOrientationHandleButton");
            var tmv_rsp = tmv.FindControl<StackPanel>("RightStackPanel");
            var tmv_gltb = tmv.FindControl<Control>("GridLayoutToggleButton");

            var tmv_zoom_slider_cg = tmv.FindControl<Grid>("ZoomSliderContainerGrid");
            var tmv_zoom_slider_track_b = tmv.FindControl<Border>("ZoomTrackLine");
            var tmv_zoom_slider_min_b = tmv.FindControl<Border>("ZoomMinLine");
            var tmv_zoom_slider_max_b = tmv.FindControl<Border>("ZoomMaxLine");
            var tmv_zoom_slider_val_btn = tmv.FindControl<Button>("CurZoomFactorButton");

            double resizer_short_side = 0;

            mwcg.ColumnDefinitions.Clear();
            mwcg.RowDefinitions.Clear();
            if (mwvm.IsHorizontalOrientation) {
                // HORIZONTAL
                tmvm.TitleMenuWidth = mwvm.MainWindowWidth;
                tmvm.TitleMenuHeight = tmvm.DefaultTitleMenuFixedLength;
                var tmv_rd = new RowDefinition(Math.Max(0, tmvm.TitleMenuHeight), GridUnitType.Pixel);

                fmvm.FilterMenuWidth = mwvm.MainWindowWidth;
                fmvm.FilterMenuHeight = fmvm.DefaultFilterMenuFixedSize;
                var fmv_rd = new RowDefinition(Math.Max(0, fmvm.FilterMenuHeight), GridUnitType.Pixel);


                if (mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Top) {
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

                tmv_zoom_slider_cg.Width = tmvm.ZoomSliderLength;
                tmv_zoom_slider_cg.Height = tmvm.DefaultTitleMenuFixedLength;

                tmv_zoom_slider_track_b.Width = double.NaN;
                tmv_zoom_slider_track_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_track_b.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_zoom_slider_track_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_min_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_min_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_min_b.HorizontalAlignment = HorizontalAlignment.Left;
                tmv_zoom_slider_min_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_max_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_max_b.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_max_b.HorizontalAlignment = HorizontalAlignment.Right;
                tmv_zoom_slider_max_b.VerticalAlignment = VerticalAlignment.Center;

                tmv_zoom_slider_val_btn.Width = tmvm.ZoomSliderValueLength;
                tmv_zoom_slider_val_btn.Height = tmvm.DefaultTitleMenuFixedLength * 0.5;

                tmv_gltb.Margin = new Thickness(10, 0, 0, 0);
                //tmv_zoom_slider_val_btn.HorizontalAlignment = HorizontalAlignment.Center;
                //tmv_zoom_slider_val_btn.VerticalAlignment = VerticalAlignment.Stretch;
            } else {
                // VERTICAL

                tmvm.TitleMenuWidth = tmvm.DefaultTitleMenuFixedLength;
                tmvm.TitleMenuHeight = mwvm.MainWindowHeight;
                var tv_cd = new ColumnDefinition(Math.Max(0, tmvm.TitleMenuWidth), GridUnitType.Pixel);

                fmvm.FilterMenuWidth = mwvm.MainWindowWidth - tmvm.TitleMenuWidth;
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

                tmv_zoom_slider_cg.Width = tmvm.DefaultTitleMenuFixedLength;
                tmv_zoom_slider_cg.Height = tmvm.ZoomSliderLength;

                tmv_zoom_slider_track_b.Width = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_track_b.Height = double.NaN;
                tmv_zoom_slider_track_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_track_b.VerticalAlignment = VerticalAlignment.Stretch;

                tmv_zoom_slider_min_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.5;
                tmv_zoom_slider_min_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_min_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_min_b.VerticalAlignment = VerticalAlignment.Top;

                tmv_zoom_slider_max_b.Width = tmvm.DefaultTitleMenuFixedLength * 0.5; ;
                tmv_zoom_slider_max_b.Height = tmvm.ZoomSliderLineWidth;
                tmv_zoom_slider_max_b.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider_max_b.VerticalAlignment = VerticalAlignment.Bottom;

                tmv_zoom_slider_val_btn.Width = tmvm.DefaultTitleMenuFixedLength * 0.5; ;
                tmv_zoom_slider_val_btn.Height = tmvm.ZoomSliderValueLength;

                tmv_gltb.Margin = new Thickness(0, 10, 0, 0);
            }
            tmv.PositionZoomValueButton();
        }
        private void UpdateSidebarGridsplitter2() {
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
            if (ssbivm == null) {
                // closing
                if (BindingContext.IsHorizontalOrientation) {
                } else {
                    mwtg.RowDefinitions[0].Height = new GridLength(mwtg.Bounds.Height - sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel);
                    mwtg.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Auto);
                    mwtg.RowDefinitions[2].Height = new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel);
                    return;

                }
                return;
            } else {
                // opening
                if (BindingContext.IsHorizontalOrientation) {
                } else {
                    mwtg.RowDefinitions[0].Height = new GridLength(mwtg.Bounds.Height - ssbivm.DefaultSidebarHeight - sbbg.Bounds.Height, GridUnitType.Pixel);
                    mwtg.RowDefinitions[1].Height = new GridLength(ssbivm.DefaultSidebarHeight, GridUnitType.Auto);
                    mwtg.RowDefinitions[2].Height = new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel);
                    return;

                }
                return;
            }
        }
        private void UpdateSidebarGridsplitter() {
            //UpdateSidebarGridsplitter2();
            //return;
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
                avail_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength;//  - fmvm.FilterMenuHeight;

                nsbi_w = mwtg.Bounds.Width;
                nsbi_h = is_opening ? ssbivm.DefaultSidebarHeight : 0;

                nctrcb_w = mwtg.Bounds.Width;
                nctrcb_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength;// - fmvm.FilterMenuHeight; // - nsbi_h
            }
            nsbi_w = Math.Max(0, nsbi_w);
            nsbi_h = Math.Max(0, nsbi_h);
            nctrcb_w = Math.Max(0, nctrcb_w);
            nctrcb_h = Math.Max(0, nctrcb_h);

            ssbcb.IsVisible = true;
            sbgs.IsVisible = is_opening;

            if (mwvm.IsMainWindowOrientationDragging) {
                // skip animate on orientation change because current values maybe transitioning 
                // otherwise random flickering or target size is wrong
                sbicvm.ContainerBoundWidth = nsbi_w;
                sbicvm.ContainerBoundHeight = nsbi_h;

                ctrvm.ContainerBoundWidth = nctrcb_w;
                ctrvm.ContainerBoundHeight = nctrcb_h;
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
                ctrvm.ContainerBoundWidth = Math.Max(0, mw_w - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.ContainerBoundWidth);
            } else {
                ctrvm.ContainerBoundHeight = Math.Max(0, mw_h - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.ContainerBoundHeight - fmvm.FilterMenuHeight);
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

            if (tw > mwvm.AvailableContentAndSidebarWidth) {
                double w_diff = tw - mwvm.AvailableContentAndSidebarWidth;
                double nctr_w = Math.Max(0, ctrvm.ContainerBoundWidth - (w_diff / 2));
                double nsbic_w = Math.Max(0, sbicvm.ContainerBoundWidth - (w_diff / 2));
                ctrvm.ContainerBoundWidth = nctr_w;
                sbicvm.ContainerBoundWidth = nsbic_w;
                if (ctrvm.ContainerBoundWidth <= 0) {
                    // NOTE this only clamps when it becomes 0, which seems to be in 
                    // orientation change w/ sidebar visible
                    ctrvm.ContainerBoundWidth = MIN_TRAY_VAR_DIM_LENGTH;
                    sbicvm.ContainerBoundWidth = tw - MIN_TRAY_VAR_DIM_LENGTH;
                }
            }
            if (th > mwvm.AvailableContentAndSidebarHeight) {
                double h_diff = th - mwvm.AvailableContentAndSidebarHeight;
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
            }
        }
        #endregion

        #endregion

        #region Protected Overrides

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged: {
                        var mwvm = MpAvMainWindowViewModel.Instance;
                        if (mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Top) {
                            // can't figure out how to make resizer align to bottom so have to manually translate to bottom

                            //var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
                            //var resizerTransform = resizerView.RenderTransform as TranslateTransform;
                            //resizerTransform.Y = mwvm.MainWindowHeight - resizerView.Height;
                        }
                        break;
                    }
                case MpMessageType.MainWindowOrientationChangeEnd:
                    this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView").PositionZoomValueButton();
                    break;
            }
        }


        #region Event Handlers
        private void SidebarSplitter_DragDelta(object sender, VectorEventArgs e) {
            if (!e.Vector.X.IsNumber() || !e.Vector.Y.IsNumber()) {
                MpDebug.Break();
            }
            UpdateClipTrayContainerSize(e.Vector.ToPortablePoint());
        }

        #endregion

        #endregion
    }
}
