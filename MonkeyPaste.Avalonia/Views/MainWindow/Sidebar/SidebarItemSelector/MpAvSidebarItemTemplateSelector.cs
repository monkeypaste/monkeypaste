using System;
using System.Collections.Generic;
using System.Diagnostics;
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


    public class MpAvSidebarItemTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public IControl Build(object param) {
            if (param == null) {
                return null;
            }

            var sbivm = param as MpISidebarItemViewModel;
            if (sbivm == null) {
                return null;
            }
            string keyStr = null;
            if(sbivm is MpAvTagTrayViewModel) {
                keyStr = "TagTreeViewTemplate";
            } else if (sbivm is MpAvClipboardHandlerCollectionViewModel) {
                keyStr = "ClipboardHandlerSelectorViewTemplate";
            } else if (sbivm is MpAvAnalyticItemCollectionViewModel) {
                keyStr = "AnalyticItemSelectorViewTemplate";
            } else if (sbivm is MpAvTriggerCollectionViewModel) {
                keyStr = "TriggerActionChooserViewTemplate";
            } else {
                // what's the param?
                Debugger.Break();
                return null;
            }
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpISidebarItemViewModel;
        }
    }
}
