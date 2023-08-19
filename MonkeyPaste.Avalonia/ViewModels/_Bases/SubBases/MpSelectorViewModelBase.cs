using MonkeyPaste.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    public abstract class MpSelectorViewModelBase<P, C> :
        MpAvViewModelBase<P>,
        MpISelectorViewModel
        where P : class
        where C : class, MpISelectableViewModel {

        public MpSelectorViewModelBase() : base(null) { }

        public MpSelectorViewModelBase(P p) : base(p) { }

        public virtual ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();

        public virtual C SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsSelected = x == value);
                    if (Mp.Services.StartupState.LoadedDateTime != null &&
                        SelectedItem != null) {
                        SelectedItem.LastSelectedDateTime = DateTime.Now;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public virtual C LastSelectedItem => Items.OrderByDescending(x => x.LastSelectedDateTime).FirstOrDefault();
        public bool HasItems => Items.Count > 0;

        public bool IsAnySelected => SelectedItem != null;

        object MpISelectorViewModel.SelectedItem {
            get => SelectedItem;
            set => SelectedItem = (C)value;
        }
        //public List<C> SelectedItems => Items.Where(x => x.IsSelected).ToList();
    }
}
