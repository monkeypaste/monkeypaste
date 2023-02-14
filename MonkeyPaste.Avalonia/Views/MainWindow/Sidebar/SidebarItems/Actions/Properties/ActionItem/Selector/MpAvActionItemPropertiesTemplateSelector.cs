using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public class MpAvActionItemPropertiesTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object item) {

            var aivm = item as MpAvActionViewModelBase;
            if (aivm == null) {
                return null;
            }

            string resourceKeyStr = aivm.ActionType.ToString() + "PropertiesTemplate";
            if (aivm.ActionType == MpActionType.Trigger) {
                resourceKeyStr = (aivm as MpAvTriggerActionViewModelBase).TriggerType.ToString() + "PropertiesTemplate";
            }
            return AvailableTemplates[resourceKeyStr].Build(item);
        }

        public bool Match(object data) {
            return data is MpAvActionViewModelBase;
        }
    }
}
