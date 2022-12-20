

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using MonoMac.ImageKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
            //var outer_designer_border = this.FindControl<Border>("ActionDesignerOuterContainerBorder");
            //outer_designer_border.GetObservable(Border.IsVisibleProperty).Subscribe(value => OnAcionDesignerIsVisibleChanged());
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAcionDesignerIsVisibleChanged() {
            var tsvcg = this.FindControl<Grid>("TriggerSelectorViewContainerGrid");
            var outer_designer_border = this.FindControl<Border>("ActionDesignerOuterContainerBorder");
            if(outer_designer_border.IsVisible) {
                // ensure defaultsidebar width is set for designer on initial show

                Dispatcher.UIThread.Post(async () => {
                    var inner_designer_border = this.FindControl<Border>("ActionDesignerInnerContainerBorder");
                    while(inner_designer_border.DataContext == null) {
                        // datacontext won't be available on initial action sidebar show
                        await Task.Delay(100);
                    }
                    if (inner_designer_border.DataContext is MpAvTriggerActionViewModelBase tavmb) {
                        //inner_designer_border.GetVisualDescendant<MpAvActionDesignerView>().
                        //tsvcg.ColumnDefinitions[1].Width = new GridLength(tavmb.DefaultSidebarWidth, GridUnitType.Pixel);
                    } else {
                        Debugger.Break();
                    }
                });
                
            } else {
                // collapse designer/splitter when none selected
                tsvcg.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
            }            

        }
    }
}
