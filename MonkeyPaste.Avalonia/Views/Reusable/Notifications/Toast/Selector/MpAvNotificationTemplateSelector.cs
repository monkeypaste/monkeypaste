using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvNotificationTemplateSelector : IDataTemplate {
        // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();


        Control ITemplate<object, Control>.Build(object param) {
            string key;
            if (param is MpAvLoaderNotificationViewModel) {
                key = "LoaderTemplate";
            } else if (param is MpAvMessageNotificationViewModel) {
                key = "MessageTemplate";
            } else if (param is MpAvUserActionNotificationViewModel) {
                key = "UserActionTemplate";
            } else {
                throw new Exception("Unknown notification: " + param);
            }
            // build the control to display
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            // Check if we can accept the provided data
            return data is MpAvNotificationViewModelBase;
        }
    }
}
