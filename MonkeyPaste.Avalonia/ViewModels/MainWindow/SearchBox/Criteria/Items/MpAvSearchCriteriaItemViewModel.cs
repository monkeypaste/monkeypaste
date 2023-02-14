using Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchCriteriaItemViewModel :
        MpViewModelBase<MpAvSearchCriteriaItemCollectionViewModel>,
        MpIQueryInfo {
        #region Private Variables       
        #endregion

        #region Constants
        public const string DEFAULT_OPTION_LABEL = " - Please Select - ";
        public const double DEFAULT_HEIGHT = 50;
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
                        Debugger.Break();
                    } else {
                        return ttvm.TagId;
                    }
                }
                return MpTag.AllTagId;
            }
        }

        public MpContentQueryBitFlags QueryFlags {
            get {
                return
                    SelectedOptionPath
                    .Select(x => x.FilterValue)
                    .AggregateOrDefault((a, b) => a | b);
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

        private ObservableCollection<MpAvSearchCriteriaOptionViewModel> _items;
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> Items {
            get {
                if (_items == null) {
                    _items = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();
                }
                _items.Clear();
                var node = RootOptionViewModel;
                while (node != null) {
                    _items.Add(node);
                    int selIdx = node.Items.IndexOf(node.SelectedItem);
                    if (selIdx <= 0 || !node.HasChildren) {
                        break;
                    }
                    node = node.SelectedItem;
                }

                return _items;
            }
        }

        private ObservableCollection<string> _joinTypeLabels;
        public ObservableCollection<string> JoinTypeLabels {
            get {
                if (_joinTypeLabels == null) {
                    _joinTypeLabels = new ObservableCollection<string>(
                        typeof(MpNextJoinOptionType)
                        .EnumToLabels()
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
            string[] labels = typeof(MpRootOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, rovm);
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.Label = labels[i];
                switch ((MpRootOptionType)i) {
                    case MpRootOptionType.Content:
                        tovm.ItemsOptionType = typeof(MpContentOptionType);
                        tovm.Items = GetContentOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Collection:
                        tovm.ItemsOptionType = typeof(MpTag);
                        tovm.Items = GetCollectionOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Source:
                        tovm.ItemsOptionType = typeof(MpSourceOptionType);
                        tovm.Items = GetSourceOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.DateOrTime:
                        tovm.ItemsOptionType = typeof(MpDateTimeTypeOptionType);
                        tovm.Items = GetDateTimeTypeOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.ContentType:
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
            string[] labels = typeof(MpTextOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetNumberOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/*/WithinLast/*/

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpNumberOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Decimal;
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetColorOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/Image/Color

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpColorOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpColorOptionType)i) {
                    case MpColorOptionType.Hex:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Hex;
                        ovm.FilterValue = MpContentQueryBitFlags.Hex;
                        break;
                    case MpColorOptionType.RGBA:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.ByteX4 | MpSearchCriteriaUnitFlags.UnitDecimalX4;
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
            string[] labels = typeof(MpDimensionOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Integer;
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
            string[] labels = typeof(MpImageOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
            string[] labels = typeof(MpFileOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
                    }
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetFileContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Content/TypeSpecific/File

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpFileContentOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
            string[] labels = typeof(MpContentTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
            string[] labels = typeof(MpContentOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpContentOptionType)i) {
                    case MpContentOptionType.AnyText:
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
            string[] labels = typeof(MpContentTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
                .Where(x => x.TagId != Parent.QueryTagId)
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
            string[] labels = typeof(MpAppOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpAppOptionType)i) {
                    case MpAppOptionType.ProcessPath:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.FilterValue = MpContentQueryBitFlags.AppPath;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpAppOptionType.ApplicationName:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
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
            string[] labels = typeof(MpWebsiteOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpWebsiteOptionType)i) {
                    case MpWebsiteOptionType.Domain:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.FilterValue = MpContentQueryBitFlags.UrlDomain;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpWebsiteOptionType.Url:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.FilterValue = MpContentQueryBitFlags.Url;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpWebsiteOptionType.Title:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.FilterValue = MpContentQueryBitFlags.UrlTitle;
                        ovm.ItemsOptionType = typeof(MpTextOptionType);
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetSourceOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/Source

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpSourceOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpSourceOptionType)i) {
                    case MpSourceOptionType.Device:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpUserDevice);
                        ovm.Items = GetDeviceOptionViewModel(ovm);
                        break;
                    case MpSourceOptionType.App:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.ItemsOptionType = typeof(MpAppOptionType);
                        ovm.Items = GetAppOptionViewModel(ovm);
                        break;
                    case MpSourceOptionType.Website:
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
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateTimeTypeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime

            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateTimeTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpDateTimeTypeOptionType)i) {
                    case MpDateTimeTypeOptionType.Created:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Created;
                        ovm.ItemsOptionType = typeof(MpDateTimeOptionType);
                        ovm.Items = GetDateTimeOptionViewModel(ovm);
                        break;
                    case MpDateTimeTypeOptionType.Modified:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Modified;
                        ovm.ItemsOptionType = typeof(MpDateTimeTypeOptionType);
                        ovm.Items = GetDateTimeOptionViewModel(ovm);
                        break;
                    case MpDateTimeTypeOptionType.Pasted:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.FilterValue = MpContentQueryBitFlags.Pasted;
                        ovm.ItemsOptionType = typeof(MpDateTimeTypeOptionType);
                        ovm.Items = GetDateTimeOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateTimeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            // PATH: /Root/DateOrTime/Created
            // PATH: /Root/DateOrTime/Modified
            // PATH: /Root/DateOrTime/Pasted

            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateTimeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
                        ovm.FilterValue = MpContentQueryBitFlags.Between;
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
            string[] labels = typeof(MpDateBeforeUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

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
            string[] labels = typeof(MpDateAfterUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                switch ((MpDateAfterUnitType)i) {
                    case MpDateAfterUnitType.Yesterday:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        tovm.Value = 1.ToString();
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
            string[] labels = typeof(MpTimeSpanWithinUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.ItemsOptionType = typeof(MpNumberOptionType);
                tovm.Items = GetNumberOptionViewModel(tovm);
                switch ((MpTimeSpanWithinUnitType)i) {
                    case MpTimeSpanWithinUnitType.Hours:
                        tovm.FilterValue = MpContentQueryBitFlags.Hours;
                        break;
                    case MpTimeSpanWithinUnitType.Days:
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        break;
                    case MpTimeSpanWithinUnitType.Weeks:
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        break;
                    case MpTimeSpanWithinUnitType.Months:
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
                        break;
                    case MpTimeSpanWithinUnitType.Years:
                        tovm.FilterValue = MpContentQueryBitFlags.Days;
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

        public double CriteriaItemHeight =>
            Parent == null ? 0 :
                DEFAULT_HEIGHT * (IsJoinPanelVisible ? 2 : 1);

        #endregion

        #region Layout        
        #endregion

        #region State

        public bool HasCriteriaChanged { get; set; } = false;

        public bool IsDragOverTop { get; set; }
        public bool IsDragOverBottom { get; set; }

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

        public bool IsJoinDropDownOpen { get; set; }
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
            !Items[Items.Count - 1].HasChildren &&
            !Items[Items.Count - 1].UnitType.HasFlag(MpSearchCriteriaUnitFlags.EnumerableValue);

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
                    return null;
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
                //if(IsAdvancedTail) {
                //    return MpLogicalQueryType.Or;
                //}
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
                //if(IsAdvancedTail) {
                //    return MpLogicalQueryType.Or;
                //}
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
        //public MpQueryType QueryType { 
        //    get {
        //        if(SearchCriteriaItem == null) {
        //            return MpQueryType.Simple;
        //        }
        //        //if(IsAdvancedTail) {
        //        //    return MpLogicalQueryType.Or;
        //        //}
        //        return SearchCriteriaItem.QueryType;
        //    }     
        //} 

        public MpSearchCriteriaItem SearchCriteriaItem { get; set; }


        #endregion

        #endregion

        #region Public Methods

        public MpAvSearchCriteriaItemViewModel() : this(null) { }

        public MpAvSearchCriteriaItemViewModel(MpAvSearchCriteriaItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchCriteriaItemViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        public async Task InitializeAsync(MpSearchCriteriaItem sci) {
            IsBusy = true;
            await Task.Delay(1);

            SearchCriteriaItem = sci == null ? new MpSearchCriteriaItem() : sci;

            RootOptionViewModel = GetRootOption();
            var cur_opt = RootOptionViewModel;
            var sel_idxs = SearchOptions.SplitNoEmpty(",").Select(x => int.Parse(x));
            foreach (var sel_idx in sel_idxs) {
                if (cur_opt == null) {
                    // is this expected behavior?
                    Debugger.Break();
                    break;
                }
                cur_opt.SelectedItemIdx = sel_idx;
                cur_opt = cur_opt.SelectedItem;
            }
            if (LeafValueOptionViewModel != null) {
                LeafValueOptionViewModel.Value = MatchValue;
                LeafValueOptionViewModel.IsChecked = IsCaseSensitive;
                LeafValueOptionViewModel.IsChecked2 = IsWholeWord;
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedJoinTypeIdx));

            IsBusy = false;
        }

        public void NotifyValueChanged(MpAvSearchCriteriaOptionViewModel ovm) {
            if (Parent == null ||
                IsBusy ||
                Parent.IsBusy) {
                // don't notify during load
                return;
            }
            IgnoreHasModelChanged = true;

            SearchOptions =
                string.Join(",", SelectedOptionPath.Where(x => x.SelectedItemIdx >= 0).Select(x => x.SelectedItemIdx));
            if (LeafValueOptionViewModel == null) {
                MatchValue = null;
                IsCaseSensitive = false;
                IsWholeWord = false;
            } else {
                MatchValue = LeafValueOptionViewModel.Value;
                IsCaseSensitive = LeafValueOptionViewModel.IsChecked;
                IsWholeWord = LeafValueOptionViewModel.IsChecked2;
            }

            IgnoreHasModelChanged = false;

            // flag criteria changed for refresh query
            HasCriteriaChanged = true;
            if (ovm.IsValueOption) {
                MpPlatform.Services.Query.NotifyQueryChanged(true);
            }
        }

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {

        }
        private void MpAvSearchCriteriaItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IgnoreHasModelChanged):
                    if (!IgnoreHasModelChanged && HasModelChanged) {
                        OnPropertyChanged(nameof(HasModelChanged));
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        if (IgnoreHasModelChanged) {
                            break;
                        }
                        if (QueryTagId == 0) {
                            // ignore write while pending
                            // QueryTagId is set in convertPending
                            break;
                        }
                        Task.Run(async () => {
                            IsBusy = true;
                            await SearchCriteriaItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            IsBusy = false;
                        });
                    }
                    break;
                case nameof(IsJoinDropDownOpen):
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    OnPropertyChanged(nameof(JoinType));
                    break;
                case nameof(IsJoinPanelVisible):
                    OnPropertyChanged(nameof(CriteriaItemHeight));
                    break;
                case nameof(CriteriaItemHeight):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.MaxSearchCriteriaListBoxHeight));
                    break;
                case nameof(SortOrderIdx):
                    // update tail to hide simple OR join
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    break;
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
                Parent.RemoveSearchCriteriaItemCommand.Execute(this);
            }, () => CanRemoveOrSortThisCriteriaItem);

        #endregion
    }
}
