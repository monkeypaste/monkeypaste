using Avalonia.Controls.Templates;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using Avalonia.Metadata;

namespace MonkeyPaste.Avalonia {
    public class MpAvNotificationTemplateSelector : IDataTemplate {
        // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();


        public IControl Build(object param) {
            string key;
            if(param is MpLoaderNotificationViewModel) {
                key = "LoaderTemplate";
            } else {
                return null;
            }
            // build the control to display
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            // Check if we can accept the provided data
            return data is MpNotificationViewModelBase;
        }
    }
}
