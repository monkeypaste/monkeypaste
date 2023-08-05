using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionMessageBodyTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            string key = "PlainTextMessageTemplate";
            if (param is MpAvParameterRequestMessageViewModel prmvm) {
                key = "ParameterRequestMessageTemplate";
            } else if (param is MpAvAnnotationMessageViewModel) {
                key = "AnnotationMessageTemplate";
            }

            if (param is MpAvITransactionNodeViewModel tnvm &&
                tnvm.HostClipTileViewModel.TransactionCollectionViewModel.IsPlainTextView) {
                key = "PlainTextMessageTemplate";
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvViewModelBase;
        }
    }
}
