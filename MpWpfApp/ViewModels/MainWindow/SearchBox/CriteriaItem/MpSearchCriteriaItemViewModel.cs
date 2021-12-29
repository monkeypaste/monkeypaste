using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Documents;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    /*
        Name, Contents - Text - Matches, Contains, Begins/Ends With
        Number - Equals, Greater/Less Than, Is Not
        Kind - is (Types, Other - Text)
        DateTime - is (within last (days,weeks,months,years) 
                    # days/weeks/months/years, exactly , before, after (all date pickers), 
                    today, yesterday, this week/month/year 
    */
    public enum MpSearchCriteriaPropertyType {
        Content = 0,
        Description,
        ContentType,
        Collection,
        Title,
        Application,
        Website,
        Time,
        Device,
        Logical
    }

    public enum MpSearchCriteriaUnitType {
        None = 0,
        Text = 1,
        Number = 2,
        DateTime = 4,
        TimeSpan = 8,
        Enumerable = 16,
        Other = 32
    };

    public class MpSearchCriteriaItemViewModel : MpViewModelBase<MpSearchBoxViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region Item Sources

        public ObservableCollection<string> PrimaryLabels {
            get {
                return new ObservableCollection<string>() {
                    "Content",
                    "Description",
                    "Content Type",
                    "Collection",
                    "Title",
                    "Application",
                    "Website",
                    "Date/Time",
                    "Device",
                    "Logical"
                };
            }
        }

        public ObservableCollection<string> SecondaryLabels {
            get {
                switch (SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Title:
                    case MpSearchCriteriaPropertyType.Content:
                    case MpSearchCriteriaPropertyType.Description:
                        return TextUnitOptionLabels;

                    case MpSearchCriteriaPropertyType.ContentType:
                        return ContentTypeOptionLabels;

                    case MpSearchCriteriaPropertyType.Collection:
                        return CollectionsOptionLabels;

                    case MpSearchCriteriaPropertyType.Application:
                        return ApplicaitonTypeUnitOptionLabels;

                    case MpSearchCriteriaPropertyType.Website:
                        return WebContentTypeUnitOptionLabels;

                    case MpSearchCriteriaPropertyType.Time:
                        return DateTimeUnitOptionLabels;

                    case MpSearchCriteriaPropertyType.Device:
                        return DeviceOptionLabels;
                    case MpSearchCriteriaPropertyType.Logical:
                        return LogicalTypeOptionLabels;
                    default: return null;
                }
            }
        }

        public ObservableCollection<string> TertiaryLabels {
            get {
                switch(SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Collection:
                        if(SelectedSecondaryIdx == CollectionsOptionLabels.Count - 1) {
                            return TextUnitOptionLabels;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Application:
                    case MpSearchCriteriaPropertyType.Website:
                        return TextUnitOptionLabels;
                }
                return null;
            }
        }

        public ObservableCollection<string> QuaternaryLabels {
            get {
                switch (SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Collection:
                        if (SelectedSecondaryIdx == CollectionsOptionLabels.Count - 1) {
                            return TextUnitOptionLabels;
                        }
                        break;
                }
                return null;
            }
        }
        

        #region Option Labels

        public ObservableCollection<string> TextUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Matches",
                    "Contains",
                    "Begins With",
                    "Ends With",
                    "RegEx"
                };
            }
        }

        public ObservableCollection<string> LogicalTypeOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "And",
                    "Or",
                    "Not",
                    "Xor"
                };
            }
        }

        public ObservableCollection<string> ContentTypeOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Text",
                    "Image",
                    "Files"
                };
            }
        }        

        public ObservableCollection<string> NumberUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Equals",
                    "Greater Than",
                    "Less Than",
                    "Is Not"
                };
            }
        }

        public ObservableCollection<string> TimeSpanWithinUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Hours",
                    "Days",
                    "Weeks",
                    "Months",
                    "Years"
                };
            }
        }

        public ObservableCollection<string> TimeSpanFromDateTimeUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Today",
                    "Yesterday",
                    "This Week",
                    "This Month",
                    "This Year"
                };
            }
        }

        public ObservableCollection<string> DateTimeUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Exactly",
                    "Before",
                    "After"
                };
            }
        }

        public ObservableCollection<string> CollectionsOptionLabels { get; private set; } = new ObservableCollection<string>();

        public ObservableCollection<string> DeviceOptionLabels { get; private set; } = new ObservableCollection<string>();

        public ObservableCollection<string> FileContentTypeUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Path",
                    "Name",
                    "Extension"
                };
            }
        }

        public ObservableCollection<string> ApplicaitonTypeUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Path",
                    "Name"
                };
            }
        }

        public ObservableCollection<string> WebContentTypeUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Url",
                    "Domain",
                    "Title"
                };
            }
        }

        #endregion

        #endregion

        #region Appearance

        public Brush AddCriteriaItemButtonBorderBrush {
            get {
                return IsOverAddCriteriaButton ? Brushes.DimGray : Brushes.LightGray;
            }
        }

        public Brush RemoveCriteriaItemButtonBorderBrush {
            get {
                return IsOverRemoveCriteriaButton ? Brushes.DimGray : Brushes.LightGray;
            }
        }

        public Brush CriteriaItemBorderBrush {
            get {
                return IsHovering ? IsSelected ? Brushes.Red : Brushes.Yellow : Brushes.DimGray;
            }
        }

        #endregion

        #region State

        public bool HasSecondaryLabels => SecondaryLabels != null && SecondaryLabels.Count > 0;

        public bool CanInputText => SearchCriteriaUnitTypeFlags.HasFlag(MpSearchCriteriaUnitType.Text);

        public bool CanInputDate => SearchCriteriaUnitTypeFlags.HasFlag(MpSearchCriteriaUnitType.DateTime);

        public bool HasTertiaryLabels => TertiaryLabels != null && TertiaryLabels.Count > 0;

        public bool IsSelected { get; set; } = false;

        public bool IsHovering { get; set; } = false;

        public int SelectedCriteriaTypeIdx { get; set; } = 0;

        public int SelectedSecondaryIdx { get; set; } = 0;

        public int SelectedTertiaryIdx { get; set; } = 0;

        public int SelectedQuaternaryIdx { get; set; } = 0;

        public MpSearchCriteriaPropertyType SelectedCriteriaType => (MpSearchCriteriaPropertyType)SelectedCriteriaTypeIdx;

        public MpSearchCriteriaUnitType SearchCriteriaUnitTypeFlags {
            get {
                switch (SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Title:
                    case MpSearchCriteriaPropertyType.Content:
                        return MpSearchCriteriaUnitType.Text;

                    case MpSearchCriteriaPropertyType.Collection:
                        if (SelectedSecondaryIdx == CollectionsOptionLabels.Count - 1) {
                            return MpSearchCriteriaUnitType.Enumerable | MpSearchCriteriaUnitType.Text;
                        }
                        return MpSearchCriteriaUnitType.Enumerable;

                    case MpSearchCriteriaPropertyType.Application:
                        return MpSearchCriteriaUnitType.Enumerable;

                    case MpSearchCriteriaPropertyType.ContentType:
                        return MpSearchCriteriaUnitType.Text;

                    case MpSearchCriteriaPropertyType.Time:
                        return MpSearchCriteriaUnitType.DateTime;

                    default: return MpSearchCriteriaUnitType.None;
                }
            }
        }

        public MpContentFilterType FilterFlags {
            get {
                MpContentFilterType ff = MpContentFilterType.None;
                switch (SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Description:
                        ff |= MpContentFilterType.Meta;
                        if (SelectedSecondaryIdx == TextUnitOptionLabels.Count - 1) {
                            ff |= MpContentFilterType.Regex;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Title:
                        ff |= MpContentFilterType.Title;
                        if(SelectedSecondaryIdx == TextUnitOptionLabels.Count - 1) {
                            ff |= MpContentFilterType.Regex;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Content:
                        ff |= MpContentFilterType.Content;
                        if (SelectedSecondaryIdx == TextUnitOptionLabels.Count - 1) {
                            ff |= MpContentFilterType.Regex;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Collection:
                        ff |= MpContentFilterType.Tag;
                        if (SelectedTertiaryIdx == TextUnitOptionLabels.Count - 1) {
                            if(SelectedQuaternaryIdx == TextUnitOptionLabels.Count - 1) {
                                ff |= MpContentFilterType.Regex;
                            }
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Application:
                        if (SelectedSecondaryIdx == 0) {
                            ff |= MpContentFilterType.AppPath;
                        } else if(SelectedSecondaryIdx == 1) {
                            ff |= MpContentFilterType.AppName;
                        }
                        if (SelectedTertiaryIdx == TextUnitOptionLabels.Count - 1) {
                            ff |= MpContentFilterType.Regex;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Website:
                        if (SelectedSecondaryIdx == 0) {
                            ff |= MpContentFilterType.Url;
                        } else if (SelectedSecondaryIdx == 1) {
                            ff |= MpContentFilterType.Url;
                        } else if (SelectedSecondaryIdx == 2) {
                            ff |= MpContentFilterType.UrlTitle;
                        }
                        if (SelectedTertiaryIdx == TextUnitOptionLabels.Count - 1) {
                            ff |= MpContentFilterType.Regex;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.ContentType:
                        if(SelectedSecondaryIdx == 0) {
                            ff |= MpContentFilterType.TextType;
                        } else if (SelectedSecondaryIdx == 1) {
                            ff |= MpContentFilterType.ImageType;
                        } else if (SelectedSecondaryIdx == 2) {
                            ff |= MpContentFilterType.FileType;
                        }
                        break;
                    case MpSearchCriteriaPropertyType.Time:
                        ff |= MpContentFilterType.Time;
                        break;
                    case MpSearchCriteriaPropertyType.Logical:
                        return ff;
                    default:
                        throw new Exception("Unknonw criteria");
                }
                return ff;
            }
        }

        public int SortOrderIdx {
            get {
                if(SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.SortOrderIdx;
            }
            set {
                if (SearchCriteriaItem.SortOrderIdx != value) {
                    SearchCriteriaItem.SortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public bool IsOverAddCriteriaButton { get; set; } = false;

        public bool IsOverRemoveCriteriaButton { get; set; } = false;

        #endregion

        #region Model

        public bool IsCaseSensitive { get; set; } = false;

        public string InputValue {
            get {
                if(SearchCriteriaItem == null) {
                    return string.Empty;
                }
                return SearchCriteriaItem.InputValue;
            }
            set {
                if(SearchCriteriaItem.InputValue != value) {
                    SearchCriteriaItem.InputValue = value;
                    OnPropertyChanged(nameof(InputValue));
                }
            }
        }

        public MpSearchCriteriaItem SearchCriteriaItem { get; set; }

        #endregion

        #endregion

        #region Public Methods

        public MpSearchCriteriaItemViewModel() : base(null) { }

        public MpSearchCriteriaItemViewModel(MpSearchBoxViewModel parent) : base(parent) {
            PropertyChanged += MpSearchCriteriaItemViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpSearchCriteriaItem sci) {
            IsBusy = true;

            SearchCriteriaItem = sci;
            SelectedCriteriaTypeIdx = 0;

            MpTagTrayViewModel.Instance.TagTileViewModels.ForEach(x => CollectionsOptionLabels.Add(x.TagName));
            CollectionsOptionLabels.Sort(x=>x);
            CollectionsOptionLabels.Add(" - custom -");
            OnPropertyChanged(nameof(CollectionsOptionLabels));

            var dl = await MpDb.Instance.GetItemsAsync<MpUserDevice>();

            dl.ForEach(x => DeviceOptionLabels.Add(x.MachineName));
            DeviceOptionLabels.Sort(x => x);
            DeviceOptionLabels.Add(" - custom -");
            OnPropertyChanged(nameof(DeviceOptionLabels));

            IsBusy = false;
        }

        public MpIQueryInfo ToQueryInfo() {
            var qi = new MpWpfQueryInfo();
            qi.SortOrderIdx = Parent.CriteriaItems.IndexOf(this) + 1;
            qi.FilterFlags = this.FilterFlags;

            if(CanInputText) {
                if(SecondaryLabels != null && SecondaryLabels[1] == "Matches") {
                    qi.TextFlags = (MpTextFilterFlagType)SelectedSecondaryIdx;
                } else if (TertiaryLabels != null && TertiaryLabels.Count > 0 && TertiaryLabels[1] == "Matches") {
                    qi.TextFlags = (MpTextFilterFlagType)SelectedTertiaryIdx;
                } else if (QuaternaryLabels != null && QuaternaryLabels.Count > 0 && QuaternaryLabels[1] == "Matches") {
                    qi.TextFlags = (MpTextFilterFlagType)SelectedQuaternaryIdx;
                }                
            }
            if (CanInputDate) {
                qi.TimeFlags = (MpTimeFilterFlagType)SelectedTertiaryIdx;
            }

            if(CanInputDate || CanInputText) {
                qi.SearchText = InputValue;
            } else {
                if ((MpSearchCriteriaPropertyType)SelectedCriteriaTypeIdx == MpSearchCriteriaPropertyType.Collection) {
                    qi.SearchText = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagName == CollectionsOptionLabels[SelectedSecondaryIdx].ToLower()).TagId.ToString();
                } 
            }

            if((MpSearchCriteriaPropertyType)SelectedCriteriaTypeIdx == MpSearchCriteriaPropertyType.Logical) {
                qi.LogicFlags = (MpLogicalFilterFlagType)SelectedSecondaryIdx;
            }

            return qi;
        }

        #endregion

        #region Private Methods

        private void MpSearchCriteriaItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedCriteriaTypeIdx):
                    SelectedSecondaryIdx = SelectedTertiaryIdx = 0;
                    Refresh();
                    InputValue = string.Empty;
                    break;
                case nameof(SelectedSecondaryIdx):
                    SelectedTertiaryIdx = 0;
                    Refresh();
                    InputValue = string.Empty;
                    break;
                case nameof(SelectedTertiaryIdx):
                    Refresh();
                    InputValue = string.Empty;
                    break;
                case nameof(InputValue):
                    Refresh();
                    break;
            }
        }

        private void Refresh() {

            OnPropertyChanged(nameof(SecondaryLabels));
            OnPropertyChanged(nameof(TertiaryLabels));
            OnPropertyChanged(nameof(CanInputText));
            OnPropertyChanged(nameof(CanInputDate));
        }

        #endregion

        #region Commands

        #endregion
    }
}
