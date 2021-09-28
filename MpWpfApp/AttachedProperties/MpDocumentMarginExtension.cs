using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpDocumentMarginExtension : DependencyObject {
        
        public static double GetDocumentMargin(DependencyObject obj) {
            return (double)obj.GetValue(DocumentMarginProperty);
        }
        public static void SetDocumentMargin(DependencyObject obj, double value) {
            obj.SetValue(DocumentMarginProperty, value);
        }
        public static readonly DependencyProperty DocumentMarginProperty =
          DependencyProperty.RegisterAttached(
            "DocumentMargin",
            typeof(double),
            typeof(MpDocumentMarginExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    var rtb = (RichTextBox)obj;
                    double marginOffset = ((double)e.NewValue);
                    var ds = rtb.Document.GetDocumentSize();
                    rtb.Document.PageWidth = marginOffset;       
                }
            });
    }
}