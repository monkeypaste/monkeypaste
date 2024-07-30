using Avalonia;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace iosKeyboardTest.iOS {
    public enum MenuPageType {
        None = 0,
        TabSelector,
        Completions,
        OtherTab
    }
    public enum MenuItemType {
        None = 0,
        BackButton,
        OptionsButton,
        CompletionItem,
        OtherTabItem
    }
    public class MenuViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {

        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        public void Layout(bool invalidate) {
        }

        public void Measure(bool invalidate) {
            RaisePropertyChanged(nameof(MenuRect));
        }

        public void Paint(bool invalidate) {
        }

        public void Render(bool invalidate) {
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
        IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        IKeyboardInputConnection InputConnection { get; set; }
        #endregion

        #region View Models
        public KeyboardViewModel Parent { get; private set; }
        public ObservableCollection<string> CompletionItems { get; set; } = [];
        public IEnumerable<string> CompletionDisplayValues {
            get {
                string leading_word = GetLeadingWord(LastInfo,false);
                //string actual_leading_text = GetLeadingWord(LastInfo,true);
                foreach(string comp_val in CompletionItems) {
                    string out_val = comp_val;
                    //if(comp_val.ToLower().StartsWith(leading_word.ToLower()) {
                    //    // a completion (NOT an auto-correct result)
                    //    // preprend actual leading to result
                    //    out_val = actual_leading_text.Replace(leading_word, comp_val);
                    //}
                    if(Parent.IsShiftOnLock || (leading_word.IsAllCaps() && leading_word.Length > 1)) {
                        yield return out_val.ToUpper();
                    } else if(Parent.IsShiftOnTemp || leading_word.StartsWithCapitalCaseChar()) {
                        yield return out_val.ToTitleCase();
                    } else {
                        yield return out_val;
                    }

                }
            }
        }
            
        #endregion

        #region Appearance
        public string MenuBgHexColor => KeyboardPalette.MenuBgHex;
        public string MenuFgHexColor => KeyboardPalette.MenuFgHex;
        public string BackButtonBgHexColor { get; private set; } = KeyboardPalette.MenuBgHex;
        public string OptionsButtonBgHexColor { get; private set; } = KeyboardPalette.MenuBgHex;
        public IEnumerable<string> CompletionItemBgHexColors {
            get {
                for (int i = 0; i < CompletionItems.Count; i++) {
                    if(i == PressedCompletionItemIdx && !IsScrolling) {
                        yield return KeyboardPalette.MenuItemPressedBgHex;
                    } else {
                        yield return KeyboardPalette.MenuBgHex;
                    }
                }
            }
        }

        #endregion

        #region Layout
        public double CompletionItemFontSize => 16;
        public Rect MenuRect =>
            Parent.MenuRect;
        double ButtonWidthRatio => 0.1;

        Rect _backButtonRect;
        public Rect BackButtonRect {
            get {
                if(!IsBackButtonVisible) {
                    return new();
                }
                if(_backButtonRect == default) {
                    double x = 0;
                    double y = 0;
                    double w = MenuRect.Width * ButtonWidthRatio;
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
                    double w = Math.Min(BackButtonRect.Width, BackButtonRect.Height);// - 5;
                    double h = w;
                    double x = BackButtonRect.Left + (BackButtonRect.Width - w) / 2;
                    double y = BackButtonRect.Top + (BackButtonRect.Height - h) / 2;
                    _backButtonImageRect = new Rect(x, y, w, h);
                }
                return _backButtonImageRect;
            }
        }

        Rect _optionsButtonRect;
        public Rect OptionsButtonRect {
            get {
                if(_optionsButtonRect == default) {
                    double w = MenuRect.Width * ButtonWidthRatio;
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
                    double w = Math.Min(OptionsButtonRect.Width, OptionsButtonRect.Height);// - 5;
                    double h = w;
                    double x = OptionsButtonRect.Left + (OptionsButtonRect.Width - w) / 2;
                    double y = OptionsButtonRect.Top + (OptionsButtonRect.Height - h) / 2;
                    _optionButtonImageRect = new Rect(x, y, w, h);
                }
                return _optionButtonImageRect;
            }
        }

        Rect _innerMenuRect;
        public Rect InnerMenuRect {
            get {
                if(_innerMenuRect == default) {
                    double w = MenuRect.Width - OptionsButtonRect.Width - BackButtonRect.Width;
                    double h = MenuRect.Height;
                    double x = BackButtonRect.Right;
                    double y = 0;
                    _innerMenuRect = new Rect(x, y, w, h);
                }
                return _innerMenuRect;
            }
        }

        int MaxVisibleCompletionItems => 3;

        Rect[] _completionItemBaseRects;
        public Rect[] CompletionItemRects {
            get {
                if(_completionItemBaseRects == null) {
                    double w = InnerMenuRect.Width / MaxVisibleCompletionItems;
                    double h = InnerMenuRect.Height;
                    _completionItemBaseRects = new Rect[Parent.MaxCompletionResults];
                    for (int i = 0; i < _completionItemBaseRects.Length; i++) {
                        double x = InnerMenuRect.Left + i * w;
                        double y = InnerMenuRect.Top;
                        _completionItemBaseRects[i] = new Rect(x, y, w, h);
                    }
                }
                return _completionItemBaseRects.Select(x=>new Rect(x.X - CompletionScrollOffset,x.Y,x.Width,x.Height)).ToArray();
            }
        }
        double MinCompletionScrollDisplacement => 3;
        double LastCompletionScrollOffset { get; set; }
        double CompletionScrollDisplacement { get; set; } 
        public double CompletionScrollOffset { get; private set; }
        double _maxCompletionScrollOffset = -1;
        public double MaxCompletionScrollOffset {
            get {
                if(_maxCompletionScrollOffset < 0) {
                    _maxCompletionScrollOffset = CompletionItemRects.Last().Right;
                }
                return _maxCompletionScrollOffset;
            }
        }

        List<(MenuItemType, Rect)> _hitRects;
        List<(MenuItemType,Rect)> ItemHitRectLookup {
            get {
                if(_hitRects == null) {
                    _hitRects = new List<(MenuItemType, Rect)>() {
                        (MenuItemType.BackButton, BackButtonRect ),
                        (MenuItemType.OptionsButton, OptionsButtonRect ),
                    };
                    foreach(var cir in CompletionItemRects) {
                        _hitRects.Add((MenuItemType.CompletionItem, cir));
                    }
                    // TODO add otherTabItems here
                }
                return _hitRects;
            }
        }
        #endregion

        #region State
        TextRange LastInfo { get; set; }
        bool IsScrolling =>
            CompletionScrollDisplacement > MinCompletionScrollDisplacement;
        int PressedCompletionItemIdx { get; set; } = -1;
        public TextRange LastAutoCorrectRange { get; set; }
        (MenuItemType ownerType, int ownerIdx) TouchOwner { get; set; }
        string TouchId { get; set; }
        public MenuPageType MenuPageType { get; set; } = MenuPageType.TabSelector;
        bool IsBackButtonVisible =>
            MenuPageType != MenuPageType.TabSelector; 
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events

        #endregion

        #region Constructors
        public MenuViewModel(KeyboardViewModel parent) {
            Parent = parent;
        }

        public void SetInputConnection(IKeyboardInputConnection conn) {
            InputConnection = conn;

            //Task.Run(() => {
                TextCorrector.Init(InputConnection.AssetLoader);

            //}).FireAndForgetSafeAsync();
        }
        #endregion

        #region Public Methods
        public bool HandleMenuTouch(TouchEventType touchType, Touch touch) {
            if(touchType == TouchEventType.Press &&
                TouchId == null &&
                MenuRect.Contains(touch.Location)) {
                SetPressed(touch, true);
                Renderer.Paint(true);
                return true;
            } 

            if(TouchId != touch.Id) {
                return false;
            }

            if(touchType == TouchEventType.Move) {
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    CompletionScrollOffset = 
                        Math.Clamp(
                            CompletionScrollOffset + (touch.PressLocation.X - touch.Location.X), 
                            0, 
                            MaxCompletionScrollOffset);

                    CompletionScrollDisplacement += Math.Abs(CompletionScrollOffset - LastCompletionScrollOffset);
                    LastCompletionScrollOffset = CompletionScrollOffset;
                    Debug.WriteLine($"Offset: {CompletionScrollOffset}");
                }
            } else if(touchType == TouchEventType.Release) {
                if(CanPerformAction(touch)) {
                    PerformMenuAction(TouchOwner);
                }
                SetPressed(touch, false);                
            }
            Renderer.Render(true);
            return true;
        }

        public void ShowCompletion(TextRange textInfo) {
            //Task.Run(() => {
            if (GetLeadingWord(textInfo) is not { } input ||
                !TextCorrector.IsLoaded) {
                return;
            }
            CompletionItems.Clear();
            if(string.IsNullOrEmpty(input) && !Parent.IsNextWordCompletionEnabled) {
                // don't do beginning of word
                MenuPageType = MenuPageType.TabSelector;
                return;
            }

            MenuPageType = MenuPageType.Completions;
            LastInfo = textInfo;
            LastCompletionScrollOffset = 0;
            CompletionScrollOffset = 0;
            CompletionScrollDisplacement = 0;

            if(LastAutoCorrectRange != null) {
                // TODO? may need to check if input is space here, if so don't clear LastAutoCorrect
                if(Parent.IsBackspaceUndoLastAutoCorrectEnabled &&
                    WasBackspace(textInfo,LastAutoCorrectRange) &&
                    GetLeadingWord(textInfo) is { } auto_corrected_word) {
                    // delete auto corrected word
                    InputConnection.OnBackspace(auto_corrected_word.Length);
                    LastAutoCorrectRange = null;
                    return;
                }

                LastAutoCorrectRange = null;
            }
            var results = TextCorrector.GetResults(input.ToLower(), Parent.IsAutoCorrectEnabled, Parent.MaxCompletionResults, out string autoCorrectResult);
            if (Parent.IsAutoCorrectEnabled && !string.IsNullOrEmpty(autoCorrectResult)) {
                LastAutoCorrectRange = textInfo.Clone();
                LastAutoCorrectRange.SelectedText = autoCorrectResult;
                LastAutoCorrectRange.Select(LastAutoCorrectRange.SelectionEndIdx, 0);
                InputConnection.OnText(autoCorrectResult);
            }
            CompletionItems.AddRange(results);
            //InputConnection.MainThread.Post(() => {
                this.Renderer.Render(true);
            //});
            //});
        }

        public void GoBack() {
            MenuPageType back_to_page = MenuPageType;
            switch (MenuPageType) {
                case MenuPageType.Completions:
                    back_to_page = MenuPageType.TabSelector;
                    break;
                case MenuPageType.OtherTab:
                    back_to_page = MenuPageType.TabSelector;
                    break;
            }
            if(back_to_page == MenuPageType) {
                return;
            }
            MenuPageType = back_to_page;
            this.Renderer.Render(true);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        bool CanPerformAction(Touch touch) {
            var release_owner = FindTouchOwner(touch);
            if (TouchOwner.ownerType != release_owner.ownerType ||
                TouchOwner.ownerIdx != release_owner.ownerIdx ||
                CompletionScrollDisplacement >= MinCompletionScrollDisplacement) {
                // release not over press or was a scroll
                return false;
            }
            return true;
        }
        bool WasBackspace(TextRange curRange,TextRange lastRange) {
            if(lastRange.LeadingText.Length > 0 &&
                curRange.LeadingText == lastRange.LeadingText.Substring(0,lastRange.LeadingText.Length - 1)) {
                return true;
            }
            return false;
        }
        string GetLeadingWord(TextRange textRange, bool ignoreCompound = false) {
            if(textRange == null ||
                textRange.LeadingText is not { } leading_text) {
                return string.Empty;
            }
            int leading_word_idx = 0;
            string last_char = string.Empty;
            for (int i = 0; i < leading_text.Length; i++) {
                int text_idx = leading_text.Length - i - 1;
                string cur_char = leading_text[text_idx].ToString();
                if(!ignoreCompound &&
                    Parent.IsCompoundWordBreakEnabled && 
                    !string.IsNullOrEmpty(last_char) &&
                    i + 1 <leading_text.Length) {
                    /*
                    snake_case
                    camelCase
                    PascalCase
                    kebab-case
                    */
                    if(cur_char == "_" || 
                        cur_char == "-" ||
                        cur_char.StartsWithCapitalCaseChar() != last_char.StartsWithCapitalCaseChar()) {
                        leading_word_idx = leading_word_idx + 1;
                        break;
                    }
                }
                if(Parent.EndOfWordChars.IndexOf(cur_char) >= 0) {
                    leading_word_idx = text_idx;
                    break;
                }
                last_char = cur_char;
            }
            if(leading_word_idx < 0) {
                return string.Empty;
            }
            return leading_text.Substring(leading_word_idx, leading_text.Length - leading_word_idx);
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
                    DoCompletion(CompletionDisplayValues.ElementAt(owner.Item2));                    
                    break;
            }
            
        }
        void DoCompletion(string completionText) {
            if(GetLeadingWord(LastInfo) is { } orig_leading_text) {
                // remove whats being completed
                InputConnection.OnBackspace(orig_leading_text.Length);
            }
            // only insert space if not url/email
            string completion_suffix = Parent.IsEmailLayout || Parent.IsUrlLayout ? string.Empty : " ";
            string output_text = completionText + completion_suffix;
            InputConnection.OnText(output_text);
        }
        (MenuItemType ownerType, int ownerIdx) FindTouchOwner(Touch touch) {
            for (int i = 0; i < ItemHitRectLookup.Count; i++) {
                var kvp = ItemHitRectLookup[i];
                if (kvp.Item2.Contains(touch.Location)) {
                    if (kvp.Item1 == MenuItemType.CompletionItem) {
                        int item_idx = i - 2;
                        return (MenuItemType.CompletionItem, item_idx);
                    }
                    return (kvp.Item1, 0);
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
                    PressedCompletionItemIdx = isPressed ? TouchOwner.Item2 : -1;
                    break;
            }
            TouchId = isPressed ? touch.Id : null;
            TouchOwner = isPressed ? TouchOwner : default;
        }
        #endregion

        #region Commands
        #endregion
    }
}
