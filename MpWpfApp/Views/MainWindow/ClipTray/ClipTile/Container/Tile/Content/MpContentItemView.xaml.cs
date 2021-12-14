using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpContentItemView.xaml
    /// </summary>
    public partial class MpContentItemView : MpUserControl<MpContentItemViewModel> {
        private static MpContentContextMenuView _ContentContextMenu;

        public MpContentItemView() : base() {
            InitializeComponent();            
        }

        private void ContentListItemView_Loaded(object sender, RoutedEventArgs e) {
        }

        private void ContentListItemView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpContentItemViewModel ocivm) {
                ocivm.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
            if (e.NewValue != null && e.NewValue is MpContentItemViewModel ncivm) {
                if (!ncivm.IsPlaceholder) {
                    ncivm.OnUiUpdateRequest += Civm_OnUiUpdateRequest;
                }
            }
        }

        private void ContentListItemView_MouseEnter(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.IsHovering = true;
            if(!MpDragDropManager.Instance.IsDragAndDrop &&
                (!BindingContext.Parent.IsExpanded || !BindingContext.IsSelected)) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.OverDragItem;
            }
        }

        private void ContentListItemView_MouseLeave(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.IsHovering = false;
            if (!MpDragDropManager.Instance.IsDragAndDrop) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }
        }
        #region Event Handlers

        #region View Model Ui Requests

        private void Civm_OnUiUpdateRequest(object sender, EventArgs e) {
            this.UpdateLayout();            
        }


        #endregion

        #endregion

        private void Border_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext != null) {
                BindingContext.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (BindingContext.IsEditingTitle ||
                (BindingContext.IsSelected &&
                 BindingContext.Parent.IsExpanded)) {
                e.Handled = false;
                return;
            }
            BindingContext.IsSelected = true;

            MpDragDropManager.Instance.StartDragCheck(
                e.GetPosition(Application.Current.MainWindow),
                MpClipTrayViewModel.Instance.PersistentSelectedModels);

            e.Handled = true;
        }

        private void Border_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }
            if (_ContentContextMenu == null) {
                _ContentContextMenu = new MpContentContextMenuView();
            }

            if (!BindingContext.IsSelected) {
                BindingContext.IsSelected = true;
            }

            e.Handled = true;

            ContextMenu = _ContentContextMenu;
            ContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;
        }
    }
}
