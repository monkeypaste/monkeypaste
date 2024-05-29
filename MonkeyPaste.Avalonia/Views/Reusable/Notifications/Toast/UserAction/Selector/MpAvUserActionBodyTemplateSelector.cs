using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste.Common;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserActionBodyTemplateSelector : IDataTemplate {
        // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();


        Control ITemplate<object, Control>.Build(object param) {
            string key = "TextBodyTemplate";
            if (param is MpAvUserActionNotificationViewModel uanvm) {
                if (uanvm.HasParams) {
                    if(uanvm.Body is MpAvAnalyticItemPresetViewModel) {
                        key = "ExecParamCollectionTemplate";
                    } else if (uanvm.Body is MpAvSettingsFrameViewModel) {
                        key = "PrefParamCollectionTemplate";
                    } else {
                        MpDebug.Break($"Unknown ntf body {uanvm.Body}");
                    }
                } else if (uanvm.ShowTextBox) {
                    key = "TextBoxTemplate";
                } else if (uanvm.ShowBusySpinner || uanvm.ShowProgressSpinner) {
                    key = "BusyTemplate";
                }
                if (key == "TextBodyTemplate" &&
                    uanvm.Body.ToStringOrEmpty().ContainsHtml()) {
                    key = "HtmlBodyTemplate";
                }
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            // Check if we can accept the provided data
            return true;
        }
    }
}
