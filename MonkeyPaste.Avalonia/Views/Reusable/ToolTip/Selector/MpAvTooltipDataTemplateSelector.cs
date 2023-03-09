using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvTooltipDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param == null) {
                return null;
            }
            string key = param.ToString().StartsWith("<") ?
                "HtmlTemplate" : "TextTemplate";
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is string;
        }

    }
}
