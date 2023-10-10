using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountPurchaseTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not bool showPanel) {
                return null;
            }
            string key = showPanel ? "SubscriptionPanelTemplate" : "ShowButtonTemplate";
            if (AvailableTemplates[key].Build(param) is Control c) {
                c.DataContext = MpAvAccountViewModel.Instance;
                return c;
            }
            return null;
        }

        public bool Match(object data) {
            return true;
        }

    }
}
