﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System.Windows.Input;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public enum MpHtmlStyleType {
        None = 0,
        Tooltip,
        Content
    }

    public static class MpAvHtmlStylerExtension {
        static MpAvHtmlStylerExtension() {
            MpAvThemeViewModel.Instance
                .CustomFontFamilyNames
                .ForEach(x =>
                    HtmlRender.AddFontFamily(MpAvStringToFontFamilyConverter.Instance.Convert(x, null, null, null) as FontFamily));

            IsEnabledProperty.Changed.AddClassHandler<HtmlControl>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region HtmlStyleType AvaloniaProperty
        public static MpHtmlStyleType GetHtmlStyleType(AvaloniaObject obj) {
            return obj.GetValue(HtmlStyleTypeProperty);
        }

        public static void SetHtmlStyleType(AvaloniaObject obj, MpHtmlStyleType value) {
            obj.SetValue(HtmlStyleTypeProperty, value);
        }

        public static readonly AttachedProperty<MpHtmlStyleType> HtmlStyleTypeProperty =
            AvaloniaProperty.RegisterAttached<object, HtmlControl, MpHtmlStyleType>(
                "HtmlStyleType",
                MpHtmlStyleType.None);

        #endregion

        #region DefaultFontSize AvaloniaProperty
        public static double GetDefaultFontSize(AvaloniaObject obj) {
            return obj.GetValue(DefaultFontSizeProperty);
        }

        public static void SetDefaultFontSize(AvaloniaObject obj, double value) {
            obj.SetValue(DefaultFontSizeProperty, value);
        }

        public static readonly AttachedProperty<double> DefaultFontSizeProperty =
            AvaloniaProperty.RegisterAttached<object, HtmlControl, double>(
                "DefaultFontSize",
                MpAvPrefViewModel.Instance.DefaultFontSize);

        #endregion

        #region DefaultColor AvaloniaProperty
        public static string GetDefaultColor(AvaloniaObject obj) {
            return obj.GetValue(DefaultColorProperty);
        }

        public static void SetDefaultColor(AvaloniaObject obj, string value) {
            obj.SetValue(DefaultColorProperty, value);
        }

        public static readonly AttachedProperty<string> DefaultColorProperty =
            AvaloniaProperty.RegisterAttached<object, HtmlControl, string>(
                "DefaultColor",
                Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeInteractiveColor.ToString()));

        #endregion

        #region LinkClickCommand AvaloniaProperty
        public static ICommand GetLinkClickCommand(AvaloniaObject obj) {
            return obj.GetValue(LinkClickCommandProperty);
        }

        public static void SetLinkClickCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LinkClickCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LinkClickCommandProperty =
            AvaloniaProperty.RegisterAttached<object, HtmlControl, ICommand>(
                "LinkClickCommand",
                null);

        #endregion

        #region LinkClickCommandParameter AvaloniaProperty
        public static object GetLinkClickCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(LinkClickCommandParameterProperty);
        }

        public static void SetLinkClickCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(LinkClickCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> LinkClickCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, HtmlControl, object>(
                "LinkClickCommandParameter",
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
            AvaloniaProperty.RegisterAttached<object, DataGrid, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(HtmlControl hc, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                hc.AttachedToVisualTree += Hc_AttachedToVisualTree;
                if (hc.IsAttachedToVisualTree()) {
                    Hc_AttachedToVisualTree(hc, null);
                }
            } else {
                Hc_DetachedFromVisualTree(hc, null);
            }
        }

        private static void Hc_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not HtmlControl hc) {
                return;
            }
            hc.DetachedFromVisualTree += Hc_DetachedFromVisualTree;
            hc.RenderError += Hc_RenderError;
            hc.StylesheetLoad += Hc_StylesheetLoad;
            hc.LinkClicked += Hc_LinkClicked;
        }

        private static void Hc_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not HtmlControl hc) {
                return;
            }
            hc.DetachedFromVisualTree -= Hc_DetachedFromVisualTree;
            hc.RenderError -= Hc_RenderError;
            hc.StylesheetLoad -= Hc_StylesheetLoad;
            hc.LinkClicked -= Hc_LinkClicked;
        }
        private static void Hc_LinkClicked(object sender, HtmlRendererRoutedEventArgs<TheArtOfDev.HtmlRenderer.Core.Entities.HtmlLinkClickedEventArgs> e) {
            if (sender is not HtmlControl hc ||
                GetLinkClickCommand(hc) is not ICommand cmd) {
                return;
            }
            e.Event.Handled = true;
            cmd.Execute(GetLinkClickCommandParameter(hc));
        }

        private static void Hc_StylesheetLoad(object sender, HtmlRendererRoutedEventArgs<TheArtOfDev.HtmlRenderer.Core.Entities.HtmlStylesheetLoadEventArgs> e) {
            if (sender is not HtmlControl hc) {
                return;
            }
            if (GetStyleSheet(hc) is string ss &&
                ss != null) {
                e.Event.SetStyleSheet = ss;
            }
        }

        private static void Hc_RenderError(object sender, HtmlRendererRoutedEventArgs<TheArtOfDev.HtmlRenderer.Core.Entities.HtmlRenderErrorEventArgs> e) {
            MpDebug.Break(e.Event.ToString());
        }


        #endregion


        #endregion

        private static string GetStyleSheet(HtmlControl hc) {
            switch (GetHtmlStyleType(hc)) {
                default:
                    return string.Format(
@"* {{ color: {0}; font: {1}px {2}; }}
a:link {{ text-decoration: none; }}
a:hover {{ text-decoration: underline; }}",
                        GetDefaultColor(hc),
                        GetDefaultFontSize(hc),
                        MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily);

                case MpHtmlStyleType.Content:
                    return @"h1, h2, h3 { color: navy; font-weight:normal; }
                            h1 { margin-bottom: .47em }
                            h2 { margin-bottom: .3em }
                            h3 { margin-bottom: .4em }
                            ul { margin-top: .5em }
                            ul li {margin: .25em}
                            body { font:10pt Tahoma }
		                    pre  { border:solid 1px gray; background-color:#eee; padding:1em }
                            a:link { text-decoration: none; }
                            a:hover { text-decoration: underline; }
                            .gray    { color:gray; }
                            .example { background-color:#efefef; corner-radius:5px; padding:0.5em; }
                            .whitehole { background-color:white; corner-radius:10px; padding:15px; }
                            .caption { font-size: 1.1em }
                            .comment { color: green; margin-bottom: 5px; margin-left: 3px; }
                            .comment2 { color: green; }";
                case MpHtmlStyleType.Tooltip:
                    return @"h1, h2, h3 { color: navy; font-weight:normal; }
                    h1 { margin-bottom: .47em }
                    h2 { margin-bottom: .3em }
                    h3 { margin-bottom: .4em }
                    ul { margin-top: .5em }
                    ul li {margin: .25em}
                    body { font:10pt Tahoma }
		            pre  { border:solid 1px gray; background-color:#eee; padding:1em }
                    a:link { text-decoration: none; }
                    a:hover { text-decoration: underline; }
                    .gray    { color:gray; }
                    .example { background-color:#efefef; corner-radius:5px; padding:0.5em; }
                    .whitehole { background-color:white; corner-radius:10px; padding:15px; }
                    .caption { font-size: 1.1em }
                    .comment { color: green; margin-bottom: 5px; margin-left: 3px; }
                    .comment2 { color: green; }";
            }
        }
    }
}