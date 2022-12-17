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
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using Pango;

namespace MonkeyPaste.Avalonia {

    public class MpAvActionItemPropertiesTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object item) {

            var aivm = item as MpAvActionViewModelBase;
            if (aivm == null) {
                return null;
            }

            string resourceKeyStr = aivm.ActionType.ToString() + "PropertiesTemplate";
            if (aivm.ActionType == MpActionType.Trigger) {
                resourceKeyStr = (aivm as MpAvTriggerActionViewModelBase).TriggerType.ToString() + "PropertiesTemplate";
            }
            return AvailableTemplates[resourceKeyStr].Build(item);
        }

        public bool Match(object data) {
            return data is MpAvActionViewModelBase;
        }
    }
}
