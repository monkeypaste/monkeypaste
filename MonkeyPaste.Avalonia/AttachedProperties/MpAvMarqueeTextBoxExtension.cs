﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        #region ForegroundHexColor AvaloniaProperty
        public static string GetForegroundHexColor(AvaloniaObject obj) {
            return obj.GetValue(ForegroundHexColorProperty);
        }

        public static void SetForegroundHexColor(AvaloniaObject obj, string value) {
            obj.SetValue(ForegroundHexColorProperty, value);
        }

        public static readonly AttachedProperty<string> ForegroundHexColorProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, string>(
                "ForegroundHexColor",
                MpSystemColors.White);

        #endregion

        #region DropShadowHexColor AvaloniaProperty
        public static string GetDropShadowHexColor(AvaloniaObject obj) {
            return obj.GetValue(DropShadowHexColorProperty);
        }

        public static void SetDropShadowHexColor(AvaloniaObject obj, string value) {
            obj.SetValue(DropShadowHexColorProperty, value);
        }

        public static readonly AttachedProperty<string> DropShadowHexColorProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, string>(
                "DropShadowHexColor",
                MpSystemColors.Black);

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
            Point? canvasMp = null;

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

            #region Event Handlers

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is TextBox tb) {
                    tb.IsVisible = false;
                    //tb.RenderTransformOrigin = RelativePoint.TopLeft;

                    Panel parentPanel = null;
                    var canvas = new Canvas() {
                        //RenderTransformOrigin = RelativePoint.TopLeft,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        //VerticalAlignment = VerticalAlignment.Center
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
                    if (e == null) {
                        tb.AttachedToVisualTree += AttachedToVisualHandler;
                    }

                    Init(tb);
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is TextBox tb) {
                    tb.AttachedToVisualTree -= AttachedToVisualHandler;
                    tb.DetachedFromVisualTree -= DetachedToVisualHandler;
                    tb.DataContextChanged -= Tb_DataContextChanged;
                    tb.GotFocus -= Tb_GotFocus;
                    tb.LostFocus -= Tb_LostFocus;
                }
            }

            void ParentPanel_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
                if (sender is Panel parentPanel) {
                    Init(GetTextBoxFromParent(parentPanel));
                }
            }

            void ParentPanel_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
                if (sender is Panel p) {
                    var tb = GetTextBoxFromParent(p);
                    if (tb.IsVisible || !GetEditOnMouseClick(tb)) {
                        return;
                    }
                    e.Handled = true;
                    SetIsReadOnly(tb, false);
                }
            }

            void ParentPanel_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
                if (sender is Panel p) {
                    //p.Children.Clear();
                    p.EffectiveViewportChanged -= ParentPanel_EffectiveViewportChanged;
                    p.RemoveHandler(Panel.PointerPressedEvent, ParentPanel_PointerPressed);
                    p.DetachedFromVisualTree -= ParentPanel_DetachedFromVisualTree;
                }
            }


            void Canvas_IsVisibleChanged(Canvas canvas, bool isVisible) {
                if (isVisible) {
                    Canvas_PointerEnter(canvas, null);
                }
                canvas.InvalidateAll();
            }

            void Canvas_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e) {
                if (sender is Canvas canvas) {
                    if(e == null) {
                        // this means this handler is triggered from visibility change so set 
                        // cursor to canvas origin
                        canvasMp = canvas.Bounds.Position;
                    } else {
                        canvasMp = e.GetPosition(canvas.Parent);
                    }                   
                    
                    AnimateAsync(canvas).FireAndForgetSafeAsync(canvas.DataContext as MpViewModelBase);
                }
            }
            void Canvas_PointerMoved(object sender, PointerEventArgs e) {
                if(sender is Canvas canvas) {
                    canvasMp = e.GetPosition(canvas.Parent);
                }
            }
            void Canvas_PointerLeave
                (object sender, PointerEventArgs e) {
                if (sender is Canvas canvas) {
                    canvasMp = null;
                }
            }

            void Canvas_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
                if (sender is Canvas canvas) {
                    canvas.PointerEnter -= Canvas_PointerEnter;
                    canvas.DetachedFromVisualTree -= Canvas_DetachedFromVisualTree;
                    canvas.PointerMoved -= Canvas_PointerMoved;
                    canvas.PointerLeave -= Canvas_PointerLeave;
                }
            }

            void Tb_DataContextChanged(object sender, EventArgs e) {
                if (sender is TextBox tb) {
                    Init(tb);
                }
            }

            void Tb_GotFocus(object sender, RoutedEventArgs e) {
                if (sender is TextBox tb) {
                    tb.SelectAll();
                }
            }

            void Tb_LostFocus(object sender, RoutedEventArgs e) {
                if (sender is TextBox tb) {
                    SetIsReadOnly(tb, true);
                }
            }

            void Tb_IsVisibleChanged(TextBox tb, bool isVisible) {
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return;
                }
                if (isVisible) {
                    if (!tb.IsFocused) {
                        //tb.Focus();
                        //tb.CaretIndex = 0;
                        //tb.SelectAll();
                        //MpAvIsFocusedExtension.SetIsFocused(tb, true);
                    }
                }
                //tb.InvalidateAll();
            }

            #endregion


            void Init(TextBox tb) {
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

                double maxRenderWidth = tb.Bounds.Width;
                if (tb.Parent is Panel p) {
                    maxRenderWidth = p.MaxWidth;
                }
                if (ftSize.Width > (int)maxRenderWidth) {
                    canvas.Children.Add(img2);
                    canvas.Width = (double)(textBmp.PixelSize.Width * 2);
                } else {
                    canvas.Width = (double)(textBmp.PixelSize.Width);
                }
            }

            async Task AnimateAsync(Canvas canvas) {
                while (true) {
                    if (!canvas.IsVisible || canvas.Children.Count < 2) {
                        return;
                    }
                    var img1 = canvas.Children.ElementAt(0) as Image;
                    var img2 = canvas.Children.ElementAt(1) as Image;

                    var cmp = canvasMp.HasValue ? canvasMp.Value : new Point();
                    bool isReseting = !canvasMp.HasValue || !canvas.Bounds.Contains(cmp);

                    double velMultiplier = cmp.X / (canvas.Parent as Panel).MaxWidth;
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

                    Canvas.SetLeft(img1, nLeft1);
                    Canvas.SetLeft(img2, nLeft2);

                    if (isReseting) {
                        if (Math.Abs(nLeft1) < deltaX || Math.Abs(nLeft2) < deltaX) {
                            Canvas.SetLeft(img1, 0);
                            Canvas.SetLeft(img2, img1.Width);
                            canvas.InvalidateVisual();
                            return;
                        }
                    }

                    canvas.InvalidateVisual();

                    await Task.Delay(20);
                }
            }

            TextBox GetTextBoxFromParent(Panel p) {
                var tb = p.Children.FirstOrDefault(x => x is TextBox tb && GetCanvas(tb) != null);
                return tb as TextBox;
            }

            Bitmap GetMarqueeBitmap(TextBox tb, out Size ftSize) {
                var canvas = GetCanvas(tb);
                int pad = (int)GetTailPadding(canvas);
                
                
                Size textSize = new Size(tb.Text.Length * tb.FontSize, tb.FontSize);
                var ft = new FormattedText(
                        tb.Text,
                        new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight),
                        tb.FontSize,
                        tb.TextAlignment,
                        tb.TextWrapping,
                        new Size());

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
                    context.DrawText(GetDropShadowHexColor(tb).ToAvBrush(), GetDropShadowOffset(canvas), ft.PlatformImpl);
                    context.DrawText(GetForegroundHexColor(tb).ToAvBrush(), new Point(0, 0), ft.PlatformImpl);
                }

                var bmp = ftBmp.ToAvBitmap();

                MpFileIo.WriteByteArrayToFile(@"C:\Users\tkefauver\Desktop\text_bmp.png", bmp.ToByteArray(), false);

                return bmp;
            }
        }

        #endregion

        #endregion
    }
}