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
        ContentType,
        Collection,
        Title,
        Source,
        Time
        //CopiedDate,
        //PastedDate,
        //ModifiedDate
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

        public ObservableCollection<string> CriteriaTypeLabels {
            get {
                return new ObservableCollection<string>() {
                    "Content",
                    "Content Type",
                    "Collection",
                    "Title",
                    "Source",
                    "Time"
                    //"Copied Date",
                    //"Pasted Date",
                    //"Modified Date"
                };
            }
        }

        public ObservableCollection<string> SecondaryLabels {
            get {
                switch (SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Title:
                    case MpSearchCriteriaPropertyType.Content:
                        return TextUnitOptionLabels;
                    case MpSearchCriteriaPropertyType.ContentType:
                        return ContentTypeOptionLabels;

                    //case MpSearchCriteriaPropertyType.ContentType:
                    //case MpSearchCriteriaPropertyType.Collection:
                    //case MpSearchCriteriaPropertyType.Source:
                    //    return MpSearchCriteriaUnitType.Text | MpSearchCriteriaUnitType.Enumerable;

                    //case MpSearchCriteriaPropertyType.Time:
                    //    return MpSearchCriteriaUnitType.Text |
                    //           MpSearchCriteriaUnitType.Number |
                    //           MpSearchCriteriaUnitType.DateTime |
                    //           MpSearchCriteriaUnitType.TimeSpan |
                    //           MpSearchCriteriaUnitType.Enumerable;

                    default: return null;
                }
            }
        }

        public ObservableCollection<string> TertiaryOptionLabels {
            get {
                return null;
            }
        }

        public ObservableCollection<string> TextUnitOptionLabels {
            get {
                return new ObservableCollection<string>() {
                    "Matches",
                    "Contains",
                    "Begins With",
                    "Ends With"
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

        //public bool HasTertiaryLabels => SecondaryLabels != null && SecondaryLabels[SelectedSecondaryIdx] ;

        public bool IsSelected { get; set; } = false;

        public bool IsHovering { get; set; } = false;

        public int SelectedCriteriaTypeIdx { get; set; } = 0;

        public int SelectedSecondaryIdx { get; set; } = 0;

        public MpSearchCriteriaPropertyType SelectedCriteriaType => (MpSearchCriteriaPropertyType)SelectedCriteriaTypeIdx;

        public MpSearchCriteriaUnitType SearchCriteriaUnitTypeFlags {
            get {
                switch(SelectedCriteriaType) {
                    case MpSearchCriteriaPropertyType.Title:
                    case MpSearchCriteriaPropertyType.Content:
                        return MpSearchCriteriaUnitType.Text;

                    case MpSearchCriteriaPropertyType.ContentType:
                    case MpSearchCriteriaPropertyType.Collection:
                    case MpSearchCriteriaPropertyType.Source:
                        return MpSearchCriteriaUnitType.Text | MpSearchCriteriaUnitType.Enumerable;

                    case MpSearchCriteriaPropertyType.Time:
                        return MpSearchCriteriaUnitType.Text | 
                               MpSearchCriteriaUnitType.Number | 
                               MpSearchCriteriaUnitType.DateTime |
                               MpSearchCriteriaUnitType.TimeSpan |
                               MpSearchCriteriaUnitType.Enumerable;

                    default: return MpSearchCriteriaUnitType.None;
                }
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
            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpSearchCriteriaItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedCriteriaTypeIdx):
                    OnPropertyChanged(nameof(SecondaryLabels));
                    break;
            }
        }

        #endregion

        #region Commands

        #endregion
    }
}
