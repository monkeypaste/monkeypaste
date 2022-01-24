using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpMatchActionCellTemplateSelector : DataTemplateSelector {
        public DataTemplate CompareTemplate { get; set; }
        public DataTemplate AnalyzeTemplate { get; set; }
        public DataTemplate ClassifyTemplate { get; set; }
        public DataTemplate TransformTemplate { get; set; }
        public DataTemplate NoneTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var mvm = item as MpMatcherViewModel;
            if (mvm == null) {
                return NoneTemplate;
            }
            switch(mvm.TriggerActionType) {
                case MonkeyPaste.MpMatcherActionType.Analyze:
                    return AnalyzeTemplate;
                case MonkeyPaste.MpMatcherActionType.Compare:
                    return CompareTemplate;
                case MonkeyPaste.MpMatcherActionType.Classify:
                    return ClassifyTemplate;
                case MonkeyPaste.MpMatcherActionType.Transform:
                    return TransformTemplate;
            }
            return NoneTemplate;
        }
    }
}
