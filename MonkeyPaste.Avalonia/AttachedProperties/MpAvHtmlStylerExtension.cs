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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public enum MpHtmlStyleType {
        None = 0,
        Tooltip,
        Content
    }

    public static class MpAvHtmlStylerExtension {
        private static (string name,string data) _syntaxStyle;
        static MpAvHtmlStylerExtension() {
            //if (MpAvThemeViewModel.Instance != null &&
            //    MpAvThemeViewModel.Instance.CustomFontFamilyNames != null) {
            //    try {
            //        MpAvThemeViewModel.Instance
            //        .CustomFontFamilyNames
            //        .ForEach(x =>
            //            HtmlRender.AddFontFamily(MpAvStringToFontFamilyConverter.Instance.Convert(x, null, null, null) as FontFamily));
            //    }
            //    catch (Exception ex) {
            //        MpConsole.WriteTraceLine($"Error initializing html font familys.", ex);
            //    }
            //}
            IsEnabledProperty.Changed.AddClassHandler<HtmlControl>((x, y) => HandleIsEnabledChanged(x, y));

            // need handlers for any css related properties
            WrapTextProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x,true));
            SyntaxThemeNameProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x,true));
            HtmlStyleTypeProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultFontSizeProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultFontFamilyProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            DefaultHexColorProperty.Changed.AddClassHandler<HtmlControl>((x, y) => UpdateContent(x));
            ShowUnderlinesProperty.Changed.AddClassHandler<HtmlControl>((x, y) => ToggleUnderlines(x));
        }

        #region Properties

        #region SyntaxThemeName AvaloniaProperty
        public static string GetSyntaxThemeName(AvaloniaObject obj) {
            return obj.GetValue(SyntaxThemeNameProperty);
        }

        public static void SetSyntaxThemeName(AvaloniaObject obj, string value) {
            obj.SetValue(SyntaxThemeNameProperty, value);
        }

        public static readonly AttachedProperty<string> SyntaxThemeNameProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "SyntaxThemeName",
                null);

        #endregion
        
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

        #region WrapText AvaloniaProperty
        public static bool GetWrapText(AvaloniaObject obj) {
            return obj.GetValue(WrapTextProperty);
        }

        public static void SetWrapText(AvaloniaObject obj, bool value) {
            obj.SetValue(WrapTextProperty, value);
        }

        public static readonly AttachedProperty<bool> WrapTextProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "WrapText",
                true);

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
            if (MpAvThemeViewModel.Instance != null &&
                MpAvThemeViewModel.Instance.CustomFontFamilyNames != null &&
                hc.Container != null) {
                try {
                    MpAvThemeViewModel.Instance
                    .CustomFontFamilyNames
                    .ForEach(x =>
                        hc.Container.AddFontFamily(MpAvStringToFontFamilyConverter.Instance.Convert(x, null, null, null) as FontFamily));
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error initializing html font familys.", ex);
                }
            }

            hc.DetachedFromVisualTree += Hc_DetachedFromVisualTree;
            hc.RenderError += Hc_RenderError;
            hc.StylesheetLoad += Hc_StylesheetLoad;
            hc.LinkClicked += Hc_LinkClicked;
            hc.GetObservable(HtmlControl.TextProperty).Subscribe(value => OnTextChanged(hc)).AddDisposable(hc);
            UpdateContent(hc, true);
        }
        private static void OnTextChanged(HtmlControl hc) {
            UpdateContent(hc);
        }
        private static void UpdateContent(HtmlControl hc, bool set_style = false) {
            string html_doc_str = hc.Text.ToStringOrEmpty();
            if (set_style) {
                // for some reason changing the style sheet makes html disappear so only setting on load now
                hc.BaseStylesheet = GetStyleSheet(hc);
                hc.Text = null;
            }

            if (!html_doc_str.ToStringOrEmpty().IsStringHtmlDocument()) {
                // ensure text is full html doc or stylesheet stuff doesn't work
                if (!html_doc_str.StartsWith("<")) {
                    // BUG htmlRenderer seems to ignore raw text element html
                    if (html_doc_str.Contains("\n") || html_doc_str.Contains("<br>") || html_doc_str.Contains("<br/>")) {
                        // multi-line 

                    }
                }
                html_doc_str = html_doc_str.ToHtmlDocumentFromTextOrPartialHtml();
            }
            hc.SetHtml(html_doc_str);
        }

        private static void ToggleUnderlines(HtmlControl hc) {
            if (hc is not MpAvHtmlPanel hp ||
                hp.Text.ToHtmlDocument() is not { } doc) {
                return;
            }
            try {
                if (doc.DocumentNode.SelectNodesSafe("//*").Where(x => x.IsBlockElement()) is not { } pl) {
                    return;
                }

                bool is_enabling = GetShowUnderlines(hc);
                bool has_underlines = pl.Any(x => x.HasClass("underline"));
                if (is_enabling && !has_underlines) {
                    // show underlines
                    pl.ForEach(x => x.AddClass("underline"));
                } else if (!is_enabling && has_underlines) {
                    // hide underlines
                    pl.ForEach(x => x.RemoveClass("underline"));
                }
                if(GetHtmlStyleType(hc) == MpHtmlStyleType.Content) {

                }
                hc.SetHtml(doc.DocumentNode.OuterHtml);
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

            hc.ClearDisposables();
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

        //Full Style:
        /*
        html, address,
        blockquote,
        body, dd, div,
        dl, dt, fieldset, form,
        frame, frameset,
        h1, h2, h3, h4,
        h5, h6, noframes,
        ol, p, ul, center,
        dir, menu, pre   { display: block }
        li              { display: list-item }
        head            { display: none }
        table           { display: table }
        tr              { display: table-row }
        thead           { display: table-header-group }
        tbody           { display: table-row-group }
        tfoot           { display: table-footer-group }
        col             { display: table-column }
        colgroup        { display: table-column-group }
        td, th          { display: table-cell }
        caption         { display: table-caption }
        th              { font-weight: bolder; text-align: center }
        caption         { text-align: center }
        body            { margin: 8px }
        h1              { font-size: 2em; margin: .67em 0 }
        h2              { font-size: 1.5em; margin: .75em 0 }
        h3              { font-size: 1.17em; margin: .83em 0 }
        h4,
        blockquote, ul,
        fieldset, form,
        ol, dl, dir,
        menu            { margin: 1.12em 0 }
        h5              { font-size: .83em; margin: 1.5em 0 }
        h6              { font-size: .75em; margin: 1.67em 0 }
        h1, h2, h3, h4,
        h5, h6, b,
        strong          { font-weight: bolder; }
        blockquote      { margin-left: 40px; margin-right: 40px }
        i, cite, em,
        var, address    { font-style: italic }
        pre, tt, code,
        kbd, samp       { font-family: monospace }
        pre             { white-space: pre }
        button, textarea,
        input, select   { display: inline-block }
        big             { font-size: 1.17em }
        small, sub, sup { font-size: .83em }
        sub             { vertical-align: sub }
        sup             { vertical-align: super }
        table           { border-spacing: 2px; }
        thead, tbody,
        tfoot, tr       { vertical-align: middle }
        td, th          { vertical-align: inherit }
        s, strike, del  { text-decoration: line-through }
        hr              { border: 1px inset; }
        ol, ul, dir,
        menu, dd        { margin-left: 40px }
        ol              { list-style-type: decimal }
        ol ul, ul ol,
        ul ul, ol ol    { margin-top: 0; margin-bottom: 0 }
        ol ul, ul ul   { list-style-type: circle }
        ul ul ul, 
        ol ul ul, 
        ul ol ul        { list-style-type: square }
        u, ins          { text-decoration: underline }
        br:before       { content: ""\A"" }
        :before, :after { white-space: pre-line }
        center          { text-align: center }
        :link, :visited { text-decoration: underline }
        :focus          { outline: thin dotted invert }

        // Begin bidirectionality settings (do not change) 
        BDO[DIR=""ltr""]  { direction: ltr; unicode-bidi: bidi-override }
        BDO[DIR=""rtl""]  { direction: rtl; unicode-bidi: bidi-override }

        *[DIR=""ltr""]    { direction: ltr; unicode-bidi: embed }
        *[DIR=""rtl""]    { direction: rtl; unicode-bidi: embed }

        @media print {
          h1            { page-break-before: always }
          h1, h2, h3,
          h4, h5, h6    { page-break-after: avoid }
          ul, ol, dl    { page-break-before: avoid }
        }

        // Not in the specification but necessary 
        a               { color: #0055BB; text-decoration:underline }
        table           { border-color:#dfdfdf; }
        td, th          { border-color:#dfdfdf; overflow: hidden; }
        style, title,
        script, link,
        meta, area,
        base, param     { display:none }
        hr              { border-top-color: #9A9A9A; border-left-color: #9A9A9A; border-bottom-color: #EEEEEE; border-right-color: #EEEEEE; }
        pre             { font-size: 10pt; margin-top: 15px; }
        
        //This is the background of the HtmlToolTip
        .htmltooltip {
            border:solid 1px #767676;
            background-color:white;
            background-gradient:#E4E5F0;
            padding: 8px; 
            Font: 9pt Tahoma;
        }
        */
        private static string DefaultStyleSheet = @"
        html, address,
        blockquote,
        body, dd, div,
        dl, dt, fieldset, form,
        frame, frameset,
        h1, h2, h3, h4,
        h5, h6, noframes,
        ol, p, ul, center,
        dir, menu, pre   { display: block }
        li              { display: list-item }
        head            { display: none }
        table           { display: table }
        tr              { display: table-row }
        thead           { display: table-header-group }
        tbody           { display: table-row-group }
        tfoot           { display: table-footer-group }
        col             { display: table-column }
        colgroup        { display: table-column-group }
        td, th          { display: table-cell }
        caption         { display: table-caption }
        th              { font-weight: bolder; text-align: center }
        caption         { text-align: center }
        body            { margin: 0px }
        h1              { font-size: 2em; margin: .67em 0 }
        h2              { font-size: 1.5em; margin: .75em 0 }
        h3              { font-size: 1.17em; margin: .83em 0 }
        h4,
        blockquote, ul,
        fieldset, form,
        ol, dl, dir,
        menu            { margin: 1.12em 0 }
        h5              { font-size: .83em; margin: 1.5em 0 }
        h6              { font-size: .75em; margin: 1.67em 0 }
        h1, h2, h3, h4,
        h5, h6, b,
        strong          { font-weight: bolder; }
        blockquote      { margin-left: 40px; margin-right: 40px }
        i, cite, em,
        var, address    { font-style: italic }
        pre, tt, code,
        kbd, samp       { font-family: monospace }
        pre             { white-space: normal; }
        button, textarea,
        input, select   { display: inline-block }
        big             { font-size: 1.17em }
        small, sub, sup { font-size: .83em }
        sub             { vertical-align: sub }
        sup             { vertical-align: super }
        table           { border-spacing: 2px; }
        thead, tbody,
        tfoot, tr       { vertical-align: middle }
        td, th          { vertical-align: inherit }
        s, strike, del  { text-decoration: line-through }
        hr              { border: 1px inset; }
        ol, ul, dir,
        menu, dd        { margin-left: 40px }
        ol              { list-style-type: decimal }
        ol ul, ul ol,
        ul ul, ol ol    { margin-top: 0; margin-bottom: 0 }
        ol ul, ul ul    { list-style-type: circle }
        ul ul ul, 
        ol ul ul, 
        ul ol ul        { list-style-type: square }
        u, ins          { text-decoration: underline }
        br:before       { content: ""\A"" }
        :before, :after { white-space: pre-line }
        center          { text-align: center }
        :link, :visited { text-decoration: underline }
        :focus          { outline: thin dotted invert }

        /* Begin bidirectionality settings (do not change) */
        BDO[DIR=""ltr""]  { direction: ltr; unicode-bidi: bidi-override }
        BDO[DIR=""rtl""]  { direction: rtl; unicode-bidi: bidi-override }

        *[DIR=""ltr""]    { direction: ltr; unicode-bidi: embed }
        *[DIR=""rtl""]    { direction: rtl; unicode-bidi: embed }

        @media print {
          h1            { page-break-before: always }
          h1, h2, h3,
          h4, h5, h6    { page-break-after: avoid }
          ul, ol, dl    { page-break-before: avoid }
        }

        /* Not in the specification but necessary */
        a               { color: #0055BB; text-decoration:underline }
        table           { border-color:#dfdfdf; }
        td, th          { border-color:#dfdfdf; overflow: hidden; }
        style, title,
        script, link,
        meta, area,
        base, param     { display:none }
        hr              { border-top-color: #9A9A9A; border-left-color: #9A9A9A; border-bottom-color: #EEEEEE; border-right-color: #EEEEEE; }
        pre             { font-size: 10pt; margin-top: 15px; }
        
        /* This is the background of the HtmlToolTip */
        .htmltooltip {
            border:solid 1px #767676;
            background-color:white;
            background-gradient:#E4E5F0;
            padding: 8px; 
            Font: 9pt Tahoma;
        }
";

        private static string GetStyleSheet(HtmlControl hc) {
            // NOTE this is sample css from HtmlRenderer proj:
            var style_type = GetHtmlStyleType(hc);

            var sb = new StringBuilder(DefaultStyleSheet);
            //string css_str = string.Empty;
            string type_style = string.Empty;
            switch (style_type) {
                case MpHtmlStyleType.Content:
                    string body_wrap = GetWrapText(hc) ? "white-space: normal;  word-break: break-all;" : "white-space: pre;";
                    string code_wrap = GetWrapText(hc) ? "white-space: pre-wrap;" : "white-space: pre;";
                    type_style = string.Format(@"
* {{ margin: 0; padding: 0; }}
body {{ color: {0}; font-size: {1}px; font-family: {2}; {10} }}
p {{ height: 1em; line-height: 1.2em; margin: 0; padding: 0; }}
h6 {{ line-height: 2em; }}
h5 {{ line-height: 2.2em; }}
h4 {{ line-height: 2.4em; }}
h3 {{ line-height: 2.6em; }}
h2 {{ line-height: 2.8em; }}
h1 {{ line-height: 3em; }}
.underline {{ text-decoration: underline; text-underline-offset: -2; }}
.highlight-inactive {{ background-color: {3}; color: {4}; }}
.highlight-active {{ background-color: {5}; color: {6}; }}
a:link {{ text-decoration: none; color: {7}; }}
a:hover {{ text-decoration: underline; color: {8}; text-underline-offset: -2; }}
div.ql-code-block, code.ql-code-block {{ {11} }}
pre, code {{ font-family: {9}; }}",
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor).ToPortableColor().ToHex(true), //0
GetDefaultFontSize(hc), //1
GetDefaultFontFamily(hc).ToCssStringPropValue(), //2
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightInactiveColor).ToPortableColor().ToHex(true), //3
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightInactiveColor).ToPortableColor().ToHex(true).ToContrastForegoundColor(remove_alpha: true), //4
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightActiveColor).ToPortableColor().ToHex(true), //5
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightActiveColor).ToPortableColor().ToHex(true).ToContrastForegoundColor(remove_alpha: true), //6
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeContentLinkColor).ToPortableColor().ToHex(true), //7
Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeContentLinkHoverColor).ToPortableColor().ToHex(true), //8
MpAvPrefViewModel.Instance.DefaultCodeFontFamily.ToCssStringPropValue(), //9
body_wrap, //10
code_wrap //11
);
                    break;
                case MpHtmlStyleType.Tooltip:
                default:
                    type_style = string.Format(
@"* {{ margin: 0; padding: 0; }}
body {{ color: {0}; font-size: {1}px; font-family: {2}; line-height: 1; }}
p {{ margin: 0; height: 1em; line-height: 1; }}
.paste-tooltip-suffix {{ font-style: italic; color: {3}; }}
a:link {{ text-decoration: none; }}
a:hover {{ text-decoration: underline; }}",
GetDefaultHexColor(hc).RemoveHexAlpha(), //0
GetDefaultFontSize(hc), //1
GetDefaultFontFamily(hc).ToCssStringPropValue(), //2
MpSystemColors.gold1.RemoveHexAlpha()
                        );
                    break;
            }

            sb.AppendLine(type_style);

            if (GetSyntaxThemeName(hc) is string theme_name) {
                if(_syntaxStyle.IsDefault() || _syntaxStyle.name != theme_name) {
                    _syntaxStyle.name = theme_name;
                    _syntaxStyle.data = MpAvSyntaxThemeHelpers.ReadThemeText(theme_name);
                }
                sb.AppendLine(_syntaxStyle.data);
            }

            string css_str = sb.ToString();
            return css_str;
        }
    }

}
