using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvNotificationBodyTemplateSelector : IDataTemplate {
        // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();


        Control ITemplate<object, Control>.Build(object param) {
            string key;
            if (param is MpAvClipTileViewModel) {
                key = "RichHtmlTemplate";
            } else {
                key = "PlainTextTemplate";
            }
            // build the control to display
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            // Check if we can accept the provided data
            return true;// data is MpNotificationViewModelBase;
        }
    }
}
