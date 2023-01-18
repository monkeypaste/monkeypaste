using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionMessageBodyTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object param) {
            string key = "PlainTextMessageTemplate";
            if (param is MpAvParameterRequestMessageViewModel prmvm) {
                key = "ParameterRequestMessageTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpViewModelBase;
        }
    }
}
