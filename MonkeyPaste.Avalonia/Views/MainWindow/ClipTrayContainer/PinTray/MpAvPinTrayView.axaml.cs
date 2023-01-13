using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel> { 
        #region Private Variables

        private MpAvDropHostAdorner _dropAdorner;

        #endregion

        #region Statics

        public static MpAvPinTrayView Instance { get; private set; }

        #endregion


        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragEnter] PinTrayListBox: ");
            BindingContext.IsDragOverPinTray = true;
        }

        private void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] PinTrayListBox: ");
            var ptr_mp = e.GetPosition(sender as Control).ToPortablePoint();
            int drop_idx = GetDropIdx(ptr_mp);
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            MpConsole.WriteLine("[DragOver] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: "+is_copy+" IsValid: "+is_drop_valid);
            e.DragEffects = DragDropEffects.None;
            if (is_drop_valid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                MpLine dropLine = CreateDropLine(drop_idx, is_copy);
                DrawAdorner(dropLine);
            } else {
                ClearAdorner();
            }
        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            MpConsole.WriteLine("[DragLeave] PinTrayListBox: ");
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            // NOTE only pin tray allows drop not clip tray

            var host_mp = e.GetPosition(sender as Control).ToPortablePoint();
            int drop_idx = GetDropIdx(host_mp);
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            MpConsole.WriteLine("[Drop] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);

            e.DragEffects = DragDropEffects.None;
            if (is_drop_valid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                bool is_internal = e.Data.ContainsInternalContentItem();
                if (is_internal) {
                    // Internal Drop
                    await PerformTileDropAsync(drop_idx, e.Data, is_copy);
                } else {
                    // External Drop
                    await PerformExternalOrPartialDropAsync(drop_idx, e.Data);
                }
            }

            ResetDrop();
        }

        #endregion

        #region Drop Helpers
        private int GetDropIdx(MpPoint host_mp) {
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_lb_mp = this.TranslatePoint(host_mp.ToAvPoint(), ptr_lb).Value.ToPortablePoint();

            if (!ptr_lb.Bounds.Contains(ptr_lb_mp.ToAvPoint())) {
                return -1;
            }

            if (BindingContext.PinnedItems.Count == 0) {
                // TODO Add logic for pin popout overlay or add binding somewhere when empty
                return 0;
            }

            MpRectSideHitTest closet_side_ht = null;
            int closest_side_lbi_idx = -1;
            for (int i = 0; i < BindingContext.PinnedItems.Count; i++) {
                var lbi_rect = ptr_lb.ItemContainerGenerator.ContainerFromIndex(i).Bounds.ToPortableRect();
                var cur_tup = lbi_rect.GetClosestSideToPoint(ptr_lb_mp, "t,b");
                if (closet_side_ht == null || cur_tup.ClosestSideDistance < closet_side_ht.ClosestSideDistance) {
                    closet_side_ht = cur_tup;
                    closest_side_lbi_idx = i;
                }
            }

            if (closet_side_ht.ClosestSideLabel == "r") {
                return closest_side_lbi_idx + 1;
            }
            return closest_side_lbi_idx;
        }

        private bool IsDropValid(IDataObject avdo, int drop_idx, bool is_copy) {
            // called in DropExtension DragOver 

            MpConsole.WriteLine("IsDropValid DropIdx: " + drop_idx);
            if (drop_idx < 0) {
                return false;
            }
            string drag_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if(string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                // Tile drop is always valid
                return true;
            }

            var drag_pctvm = BindingContext.PinnedItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
            int drag_pctvm_idx = BindingContext.PinnedItems.IndexOf(drag_pctvm);
            
            bool is_drop_onto_same_idx = drop_idx == drag_pctvm_idx || drop_idx == drag_pctvm_idx + 1;
            bool is_internal = avdo.ContainsInternalContentItem();
            bool is_partial_drop = !is_internal;

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

        private async Task PerformTileDropAsync(int drop_idx, IDataObject avdo, bool isCopy) {
            string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x=>x.PublicHandle == drop_ctvm_pub_handle);
            if (drop_ctvm == null) {
                Debugger.Break();
            }

            if (isCopy) {
                //  duplicate
                var dup_ci = (MpCopyItem)await BindingContext.SelectedItem.CopyItem.Clone(true);

                await dup_ci.WriteToDatabaseAsync();


                drop_ctvm = await BindingContext.CreateClipTileViewModel(dup_ci, -1);
            } 
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });
            MpConsole.WriteLine($"Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
        }

        private async Task PerformExternalOrPartialDropAsync(int drop_idx, IDataObject avdo) {
            MpPortableDataObject mpdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.ReadDragDropDataObject(avdo) as MpPortableDataObject;
            //int drag_ciid = -1;
            string drag_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (!string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                var drag_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
                if (drag_ctvm != null) {
                    // tile sub-selection drop
                    //drag_ciid = drag_ctvm.CopyItemId;
                    mpdo.SetData(MpPortableDataFormats.LinuxUriList, new string[] { MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(drag_ctvm.CopyItem) });
                }
            }
            var avdo_ci = await MpPlatformWrapper.Services.CopyItemBuilder.BuildAsync(mpdo);//,drag_ciid);

            var drop_ctvm = await BindingContext.CreateClipTileViewModel(avdo_ci, -1);
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });

            MpConsole.WriteLine($"PARTIAL Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
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
            var ref_lbi = ptr_lb.ItemContainerGenerator.ContainerFromIndex(ref_lbi_idx) as ListBoxItem;
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
            var lbi_child = ctv.FindControl<MpAvClipBorder>("ClipTileContainerBorder").Child;
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
            if(_dropAdorner != null) {
                // should only happen once
                Debugger.Break();
            }

            _dropAdorner = new MpAvDropHostAdorner(adornedControl);
            adornedControl
                .AddOrReplaceAdornerAsync(_dropAdorner)
                .FireAndForgetSafeAsync(BindingContext);

            MpConsole.WriteLine("Adorner added to control: " + adornedControl);
        }

        #endregion

        #endregion

        public MpAvPinTrayView() {
            if (Instance != null) {
                // ensure singleton
                Debugger.Break();
                return;
            }
            Instance = this;

            InitializeComponent();

            PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
            PinTrayListBox.AttachedToVisualTree += PinTrayListBox_AttachedToVisualTree;
            PinTrayListBox.ItemContainerGenerator.Materialized += ItemContainerGenerator_Materialized;
            PinTrayListBox.ItemContainerGenerator.Dematerialized += ItemContainerGenerator_Dematerialized;
            PinTrayListBox.GotFocus += PinTrayListBox_GotFocus;
        }

        private void PinTrayListBox_GotFocus(object sender, GotFocusEventArgs e) {
            if(BindingContext.IsPinTrayEmpty) {
                return;
            }
            if(e.NavigationMethod == NavigationMethod.Tab) {
                if(e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                    // shift tab from clip tray select last
                    BindingContext.SelectedItem = BindingContext.PinnedItems.Last();
                } else {
                    BindingContext.SelectedItem = BindingContext.PinnedItems.First();
                }
            }
        }

        private void ItemContainerGenerator_Dematerialized(object sender, ItemContainerEventArgs e) {
            if (BindingContext == null || BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow) {
                // ignore collection changed if user in workflow
                MpConsole.WriteLine($"PinTray dematerialized {e.Containers.Count} items was ignored. HasUserAlteredPinTrayWidthSinceWindowShow: {BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow} ");
                return;
            }
            MpSize remaining_size = new MpSize();
            if(!BindingContext.IsPinTrayEmpty) {
                remaining_size.Width = BindingContext.PinnedItems.Max(x => x.TrayRect.Right);
                remaining_size.Height = BindingContext.PinnedItems.Max(x => x.TrayRect.Bottom);
            }

            var gs = this.GetVisualAncestor<MpAvClipTrayContainerView>().GetVisualDescendant<GridSplitter>();
            var gs_grid = gs.GetVisualAncestor<Grid>();
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {

                //double adjusted_width = Math.Min(BindingContext.MaxPinTrayScreenWidth + gs.Bounds.Width, Math.Max(this.Bounds.Width + gs.Bounds.Width, remaining_size.Width));
                gs_grid.ColumnDefinitions[0].Width = new GridLength(remaining_size.Width, GridUnitType.Auto);
            } else {
                //double adjusted_height = Math.Min(BindingContext.MaxPinTrayScreenHeight + gs.Bounds.Height, Math.Max(this.Bounds.Height + gs.Bounds.Height, remaining_size.Height));
                gs_grid.RowDefinitions[0].Height = new GridLength(remaining_size.Height, GridUnitType.Auto);
            }
        }

        private void ItemContainerGenerator_Materialized(object sender, ItemContainerEventArgs e) {
            if (BindingContext == null ||
                BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow ||
                e.Containers.Count == 0) {
                // ignore collection changed if user in workflow
                MpConsole.WriteLine($"PinTray materialized {e.Containers.Count} items was ignored. HasUserAlteredPinTrayWidthSinceWindowShow: {BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow} ");
                return;
            }
            MpSize added_size = new MpSize();
            added_size.Width = e.Containers
                    .Where(x => x.ContainerControl != null && x.ContainerControl.DataContext != null && x.ContainerControl.DataContext is MpAvClipTileViewModel)
                    .Select(x => x.ContainerControl.DataContext)
                    .Cast<MpAvClipTileViewModel>()
                    .Sum(x => x.TrayRect.Width);
            added_size.Height = e.Containers
                    .Where(x => x.ContainerControl != null && x.ContainerControl.DataContext != null && x.ContainerControl.DataContext is MpAvClipTileViewModel)
                    .Select(x => x.ContainerControl.DataContext)
                    .Cast<MpAvClipTileViewModel>()
                    .Sum(x => x.TrayRect.Height);



            var gs = this.GetVisualAncestor<MpAvClipTrayContainerView>().GetVisualDescendant<GridSplitter>();
            var gs_grid = gs.GetVisualAncestor<Grid>();
            var adjusted_size = new MpSize();
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                //if (!this.Bounds.Width.IsNumber() || !gs.Bounds.Width.IsNumber() || !added_size.Width.IsNumber()) {
                //    Debugger.Break();
                //}
                adjusted_size.Width = Math.Min(BindingContext.MaxPinTrayScreenWidth + gs.Bounds.Width, Math.Max(this.Bounds.Width + gs.Bounds.Width, added_size.Width + 40.0d));
                gs_grid.ColumnDefinitions[0].Width = new GridLength(adjusted_size.Width, GridUnitType.Pixel);
            } else {
                adjusted_size.Height = Math.Min(BindingContext.MaxPinTrayScreenHeight + gs.Bounds.Height, Math.Max(this.Bounds.Height + gs.Bounds.Height, added_size.Height));
                gs_grid.RowDefinitions[0].Height = new GridLength(adjusted_size.Height, GridUnitType.Pixel);
            }
            //MpConsole.WriteLine($"PinTray materialized {e.Containers.Count} items. AddedSize: {added_size} AdjustedSize: {adjusted_size}");
        }

        private void PinTrayListBox_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var ptrlb = sender as ListBox;
            DragDrop.SetAllowDrop(ptrlb, true);
            ptrlb.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            ptrlb.AddHandler(DragDrop.DragOverEvent, DragOver);
            ptrlb.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            ptrlb.AddHandler(DragDrop.DropEvent, Drop);
            InitAdorner(ptrlb);

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.PinTrayEmptyOrHasTile:
                    // NOTE these layout values need to match UpdateContentOrientation settings
                    var ptrlb = this.FindControl<ListBox>("PinTrayListBox");
                    if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                        // to avoid interfering w/ user-defined layout
                    }else {
                        ptrlb.Padding = new Thickness();
                    }
                    if(MpAvClipTrayViewModel.Instance.ListOrientation == Orientation.Horizontal) {
                        ptrlb.Padding = new Thickness(10, 0, 10, 0);
                    } else {
                        ptrlb.Padding = new Thickness(10, 10, 10, 10);
                    }
                    break;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
