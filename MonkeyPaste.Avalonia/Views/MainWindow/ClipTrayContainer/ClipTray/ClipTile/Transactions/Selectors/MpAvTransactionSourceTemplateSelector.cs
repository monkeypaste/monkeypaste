using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionSourceTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            string key = "PlainTextSourceTemplate";
            if (param is MpAvAnalyzerSourceViewModel) {
                key = "AnalyzerSourceTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvITransactionNodeViewModel;
        }
    }
}
