using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;
namespace MonkeyPaste.Avalonia {


    public class MpAvActionPropertyIconTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param == null) {
                return null;
            }
            string keyStr = null;
            if (param is MpAvActionViewModelBase avm) {
                if (avm.IsValid) {
                    keyStr = param is MpAvTriggerActionViewModelBase ? "TriggerTemplate" : "ActionTemplate";
                } else {
                    keyStr = "ErrorTemplate";
                }
            }
            if (string.IsNullOrEmpty(keyStr)) {
                return null;
            }
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvActionViewModelBase;
        }
    }
}
