using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpDocumentRtfExtension : DependencyObject {
        public static FlowDocument GetDocumentRtf(DependencyObject obj) {
            return (FlowDocument)obj.GetValue(DocumentRtfProperty);
        }
        public static void SetDocumentRtf(DependencyObject obj, FlowDocument value) {
            obj.SetValue(DocumentRtfProperty, value);
        }
        public static readonly DependencyProperty DocumentRtfProperty =
          DependencyProperty.RegisterAttached(
            "DocumentRtf",
            typeof(FlowDocument),
            typeof(MpDocumentRtfExtension),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (obj, e) => {
                    var richTextBox = (RichTextBox)obj;
                    richTextBox.Document = e.NewValue as FlowDocument;
                }
            });
    }
}
