using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpSpanOperations : DependencyObject {
        public static IEnumerable GetInlineSource(DependencyObject obj) {
            return (IEnumerable)obj.GetValue(InlineSourceProperty);
        }

        public static void SetInlineSource(DependencyObject obj, IEnumerable value) {
            obj.SetValue(InlineSourceProperty, value);
        }

        public static readonly DependencyProperty InlineSourceProperty =
            DependencyProperty.RegisterAttached("InlineSource", typeof(IEnumerable), typeof(MpSpanOperations), new UIPropertyMetadata(null, OnInlineSourceChanged));

        private static void OnInlineSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var span = d as Span;
            if (span == null) {
                // It's a demo only. Can work with only spans... 
                return;
            }
            span.Inlines.Clear();

            var inlines = e.NewValue as IEnumerable;
            if (inlines != null) {
                foreach (var inline in inlines) {
                    // We assume only inlines will come in collection:
                    span.Inlines.Add(new Run(inline.ToString()));
                }

            }
        }
    }
}
