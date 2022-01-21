using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpContextMenuItemCollectionViewModel : MpViewModelBase {
        public ObservableCollection<MpContextMenuItemViewModel> MenuItems { get; set; } = new ObservableCollection<MpContextMenuItemViewModel>();

        public MpContextMenuItemCollectionViewModel() : base(null) { }
    }
}
