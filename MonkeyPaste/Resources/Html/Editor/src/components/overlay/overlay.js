var IsUnderlinesVisible = false;
var IsHighlightingVisible = false;
var IsDropPreviewVisible = false;

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

function drawUnderlines(ctx, color = 'red', thickness = '1') {
    updateOverlayBounds();

    let p1 = null;
    let p2 = null;
    let count = quill.getLength();

    for (var i = 0; i < count; i++) {
        let idx_rect = quill.getBounds(i);
        //drawRect(ctx, idx_rect);
        let isTail = i == quill.getLength() - 1;

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

        if (p1 && p2) {
            log('line from: ' + p1.x + ',' + p1.y + ' to: ' + p2.x + ',' + p2.y);

            drawLine(ctx, p1.x, p1.y, p2.x, p2.y, color, thickness);
            p1 = p2 = null;

            if (isTail) {
                return;
            }

            p1 = { x: idx_rect.left, y: idx_rect.bottom };
		}
	}
}

function clearUnderlines(ctx) {
    drawUnderlines(ctx,'transparent');
}

function drawHighlighting(ctx, forceColor) {
    if (HighlightRects) {
        for (var i = 0; i < HighlightRects.length; i++) {
            let hl_color = forceColor ? forceColor : i == SelectedHighlightRectIdx ? 'rgba(255,0,0,50)' : 'rgba(0,255,255,50';
            drawRect(ctx, HighlightRects[i], hl_color);
		}
	}
}

function clearHighlighting(ctx) {
    drawHighlighting(ctx, 'transparent');
}

function drawOverlay() {
    let canvas = document.getElementById('overlayCanvas');
    let ctx = canvas.getContext('2d');

    if (IsUnderlinesVisible) {
        drawUnderlines(ctx);
    } else {
        clearUnderlines(ctx);
    }

    if (IsHighlightingVisible) {
        drawHighlighting(ctx);
    } else {
        clearHighlighting(ctx);
	}
}