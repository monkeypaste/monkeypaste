using Avalonia;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvNullableBoolToUnsetValueConverter : IValueConverter {
        public static readonly MpAvNullableBoolToUnsetValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                return boolVal;
            }
            return AvaloniaProperty.UnsetValue;

        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                return boolVal;
            }
            return null;
        }
    }
    public abstract class MpAvAppOleMenuViewModelBase : MpAvViewModelBase,
        MpAvIMenuItemViewModel {
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
        public bool IsVisible => true;
        public virtual MpMenuItemType MenuItemType =>
            MpMenuItemType.Checkable;
        public IEnumerable<MpAvIMenuItemViewModel> SubItems { get; set; }

        public virtual bool? IsChecked {
            get {

                if (SubItems == null) {
                    return false;
                }
                return SubItems.Any(x => x.IsChecked.IsTrueOrNull()) ? null : false;
            }
            set => throw new NotImplementedException();
        }

        //protected bool ContainsCheck(MpAvIMenuItemViewModel mivm) {

        //}

        public abstract ICommand Command { get; }
        public abstract string Header { get; }
        public abstract object IconSourceObj { get; }
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
            if (isSourceCall && RelativeRoot != null) {
                RelativeRoot
                    .SubItems
                    .OfType<MpAvAppOleMenuViewModelBase>()
                    .ForEach(x => x.RefreshChecks(false));
                return;
            }
            OnPropertyChanged(nameof(IsChecked));
            if (SubItems != null) {
                SubItems.OfType<MpAvAppOleMenuViewModelBase>().ForEach(x => x.RefreshChecks(false));
            }
        }

        #endregion


        public ICommand ToggleCheckedCommand => new MpCommand(
            () => {

            });

    }
}
