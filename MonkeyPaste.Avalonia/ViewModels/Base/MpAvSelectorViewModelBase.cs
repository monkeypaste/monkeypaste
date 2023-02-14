using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvSelectorViewModelBase<P, C> :
        MpSelectorViewModelBase<P, C>
        where P : class
        where C : class, MpISelectableViewModel {

        // BUG error MSB4018: System.ArgumentException: Member 'System.Collections.ObjectModel.ObservableCollection`1' is declared in another module and needs to be imported
        // Must override .net standard observable collection in avalonia for some reason
        public override ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();
        public override C SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsSelected = x == value);
                    if (MpPlatform.Services.StartupState.LoadedDateTime != null &&
                        SelectedItem != null) {
                        SelectedItem.LastSelectedDateTime = DateTime.Now;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public MpAvSelectorViewModelBase() : base(null) { }

        public MpAvSelectorViewModelBase(P p) : base(p) { }
    }

    public abstract class MpAvMultiSelectorViewModelBase<P, C> :
        MpMultiSelectorViewModelBase<P, C>
        where P : class
        where C : class, MpISelectableViewModel {

        // BUG error MSB4018: System.ArgumentException: Member 'System.Collections.ObjectModel.ObservableCollection`1' is declared in another module and needs to be imported
        // Must override .net standard observable collection in avalonia for some reason
        public override ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();
        public override C SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsSelected = x == value);
                    if (MpPlatform.Services.StartupState.LoadedDateTime != null &&
                        SelectedItem != null) {
                        SelectedItem.LastSelectedDateTime = DateTime.Now;
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public override IList<C> SelectedItems {
            get => Items.Where(x => x.IsSelected).ToList();
            set => Items.ForEach(x => x.IsSelected = value == null ? false : value.Contains(x));
        }

        public MpAvMultiSelectorViewModelBase() : base(null) { }

        public MpAvMultiSelectorViewModelBase(P p) : base(p) { }
    }
}
