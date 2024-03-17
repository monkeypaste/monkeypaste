using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileDataObjectItemViewModel : MpAvViewModelBase<MpAvFileItemCollectionViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel {

        #region Properties

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion

        #region State

        public bool IsAvailable =>
            Path.IsFileOrDirectory();
        #endregion

        #region Appearance
        #endregion

        #region Model
        public string Path { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvFileDataObjectItemViewModel() : base(null) { }

        public MpAvFileDataObjectItemViewModel(MpAvFileItemCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(string path) {
            IsBusy = true;

            await Task.Delay(1);
            Path = path;

            IsBusy = false;
        }

        #endregion

        #region Protected Overrides

        #endregion

        #region Commands 

        public ICommand RemoveThisFileItemCommand => new MpCommand(
            () => {
                if (Parent == null) {
                    return;
                }
                Parent.RemoveFileItemCommand.Execute(this);
            });
        #endregion
    }
}
