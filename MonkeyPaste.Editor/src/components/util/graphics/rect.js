function drawRect(ctx, rect, fill = 'black', stroke = 'black', lineWidth = 0, opacity = 1.0) {
    let strokStyleStr = cleanColorStyle(stroke, opacity != 1.0 ? opacity : null);
    ctx.strokeStyle = strokStyleStr;
    let fillStyleStr = cleanColorStyle(fill, opacity != 1.0 ? opacity : null);
    ctx.fillStyle = fillStyleStr;
    ctx.lineWidth = lineWidth;

    ctx.fillRect(rect.left, rect.top, rect.width, rect.height);
    if (lineWidth > 0) {
        ctx.strokeRect(rect.left, rect.top, rect.width, rect.height);
    }
}

function isPointInRect(rect, p) {
    if (!rect || !p) {
        return false;
	}
    return p.x >= rect.left && p.x <= rect.right && p.y >= rect.top && p.y <= rect.bottom;
}

function rectContainsRect(rect, other_rect) {
    if (!rect || !other_rect) {
        return false;
    }
    return isPointInRect(rect, { x: other_rect.left, y: other_rect.top }) && isPointInRect(rect, { x: other_rect.right, y: other_rect.bottom });
}

function rectUnion(rect_a, rect_b) {
    let rect_u = {
        left: Math.min(rect_a.left, rect_b.left),
        top: Math.min(rect_a.top, rect_b.top),
        right: Math.max(rect_a.right, rect_b.right),
        bottom: Math.max(rect_a.bottom, rect_b.bottom),
    }
    rect_u = cleanRect(rect_u);
    return rect_u;
}

function rectsUnion(rects) {
    let union_rect = null;
    for (var i = 0; i < rects.length; i++) {
        if (!union_rect) {
            union_rect = rects[i];
        } else {
            union_rect = rectUnion(union_rect, rects[i]);
        }
    }
    return union_rect;
}

function parsePointFromSides(rect, sides) {
    let p = { x: 0, y: 0 };
    if (!sides) {
        return p;
    }
    if (sides.split('|').includes('left')) {
        p.x = rect.left;
    } else if (sides.split('|').includes('right')) {
        p.x = rect.right;
    }
    if (sides.split('|').includes('top')) {
        p.y = rect.top;
    } else if (sides.split('|').includes('bottom')) {
        p.y = rect.bottom;
    }
    return p;
}

function inflateRect(rect, dl, dt, dr, db) {
    rect.left += dl;
    rect.top += dt;
    rect.right += dr;
    rect.bottom += db;
    rect = cleanRect(rect);
    return rect;
}

function moveRectLocation(rect, loc) {
    let w = rect.width;
    let h = rect.height;
    return cleanRect({
        left: loc.x,
        top: loc.y,
        right: loc.x + w,
        bottom: loc.y + h
    });
}

function cleanRect(rect) {
    return {
        left: rect ? rect.left : 0,
        top: rect ? rect.top : 0,
        right: rect ? rect.right : 0,
        bottom: rect ? rect.bottom : 0,
        width: rect ? rect.right - rect.left : 0,
        height: rect ? rect.bottom - rect.top : 0
    };
}