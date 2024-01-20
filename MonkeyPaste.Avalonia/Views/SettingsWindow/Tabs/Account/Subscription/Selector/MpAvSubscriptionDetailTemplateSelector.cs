using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvSubscriptionDetailTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpAvSubscriptionItemViewModel sivm) {
                return null;
            }
            string key = $"{sivm.AccountType}Template";
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvSubscriptionItemViewModel;
        }

    }
}
