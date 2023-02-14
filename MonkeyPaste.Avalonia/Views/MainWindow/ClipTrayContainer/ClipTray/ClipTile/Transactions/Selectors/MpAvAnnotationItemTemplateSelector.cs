using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnnotationItemTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        //Control ITemplate<object, Control>.Build(object param) {
        //    string key = "PlainTextMessageTemplate";
        //    if (param is MpAvParameterRequestMessageViewModel prmvm) {
        //        key = "ParameterRequestMessageTemplate";
        //    }

        //    return AvailableTemplates[key].Build(param);
        //}

        public bool Match(object data) {
            return data is MpViewModelBase;
        }

        Control ITemplate<object, Control>.Build(object param) {
            string key = "PlainTextMessageTemplate";
            if (param is MpAvParameterRequestMessageViewModel prmvm) {
                key = "ParameterRequestMessageTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }
    }
}
