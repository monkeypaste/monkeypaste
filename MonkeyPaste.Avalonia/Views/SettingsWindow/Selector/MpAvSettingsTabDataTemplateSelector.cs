using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsTabDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            string key = null;
            if (param is int tabId) {
                switch (tabId) {
                    case 0:
                        key = "AccountTemplate";
                        break;
                    case 1:
                        key = "PreferencesTemplate";
                        break;
                }
            }
            if (string.IsNullOrEmpty(key)) {
                return null;
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is int;
        }

    }
}
