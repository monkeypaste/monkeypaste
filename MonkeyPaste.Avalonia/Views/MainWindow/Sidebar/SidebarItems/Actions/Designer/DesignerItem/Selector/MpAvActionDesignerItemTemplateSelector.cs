using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Wpf;
namespace MonkeyPaste.Avalonia {


    public class MpAvActionDesignerItemTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object param) {
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
