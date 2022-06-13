using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpRichTextFormatExtension : DependencyObject {
        #region RichTextFormat DependencyProperty

        public static MpRichTextFormatInfoFormat GetRichTextFormat(DependencyObject obj) {
            return (MpRichTextFormatInfoFormat)obj.GetValue(RichTextFormatProperty);
        }

        public static void SetRichTextFormat(DependencyObject obj, MpRichTextFormatInfoFormat value) {
            obj.SetValue(RichTextFormatProperty, value);
        }

        public static readonly DependencyProperty RichTextFormatProperty =
            DependencyProperty.RegisterAttached(
                "RichTextFormat",
                typeof(MpRichTextFormatInfoFormat),
                typeof(MpRichTextFormatExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region IsEnabled DependencyProperty

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled", 
                typeof(bool),
                typeof(MpRichTextFormatExtension),
                new PropertyMetadata() {
                    PropertyChangedCallback = (s, e) => {

                        if ((bool)e.NewValue) {
                            if (s is TextBlock tb) {
                                if (tb.IsLoaded) {
                                    TeOrTb_Loaded(s, null);
                                } else {
                                    tb.Loaded += TeOrTb_Loaded;
                                }
                            } else if (s is TextElement te) {
                                if (te.IsLoaded) {
                                    TeOrTb_Loaded(s, null);
                                } else {
                                    te.Loaded += TeOrTb_Loaded;
                                }
                            }
                        } else {
                            TeOrTb_Unloaded(s, null);
                        }
                    }
                });

        #endregion

        #region Event Handlers

        private static void TeOrTb_Loaded(object s, RoutedEventArgs e) {
            if (s is TextBlock tb) {
                if (e == null) {
                    tb.Loaded += TeOrTb_Loaded;
                }
                tb.Unloaded += TeOrTb_Unloaded;
            } else if (s is TextElement te) {
                if (e == null) {
                    te.Loaded += TeOrTb_Loaded;
                }
                te.Unloaded += TeOrTb_Unloaded;
            }
            if(s is DependencyObject dpo) {
                SetFormat(dpo);
            }
            
        }

        private static void TeOrTb_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is TextElement te) {
                te.Loaded -= TeOrTb_Loaded;
                te.Unloaded -= TeOrTb_Unloaded;
            } else if (sender is TextBlock tb) {
                tb.Loaded -= TeOrTb_Loaded;
                tb.Unloaded -= TeOrTb_Unloaded;
            }

        }

        #endregion

        private static void SetFormat(DependencyObject dpo) {
            var format = GetRichTextFormat(dpo);
            if (format == null) {
                return;
            }
            if(dpo is TextElement te) {
                if (te is Inline i) {
                    var inlineFormat = format.inlineFormat;
                    if (inlineFormat == null) {
                        return;
                    }
                    i.Background = inlineFormat.background.ToSolidColorBrush();
                    i.Foreground = inlineFormat.color.ToSolidColorBrush();
                    i.FontStyle = inlineFormat.italic ? FontStyles.Italic : FontStyles.Normal;
                    i.FontWeight = inlineFormat.bold ? FontWeights.Bold : FontWeights.Normal;
                    i.TextDecorations = inlineFormat.underline ? TextDecorations.Underline : i.TextDecorations;
                    i.FontFamily = string.IsNullOrEmpty(inlineFormat.font) ?
                                        i.FontFamily :
                                        new FontFamily(inlineFormat.font);
                    i.FontSize = inlineFormat.size;
                    i.BaselineAlignment = string.IsNullOrEmpty(inlineFormat.script) ?
                                            i.BaselineAlignment :
                                            inlineFormat.script.ToEnum<BaselineAlignment>();
                }
            } else if (dpo is TextBlock tb) {
                var inlineFormat = format.inlineFormat;
                if (inlineFormat == null) {
                    return;
                }
                tb.Background = inlineFormat.background.ToSolidColorBrush();
                tb.Foreground = inlineFormat.color.ToSolidColorBrush();
                tb.FontStyle = inlineFormat.italic ? FontStyles.Italic : FontStyles.Normal;
                tb.FontWeight = inlineFormat.bold ? FontWeights.Bold : FontWeights.Normal;
                tb.TextDecorations = inlineFormat.underline ? TextDecorations.Underline : tb.TextDecorations;
                tb.FontFamily = string.IsNullOrEmpty(inlineFormat.font) ?
                                    tb.FontFamily :
                                    new FontFamily(inlineFormat.font);
                tb.FontSize = inlineFormat.size;
                //tb.BaselineOffset = string.IsNullOrEmpty(inlineFormat.script) ?
                //                        tb.BaselineAlignment :
                //                        inlineFormat.script.ToEnum<BaselineAlignment>();
            }
        }
    }
}
