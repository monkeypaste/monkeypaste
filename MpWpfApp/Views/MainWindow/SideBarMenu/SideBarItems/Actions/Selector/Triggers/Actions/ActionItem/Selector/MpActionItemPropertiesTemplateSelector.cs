using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpActionItemPropertiesTemplateSelector : DataTemplateSelector {
        public DataTemplate EmptyPropertiesTemplate { get; set; }
        public DataTemplate TriggerPropertiesTemplate { get; set; }
        public DataTemplate ComparePropertiesTemplate { get; set; }
        public DataTemplate AnalyzePropertiesTemplate { get; set; }
        public DataTemplate ClassifyPropertiesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var fe = container as FrameworkElement;
            if (item == null || fe == null) {
                return null;
            }

            var aivm = item as MpActionViewModelBase;
            if (aivm == null) {
                return null;
            }

            if (fe.Name == "RootActionPropertyContentControl" && aivm.ActionType != MpActionType.Trigger) {
                return EmptyPropertiesTemplate;
            }

            switch (aivm.ActionType) {
                case MpActionType.Trigger:
                    return TriggerPropertiesTemplate;
                case MpActionType.Compare:
                    return ComparePropertiesTemplate;
                case MpActionType.Analyze:
                    return AnalyzePropertiesTemplate;
                case MpActionType.Classify:
                    return ClassifyPropertiesTemplate;
                //case MpActionType.None:
                //    return EmptyPropertiesTemplate;
            }
            return null;
        }
    }
}
