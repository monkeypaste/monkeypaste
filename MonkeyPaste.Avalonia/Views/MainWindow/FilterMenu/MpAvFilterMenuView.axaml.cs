using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Mechanism.AvaloniaUI.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvFilterMenuView : MpAvUserControl<MpAvFilterMenuViewModel> {
        List<ToolBar> toolbars = new List<ToolBar>();
        public MpAvFilterMenuView() {
            InitializeComponent();
            MpAvTagTrayViewModel.Instance.PinnedItems.CollectionChanged += PinnedItems_CollectionChanged;
            //Dispatcher.UIThread.Post(async () => {
            //    while (toolbars.Count < 4) {
            //        toolbars = this.GetVisualDescendants<ToolBar>().ToList();
            //        await Task.Delay(100);
            //    }
            //    //foreach (var tb in toolbars) {
            //    //    tb.EffectiveViewportChanged += Tb_EffectiveViewportChanged;
            //    //}
            //});
        }


        private void PinnedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            toolbars.ForEach(x => x.InvalidateAll());
        }

        private void Tb_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            var tb = sender as ToolBar;
            if(tb.Content is Control control) {
                switch(control.Tag as string) {
                    case "Search":
                        MpAvSearchBoxViewModel.Instance.SearchBoxViewWidth = tb.Bounds.Width;
                        break;
                    case "Sort":
                        MpAvClipTileSortViewModel.Instance.ClipTileSortViewWidth = tb.Bounds.Width;
                        break;
                    case "PlayPause":
                        MpAvClipTrayViewModel.Instance.PlayPauseButtonWidth = tb.Bounds.Width;
                        break;
                }
            }

            var tttb = toolbars.FirstOrDefault(x => x.Content is Control control && control.Tag.ToString() == "TagTray");
            if(tttb != null) {
                tttb.Width = MpAvTagTrayViewModel.Instance.TagTrayScreenWidth;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
