using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTextTemplateRtfExtension : DependencyObject {
        #region Private Variables

        private static readonly double _EDITOR_DEFAULT_WIDTH = 900;

        private static double _readOnlyWidth;

        #endregion


        #region IsSelected

        public static bool GetIsSelected(DependencyObject obj) {
            return (bool)obj.GetValue(IsSelectedProperty);
        }
        public static void SetIsSelected(DependencyObject obj, bool value) {
            obj.SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(MpTextTemplateRtfExtension),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region IsContentReadOnly

        public static bool GetIsContentReadOnly(DependencyObject obj) {
            return (bool)obj.GetValue(IsContentReadOnlyProperty);
        }
        public static void SetIsContentReadOnly(DependencyObject obj, bool value) {
            obj.SetValue(IsContentReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsContentReadOnlyProperty =
          DependencyProperty.RegisterAttached(
            "IsContentReadOnly",
            typeof(bool),
            typeof(MpTextTemplateRtfExtension),
            new FrameworkPropertyMetadata() {
                PropertyChangedCallback = async (s, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    var fe = s as FrameworkElement;
                    bool isReadOnly = (bool)e.NewValue;
                    if (isReadOnly) {
                        EnableReadOnly(fe);
                    } else {
                        DisableReadOnly(fe);
                    }
                }
            });

        private static void EnableReadOnly(FrameworkElement fe) {
            if (fe.DataContext is MpContentItemViewModel civm) {
                var rb = fe.GetVisualAncestor<MpResizeBehavior>();
                if (rb != null) {
                    rb.Resize(_readOnlyWidth - rb.AssociatedObjectRef.ActualWidth, 0);
                }

                //MpMasterTemplateModelCollection.Update(qrm.updatedAllAvailableTextTemplates, qrm.userDeletedTemplateGuids).FireAndForgetSafeAsync(civm);
            }
        }

        private static void DisableReadOnly(FrameworkElement fe) {
            if (fe.DataContext is MpContentItemViewModel civm) {
                var rb = fe.GetVisualAncestor<MpResizeBehavior>();
                if (rb != null) {
                    _readOnlyWidth = rb.AssociatedObjectRef.ActualWidth;
                    if (rb.AssociatedObjectRef.ActualWidth < _EDITOR_DEFAULT_WIDTH) {
                        rb.Resize(_EDITOR_DEFAULT_WIDTH - rb.AssociatedObjectRef.ActualWidth, 0);
                    }
                    MpIsFocusedExtension.SetIsFocused(fe, true);
                }
            }
        }

        #endregion

        #region DocumentRtf

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
            typeof(MpTextTemplateRtfExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback =  (obj, e) => {
                    string rtf = string.Empty;
                    if (string.IsNullOrEmpty((string)e.NewValue)) {
                        rtf = string.Empty;
                    } else {
                        rtf = (string)e.NewValue;
                    }
                    var fe = (FrameworkElement)obj;
                    var fd = ((string)e.NewValue).ToFlowDocument(out Size docSize);

                    if (fe.DataContext is MpContentItemViewModel civm) {
                        if (fe is RichTextBox rtb) {
                            civm.UnformattedContentSize = docSize;
                            rtb.Document = fd;
                            rtb.FitDocToRtb();
                        } else if (fe is FlowDocumentPageViewer fdr) {
                            fdr.Document = fd;
                            fdr.UpdateLayout();
                        } else if (fe is FlowDocumentScrollViewer fdsv) {
                            fdsv.Document = fd;
                            fdsv.UpdateLayout();
                        }
                    }
                }
            });

        #endregion
    }
}