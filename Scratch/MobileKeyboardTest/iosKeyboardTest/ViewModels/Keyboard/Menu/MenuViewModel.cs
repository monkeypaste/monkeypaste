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
        object _completionLock = new();
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
        public ObservableCollection<string> CompletionItems { get; set; } = [];
        public IEnumerable<string> CompletionDisplayValues {
            get {
                string leading_word = GetLeadingWord(LastTextInfo,sentenceWordsOnly: true);
                foreach(string comp_val in CompletionItems) {
                    yield return GetCompletionDisplayValue(leading_word, comp_val);
                }
            }
        }


        #endregion

        #region Appearance
        string ConfirmIgnoreAutoCorrectCompletionText => "✓";
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

        int MaxVisibleCompletionItems => 3;

        Rect[] _maxCompletionItemRects;
        public Rect[] CompletionItemRects {
            get {
                if(_maxCompletionItemRects == null) {
                    double w = InnerMenuRect.Width / MaxVisibleCompletionItems;
                    double h = InnerMenuRect.Height;
                    _maxCompletionItemRects = new Rect[Parent.MaxCompletionResults];
                    for (int i = 0; i < _maxCompletionItemRects.Length; i++) {
                        double x = InnerMenuRect.Left + i * w;
                        double y = InnerMenuRect.Top;
                        _maxCompletionItemRects[i] = new Rect(x, y, w, h);
                    }
                }
                // NOTE only provide rects for available completions
                return 
                    _maxCompletionItemRects
                    .Take(CompletionItems.Count)
                    .Select(x=>new Rect(x.X - CompletionScrollOffset,x.Y,x.Width,x.Height))
                    .ToArray();
            }
        }

        IEnumerable<Point> _completionItemTextLocs;
        public IEnumerable<Point> CompletionItemTextLocs {
            get {
                foreach(string comp_disp_val in CompletionDisplayValues) {
                    if(InputConnection == null || InputConnection.TextMeasurer == null) {
                        yield return new();
                        continue;
                    }
                    var comp_item_rect =
                        InputConnection.TextMeasurer
                        .MeasureText(comp_disp_val, CompletionItemFontSize, CompletionTextAlignment, out double ascent, out double descent);
                    double cix = comp_item_rect.Center.X;
                    double ciy = comp_item_rect.Center.Y - ((ascent + descent) / 2);
                    yield return new Point(cix, ciy);
                }
            }
        }
        public TextAlignment CompletionTextAlignment =>
            TextAlignment.Center;

        double MinCompletionScrollDisplacement => 3;
        double LastCompletionScrollOffset { get; set; }
        double CompletionScrollDisplacement { get; set; } 
        double CompletionScrollVelocity { get; set; }
        public double CompletionScrollOffset { get; private set; }
        double MinCompletionScrollOffset {
            get {
                if(TouchOwner == default || TouchOwner.ownerType != MenuItemType.CompletionItem) {
                    // default 0
                    return 0;
                }
                return -InnerMenuRect.Width / 4;
            }
        }
        double _maxCompletionScrollOffset = -1;
        public double MaxCompletionScrollOffset {
            get {
                if(_maxCompletionScrollOffset < 0) {
                    _maxCompletionScrollOffset = Math.Max(0,CompletionItemRects.Last().Right - InnerMenuRect.Right);
                }
                return _maxCompletionScrollOffset;
            }
        }

        #endregion

        #region State
        TextRangeInfo LastTextInfo { get; set; }
        bool IsScrolling =>
            CompletionScrollDisplacement > MinCompletionScrollDisplacement;
        int PressedCompletionItemIdx { get; set; } = -1;
        TextRangeInfo LastAutoCorrectRange { get; set; }
        string LastAutoCorrectedIncorrectText { get; set; }
        (MenuItemType ownerType, int ownerIdx) TouchOwner { get; set; }
        string TouchId { get; set; }
        public MenuPageType MenuPageType { get; set; } = MenuPageType.TabSelector;
        public bool IsBackButtonVisible =>
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
            CompletionItems.CollectionChanged += (s, e) => {
                if(CompletionItems.Count > 8) {

                }
            };
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
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    StopScrollAnimation();
                }
                Renderer.Paint(true);
                return true;
            } 

            if(TouchId != touch.Id) {
                return false;
            }

            if(touchType == TouchEventType.Move) {
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    SetCompletionScrollOffset(CompletionScrollOffset + (touch.LastLocation.X - touch.Location.X));
                    Debug.WriteLine($"Offset: {CompletionScrollOffset}");
                }
            } else if(touchType == TouchEventType.Release) {
                if(CanPerformAction(touch)) {
                    PerformMenuAction(TouchOwner);
                }
                if(TouchOwner.ownerType == MenuItemType.CompletionItem) {
                    StartScrollAnimationAsync(touch).FireAndForgetSafeAsync();
                }
                SetPressed(touch, false);                
            }
            Renderer.Render(true);
            return true;
        }
        public void ShowCompletion(TextRangeInfo textInfo) {
           lock (_completionLock) {
                // NOTE lock ensures completion count changes between insert changes
                // don't throw off the renderers references
                if (GetLeadingWord(textInfo) is not { } raw_input ||
                    !TextCorrector.IsLoaded) {
                    return;
                }
                string input = raw_input.Trim();
                CompletionItems.Clear();
                if (string.IsNullOrEmpty(input) && !Parent.IsNextWordCompletionEnabled) {
                    // don't do beginning of word
                    MenuPageType = MenuPageType.TabSelector;
                    return;
                }
                MenuPageType = MenuPageType.Completions;
                ResetCompletionScroll();

                if (CheckForAutoCorrectUndo(textInfo)) {
                    return;
                }

                var results = TextCorrector.GetResults(input.ToLower(), Parent.IsAutoCorrectEnabled, Parent.MaxCompletionResults, out string autoCorrectResult);
                if (Parent.IsAutoCorrectEnabled && 
                    !string.IsNullOrEmpty(autoCorrectResult) &&
                    results.ToList() is { } result_list) {
                    // pre-pend results with check-mark to signify autocorrect pending in menu strip
                    result_list.Insert(0, ConfirmIgnoreAutoCorrectCompletionText);
                    // ensure results within max count
                    results = result_list.Take(Parent.MaxCompletionResults);
                    StartAutoCorrect(textInfo, autoCorrectResult);
                } else if (LastAutoCorrectRange != null && WasSpace(textInfo, LastTextInfo)) {
                    DoAutoCorrect(textInfo);
                }
                CompletionItems.Clear();
                CompletionItems.AddRange(results);

                LastTextInfo = textInfo;

                this.Renderer.Render(true);
            }
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

        #region Private Methods

        #region Touch Actions
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
        void PerformMenuAction((MenuItemType,int) owner) {
            switch (owner.Item1) {
                case MenuItemType.BackButton:
                    GoBack();
                    break;
                case MenuItemType.OptionsButton:
                    InputConnection.OnShowPreferences(null);
                    break;
                case MenuItemType.CompletionItem:
                    DoCompletion(LastTextInfo, CompletionDisplayValues.ElementAt(owner.Item2));                    
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
            for (int i = 0; i < CompletionItemRects.Length; i++) {
                if (CompletionItemRects[i].Contains(touch.Location)) {
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
                    PressedCompletionItemIdx = isPressed ? TouchOwner.Item2 : -1;
                    break;
            }
            TouchId = isPressed ? touch.Id : null;
            TouchOwner = isPressed ? TouchOwner : default;
        }

        #endregion

        #region Insert Helpers

        bool WasSpace(TextRangeInfo curRange, TextRangeInfo lastRange) {
            if (curRange.LeadingText.Length > 0 &&
                curRange.LeadingText.Substring(0, curRange.LeadingText.Length - 1) == lastRange.LeadingText &&
                curRange.LeadingText.Last() == ' ') {
                return true;
            }
            return false;
        }
        bool WasBackspace(TextRangeInfo curRange, TextRangeInfo lastRange) {
            if (lastRange.LeadingText.Length > 0 &&
                curRange.LeadingText == lastRange.LeadingText.Substring(0, lastRange.LeadingText.Length - 1)) {
                return true;
            }
            return false;
        }
        string GetLeadingWord(TextRangeInfo textRange, bool sentenceWordsOnly = false, bool acceptWhitespace = false) {
            if (textRange == null ||
                textRange.LeadingText is not { } leading_text) {
                return string.Empty;
            }
            int leading_word_idx = 0;
            string last_char = string.Empty;
            for (int i = 0; i < leading_text.Length; i++) {
                int text_idx = leading_text.Length - i - 1;
                string cur_char = leading_text[text_idx].ToString();
                if (!sentenceWordsOnly &&
                    Parent.IsCompoundWordBreakEnabled &&
                    !string.IsNullOrEmpty(last_char) &&
                    i + 1 < leading_text.Length) {
                    /*
                    snake_case
                    camelCase
                    PascalCase
                    kebab-case
                    */
                    if (cur_char == "_" ||
                        cur_char == "-" ||
                        cur_char.StartsWithCapitalCaseChar() != last_char.StartsWithCapitalCaseChar()) {
                        leading_word_idx = leading_word_idx + 1;
                        break;
                    }
                }
                int eow_idx = Parent.EndOfWordChars.IndexOf(cur_char);
                if (eow_idx >= 0) {
                    if(acceptWhitespace && Parent.EndOfWordChars[eow_idx] == " ") {
                        // allow whitespace...
                        // needed when replacing autocorrected text since its replaced
                        // AFTER hitting space
                    } else {
                        leading_word_idx = text_idx;
                        break;
                    }
                    
                }
                last_char = cur_char;
            }
            if (leading_word_idx < 0) {
                return string.Empty;
            }
            return leading_text.Substring(leading_word_idx, leading_text.Length - leading_word_idx);
        }
        #endregion

        #region Completion

        string GetCompletionDisplayValue(string leading_word, string comp_val, bool isForMenuStrip = true) {
            //if(isForMenuStrip &&
            //    comp_val == ConfirmIgnoreAutoCorrectCompletionText) {
            //    // don't mess w/ the check mark
            //    return ConfirmIgnoreAutoCorrectCompletionText;
            //}
            string out_val = comp_val;
            //if(comp_val.ToLower().StartsWith(leading_word.ToLower()) {
            //    // a completion (NOT an auto-correct result)
            //    // preprend actual leading to result
            //    out_val = actual_leading_text.Replace(leading_word, comp_val);
            //}
            if (Parent.IsShiftOnLock || (leading_word.Length > 1 && leading_word.IsAllCaps())) {
                return out_val.ToUpper();
            } else if (Parent.IsShiftOnTemp || leading_word.StartsWithCapitalCaseChar()) {
                return out_val.ToTitleCase();
            }

            return out_val;
        }
        void DoCompletion(TextRangeInfo textInfo, string completionText, bool isAutoCorrect = false) {
            if (completionText == ConfirmIgnoreAutoCorrectCompletionText) {
                // cancel auto correct by inserting a space
                InputConnection.OnText(" ");
                return;
            }
            if (GetLeadingWord(textInfo, acceptWhitespace: isAutoCorrect) is { } orig_leading_text) {
                // remove whats being completed
                InputConnection.OnBackspace(orig_leading_text.Length);
            }
            // only insert space if not url/email
            string completion_suffix = Parent.IsEmailLayout || Parent.IsUrlLayout ? string.Empty : " ";
            string output_text = completionText + completion_suffix;
            InputConnection.OnText(output_text);
        }

        #endregion

        #region Auto-Correct
        bool CheckForAutoCorrectUndo(TextRangeInfo curRange) {
            if (LastAutoCorrectRange == null) {
                return false;
            }

            bool is_undo = false;
            if (Parent.IsBackspaceUndoLastAutoCorrectEnabled &&
                WasBackspace(curRange, LastAutoCorrectRange)) {
                is_undo = true;
            } else if (WasSpace(curRange, LastTextInfo)) {
                // auto-correct will occur
                return false;
            }
            FinishAutoCorrect(curRange, is_undo);
            return is_undo;
        }
        void StartAutoCorrect(TextRangeInfo curRange, string autoCorrectedText) {
            // get incorrect text
            string incorrect_text = GetLeadingWord(curRange);
            int inc_len = incorrect_text.Length;
            // format correct text based on incorrect/shift state
            string out_val = GetCompletionDisplayValue(incorrect_text, autoCorrectedText, false);

            // store incorrect text
            LastAutoCorrectedIncorrectText = incorrect_text.Trim();
            // store newly corrected info with corrected text as sel range
            LastAutoCorrectRange = curRange.Clone();
            LastAutoCorrectRange.Select(LastAutoCorrectRange.SelectionStartIdx - inc_len, inc_len);
            LastAutoCorrectRange.SelectedText = out_val;
        }
        void FinishAutoCorrect(TextRangeInfo curRange, bool is_undo) {
            if (is_undo) {
                DoCompletion(curRange, LastAutoCorrectedIncorrectText);
            }
            LastAutoCorrectRange = null;
            LastAutoCorrectedIncorrectText = null;
        }
        void DoAutoCorrect(TextRangeInfo textInfo) {
            DoCompletion(textInfo, LastAutoCorrectRange.SelectedText.Trim(), isAutoCorrect: true);
        }
        #endregion

        #region Scroll
        void SetCompletionScrollOffset(double new_offset) {
            CompletionScrollOffset = Math.Clamp(new_offset, MinCompletionScrollOffset, MaxCompletionScrollOffset);
            CompletionScrollDisplacement += Math.Abs(CompletionScrollOffset - LastCompletionScrollOffset);
            LastCompletionScrollOffset = CompletionScrollOffset;
        }
        async Task StartScrollAnimationAsync(Touch touch) {
            if (!CompletionItemRects.Any()) {
                return;
            }
            CompletionScrollVelocity = touch.Velocity.X;
            int dir = CompletionScrollVelocity > 0 ? 1 : -1;
            int delay = 20;
            double dampening = 0.95d;
            double snap_t = 0.5;
            double max_v = 10;
            double min_v = 0.1;

            while (true) {
                CompletionScrollVelocity = Math.Clamp(CompletionScrollVelocity * dampening, -max_v, max_v);
                if (Math.Abs(CompletionScrollVelocity) < min_v) {
                    // snap
                    double dist = FindSnapCompletionRectDisp();
                    double snap_v = dist / snap_t;
                    while (true) {
                        if (CompletionScrollVelocity == 0) {
                            // canceled
                            return;
                        }
                        SetCompletionScrollOffset(CompletionScrollOffset + snap_v);
                        dist -= snap_v;
                        if (dist == 0 || (dist < 0 && snap_v > 0) || (dist > 0 && snap_v < 0)) {
                            // snap was either exact or now past target
                            SetCompletionScrollOffset(CompletionScrollOffset + dist);
                            StopScrollAnimation();
                            Renderer.Measure(true);
                            return;
                        }
                        Renderer.Measure(true);
                        await Task.Delay(delay);
                    }
                }
                SetCompletionScrollOffset(CompletionScrollOffset - CompletionScrollVelocity);
                Renderer.Measure(true);
                await Task.Delay(delay);
            }
        }
        double FindSnapCompletionRectDisp() {
            var closest_item_rect =
                CompletionItemRects
                .Aggregate((a, b) => Math.Abs(InnerMenuRect.Left - a.Left) < Math.Abs(InnerMenuRect.Left - b.Left) ? a : b);
            return closest_item_rect.Left - InnerMenuRect.Left;
        }
        void StopScrollAnimation() {
            CompletionScrollVelocity = 0;
            CompletionScrollDisplacement = 0;
        }
        void ResetCompletionScroll() {
            StopScrollAnimation();
            LastCompletionScrollOffset = 0;
            CompletionScrollOffset = 0;
        }
        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
