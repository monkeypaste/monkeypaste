using MonkeyPaste;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Windows.UI.Text;

namespace MpWpfApp {
    public class MpRtbTemplateCollection : DependencyObject {
        public static ObservableCollection<MpTemplateHyperlink> GetTemplateViews(DependencyObject obj) {
            return (ObservableCollection<MpTemplateHyperlink>)obj.GetValue(TemplateViewsProperty);
        }
        public static void SetTemplateViews(DependencyObject obj, ObservableCollection<MpTemplateHyperlink> value) {
            obj.SetValue(TemplateViewsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Hyperlinks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TemplateViewsProperty =
            DependencyProperty.Register(
                "TemplateViews", 
                typeof(ObservableCollection<MpTemplateHyperlink>), 
                typeof(MpRtbTemplateCollection), 
                new PropertyMetadata(
                    new ObservableCollection<MpTemplateHyperlink>()));

        public static MpTemplateCollectionViewModel GetTemplateCollection(DependencyObject obj) {
            return (MpTemplateCollectionViewModel)obj.GetValue(TemplateCollectionProperty);
        }
        public static void SetTemplateCollection(DependencyObject obj, MpTemplateCollectionViewModel value) {
            obj.SetValue(TemplateCollectionProperty, value);
        }
        public static readonly DependencyProperty TemplateCollectionProperty =
          DependencyProperty.RegisterAttached(
            "TemplateCollection",
            typeof(MpTemplateCollectionViewModel),
            typeof(MpRtbTemplateCollection),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    //called when contentItem is loaded only
                    ClearTemplateViews(obj);

                    CreateTemplateViews(obj);
                }
            });

        public static void CreateTemplateViews(DependencyObject obj) {
            var rtb = (RichTextBox)obj;
            var thlcvm = (rtb.DataContext as MpContentItemViewModel).TemplateCollection;
            MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                thlcvm.Templates.Clear();
                if (thlcvm.Parent == null) {
                    return;
                }
                var citl = await MpCopyItemProvider.Instance.GetTemplatesAsync(thlcvm.Parent.CopyItemId);
                var hlc = GetTemplateViews(obj);
                hlc.Clear();
                SetTemplateViews(obj, hlc);
                foreach (var cit in citl) {
                    string templateName = string.Format(
                    @"{0}{1}{2}",
                    MpTemplateCollectionViewModel.TEMPLATE_PREFIX,
                    cit.TemplateName,
                    MpTemplateCollectionViewModel.TEMPLATE_SUFFIX);
                    //find all template ranges and convert to template hyperlinks and register events
                    var trl = MpHelpers.Instance.FindStringRangesFromPosition(rtb.Document.ContentStart, templateName, true);
                    foreach (var tr in trl) {
                        var thl = MpTemplateHyperlink.Create(tr, cit);
                    }
                }
            });
            
        }

        public static void ClearTemplateViews(DependencyObject obj) {
            //called when content is (re)loaded, before data is saved back to model and bore item is pasted 
            var hlc = GetTemplateViews(obj);
            //clear templates to spans
            hlc.ForEach(x => x.Tag = null);
            hlc.ForEach(x => MpTemplateHyperlink.ConvertToSpan(x));
            hlc.Clear();
            SetTemplateViews(obj, hlc);
        }
    }
}