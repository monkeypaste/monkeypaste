using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpActionItemTemplateSelector : DataTemplateSelector {
        public DataTemplate AnalyzeTemplate { get; set; }
        public DataTemplate ClassifyTemplate { get; set; }
        public DataTemplate TransformTemplate { get; set; } //unimplemented...
        public DataTemplate MacroTemplate { get; set; }
        public DataTemplate TimerTemplate { get; set; }
        public DataTemplate CompareTemplate { get; set; }
        public DataTemplate TriggerTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var mvm = item as MpActionViewModelBase;
            if (mvm == null) {
                return null;
            }
            switch(mvm.ActionType) {
                case MpActionType.Analyze:
                    // TODO figure out differences or remove TransformType
                    return AnalyzeTemplate;
                case MpActionType.Classify:
                    return ClassifyTemplate;
                case MpActionType.Macro:
                    return MacroTemplate;
                case MpActionType.Timer:
                    return TimerTemplate;
                case MpActionType.Compare:
                    return CompareTemplate;
                case MpActionType.Trigger:
                    return TriggerTemplate;
            }
            return null;
        }
    }
}
