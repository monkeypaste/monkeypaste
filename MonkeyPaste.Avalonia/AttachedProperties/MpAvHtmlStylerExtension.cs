using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public enum MpHtmlStyleType {
        None = 0,
        Tooltip,
        Content
    }

    public static class MpAvHtmlStylerExtension {
        private static Dictionary<HtmlControl, List<IDisposable>> _disposableLookup = [];
        static MpAvHtmlStylerExtension() {
            try {
                MpAvThemeViewModel.Instance
                .CustomFontFamilyNames
                .ForEach(x =>
                    HtmlRender.AddFontFamily(MpAvStringToFontFamilyConverter.Instance.Convert(x, null, null, null) as FontFamily));
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error initializing html font familys.", ex);
            }
            IsEnabledProperty.Changed.AddClassHandler<HtmlControl>((x, y) => HandleIsEnabledChanged(x, y));

            // need handlers for any css related properties
            HtmlStyleTypeProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultFontSizeProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultFontFamilyProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultHexColorProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            ShowUnderlinesProperty.Changed.AddClassHandler<HtmlControl>((x, y) => ToggleUnderlines(x));
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
            AvaloniaProperty.RegisterAttached<object, Control, MpHtmlStyleType>(
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
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "DefaultFontSize",
                MpAvPrefViewModel.Instance == null ?
                    MpAvPrefViewModel.BASE_DEFAULT_FONT_SIZE :
                    MpAvPrefViewModel.Instance.DefaultFontSize);

        #endregion

        #region DefaultFontFamily AvaloniaProperty
        public static string GetDefaultFontFamily(AvaloniaObject obj) {
            return obj.GetValue(DefaultFontFamilyProperty);
        }

        public static void SetDefaultFontFamily(AvaloniaObject obj, string value) {
            obj.SetValue(DefaultFontFamilyProperty, value);
        }

        public static readonly AttachedProperty<string> DefaultFontFamilyProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "DefaultFontFamily",
                MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily);

        #endregion

        #region DefaultHexColor AvaloniaProperty
        public static string GetDefaultHexColor(AvaloniaObject obj) {
            return obj.GetValue(DefaultHexColorProperty);
        }

        public static void SetDefaultHexColor(AvaloniaObject obj, string value) {
            obj.SetValue(DefaultHexColorProperty, value);
        }

        public static readonly AttachedProperty<string> DefaultHexColorProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "DefaultHexColor",
                Mp.Services == null || Mp.Services.PlatformResource == null ?
                    "#000000" :
                    Mp.Services.PlatformResource
                    .GetResource<string>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString()));

        #endregion

        #region ShowUnderlines AvaloniaProperty
        public static bool GetShowUnderlines(AvaloniaObject obj) {
            return obj.GetValue(ShowUnderlinesProperty);
        }

        public static void SetShowUnderlines(AvaloniaObject obj, bool value) {
            obj.SetValue(ShowUnderlinesProperty, value);
        }

        public static readonly AttachedProperty<bool> ShowUnderlinesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "ShowUnderlines",
                false);

        #endregion

        #region ZoomFactor AvaloniaProperty
        public static double GetZoomFactor(AvaloniaObject obj) {
            return obj.GetValue(ZoomFactorProperty);
        }

        public static void SetZoomFactor(AvaloniaObject obj, double value) {
            obj.SetValue(ZoomFactorProperty, value);
        }

        public static readonly AttachedProperty<double> ZoomFactorProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ZoomFactor",
                1.0d);

        #endregion

        #region LinkClickCommand AvaloniaProperty
        public static ICommand GetLinkClickCommand(AvaloniaObject obj) {
            return obj.GetValue(LinkClickCommandProperty);
        }

        public static void SetLinkClickCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(LinkClickCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> LinkClickCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
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
            AvaloniaProperty.RegisterAttached<object, Control, object>(
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
            List<IDisposable> dl = null;
            if (!_disposableLookup.ContainsKey(hc)) {
                dl = [];
                _disposableLookup.Add(hc, dl);
            } else {
                dl = _disposableLookup[hc];
            }

            hc.DetachedFromVisualTree += Hc_DetachedFromVisualTree;
            hc.RenderError += Hc_RenderError;
            hc.StylesheetLoad += Hc_StylesheetLoad;
            hc.LinkClicked += Hc_LinkClicked;
            hc.GetObservable(HtmlControl.TextProperty).Subscribe(value => OnTextChanged(hc)).AddDisposable(dl);
            UpdateContent(hc, true);
        }
        private static void OnTextChanged(HtmlControl hc) {
            UpdateContent(hc);
        }
        private static void UpdateContent(HtmlControl hc, bool set_style = false) {
            if (set_style) {
                // for some reason changing the style sheet makes html disappear so only setting on load now
                hc.BaseStylesheet = GetStyleSheet(hc);
            }
            if (!hc.Text.ToStringOrEmpty().IsStringHtmlDocument()) {
                // ensure text is full html doc or stylesheet stuff doesn't work
                hc.Text = hc.Text.ToStringOrEmpty().ToHtmlDocumentFromTextOrPartialHtml();
            }
            hc.Redraw();
        }

        private static void ToggleUnderlines(HtmlControl hc) {
            if (hc.Text is not string html_str) {
                return;
            }
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(html_str);
                if (doc.DocumentNode.SelectNodes("//p") is not { } pl) {
                    return;
                }
                bool is_disabling = pl.Any(x => x.HasClass("underline"));
                if (is_disabling) {
                    pl.ForEach(x => x.RemoveClass("underline"));
                } else {
                    pl.ForEach(x => x.AddClass("underline"));
                }

                hc.Text = doc.DocumentNode.OuterHtml;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error toggling underlines. ", ex);
            }
        }

        private static void Hc_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not HtmlControl hc) {
                return;
            }
            hc.DetachedFromVisualTree -= Hc_DetachedFromVisualTree;
            hc.RenderError -= Hc_RenderError;
            hc.StylesheetLoad -= Hc_StylesheetLoad;
            hc.LinkClicked -= Hc_LinkClicked;
            if (_disposableLookup.TryGetValue(hc, out var dl)) {
                dl.ForEach(x => x.Dispose());
                _disposableLookup.Remove(hc);
            }
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
            // NOTE this is sample css from HtmlRenderer proj:
            /*
            @"h1, h2, h3 { color: navy; font-weight:normal; }
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
            */
            string css_str = string.Empty;
            switch (GetHtmlStyleType(hc)) {
                case MpHtmlStyleType.Content:
                    css_str = string.Format(
@"* {{ margin: 0; padding: 0; }}
body {{ white-space: pre-wrap; line-height: {1}px; color: {0}; font-size: {1}px; font-family: {2}; }}
p {{ margin: 0; }}
.paste-tooltip-suffix {{ font-style: italic; color: {3}; }}
.underline {{ text-decoration: underline;  }}
.highlight {{ background-color: yellow; color: black; }}
.highlight-active {{ background-color: lime; color: black; }}
a:link {{ text-decoration: none; }}
a:hover {{ text-decoration: underline; }}",
                        GetDefaultHexColor(hc).RemoveHexAlpha(), //0
                        GetDefaultFontSize(hc), //1
                        GetDefaultFontFamily(hc), //2
                        MpSystemColors.gold1.RemoveHexAlpha(), //3
                        Mp.Services.PlatformResource
                        .GetResource<IBrush>("HighlightBrush_inactive").ToPortableColor().ToHex(true) //5
                        );
                    break;
                case MpHtmlStyleType.Tooltip:
                default:
                    css_str = string.Format(
@"* {{ margin: 0; padding: 0; }}
body {{ white-space: pre-wrap; line-height: {1}px; color: {0}; font-size: {1}px; font-family: {2}; }}
p {{ margin: 0; }}
.paste-tooltip-suffix {{ font-style: italic; color: {3}; }}
a:link {{ text-decoration: none; }}
a:hover {{ text-decoration: underline; }}",
                        GetDefaultHexColor(hc).RemoveHexAlpha(), //0
                        GetDefaultFontSize(hc), //1
                        GetDefaultFontFamily(hc), //2
                        MpSystemColors.gold1.RemoveHexAlpha() //3
                        );
                    break;
            }
            return css_str;
        }
    }
}
