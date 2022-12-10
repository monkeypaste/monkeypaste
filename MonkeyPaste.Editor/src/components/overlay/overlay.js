var IsHighlightingVisible = false;

var HighlightRects = [];
var SelectedHighlightRectIdx = -1;

var IsCaretBlinkOn = false;
var CaretBlinkOffColor = null;

function updateOverlayBounds(overlayCanvas) {
    let editorRect = getEditorContainerRect();
    let window_rect = getWindowRect();

    overlayCanvas.style.left = window_rect.left;
	overlayCanvas.style.top = editorRect.top;
    overlayCanvas.width = window_rect.width;
    overlayCanvas.height = window_rect.height;
}

function drawHighlighting(ctx, forceColor) {
    if (HighlightRects) {
        for (var i = 0; i < HighlightRects.length; i++) {
            let hl_color = forceColor ? forceColor : i == SelectedHighlightRectIdx ? 'rgba(255,0,0,50)' : 'rgba(0,255,255,50';
            drawRect(ctx, HighlightRects[i], hl_color);
		}
	}
}

// #region Ole Previews

function getPreviewLines(drop_idx, block_state) {
    if (drop_idx < 0) {
        return [];
    }

    let block_line_offset = 3.0;
    let editor_rect = getEditorContainerRect(false);
    //let editor_rect = getWindowRect();

    let line_start_idx = getLineStartDocIdx(drop_idx);
    let line_start_rect = getCharacterRect(line_start_idx);
    let pre_y = line_start_rect.top - block_line_offset;
    let pre_line = { x1: 0, y1: pre_y, x2: editor_rect.width, y2: pre_y };

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

// #endregion Ole preview

function drawFancyTextSelection(ctx) {
    let sel_rects = getRangeRects(getDocSelection());
    sel_rects.forEach((srect) => drawRect(ctx, srect, 'purple', 'goldenrod', 1.5, 100/255));
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
        return;
	}


    let sel = getDocSelection();
    let sel_bg_color = DefaultSelectionBgColor;
    let sel_fg_color = DefaultSelectionFgColor;
    let caret_color = DefaultCaretColor;

    if (isDropping() || isDragging()) {
        if (isDragging()) {
            // ignoring invalidity if external drop
            let is_drop_valid = DropIdx >= 0 || !isDropping();
            if (is_drop_valid) {
                if (isDragCopy()) {
                    sel_bg_color = 'lime';
                    log('copy recognized in sel draw');
                }

                if (isDropHtml()) {
                    sel_fg_color = 'orange';
                }
            } else {
                sel_bg_color = 'salmon';
            }
        }
    }
    else if (isSubSelectionEnabled()) {
        if (isEditorToolbarVisible()) {
            if (isSelAtFocusTemplateInsert()) {
                // hide cursor within focus template
                caret_color = 'transparent';
            }
        } else {
            caret_color = 'red';
        }
    } else {
        // in no select hide cursor
        caret_color = 'transparent';
	}

    setTextSelectionBgColor(sel_bg_color);
    setTextSelectionFgColor(sel_fg_color);

    setCaretColor(caret_color);

    if (CurFindReplaceDocRangesRects) {
        let scroll_y = getEditorContainerElement().scrollTop;
        let active_rect_range_kvp = CurFindReplaceDocRangeRectIdxLookup[CurFindReplaceDocRangeIdx];
        CurFindReplaceDocRangesRects.forEach((cur_rect, rect_idx) => {

            let cur_bg_color = sel_bg_color;
            if (rect_idx >= active_rect_range_kvp[0] &&
                rect_idx <= active_rect_range_kvp[1]) {
                cur_bg_color = 'lime';
            } else {
                cur_bg_color = sel_bg_color
            }
            let adj_rect = cleanRect(cur_rect);
            adj_rect.top -= scroll_y;
            adj_rect.bottom -= scroll_y;
            drawRect(ctx, adj_rect, cur_bg_color, sel_fg_color, 0.5, 125 / 255);
        });
    } else if (BlurredSelectionRects) {
        let scroll_y = getEditorContainerElement().scrollTop;

        BlurredSelectionRects.forEach((sel_rect) => {
            sel_rect.top -= scroll_y;
            sel_rect.bottom -= scroll_y;
            drawRect(ctx, sel_rect, sel_bg_color, sel_fg_color, 0.5, 125 / 255);
        });
	}
}

function caretBlinkTick() {
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


function drawOverlay() {
    let overlayCanvas = document.getElementById('overlayCanvas');
    updateOverlayBounds(overlayCanvas);

    if (!overlayCanvas.getContext) {
        return;
    }

    let ctx = overlayCanvas.getContext('2d');
    ctx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);

    drawTextSelection(ctx);

    if (IsHighlightingVisible) {
        drawHighlighting(ctx);
    } 

    if (isDropping()) {
       drawDropPreview(ctx);
    } else if (isAnyAppendEnabled()) {
        // MOTE don't draw append if dropping
        drawAppendNotifierPreview(ctx);
    }
}