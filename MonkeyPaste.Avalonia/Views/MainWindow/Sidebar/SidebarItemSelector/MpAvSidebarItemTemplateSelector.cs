using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var c = AvailableTemplates[keyStr].Build(param);
#if MOBILE_OR_WINDOWED
            if(sbivm is MpAvTagTrayViewModel) {
                return c;
            }
            return new ScrollViewer() {
                Content = new Viewbox() {
                    Name = "SidebarItemViewbox",
                    Stretch = Stretch.UniformToFill,
                    Margin = new Thickness(10),
                    Child = c
                }
            };
            //return c;
#else
            return c;
#endif
        }

        public bool Match(object data) {
            return data is MpISidebarItemViewModel;
        }
    }
}
