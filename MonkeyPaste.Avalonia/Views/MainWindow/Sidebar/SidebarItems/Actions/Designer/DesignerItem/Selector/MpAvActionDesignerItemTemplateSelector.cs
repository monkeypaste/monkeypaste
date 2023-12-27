using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;
namespace MonkeyPaste.Avalonia {


    public class MpAvActionDesignerItemTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpAvActionViewModelBase avm) {
                return null;
            }
            string keyStr = $"{avm.DesignerShapeType}Template";
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvActionViewModelBase;
        }
    }
}
