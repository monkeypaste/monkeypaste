using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        #region Private Variables

        private MpAvDropHostAdorner _dropAdorner;

        #endregion

        #region Statics

        public static MpAvPinTrayView Instance { get; private set; }

        #endregion

        public MpAvPinTrayView() {
            if (Instance != null) {
                // ensure singleton
                MpDebug.Break();
                return;
            }
            Instance = this;

            AvaloniaXamlLoader.Load(this);
            InitDnd();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            var ptrlb = this.FindControl<ListBox>("PinTrayListBox");
            ptrlb.AddHandler(KeyDownEvent, PinTrayListBox_KeyDown, RoutingStrategies.Tunnel);
        }

        private void PinTrayListBox_KeyDown(object sender, KeyEventArgs e) {
            // prevent default list arrow navigation (it doesn't account for row nav)
            if (e.Key != Key.Left &&
                e.Key != Key.Up &&
                e.Key != Key.Right &&
                e.Key != Key.Down) {
                return;
            }
            e.Handled = BindingContext.CanTileNavigate();
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.PinTrayEmptyOrHasTile:
                    // NOTE these layout values need to match UpdateContentOrientation settings
                    var ptrlb = this.FindControl<ListBox>("PinTrayListBox");
                    if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                        // to avoid interfering w/ user-defined layout
                    } else {
                        ptrlb.Padding = new Thickness();
                    }
                    if (MpAvClipTrayViewModel.Instance.ListOrientation == Orientation.Horizontal) {
                        ptrlb.Padding = new Thickness(10, 0, 10, 0);
                    } else {
                        ptrlb.Padding = new Thickness(10, 10, 10, 10);
                    }
                    break;
            }
        }


        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] PinTrayListBox: ");
            BindingContext.IsDragOverPinTray = true;
        }

        private void DragOver(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragOver] PinTrayListBox: ");
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_mp = e.GetPosition(ptr_lb).ToPortablePoint();
            int drop_idx = ptr_lb.GetDropIdx(ptr_mp, Orientation.Horizontal);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            //MpConsole.WriteLine("[DragOver] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);
            e.DragEffects = DragDropEffects.None;
            if (is_drop_valid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                MpLine dropLine = CreateDropLine(drop_idx, is_copy);
                DrawAdorner(dropLine);
            } else {
                ClearAdorner();
            }
        }
        private void DragLeave(object sender, DragEventArgs e) {
            // MpConsole.WriteLine("[DragLeave] PinTrayListBox: ");
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            // NOTE only pin tray allows drop not clip tray

            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_mp = e.GetPosition(ptr_lb).ToPortablePoint();
            int drop_idx = ptr_lb.GetDropIdx(ptr_mp, Orientation.Horizontal);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            // MpConsole.WriteLine("[Drop] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);

            e.DragEffects = DragDropEffects.None;
            if (!is_drop_valid) {
                ResetDrop();
                return;
            }
            BindingContext.IsPinTrayBusy = true;

            e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;

            // NOTE need to use processed/output data object, avdo becomes disposed
            var mpdo = await
                Mp.Services.DataObjectHelperAsync
                .ReadDragDropDataObjectAsync(e.Data) as MpAvDataObject;

            Dispatcher.UIThread.Post(async () => {
                var avdo_ci = await mpdo.ToCopyItemAsync(is_copy);
                if (avdo_ci == null || avdo_ci.Id <= 0) {
                    // source rejected, add blocked or no content-enabled format
                } else {
                    var drop_ctvm = await BindingContext.CreateClipTileViewModelAsync(avdo_ci, -1);
                    BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });

                    MpConsole.WriteLine($"PARTIAL Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
                }

                while (BindingContext.IsAnyBusy) {
                    await Task.Delay(100);
                }
                BindingContext.IsPinTrayBusy = false;
                ResetDrop();
            });

        }

        #endregion

        #region Drop Helpers

        private void InitDnd() {
            var ptrlb = this.FindControl<ListBox>("PinTrayListBox");
            DragDrop.SetAllowDrop(ptrlb, true);
            ptrlb.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            ptrlb.AddHandler(DragDrop.DragOverEvent, DragOver);
            ptrlb.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            ptrlb.AddHandler(DragDrop.DropEvent, Drop);
            InitAdorner(ptrlb);
        }

        private bool IsDropValid(IDataObject avdo, int drop_idx, bool is_copy) {
            // called in DropExtension DragOver 

            //MpConsole.WriteLine("IsDropValid DropIdx: " + drop_idx);
            if (avdo.ContainsTagItem()) {
                return false;
            }
            if (drop_idx < 0) {
                return false;
            }
            string drag_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                // Tile drop is always valid
                return true;
            }

            var drag_pctvm = BindingContext.PinnedItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
            int drag_pctvm_idx = BindingContext.PinnedItems.IndexOf(drag_pctvm);

            bool is_drop_onto_same_idx = drop_idx == drag_pctvm_idx || drop_idx == drag_pctvm_idx + 1;
            bool is_internal = avdo.ContainsContentRef();
            bool is_partial_drop = avdo.ContainsPartialContentRef();

            if (!is_copy && !is_partial_drop && is_drop_onto_same_idx) {
                // don't allow moving full item if it'll be at same position 
                return false;
            }
            // TODO Check data here (which shouldn't matter for internal but would be general app-level check from external)

            //avdo.GetDataFormats().ForEach(x => MpConsole.WriteLine($"FORMAT: '{x}' Data: '{avdo.Get(x)}'"));


            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOverPinTray = false;
            ClearAdorner();
        }


        #endregion

        #region Adorner

        private void ClearAdorner() {
            _dropAdorner.IsVisible = false;
            _dropAdorner.DrawDropAdorner(null);
        }

        private void DrawAdorner(MpLine dropLine) {
            _dropAdorner.IsVisible = true;
            if (dropLine == null) {
                return;
            }
            _dropAdorner.DrawDropAdorner(new MpShape[] { dropLine });
        }

        private MpLine CreateDropLine(int drop_idx, bool isCopy) {
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            int total_count = BindingContext.PinnedItems.Count;
            if (drop_idx > total_count || total_count == 0) {
                return null;
            }
            MpLine drop_line;
            int ref_lbi_idx = Math.Clamp(drop_idx, 0, total_count - 1);
            var ref_lbi = ptr_lb.ContainerFromIndex(ref_lbi_idx) as ListBoxItem;
            var ref_lbi_rect = ref_lbi.Bounds.ToPortableRect();

            if (drop_idx < total_count) {
                drop_line = ref_lbi_rect.GetSideByLabel("l");
            } else {
                drop_line = ref_lbi_rect.GetSideByLabel("r");
            }
            // NOTE this cleans up the line...for some reason container bounds ignores lb padding 
            var ptr_lb_wp = ptr_lb.GetVisualDescendant<WrapPanel>();

            // lbi bounds need to be relative to items panel not lb, lb doesn't account for box model calc
            var items_panel_offset = ptr_lb_wp.TranslatePoint(new Point(0, 0), ptr_lb).Value.ToPortablePoint();
            drop_line.P1 += items_panel_offset;
            drop_line.P2 += items_panel_offset;

            // get child to adjust height
            var ctv = ref_lbi.GetVisualDescendant<MpAvClipTileView>();
            // NOTE get inner child because of border thickness
            var lbi_child = ctv.FindControl<MpAvClipBorder>("ClipTileContainerBorder").Content as Control;
            var lbi_child_tl = lbi_child.TranslatePoint(new Point(0, 0), ptr_lb_wp);
            drop_line.P1.Y = lbi_child_tl.Value.Y;

            var lbi_child_bl = lbi_child.TranslatePoint(new Point(0, lbi_child.Bounds.Height), ptr_lb_wp);
            drop_line.P2.Y = lbi_child_bl.Value.Y;

            drop_line.StrokeOctColor = isCopy ? MpSystemColors.limegreen : MpSystemColors.White;
            drop_line.StrokeThickness = 2;
            drop_line.StrokeDashStyle = new double[] { 5, 2 };

            return drop_line;
        }

        private void InitAdorner(Control adornedControl) {
            if (_dropAdorner != null) {
                // should only happen once
                MpDebug.Break();
            }

            Dispatcher.UIThread.Post(async () => {
                while (!Mp.Services.StartupState.IsReady) {
                    await Task.Delay(100);
                }
                _dropAdorner = new MpAvDropHostAdorner(adornedControl);
                await adornedControl
                    .AddOrReplaceAdornerAsync(_dropAdorner);
            });

            //MpConsole.WriteLine("Adorner added to control: " + adornedControl);
        }

        #endregion

        #endregion
    }
}
