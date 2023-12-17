using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Linq;
using System.Windows.Input;
//using Xamarin.Forms;

namespace MonkeyPaste.Avalonia {
    public class MpAvMenuItemHostViewModel : MpAvViewModelBase, MpAvIPopupSelectorMenuViewModel {
        #region Interfaces

        public bool IsOpen { get; set; }
        public MpAvMenuItemViewModel PopupMenu { get; }
        public object SelectedIconResourceObj { get; }
        public string SelectedLabel { get; }


        #endregion

        public MpAvMenuItemHostViewModel() : this(null, null) { }
        public MpAvMenuItemHostViewModel(MpAvMenuItemViewModel root_mivm, object selected_identifier) {
            PopupMenu = root_mivm;
            var sel_mivm = FindItemByIdentifier(selected_identifier, null);
            if (sel_mivm != null) {
                SelectedIconResourceObj = sel_mivm.IconSourceObj;
                SelectedLabel = sel_mivm.Header;
            }
        }

        public MpAvMenuItemViewModel FindItemByIdentifier(object identifier, MpAvMenuItemViewModel cur_mivm) {
            if (identifier == null) {
                return null;
            }
            cur_mivm = cur_mivm ?? PopupMenu;
            if (cur_mivm == null) {
                return null;
            }
            if (identifier.Equals(cur_mivm.Identifier)) {
                return cur_mivm;
            }
            if (cur_mivm.SubItems == null) {
                return null;
            }
            return
                cur_mivm.SubItems.OfType<MpAvMenuItemViewModel>()
                .FirstOrDefault(x => FindItemByIdentifier(identifier, x) != null);

        }
        public ICommand ShowSelectorMenuCommand => new MpCommand<object>(
            (args) => {

                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenu);
            });
    }
}
