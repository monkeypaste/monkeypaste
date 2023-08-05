using Avalonia.Threading;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHaveGuid {
        string Guid { get; }
    }
    public class MpAvAnnotationItemViewModel :
        MpViewModelBase<MpAvAnnotationMessageViewModel>,
        MpAvITransactionNodeViewModel,
        MpIHaveGuid,
        MpIClampedValue {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvAnnotationItemViewModel _LastSelectedItem;
        #endregion

        #region Interfaces

        #region MpIHaveGuid Implementation

        string MpIHaveGuid.Guid =>
            AnnotationGuid;

        #endregion

        #region MpIClampedValue Implementation

        double MpIClampedValue.min => AnnotationMinScore;
        double MpIClampedValue.max => AnnotationMaxScore;
        double MpIClampedValue.value => AnnotationScore;

        #endregion

        #region MpIPlainTextViewModel Implementation

        public string PlainText {
            get {
                return "TEST Annotation";
            }
        }
        #endregion

        #region MpAvITransactionNodeViewModel Implementation
        public MpAvClipTileViewModel HostClipTileViewModel =>
            Parent == null || Parent.Parent == null || Parent.Parent.Parent == null ?
                null :
                Parent.Parent.Parent.Parent;
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; private set; }
        public IEnumerable<MpITreeItemViewModel> Children => Items;
        public bool IsSelected {
            get {
                if (Parent is MpAvAnnotationMessageViewModel iamvm) {
                    return iamvm.SelectedItem == this;
                }
                return false;
            }
            set {
                if (value && IsSelected != value && Parent != null) {
                    Parent.SelectedItem = this;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public DateTime LastSelectedDateTime { get; set; }
        public bool IsHovering { get; set; }
        public string Body =>
            AnnotationBody;
        public string LabelText =>
            AnnotationLabel;
        public object ComparableSortValue =>
            AnnotationLabel;
        public object IconSourceObj {
            get {
                if (Children != null && Children.Any()) {
                    return "FolderOutlineImage";
                }
                if (this is MpAvImageAnnotationItemViewModel) {
                    return "BoxCornersImage";
                }
                if (AnnotationScore > 0) {
                    return "EquationImage";
                }
                return "ObjectImage";
            }
        }
        public MpAvMenuItemViewModel ContextMenuItemViewModel =>
            null;
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsAnyBusy);

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvAnnotationItemViewModel> Items { get; private set; } = new ObservableCollection<MpAvAnnotationItemViewModel>();
        #endregion

        #region State

        public double ScorePercent {
            get {
                double length = AnnotationMaxScore - AnnotationMinScore;
                double percent = (AnnotationScore - AnnotationMinScore) / length;
                return percent;
            }
        }

        public double CurScorePercent { get; set; }

        #endregion

        #region Model

        public double AnnotationMinScore {
            get {
                if (Annotation == null) {
                    return 0;
                }
                return Annotation.minScore;
            }
        }
        public double AnnotationMaxScore {
            get {
                if (Annotation == null) {
                    return 0;
                }
                return Annotation.maxScore;
            }
        }
        public double AnnotationScore {
            get {
                if (Annotation == null) {
                    return 0;
                }
                return Annotation.score;
            }
        }
        public string AnnotationType {
            get {
                if (Annotation == null) {
                    return null;
                }
                return Annotation.type;
            }
        }
        public string AnnotationBody {
            get {
                if (Annotation == null) {
                    return null;
                }
                return Annotation.body;
            }
        }
        public string AnnotationLabel {
            get {
                if (Annotation == null) {
                    return null;
                }
                return Annotation.label;
            }
        }

        public string AnnotationGuid {
            get {
                if (Annotation == null) {
                    return null;
                }
                return Annotation.guid;
            }
        }

        public virtual MpAnnotationNodeFormat Annotation { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvAnnotationItemViewModel(MpAvAnnotationMessageViewModel parent) : base(parent) {
            PropertyChanged += MpAvAnnotationItemViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }
        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAnnotationNodeFormat ianf, MpAvAnnotationItemViewModel parentTreeItem) {
            bool wasBusy = IsBusy;
            IsBusy = true;

            ParentTreeItem = parentTreeItem ?? Parent as MpITreeItemViewModel;
            Annotation = ianf;

            Items.Clear();
            if (Annotation.children != null) {
                foreach (var ca in Annotation.children) {
                    var cavm = await CreateAnnotationViewModel(ca);
                    Items.Add(cavm);
                }
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Children));

            IsBusy = wasBusy;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task<MpAvAnnotationItemViewModel> CreateAnnotationViewModel(MpAnnotationNodeFormat anf) {
            MpAvAnnotationItemViewModel aivm = null;
            if (anf is MpImageAnnotationNodeFormat ianf) {
                aivm = new MpAvImageAnnotationItemViewModel(Parent);
            } else {
                aivm = new MpAvAnnotationItemViewModel(Parent);
            }
            await aivm.InitializeAsync(anf, this);
            return aivm;
        }


        private void MpAvAnnotationItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        IsExpanded = true;
                        Dispatcher.UIThread.Post(async () => {
                            // animate score from last selected percent
                            CurScorePercent = _LastSelectedItem == null ? 0 : _LastSelectedItem.CurScorePercent;
                            _LastSelectedItem = this;
                            double percent_v = 0.05 * (CurScorePercent < ScorePercent ? 1 : -1);
                            while (Math.Abs(CurScorePercent - ScorePercent) > Math.Abs(percent_v * 2)) {
                                CurScorePercent += percent_v;
                                await Task.Delay(30);
                            }
                            CurScorePercent = ScorePercent;
                        });
                        if (!HostClipTileViewModel.IsSubSelectionEnabled &&
                            HostClipTileViewModel.TransactionCollectionViewModel.IsTransactionPaneOpen) {
                            // this is to ensure annotations are drawn in editor but 
                            // pretty sure editor enables subselection when annotations are selected
                            HostClipTileViewModel.EnableSubSelectionCommand.Execute(null);
                        }
                    }
                    break;
                case nameof(IsExpanded):
                    if (IsExpanded && ParentTreeItem != null && !ParentTreeItem.IsExpanded) {
                        ParentTreeItem.IsExpanded = true;
                    }
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            //OnPropertyChanged(nameof(Items));
            //OnPropertyChanged(nameof(Children));
        }

        #endregion

        #region Commands
        #endregion
    }
}
