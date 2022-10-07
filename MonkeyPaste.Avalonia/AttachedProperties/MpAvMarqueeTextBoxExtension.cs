using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMarqueeTextBoxExtension {
        static MpAvMarqueeTextBoxExtension() {
            IsEnabledProperty.Changed.AddClassHandler<TextBox>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region Canvas AvaloniaProperty
        public static Canvas GetCanvas(AvaloniaObject obj) {
            return obj.GetValue(CanvasProperty);
        }

        public static void SetCanvas(AvaloniaObject obj, Canvas value) {
            obj.SetValue(CanvasProperty, value);
        }

        public static readonly AttachedProperty<Canvas> CanvasProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, Canvas>(
                "Canvas",
                null);

        #endregion

        #region EditOnMouseClick AvaloniaProperty
        public static bool GetEditOnMouseClick(AvaloniaObject obj) {
            return obj.GetValue(EditOnMouseClickProperty);
        }

        public static void SetEditOnMouseClick(AvaloniaObject obj, bool value) {
            obj.SetValue(EditOnMouseClickProperty, value);
        }

        public static readonly AttachedProperty<bool> EditOnMouseClickProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "EditOnMouseClick",
                true);

        #endregion

        #region DropShadowOffset AvaloniaProperty
        public static Point GetDropShadowOffset(AvaloniaObject obj) {
            return obj.GetValue(DropShadowOffsetProperty);
        }

        public static void SetDropShadowOffset(AvaloniaObject obj, Point value) {
            obj.SetValue(DropShadowOffsetProperty, value);
        }

        public static readonly AttachedProperty<Point> DropShadowOffsetProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, Point>(
                "DropShadowOffset",
                new Point(1,1));

        #endregion

        #region IsReadOnly AvaloniaProperty
        public static bool GetIsReadOnly(AvaloniaObject obj) {
            return obj.GetValue(IsReadOnlyProperty);
        }

        public static void SetIsReadOnly(AvaloniaObject obj, bool value) {
            obj.SetValue(IsReadOnlyProperty, value);
        }

        public static readonly AttachedProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "IsReadOnly",
                true,false, BindingMode.TwoWay);

        #endregion

        #region ForegroundBrush AvaloniaProperty
        public static IBrush GetForegroundBrush(AvaloniaObject obj) {
            return obj.GetValue(ForegroundBrushProperty);
        }

        public static void SetForegroundBrush(AvaloniaObject obj, IBrush value) {
            obj.SetValue(ForegroundBrushProperty, value);
        }

        public static readonly AttachedProperty<IBrush> ForegroundBrushProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, IBrush>(
                "ForegroundBrush",
                Brushes.White);

        #endregion

        #region DropShadowBrush AvaloniaProperty
        public static IBrush GetDropShadowBrush(AvaloniaObject obj) {
            return obj.GetValue(DropShadowBrushProperty);
        }

        public static void SetDropShadowBrush(AvaloniaObject obj, IBrush value) {
            obj.SetValue(DropShadowBrushProperty, value);
        }

        public static readonly AttachedProperty<IBrush> DropShadowBrushProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, IBrush>(
                "DropShadowBrush",
                Brushes.Black);

        #endregion

        #region TailPadding AvaloniaProperty
        public static double GetTailPadding(AvaloniaObject obj) {
            return obj.GetValue(TailPaddingProperty);
        }

        public static void SetTailPadding(AvaloniaObject obj, double value) {
            obj.SetValue(TailPaddingProperty, value);
        }

        public static readonly AttachedProperty<double> TailPaddingProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, double>(
                "TailPadding",
                30.0d);

        #endregion

        #region MaxVelocity AvaloniaProperty
        public static double GetMaxVelocity(AvaloniaObject obj) {
            return obj.GetValue(MaxVelocityProperty);
        }

        public static void SetMaxVelocity(AvaloniaObject obj, double value) {
            obj.SetValue(MaxVelocityProperty, value);
        }

        public static readonly AttachedProperty<double> MaxVelocityProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, double>(
                "MaxVelocity",
                -5.0d);

        #endregion

        #region MaxPanelWidth AvaloniaProperty
        public static double GetMaxPanelWidth(AvaloniaObject obj) {
            return obj.GetValue(MaxPanelWidthProperty);
        }

        public static void SetMaxPanelWidth(AvaloniaObject obj, double value) {
            obj.SetValue(MaxPanelWidthProperty, value);
        }

        public static readonly AttachedProperty<double> MaxPanelWidthProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, double>(
                "MaxPanelWidth",
                default);

        #endregion

        #region Margin AvaloniaProperty
        public static Thickness GetMargin(AvaloniaObject obj) {
            return obj.GetValue(MarginProperty);
        }

        public static void SetMargin(AvaloniaObject obj, Thickness value) {
            obj.SetValue(MarginProperty, value);
        }

        public static readonly AttachedProperty<Thickness> MarginProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, Thickness>(
                "Margin",
                new Thickness());

        #endregion

        #region CancelEditCommand AvaloniaProperty
        public static ICommand GetCancelEditCommand(AvaloniaObject obj) {
            return obj.GetValue(CancelEditCommandProperty);
        }

        public static void SetCancelEditCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(CancelEditCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> CancelEditCommandProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, ICommand>(
                "CancelEditCommand",
                null);

        #endregion

        #region FinishEditCommand AvaloniaProperty
        public static ICommand GetFinishEditCommand(AvaloniaObject obj) {
            return obj.GetValue(FinishEditCommandProperty);
        }

        public static void SetFinishEditCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(FinishEditCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> FinishEditCommandProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, ICommand>(
                "FinishEditCommand",
                null);

        #endregion

        #region TotalLoopWaitMs AvaloniaProperty
        public static int GetTotalLoopWaitMs(AvaloniaObject obj) {
            return obj.GetValue(TotalLoopWaitMsProperty);
        }

        public static void SetTotalLoopWaitMs(AvaloniaObject obj, int value) {
            obj.SetValue(TotalLoopWaitMsProperty, value);
        }

        public static readonly AttachedProperty<int> TotalLoopWaitMsProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, int>(
                "TotalLoopWaitMs",
                1000);

        #endregion

        #region CurLoopWaitMs AvaloniaProperty
        public static int GetCurLoopWaitMs(AvaloniaObject obj) {
            return obj.GetValue(CurLoopWaitMsProperty);
        }

        public static void SetCurLoopWaitMs(AvaloniaObject obj, int value) {
            obj.SetValue(CurLoopWaitMsProperty, value);
        }

        public static readonly AttachedProperty<int> CurLoopWaitMsProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, int>(
                "CurLoopWaitMs",
                0);

        #endregion

        #region DistTraveled AvaloniaProperty
        public static double GetDistTraveled(AvaloniaObject obj) {
            return obj.GetValue(DistTraveledProperty);
        }

        public static void SetDistTraveled(AvaloniaObject obj, double value) {
            obj.SetValue(DistTraveledProperty, value);
        }

        public static readonly AttachedProperty<double> DistTraveledProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, double>(
                "DistTraveled",
                .0d);

        #endregion

        #region CanvasMousePoint AvaloniaProperty
        public static MpPoint GetCanvasMousePoint(AvaloniaObject obj) {
            return obj.GetValue(CanvasMousePointProperty);
        }

        public static void SetCanvasMousePoint(AvaloniaObject obj, MpPoint value) {
            obj.SetValue(CanvasMousePointProperty, value);
        }

        public static readonly AttachedProperty<MpPoint> CanvasMousePointProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, MpPoint>(
                "CanvasMousePoint",
                null);

        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            

            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is TextBox tb) {
                    if (tb.IsInitialized) {
                        AttachedToVisualHandler(tb, null);
                    } else {
                        tb.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            
        }

        #endregion

        #endregion

        #region Private Methods

        #region Event Handlers

        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is TextBox tb) {
                tb.IsVisible = false;
                //tb.RenderTransformOrigin = RelativePoint.TopLeft;

                Panel parentPanel = null;
                var canvas = new Canvas() {
                    //RenderTransformOrigin = RelativePoint.TopLeft,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = GetMargin(tb)
                };

                if (tb.Parent is Panel) {
                    parentPanel = tb.Parent as Panel;
                    var childrenToRemove = new List<Control>();
                    foreach (Control cfe in parentPanel.Children) {
                        if (cfe == tb) {
                            continue;
                        }
                        childrenToRemove.Add(cfe);
                    }
                    for (int i = 0; i < childrenToRemove.Count; i++) {
                        parentPanel.Children.Remove(childrenToRemove[i]);
                    }

                    int tbbIdx = parentPanel.Children.IndexOf(tb);
                    parentPanel.Children.Insert(tbbIdx, canvas);
                } else {
                    // unknown parent type
                    Debugger.Break();
                }
                canvas.IsVisible = true;

                SetCanvas(tb, canvas);

                canvas.Bind(
                        Canvas.HeightProperty,
                        new Binding() {
                            Source = tb,
                            Path = nameof(tb.Bounds.Height)
                        });

                canvas.Bind(
                        Canvas.IsVisibleProperty,
                        new Binding() {
                            Source = tb,
                            Path = nameof(tb.IsReadOnly),
                            Mode = BindingMode.OneWay
                        });

                tb.Bind(
                        TextBox.IsVisibleProperty,
                        new Binding() {
                            Source = tb,
                            Path = nameof(tb.IsReadOnly),
                            Mode = BindingMode.OneWay,
                            Converter = new MpAvBoolFlipConverter()
                        });

                parentPanel.EffectiveViewportChanged += ParentPanel_EffectiveViewportChanged;
                parentPanel.AddHandler(Panel.PointerPressedEvent, ParentPanel_PointerPressed, RoutingStrategies.Tunnel);
                parentPanel.DetachedFromVisualTree += ParentPanel_DetachedFromVisualTree;

                canvas.PointerEnter += Canvas_PointerEnter;
                canvas.GetObservable(Canvas.IsVisibleProperty).Subscribe(value => Canvas_IsVisibleChanged(canvas, value));
                canvas.DetachedFromVisualTree += Canvas_DetachedFromVisualTree;
                canvas.PointerMoved += Canvas_PointerMoved;
                canvas.PointerLeave += Canvas_PointerLeave;

                tb.DataContextChanged += Tb_DataContextChanged;
                tb.GetObservable(TextBox.TextProperty).Subscribe(value => Init(tb));
                tb.GotFocus += Tb_GotFocus;
                tb.LostFocus += Tb_LostFocus;
                tb.GetObservable(TextBox.IsVisibleProperty).Subscribe(value => Tb_IsVisibleChanged(tb, value));
                tb.DetachedFromVisualTree += DetachedToVisualHandler;
                tb.AddHandler(TextBox.KeyDownEvent, Tb_KeyDown, RoutingStrategies.Tunnel);
                if (e == null) {
                    tb.AttachedToVisualTree += AttachedToVisualHandler;
                }

                Init(tb);
            }
        }

        private static void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is TextBox tb) {
                tb.AttachedToVisualTree -= AttachedToVisualHandler;
                tb.DetachedFromVisualTree -= DetachedToVisualHandler;
                tb.DataContextChanged -= Tb_DataContextChanged;
                tb.GotFocus -= Tb_GotFocus;
                tb.LostFocus -= Tb_LostFocus;
                tb.RemoveHandler(TextBox.KeyDownEvent, Tb_KeyDown);
            }
        }

        private static void ParentPanel_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            if (sender is Panel parentPanel) {
                Init(GetTextBoxFromParent(parentPanel));
            }
        }

        private static void ParentPanel_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (sender is Panel p) {
                var tb = GetTextBoxFromParent(p);
                if (tb.IsVisible || !GetEditOnMouseClick(tb)) {
                    return;
                }
                e.Handled = true;
                SetIsReadOnly(tb, false);
            }
        }

        private static void ParentPanel_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Panel p) {
                //p.Children.Clear();
                p.EffectiveViewportChanged -= ParentPanel_EffectiveViewportChanged;
                p.RemoveHandler(Panel.PointerPressedEvent, ParentPanel_PointerPressed);
                p.DetachedFromVisualTree -= ParentPanel_DetachedFromVisualTree;
            }
        }


        private static void Canvas_IsVisibleChanged(Canvas canvas, bool isVisible) {
            if (isVisible) {
                Canvas_PointerEnter(canvas, null);
            }
            canvas.InvalidateAll();
        }

        private static void Canvas_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (sender is Canvas canvas &&
                canvas.Parent is Panel) {
                MpPoint mp;
                if (e == null) {
                    // this means this handler is triggered from visibility change so set 
                    // cursor to canvas origin
                    mp = new MpPoint(canvas.Width / 2, 0);
                } else {
                    mp = e.GetPosition(canvas.Parent).ToPortablePoint();
                    
                }
                SetCanvasMousePoint(canvas, mp);
                AnimateAsync(canvas).FireAndForgetSafeAsync(canvas.DataContext as MpViewModelBase);
            }
        }
        private static void Canvas_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is Canvas canvas) {
                var mp = e.GetPosition(canvas.Parent).ToPortablePoint();
                SetCanvasMousePoint(canvas, mp);
            }
        }
        private static void Canvas_PointerLeave
            (object sender, PointerEventArgs e) {
            if (sender is Canvas canvas) {
                SetCanvasMousePoint(canvas, null);
            }
        }

        private static void Canvas_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Canvas canvas) {
                canvas.PointerEnter -= Canvas_PointerEnter;
                canvas.DetachedFromVisualTree -= Canvas_DetachedFromVisualTree;
                canvas.PointerMoved -= Canvas_PointerMoved;
                canvas.PointerLeave -= Canvas_PointerLeave;
            }
        }

        private static void Tb_DataContextChanged(object sender, EventArgs e) {
            if (sender is TextBox tb) {
                Init(tb);
            }
        }

        private static void Tb_GotFocus(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                tb.SelectAll();
            }
        }

        private static void Tb_LostFocus(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                SetIsReadOnly(tb, true);
            }
        }

        private static void Tb_IsVisibleChanged(TextBox tb, bool isVisible) {
            if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                return;
            }
            if (isVisible) {
                Dispatcher.UIThread.Post(async () => {
                    await Task.Delay(500);
                    tb.SelectAll();
                    //MpAvIsFocusedExtension.SetIsFocused(tb, true);
                    tb.Focus();
                });
            }
            //tb.InvalidateAll();
        }

        private static void Tb_KeyDown(object sender, KeyEventArgs e) {
            if (sender is Control control) {
                if (e.Key == Key.Enter && GetFinishEditCommand(control) is ICommand finishCmd) {
                    e.Handled = true;
                    finishCmd.Execute(null);

                } else if (e.Key == Key.Escape && GetCancelEditCommand(control) is ICommand cancelCmd) {
                    e.Handled = true;
                    cancelCmd.Execute(null);
                }
            }
        }

        #endregion


        private static void Init(TextBox tb) {
            if (tb == null || tb.DataContext == null || !tb.IsInitialized ||
                (tb.Parent is Panel panel && !panel.IsInitialized)) {
                return;
            }

            Canvas canvas = GetCanvas(tb);

            if (canvas == null) {
                return;
            } else if (canvas.Parent == null) {
                //TextBoxBase_Loaded(tbb,null);
                Debugger.Break();
            }

            var textBmp = GetMarqueeBitmap(tb, out Size ftSize);

            var img1 = new Image() {
                Source = textBmp,
                Width = textBmp.PixelSize.Width,
                Height = textBmp.PixelSize.Height,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var img2 = new Image() {
                Source = textBmp,
                Width = textBmp.PixelSize.Width,
                Height = textBmp.PixelSize.Height,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            canvas.Children.Clear();
            canvas.Children.Add(img1);

            if (GetMaxPanelWidth(tb) == default &&
                tb.Parent is Panel p) {
                double maxWidth = p.MaxWidth.IsNumber() ? p.MaxWidth : p.Bounds.Width;
                SetMaxPanelWidth(tb, maxWidth);
            }

            double maxRenderWidth = GetMaxPanelWidth(tb);

            if (ftSize.Width > (int)maxRenderWidth) {
                canvas.Children.Add(img2);
                canvas.Width = (double)(textBmp.PixelSize.Width * 2);
            } else {
                canvas.Width = (double)(textBmp.PixelSize.Width);
            }
        }

        private static async Task AnimateAsync(Canvas canvas) {
            while (true) {
                int delayMs = 20;

                if (!canvas.IsVisible || canvas.Children.Count < 2) {
                    return;
                }

                var img1 = canvas.Children.ElementAt(0) as Image;
                var img2 = canvas.Children.ElementAt(1) as Image;

                var cmp = GetCanvasMousePoint(canvas);
                cmp = cmp == null ? new MpPoint() : cmp;
                bool isReseting = GetCanvasMousePoint(canvas) == null || !canvas.Bounds.Contains(cmp.ToAvPoint());

                var tb = GetTextBoxFromParent(canvas.Parent as Panel);
                double max_width = GetMaxPanelWidth(tb);
                double velMultiplier = cmp.X / max_width;
                velMultiplier = isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, velMultiplier));

                double deltaX = GetMaxVelocity(canvas) * velMultiplier;

                double left1 = Canvas.GetLeft(img1).HasValue() ? Canvas.GetLeft(img1) : 0;
                double right1 = left1 + img1.Width;

                double left2 = Canvas.GetLeft(img2).HasValue() ? Canvas.GetLeft(img2) : img1.Width;
                double right2 = left2 + img2.Width;

                if (isReseting) {
                    if (Math.Abs(left1) < Math.Abs(left2)) {
                        if (left1 < 0) {
                            deltaX *= -1;
                        }
                    } else {
                        if (left2 < 0) {
                            deltaX *= -1;
                        }
                    }
                }

                double nLeft1 = left1 + deltaX;
                double nRight1 = right1 + deltaX;
                double nLeft2 = left2 + deltaX;
                double nRight2 = right2 + deltaX;

                if (!isReseting) {
                    if (nLeft1 < nLeft2 && nLeft2 < 0) {
                        nLeft1 = nRight2;
                    } else if (nLeft2 < nLeft1 && nLeft1 < 0) {
                        nLeft2 = nRight1;
                    }
                }

                if (isReseting) {
                    if (Math.Abs(nLeft1) < deltaX || Math.Abs(nLeft2) < deltaX) {
                        Canvas.SetLeft(img1, 0);
                        Canvas.SetLeft(img2, img1.Width);
                        canvas.InvalidateVisual();

                        int test = GetTotalLoopWaitMs(canvas);
                        SetCurLoopWaitMs(canvas, 0);
                        SetDistTraveled(canvas, 0);
                        return;
                    }
                }

                double maxLoopDeltaX = 5;
                int curLoopDelayMs = GetCurLoopWaitMs(canvas);
                double distTraveled = GetDistTraveled(canvas);
                bool isInitialLoop = Math.Abs(distTraveled) < img1.Width;
                bool isLoopDelaying = !isInitialLoop &&
                                        (Math.Abs(nLeft1) < maxLoopDeltaX || Math.Abs(nLeft2) < maxLoopDeltaX);

                if (isLoopDelaying) {
                    //pause this cycle
                    curLoopDelayMs += delayMs;
                    // initial loop delay (snap to 0)
                    nLeft1 = 0;
                    nLeft2 = img1.Width;
                }
                if (curLoopDelayMs > 1000) {
                    //loop delay is over reset elapsed and bump so not caught next pass
                    curLoopDelayMs = 0;
                    double vel_dir = GetMaxVelocity(canvas) > 0 ? 1 : -1;
                    nLeft1 = (maxLoopDeltaX + 0.5) * vel_dir;
                    nLeft2 = nLeft1 + img1.Width;
                }

                Canvas.SetLeft(img1, nLeft1);
                Canvas.SetLeft(img2, nLeft2);
                SetCurLoopWaitMs(canvas, curLoopDelayMs);
                SetDistTraveled(canvas, distTraveled + deltaX);

                canvas.InvalidateVisual();

                await Task.Delay(delayMs);
            }
        }

        private static TextBox GetTextBoxFromParent(Panel p) {
            var tb = p.Children.FirstOrDefault(x => x is TextBox tb && GetCanvas(tb) != null);
            return tb as TextBox;
        }

        private static Bitmap GetMarqueeBitmap(TextBox tb, out Size ftSize) {
            var canvas = GetCanvas(tb);
            int pad = (int)GetTailPadding(canvas);
            double fs = Math.Max(1.0d, tb.FontSize);

            Size textSize = new Size(tb.Text.Length * fs, fs);
            var ft = tb.ToFormattedText();
            ft.FontSize = Math.Max(1.0d, tb.FontSize);
            
            ftSize = ft.Bounds.Size;
            ft.Constraint = ftSize;
            // pixelsPerDip = 1.75
            // pixelsPerInch = 168

            var dpi = MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelsPerInch.ToAvVector();
            double pixelsPerDip = 1;// MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelDensity;

            var ftBmp = new RenderTargetBitmap(
                new PixelSize(
                    (int)Math.Max(1.0, ft.Bounds.Width * pixelsPerDip) + pad + (int)GetDropShadowOffset(canvas).X,
                    (int)Math.Max(1.0, ft.Bounds.Height * pixelsPerDip) + (int)GetDropShadowOffset(canvas).Y));

            using (var context = ftBmp.CreateDrawingContext(null)) {
                context.Clear(Colors.Transparent);
                context.DrawText(GetDropShadowBrush(tb), GetDropShadowOffset(canvas), ft.PlatformImpl);
                context.DrawText(GetForegroundBrush(tb), new Point(0, 0), ft.PlatformImpl);
            }

            var bmp = ftBmp.ToAvBitmap();

            //MpFileIo.WriteByteArrayToFile(@"C:\Users\tkefauver\Desktop\text_bmp.png", bmp.ToByteArray(), false);

            return bmp;
        }
        #endregion
    }
}
