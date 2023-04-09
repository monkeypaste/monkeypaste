using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionNodeDetailTemplateSelector : IDataTemplate {
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
            //return data is MpViewModelBase;
            return true;
        }

        Control ITemplate<object, Control>.Build(object param) {
            string key = "EmptyDetailTemplate";
            if (param is MpAvAnnotationItemViewModel iaivm) {
                key = "AnnotationItemDetailTemplate";
            }

            return AvailableTemplates[key].Build(param);
        }
    }
}
