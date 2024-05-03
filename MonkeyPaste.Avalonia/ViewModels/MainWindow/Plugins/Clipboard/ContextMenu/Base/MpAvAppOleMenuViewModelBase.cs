using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvAppOleMenuViewModelBase : MpAvViewModelBase, MpAvIMenuItemViewModel {
        #region Interfaces

        #region MpAvIMenuItemViewModel Implementation
        public object CommandParameter { get; }
        public string InputGestureText { get; }
        public bool StaysOpenOnClick =>
            true;
        public bool IsSubMenuOpen { get; set; }
        public bool HasLeadingSeparator { get; }
        public virtual bool IsThreeState =>
            true;
        public virtual bool IsEnabled =>
            true;
        public bool IsVisible => true;
        public virtual MpMenuItemType MenuItemType =>
            MpMenuItemType.CheckableWithIcon;
        public IEnumerable<MpAvIMenuItemViewModel> SubItems { get; set; }

        public virtual bool? IsChecked {
            get {
                var checkable_sub_items = SubItems.OfType<MpAvAppOleMenuViewModelBase>();

                bool? result =
                    checkable_sub_items.All(x => x.IsChecked.IsTrue()) ?
                        true :
                        checkable_sub_items.Any(x => x.IsChecked.IsTrueOrNull()) ?
                            null : false;
                return result;
            }
        }

        string MpAvIMenuItemViewModel.IconBorderHexColor =>
            MpSystemColors.Transparent;
        
        public bool IsHovering { get; set; }


        ICommand MpAvIMenuItemViewModel.Command =>
            CheckCommand;
        public abstract string Header { get; }
        public abstract object IconSourceObj { get; }

        public abstract MpIAsyncCommand<object> CheckCommand { get; }
        #endregion
        #endregion

        #region Properties

        protected MpAvAppOleReaderOrWriterMenuViewModel RelativeRoot {
            get {
                MpAvAppOleReaderOrWriterMenuViewModel root = null;
                MpAvViewModelBase curObj = this;
                while (root == null) {
                    if (curObj is MpAvAppOleReaderOrWriterMenuViewModel) {
                        root = curObj as MpAvAppOleReaderOrWriterMenuViewModel;
                        break;
                    }
                    curObj = curObj.ParentObj as MpAvViewModelBase;
                    if (curObj == null) {
                        return null;
                    }
                }
                return root;
            }
        }

        protected virtual object MenuArg =>
            (RelativeRoot.ParentObj as MpAvAppOleRootMenuViewModel).MenuArg;

        #endregion

        #region Constructors
        public MpAvAppOleMenuViewModelBase(object parent) : base(parent) { }
        #endregion

        #region Protected Methods

        public void RefreshChecks(bool isSourceCall) {
            OnPropertyChanged(nameof(IsChecked));

            if (isSourceCall && RelativeRoot != null) {
                RelativeRoot
                    .SubItems
                    .OfType<MpAvAppOleMenuViewModelBase>()
                    .ForEach(x => x.RefreshChecks(false));
                RelativeRoot.OnPropertyChanged(nameof(RelativeRoot.IsChecked));

                MpAvClipTrayViewModel.Instance.UpdatePasteInfoMessageCommand.Execute(MenuArg);

                if (MenuArg is MpAvAppViewModel avm) {
                    // avm.OleFormatInfos.InitializeAsync(avm.AppId).FireAndForgetSafeAsync();
                }
                return;
            }
            if (SubItems != null) {
                SubItems.OfType<MpAvAppOleMenuViewModelBase>().ForEach(x => x.RefreshChecks(false));
            }
        }

        #endregion

    }
}
