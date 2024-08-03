using Avalonia;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Point = Avalonia.Point;

namespace iosKeyboardTest {
    public class MenuViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {

        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        void IKeyboardViewRenderer.Layout(bool invalidate) {
        }

        void IKeyboardViewRenderer.Measure(bool invalidate) {
            RaisePropertyChanged(nameof(MenuRect));
        }

        void IKeyboardViewRenderer.Paint(bool invalidate) {
        }

        void IKeyboardViewRenderer.Render(bool invalidate) {
        }

        #endregion

        #region IKeyboardRenderSource Implementation
        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        #endregion
        #endregion

        #region Properties

        #region Members
        IKeyboardViewRenderer _renderer;
        public IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        IKeyboardInputConnection InputConnection { get; set; }
        #endregion

        #region View Models
        public KeyboardViewModel Parent { get; private set; }
        public AutoCompleteViewModel AutoCompleteViewModel { get; private set; }


        #endregion

        #region Appearance
        public string MenuBgHexColor => KeyboardPalette.MenuBgHex;
        public string MenuFgHexColor => KeyboardPalette.MenuFgHex;
        public string BackButtonBgHexColor { get; private set; } = KeyboardPalette.MenuBgHex;
        public string OptionsButtonBgHexColor { get; private set; } = KeyboardPalette.MenuBgHex;

        #endregion

        #region Layout
        public Rect MenuRect =>
            Parent.MenuRect;

        Rect _innerMenuRect;
        public Rect InnerMenuRect {
            get {
                if (_innerMenuRect == default) {
                    double w = MenuRect.Width - OptionsButtonRect.Width - BackButtonRect.Width;
                    double h = MenuRect.Height;
                    double x = BackButtonRect.Right;
                    double y = 0;
                    _innerMenuRect = new Rect(x, y, w, h);
                }
                return _innerMenuRect;
            }
        }
        double ButtonMenuWidthRatio => 0.1;
        double ButtonImageSizeRatio => 0.75;

        Rect _backButtonRect;
        public Rect BackButtonRect {
            get {
                if(!IsBackButtonVisible) {
                    return new();
                }
                if(_backButtonRect == default) {
                    double x = 0;
                    double y = 0;
                    double w = MenuRect.Width * ButtonMenuWidthRatio;
                    double h = MenuRect.Height;
                    _backButtonRect = new Rect(x, y, w, h);
                }
                return _backButtonRect;
            }
        }
        
        Rect _backButtonImageRect;
        public Rect BackButtonImageRect {
            get {
                if(!IsBackButtonVisible) {
                    return new();
                }
                if(_backButtonImageRect == default) {
                    double w = Math.Min(BackButtonRect.Width, BackButtonRect.Height) * ButtonImageSizeRatio;
                    double h = w;
                    double x = BackButtonRect.Left + ((BackButtonRect.Width - w) / 2d);
                    double y = BackButtonRect.Top + ((BackButtonRect.Height - h) / 2d);
                    _backButtonImageRect = new Rect(x, y, w, h);
                }
                return _backButtonImageRect;
            }
        }

        Rect _optionsButtonRect;
        public Rect OptionsButtonRect {
            get {
                if(_optionsButtonRect == default) {
                    double w = MenuRect.Width * ButtonMenuWidthRatio;
                    double h = MenuRect.Height;
                    double x = MenuRect.Right - w;
                    double y = MenuRect.Top;
                    _optionsButtonRect = new Rect(x, y, w, h);
                }
                return _optionsButtonRect;
            }
        }
        Rect _optionButtonImageRect;
        public Rect OptionButtonImageRect {
            get {
                if (_optionButtonImageRect == default) {
                    double w = Math.Min(OptionsButtonRect.Width, OptionsButtonRect.Height) * ButtonImageSizeRatio;
                    double h = w;
                    double x = OptionsButtonRect.Left + ((OptionsButtonRect.Width - w) / 2d);
                    double y = OptionsButtonRect.Top + ((OptionsButtonRect.Height - h) / 2d);
                    _optionButtonImageRect = new Rect(x, y, w, h);
                }
                return _optionButtonImageRect;
            }
        }        

        #endregion

        #region State
        public (MenuItemType ownerType, int ownerIdx) TouchOwner { get; set; }
        string TouchId { get; set; }
        public MenuPageType CurMenuPageType { get; set; } = MenuPageType.TabSelector;
        public bool IsBackButtonVisible =>
            CurMenuPageType != MenuPageType.TabSelector; 
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors
        public MenuViewModel(KeyboardViewModel parent) {
            Parent = parent;
            AutoCompleteViewModel = new AutoCompleteViewModel(this);
        }
        #endregion

        #region Public Methods
        public bool HandleMenuTouch(TouchEventType touchType, Touch touch) {
            if(touchType == TouchEventType.Press &&
                TouchId == null &&
                MenuRect.Contains(touch.Location)) {
                SetPressed(touch, true);
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    AutoCompleteViewModel.StopScrollAnimation();
                }
                Renderer.Paint(true);
                return true;
            } 

            if(TouchId != touch.Id) {
                return false;
            }

            if(touchType == TouchEventType.Move) {
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    AutoCompleteViewModel.SetCompletionScrollOffset(
                        AutoCompleteViewModel.CompletionScrollOffset + (touch.LastLocation.X - touch.Location.X));
                    Debug.WriteLine($"Offset: {AutoCompleteViewModel.CompletionScrollOffset}");
                }
            } else if(touchType == TouchEventType.Release) {
                if(CanPerformAction(touch)) {
                    PerformMenuAction(TouchOwner);
                }
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    AutoCompleteViewModel.StartScrollAnimationAsync(touch).FireAndForgetSafeAsync();
                }
                SetPressed(touch, false);                
            }
            Renderer.Render(true);
            return true;
        }
        
        public void GoBack() {
            MenuPageType back_to_page = CurMenuPageType;
            switch (CurMenuPageType) {
                case MenuPageType.Completions:
                    back_to_page = MenuPageType.TabSelector;
                    break;
                case MenuPageType.OtherTab:
                    back_to_page = MenuPageType.TabSelector;
                    break;
            }
            if(back_to_page == CurMenuPageType) {
                return;
            }
            CurMenuPageType = back_to_page;
            this.Renderer.Render(true);
        }

        #endregion

        #region Private Methods

        #region Touch Actions
        bool CanPerformAction(Touch touch) {
            var release_owner = FindTouchOwner(touch);
            if (TouchOwner.ownerType != release_owner.ownerType ||
                TouchOwner.ownerIdx != release_owner.ownerIdx ||
                AutoCompleteViewModel.CompletionScrollDisplacement >= AutoCompleteViewModel.MinCompletionScrollDisplacement) {
                // release not over press or was a scroll
                return false;
            }
            return true;
        }
        void PerformMenuAction((MenuItemType,int) owner) {
            switch (owner.Item1) {
                case MenuItemType.BackButton:
                    GoBack();
                    break;
                case MenuItemType.OptionsButton:
                    InputConnection.OnShowPreferences(null);
                    break;
                case MenuItemType.CompletionItem:
                    AutoCompleteViewModel.DoCompletion(AutoCompleteViewModel.LastTextInfo, AutoCompleteViewModel.CompletionDisplayValues.ElementAt(owner.Item2));                    
                    break;
            }
            
        }        
        (MenuItemType ownerType, int ownerIdx) FindTouchOwner(Touch touch) {
            if(BackButtonRect.Contains(touch.Location)) {
                return (MenuItemType.BackButton, 0);
            }
            if(OptionsButtonRect.Contains(touch.Location)) {
                return (MenuItemType.OptionsButton, 0);
            }
            for (int i = 0; i < AutoCompleteViewModel.CompletionItemRects.Length; i++) {
                if (AutoCompleteViewModel.CompletionItemRects[i].Contains(touch.Location)) {
                    return (MenuItemType.CompletionItem, i);
                }
            }
            return default;
        }
        void SetPressed(Touch touch,bool isPressed) {
            if(TouchOwner == default) {
                TouchOwner = FindTouchOwner(touch);
            }
            string new_bg_color = isPressed ? KeyboardPalette.MenuItemPressedBgHex : KeyboardPalette.MenuBgHex;
            switch(TouchOwner.Item1) {
                case MenuItemType.BackButton:
                    BackButtonBgHexColor = new_bg_color;
                    break;
                case MenuItemType.OptionsButton:
                    OptionsButtonBgHexColor = new_bg_color;
                    break;
                case MenuItemType.CompletionItem:
                    AutoCompleteViewModel.PressedCompletionItemIdx = isPressed ? TouchOwner.Item2 : -1;
                    break;
            }
            TouchId = isPressed ? touch.Id : null;
            TouchOwner = isPressed ? TouchOwner : default;
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
