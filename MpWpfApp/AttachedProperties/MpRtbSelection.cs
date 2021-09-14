using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpRtbSelection : DependencyObject {
        public static TextRange GetRtbSelection(DependencyObject obj) {
            return (TextRange)obj.GetValue(RtbSelectionProperty);
        }
        public static void SetRtbSelection(DependencyObject obj, TextRange value) {
            obj.SetValue(RtbSelectionProperty, value);
        }
        public static readonly DependencyProperty RtbSelectionProperty =
          DependencyProperty.RegisterAttached(
            "RtbSelection",
            typeof(TextRange),
            typeof(MpRtbSelection),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    var rtb = (RichTextBox)obj;
                    if(e.NewValue == null) {
                        rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
                        return;
                    }
                    var rtbvm = rtb.DataContext as MpRtbItemViewModel;
                    rtb.SelectionChanged += (s, e1) => {
                        rtbvm.RtbSelectionRange = rtb.Selection;
                    };
                }
            });
    }
}