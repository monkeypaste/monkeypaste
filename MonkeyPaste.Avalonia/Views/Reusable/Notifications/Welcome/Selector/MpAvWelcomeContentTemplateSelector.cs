using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeContentTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not int wptIdx) {
                return null;
            }
            string key;
            object dc;
            switch ((MpWelcomePageType)wptIdx) {
                case MpWelcomePageType.DbPassword:
                    key = "DbPasswordTemplate";
                    dc = MpAvWelcomeNotificationViewModel.Instance;
                    break;
                case MpWelcomePageType.DragToOpen:
                    key = "DragToOpenTemplate";
                    dc = MpAvWelcomeNotificationViewModel.Instance.CurOptGroupViewModel;
                    break;
                default:
                    key = "OptionsTemplate";
                    dc = MpAvWelcomeNotificationViewModel.Instance.CurOptGroupViewModel;
                    break;
            }
            if (AvailableTemplates[key].Build(param) is Control c) {
                c.DataContext = dc;
                return c;
            }
            return null;
        }

        public bool Match(object data) {
            return data is int;
        }

    }
}
