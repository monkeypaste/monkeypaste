using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpDocumentRtfExtension : DependencyObject {
        public static string GetDocumentRtf(DependencyObject obj) {
            return (string)obj.GetValue(DocumentRtfProperty);
        }
        public static void SetDocumentRtf(DependencyObject obj, string value) {
            obj.SetValue(DocumentRtfProperty, value);
        }
        public static readonly DependencyProperty DocumentRtfProperty =
          DependencyProperty.RegisterAttached(
            "DocumentRtf",
            typeof(string),
            typeof(MpDocumentRtfExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (string.IsNullOrEmpty((string)e.NewValue)) {
                        return;
                    }
                    var rtb = (RichTextBox)obj;
                    //rtb.SetRtf((string)e.NewValue);
                    rtb.Document = ((string)e.NewValue).ToFlowDocument();
                }
            });
    }
}