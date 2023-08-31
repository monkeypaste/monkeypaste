using Avalonia;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemViewModel :
        MpAvViewModelBase<MpAvSearchCriteriaItemCollectionViewModel>,
        MpIQueryInfo {
        #region Private Variables       

        private MpSearchCriteriaItem _lastSavedCriteria;

        #endregion

        #region Constants

        public static string DEFAULT_OPTION_LABEL = UiStrings.SearchCriteriaDefaultOptionLabel;

        #endregion

        #region Statics
        // NOTE must match xaml
        public static Thickness CRITERIA_ITEM_BORDER_THICKNESS =>
            new Thickness(0, 1, 0, 1);


        #endregion

        #region Interfaces

        #region MpIQueryInfo Implementation

        public MpQueryType QueryType =>
            MpQueryType.Advanced;

        public bool IsDescending =>
            MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
        public MpContentSortType SortType =>
            MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;

        public int TagId {
            get {
                if (QueryFlags.HasFlag(MpContentQueryBitFlags.Tag)) {
                    var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.Tag.Guid == MatchValue);
                    if (ttvm == null) {
                        // should have found, is tagId a pending query id?
                        MpDebug.Break();
                    } else {
                        return ttvm.TagId;
                    }
                }
                return MpTag.AllTagId;
            }
        }

        public MpContentQueryBitFlags QueryFlags {
            get {
                MpContentQueryBitFlags flags =
                    SelectedOptionPath
                    .Select(x => x.FilterValue)
                    .AggregateOrDefault((a, b) => a | b);
                if (IsCaseSensitive) {
                    flags |= MpContentQueryBitFlags.CaseSensitive;
                }
                if (IsWholeWord) {
                    flags |= MpContentQueryBitFlags.WholeWord;
                }
                return flags;
            }
        }

        public MpIQueryInfo Next {
            get {
                /*
                Join Notes:
                    Although it’s not obvious, you can also 
                    use Boolean search terms to set up a Finder 
                    search—to exclude criteria or to create an OR search. 
                    Once you have one condition set up, you can add a Boolean 
                    term to your next condition by option-clicking on 
                    the plus sign. The plus sign will turn into an ellipsis (…), 
                    and you’ll get a new pull-down menu with options for Any (OR), 
                    All (AND), or None (NOT). (For more details, see Add conditions 
                    to Finder searches) from https://www.macworld.com/article/189989/spotlight3.html
                */
                if (Parent == null) {
                    return null;
                }
                if (SortOrderIdx < Parent.Items.Count - 1) {
                    return Parent.SortedItems.ElementAt(SortOrderIdx + 1);
                }
                // tack simple onto end as OR join
                //return MpAvSimpleQueryViewModel.Instance;
                return null;

            }
        }

        #region MpITextMatchInfo Implementation

        bool MpITextMatchInfo.CaseSensitive =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive);

        bool MpITextMatchInfo.WholeWord =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord);
        bool MpITextMatchInfo.UseRegex =>
            (this as MpIQueryInfo).QueryFlags.HasFlag(MpContentQueryBitFlags.Regex);
        #endregion

        #endregion

        #endregion

        #region Properties

        #region ViewModels

        public MpAvSearchCriteriaOptionViewModel RootOptionViewModel { get; private set; }

        public IList<MpAvSearchCriteriaOptionViewModel> SelectedOptionPath {
            get {
                var sopl = new List<MpAvSearchCriteriaOptionViewModel>();
                var cur_ovm = RootOptionViewModel;
                while (cur_ovm != null) {
                    sopl.Add(cur_ovm);
                    cur_ovm = cur_ovm.SelectedItem;
                }
                return sopl;
            }
        }

        public MpAvSearchCriteriaOptionViewModel LeafValueOptionViewModel =>
            SelectedOptionPath.FirstOrDefault(x => x.IsValueOption);

        public IEnumerable<MpAvSearchCriteriaOptionViewModel> ValueOptionViewModels =>
            SelectedOptionPath
            .Where(x => x.IsValueOption)
            .ToList();
        private ObservableCollection<MpAvSearchCriteriaOptionViewModel> _items = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> Items {
            get {
                var sel_items = new List<MpAvSearchCriteriaOptionViewModel>();
                var node = RootOptionViewModel;
                while (node != null) {
                    sel_items.Add(node);
                    int selIdx = node.Items.IndexOf(node.SelectedItem);
                    if ((selIdx <= 0 && !node.IsRootMultiValueOption) || !node.HasChildren) {
                        break;
                    }
                    node = node.SelectedItem;
                }
                int count = Math.Max(_items.Count, sel_items.Count);
                for (int i = 0; i < count; i++) {
                    if (_items.Count <= i) {
                        if (i < sel_items.Count) {
                            _items.Add(sel_items[i]);
                        } else {
                            continue;
                        }
                    } else if (sel_items.Count <= i) {
                        _items.RemoveAt(i);
                    } else if (_items[i] == sel_items[i]) {
                        continue;
                    } else {
                        _items[i] = sel_items[i];
                    }
                }
                return _items;
            }
        }
        //public IEnumerable<MpAvSearchCriteriaOptionViewModel> Items {
        //    get {
        //        var node = RootOptionViewModel;
        //        while (node != null) {
        //            yield return node;
        //            int selIdx = node.Items.IndexOf(node.SelectedItem);
        //            if ((selIdx <= 0 && !node.IsRootMultiValueOption) || !node.HasChildren) {
        //                break;
        //            }
        //            node = node.SelectedItem;
        //        }

        //        yield break;
        //    }
        //}

        private ObservableCollection<string> _joinTypeLabels;
        public ObservableCollection<string> JoinTypeLabels {
            get {
                if (_joinTypeLabels == null) {
                    _joinTypeLabels = new ObservableCollection<string>(
                        typeof(MpNextJoinOptionType)
                        .EnumToUiStrings()
                        .Skip(1));
                }
                return _joinTypeLabels;
            }
        }

        #region Options

        #region Root Option

        private MpAvSearchCriteriaOptionViewModel GetRootOption() {
            // PATH: /Root
            var rovm = new MpAvSearchCriteriaOptionViewModel(this, null);
            rovm.ItemsOptionType = typeof(MpRootOptionType);
            rovm.IsSelected = true;
            rovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
            rovm.Items.Clear();
            string[] labels = typeof(MpRootOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, rovm);
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.Label = labels[i];
                switch ((MpRootOptionType)i) {
                    case MpRootOptionType.Clips:
                        tovm.ItemsOptionType = typeof(MpContentOptionType);
                        tovm.Items = GetContentOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Collection:
                        tovm.ItemsOptionType = typeof(MpTag);
                        tovm.Items = GetCollectionOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Sources:
                        tovm.ItemsOptionType = typeof(MpSourcesOptionType);
                        tovm.Items = GetSourcesOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.History:
                        tovm.ItemsOptionType = typeof(MpTransactionType);
                        tovm.Items = GetHistoryTypeOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Type:
                        tovm.ItemsOptionType = typeof(MpContentTypeOptionType);
                        tovm.Items = GetContentTypeOptionViewModel(tovm);
                        break;
                }
                rovm.Items.Add(tovm);
            }
            return rovm;
        }

        #endregion

        #region Content

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetTextOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/AnyText
            // PATH: /Root/Content/Title
            // PATH: /Root/Content/TypeSpecific/Text

            var tovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpTextOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                if (i == labels.Length - 1) {
                    tovm.UnitType |= MpSearchCriteriaUnitFlags.RegEx;
                } else {
                    tovm.UnitType |= MpSearchCriteriaUnitFlags.CaseSensitivity | MpSearchCriteriaUnitFlags.WholeWord;
                }
                switch ((MpTextOptionType)i) {
                    case MpTextOptionType.Matches:
                        tovm.FilterValue = MpContentQueryBitFlags.Matches;
                        break;
                    case MpTextOptionType.Contains:
                        tovm.FilterValue = MpContentQueryBitFlags.Contains;
                        break;
                    case MpTextOptionType.BeginsWith:
                        tovm.FilterValue = MpContentQueryBitFlags.BeginsWith;
                        break;
                    case MpTextOptionType.EndsWith:
                        tovm.FilterValue = MpContentQueryBitFlags.EndsWith;
                        break;
                    case MpTextOptionType.RegEx:
                        tovm.FilterValue = MpContentQueryBitFlags.Regex;
                        break;
                }
                tovml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(tovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetNumberOptionViewModel(MpAvSearchCriteriaOptionViewModel parent, MpSearchCriteriaUnitFlags numUnit) {
            // PATH: /Root/DateOrTime/*/WithinLast/*/
            // PATH: /Root/Clips/Type Specific/Image/Dimension/*/

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpNumberOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = numUnit;
                switch ((MpNumberOptionType)i) {
                    case MpNumberOptionType.Equals:
                        tovm.FilterValue = MpContentQueryBitFlags.Equals;
                        break;
                    case MpNumberOptionType.LessThan:
                        tovm.FilterValue = MpContentQueryBitFlags.LessThan;
                        break;
                    case MpNumberOptionType.GreaterThan:
                        tovm.FilterValue = MpContentQueryBitFlags.GreaterThan;
                        break;
                    case MpNumberOptionType.IsNot:
                        tovm.FilterValue = MpContentQueryBitFlags.IsNot;
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetColorOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/Image/Color

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpColorOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                ovm.Items = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(new[] {
                    new MpAvSearchCriteriaOptionViewModel(this,ovm) {
                        Label = "Distance",
                        UnitType = MpSearchCriteriaUnitFlags.UnitDecimal,
                        FilterValue = MpContentQueryBitFlags.ColorDistance,
                        IsSelected = true
                    }
                });
                ovm.SelectedItem = ovm.Items[0];
                switch ((MpColorOptionType)i) {
                    case MpColorOptionType.Hex:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Hex;
                        ovm.FilterValue = MpContentQueryBitFlags.Hex;
                        break;
                    case MpColorOptionType.ARGB:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Rgba;
                        ovm.FilterValue = MpContentQueryBitFlags.Rgba;
                        break;
                }
                novml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDimensionsOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/Image/Dimensions

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDimensionOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.ItemsOptionType = typeof(MpNumberOptionType);
                tovm.Items = GetNumberOptionViewModel(tovm, MpSearchCriteriaUnitFlags.Integer);
                tovm.Items.ForEach(x => x.UnitLabel = "px");
                switch ((MpDimensionOptionType)i) {
                    case MpDimensionOptionType.Width:
                        tovm.FilterValue = MpContentQueryBitFlags.Width;
                        break;
                    case MpDimensionOptionType.Height:
                        tovm.FilterValue = MpContentQueryBitFlags.Height;
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetImageContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/Image
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpImageOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpImageOptionType)i) {
                    case MpImageOptionType.Dimensions:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpDimensionOptionType);
                        ovm.Items = GetDimensionsOptionViewModel(ovm);
                        break;
                    case MpImageOptionType.Color:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpColorOptionType);
                        ovm.Items = GetColorOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetFileOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/File/Kind

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpFileOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                MpFileOptionType fot = (MpFileOptionType)i;
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                if (fot != MpFileOptionType.None) {
                    ovm.FilterValue = MpContentQueryBitFlags.FileExt;
                }
                if (fot == MpFileOptionType.Custom) {
                    ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                    ovm.ItemsOptionType = typeof(MpTextOptionType);
                    ovm.Items = GetTextOptionViewModel(ovm);
                } else {
                    ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                    if (MpFileExtensionsHelper.ExtLookup.TryGetValue((MpFileOptionType)i, out var exts)) {
                        ovm.Values = exts.ToArray();
                        ovm.FilterValue |= MpContentQueryBitFlags.Regex;
                        ovm.CsvFormatProperties = new MpCsvFormatProperties() { EocSeparator = "|" };
                    }
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetFileContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/File

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpFileContentOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpFileContentOptionType)i) {
                    case MpFileContentOptionType.Name:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.FileName;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpFileContentOptionType.Path:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.FilePath;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpFileContentOptionType.Kind:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.FileExt;
                        ovm.ItemsOptionType = typeof(MpFileOptionType);
                        ovm.Items = GetFileOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentTypeContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentTypeOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpContentTypeOptionType)i) {
                    case MpContentTypeOptionType.Text:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.TextType;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentTypeOptionType.Image:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.ImageType;
                        ovm.ItemsOptionType = typeof(MpImageOptionType);
                        ovm.Items = GetImageContentOptionViewModel(ovm);
                        break;
                    case MpContentTypeOptionType.Files:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.FileType;
                        ovm.ItemsOptionType = typeof(MpFileContentOptionType);
                        ovm.Items = GetFileContentOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpContentOptionType)i) {
                    case MpContentOptionType.Content:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Content;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.TypeSpecific:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpContentTypeOptionType);
                        ovm.Items = GetContentTypeContentOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.Title:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Title;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.Annotation:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Annotations;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.Color:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.ItemColor;
                        ovm.ItemsOptionType = typeof(MpColorOptionType);
                        ovm.Items = GetColorOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Content Type Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentTypeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/ContentType

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentTypeOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                ovm.Label = labels[i];
                ovm.Value = ((MpCopyItemType)i).ToString();
                switch ((MpContentTypeOptionType)i) {
                    case MpContentTypeOptionType.Text:
                        ovm.FilterValue = MpContentQueryBitFlags.TextType;
                        break;
                    case MpContentTypeOptionType.Image:
                        ovm.FilterValue = MpContentQueryBitFlags.ImageType;
                        break;
                    case MpContentTypeOptionType.Files:
                        ovm.FilterValue = MpContentQueryBitFlags.FileType;
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Collection Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetCollectionOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Collections
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();

            // NOTE excluding current tag as filter
            var ttvml =
                MpAvTagTrayViewModel.Instance
                .Items
                .Where(x => x.TagId != Parent.QueryTagId && x.IsLinkTag && !x.IsAllTag)
                .OrderBy(x => x.TagName).ToList();

            ttvml.Insert(0, null);
            foreach (var ttvm in ttvml) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                if (ttvm == null) {
                    // add no selection label
                    tovm.Label = DEFAULT_OPTION_LABEL;
                } else {
                    tovm.Label = ttvm.TagName;
                    tovm.Value = ttvm.Tag.Guid;
                }
                tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                tovm.FilterValue = MpContentQueryBitFlags.Tag;
                iovml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Source Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDeviceOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Source/Device

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            var udl = Parent.UserDevices.ToList();
            udl.Insert(0, null);
            foreach (var ud in udl) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                if (ud == null) {
                    // add no selection label
                    ovm.Label = DEFAULT_OPTION_LABEL;
                } else {
                    ovm.Label = ud.MachineName;
                    ovm.Value = ud.Guid;
                }
                ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                ovm.FilterValue = MpContentQueryBitFlags.DeviceName;
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetAppOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Source/App

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpAppOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpAppOptionType)i) {
                    case MpAppOptionType.ProcessPath:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.AppPath;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpAppOptionType.ApplicationName:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.AppName;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetWebsiteOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Source/Website

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpWebsiteOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpWebsiteOptionType)i) {
                    case MpWebsiteOptionType.Domain:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.UrlDomain;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpWebsiteOptionType.Url:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Url;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpWebsiteOptionType.Title:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.UrlTitle;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetSourcesOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Source

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpSourcesOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpSourcesOptionType)i) {
                    case MpSourcesOptionType.Device:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpUserDevice);
                        ovm.Items = GetDeviceOptionViewModel(ovm);
                        break;
                    case MpSourcesOptionType.App:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpAppOptionType);
                        ovm.Items = GetAppOptionViewModel(ovm);
                        break;
                    case MpSourcesOptionType.Website:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpWebsiteOptionType);
                        ovm.Items = GetWebsiteOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Date Options
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetHistoryTypeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpTransactionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                ovm.Label = labels[i];
                ovm.ItemsOptionType = typeof(MpDateTimeOptionType);
                ovm.Items = GetDateTimeOptionViewModel(ovm);
                ovm.FilterValue =
                    i == 0 ?
                        MpContentQueryBitFlags.None :
                        ((MpTransactionType)i).ToString().ToEnum<MpContentQueryBitFlags>();
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateTimeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/Created
            // PATH: /Root/DateOrTime/Modified
            // PATH: /Root/DateOrTime/Pasted

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateTimeOptionType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpDateTimeOptionType)i) {
                    case MpDateTimeOptionType.Before:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Before;
                        ovm.ItemsOptionType = typeof(MpDateBeforeUnitType);
                        ovm.Items = GetDateBeforeOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.After:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.After;
                        ovm.ItemsOptionType = typeof(MpDateAfterUnitType);
                        ovm.Items = GetDateAfterOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.WithinLast:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.WithinLast;
                        ovm.ItemsOptionType = typeof(MpTimeSpanWithinUnitType);
                        ovm.Items = GetTimeSpanWithinOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.Exact:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        ovm.FilterValue = MpContentQueryBitFlags.Exactly;
                        break;
                }
                novml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateBeforeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/Created/Before
            // PATH: /Root/DateOrTime/Modified/Before
            // PATH: /Root/DateOrTime/Pasted/Before

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateBeforeUnitType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                switch ((MpDateBeforeUnitType)i) {
                    case MpDateBeforeUnitType.Today:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = 0.ToString();
                        break;
                    case MpDateBeforeUnitType.Yesterday:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = 1.ToString();
                        break;
                    case MpDateBeforeUnitType.ThisWeek:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = (((int)DateTime.Today.DayOfWeek) + 1).ToString();
                        break;
                    case MpDateBeforeUnitType.ThisMonth:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = DateTime.Today.Day.ToString();
                        break;
                    case MpDateBeforeUnitType.ThisYear:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = DateTime.Today.DayOfYear.ToString();
                        break;
                    case MpDateBeforeUnitType.Exact:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        tovm.FilterValue = MpContentQueryBitFlags.Exactly;
                        break;
                    case MpDateBeforeUnitType.Startup:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        // NOTE using -1 as special match value to denote flag as startup ticks
                        // since no other date/time matchvalue will be < 0 this should be ok?
                        tovm.Value = (-1).ToString();
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateAfterOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/Created/After
            // PATH: /Root/DateOrTime/Modified/After
            // PATH: /Root/DateOrTime/Pasted/After

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateAfterUnitType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                switch ((MpDateAfterUnitType)i) {
                    case MpDateAfterUnitType.Yesterday:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = 0.ToString();
                        break;
                    case MpDateAfterUnitType.LastWeek:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = (((int)DateTime.Today.DayOfWeek) + 1).ToString();
                        break;
                    case MpDateAfterUnitType.LastMonth:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = DateTime.Today.Day.ToString();
                        break;
                    case MpDateAfterUnitType.LastYear:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = DateTime.Today.DayOfYear.ToString();
                        break;
                    case MpDateAfterUnitType.Exact:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        tovm.FilterValue = MpContentQueryBitFlags.Exactly;
                        break;
                    case MpDateAfterUnitType.Startup:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        // NOTE using -1 as special match value to denote flag as startup ticks
                        // since no other date/time matchvalue will be < 0 this should be ok?
                        tovm.Value = (-1).ToString();
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }


        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetTimeSpanWithinOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/Created/WithinLast
            // PATH: /Root/DateOrTime/Modified/WithinLast
            // PATH: /Root/DateOrTime/Pasted/WithinLast

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpTimeSpanWithinUnitType).EnumToUiStrings(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Decimal;
                switch ((MpTimeSpanWithinUnitType)i) {
                    case MpTimeSpanWithinUnitType.Hours:
                        tovm.FilterValue = MpContentQueryBitFlags.Hours;
                        break;
                    case MpTimeSpanWithinUnitType.Days:
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        break;
                    case MpTimeSpanWithinUnitType.Weeks:
                        tovm.FilterValue = MpContentQueryBitFlags.Weeks;
                        break;
                    case MpTimeSpanWithinUnitType.Months:
                        tovm.FilterValue = MpContentQueryBitFlags.Months;
                        break;
                    case MpTimeSpanWithinUnitType.Years:
                        tovm.FilterValue = MpContentQueryBitFlags.Years;
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }



        #endregion


        #endregion

        #endregion

        #region Appearance


        #endregion

        #region Layout       

        public double CriteriaItemHeight =>
            Parent == null ? 0 :
                Parent.DefaultCriteriaRowHeight * (IsJoinPanelVisible ? 2 : 1) +
                (Parent.CriteriaDropLineHeight * 2);
        #endregion

        #region State

        public bool HasMultiValues =>
            ValueOptionViewModels.Count() > 1;

        public bool IsDragging { get; set; }
        public bool IsDragOverTop { get; set; }
        public bool IsDragOverBottom { get; set; }

        public bool IsDragOverCopy { get; set; }

        public int SelectedJoinTypeIdx {
            get => (int)(JoinType - 1);
            set {
                if (value < 0) {
                    // BUG ui randomly setting idx to -1
                    return;
                }
                if (SelectedJoinTypeIdx != value) {
                    JoinType = (MpLogicalQueryType)(value + 1);
                    OnPropertyChanged(nameof(SelectedJoinTypeIdx));
                }
            }
        }

        public bool CanRemoveOrSortThisCriteriaItem {
            get {
                if (Parent == null) {
                    return false;
                }
                if (Parent.Items.Count > 1) {
                    return true;
                }
                // reject last item remove
                return SortOrderIdx > 0;
            }
        }
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsAnyBusy);

        public bool IsJoinPanelVisible =>
            JoinType != MpSearchCriteriaItem.DEFAULT_QUERY_JOIN_TYPE;

        public bool IsInputVisible =>
            Items.Any() &&
            !Items.Last().HasChildren &&
            !Items.Last().UnitType.HasFlag(MpSearchCriteriaUnitFlags.EnumerableValue);

        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedItem == this;
            }
        }

        public bool IsEmptyCriteria {
            get {
                //if(SelectedOptionPath.Count == 0) {
                //    return true;
                //}
                //if(SelectedOptionPath.Count == 1 &&
                //    SelectedOptionPath.First() is MpAvSearchCriteriaOptionViewModel ovm) {
                //    return ovm.SelectedItem == null;
                //}
                //return false;
                if (LeafValueOptionViewModel == null ||
                    string.IsNullOrEmpty(LeafValueOptionViewModel.Value)) {
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Model

        public int SortOrderIdx {
            get {
                if (SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.SortOrderIdx;
            }
            set {
                if (SearchCriteriaItem.SortOrderIdx != value) {
                    SearchCriteriaItem.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public int SearchCriteriaItemId {
            get {
                if (SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.Id;
            }
        }

        public bool IsCaseSensitive {
            get {
                if (SearchCriteriaItem == null) {
                    return false;
                }
                return SearchCriteriaItem.IsCaseSensitive;
            }
            set {
                if (IsCaseSensitive != value) {
                    SearchCriteriaItem.IsCaseSensitive = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsCaseSensitive));
                }
            }
        }

        public bool IsWholeWord {
            get {
                if (SearchCriteriaItem == null) {
                    return false;
                }
                return SearchCriteriaItem.IsWholeWord;
            }
            set {
                if (IsWholeWord != value) {
                    SearchCriteriaItem.IsWholeWord = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsWholeWord));
                }
            }
        }

        public string MatchValue {
            get {
                if (SearchCriteriaItem == null) {
                    return string.Empty;
                }
                return SearchCriteriaItem.MatchValue;
            }
            set {
                if (MatchValue != value) {
                    SearchCriteriaItem.MatchValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(MatchValue));
                }
            }
        }

        public string SearchOptions {
            get {
                if (SearchCriteriaItem == null) {
                    return string.Empty;
                }
                return SearchCriteriaItem.Options;
            }
            set {
                if (SearchOptions != value) {
                    SearchCriteriaItem.Options = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SearchOptions));
                }
            }
        }

        public MpLogicalQueryType JoinType {
            get {
                if (SearchCriteriaItem == null) {
                    return MpLogicalQueryType.None;
                }
                return SearchCriteriaItem.JoinType;
            }
            set {
                if (JoinType != value) {
                    SearchCriteriaItem.JoinType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    OnPropertyChanged(nameof(CriteriaItemHeight));
                    OnPropertyChanged(nameof(JoinType));
                }
            }
        }
        public int QueryTagId {
            get {
                if (SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.QueryTagId;
            }
            set {
                if (QueryTagId != value) {
                    SearchCriteriaItem.QueryTagId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(QueryTagId));
                }
            }
        }
        public MpSearchCriteriaItem SearchCriteriaItem { get; set; }


        #endregion

        #endregion

        #region Public Methods

        public MpAvSearchCriteriaItemViewModel() : this(null) { }

        public MpAvSearchCriteriaItemViewModel(MpAvSearchCriteriaItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchCriteriaItemViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpSearchCriteriaItem sci) {
            IsBusy = true;
            IgnoreHasModelChanged = true;

            await Task.Delay(1);

            if (sci == null) {
                MpDebug.Break("should be created in collection create ");

            }
            SearchCriteriaItem = sci;

            RootOptionViewModel = GetRootOption();
            var cur_opt = RootOptionViewModel;
            var sel_idxs = SearchOptions.SplitNoEmpty(",").Select(x => int.Parse(x));
            foreach (var sel_idx in sel_idxs) {
                if (cur_opt == null) {
                    // is this expected behavior?
                    MpDebug.Break();
                    break;
                }
                cur_opt.SelectedItemIdx = sel_idx;
                cur_opt = cur_opt.SelectedItem;
            }
            if (LeafValueOptionViewModel != null) {
                if (HasMultiValues) {
                    // hex or rgba
                    var match_parts = MatchValue.SplitNoEmpty(",");
                    if (match_parts.Any()) {
                        var hex_or_rga_vm = Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.Hex || x.UnitType == MpSearchCriteriaUnitFlags.Rgba);
                        if (hex_or_rga_vm.UnitType == MpSearchCriteriaUnitFlags.Rgba) {
                            hex_or_rga_vm.Values = match_parts.First().ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value).ToArray();
                        } else {
                            hex_or_rga_vm.Value = match_parts.First();
                        }
                        if (match_parts.Count() > 1) {
                            var dist_vm = Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.UnitDecimal);
                            dist_vm.Value = match_parts.Last();
                        }
                    }

                } else {
                    LeafValueOptionViewModel.Value = MatchValue;
                    LeafValueOptionViewModel.IsChecked = IsCaseSensitive;
                    LeafValueOptionViewModel.IsChecked2 = IsWholeWord;
                }

            }

            //RefreshOptionItems();
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedJoinTypeIdx));
            _lastSavedCriteria = SearchCriteriaItem;

            IsBusy = false;
        }

        public void NotifyOptionOrValueChanged(MpAvSearchCriteriaOptionViewModel ovm) {
            if (Parent == null ||
                IsBusy ||
                Parent.IsBusy) {
                // don't notify during load
                return;
            }

            if (!HasModelChanged) {
                // only eval if false (is reset manually by save cmd)
                HasModelChanged = HasCriteriaStateChanged();
            }

            if (ovm == null || ovm.IsValueOption) {
                // whenever its changed requery on subsequent changes
                SetModelToCurrent();
                Mp.Services.Query.NotifyQueryChanged(true);
            }
        }

        public void RefreshOptionItems() {
            // HACK getting collection modified ex when using auto property so doing this instead
            //var sel_items = new List<MpAvSearchCriteriaOptionViewModel>();
            //var node = RootOptionViewModel;
            //while (node != null) {
            //    sel_items.Add(node);
            //    int selIdx = node.Items.IndexOf(node.SelectedItem);
            //    if ((selIdx <= 0 && !node.IsRootMultiValueOption) || !node.HasChildren) {
            //        break;
            //    }
            //    node = node.SelectedItem;
            //}
            //Items = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(sel_items);
            //OnPropertyChanged(nameof(Items));
        }

        #endregion

        #region Private Methods

        private void MpAvSearchCriteriaItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IgnoreHasModelChanged):
                    if (!IgnoreHasModelChanged && HasModelChanged) {
                        OnPropertyChanged(nameof(HasModelChanged));
                    }
                    break;
                case nameof(IsDragging):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyDragging));
                    break;
                case nameof(HasModelChanged):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.HasAnyCriteriaModelChanged));
                    }

                    if (HasModelChanged) {
                        if (IgnoreHasModelChanged) {
                            break;
                        }
                        if (QueryTagId == 0) {
                            // ignore write while pending
                            // QueryTagId is set in convertPending
                            break;
                        }
                        Dispatcher.UIThread.Post(async () => {
                            IsBusy = true;
                            SetModelToCurrent();
                            await SearchCriteriaItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            _lastSavedCriteria = SearchCriteriaItem;
                            IsBusy = false;
                        });
                    }
                    break;
                case nameof(JoinType):
                    NotifyOptionOrValueChanged(null);
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    break;
                case nameof(IsJoinPanelVisible):
                    OnPropertyChanged(nameof(CriteriaItemHeight));
                    break;
                case nameof(SortOrderIdx):
                    // update tail to hide simple OR join
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    break;
            }
        }

        private bool HasCriteriaStateChanged() {
            var cur = GetCurrentItemState();
            if (SearchCriteriaItem == null) {
                return true;
            }
            return !cur.IsValueEqual(_lastSavedCriteria);
        }

        private async Task ClearThisItemAsync() {
            if (SearchCriteriaItem == null) {
                return;
            }
            SearchCriteriaItem.Options = string.Empty;
            SearchCriteriaItem.MatchValue = string.Empty;
            SearchCriteriaItem.MatchValue = null;
            SearchCriteriaItem.IsCaseSensitive = false;
            SearchCriteriaItem.IsWholeWord = false;
            await SearchCriteriaItem.WriteToDatabaseAsync();
            await InitializeAsync(SearchCriteriaItem);
        }
        private MpSearchCriteriaItem GetCurrentItemState() {
            var sci = new MpSearchCriteriaItem() {
                Id = SearchCriteriaItemId,
                QueryTagId = QueryTagId,
                Guid = SearchCriteriaItem == null ? System.Guid.NewGuid().ToString() : SearchCriteriaItem.Guid,
                Options =
                    string.Join(",", SelectedOptionPath.Where(x => x.SelectedItemIdx >= 0).Select(x => x.SelectedItemIdx)),
                SortOrderIdx = SortOrderIdx,
                QueryType = QueryType,
                JoinType = JoinType
            };

            if (LeafValueOptionViewModel == null) {
                sci.MatchValue = null;
                sci.IsCaseSensitive = false;
                sci.IsWholeWord = false;
            } else {
                // reset to default
                sci.MatchValue = null;
                sci.IsCaseSensitive = false;
                sci.IsWholeWord = false;

                if (HasMultiValues) {
                    // hex or rgba
                    var hex_or_rga_vm =
                        Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.Hex || x.UnitType == MpSearchCriteriaUnitFlags.Rgba);
                    if (hex_or_rga_vm != null) {
                        sci.MatchValue = hex_or_rga_vm.Value;
                        var dist_vm = Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.UnitDecimal);
                        if (dist_vm != null) {
                            sci.MatchValue += $",{dist_vm.Value}";
                        }
                    }
                } else {
                    sci.MatchValue = LeafValueOptionViewModel.Value;
                    sci.IsCaseSensitive = LeafValueOptionViewModel.IsChecked;
                    sci.IsWholeWord = LeafValueOptionViewModel.IsChecked2;
                }
            }
            return sci;
        }

        public void SetModelToCurrent() {
            SearchOptions =
                   string.Join(",", SelectedOptionPath.Where(x => x.SelectedItemIdx >= 0).Select(x => x.SelectedItemIdx));

            if (LeafValueOptionViewModel == null) {
                MatchValue = null;
                IsCaseSensitive = false;
                IsWholeWord = false;
            } else {
                // reset to default
                MatchValue = null;
                IsCaseSensitive = false;
                IsWholeWord = false;

                if (HasMultiValues) {
                    // hex or rgba
                    var hex_or_rga_vm =
                        Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.Hex || x.UnitType == MpSearchCriteriaUnitFlags.Rgba);
                    if (hex_or_rga_vm != null) {
                        MatchValue = hex_or_rga_vm.Value;
                        var dist_vm = Items.FirstOrDefault(x => x.UnitType == MpSearchCriteriaUnitFlags.UnitDecimal);
                        if (dist_vm != null) {
                            MatchValue += $",{dist_vm.Value}";
                        }
                    }
                } else {
                    MatchValue = LeafValueOptionViewModel.Value;
                    IsCaseSensitive = LeafValueOptionViewModel.IsChecked;
                    IsWholeWord = LeafValueOptionViewModel.IsChecked2;
                }
            }
        }
        #endregion

        #region Commands

        public ICommand SelectCustomNextJoinTypeCommand => new MpCommand(
            () => {
                JoinType = MpLogicalQueryType.Or;
            }, () => JoinType == MpSearchCriteriaItem.DEFAULT_QUERY_JOIN_TYPE);

        public ICommand AddNextCriteriaItemCommand => new MpCommand(
            () => {
                Parent.AddSearchCriteriaItemCommand.Execute(this);
            }, () => Parent != null);

        public ICommand RemoveThisCriteriaItemCommand => new MpCommand(
            () => {
                if (!CanRemoveOrSortThisCriteriaItem) {
                    // NOTE since there's no way to add a row if all rows are gone, just
                    // clear it if its the last row, no different behaviorly
                    ClearThisItemAsync().FireAndForgetSafeAsync(this);
                    return;
                }
                Parent.RemoveSearchCriteriaItemCommand.Execute(this);
            });

        #endregion
    }
}
