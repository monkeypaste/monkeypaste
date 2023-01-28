using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MonkeyPaste.Common;
using PropertyChanged;
using SharpHook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using System.Linq;
using System;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonoMac.CoreText;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : Window, MpAvIResizableControl { 
        #region Private Variables

        private int? _origResizerIdx;

        #endregion

        #region Constants
        
        public const double MIN_TRAY_VAR_DIM_LENGTH = 150.0d;

        #endregion

        #region Statics
        private static MpAvMainWindow _instance;
        public static MpAvMainWindow Instance => _instance ?? (_instance = new MpAvMainWindow());
        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x, y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }

        #endregion



        #region MpAvIResizableControl Implementation
        private Control _resizerControl;
        Control MpAvIResizableControl.ResizerControl {
            get {
                if (_resizerControl == null) {
                    var mwrv = this.GetVisualDescendant<MpAvMainWindowResizerView>();
                    _resizerControl = mwrv.FindControl<Control>("MainWindowResizeBorder");
                }
                return _resizerControl;
            }
        }
        #endregion

        #region Properties

        public MpAvMainWindowViewModel BindingContext => MpAvMainWindowViewModel.Instance;

        #endregion

        #region Constructors

        public MpAvMainWindow() {


            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;
            this.PointerMoved += MainWindow_PointerMoved;
            this.PointerLeave += MainWindow_PointerLeave;

            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
            sidebarSplitter.DragDelta += SidebarSplitter_DragDelta;
            
            var advSearchSplitter = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            //advSearchSplitter.DragDelta += AdvSearchSplitter_DragDelta;
            advSearchSplitter.DragCompleted += AdvSearchSplitter_DragCompleted;
        }




        #endregion

        #region Public Methods
        public async Task InitAsync() {
            await Task.Delay(1);
            App.Desktop.MainWindow = this;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
            if (OperatingSystem.IsWindows()) {
                MpAvToolWindow_Win32.InitToolWindow(this.PlatformImpl.Handle.Handle);
            }
            DataContext = MpAvMainWindowViewModel.Instance;
        }

        public void UpdateResizerLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;

            var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
            var resizerHandle = resizerView.FindControl<Border>("MainWindowResizeOuterBorder");
            var resizerTransform = resizerView.RenderTransform as TranslateTransform;

            double resizer_long_side = mwvm.IsHorizontalOrientation ? mwvm.MainWindowWidth : mwvm.MainWindowHeight;
            double resizer_short_side = mwvm.ResizerLength;

            resizerTransform.X = 0;
            resizerTransform.Y = 0;
            switch (mwvm.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Bottom:
                    resizerHandle.Width = resizer_long_side;
                    resizerHandle.Height = resizer_short_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Stretch;

                    resizerView.Width = resizer_long_side;
                    resizerView.Height = resizer_short_side;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    //resizerView.Background = Brushes.Transparent;
                    break;
                case MpMainWindowOrientationType.Top:
                    resizerHandle.Width = resizer_long_side;
                    resizerHandle.Height = resizer_short_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Center;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Stretch;

                    resizerView.Width = resizer_long_side;
                    resizerView.Height = resizer_short_side;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.Y = mwvm.MainWindowHeight - resizerView.Height;

                    //resizerView.Background = new SolidColorBrush() {
                    //    Color = Colors.White,
                    //    Opacity = 0.5
                    //};
                    break;
                case MpMainWindowOrientationType.Left:
                    resizerHandle.Width = resizer_short_side;
                    resizerHandle.Height = resizer_long_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Center;

                    resizerView.Width = resizer_short_side;
                    resizerView.Height = mwvm.MainWindowHeight;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Right;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.X = MpAvMainWindowTitleMenuViewModel.Instance.TitleMenuWidth;
                    //resizerView.Background = new SolidColorBrush() {
                    //    Color = Colors.White,
                    //    Opacity = 0.5
                    //};
                    break;
                case MpMainWindowOrientationType.Right:
                    resizerHandle.Width = resizer_short_side;
                    resizerHandle.Height = resizer_long_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Center;

                    resizerView.Width = resizer_short_side;
                    resizerView.Height = mwvm.MainWindowHeight;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Left;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;


                    //resizerView.Background = new SolidColorBrush() {
                    //    Color = Colors.White,
                    //    Opacity = 0.5
                    //};
                    break;
            }
        }
        public void UpdateTitleLayout() {

            var test = this.GetVisualDescendants<Control>().Where(x => x is IDescription).Cast<IDescription>().ToList();

            var mwvm = MpAvMainWindowViewModel.Instance;
            var tmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;

            var mwcg = this.FindControl<Grid>("MainWindowContainerGrid");
            var tmv = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
            var fmv = this.FindControl<MpAvFilterMenuView>("FilterMenuView");
            var sclbv_gs = this.FindControl<GridSplitter>("AdvancedSearchSplitter");
            var sclbv = this.FindControl<MpAvSearchCriteriaListBoxView>("SearchDetailView");
            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");

            var tmv_cg = tmv.FindControl<Grid>("TitlePanel");
            var tmv_lsp = tmv.FindControl<StackPanel>("LeftStackPanel");
            var tmv_wohb = tmv.FindControl<Button>("WindowOrientationHandleButton");
            var tmv_rsp = tmv.FindControl<StackPanel>("RightStackPanel");
            var tmv_zoom_slider_cg = tmv.FindControl<Grid>("ZoomSliderContainerGrid");
            var tmv_zoom_slider = tmv.FindControl<Slider>("ZoomFactorSlider");

            double resizer_short_side = 0;

            var sclbv_rd = new RowDefinition(Math.Max(0, scicvm.BoundCriteriaListBoxScreenHeight), GridUnitType.Pixel);
            sclbv_rd.Bind(
                RowDefinition.HeightProperty,
                new Binding() {
                    Source = scicvm,
                    Path = nameof(scicvm.BoundCriteriaListBoxScreenHeight),
                    Mode = BindingMode.TwoWay,
                    Converter = MpAvDoubleToGridLengthConverter.Instance
                });            
            sclbv_rd.Bind(
                RowDefinition.MaxHeightProperty,
                new Binding() {
                    Source = scicvm,
                    Path = nameof(scicvm.MaxSearchCriteriaListBoxHeight)
                });

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
                    mwcg.RowDefinitions.Add(sclbv_rd);
                    mwcg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
                    mwcg.RowDefinitions.Add(tmv_rd);

                    Grid.SetRow(fmv, 0);
                    Grid.SetRow(sclbv_gs, 1);
                    Grid.SetRow(sclbv, 1);
                    Grid.SetRow(mwtg, 2);
                    Grid.SetRow(tmv, 3);

                    tmv.Margin = new Thickness(0, 0, 0, resizer_short_side);
                } else {
                    // BOTTOM
                    mwcg.RowDefinitions.Add(tmv_rd);
                    mwcg.RowDefinitions.Add(fmv_rd);
                    mwcg.RowDefinitions.Add(sclbv_rd);
                    mwcg.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

                    Grid.SetRow(tmv, 0);
                    Grid.SetRow(fmv, 1);
                    Grid.SetRow(sclbv_gs, 2);
                    Grid.SetRow(sclbv, 2);
                    Grid.SetRow(mwtg, 3);

                    tmv.Margin = new Thickness(0, resizer_short_side, 0, 0);
                }
                Grid.SetColumn(fmv, 0);
                Grid.SetColumn(mwtg, 0);
                Grid.SetColumn(tmv, 0);
                Grid.SetRowSpan(tmv, 1);

                tmv_lsp.Orientation = Orientation.Horizontal;
                tmv_lsp.HorizontalAlignment = HorizontalAlignment.Left;
                tmv_lsp.VerticalAlignment = VerticalAlignment.Stretch;

                //tmv_wohb.Width = tmvm.TitleDragHandleLongLength;
                //tmv_wohb.Height = tmvm.TitleDragHandleShortLength;
                tmv_wohb.VerticalAlignment = VerticalAlignment.Stretch;
                tmv_wohb.HorizontalAlignment = HorizontalAlignment.Center;
                if(tmv_wohb.GetVisualDescendant<Image>() is Image tmv_wohb_img &&
                    tmv_wohb_img.RenderTransform is RotateTransform rott) {
                    rott.Angle = 0;
                }

                tmv_rsp.Orientation = Orientation.Horizontal;
                tmv_rsp.HorizontalAlignment = HorizontalAlignment.Right;
                tmv_rsp.VerticalAlignment = VerticalAlignment.Stretch;

                tmv_zoom_slider_cg.Width = 125;
                tmv_zoom_slider_cg.Height = tmv.Height;
                tmv_zoom_slider_cg.Margin = new Thickness(0, 0, 10, 0);

                tmv_zoom_slider.Width = 250;
                tmv_zoom_slider.Height = 40;
                tmv_zoom_slider.Margin = new Thickness(0, 0, 5, 14);
                tmv_zoom_slider.HorizontalAlignment = HorizontalAlignment.Right;
                tmv_zoom_slider.VerticalAlignment = VerticalAlignment.Top;
                tmv_zoom_slider.Orientation = Orientation.Horizontal;
            } else {
                // VERTICAL

                tmvm.TitleMenuWidth = tmvm.DefaultTitleMenuFixedLength;
                tmvm.TitleMenuHeight = mwvm.MainWindowHeight;
                var tv_cd = new ColumnDefinition(Math.Max(0, tmvm.TitleMenuWidth), GridUnitType.Pixel);

                fmvm.FilterMenuWidth = mwvm.MainWindowWidth - tmvm.TitleMenuWidth;
                fmvm.FilterMenuHeight = fmvm.DefaultFilterMenuFixedSize;
                var fv_rd = new RowDefinition(Math.Max(0, fmvm.FilterMenuHeight), GridUnitType.Pixel);

                mwcg.RowDefinitions.Add(fv_rd);
                mwcg.RowDefinitions.Add(sclbv_rd);
                mwcg.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

                if (mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Left) {
                    // LEFT
                    mwcg.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                    mwcg.ColumnDefinitions.Add(tv_cd);

                    Grid.SetRow(fmv, 0);
                    Grid.SetColumn(fmv, 0);

                    Grid.SetRow(sclbv, 1);
                    Grid.SetColumn(sclbv, 0);
                    Grid.SetRow(sclbv_gs, 1);
                    Grid.SetColumn(sclbv_gs, 0);

                    Grid.SetRow(mwtg, 2);
                    Grid.SetColumn(mwtg, 0);

                    Grid.SetRow(tmv, 0);
                    Grid.SetColumn(tmv, 1);
                    Grid.SetRowSpan(tmv, 3);

                    tmv.Margin = new Thickness(0, 0, resizer_short_side, 0);
                } else {
                    // RIGHT
                    mwcg.ColumnDefinitions.Add(tv_cd);
                    mwcg.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

                    Grid.SetRow(fmv, 0);
                    Grid.SetColumn(fmv, 1);

                    Grid.SetRow(sclbv, 1);
                    Grid.SetColumn(sclbv, 1);
                    Grid.SetRow(sclbv_gs, 1);
                    Grid.SetColumn(sclbv_gs, 1);

                    Grid.SetRow(mwtg, 2);
                    Grid.SetColumn(mwtg, 1);

                    Grid.SetRow(tmv, 0);
                    Grid.SetColumn(tmv, 0);
                    Grid.SetRowSpan(tmv, 3);

                    tmv.Margin = new Thickness(resizer_short_side, 0, 0, 0);
                }

                tmv_lsp.Orientation = Orientation.Vertical;
                tmv_lsp.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_lsp.VerticalAlignment = VerticalAlignment.Top;

                //tmv_wohb.Width = tmvm.TitleDragHandleShortLength;
                //tmv_wohb.Height = tmvm.TitleDragHandleLongLength;
                tmv_wohb.VerticalAlignment = VerticalAlignment.Center;
                tmv_wohb.HorizontalAlignment = HorizontalAlignment.Stretch;
                if (tmv_wohb.GetVisualDescendant<Image>() is Image tmv_wohb_img && 
                    tmv_wohb_img.RenderTransform is RotateTransform rott) {
                    rott.Angle = 90;
                }

                tmv_rsp.Orientation = Orientation.Vertical;
                tmv_rsp.HorizontalAlignment = HorizontalAlignment.Stretch;
                tmv_rsp.VerticalAlignment = VerticalAlignment.Bottom;

                tmv_zoom_slider_cg.Width = tmv.Width;
                tmv_zoom_slider_cg.Height = 125;
                tmv_zoom_slider_cg.Margin = new Thickness(0, 0, 0, 10);

                tmv_zoom_slider.Width = 40;
                tmv_zoom_slider.Height = 250;
                tmv_zoom_slider.Margin = new Thickness(0, 0, 5, 14);
                tmv_zoom_slider.HorizontalAlignment = HorizontalAlignment.Center;
                tmv_zoom_slider.VerticalAlignment = VerticalAlignment.Top;
                tmv_zoom_slider.Orientation = Orientation.Vertical;
            }
        }

        public void UpdateContentLayout() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;

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
            var ctrcv_ctrv = ctrcv.FindControl<MpAvClipTrayView>("ClipTrayView");

            mwtg.RowDefinitions.Clear();
            mwtg.ColumnDefinitions.Clear();
            if (mwvm.IsHorizontalOrientation) {
                // HORIZONTAL

                var sbbg_cd = new ColumnDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));

                var ssbcb_cd = new ColumnDefinition(Math.Max(0,sbicvm.BoundWidth), GridUnitType.Pixel);
                ssbcb_cd.Bind(
                    ColumnDefinition.WidthProperty,
                    new Binding() {
                        Source = sbicvm,
                        Path = nameof(sbicvm.BoundWidth),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                var ctrcb_cd = new ColumnDefinition(Math.Max(0,ctrvm.BoundWidth), GridUnitType.Pixel);
                ctrcb_cd.Bind(
                    ColumnDefinition.WidthProperty,
                    new Binding() {
                        Source = ctrvm,
                        Path = nameof(ctrvm.BoundWidth),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                mwtg.ColumnDefinitions = new ColumnDefinitions() {
                    sbbg_cd,
                     ssbcb_cd,
                     ctrcb_cd
                };

                // sidebar buttons
                Grid.SetRow(sbbg, 0);
                Grid.SetColumn(sbbg, 0);

                // sidebar content
                Grid.SetRow(ssbcb, 0);
                Grid.SetColumn(ssbcb, 1);
                // sidebar splitter
                Grid.SetRow(sbgs, 0);
                Grid.SetColumn(sbgs, 1);
                sbgs.Height = double.NaN;
                sbgs.VerticalAlignment = VerticalAlignment.Stretch;
                sbgs.Width = 5.0d;
                sbgs.HorizontalAlignment = HorizontalAlignment.Right;
                sbgs.ResizeDirection = GridResizeDirection.Columns;

                // cliptray container view
                Grid.SetRow(ctrcb, 0);
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
                if(MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                    ctrcv_ptr_lb.Padding = new Thickness(10, 0, 10, 0);
                } else {
                    ctrcv_ptr_lb.Padding = new Thickness();
                }

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Right;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Stretch;
                ctrcv_gs.Width = 5;
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

                var ctrcb_rd = new RowDefinition(Math.Max(0,ctrvm.BoundHeight), GridUnitType.Pixel);
                ctrcb_rd.Bind(
                    RowDefinition.HeightProperty,
                    new Binding() {
                        Source = ctrvm,
                        Path = nameof(ctrvm.BoundHeight),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                var ssbcb_rd = new RowDefinition(Math.Max(0,sbicvm.BoundHeight), GridUnitType.Pixel);
                ssbcb_rd.Bind(
                    RowDefinition.HeightProperty,
                    new Binding() {
                        Source = sbicvm,
                        Path = nameof(sbicvm.BoundHeight),
                        Mode = BindingMode.TwoWay,
                        Converter = MpAvDoubleToGridLengthConverter.Instance
                    });

                var sbbg_rd = new RowDefinition(
                    new GridLength(sbicvm.ButtonGroupFixedDimensionLength, GridUnitType.Pixel));



                mwtg.RowDefinitions = new RowDefinitions() {
                     ctrcb_rd,
                     ssbcb_rd,
                    sbbg_rd
                };

                // cliptray container view
                Grid.SetRow(ctrcb, 0);
                Grid.SetColumn(ctrcb, 0);

                // sidebar content
                Grid.SetRow(ssbcb, 1);
                Grid.SetColumn(ssbcb, 0);

                // sidebar buttons
                Grid.SetRow(sbbg, 2);
                Grid.SetColumn(sbbg, 0);

                // sidebar splitter
                Grid.SetRow(sbgs, 1);
                Grid.SetColumn(sbgs, 0);
                sbgs.Height = 5.0d;
                sbgs.Width = double.NaN;
                sbgs.VerticalAlignment = VerticalAlignment.Top;
                sbgs.HorizontalAlignment = HorizontalAlignment.Stretch;
                sbgs.ResizeDirection = GridResizeDirection.Rows;


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

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Stretch;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Bottom;
                ctrcv_gs.Width = double.NaN;
                ctrcv_gs.Height = 5;
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
            UpdateResizerLayout();
            mwtg.InvalidateMeasure();
        }

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

                            var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
                            var resizerTransform = resizerView.RenderTransform as TranslateTransform;
                            resizerTransform.Y = mwvm.MainWindowHeight - resizerView.Height;
                        }
                        break;
                    }
            }
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
            double nctrcb_w,nctrcb_h;
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
                avail_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength  - fmvm.FilterMenuHeight;

                nsbi_w = mwtg.Bounds.Width;
                nsbi_h = is_opening ? ssbivm.DefaultSidebarHeight : 0;

                nctrcb_w = mwtg.Bounds.Width;
                nctrcb_h = mw_h - sbicvm.ButtonGroupFixedDimensionLength - nsbi_h - fmvm.FilterMenuHeight;
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
                sbicvm.BoundWidth = nsbi_w;
                sbicvm.BoundHeight = nsbi_h;

                ctrvm.BoundWidth = nctrcb_w;
                ctrvm.BoundHeight = nctrcb_h;
                return;
            }

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

            if(mwvm.IsMainWindowOrientationDragging) {
               return;
            }

            if(mwvm.IsVerticalOrientation && splitter_delta != null) {
                // BUG grid splitter doesn't drag in vertical automatically, must manually adjust
                sbicvm.BoundHeight -= splitter_delta.Y;
            }

            double mw_w = mwvm.MainWindowWidth;
            double mw_h = mwvm.MainWindowHeight;

            if (BindingContext.IsHorizontalOrientation) {
                ctrvm.BoundWidth = Math.Max(0, mw_w - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.BoundWidth);
            } else {
                ctrvm.BoundHeight = Math.Max(0, mw_h - sbicvm.ButtonGroupFixedDimensionLength - sbicvm.BoundHeight - fmvm.FilterMenuHeight);
            }
            ClampContentSizes();
        }

        public void ClampContentSizes() {
            var mwvm = MpAvMainWindowViewModel.Instance;
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var sbicvm = MpAvSidebarItemCollectionViewModel.Instance;
            var mwtmvm = MpAvMainWindowTitleMenuViewModel.Instance;
            var fmvm = MpAvFilterMenuViewModel.Instance;

            double tw = ctrvm.BoundWidth + sbicvm.BoundWidth;
            double th = ctrvm.BoundHeight + sbicvm.BoundHeight;

            if(tw > mwvm.AvailableContentAndSidebarWidth) {
                double w_diff = tw - mwvm.AvailableContentAndSidebarWidth;
                double nctr_w = Math.Max(0,ctrvm.BoundWidth - (w_diff / 2));
                double nsbic_w = Math.Max(0,sbicvm.BoundWidth - (w_diff / 2));
                ctrvm.BoundWidth = nctr_w;
                sbicvm.BoundWidth = nsbic_w;
                if(ctrvm.BoundWidth <= 0) {
                    // NOTE this only clamps when it becomes 0, which seems to be in 
                    // orientation change w/ sidebar visible
                    ctrvm.BoundWidth = MIN_TRAY_VAR_DIM_LENGTH;
                    sbicvm.BoundWidth = tw - MIN_TRAY_VAR_DIM_LENGTH;
                }
            }
            if (th > mwvm.AvailableContentAndSidebarHeight) {
                double h_diff = th - mwvm.AvailableContentAndSidebarHeight;
                double nctr_h = Math.Max(0, ctrvm.BoundHeight - (h_diff / 2));
                double nsbic_h = Math.Max(0, sbicvm.BoundHeight - (h_diff / 2));
                ctrvm.BoundHeight = nctr_h;
                sbicvm.BoundHeight = nsbic_h;

                if (ctrvm.BoundHeight <= 0) {
                    // NOTE this only clamps when it becomes 0, which seems to be in 
                    // orientation change w/ sidebar visible
                    ctrvm.BoundHeight = MIN_TRAY_VAR_DIM_LENGTH;
                    sbicvm.BoundHeight = th - MIN_TRAY_VAR_DIM_LENGTH;
                }
            }
        }

        #region Event Handlers
        private void SidebarSplitter_DragDelta(object sender, VectorEventArgs e) {
            UpdateClipTrayContainerSize(e.Vector.ToPortablePoint());
        }

        private void AdvSearchSplitter_DragDelta(object sender, VectorEventArgs e) {
            MpAvSearchCriteriaItemCollectionViewModel.Instance
                .BoundCriteriaListBoxScreenHeight += e.Vector.ToPortablePoint().Y;
        }

        private void AdvSearchSplitter_DragCompleted(object sender, VectorEventArgs e) {
            MpAvSearchCriteriaItemCollectionViewModel.Instance
                .BoundCriteriaListBoxScreenHeight += e.Vector.ToPortablePoint().Y;
        }
        private void BoundsChangedHandler(AvaloniaPropertyChangedEventArgs<Rect> e) {
            var oldAndNewVals = e.GetOldAndNewValue<Rect>();
            MpAvMainWindowViewModel.Instance.LastMainWindowRect = oldAndNewVals.oldValue.ToPortableRect();
            MpAvMainWindowViewModel.Instance.ObservedMainWindowRect = oldAndNewVals.newValue.ToPortableRect();
        }


        private void MainWindow_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {            
            var mwvm = MpAvMainWindowViewModel.Instance;
            if (mwvm.IsResizing) {
                mwvm.IsResizerVisible = true;
            } else {
                var mw_mp = e.GetCurrentPoint(Parent).Position;
                var titleView = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
                if (titleView.Bounds.Contains(e.GetCurrentPoint(titleView.Parent).Position) &&
                    mwvm.MainWindowOrientationType != MpMainWindowOrientationType.Bottom) {
                    mwvm.IsResizerVisible = false;
                } else {
                    mwvm.IsResizerVisible = !this.Bounds.Deflate(mwvm.ResizerLength).Contains(mw_mp);
                }

                var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
                if (_origResizerIdx == null) {
                    _origResizerIdx = resizerView.ZIndex;
                }

                if (mwvm.IsResizerVisible) {
                    resizerView.ZIndex = 1000;
                } else {
                    resizerView.ZIndex = _origResizerIdx.Value;
                }

            }

        }
        private void MainWindow_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsResizerVisible = false;
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            //MpConsole.WriteLine("MainWindow ACTIVATED");
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            //MpConsole.WriteLine("MainWindow DEACTIVATED");
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
        }

        #endregion

        #endregion
    }
}
