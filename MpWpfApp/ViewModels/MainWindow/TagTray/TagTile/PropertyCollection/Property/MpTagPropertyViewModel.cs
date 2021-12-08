using MonkeyPaste;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpTagPropertyViewModel : MpViewModelBase<MpTagPropertyCollectionViewModel> {
        #region Properties

        #region Model

        public int TagId {
            get {
                if(TagProperty == null) {
                    return 0;
                }
                return TagProperty.TagId;
            }
        }

        public string PropertyData {
            get {
                if(TagProperty == null) {
                    return string.Empty;
                }
                return TagProperty.PropertyData;
            }
            set {
                if(PropertyData != value) {
                    TagProperty.PropertyData = value;
                    OnPropertyChanged(nameof(PropertyData));
                }
            }
        }

        public MpTagPropertyType PropertyType {
            get {
                if (TagProperty == null) {
                    return MpTagPropertyType.None;
                }
                return TagProperty.PropertyType;
            }
            set {
                if (PropertyType != value) {
                    TagProperty.PropertyType = value;
                    OnPropertyChanged(nameof(PropertyType));
                }
            }
        }

        public MpTagProperty TagProperty { get; private set; }

        #endregion

        #endregion

        #region Constructors
        public MpTagPropertyViewModel() : base(null) { }

        public MpTagPropertyViewModel(MpTagPropertyCollectionViewModel parent) : base(parent) {
        }

        public virtual async Task InitializeAsync(MpTagProperty tp) {
            IsBusy = true;

            await Task.Delay(1);

            TagProperty = tp;

            IsBusy = false;
        }

        #endregion
    }
}
