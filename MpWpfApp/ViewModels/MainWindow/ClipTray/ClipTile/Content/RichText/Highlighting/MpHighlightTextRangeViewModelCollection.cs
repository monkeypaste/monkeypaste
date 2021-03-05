using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpHighlightTextRangeViewModelCollection : MpObservableCollectionViewModel<MpHighlightTextRangeViewModel>, IDisposable {
        #region Private Variables

        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Properties
        public MpHighlightTextRangeViewModel SelectedHighlightTextRangeViewModel {
            get {
                var selectedItem = this.Where(x => x.IsSelected).FirstOrDefault();
                if(selectedItem == null && this.Count > 0) {
                    selectedItem = this[0];
                    selectedItem.IsSelected = true;
                }
                return selectedItem;
            }
            set {
                if(SelectedHighlightTextRangeViewModel != value && this.Contains(value)) {
                    ClearSelection();
                    this[this.IndexOf(value)].IsSelected = true;
                    OnPropertyChanged(nameof(SelectedHighlightTextRangeViewModel));
                }
            }
        }

        private int _highlightTaskCount = 0;
        public int HighlightTaskCount {
            get {
                return _highlightTaskCount;
            }
            set {
                if (_highlightTaskCount != value) {
                    _highlightTaskCount = value;
                    OnPropertyChanged(nameof(HighlightTaskCount));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModelCollection() : this(null) {}

        public MpHighlightTextRangeViewModelCollection(MpClipTileViewModel ctvm) : base() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(SelectedHighlightTextRangeViewModel):
                        ApplyHighlightingCommand.Execute(null);
                        break;
                    case nameof(HighlightTaskCount):
                        MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsSearching = HighlightTaskCount > 0;
                        if (HighlightTaskCount < 0) {
                            HighlightTaskCount = 0;
                        }
                        break;
                }
            };
            ClipTileViewModel = ctvm;            
        }

        public void Init() {
            MainWindowViewModel.SearchBoxViewModel.PrevMatchClicked += SearchBoxViewModel_PrevMatchClicked;
            MainWindowViewModel.SearchBoxViewModel.NextMatchClicked += SearchBoxViewModel_NextMatchClicked;
        }

        public async Task<Visibility> PerformHighlightingAsync(string hlt) {
            HighlightTaskCount++;
            if (ClipTileViewModel.MainWindowViewModel.IsLoading || ClipTileViewModel.IsLoading) {
                HighlightTaskCount--;
                return Visibility.Visible;
            }

            var sttvm = MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
            if (!sttvm.IsLinkedWithClipTile(ClipTileViewModel)) {
                Console.WriteLine("Clip tile w/ title " + ClipTileViewModel.CopyItemTitle + " is not linked with current tag");
                HighlightTaskCount--;
                return Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                ClearHighlightingCommand.Execute(null);
                HighlightTaskCount--;

                ClipTileViewModel.ResetContentScroll();
                return Visibility.Visible;
            }

            var result = await PerformHighlightAsync(hlt);
            HighlightTaskCount--;
            return result;
        }
        
        private async Task<Visibility> PerformHighlightAsync(string hlt) {
            var result = Visibility.Visible;
            await Dispatcher.CurrentDispatcher.InvokeAsync(
                (Action)(() => {
                    var cb = ClipTileViewModel.ClipBorder;
                    var ttb = ClipTileViewModel.TitleTextBlock;

                    RegexOptions rot = RegexOptions.None;
                    RegexOptions roc = RegexOptions.None;
                    if (Properties.Settings.Default.SearchByIsCaseSensitive) {
                        rot = RegexOptions.ExplicitCapture;
                        roc = RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    } else {
                        rot = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
                        roc = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    }
                    var mct = Regex.Matches(ClipTileViewModel.CopyItemTitle, hlt, rot);
                    var mcc = Regex.Matches(ClipTileViewModel.CopyItemPlainText, hlt, roc);

                    if (mct.Count == 0 && mcc.Count == 0) {                        
                        result = Visibility.Collapsed;
                        return;
                    }

                    Console.WriteLine("Beginning highlight clip with title: " + ClipTileViewModel.CopyItemTitle + " with highlight text: " + hlt);

                    //highlight title 
                    if (mct.Count > 0) {
                        var trl = MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                        foreach (var mr in trl) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, mr, 0));
                        }
                    }
                    switch (ClipTileViewModel.CopyItemType) {
                        case MpCopyItemType.RichText:
                        case MpCopyItemType.Composite:
                            for (int i = 0; i < ClipTileViewModel.RichTextBoxViewModelCollection.Count; i++) {
                                var rtbvm = ClipTileViewModel.RichTextBoxViewModelCollection[i];
                                var rtbvmtrl = MpHelpers.Instance.FindStringRangesFromPosition(rtbvm.Rtb.Document.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                foreach (var mr in rtbvmtrl) {
                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel,mr,i+1));
                                }
                            } 
                            break;
                        case MpCopyItemType.Image:
                            // TODO Add filtering for images
                            break;
                        case MpCopyItemType.FileList:
                            var flb = ClipTileViewModel.FileListBox;
                            for (int i = 0; i < ClipTileViewModel.FileListViewModels.Count; i++) {
                                var fivm = ClipTileViewModel.FileListViewModels[i];
                                if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                    var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                    if (container != null) {
                                        var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                        if (fitb != null) {
                                            var hlrl = MpHelpers.Instance.FindStringRangesFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                            foreach (var mr in hlrl) {
                                                this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, mr, i + 1));
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    Console.WriteLine("Ending highlighting clip with title: " + ClipTileViewModel.CopyItemTitle);
                    OnPropertyChanged(nameof(SelectedHighlightTextRangeViewModel));
                    result = this.Count > 0 ? Visibility.Visible:Visibility.Collapsed;
                }),
                DispatcherPriority.Background);

            return result;
        }

        public void ClearSelection() {
            foreach(var htrvm in this) {
                htrvm.IsSelected = false;
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(this.Count > 0) {
                this[0].IsSelected = true;
            }
        }

        public void Dispose() {
            this.Clear();
            MainWindowViewModel.SearchBoxViewModel.PrevMatchClicked -= SearchBoxViewModel_PrevMatchClicked;
            MainWindowViewModel.SearchBoxViewModel.NextMatchClicked -= SearchBoxViewModel_NextMatchClicked;
        }
        #endregion

        #region Private Methods
        private void SearchBoxViewModel_PrevMatchClicked(object sender, EventArgs e) {
            if(this.Count == 0) {
                return;
            }
            int prevIdx = this.IndexOf(SelectedHighlightTextRangeViewModel) - 1;
            if(prevIdx < 0) {
                prevIdx = this.Count - 1;
            }
            SelectedHighlightTextRangeViewModel = this[prevIdx];
        }

        private void SearchBoxViewModel_NextMatchClicked(object sender, EventArgs e) {
            if (this.Count == 0) {
                return;
            }
            int nextIdx = this.IndexOf(SelectedHighlightTextRangeViewModel) + 1;
            if (nextIdx >= this.Count) {
                nextIdx = 0;
            }
            SelectedHighlightTextRangeViewModel = this[nextIdx];
        }
        #endregion

        #region Commands
        private RelayCommand _applyHighlightingCommand;
        public ICommand ApplyHighlightingCommand {
            get {
                if(_applyHighlightingCommand == null) {
                    _applyHighlightingCommand = new RelayCommand(ApplyHighlighting);
                }
                return _applyHighlightingCommand;
            }
        }
        private void ApplyHighlighting() {
            int curContentId = -1;
            foreach(var hltrvm in this) {
                if(hltrvm.ContentId != curContentId) {
                    if (curContentId > 0 &&
                       (ClipTileViewModel.CopyItemType == MpCopyItemType.RichText ||
                        ClipTileViewModel.CopyItemType == MpCopyItemType.Composite)) {
                        ClipTileViewModel.RichTextBoxViewModelCollection[curContentId - 1].Rtb.EndChange();
                    }
                    curContentId = hltrvm.ContentId;
                    if(curContentId > 0 && 
                       (ClipTileViewModel.CopyItemType == MpCopyItemType.RichText || 
                        ClipTileViewModel.CopyItemType == MpCopyItemType.Composite)) {
                        ClipTileViewModel.RichTextBoxViewModelCollection[curContentId - 1].Rtb.BeginChange();
                    }
                }
                hltrvm.HighlightRange();
            }
            if (curContentId > 0 &&
                (ClipTileViewModel.CopyItemType == MpCopyItemType.RichText ||
                ClipTileViewModel.CopyItemType == MpCopyItemType.Composite)) {
                ClipTileViewModel.RichTextBoxViewModelCollection[curContentId - 1].Rtb.EndChange();
            }
        }

        private RelayCommand _hideHighlightingCommand;
        public ICommand HideHighlightingCommand {
            get {
                if (_hideHighlightingCommand == null) {
                    _hideHighlightingCommand = new RelayCommand(HideHighlighting);
                }
                return _hideHighlightingCommand;
            }
        }
        private void HideHighlighting() {
            foreach (var hltrvm in this) {
                hltrvm.ClearHighlighting();
            }
        }

        private RelayCommand _clearHighlightingCommand;
        public ICommand ClearHighlightingCommand {
            get {
                if (_clearHighlightingCommand == null) {
                    _clearHighlightingCommand = new RelayCommand(ClearHighlighting);
                }
                return _clearHighlightingCommand;
            }
        }
        private void ClearHighlighting() {
            HideHighlightingCommand.Execute(null);
            this.Clear();
        }        
        #endregion
    }
}
