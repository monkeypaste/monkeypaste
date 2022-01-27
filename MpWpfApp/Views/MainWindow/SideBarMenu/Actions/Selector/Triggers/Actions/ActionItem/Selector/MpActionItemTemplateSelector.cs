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
        public HierarchicalDataTemplate AnalyzeTemplate { get; set; }
        public HierarchicalDataTemplate ClassifyTemplate { get; set; }
        public HierarchicalDataTemplate TransformTemplate { get; set; } //unimplemented...
        public HierarchicalDataTemplate MacroTemplate { get; set; }
        public HierarchicalDataTemplate TimerTemplate { get; set; }
        public HierarchicalDataTemplate CompareTemplate { get; set; }
        public HierarchicalDataTemplate TriggerTemplate { get; set; }

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
