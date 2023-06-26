using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Diagnostics;
namespace MonkeyPaste.Avalonia {


    public class MpAvSidebarItemTemplateSelector : IDataTemplate {
        [Content]

        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param == null) {
                return null;
            }

            var sbivm = param as MpISidebarItemViewModel;
            if (sbivm == null) {
                return null;
            }
            string keyStr = null;
            if (sbivm is MpAvTagTrayViewModel) {
                keyStr = "TagTreeViewTemplate";
            } else if (sbivm is MpAvClipboardHandlerCollectionViewModel) {
                keyStr = "ClipboardHandlerSelectorViewTemplate";
            } else if (sbivm is MpAvAnalyticItemCollectionViewModel) {
                keyStr = "AnalyticItemSelectorViewTemplate";
            } else if (sbivm is MpAvTriggerCollectionViewModel) {
                keyStr = "TriggerActionChooserViewTemplate";
            } else {
                // what's the param?
                MpDebug.Break($"unknown sidebar item type {param?.GetType()}");
                return null;
            }
            return AvailableTemplates[keyStr].Build(param);
        }

        public bool Match(object data) {
            return data is MpISidebarItemViewModel;
        }
    }
}
