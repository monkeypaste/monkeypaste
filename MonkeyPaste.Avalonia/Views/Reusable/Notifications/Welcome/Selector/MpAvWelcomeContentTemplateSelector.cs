using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeContentTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is int wptIdx && ((MpWelcomePageType)wptIdx) is MpWelcomePageType wpt &&
                wpt == MpWelcomePageType.DbPassword) {
                if (AvailableTemplates["DbPasswordTemplate"].Build(param) is Control pc) {
                    pc.DataContext = MpAvWelcomeNotificationViewModel.Instance;
                    return pc;
                }
            }
            if (AvailableTemplates["OptionsTemplate"].Build(param) is Control c) {
                c.DataContext = MpAvWelcomeNotificationViewModel.Instance.CurOptGroupViewModel;
                return c;
            }
            return null;
        }

        public bool Match(object data) {
            return data is int;
        }

    }
}
