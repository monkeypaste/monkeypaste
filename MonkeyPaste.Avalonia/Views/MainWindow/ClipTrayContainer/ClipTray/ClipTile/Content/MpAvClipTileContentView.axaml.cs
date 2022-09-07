using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Input;
using System.Diagnostics;
using System.Security.Cryptography;
using Avalonia.Controls.Primitives;
using MonkeyPaste.Avalonia.Behaviors._Factory;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : MpAvUserControl<MpAvClipTileViewModel>, MpAvIDragDataHost {
        public MpAvContentViewDropBehavior ContentViewDropBehavior { get;  set; }
        public MpAvContentHighlightBehavior HighlightBehavior { get;  set; }


        public MpAvIContentView ContentView { get; private set; }

        #region MpAvIDragDataHost Implementation

        bool MpAvIDragDataHost.IsDragValid(MpPoint host_mp) {
            // check pointer selection/range intersection here

            return true;
        }

        async Task<IDataObject> MpAvIDragDataHost.GetDragDataObjectAsync() {
            await Task.Delay(1);

            DataObject avdo = new DataObject();
            // setup internal data format
            avdo.Set(MpAvDataObjectHelper.CLIP_TILE_DATA_FORMAT, BindingContext);

            return avdo;
        }

        void MpAvIDragDataHost.DragBegin() {
            if(BindingContext == null) {
                Debugger.Break();
            }
            BindingContext.IsItemDragging = true;
        }

        void MpAvIDragDataHost.DragEnd() {
            if (BindingContext == null) {
                Debugger.Break();
            }
            BindingContext.IsItemDragging = false;
        }
        #endregion

        public MpAvClipTileContentView() {
            InitializeComponent();
            this.AddHandler(Control.PointerPressedEvent, MpAvClipTileContentView_PointerPressed, RoutingStrategies.Tunnel);

            var cc = this.FindControl<ContentControl>("ClipTileContentControl");
            cc.AttachedToVisualTree += Cc_AttachedToVisualTree;
        }

        public void UpdateAdorners() {
            var al = AdornerLayer.GetAdornerLayer(this);
            if(al == null) {
                return;
            }
            al.Children.ForEach(x => x.InvalidateVisual());
        }


        private void Cc_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var cc = sender as ContentControl;
            var actual_content_control = cc.Content as Control;
            ContentView = actual_content_control as MpAvIContentView;
            MpAvViewBehaviorFactory.BuildAllViewBehaviors(this, actual_content_control);
        }

        private async void MpAvClipTileContentView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if(!e.IsLeftPress(sender as IVisual)) {
                e.Handled = false;
                return;
            }

            if (e.ClickCount >= 2 &&
                BindingContext.IsContentReadOnly &&
                !BindingContext.IsSubSelectionEnabled) {
                BindingContext.IsSubSelectionEnabled = true;
                MpPlatformWrapper.Services.Cursor.SetCursor(sender as InputElement, MpCursorType.IBeam);
                UpdateAdorners();
                return;
            }
            if (!BindingContext.IsTitleReadOnly ||
                !BindingContext.IsContentReadOnly ||
                 BindingContext.Parent.IsAnyResizing ||
                 BindingContext.Parent.CanAnyResize) { 
                e.Handled = false;
                return;
            }
            if (BindingContext.IsSubSelectionEnabled) {
                // NOTE only check for drag when there is selected text AND
                // drag is from somewhere in the selection range.
                // If mouse down isn't in selection range reset selection to down position

                if (ContentView.Selection.IsEmpty) {
                    e.Handled = false;
                    return;
                }
                bool isPressOnSelection = await ContentView.Selection.IsPointInRangeAsync(e.GetPosition(ContentView.Document.Owner).ToPortablePoint());
                if (!isPressOnSelection) {
                    //var mptp = Rtb.GetPositionFromPoint(e.GetPosition(Rtb),true);
                    //Rtb.Selection.Select(mptp, mptp);
                    e.Handled = false;
                    return;
                }
            }

            MpAvDragDropManager.StartDragCheck(BindingContext);

            e.Handled = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
