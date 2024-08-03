using Avalonia;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace iosKeyboardTest {
    public class AutoCompleteViewModel : ViewModelBase, IKeyboardViewRenderer, IKeyboardRenderSource {

        #region Private Variables
        object _completionLock = new();
        IKeyboardViewRenderer _renderer;
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
            
        }

        public void Paint(bool invalidate) {
            
        }

        public void Render(bool invalidate) {
            
        }

        #endregion

        #region Interfaces

        public void SetRenderer(IKeyboardViewRenderer renderer) {
            _renderer = renderer;
        }
        #endregion

        #endregion

        #region Properties

        #region Members
        IKeyboardViewRenderer Renderer =>
            _renderer ?? this;
        IKeyboardInputConnection InputConnection =>
            Parent.Parent.InputConnection;
        #endregion

        #region View Models
        public MenuViewModel Parent { get; private set; }
        public KeyboardViewModel KeyboardViewModel =>
            Parent.Parent;

        public ObservableCollection<string> CompletionItems { get; set; } = [];
        public IEnumerable<string> CompletionDisplayValues {
            get {
                string leading_word = GetLeadingWord(LastTextInfo, sentenceWordsOnly: true);
                foreach (string comp_val in CompletionItems) {
                    yield return GetCompletionDisplayValue(leading_word, comp_val);
                }
            }
        }
        #endregion

        #region Appearance
        public string AutoCompleteBgHexColor =>
            Parent.MenuBgHexColor;
        public string AutoCompleteFgHexColor =>
            Parent.MenuFgHexColor;
        string ConfirmIgnoreAutoCorrectCompletionText => "✓";

        public IEnumerable<string> CompletionItemBgHexColors {
            get {
                for (int i = 0; i < CompletionItems.Count; i++) {
                    if (i == PressedCompletionItemIdx && !IsScrolling) {
                        yield return KeyboardPalette.MenuItemPressedBgHex;
                    } else {
                        yield return KeyboardPalette.MenuBgHex;
                    }
                }
            }
        }
        #endregion

        #region Layout
        public Rect AutoCompleteRect =>
            Parent.InnerMenuRect;
        public double CompletionItemFontSize => 16;
        int MaxVisibleCompletionItems => 3;

        Rect[] _maxCompletionItemRects;
        public Rect[] CompletionItemRects {
            get {
                if (_maxCompletionItemRects == null) {
                    double w = AutoCompleteRect.Width / MaxVisibleCompletionItems;
                    double h = AutoCompleteRect.Height;
                    _maxCompletionItemRects = new Rect[KeyboardViewModel.MaxCompletionResults];
                    for (int i = 0; i < _maxCompletionItemRects.Length; i++) {
                        double x = /*AutoCompleteRect.Left + */i * w;
                        double y = /*AutoCompleteRect.Top*/0;
                        _maxCompletionItemRects[i] = new Rect(x, y, w, h);
                    }
                }
                // NOTE only provide rects for available completions
                return
                    _maxCompletionItemRects
                    .Take(CompletionItems.Count)
                    .Select(x => new Rect(x.X - CompletionScrollOffset, x.Y, x.Width, x.Height))
                    .ToArray();
            }
        }

        IEnumerable<Point> _completionItemTextLocs;
        public IEnumerable<Point> CompletionItemTextLocs {
            get {
                foreach (string comp_disp_val in CompletionDisplayValues) {
                    if (InputConnection == null || InputConnection.TextMeasurer == null) {
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

        public double MinCompletionScrollDisplacement => 3;
        double LastCompletionScrollOffset { get; set; }
        public double CompletionScrollDisplacement { get; set; }
        double CompletionScrollVelocity { get; set; }
        public double CompletionScrollOffset { get; private set; }
        double ScrollStretchDist => AutoCompleteRect.Width / 4;

        double MinCompletionScrollOffset {
            get {
                if(IsTouchOwner) {
                    return -ScrollStretchDist;
                }
                return 0;
            }
        }
        double _maxCompletionScrollOffset = -1;
        public double MaxCompletionScrollOffset {
            get {
                if(IsTouchOwner) {
                    return ScrollStretchDist;
                }
                if (_maxCompletionScrollOffset < 0) {
                    _maxCompletionScrollOffset = Math.Max(0, CompletionItemRects.Last().Right - AutoCompleteRect.Right);
                }
                return _maxCompletionScrollOffset;
            }
        }
        #endregion

        #region State
        bool IsTouchOwner =>
            Parent.TouchOwner != default && Parent.TouchOwner.ownerType == MenuItemType.CompletionItem;
        public TextRangeInfo LastTextInfo { get; set; }
        bool IsScrolling =>
            CompletionScrollDisplacement > MinCompletionScrollDisplacement;
        public int PressedCompletionItemIdx { get; set; } = -1;
        TextRangeInfo LastAutoCorrectRange { get; set; }
        string LastAutoCorrectedIncorrectText { get; set; }
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors
        public AutoCompleteViewModel(MenuViewModel parent) {
            Parent = parent;

            KeyboardViewModel.OnInputConnectionChanged += (s, e) => {
                TextCorrector.Init(InputConnection.AssetLoader);
            };

            CompletionItems.CollectionChanged += (s, e) => {
                if (CompletionItems.Count > 8) {

                }
            };
        }
        #endregion

        #region Public Methods
        public void ShowCompletion(TextRangeInfo textInfo) {
            //lock (_completionLock) {
                // NOTE lock ensures completion count changes between insert changes
                // don't throw off the renderers references
                if (GetLeadingWord(textInfo) is not { } raw_input ||
                    !TextCorrector.IsLoaded) {
                    return;
                }
                string input = raw_input.Trim();
                CompletionItems.Clear();
                if (string.IsNullOrEmpty(input) && !KeyboardViewModel.IsNextWordCompletionEnabled) {
                    // don't do beginning of word
                    Parent.CurMenuPageType = MenuPageType.TabSelector;
                    return;
                }
                Parent.CurMenuPageType = MenuPageType.Completions;
                ResetCompletionScroll();

                if (CheckForAutoCorrectUndo(textInfo)) {
                    return;
                }

                var results = TextCorrector.GetResults(input.ToLower(), KeyboardViewModel.IsAutoCorrectEnabled, KeyboardViewModel.MaxCompletionResults, out string autoCorrectResult);
                if(KeyboardViewModel.IsAutoCorrectEnabled) {
                    if (!string.IsNullOrEmpty(autoCorrectResult) &&
                    results.ToList() is { } result_list) {
                        // pre-pend results with check-mark to signify autocorrect pending in menu strip
                        result_list.Insert(0, ConfirmIgnoreAutoCorrectCompletionText);
                        // ensure results within max count
                        results = result_list.Take(KeyboardViewModel.MaxCompletionResults);
                        StartAutoCorrect(textInfo, autoCorrectResult);
                    } else if (LastAutoCorrectRange != null && WasSpace(textInfo, LastTextInfo)) {
                        DoAutoCorrect(textInfo);
                    }
                }
                
                CompletionItems.Clear();
                CompletionItems.AddRange(results);

                LastTextInfo = textInfo;

                this.Renderer.Render(true);
            //}
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

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
                    KeyboardViewModel.IsCompoundWordBreakEnabled &&
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
                int eow_idx = KeyboardViewModel.EndOfWordChars.IndexOf(cur_char);
                if (eow_idx >= 0) {
                    if (acceptWhitespace && KeyboardViewModel.EndOfWordChars[eow_idx] == " ") {
                        // allow whitespace...
                        // needed when replacing autocorrected text since its replaced
                        // AFTER hitting space
                    } else {
                        if(i > 0) {
                            leading_word_idx = text_idx + 1;
                        } else {
                            leading_word_idx = -1;
                        }
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

            if (KeyboardViewModel.IsShiftOnLock || (leading_word.Length > 1 && leading_word.IsAllCaps())) {
                return out_val.ToUpper();
            } else if (KeyboardViewModel.IsShiftOnTemp || leading_word.StartsWithCapitalCaseChar()) {
                return out_val.ToTitleCase();
            }

            return out_val;
        }
        public void DoCompletion(TextRangeInfo textInfo, string completionText, bool isAutoCorrect = false) {
            if (completionText == ConfirmIgnoreAutoCorrectCompletionText) {
                // cancel auto correct by inserting a space
                FinishAutoCorrect(textInfo, false);
                InputConnection.OnText(" ");
                return;
            }
            if (GetLeadingWord(textInfo, acceptWhitespace: isAutoCorrect) is { } orig_leading_text) {
                // remove whats being completed
                InputConnection.OnBackspace(orig_leading_text.Length);
            }
            // only insert space if not url/email
            string completion_suffix = KeyboardViewModel.IsEmailLayout || KeyboardViewModel.IsUrlLayout ? string.Empty : " ";
            string output_text = completionText + completion_suffix;
            InputConnection.OnText(output_text);
        }

        #endregion

        #region Auto-Correct
        bool CheckForAutoCorrectUndo(TextRangeInfo curRange) {
            if (!KeyboardViewModel.IsAutoCorrectEnabled ||
                LastAutoCorrectRange == null) {
                return false;
            }

            bool is_undo = false;
            if (KeyboardViewModel.IsBackspaceUndoLastAutoCorrectEnabled &&
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
        public void SetCompletionScrollOffset(double new_offset) {
            CompletionScrollOffset = Math.Clamp(new_offset, MinCompletionScrollOffset, MaxCompletionScrollOffset);
            CompletionScrollDisplacement += Math.Abs(CompletionScrollOffset - LastCompletionScrollOffset);
            LastCompletionScrollOffset = CompletionScrollOffset;
        }
        public async Task StartScrollAnimationAsync(Touch touch) {
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
        public void StopScrollAnimation() {
            CompletionScrollVelocity = 0;
            CompletionScrollDisplacement = 0;
        }
        double FindSnapCompletionRectDisp() {
            var closest_item_rect =
                CompletionItemRects
                .Aggregate((a, b) => Math.Abs(AutoCompleteRect.Left - a.Left) < Math.Abs(AutoCompleteRect.Left - b.Left) ? a : b);
            return closest_item_rect.Left - AutoCompleteRect.Left;
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
