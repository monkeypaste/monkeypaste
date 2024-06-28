using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                if(sbivm is MpAvTriggerCollectionViewModel) {
                    // action designer forced as window on mobile
                    return null;
                }
                if (sbivm is not MpAvTagTrayViewModel) {
                    c = new ScrollViewer() {
                        Content = new Viewbox() {
                            Name = "SidebarItemViewbox",
                            Stretch = Stretch.UniformToFill,
                            Margin = new Thickness(10),
                            Child = c
                        }
                    };
                }                
            }
            if(c != null) {
                c.Loaded += C_Loaded;
            }
            return c;
        }

        private async void C_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            // Reset sidebar scroll once cc is loaded/animated
            if(sender is not Control c) {
                return;
            }
            c.Loaded -= C_Loaded;
            while(MpAvSidebarItemCollectionViewModel.Instance.IsAnimating) { await Task.Delay(100); }

            var svl = 
                MpAvMainView.Instance.SelectedSidebarContainerBorder
                .GetVisualDescendants().OfType<ScrollViewer>().ToList();
            svl.AddRange(c.GetVisualDescendants().OfType<ScrollViewer>());
            svl.Distinct().ForEach(x => x.ScrollToHome());
            MpConsole.WriteLine($"Scroll reset on {svl.Distinct().Count()} sv's");
        }

        public bool Match(object data) {
            return data is MpISidebarItemViewModel;
        }
    }
}
