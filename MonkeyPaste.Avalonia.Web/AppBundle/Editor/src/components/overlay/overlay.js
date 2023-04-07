// #region Globals

var IsHighlightingVisible = false;

var HighlightRects = [];
var SelectedHighlightRectIdx = -1;

var CaretBlinkTickMs = 500;
var IsCaretBlinkOn = false;
var CaretBlinkOffColor = null;
var CaretBlinkTimerInterval = null;

// #endregion Globals

// #region Life Cycle

function initOverlay() {
    if (CaretBlinkTimerInterval == null) {
        CaretBlinkTimerInterval = setInterval(onCaretBlinkTick, CaretBlinkTickMs, getOverlayElement());
    }
}
// #endregion Life Cycle

// #region Getters

function getOverlayContext() {
    let overlayCanvas = getOverlayElement();
    if (!overlayCanvas || !overlayCanvas.getContext) {
        return null;
    }
    return overlayCanvas.getContext('2d');
}

function getOverlayElement() {
    return document.getElementById('overlayCanvas');
}

function getPreviewLines(drop_idx, block_state) {
    if (drop_idx < 0) {
        return [];
    }

    let block_line_offset = 0;// 3.0;
    let editor_rect = getEditorContainerRect(false);
    //let editor_rect = getWindowRect();

    let line_start_idx = getLineStartDocIdx(drop_idx);
    let line_start_rect = getCharacterRect(line_start_idx);
    //if (line_start_idx == 0) {
    //    line_start_rect.top += 3;
    //    line_start_rect.bottom += 3;
    //}
    let pre_y = line_start_rect.top - block_line_offset;
    let pre_line = { x1: 0, y1: Math.max(0,pre_y), x2: editor_rect.width, y2: Math.max(0,pre_y) };

    let line_end_idx = getLineEndDocIdx(drop_idx);
    let line_end_rect = getCharacterRect(line_end_idx);
    let post_y = line_end_rect.bottom + block_line_offset;
    let post_line = { x1: 0, y1: post_y, x2: editor_rect.width, y2: post_y };

    let caret_rect = getCharacterRect(drop_idx);
    let caret_line = getCaretLine(drop_idx);
    caret_line.ignoreLineStyle = true;

    let render_lines = [];
    switch (block_state) {
        case 'split':
            let pre_split_line = pre_line;
            pre_split_line.x1 = caret_line.x1;

            let post_split_line = post_line;
            post_split_line.x2 = caret_line.x1;

            render_lines.push(pre_split_line);
            render_lines.push(post_split_line);

            render_lines.push(caret_line);
            break;
        case 'pre':
            render_lines.push(pre_line);
            break;
        case 'post':

            render_lines.push(post_line);
            break;
        case 'inline':
        default:
            //render_caret_line = caret_line;
            render_lines.push(caret_line);
            break;
    }
    return render_lines;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updateOverlayBounds() {
    let editorRect = getEditorContainerRect();
    let window_rect = getWindowRect();

    let overlayCanvas = getOverlayElement();
    overlayCanvas.style.left = window_rect.left;
    overlayCanvas.style.top = editorRect.top;
    overlayCanvas.width = window_rect.width;
    overlayCanvas.height = window_rect.height;
}

// #region Draws

function drawDropPreview(ctx, color = 'red', thickness = 1.0, line_style = [5, 5]) {
    if (isDragCopy()) {
        color = 'lime';
    }
    let drop_block_state = getDropBlockState(DropIdx, WindowMouseLoc, IsShiftDown);
    let render_lines = getPreviewLines(DropIdx, drop_block_state);

    for (var i = 0; i < render_lines.length; i++) {
        let line = render_lines[i];
        drawLine(ctx, line, color, thickness, line_style)
    }
}

function drawAppendNotifierPreview(ctx, color = 'red', thickness = 1.0, line_style = [5, 5]) {
    if (IsAppendManualMode) {
        color = 'lime';
    }
    let block_state = IsAppendLineMode ? 'post' : 'inline';
    let render_lines = getPreviewLines(getAppendDocRange().index, block_state, false);
    for (var i = 0; i < render_lines.length; i++) {
        let line = render_lines[i];
        drawLine(ctx, line, color, thickness, line_style)
    }
}

function drawFancyTextSelection(ctx) {
    let sel_rects = getRangeRects(getDocSelection());
    sel_rects.forEach((srect) => drawRect(ctx, srect, 'purple', 'goldenrod', 1.5, 100 / 255));
    //let r = FancyTextSelectionRoundedCornerRadius;
    //let def_corner_radius = { tl: r, tr: r, br: r, bl: r };
    //let max_snap_dist = 5;
    //let round_rect_groups = convertRectsToRoundRectGroups(sel_rects, max_snap_dist, def_corner_radius);
    //for (var i = 0; i < round_rect_groups.length; i++) {
    //    for (var j = 0; j < round_rect_groups[i].length; j++) {
    //        let rrect = round_rect_groups[i][j];
    //        drawRoundedRect(ctx, rrect[1], rrect[0], 'purple', 'goldenrod', 0, 100/255)
    //    }
    //}

    //sel_rects.forEach((srect) => drawRoundedRect(ctx, srect, r, 'purple', 'goldenrod', 1.5, 100));
}

function drawTextSelection(ctx) {
    if (IsTextSelectionFancy) {
        drawFancyTextSelection(ctx);
        return;
    }
    if (!isDragging() &&
        !isDropping() &&
        !BlurredSelectionRects &&
        !CurFindReplaceDocRangesRects &&
        !isAppendNotifier()) {
        //return;
    }

    let sel = updateSelectionColors();

    let sel_bg_color = getTextSelectionBgColor();
    let sel_fg_color = getTextSelectionFgColor();


    if (CurFindReplaceDocRangesRects) {
        let scroll_y = getEditorContainerElement().scrollTop;
        let scroll_x = getEditorContainerElement().scrollLeft;

        let active_rect_range_kvp = CurFindReplaceDocRangeRectIdxLookup[CurFindReplaceDocRangeIdx];
        for (var i = 0; i < CurFindReplaceDocRangesRects.length; i++) {
            let cur_rect = CurFindReplaceDocRangesRects[i];
            let adj_rect = cleanRect(cur_rect);
            let is_active = false;
            if (i >= active_rect_range_kvp[0] &&
                i <= active_rect_range_kvp[1]) {
                is_active = true;
            }
            adj_rect = applyRangeRectStyle(is_active, adj_rect);
            //adj_rect.left -= scroll_x;
            //adj_rect.right -= scroll_x;
            //adj_rect.top -= scroll_y;
            //adj_rect.bottom -= scroll_y;
            drawRect(ctx, adj_rect);//, cur_bg_color, sel_fg_color, 0.5, 125 / 255);
        }
    } else if (BlurredSelectionRects) {
        let scroll_y = getEditorContainerElement().scrollTop;

        BlurredSelectionRects.forEach((sel_rect) => {
            sel_rect.top -= scroll_y;
            sel_rect.bottom -= scroll_y;
            drawRect(ctx, sel_rect, sel_bg_color, sel_fg_color, 0.5, 125 / 255);
        });
    }

    drawCaret(ctx, sel);
}

function drawCaret(ctx, sel, caret_width = 1.0, caret_opacity = 1) {
    sel = !sel || sel == null ? updateSelectionColors() : sel;

    if (!sel || sel == null || sel.length > 0) {
        return;
    }
    if (isDropping() || !quill.hasFocus()) {
        // drawn w/ drop preview lines so ignore
        return;
    }

    let caret_line = getCaretLine(sel.index);
    caret_line.ignoreLineStyle = true;
    //let caret_rect = getCharacterRect(sel.index, true, false);
    //let caret_line = { x1: caret_rect.left, y1: caret_rect.top, x2: caret_rect.left, y2: caret_rect.bottom };

    let caret_color = CaretBlinkOffColor == null ? getCaretColor() : CaretBlinkOffColor;
    //drawRect(ctx, caret_rect, caret_color, caret_color, 0, caret_opacity);
    drawLine(ctx, caret_line, caret_color, caret_width);
}

function drawTest() {
    let test_rect = getEditorContainerRect();
    drawRect(getOverlayContext(), test_rect, 'black', 'black', 0, 1);
}

function drawAnnotations(ctx) {
    if (!hasAnnotations(ctx)) {
        return;
    }
    let v_anns = getVisibleAnnotations();

    for (var i = 0; i < v_anns.length; i++) {
        drawAnnotation(ctx, v_anns[i]);
    }
}

function drawAnnotation(ctx, ann) {
    let annotation_rect = getAnnotationRect(ann);
    if (!annotation_rect) {
        return;
    }
    drawRect(ctx, annotation_rect);
}

function clearOverlay(ctx) {
    let overlayCanvas = getOverlayElement();

    ctx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
}
function drawOverlay() {
    updateOverlayBounds();

    let ctx = getOverlayContext();

    drawTextSelection(ctx);

    if (hasAnnotations()) {
        drawAnnotations(ctx);
    }

    if (isDropping()) {
        drawDropPreview(ctx);
    } else if (isAnyAppendEnabled()) {
        // MOTE don't draw append if dropping
        drawAppendNotifierPreview(ctx);
    }
}

// #endregion Draws

// #endregion Actions

// #region Event Handlers

function onCaretBlinkTick() {
    if (!isSubSelectionEnabled()) {
        return;
    }
    if (CaretBlinkOffColor) {
        CaretBlinkOffColor = null;
    } else {
        CaretBlinkOffColor = 'transparent'
    }
    if (WindowMouseDownLoc != null) {
        // don't blink if sel changing
        return;
    }
    drawOverlay();
}

// #endregion Event Handlers