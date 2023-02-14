using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;
namespace MonkeyPaste.Avalonia {


    public class MpAvActionDesignerItemTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param == null) {
                return null;
            }

            var avmb = param as MpAvActionViewModelBase;
            if (avmb == null) {
                return null;
            }
            string keyStr = string.Format(@"{0}DesignerTemplate", avmb.ActionType.ToString());
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvActionViewModelBase;
        }
    }
}
