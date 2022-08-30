//var IsUnderlinesVisible = false;
var IsHighlightingVisible = false;
//var IsDropPreviewVisible = false;

var HighlightRects = [];
var SelectedHighlightRectIdx = -1;

function updateOverlayBounds() {
	let editorRect = document.getElementById('editor').getBoundingClientRect();

	let overlayCanvas = document.getElementById('overlayCanvas');
	overlayCanvas.style.left = editorRect.left;
	overlayCanvas.style.top = editorRect.top;
	overlayCanvas.width = editorRect.width;
	overlayCanvas.height = editorRect.height;
}

function drawUnderlines(ctx, color = 'red', thickness = '0.5') {
    updateOverlayBounds();

    let p1 = null;
    let p2 = null;
    let count = quill.getLength();
    let windowRect = getWindowRect();

    for (var i = 0; i < count; i++) {
        //log('drawing idx ' + i + ' of ' + count);

        let idx_rect = quill.getBounds(i);
        //log('idx rect: ' + idx_rect);

        let isTail = i == count - 1;

        if (p1 == null) {
            // initial line
            p1 = { x: idx_rect.left, y: idx_rect.bottom };
        } else if (idx_rect.bottom > p1.y) {
            // start of new line
            let last_idx_rect = quill.getBounds(i - 1);
            p2 = { x: last_idx_rect.right, y: last_idx_rect.bottom };
        } else if (isTail) {
            // last line
            p2 = { x: idx_rect.right, y: idx_rect.bottom };
        }

        if (rectContainsPoint(windowRect, p1) && rectContainsPoint(windowRect,p2)) {
            drawLine(ctx, p1.x, p1.y, p2.x, p2.y, color, thickness);
            p1 = p2 = null;

            if (isTail) {
                return;
            }

            p1 = { x: idx_rect.left, y: idx_rect.bottom };
		}
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


function drawDropPreview(ctx, color = 'red', thickness = '0.5') {
    let drop_idx = getEditorIndexFromPoint_ByLine(MousePos);
    //log('dropIdx: ' + drop_idx + ' mp x:' + MousePos.x + ' y:' + MousePos.y);
    if (drop_idx < 0) {
        return;
    }

    let block_line_offset = 3.0;
    let window_rect = getWindowRect();

    let line_start_idx = getLineStartDocIdx(drop_idx);
    let line_start_rect = getCharacterRect(line_start_idx);
    let pre_y = line_start_rect.top - block_line_offset;
    let pre_line = { x1: 0, y1: pre_y, x2: window_rect.width, y2: pre_y };

    let line_end_idx = getLineEndDocIdx(drop_idx);
    let line_end_rect = getCharacterRect(line_end_idx);
    let post_y = line_end_rect.bottom + block_line_offset;
    let post_line = { x1: 0, y1: post_y, x2: window_rect.width, y2: post_y };

    let caret_rect = getCharacterRect(drop_idx);
    let caret_line = { x1: caret_rect.left, y1: caret_rect.top, x2: caret_rect.left, y2: caret_rect.bottom };


    let is_split_block_drop = IsCtrlDown;

    let block_threshold = Math.max(2, caret_rect.height / 4);
    let doc_start_rect = getCharacterRect(0);

    // NOTE to avoid conflicts between each line as pre/post drop only use pre for first
    // line of content then only check post for others
    let is_pre_block_drop = Math.abs(MousePos.y - doc_start_rect.top) < block_threshold || MousePos.y < doc_start_rect.top;
    let is_post_block_drop = Math.abs(MousePos.y - caret_rect.bottom) < block_threshold || MousePos.y > caret_rect.bottom;

    if (is_split_block_drop && CopyItemType != 'FileList') {
        is_pre_block_drop = is_post_block_drop = false;
    }


    let render_lines = [];
    if (is_split_block_drop) {
        let pre_split_line = pre_line;
        pre_split_line.x1 = caret_line.x1;

        let post_split_line = post_line;
        post_split_line.x2 = caret_line.x1;

        render_lines.push(pre_split_line);
        render_lines.push(post_split_line);
    } else if (is_pre_block_drop) {
        render_lines.push(pre_line);
    } else if (is_post_block_drop) {
        render_lines.push(post_line);
    } else {
        render_lines.push(caret_line);
    }

    render_lines.forEach(line => drawLine(ctx, line.x1, line.y1, line.x2, line.y2, color, thickness, [15, 5]));
}


function drawOverlay() {
    let canvas = document.getElementById('overlayCanvas');
    let ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    let isUnderlinesVisible = isReadOnly() && IsSubSelectionEnabled;
    if (isUnderlinesVisible) {
        drawUnderlines(ctx);
    } else {
        //clearUnderlines(ctx);
    }

    if (IsHighlightingVisible) {
        drawHighlighting(ctx);
    } else {
        //clearHighlighting(ctx);
    }

    let isDropPreviewVisible = isDropping();
    if (isDropPreviewVisible) {
        drawDropPreview(ctx);
    } else {
        //clearDropPreview(ctx);
    }

}