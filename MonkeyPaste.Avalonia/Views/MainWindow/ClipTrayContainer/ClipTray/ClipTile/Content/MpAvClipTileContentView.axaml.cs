using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Input;
using System.Diagnostics;
using System.Security.Cryptography;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using System.Linq;
using MonkeyPaste.Common.Utils.Extensions;
using Avalonia.Threading;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : 
        MpAvUserControl<MpAvClipTileViewModel>, 
        MpAvIDragHost,
        MpAvIDropHost {
        private bool _wasSubSelectionEnabledBeforeDragOver { get; set; } = false;

        public MpAvIContentView ContentView { get; private set; }

        #region MpAvIDragHost Implementation

        bool MpAvIDragHost.CanDrag => ContentView != null && ContentView.CanDrag;

        bool MpAvIDragHost.IsDragValid(MpPoint host_mp) {
            // check pointer selection/range intersection here
            if(ContentView == null || !ContentView.CanDrag) {
                return false;
            }
            return true;
        }

        async Task<IDataObject> MpAvIDragHost.GetDragDataObjectAsync(bool fillTemplates) {
            if(BindingContext == null) {
                return null;
            }
            var mpavdo = await BindingContext.ConvertToDataObject(fillTemplates);

            var avdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformClipboardDataObject(mpavdo) as DataObject;

            bool is_all_selected = false;
            if (BindingContext.IsSubSelectionEnabled) {
                is_all_selected = ContentView.Selection.Start.Offset == ContentView.Document.ContentStart.Offset &&
                                    ContentView.Selection.End.Offset == ContentView.Document.ContentEnd.Offset;
            } else {
                is_all_selected = true;
            }
            if (is_all_selected) {
                // only attach internal data format for entire tile
                avdo.Set(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, BindingContext);
            }
            return avdo;
        }

        void MpAvIDragHost.DragBegin() {
            if(BindingContext == null) {
                Debugger.Break();
            }
            BindingContext.IsItemDragging = true;
        }

        void MpAvIDragHost.DragEnd() {
            if (BindingContext == null) {
                Debugger.Break();
            }
            BindingContext.IsItemDragging = false;
        }
        #endregion

        #region MpAvIDropHost Implementation


        bool MpAvIDropHost.IsDropEnabled => true;

        bool MpAvIDropHost.IsDropValid(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) {
            //if(IsDragDataSelf(avdo)) {
            //    return false;
            //}
            if (!BindingContext.IsSubSelectionEnabled) {
                return true;
            }
                bool isValid = MpPortableDataFormats.RegisteredFormats.Any(x => avdo.GetDataFormats().Contains(x));
            return isValid;
        }

        void MpAvIDropHost.DragOver(MpPoint host_mp, IDataObject avdo, DragDropEffects dragEffects) {
            MpConsole.WriteLine($"[Tile '{BindingContext.CopyItemTitle}' DragOver data count: {avdo.GetDataFormats().Count()}]");
            if (dragEffects == DragDropEffects.None) {
                return;
            }
            if(!BindingContext.IsSubSelectionEnabled) {
                // should only be on first drag over
                if(ContentView is MpAvCefNetWebView wv) {
                    wv.PointerLeave += Cv_PointerLeave;
                    
                    Dispatcher.UIThread.Post( () => {
                        var hdobjMsg = new MpQuillDragDropDataObjectMessage() {
                            items = avdo.GetDataFormats()
                                        .Where(x => MpPortableDataFormats.RegisteredFormats.Contains(x))
                                        .Select(x =>
                                            new MpQuillDragDropDataObjectItemFragment() {
                                                format = x,
                                                data = x != MpPortableDataFormats.Html ? avdo.Get(x) as string : (avdo.Get(x) as byte[]).ToBase64String()
                                            }).ToList()
                        };
                        string msgStr = hdobjMsg.SerializeJsonObjectToBase64();
                        wv.ExecuteJavascript($"setHostDataObject_ext('{msgStr}')");
                    });
                }
                BindingContext.IsSubSelectionEnabled = true;
            }
        }

        private void Cv_PointerLeave(object sender, PointerEventArgs e) {
            
            if (ContentView is Control cv) {
                cv.PointerLeave -= Cv_PointerLeave;
                BindingContext.IsSubSelectionEnabled = false;
            }
        }

        void MpAvIDropHost.DragLeave() {
            MpConsole.WriteLine($"[Tile '{BindingContext.CopyItemTitle}]' DragLeave]");
            
            if(BindingContext.IsItemDragging || this.IsPointerOver) {
                return;
            }


        }

        async Task<DragDropEffects> MpAvIDropHost.DropDataObjectAsync(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) { 
            MpConsole.WriteLine($"[Tile '{BindingContext.CopyItemTitle}]' Drop]");
            await Task.Delay(1);
            // editor should handle drop internally
            return dragEffects;
        }

        #region Drag Helpers

        private bool IsDragDataSelf(IDataObject avdo) {
            if (avdo.Get(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT) is MpAvClipTileViewModel ctvm) {
                if (ctvm.CopyItemId == BindingContext.CopyItemId) {
                    return true;
                }
                return ctvm.ItemType == BindingContext.ItemType;
            }
            return false;
        }
        #endregion

        #endregion

        public MpAvClipTileContentView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void ContentTemplate_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is UserControl contentContainer) {
                ContentView = contentContainer.Content as MpAvIContentView;
            }
        }


        //public void UpdateAdorners() {
        //    var al = AdornerLayer.GetAdornerLayer(this);
        //    if(al == null) {
        //        return;
        //    }
        //    al.Children.ForEach(x => x.InvalidateVisual());
        //}


        //private void Cc_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
        //    var cc = sender as ContentControl;
        //    var actual_content_control = cc.Content as Control;
        //    ContentView = actual_content_control as MpAvIContentView;
        //}

        //private async void MpAvClipTileContentView_PointerPressed(object sender, PointerPressedEventArgs e) {
        //    if(!e.IsLeftPress(sender as IVisual)) {
        //        e.Handled = false;
        //        return;
        //    }

        //    if (e.ClickCount >= 2 &&
        //        BindingContext.IsContentReadOnly &&
        //        !BindingContext.IsSubSelectionEnabled) {
        //        BindingContext.IsSubSelectionEnabled = true;
        //        MpPlatformWrapper.Services.Cursor.SetCursor(sender as InputElement, MpCursorType.IBeam);
        //        UpdateAdorners();
        //        return;
        //    }
        //    if (!BindingContext.IsTitleReadOnly ||
        //        !BindingContext.IsContentReadOnly ||
        //         BindingContext.Parent.IsAnyResizing ||
        //         BindingContext.Parent.CanAnyResize) { 
        //        e.Handled = false;
        //        return;
        //    }
        //    if (BindingContext.IsSubSelectionEnabled) {
        //        // NOTE only check for drag when there is selected text AND
        //        // drag is from somewhere in the selection range.
        //        // If mouse down isn't in selection range reset selection to down position

        //        if (ContentView.Selection.IsEmpty) {
        //            e.Handled = false;
        //            return;
        //        }
        //        bool isPressOnSelection = await ContentView.Selection.IsPointInRangeAsync(e.GetPosition(ContentView.Document.Owner).ToPortablePoint());
        //        if (!isPressOnSelection) {
        //            //var mptp = Rtb.GetPositionFromPoint(e.GetPosition(Rtb),true);
        //            //Rtb.Selection.Select(mptp, mptp);
        //            e.Handled = false;
        //            return;
        //        }
        //    }

        //    MpAvDragDropManager.StartDragCheck(BindingContext);

        //    e.Handled = true;
        //}

    }
}
