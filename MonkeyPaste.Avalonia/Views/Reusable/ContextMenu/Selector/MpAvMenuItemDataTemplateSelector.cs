using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    internal class MpAvMenuItemDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public string GetTemplateName(object param) {
            string keyStr = string.Empty;
            if (param is MpMenuItemViewModel cmivm) {
                keyStr = cmivm.ContentTemplateName;
            }
            return keyStr;
        }
        Control ITemplate<object, Control>.Build(object param) {
            string keyStr = GetTemplateName(param);
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpMenuItemViewModel;
        }
    }
}
