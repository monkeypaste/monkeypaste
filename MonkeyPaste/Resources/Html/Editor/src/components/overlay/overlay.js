//var IsUnderlinesVisible = false;
var IsHighlightingVisible = false;
//var IsDropPreviewVisible = false;

var HighlightRects = [];
var SelectedHighlightRectIdx = -1;

var IsCaretBlinkOn = false;
var CaretBlinkOffColor = null;

function updateOverlayBounds(overlayCanvas) {
    let editorRect = getEditorContainerRect();
	overlayCanvas.style.left = editorRect.left;
	overlayCanvas.style.top = editorRect.top;
	overlayCanvas.width = editorRect.width;
	overlayCanvas.height = editorRect.height;
}

function testOverlay() {
    let canvas = document.getElementById('overlayCanvas');
    updateOverlayBounds(canvas);

    if (!canvas.getContext) {
        return;
    }

    let ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    let overlay_rect = cleanRect(canvas.getBoundingClientRect())

    drawRect(ctx,overlay_rect, 'transparent', 'blue', 3);
}

function drawUnderlines(ctx, color = 'red', thickness = '0.5') {
    let p1 = null;
    let p2 = null;
    let count = quill.getLength();
    let windowRect = getEditorContainerRect();

    for (var i = 0; i < count; i++) {
        
        //log('drawing idx ' + i + ' of ' + count);

        let idx_rect = getCharacterRect(i); //quill.getBounds(i);
        //log('idx rect: ' + idx_rect);

        let is_initial_line = p1 == null;        
        let is_tail = i == count - 1;
        let is_new_line = false;


        if (is_initial_line) {
            // initial line
            p1 = { x: idx_rect.left, y: idx_rect.bottom };
            
        } else if (is_tail) {
            // last line
            p2 = { x: idx_rect.right, y: idx_rect.bottom };
        } else {
            let idx_block_elm = getBlockElementAtDocIdx(i);
            let last_block_elm = getBlockElementAtDocIdx(i - 1);
            if (idx_block_elm != last_block_elm ||
                idx_rect.bottom > p1.y) {
                // start of new line
                let last_idx_rect = getCharacterRect(i - 1);// quill.getBounds(i - 1);
                p2 = { x: last_idx_rect.right, y: last_idx_rect.bottom };
            } 
		}
        //if (is_initial_line) {
        //    // initial line
        //    p1 = { x: idx_rect.left, y: idx_rect.bottom };
        //} else if (is_new_line) {
        //    // start of new line
        //    let last_idx_rect = getCharacterRect(i - 1);// quill.getBounds(i - 1);
        //    p2 = { x: last_idx_rect.right, y: last_idx_rect.bottom };
        //} else if (is_tail) {
        //    // last line
        //    p2 = { x: idx_rect.right, y: idx_rect.bottom };
        //}

        //if (isPointInRect(windowRect, p1) && isPointInRect(windowRect, p2)) {
        if (p1 && p2) {
            // force straight lines

            let max_line_bottom = Math.max(p1.y, p2.y);
            drawLine(ctx, { x1: p1.x, y1: max_line_bottom, x2: p2.x, y2: max_line_bottom }, color, thickness);
            p1 = p2 = null;
            if (is_tail) {
                return;
            }

            p1 = { x: idx_rect.left, y: max_line_bottom };
            
        }
        //if (isTemplateAtDocIdx(i + 1)) {
        //    // skip template idx
        //    i += 3;
        //}
	}
}

function drawHighlighting(ctx, forceColor) {
    if (HighlightRects) {
        for (var i = 0; i < HighlightRects.length; i++) {
            let hl_color = forceColor ? forceColor : i == SelectedHighlightRectIdx ? 'rgba(255,0,0,50)' : 'rgba(0,255,255,50';
            drawRect(ctx, HighlightRects[i], hl_color);
		}
	}
}

function drawDropPreview(ctx, color = 'red', thickness = '0.5', line_style = [5,5], alpha = 255) {
    let drop_idx = DropIdx;
    if (isDragCopy()) {
        color = 'lime';
	}
    log('dropIdx: ' + drop_idx);
    if (drop_idx < 0) {
        return;
    }

    let block_line_offset = 3.0;
    let editor_rect = getEditorContainerRect();

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

    IsSplitDrop = IsShiftDown; //IsCtrlDown || IsAltDown;

    let block_threshold = Math.max(2, caret_line.height / 4);
    let doc_start_rect = getCharacterRect(0);

    // NOTE to avoid conflicts between each line as pre/post drop only use pre for first
    // line of content then only check post for others
    IsPreBlockDrop = Math.abs(WindowMouseLoc.y - doc_start_rect.top) < block_threshold || WindowMouseLoc.y < doc_start_rect.top;
    IsPostBlockDrop = Math.abs(WindowMouseLoc.y - caret_line.y2) < block_threshold || WindowMouseLoc.y > caret_line.y2;
  
    if (IsSplitDrop && CopyItemType != 'FileList') {
        IsPreBlockDrop = false;
        IsPostBlockDrop = false;
    }

    let render_lines = [];
    let render_caret_line = null;

    if (IsSplitDrop) {
        let pre_split_line = pre_line;
        pre_split_line.x1 = caret_line.x1;

        let post_split_line = post_line;
        post_split_line.x2 = caret_line.x1;

        render_lines.push(pre_split_line);
        render_lines.push(post_split_line);

        render_caret_line = caret_line;
    } else if (IsPreBlockDrop) {
        render_lines.push(pre_line);
    } else if (IsPostBlockDrop) {
        render_lines.push(post_line);
    } else {
        render_caret_line = caret_line;
    }

    for (var i = 0; i < render_lines.length; i++) {
        let line = render_lines[i];
        drawLine(ctx, line, color, thickness, line_style)
    }
    if (render_caret_line) {        
        drawLine(ctx, render_caret_line, color, thickness);
	}

}

function drawFancyTextSelection(ctx) {
    let sel_rects = getRangeRects(getEditorSelection(), false, false);

    let r = FancyTextSelectionRoundedCornerRadius;
    let def_corner_radius = { tl: r, tr: r, br: r, bl: r };
    let max_snap_dist = 5;
    let round_rect_groups = convertRectsToRoundRectGroups(sel_rects, max_snap_dist, def_corner_radius);
    for (var i = 0; i < round_rect_groups.length; i++) {
        for (var j = 0; j < round_rect_groups[i].length; j++) {
            let rrect = round_rect_groups[i][j];
            drawRoundedRect(ctx, rrect[1], rrect[0], 'purple', 'goldenrod', 0, 100)
        }
    }

    //sel_rects.forEach((srect) => drawRoundedRect(ctx, srect, r, 'purple', 'goldenrod', 1.5, 100));
}

function drawTextSelection(ctx) {
    if (IsTextSelectionFancy) {
        drawFancyTextSelection(ctx);
        return;
    }


    let sel = getEditorSelection();
    let sel_bg_color = DefaultSelectionBgColor;
    let sel_fg_color = DefaultSelectionFgColor;
    let caret_color = DefaultCaretColor;

    if (IsDropping || IsDragging) {
        if (IsDragging) {
            if (isDropValid() || IsDragging) {
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
            // NOTE always override caret during drop to make it nice and thicky
            //caret_color = 'transparent';
		}
    } else if (IsSubSelectionEnabled) {
        if (isEditorToolbarVisible()) {
            if (IsTemplateAtInsert) {
                caret_color = 'transparent';
			}
        } else {
            caret_color = 'red';
		}
        
    }

    setTextSelectionBgColor(sel_bg_color);
    setTextSelectionFgColor(sel_fg_color);

    setCaretColor(caret_color);

    if (BlurredSelectionRects != null) {
        BlurredSelectionRects.forEach((sel_rect) => {
            drawRect(ctx, sel_rect, sel_bg_color,'transparent', 0, 75);
        });
    }

    if (IsSubSelectionEnabled && !IsDropping && sel && sel.length == 0 && isReadOnly()) {
        // caret is hidden when not editable, only draw caret if sel not range or dropping
        // (drop preview draws if non-block dropping )
  //      if (isEditorToolbarVisible()) {
  //          IsCaretBlinkOn = false;
  //          CaretBlinkOffColor = null;
  //          clearInterval(caretBlinkTick);
		//} else if (!IsCaretBlinkOn) {
  //          IsCaretBlinkOn = true;
  //          CaretBlinkOffColor = null;
  //          setInterval(caretBlinkTick, 500);
  //      } 
        let caret_display_color = CaretBlinkOffColor ? CaretBlinkOffColor : caret_color;
        let caret_line = getCaretLine(sel.index);
        drawLine(ctx, caret_line, caret_display_color);
    } else {
        IsCaretBlinkOn = false;
        CaretBlinkOffColor = null;
        clearInterval(caretBlinkTick);
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
    let canvas = document.getElementById('overlayCanvas');
    updateOverlayBounds(canvas);

    if (!canvas.getContext) {
        return;
    }

    let ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    drawTextSelection(ctx);

    //let isUnderlinesVisible = !isEditorToolbarVisible() && IsSubSelectionEnabled && !isDropping();
    //if (isUnderlinesVisible) {
    //    drawUnderlines(ctx);
    //} 

    if (IsHighlightingVisible) {
        drawHighlighting(ctx);
    } 

    let isDropPreviewVisible = IsDropping;
    if (isDropPreviewVisible) {
       drawDropPreview(ctx);
    } 
}