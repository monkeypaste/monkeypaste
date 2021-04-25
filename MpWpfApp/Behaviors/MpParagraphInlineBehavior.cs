using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpParagraphInlineBehavior : DependencyObject {
        public static readonly DependencyProperty TemplateResourceNameProperty =
            DependencyProperty.RegisterAttached("TemplateResourceName",
                                                typeof(string),
                                                typeof(MpParagraphInlineBehavior),
                                                new UIPropertyMetadata(null, OnParagraphInlineChanged));
        public static string GetTemplateResourceName(DependencyObject obj) {
            return (string)obj.GetValue(TemplateResourceNameProperty);
        }
        public static void SetTemplateResourceName(DependencyObject obj, string value) {
            obj.SetValue(TemplateResourceNameProperty, value);
        }

        public static readonly DependencyProperty ParagraphInlineSourceProperty =
            DependencyProperty.RegisterAttached("ParagraphInlineSource",
                                                typeof(IEnumerable),
                                                typeof(MpParagraphInlineBehavior),
                                                new UIPropertyMetadata(null, OnParagraphInlineChanged));
        public static IEnumerable GetParagraphInlineSource(DependencyObject obj) {
            return (IEnumerable)obj.GetValue(ParagraphInlineSourceProperty);
        }
        public static void SetParagraphInlineSource(DependencyObject obj, IEnumerable value) {
            obj.SetValue(ParagraphInlineSourceProperty, value);
        }

        private static void OnParagraphInlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Paragraph paragraph = d as Paragraph;
            IEnumerable inlines = MpParagraphInlineBehavior.GetParagraphInlineSource(paragraph);
            string templateName = MpParagraphInlineBehavior.GetTemplateResourceName(paragraph);
            if (inlines != null && templateName != null) {
                paragraph.Inlines.Clear();
                foreach (var inline in inlines) {
                    ArrayList templateList = paragraph.Tag as ArrayList;
                    Span span = new Span();
                    span.DataContext = inline;
                    foreach (var templateInline in templateList) {
                        span.Inlines.Add(templateInline as Inline);
                    }
                    paragraph.Inlines.Add(span);
                }
            }
        }
    }
}
