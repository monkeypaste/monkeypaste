using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvImageAnnotationItemViewModel : MpAvAnnotationItemViewModel {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Model

        public MpImageAnnotationNodeFormat ImageAnnotation =>
            Annotation == null ? null : Annotation as MpImageAnnotationNodeFormat;

        #endregion

        #endregion

        #region Constructors
        public MpAvImageAnnotationItemViewModel(MpAvAnnotationMessageViewModel parent) : base(parent) {
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpAnnotationNodeFormat ianf, MpAvAnnotationItemViewModel parentTreeItem) {
            IsBusy = true;
            await base.InitializeAsync(ianf, parentTreeItem);
            OnPropertyChanged(nameof(ImageAnnotation));
            IsBusy = false;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
